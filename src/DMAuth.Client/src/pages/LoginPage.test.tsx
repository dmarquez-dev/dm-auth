import { describe, it, expect, vi, beforeEach } from 'vitest'
import { screen } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { Route, Routes } from 'react-router-dom'
import { LoginPage } from './LoginPage'
import { renderWithProviders } from '../test/renderWithProviders'
import { authApi } from '../api/authApi'

vi.mock('../api/authApi', () => ({
  authApi: {
    login: vi.fn(),
    register: vi.fn(),
    logout: vi.fn(),
    me: vi.fn(() => new Promise(() => {})),
  },
}))

const setup = (url = '/login') =>
  renderWithProviders(
    <Routes>
      <Route path="/login" element={<LoginPage />} />
      <Route path="/dashboard" element={<p>Dashboard page</p>} />
    </Routes>,
    { initialEntries: [url], user: null }
  )

describe('LoginPage', () => {
  beforeEach(() => { vi.clearAllMocks() })

  it('renders email field, password field, and submit button', () => {
    setup()
    expect(screen.getByLabelText(/email/i)).toBeInTheDocument()
    expect(screen.getByLabelText(/password/i)).toBeInTheDocument()
    expect(screen.getByRole('button', { name: /sign in/i })).toBeInTheDocument()
  })

  it('shows validation errors when the form is submitted empty', async () => {
    const user = userEvent.setup()
    setup()
    await user.click(screen.getByRole('button', { name: /sign in/i }))
    expect(await screen.findByText('Email is required')).toBeVisible()
    expect(screen.getByText('Password is required')).toBeVisible()
  })

  it('shows a validation error for an invalid email format', async () => {
    const user = userEvent.setup()
    setup()
    await user.type(screen.getByLabelText(/email/i), 'notanemail')
    await user.click(screen.getByRole('button', { name: /sign in/i }))
    expect(await screen.findByText('Enter a valid email')).toBeVisible()
  })

  it('calls authApi.login with the entered credentials', async () => {
    const user = userEvent.setup()
    vi.mocked(authApi.login).mockResolvedValueOnce({ data: {} } as never)
    setup()
    await user.type(screen.getByLabelText(/email/i), 'a@test.com')
    await user.type(screen.getByLabelText(/password/i), 'Secret1!')
    await user.click(screen.getByRole('button', { name: /sign in/i }))
    expect(authApi.login).toHaveBeenCalledWith({ email: 'a@test.com', password: 'Secret1!' })
  })

  it('navigates to /dashboard after a successful login', async () => {
    const user = userEvent.setup()
    vi.mocked(authApi.login).mockResolvedValueOnce({ data: {} } as never)
    // invalidateQueries awaits the ['auth', 'me'] refetch — resolve it so navigate() is reached
    vi.mocked(authApi.me).mockResolvedValueOnce({
      data: { userId: 'u1', username: 'alice', email: 'a@test.com', displayName: 'Alice', emailVerified: true },
    } as never)
    setup()
    await user.type(screen.getByLabelText(/email/i), 'a@test.com')
    await user.type(screen.getByLabelText(/password/i), 'Secret1!')
    await user.click(screen.getByRole('button', { name: /sign in/i }))
    expect(await screen.findByText('Dashboard page')).toBeVisible()
  })

  it('shows "Incorrect email or password" on a 401 response', async () => {
    const user = userEvent.setup()
    const error = Object.assign(new Error('Unauthorized'), {
      isAxiosError: true,
      response: { status: 401 },
    })
    vi.mocked(authApi.login).mockRejectedValueOnce(error)
    setup()
    await user.type(screen.getByLabelText(/email/i), 'a@test.com')
    await user.type(screen.getByLabelText(/password/i), 'wrong')
    await user.click(screen.getByRole('button', { name: /sign in/i }))
    expect(await screen.findByText('Incorrect email or password.')).toBeVisible()
  })

  it('shows a generic error message on a non-401 failure', async () => {
    const user = userEvent.setup()
    vi.mocked(authApi.login).mockRejectedValueOnce(new Error('Network Error'))
    setup()
    await user.type(screen.getByLabelText(/email/i), 'a@test.com')
    await user.type(screen.getByLabelText(/password/i), 'Secret1!')
    await user.click(screen.getByRole('button', { name: /sign in/i }))
    expect(await screen.findByText('Something went wrong. Please try again.')).toBeVisible()
  })

  it('shows the "Account created" banner when ?registered=1 is in the URL', () => {
    setup('/login?registered=1')
    expect(screen.getByText(/account created/i)).toBeVisible()
  })
})
