import { useForm } from 'react-hook-form'
import { zodResolver } from '@hookform/resolvers/zod'
import { z } from 'zod'
import { useMutation, useQueryClient } from '@tanstack/react-query'
import { isAxiosError } from 'axios'
import { useAuth } from '../auth/useAuth'
import { userApi } from '../api/userApi'

const profileSchema = z.object({
  displayName: z
    .string()
    .min(1, 'Display name is required')
    .max(100, 'Display name must be 100 characters or fewer'),
})

const passwordSchema = z
  .object({
    currentPassword: z.string().min(1, 'Current password is required'),
    newPassword: z.string().min(8, 'New password must be at least 8 characters'),
    confirmPassword: z.string().min(1, 'Please confirm your new password'),
  })
  .refine(values => values.newPassword === values.confirmPassword, {
    message: 'Passwords do not match',
    path: ['confirmPassword'],
  })

type ProfileFormValues = z.infer<typeof profileSchema>
type PasswordFormValues = z.infer<typeof passwordSchema>

function Field({
  label,
  value,
}: {
  label: string
  value: string
}) {
  return (
    <div>
      <p className="text-xs font-medium text-gray-500">{label}</p>
      <p className="mt-0.5 text-sm text-gray-900">{value}</p>
    </div>
  )
}

