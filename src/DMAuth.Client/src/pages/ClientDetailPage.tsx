import { useState } from 'react'
import { useParams, useNavigate } from 'react-router-dom'
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query'
import { useForm, useFieldArray } from 'react-hook-form'
import { zodResolver } from '@hookform/resolvers/zod'
import { z } from 'zod'
import { isAxiosError } from 'axios'
import { clientApi } from '../api/clientApi'

const AVAILABLE_SCOPES = ['openid', 'profile', 'email', 'offline_access']

const schema = z.object({
  clientName: z.string().min(1, 'Name is required').max(200, 'Name must be 200 characters or fewer'),
  redirectUris: z
    .array(z.object({ value: z.string().url('Must be a valid URL') }))
    .min(1, 'At least one redirect URI is required'),
  allowedScopes: z.array(z.string()).min(1, 'Select at least one scope'),
})

type FormValues = z.infer<typeof schema>

function DeleteConfirmModal({
  clientName,
  onConfirm,
  onCancel,
  isPending,
}: {
  clientName: string
  onConfirm: () => void
  onCancel: () => void
  isPending: boolean
}) {
  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/40 px-4">
      <div className="w-full max-w-sm rounded-xl bg-white p-6 shadow-xl">
        <h2 className="mb-2 text-lg font-semibold text-gray-900">Delete client?</h2>
        <p className="mb-5 text-sm text-gray-500">
          <span className="font-medium text-gray-800">{clientName}</span> will be permanently deleted.
          Any applications using this client will stop working.
        </p>
        <div className="flex gap-3">
          <button
            onClick={onConfirm}
            disabled={isPending}
            className="flex-1 rounded-md bg-red-600 px-4 py-2 text-sm font-medium text-white hover:bg-red-700 disabled:opacity-60 transition-colors"
          >
            {isPending ? 'Deleting…' : 'Delete'}
          </button>
          <button
            onClick={onCancel}
            disabled={isPending}
            className="flex-1 rounded-md border border-gray-300 bg-white px-4 py-2 text-sm font-medium text-gray-700 hover:bg-gray-50 transition-colors"
          >
            Cancel
          </button>
        </div>
      </div>
    </div>
  )
}

