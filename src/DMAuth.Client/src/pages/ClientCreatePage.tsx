import { useState } from 'react'
import { useForm, useFieldArray } from 'react-hook-form'
import { zodResolver } from '@hookform/resolvers/zod'
import { z } from 'zod'
import { useMutation, useQueryClient } from '@tanstack/react-query'
import { useNavigate } from 'react-router-dom'
import { isAxiosError } from 'axios'
import { clientApi } from '../api/clientApi'
import type { RegisterClientResponse } from '../types/client'

const AVAILABLE_SCOPES = ['openid', 'profile', 'email', 'offline_access']

const schema = z.object({
  clientName: z.string().min(1, 'Name is required').max(200, 'Name must be 200 characters or fewer'),
  clientType: z.enum(['Public', 'Confidential']),
  redirectUris: z
    .array(z.object({ value: z.string().url('Must be a valid URL') }))
    .min(1, 'At least one redirect URI is required'),
  allowedScopes: z.array(z.string()).min(1, 'Select at least one scope'),
})

type FormValues = z.infer<typeof schema>

function SecretModal({
  result,
  onClose,
}: {
  result: RegisterClientResponse
  onClose: () => void
}) {
  const [copied, setCopied] = useState<string | null>(null)

  const copy = (value: string, key: string) => {
    navigator.clipboard.writeText(value)
    setCopied(key)
    setTimeout(() => setCopied(null), 2000)
  }

  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/40 px-4">
      <div className="w-full max-w-md rounded-xl bg-white p-6 shadow-xl">
        <h2 className="mb-1 text-lg font-semibold text-gray-900">Client registered</h2>
        <p className="mb-5 text-sm text-gray-500">
          {result.clientSecret
            ? 'Save your client secret now — it will not be shown again.'
            : 'Your client has been registered.'}
        </p>

        <div className="flex flex-col gap-4">
          <div>
            <p className="mb-1 text-xs font-medium text-gray-500">Client ID</p>
            <div className="flex items-center gap-2 rounded-md border border-gray-200 bg-gray-50 px-3 py-2">
              <code className="flex-1 truncate text-xs text-gray-800">{result.oAuthClientId}</code>
              <button
                onClick={() => copy(result.oAuthClientId, 'id')}
                className="shrink-0 text-xs font-medium text-indigo-600 hover:text-indigo-500"
              >
                {copied === 'id' ? 'Copied!' : 'Copy'}
              </button>
            </div>
          </div>

          {result.clientSecret && (
            <div>
              <p className="mb-1 text-xs font-medium text-gray-500">Client secret</p>
              <div className="flex items-center gap-2 rounded-md border border-amber-200 bg-amber-50 px-3 py-2">
                <code className="flex-1 truncate text-xs text-gray-800">{result.clientSecret}</code>
                <button
                  onClick={() => copy(result.clientSecret!, 'secret')}
                  className="shrink-0 text-xs font-medium text-indigo-600 hover:text-indigo-500"
                >
                  {copied === 'secret' ? 'Copied!' : 'Copy'}
                </button>
              </div>
            </div>
          )}
        </div>

        <button
          onClick={onClose}
          className="mt-6 w-full rounded-md bg-indigo-600 px-4 py-2 text-sm font-medium text-white hover:bg-indigo-700 transition-colors"
        >
          Done
        </button>
      </div>
    </div>
  )
}

