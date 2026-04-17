import { describe, it, expect, vi, beforeEach } from 'vitest'
import { screen } from '@testing-library/react'
import { DashboardPage } from './DashboardPage'
import { renderWithProviders } from '../test/renderWithProviders'
import { clientApi } from '../api/clientApi'
import type { User } from '../types/auth'
import type { OAuthClient } from '../types/client'

vi.mock('../api/clientApi', () => ({
  clientApi: {
    listClients: vi.fn(),
    getClient: vi.fn(),
    createClient: vi.fn(),
    updateClient: vi.fn(),
    deleteClient: vi.fn(),
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

const makeClient = (overrides: Partial<OAuthClient> = {}): OAuthClient => ({
  clientId: 'c1',
  clientName: 'My App',
  oAuthClientId: 'oauth-c1',
  clientType: 'Public',
  isActive: true,
  redirectUris: ['http://localhost/cb'],
  allowedScopes: ['openid'],
  createdAt: '2024-01-01T00:00:00Z',
  updatedAt: null,
  ...overrides,
})

const setup = () =>
  renderWithProviders(<DashboardPage />, { user: testUser })

describe('DashboardPage', () => {
  beforeEach(() => { vi.clearAllMocks() })

  it('shows a welcome heading with the user display name', () => {
    vi.mocked(clientApi.listClients).mockReturnValue(new Promise(() => {}) as never)
    setup()
    expect(screen.getByText(/welcome back, alice/i)).toBeInTheDocument()
  })

  it('shows the user email below the heading', () => {
    vi.mocked(clientApi.listClients).mockReturnValue(new Promise(() => {}) as never)
    setup()
    expect(screen.getByText('alice@test.com')).toBeInTheDocument()
  })

  it('shows a loading skeleton for the client count while fetching', () => {
    vi.mocked(clientApi.listClients).mockReturnValue(new Promise(() => {}) as never)
    setup()
    expect(document.querySelector('.animate-pulse')).toBeInTheDocument()
  })

  it('shows the correct registered-client count once loaded', async () => {
    vi.mocked(clientApi.listClients).mockResolvedValueOnce({
      data: [makeClient({ clientId: 'c1' }), makeClient({ clientId: 'c2' })],
    } as never)
    setup()
    expect(await screen.findByText('2')).toBeInTheDocument()
  })

  it('renders the username stat card', () => {
    vi.mocked(clientApi.listClients).mockReturnValue(new Promise(() => {}) as never)
    setup()
    expect(screen.getByText('alice')).toBeInTheDocument()
  })

  it('renders quick-action links for New client, Manage clients, and Edit profile', () => {
    vi.mocked(clientApi.listClients).mockReturnValue(new Promise(() => {}) as never)
    setup()
    expect(screen.getByRole('link', { name: /new client/i })).toBeInTheDocument()
    expect(screen.getByRole('link', { name: /manage clients/i })).toBeInTheDocument()
    expect(screen.getByRole('link', { name: /edit profile/i })).toBeInTheDocument()
  })
})