export function ProfilePage() {
  const { user } = useAuth()
  const queryClient = useQueryClient()

  const profileForm = useForm<ProfileFormValues>({
    resolver: zodResolver(profileSchema),
    values: { displayName: user?.displayName ?? '' },
  })

  const passwordForm = useForm<PasswordFormValues>({
    resolver: zodResolver(passwordSchema),
  })

  const updateProfile = useMutation({
    mutationFn: (values: ProfileFormValues) => userApi.updateProfile(values),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['auth', 'me'] })
      profileForm.reset(profileForm.getValues())
    },
    onError: (error) => {
      const message = isAxiosError(error)
        ? (error.response?.data?.message ?? 'Failed to update profile.')
        : 'Something went wrong.'
      profileForm.setError('root', { message })
    },
  })

  const changePassword = useMutation({
    mutationFn: (values: PasswordFormValues) =>
      userApi.changePassword({
        currentPassword: values.currentPassword,
        newPassword: values.newPassword,
      }),
    onSuccess: () => {
      passwordForm.reset()
    },
    onError: (error) => {
      if (isAxiosError(error) && error.response?.status === 401) {
        passwordForm.setError('currentPassword', { message: 'Incorrect password.' })
        return
      }
      passwordForm.setError('root', { message: 'Failed to change password. Please try again.' })
    },
  })

  return (
    <div className="max-w-xl">
      <h1 className="mb-8 text-2xl font-semibold text-gray-900">Profile</h1>

      {/* Account info */}
      <section className="mb-8 rounded-xl border border-gray-200 bg-white p-6">
        <h2 className="mb-4 text-sm font-semibold text-gray-700">Account</h2>
        <div className="flex flex-col gap-4">
          <Field label="Username" value={user?.username ?? '—'} />
          <Field label="Email" value={user?.email ?? '—'} />
          <div>
            <p className="text-xs font-medium text-gray-500">Email verified</p>
            <span className={`mt-0.5 inline-flex items-center rounded-full px-2 py-0.5 text-xs font-medium ${
              user?.emailVerified
                ? 'bg-green-50 text-green-700'
                : 'bg-yellow-50 text-yellow-700'
            }`}>
              {user?.emailVerified ? 'Verified' : 'Not verified'}
            </span>
          </div>
        </div>
      </section>

      {/* Edit display name */}
      <section className="mb-8 rounded-xl border border-gray-200 bg-white p-6">
        <h2 className="mb-4 text-sm font-semibold text-gray-700">Display name</h2>
        <form
          onSubmit={profileForm.handleSubmit(values => updateProfile.mutate(values))}
          noValidate
          className="flex flex-col gap-4"
        >
          <div>
            <input
              {...profileForm.register('displayName')}
              type="text"
              autoComplete="name"
              className="w-full rounded-md border border-gray-300 px-3 py-2 text-sm text-gray-900 focus:border-indigo-500 focus:outline-none focus:ring-1 focus:ring-indigo-500"
            />
            {profileForm.formState.errors.displayName && (
              <p className="mt-1 text-xs text-red-600">
                {profileForm.formState.errors.displayName.message}
              </p>
            )}
          </div>

          {profileForm.formState.errors.root && (
            <p className="rounded-md bg-red-50 px-3 py-2 text-sm text-red-700">
              {profileForm.formState.errors.root.message}
            </p>
          )}

          {updateProfile.isSuccess && (
            <p className="rounded-md bg-green-50 px-3 py-2 text-sm text-green-700">
              Display name updated.
            </p>
          )}

          <div>
            <button
              type="submit"
              disabled={updateProfile.isPending || !profileForm.formState.isDirty}
              className="rounded-md bg-indigo-600 px-4 py-2 text-sm font-medium text-white hover:bg-indigo-700 focus:outline-none focus:ring-2 focus:ring-indigo-500 focus:ring-offset-2 disabled:opacity-60 transition-colors"
            >
              {updateProfile.isPending ? 'Saving…' : 'Save'}
            </button>
          </div>
        </form>
      </section>

      {/* Change password */}
      <section className="rounded-xl border border-gray-200 bg-white p-6">
        <h2 className="mb-4 text-sm font-semibold text-gray-700">Change password</h2>
        <form
          onSubmit={passwordForm.handleSubmit(values => changePassword.mutate(values))}
          noValidate
          className="flex flex-col gap-4"
        >
          <div>
            <label htmlFor="currentPassword" className="mb-1 block text-sm font-medium text-gray-700">
              Current password
            </label>
            <input
              {...passwordForm.register('currentPassword')}
              id="currentPassword"
              type="password"
              autoComplete="current-password"
              className="w-full rounded-md border border-gray-300 px-3 py-2 text-sm text-gray-900 focus:border-indigo-500 focus:outline-none focus:ring-1 focus:ring-indigo-500"
            />
            {passwordForm.formState.errors.currentPassword && (
              <p className="mt-1 text-xs text-red-600">
                {passwordForm.formState.errors.currentPassword.message}
              </p>
            )}
          </div>

          <div>
            <label htmlFor="newPassword" className="mb-1 block text-sm font-medium text-gray-700">
              New password
            </label>
            <input
              {...passwordForm.register('newPassword')}
              id="newPassword"
              type="password"
              autoComplete="new-password"
              className="w-full rounded-md border border-gray-300 px-3 py-2 text-sm text-gray-900 focus:border-indigo-500 focus:outline-none focus:ring-1 focus:ring-indigo-500"
            />
            {passwordForm.formState.errors.newPassword && (
              <p className="mt-1 text-xs text-red-600">
                {passwordForm.formState.errors.newPassword.message}
              </p>
            )}
          </div>

          <div>
            <label htmlFor="confirmPassword" className="mb-1 block text-sm font-medium text-gray-700">
              Confirm new password
            </label>
            <input
              {...passwordForm.register('confirmPassword')}
              id="confirmPassword"
              type="password"
              autoComplete="new-password"
              className="w-full rounded-md border border-gray-300 px-3 py-2 text-sm text-gray-900 focus:border-indigo-500 focus:outline-none focus:ring-1 focus:ring-indigo-500"
            />
            {passwordForm.formState.errors.confirmPassword && (
              <p className="mt-1 text-xs text-red-600">
                {passwordForm.formState.errors.confirmPassword.message}
              </p>
            )}
          </div>

          {passwordForm.formState.errors.root && (
            <p className="rounded-md bg-red-50 px-3 py-2 text-sm text-red-700">
              {passwordForm.formState.errors.root.message}
            </p>
          )}

          {changePassword.isSuccess && (
            <p className="rounded-md bg-green-50 px-3 py-2 text-sm text-green-700">
              Password changed successfully.
            </p>
          )}

          <div>
            <button
              type="submit"
              disabled={changePassword.isPending}
              className="rounded-md bg-indigo-600 px-4 py-2 text-sm font-medium text-white hover:bg-indigo-700 focus:outline-none focus:ring-2 focus:ring-indigo-500 focus:ring-offset-2 disabled:opacity-60 transition-colors"
            >
              {changePassword.isPending ? 'Changing…' : 'Change password'}
            </button>
          </div>
        </form>
      </section>
    </div>
  )
}