export function ClientCreatePage() {
  const navigate = useNavigate()
  const queryClient = useQueryClient()
  const [createdClient, setCreatedClient] = useState<RegisterClientResponse | null>(null)

  const {
    register,
    control,
    handleSubmit,
    watch,
    setValue,
    setError,
    formState: { errors },
  } = useForm<FormValues>({
    resolver: zodResolver(schema),
    defaultValues: {
      clientName: '',
      clientType: 'Public',
      redirectUris: [{ value: '' }],
      allowedScopes: ['openid'],
    },
  })

  const { fields, append, remove } = useFieldArray({ control, name: 'redirectUris' })
  const selectedScopes = watch('allowedScopes')

  const create = useMutation({
    mutationFn: (values: FormValues) =>
      clientApi.createClient({
        clientName: values.clientName,
        clientType: values.clientType,
        redirectUris: values.redirectUris.map(u => u.value),
        allowedScopes: values.allowedScopes,
      }),
    onSuccess: response => {
      queryClient.invalidateQueries({ queryKey: ['clients'] })
      setCreatedClient(response.data)
    },
    onError: error => {
      const message = isAxiosError(error)
        ? (error.response?.data?.message ?? 'Failed to register client.')
        : 'Something went wrong.'
      setError('root', { message })
    },
  })

  const toggleScope = (scope: string) => {
    const next = selectedScopes.includes(scope)
      ? selectedScopes.filter(s => s !== scope)
      : [...selectedScopes, scope]
    setValue('allowedScopes', next, { shouldValidate: true })
  }

  const handleModalClose = () => {
    navigate(`/clients/${createdClient!.clientId}`)
  }

  return (
    <>
      {createdClient && <SecretModal result={createdClient} onClose={handleModalClose} />}

      <div className="max-w-xl">
        <div className="mb-6">
          <h1 className="text-2xl font-semibold text-gray-900">New client</h1>
          <p className="mt-1 text-sm text-gray-500">Register an OAuth 2.0 client application.</p>
        </div>

        <form onSubmit={handleSubmit(values => create.mutate(values))} noValidate className="flex flex-col gap-6">

          {/* Name */}
          <div className="rounded-xl border border-gray-200 bg-white p-6">
            <label htmlFor="clientName" className="mb-1 block text-sm font-medium text-gray-700">
              Name
            </label>
            <input
              {...register('clientName')}
              id="clientName"
              type="text"
              placeholder="My Application"
              className="w-full rounded-md border border-gray-300 px-3 py-2 text-sm text-gray-900 placeholder-gray-400 focus:border-indigo-500 focus:outline-none focus:ring-1 focus:ring-indigo-500"
            />
            {errors.clientName && (
              <p className="mt-1 text-xs text-red-600">{errors.clientName.message}</p>
            )}
          </div>

          {/* Client type */}
          <div className="rounded-xl border border-gray-200 bg-white p-6">
            <p className="mb-3 text-sm font-medium text-gray-700">Client type</p>
            <div className="flex flex-col gap-3">
              {(['Public', 'Confidential'] as const).map(type => (
                <label key={type} className="flex cursor-pointer items-start gap-3">
                  <input
                    {...register('clientType')}
                    type="radio"
                    value={type}
                    className="mt-0.5 h-4 w-4 border-gray-300 text-indigo-600 focus:ring-indigo-500"
                  />
                  <div>
                    <p className="text-sm font-medium text-gray-900">{type}</p>
                    <p className="text-xs text-gray-500">
                      {type === 'Public'
                        ? 'Cannot securely store a secret (SPA, mobile app). Uses PKCE only.'
                        : 'Can securely store a secret (server-side app). Receives a client_secret on registration.'}
                    </p>
                  </div>
                </label>
              ))}
            </div>
            {errors.clientType && (
              <p className="mt-2 text-xs text-red-600">{errors.clientType.message}</p>
            )}
          </div>

          {/* Redirect URIs */}
          <div className="rounded-xl border border-gray-200 bg-white p-6">
            <p className="mb-3 text-sm font-medium text-gray-700">Redirect URIs</p>
            <div className="flex flex-col gap-2">
              {fields.map((field, index) => (
                <div key={field.id} className="flex items-center gap-2">
                  <input
                    {...register(`redirectUris.${index}.value`)}
                    type="url"
                    placeholder="https://example.com/callback"
                    className="flex-1 rounded-md border border-gray-300 px-3 py-2 text-sm text-gray-900 placeholder-gray-400 focus:border-indigo-500 focus:outline-none focus:ring-1 focus:ring-indigo-500"
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
            <p className="mb-3 text-sm font-medium text-gray-700">Allowed scopes</p>
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

          <div className="flex gap-3">
            <button
              type="submit"
              disabled={create.isPending}
              className="rounded-md bg-indigo-600 px-4 py-2 text-sm font-medium text-white hover:bg-indigo-700 focus:outline-none focus:ring-2 focus:ring-indigo-500 focus:ring-offset-2 disabled:opacity-60 transition-colors"
            >
              {create.isPending ? 'Registering…' : 'Register client'}
            </button>
            <button
              type="button"
              onClick={() => navigate('/clients')}
              className="rounded-md border border-gray-300 bg-white px-4 py-2 text-sm font-medium text-gray-700 hover:bg-gray-50 transition-colors"
            >
              Cancel
            </button>
          </div>
        </form>
      </div>
    </>
  )
}