export function ClientDetailPage() {
  const { id } = useParams<{ id: string }>()
  const navigate = useNavigate()
  const queryClient = useQueryClient()
  const [showDeleteModal, setShowDeleteModal] = useState(false)

  const { data: client, isLoading, isError } = useQuery({
    queryKey: ['clients', id],
    queryFn: () => clientApi.getClient(id!).then(r => r.data),
    enabled: !!id,
  })

  const {
    register,
    control,
    handleSubmit,
    watch,
    setValue,
    setError,
    reset,
    formState: { errors, isDirty },
  } = useForm<FormValues>({
    resolver: zodResolver(schema),
    values: client
      ? {
          clientName: client.clientName,
          redirectUris: client.redirectUris.map(uri => ({ value: uri })),
          allowedScopes: [...client.allowedScopes],
        }
      : undefined,
  })

  const { fields, append, remove } = useFieldArray({ control, name: 'redirectUris' })
  const selectedScopes = watch('allowedScopes') ?? []

  const update = useMutation({
    mutationFn: (values: FormValues) =>
      clientApi.updateClient(id!, {
        clientName: values.clientName,
        redirectUris: values.redirectUris.map(u => u.value),
        allowedScopes: values.allowedScopes,
      }),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['clients'] })
      queryClient.invalidateQueries({ queryKey: ['clients', id] })
      reset(undefined, { keepValues: true })
    },
    onError: error => {
      const message = isAxiosError(error)
        ? (error.response?.data?.message ?? 'Failed to update client.')
        : 'Something went wrong.'
      setError('root', { message })
    },
  })

  const remove_ = useMutation({
    mutationFn: () => clientApi.deleteClient(id!),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['clients'] })
      navigate('/clients')
    },
  })

  const toggleScope = (scope: string) => {
    const next = selectedScopes.includes(scope)
      ? selectedScopes.filter(s => s !== scope)
      : [...selectedScopes, scope]
    setValue('allowedScopes', next, { shouldDirty: true, shouldValidate: true })
  }

  if (isLoading) {
    return (
      <div className="max-w-xl space-y-4">
        {[...Array(3)].map((_, i) => (
          <div key={i} className="h-32 animate-pulse rounded-xl bg-gray-100" />
        ))}
      </div>
    )
  }

  if (isError || !client) {
    return (
      <div className="max-w-xl">
        <p className="text-sm text-red-600">Client not found or you don't have access.</p>
      </div>
    )
  }

  return (
    <>
      {showDeleteModal && (
        <DeleteConfirmModal
          clientName={client.clientName}
          onConfirm={() => remove_.mutate()}
          onCancel={() => setShowDeleteModal(false)}
          isPending={remove_.isPending}
        />
      )}

      <div className="max-w-xl">
        <div className="mb-6">
          <h1 className="text-2xl font-semibold text-gray-900">{client.clientName}</h1>
          <p className="mt-1 font-mono text-xs text-gray-400">{client.oAuthClientId}</p>
        </div>

        <form onSubmit={handleSubmit(values => update.mutate(values))} noValidate className="flex flex-col gap-6">

          {/* Info */}
          <div className="rounded-xl border border-gray-200 bg-white p-6">
            <h2 className="mb-4 text-sm font-semibold text-gray-700">Details</h2>
            <div className="mb-4 flex flex-col gap-3 text-sm">
              <div className="flex justify-between">
                <span className="text-gray-500">Type</span>
                <span className="font-medium text-gray-900">{client.clientType}</span>
              </div>
              <div className="flex justify-between">
                <span className="text-gray-500">Status</span>
                <span className={`inline-flex rounded-full px-2 py-0.5 text-xs font-medium ${
                  client.isActive ? 'bg-green-50 text-green-700' : 'bg-gray-100 text-gray-500'
                }`}>
                  {client.isActive ? 'Active' : 'Inactive'}
                </span>
              </div>
              <div className="flex justify-between">
                <span className="text-gray-500">Created</span>
                <span className="text-gray-900">{new Date(client.createdAt).toLocaleDateString()}</span>
              </div>
            </div>

            {/* Editable name */}
            <div className="border-t border-gray-100 pt-4">
              <label htmlFor="clientName" className="mb-1 block text-sm font-medium text-gray-700">
                Name
              </label>
              <input
                {...register('clientName')}
                id="clientName"
                type="text"
                className="w-full rounded-md border border-gray-300 px-3 py-2 text-sm text-gray-900 focus:border-indigo-500 focus:outline-none focus:ring-1 focus:ring-indigo-500"
              />
              {errors.clientName && (
                <p className="mt-1 text-xs text-red-600">{errors.clientName.message}</p>
              )}
            </div>
          </div>

          {/* Redirect URIs */}
          <div className="rounded-xl border border-gray-200 bg-white p-6">
            <h2 className="mb-3 text-sm font-semibold text-gray-700">Redirect URIs</h2>
            <div className="flex flex-col gap-2">
              {fields.map((field, index) => (
                <div key={field.id} className="flex items-center gap-2">
                  <input
                    {...register(`redirectUris.${index}.value`)}
                    type="url"
                    className="flex-1 rounded-md border border-gray-300 px-3 py-2 text-sm text-gray-900 focus:border-indigo-500 focus:outline-none focus:ring-1 focus:ring-indigo-500"
                  />
                  {fields.length > 1 && (
                    <button
                      type="button"
                      onClick={() => remove(index)}
                      className="shrink-0 rounded-md p-1.5 text-gray-400 hover:bg-gray-100 hover:text-gray-600"
                      aria-label="Remove"
                    >
                      <svg className="h-4 w-4" fill="none" viewBox="0 0 24 24" strokeWidth={2} stroke="currentColor">
                        <path strokeLinecap="round" strokeLinejoin="round" d="M6 18 18 6M6 6l12 12" />
                      </svg>
                    </button>
                  )}
                </div>
              ))}
              {errors.redirectUris && !Array.isArray(errors.redirectUris) && (
                <p className="text-xs text-red-600">{errors.redirectUris.message}</p>
              )}
              {Array.isArray(errors.redirectUris) &&
                errors.redirectUris.map((err, i) =>
                  err?.value ? (
                    <p key={i} className="text-xs text-red-600">URI {i + 1}: {err.value.message}</p>
                  ) : null
                )}
            </div>
            <button
              type="button"
              onClick={() => append({ value: '' })}
              className="mt-3 text-sm font-medium text-indigo-600 hover:text-indigo-500"
            >
              + Add redirect URI
            </button>
          </div>

          {/* Scopes */}
          <div className="rounded-xl border border-gray-200 bg-white p-6">
            <h2 className="mb-3 text-sm font-semibold text-gray-700">Allowed scopes</h2>
            <div className="flex flex-col gap-2">
              {AVAILABLE_SCOPES.map(scope => (
                <label key={scope} className="flex cursor-pointer items-center gap-3">
                  <input
                    type="checkbox"
                    checked={selectedScopes.includes(scope)}
                    onChange={() => toggleScope(scope)}
                    className="h-4 w-4 rounded border-gray-300 text-indigo-600 focus:ring-indigo-500"
                  />
                  <span className="text-sm text-gray-800">{scope}</span>
                </label>
              ))}
            </div>
            {errors.allowedScopes && (
              <p className="mt-2 text-xs text-red-600">{errors.allowedScopes.message}</p>
            )}
          </div>

          {errors.root && (
            <p className="rounded-md bg-red-50 px-3 py-2 text-sm text-red-700">
              {errors.root.message}
            </p>
          )}

          {update.isSuccess && (
            <p className="rounded-md bg-green-50 px-3 py-2 text-sm text-green-700">
              Client updated.
            </p>
          )}

          <div className="flex gap-3">
            <button
              type="submit"
              disabled={update.isPending || !isDirty}
              className="rounded-md bg-indigo-600 px-4 py-2 text-sm font-medium text-white hover:bg-indigo-700 disabled:opacity-60  transition-colors"
            >
              {update.isPending ? 'Saving…' : 'Save changes'}
            </button>
            <button
              type="button"
              onClick={() => reset()}
              disabled={!isDirty || update.isPending}
              className="rounded-md border border-gray-300 bg-white px-4 py-2 text-sm font-medium text-gray-700 hover:bg-gray-50 disabled:opacity-60  transition-colors"
            >
              Discard
            </button>
          </div>
        </form>

        {/* Danger zone */}
        <div className="mt-8 rounded-xl border border-red-200 bg-white p-6">
          <h2 className="mb-1 text-sm font-semibold text-red-700">Danger zone</h2>
          <p className="mb-4 text-sm text-gray-500">
            Deleting this client is permanent and cannot be undone.
          </p>
          <button
            type="button"
            onClick={() => setShowDeleteModal(true)}
            className="rounded-md border border-red-300 px-4 py-2 text-sm font-medium text-red-700 hover:bg-red-50 transition-colors"
          >
            Delete client
          </button>
        </div>
      </div>
    </>
  )
}
