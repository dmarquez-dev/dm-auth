import { describe, it, expect, vi, beforeEach, afterEach } from 'vitest'
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
      <Route path="/clients" element={<p>Clients page</p>} />
    </Routes>,
    { initialEntries: [url], user: null }
  )

// Replaces window.location with a stub whose assign() is a spy, keeping the real origin
// so the API base URL still resolves against it.
const stubLocationAssign = () => {
  const assign = vi.fn()
  vi.stubGlobal('location', {
    origin: window.location.origin,
    pathname: '/login',
    search: '',
    href: `${window.location.origin}/login`,
    assign,
  })
  return assign
}

const signIn = async (user: ReturnType<typeof userEvent.setup>) => {
  vi.mocked(authApi.login).mockResolvedValueOnce({ data: {} } as never)
  // invalidateQueries awaits the ['auth', 'me'] refetch — resolve it so the redirect is reached
  vi.mocked(authApi.me).mockResolvedValueOnce({
    data: { userId: 'u1', username: 'alice', email: 'a@test.com', displayName: 'Alice', emailVerified: true },
  } as never)
  await user.type(screen.getByLabelText(/email/i), 'a@test.com')
  await user.type(screen.getByLabelText(/password/i), 'Secret1!')
  await user.click(screen.getByRole('button', { name: /sign in/i }))
}

describe('LoginPage', () => {
  beforeEach(() => { vi.clearAllMocks() })
  afterEach(() => {
    vi.unstubAllGlobals()
    vi.unstubAllEnvs()
  })

  it('navigates through the router to an app-relative returnUrl after a successful login', async () => {
    const user = userEvent.setup()
    const assign = stubLocationAssign()
    setup('/login?returnUrl=%2Fclients')
    await signIn(user)
    expect(await screen.findByText('Clients page')).toBeVisible()
    expect(assign).not.toHaveBeenCalled()
  })

  it('leaves the SPA for the API /connect/authorize returnUrl after a successful login', async () => {
    vi.stubEnv('VITE_API_BASE_URL', 'https://api.example.test/')
    const returnUrl = 'https://api.example.test/connect/authorize?client_id=dmauth_x&state=s1'
    const user = userEvent.setup()
    const assign = stubLocationAssign()
    setup(`/login?returnUrl=${encodeURIComponent(returnUrl)}`)
    await signIn(user)
    await vi.waitFor(() => expect(assign).toHaveBeenCalledWith(returnUrl))
    expect(assign).toHaveBeenCalledTimes(1)
    expect(screen.queryByText('Dashboard page')).not.toBeInTheDocument()
  })

  it.each([
    ['a protocol-relative URL', undefined, '//example.org'],
    ['an /connect/authorize URL on a foreign origin', 'https://api.example.test/', 'https://evil.example/connect/authorize?client_id=dmauth_x'],
    ['an API-origin URL that is not /connect/authorize', 'https://api.example.test/', 'https://api.example.test/api/users/me'],
  ])('falls back to /dashboard for %s after a successful login', async (_label, apiBaseUrl, returnUrl) => {
    vi.stubEnv('VITE_API_BASE_URL', apiBaseUrl)
    const user = userEvent.setup()
    const assign = stubLocationAssign()
    setup(`/login?returnUrl=${encodeURIComponent(returnUrl)}`)
    await signIn(user)
    expect(await screen.findByText('Dashboard page')).toBeVisible()
    expect(assign).not.toHaveBeenCalled()
  })

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
