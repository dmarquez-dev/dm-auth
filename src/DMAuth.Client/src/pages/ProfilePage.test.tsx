import { describe, it, expect, vi, beforeEach } from 'vitest'
import { screen } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { ProfilePage } from './ProfilePage'
import { renderWithProviders } from '../test/renderWithProviders'
import { userApi } from '../api/userApi'
import type { User } from '../types/auth'

vi.mock('../api/userApi', () => ({
  userApi: {
    getProfile: vi.fn(),
    updateProfile: vi.fn(),
    changePassword: vi.fn(),
  },
}))

vi.mock('../api/authApi', () => ({
  authApi: {
    me: vi.fn(() => new Promise(() => {})),
    login: vi.fn(),
    logout: vi.fn(),
    register: vi.fn(),
  },
}))

const testUser: User = {
  userId: 'u1',
  username: 'alice',
  email: 'alice@test.com',
  displayName: 'Alice',
  emailVerified: true,
}

const setup = (user: User = testUser) =>
  renderWithProviders(<ProfilePage />, { user })

describe('ProfilePage — account info', () => {
  beforeEach(() => { vi.clearAllMocks() })

  it('displays the username', () => {
    setup()
    expect(screen.getByText(testUser.username)).toBeInTheDocument()
  })

  it('displays the email', () => {
    setup()
    expect(screen.getByText(testUser.email)).toBeInTheDocument()
  })

  it('shows "Verified" badge when email is verified', () => {
    setup()
    expect(screen.getByText('Verified')).toBeInTheDocument()
  })

  it('shows "Not verified" badge when email is not verified', () => {
    setup({ ...testUser, emailVerified: false })
    expect(screen.getByText('Not verified')).toBeInTheDocument()
  })

  it('pre-populates the display-name input with the current display name', () => {
    setup()
    expect(screen.getByDisplayValue(testUser.displayName)).toBeInTheDocument()
  })
})

describe('ProfilePage — update display name', () => {
  beforeEach(() => { vi.clearAllMocks() })

  it('shows "Display name updated." after a successful update', async () => {
    const user = userEvent.setup()
    vi.mocked(userApi.updateProfile).mockResolvedValueOnce({ data: { ...testUser, displayName: 'New Name' } } as never)
    setup()

    const input = screen.getByDisplayValue(testUser.displayName)
    await user.clear(input)
    await user.type(input, 'New Name')
    await user.click(screen.getByRole('button', { name: /^save$/i }))

    expect(await screen.findByText('Display name updated.')).toBeVisible()
    expect(userApi.updateProfile).toHaveBeenCalledWith({ displayName: 'New Name' })
  })

  it('shows a validation error when the display name is cleared', async () => {
    const user = userEvent.setup()
    setup()

    const input = screen.getByDisplayValue(testUser.displayName)
    await user.clear(input)
    await user.click(screen.getByRole('button', { name: /^save$/i }))

    expect(await screen.findByText('Display name is required')).toBeVisible()
  })
})

describe('ProfilePage — change password', () => {
  beforeEach(() => { vi.clearAllMocks() })

  it('shows "Password changed successfully." after a successful change', async () => {
    const user = userEvent.setup()
    vi.mocked(userApi.changePassword).mockResolvedValueOnce({ data: undefined } as never)
    setup()

    await user.type(screen.getByLabelText(/current password/i), 'OldPass1!')
    await user.type(screen.getByLabelText(/^new password$/i), 'NewPass1!')
    await user.type(screen.getByLabelText(/confirm new password/i), 'NewPass1!')
    await user.click(screen.getByRole('button', { name: /change password/i }))

    expect(await screen.findByText('Password changed successfully.')).toBeVisible()
    expect(userApi.changePassword).toHaveBeenCalledWith({
      currentPassword: 'OldPass1!',
      newPassword: 'NewPass1!',
    })
  })

  it('shows "Incorrect password." when the current password is wrong (401)', async () => {
    const user = userEvent.setup()
    const error = Object.assign(new Error('Unauthorized'), {
      isAxiosError: true,
      response: { status: 401 },
    })
    vi.mocked(userApi.changePassword).mockRejectedValueOnce(error)
    setup()

    await user.type(screen.getByLabelText(/current password/i), 'WrongPass1!')
    await user.type(screen.getByLabelText(/^new password$/i), 'NewPass1!')
    await user.type(screen.getByLabelText(/confirm new password/i), 'NewPass1!')
    await user.click(screen.getByRole('button', { name: /change password/i }))

    expect(await screen.findByText('Incorrect password.')).toBeVisible()
  })

  it('shows a generic error for non-401 failures', async () => {
    const user = userEvent.setup()
    vi.mocked(userApi.changePassword).mockRejectedValueOnce(new Error('Network error'))
    setup()

    await user.type(screen.getByLabelText(/current password/i), 'OldPass1!')
    await user.type(screen.getByLabelText(/^new password$/i), 'NewPass1!')
    await user.type(screen.getByLabelText(/confirm new password/i), 'NewPass1!')
    await user.click(screen.getByRole('button', { name: /change password/i }))

    expect(await screen.findByText('Failed to change password. Please try again.')).toBeVisible()
  })

  it('shows a validation error when new passwords do not match', async () => {
    const user = userEvent.setup()
    setup()

    await user.type(screen.getByLabelText(/current password/i), 'OldPass1!')
    await user.type(screen.getByLabelText(/^new password$/i), 'NewPass1!')
    await user.type(screen.getByLabelText(/confirm new password/i), 'DifferentPass1!')
    await user.click(screen.getByRole('button', { name: /change password/i }))

    expect(await screen.findByText('Passwords do not match')).toBeVisible()
  })
})
