import { describe, it, expect, vi, beforeEach } from 'vitest'
import { screen } from '@testing-library/react'
import { ClientsPage } from './ClientsPage'
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
  renderWithProviders(<ClientsPage />, { user: testUser })

describe('ClientsPage', () => {
  beforeEach(() => { vi.clearAllMocks() })

  it('shows loading skeletons while the client list is being fetched', () => {
    vi.mocked(clientApi.listClients).mockReturnValue(new Promise(() => {}) as never)
    setup()
    // Three skeleton divs are rendered during loading
    expect(document.querySelectorAll('.animate-pulse')).toHaveLength(3)
  })

  it('shows an empty-state message when there are no clients', async () => {
    vi.mocked(clientApi.listClients).mockResolvedValueOnce({ data: [] } as never)
    setup()
    expect(await screen.findByText('No clients yet.')).toBeVisible()
  })

  it('renders a row for each client', async () => {
    const clients = [
      makeClient({ clientId: 'c1', clientName: 'App One', oAuthClientId: 'id-1' }),
      makeClient({ clientId: 'c2', clientName: 'App Two', oAuthClientId: 'id-2' }),
    ]
    vi.mocked(clientApi.listClients).mockResolvedValueOnce({ data: clients } as never)
    setup()
    expect(await screen.findByText('App One')).toBeVisible()
    expect(screen.getByText('App Two')).toBeVisible()
  })

  it('renders a "New client" link', async () => {
    vi.mocked(clientApi.listClients).mockResolvedValueOnce({ data: [] } as never)
    setup()
    expect(await screen.findByRole('link', { name: /new client/i })).toBeInTheDocument()
  })

  it('renders a "View" link for each client', async () => {
    const clients = [makeClient()]
    vi.mocked(clientApi.listClients).mockResolvedValueOnce({ data: clients } as never)
    setup()
    expect(await screen.findByRole('link', { name: /view/i })).toBeInTheDocument()
  })
})
