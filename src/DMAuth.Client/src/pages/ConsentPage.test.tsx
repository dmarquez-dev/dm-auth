import { describe, it, expect, vi, beforeEach, afterEach } from 'vitest'
import { screen } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { Route, Routes } from 'react-router-dom'
import { ConsentPage } from './ConsentPage'
import { renderWithProviders } from '../test/renderWithProviders'
import type { User } from '../types/auth'

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

const VALID_PARAMS =
  '?client_id=my-app' +
  '&redirect_uri=http%3A%2F%2Flocalhost%2Fcb' +
  '&scope=openid%20profile' +
  '&state=abc123' +
  '&code_challenge=challenge' +
  '&code_challenge_method=S256'

const setup = (url: string, user: User | null | undefined = testUser) =>
  renderWithProviders(
    <Routes>
      <Route path="/consent" element={<ConsentPage />} />
      <Route path="/login" element={<p>Login page</p>} />
    </Routes>,
    { initialEntries: [url], user }
  )

describe('ConsentPage', () => {
  beforeEach(() => { vi.clearAllMocks() })
  afterEach(() => { vi.unstubAllGlobals() })

  it('redirects to /login when the user is not authenticated', () => {
    setup(`/consent${VALID_PARAMS}`, null)
    expect(screen.getByText('Login page')).toBeInTheDocument()
  })

  it('shows an error message when required OAuth params are missing', () => {
    setup('/consent?client_id=only', testUser)
    expect(screen.getByText(/invalid authorization request/i)).toBeInTheDocument()
  })

  it('renders the client ID requesting access', () => {
    setup(`/consent${VALID_PARAMS}`, testUser)
    expect(screen.getByText('my-app')).toBeInTheDocument()
  })

  it('renders the requested scopes', () => {
    setup(`/consent${VALID_PARAMS}`, testUser)
    expect(screen.getAllByText('OpenID').length).toBeGreaterThanOrEqual(1)
    expect(screen.getByText('Verify your identity')).toBeInTheDocument()
    expect(screen.getByText('Profile')).toBeInTheDocument()
    expect(screen.getByText('Read your display name and username')).toBeInTheDocument()
  })

  it('renders the Allow access and Deny buttons', () => {
    setup(`/consent${VALID_PARAMS}`, testUser)
    expect(screen.getByRole('button', { name: /allow access/i })).toBeInTheDocument()
    expect(screen.getByRole('button', { name: /deny/i })).toBeInTheDocument()
  })

  it('shows the authenticated user display name', () => {
    setup(`/consent${VALID_PARAMS}`, testUser)
    expect(screen.getByText('Alice')).toBeInTheDocument()
  })

  it('clicking Deny sets window.location.href to the redirect_uri with error=access_denied', async () => {
    const user = userEvent.setup()
    let navigatedTo = ''
    vi.stubGlobal('location', {
      pathname: '/consent',
      search: VALID_PARAMS,
      get href() { return navigatedTo },
      set href(v: string) { navigatedTo = v },
    })

    setup(`/consent${VALID_PARAMS}`, testUser)
    await user.click(screen.getByRole('button', { name: /deny/i }))

    expect(navigatedTo).toContain('http://localhost/cb')
    expect(navigatedTo).toContain('error=access_denied')
    expect(navigatedTo).toContain('state=abc123')
  })
})
