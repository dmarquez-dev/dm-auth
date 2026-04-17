import { describe, it, expect, vi, beforeEach } from 'vitest'
import { screen } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { Route, Routes } from 'react-router-dom'
import { RegisterPage } from './RegisterPage'
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

const setup = () =>
  renderWithProviders(
    <Routes>
      <Route path="/register" element={<RegisterPage />} />
      <Route path="/login" element={<p>Login page</p>} />
    </Routes>,
    { initialEntries: ['/register'], user: null }
  )

const fillValidForm = async (user: ReturnType<typeof userEvent.setup>) => {
  await user.type(screen.getByLabelText(/^email$/i), 'alice@test.com')
  await user.type(screen.getByLabelText(/^username$/i), 'alice')
  await user.type(screen.getByLabelText(/^display name$/i), 'Alice')
  await user.type(screen.getByLabelText(/^password$/i), 'Secure1!')
  await user.type(screen.getByLabelText(/confirm password/i), 'Secure1!')
}

describe('RegisterPage', () => {
  beforeEach(() => { vi.clearAllMocks() })

  it('renders all form fields and the submit button', () => {
    setup()
    expect(screen.getByLabelText(/^email$/i)).toBeInTheDocument()
    expect(screen.getByLabelText(/^username$/i)).toBeInTheDocument()
    expect(screen.getByLabelText(/^display name$/i)).toBeInTheDocument()
    expect(screen.getByLabelText(/^password$/i)).toBeInTheDocument()
    expect(screen.getByLabelText(/confirm password/i)).toBeInTheDocument()
    expect(screen.getByRole('button', { name: /create account/i })).toBeInTheDocument()
  })

  it('shows "Email is required" when submitted with an empty email', async () => {
    const user = userEvent.setup()
    setup()
    await user.click(screen.getByRole('button', { name: /create account/i }))
    expect(await screen.findByText('Email is required')).toBeVisible()
  })

  it('shows "Enter a valid email" for a malformed email', async () => {
    const user = userEvent.setup()
    setup()
    await user.type(screen.getByLabelText(/^email$/i), 'notanemail')
    await user.click(screen.getByRole('button', { name: /create account/i }))
    expect(await screen.findByText('Enter a valid email')).toBeVisible()
  })

  it('shows a min-length error for a short password', async () => {
    const user = userEvent.setup()
    setup()
    await user.type(screen.getByLabelText(/^password$/i), 'short')
    await user.click(screen.getByRole('button', { name: /create account/i }))
    expect(await screen.findByText('Password must be at least 8 characters')).toBeVisible()
  })

  it('shows "Passwords do not match" when confirm password differs', async () => {
    const user = userEvent.setup()
    setup()
    await user.type(screen.getByLabelText(/^email$/i), 'alice@test.com')
    await user.type(screen.getByLabelText(/^username$/i), 'alice')
    await user.type(screen.getByLabelText(/^display name$/i), 'Alice')
    await user.type(screen.getByLabelText(/^password$/i), 'Secure1!')
    await user.type(screen.getByLabelText(/confirm password/i), 'Different1!')
    await user.click(screen.getByRole('button', { name: /create account/i }))
    expect(await screen.findByText('Passwords do not match')).toBeVisible()
  })

  it('calls authApi.register with the correct payload on a valid submit', async () => {
    const user = userEvent.setup()
    vi.mocked(authApi.register).mockResolvedValueOnce({ data: { userId: 'u1' } } as never)
    setup()
    await fillValidForm(user)
    await user.click(screen.getByRole('button', { name: /create account/i }))
    expect(authApi.register).toHaveBeenCalledWith({
      email: 'alice@test.com',
      username: 'alice',
      displayName: 'Alice',
      password: 'Secure1!',
    })
  })

  it('navigates to /login?registered=1 after a successful registration', async () => {
    const user = userEvent.setup()
    vi.mocked(authApi.register).mockResolvedValueOnce({ data: { userId: 'u1' } } as never)
    setup()
    await fillValidForm(user)
    await user.click(screen.getByRole('button', { name: /create account/i }))
    expect(await screen.findByText('Login page')).toBeVisible()
  })

  it('shows a conflict error when the email is already in use (409)', async () => {
    const user = userEvent.setup()
    const error = Object.assign(new Error('Conflict'), {
      isAxiosError: true,
      response: { status: 409 },
    })
    vi.mocked(authApi.register).mockRejectedValueOnce(error)
    setup()
    await fillValidForm(user)
    await user.click(screen.getByRole('button', { name: /create account/i }))
    expect(await screen.findByText('An account with this email already exists.')).toBeVisible()
  })

  it('shows a username validation error on a 400 response with Username detail', async () => {
    const user = userEvent.setup()
    const error = Object.assign(new Error('Bad Request'), {
      isAxiosError: true,
      response: {
        status: 400,
        data: { errors: { Username: ['Username is already taken.'] } },
      },
    })
    vi.mocked(authApi.register).mockRejectedValueOnce(error)
    setup()
    await fillValidForm(user)
    await user.click(screen.getByRole('button', { name: /create account/i }))
    expect(await screen.findByText('Username is already taken.')).toBeVisible()
  })

  it('shows a generic error message on an unexpected failure', async () => {
    const user = userEvent.setup()
    vi.mocked(authApi.register).mockRejectedValueOnce(new Error('Network Error'))
    setup()
    await fillValidForm(user)
    await user.click(screen.getByRole('button', { name: /create account/i }))
    expect(await screen.findByText('Something went wrong. Please try again.')).toBeVisible()
  })
})
