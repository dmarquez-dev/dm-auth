import { describe, it, expect, vi, beforeEach } from 'vitest'
import { screen } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { Route, Routes } from 'react-router-dom'
import { ClientDetailPage } from './ClientDetailPage'
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
  redirectUris: ['https://example.com/cb'],
  allowedScopes: ['openid', 'profile'],
  createdAt: '2024-01-15T00:00:00Z',
  updatedAt: null,
  ...overrides,
})

const setup = (clientId = 'c1') =>
  renderWithProviders(
    <Routes>
      <Route path="/clients/:id" element={<ClientDetailPage />} />
      <Route path="/clients" element={<p>Clients page</p>} />
    </Routes>,
    { initialEntries: [`/clients/${clientId}`], user: testUser }
  )

describe('ClientDetailPage — loading and error states', () => {
  beforeEach(() => { vi.clearAllMocks() })

  it('shows a loading skeleton while the client is being fetched', () => {
    vi.mocked(clientApi.getClient).mockReturnValue(new Promise(() => {}) as never)
    setup()
    expect(document.querySelector('.animate-pulse')).toBeInTheDocument()
  })

  it('shows an error message when the fetch fails', async () => {
    vi.mocked(clientApi.getClient).mockRejectedValueOnce(new Error('Not found'))
    setup()
    expect(await screen.findByText(/client not found/i)).toBeVisible()
  })
})

describe('ClientDetailPage — loaded state', () => {
  beforeEach(() => { vi.clearAllMocks() })

  it('renders the client name as the page heading', async () => {
    vi.mocked(clientApi.getClient).mockResolvedValueOnce({ data: makeClient() } as never)
    setup()
    expect(await screen.findByRole('heading', { name: 'My App' })).toBeVisible()
  })

  it('renders the oAuthClientId below the heading', async () => {
    vi.mocked(clientApi.getClient).mockResolvedValueOnce({ data: makeClient() } as never)
    setup()
    expect(await screen.findByText('oauth-c1')).toBeVisible()
  })

  it('renders the client type in the details section', async () => {
    vi.mocked(clientApi.getClient).mockResolvedValueOnce({ data: makeClient() } as never)
    setup()
    expect(await screen.findByText('Public')).toBeVisible()
  })

  it('renders Active status for an active client', async () => {
    vi.mocked(clientApi.getClient).mockResolvedValueOnce({ data: makeClient({ isActive: true }) } as never)
    setup()
    expect(await screen.findByText('Active')).toBeVisible()
  })

  it('renders Inactive status for an inactive client', async () => {
    vi.mocked(clientApi.getClient).mockResolvedValueOnce({ data: makeClient({ isActive: false }) } as never)
    setup()
    expect(await screen.findByText('Inactive')).toBeVisible()
  })

  it('pre-populates the name input with the client name', async () => {
    vi.mocked(clientApi.getClient).mockResolvedValueOnce({ data: makeClient() } as never)
    setup()
    expect(await screen.findByDisplayValue('My App')).toBeInTheDocument()
  })

  it('pre-populates redirect URIs from the client data', async () => {
    vi.mocked(clientApi.getClient).mockResolvedValueOnce({ data: makeClient() } as never)
    setup()
    expect(await screen.findByDisplayValue('https://example.com/cb')).toBeInTheDocument()
  })

  it('pre-checks the scopes that are already allowed', async () => {
    vi.mocked(clientApi.getClient).mockResolvedValueOnce({ data: makeClient() } as never)
    setup()
    await screen.findByRole('heading', { name: 'My App' })
    expect(screen.getByRole('checkbox', { name: /openid/i })).toBeChecked()
    expect(screen.getByRole('checkbox', { name: /profile/i })).toBeChecked()
    expect(screen.getByRole('checkbox', { name: /^email$/i })).not.toBeChecked()
  })
})

describe('ClientDetailPage — update', () => {
  beforeEach(() => { vi.clearAllMocks() })

  it('shows "Client updated." after a successful save', async () => {
    const user = userEvent.setup()
    vi.mocked(clientApi.getClient).mockResolvedValueOnce({ data: makeClient() } as never)
    vi.mocked(clientApi.updateClient).mockResolvedValueOnce({ data: undefined } as never)
    setup()
    const nameInput = await screen.findByDisplayValue('My App')
    await user.clear(nameInput)
    await user.type(nameInput, 'Updated App')
    await user.click(screen.getByRole('button', { name: /save changes/i }))
    expect(await screen.findByText('Client updated.')).toBeVisible()
    expect(clientApi.updateClient).toHaveBeenCalledWith('c1', expect.objectContaining({ clientName: 'Updated App' }))
  })

  it('shows a root error message when the update API call fails', async () => {
    const user = userEvent.setup()
    const error = Object.assign(new Error('Bad request'), {
      isAxiosError: true,
      response: { status: 400, data: { message: 'Invalid redirect URI.' } },
    })
    vi.mocked(clientApi.getClient).mockResolvedValueOnce({ data: makeClient() } as never)
    vi.mocked(clientApi.updateClient).mockRejectedValueOnce(error)
    setup()
    const nameInput = await screen.findByDisplayValue('My App')
    await user.clear(nameInput)
    await user.type(nameInput, 'Updated App')
    await user.click(screen.getByRole('button', { name: /save changes/i }))
    expect(await screen.findByText('Invalid redirect URI.')).toBeVisible()
  })
})

describe('ClientDetailPage — delete', () => {
  beforeEach(() => { vi.clearAllMocks() })

  it('shows the delete confirmation modal when the Delete client button is clicked', async () => {
    const user = userEvent.setup()
    vi.mocked(clientApi.getClient).mockResolvedValueOnce({ data: makeClient() } as never)
    setup()
    await screen.findByRole('heading', { name: 'My App' })
    await user.click(screen.getByRole('button', { name: /delete client/i }))
    expect(screen.getByText(/will be permanently deleted/i)).toBeVisible()
  })

  it('closes the modal when Cancel is clicked in the confirmation dialog', async () => {
    const user = userEvent.setup()
    vi.mocked(clientApi.getClient).mockResolvedValueOnce({ data: makeClient() } as never)
    setup()
    await screen.findByRole('heading', { name: 'My App' })
    await user.click(screen.getByRole('button', { name: /delete client/i }))
    await user.click(screen.getByRole('button', { name: /^cancel$/i }))
    expect(screen.queryByText(/will be permanently deleted/i)).not.toBeInTheDocument()
  })

  it('calls deleteClient and navigates to /clients after confirming delete', async () => {
    const user = userEvent.setup()
    vi.mocked(clientApi.getClient).mockResolvedValueOnce({ data: makeClient() } as never)
    vi.mocked(clientApi.deleteClient).mockResolvedValueOnce({ data: undefined } as never)
    setup()
    await screen.findByRole('heading', { name: 'My App' })
    await user.click(screen.getByRole('button', { name: /delete client/i }))
    await user.click(screen.getByRole('button', { name: /^delete$/i }))
    expect(await screen.findByText('Clients page')).toBeVisible()
    expect(clientApi.deleteClient).toHaveBeenCalledWith('c1')
  })

  it('shows "Deleting…" while the delete mutation is in flight', async () => {
    const user = userEvent.setup()
    vi.mocked(clientApi.getClient).mockResolvedValueOnce({ data: makeClient() } as never)
    vi.mocked(clientApi.deleteClient).mockReturnValue(new Promise(() => {}) as never)
    setup()
    await screen.findByRole('heading', { name: 'My App' })
    await user.click(screen.getByRole('button', { name: /delete client/i }))
    await user.click(screen.getByRole('button', { name: /^delete$/i }))
    expect(await screen.findByRole('button', { name: /deleting/i })).toBeInTheDocument()
  })
})

describe('ClientDetailPage — scope and URI interactions', () => {
  beforeEach(() => { vi.clearAllMocks() })

  it('toggles an unchecked scope to checked', async () => {
    const user = userEvent.setup()
    vi.mocked(clientApi.getClient).mockResolvedValueOnce({ data: makeClient() } as never)
    setup()
    await screen.findByRole('heading', { name: 'My App' })
    const emailCheckbox = screen.getByRole('checkbox', { name: /^email$/i })
    expect(emailCheckbox).not.toBeChecked()
    await user.click(emailCheckbox)
    expect(emailCheckbox).toBeChecked()
  })

  it('toggles a checked scope to unchecked', async () => {
    const user = userEvent.setup()
    vi.mocked(clientApi.getClient).mockResolvedValueOnce({ data: makeClient() } as never)
    setup()
    await screen.findByRole('heading', { name: 'My App' })
    const openidCheckbox = screen.getByRole('checkbox', { name: /openid/i })
    expect(openidCheckbox).toBeChecked()
    await user.click(openidCheckbox)
    expect(openidCheckbox).not.toBeChecked()
  })

  it('appends a new redirect URI field when "+ Add redirect URI" is clicked', async () => {
    const user = userEvent.setup()
    vi.mocked(clientApi.getClient).mockResolvedValueOnce({ data: makeClient() } as never)
    setup()
    await screen.findByRole('heading', { name: 'My App' })
    await user.click(screen.getByRole('button', { name: /\+ add redirect uri/i }))
    // With 2 fields, the Remove button becomes visible for each
    expect(screen.getAllByRole('button', { name: /remove/i })).toHaveLength(2)
  })

  it('removes a redirect URI field when Remove is clicked', async () => {
    const user = userEvent.setup()
    vi.mocked(clientApi.getClient).mockResolvedValueOnce({ data: makeClient() } as never)
    setup()
    await screen.findByRole('heading', { name: 'My App' })
    await user.click(screen.getByRole('button', { name: /\+ add redirect uri/i }))
    const removeButtons = screen.getAllByRole('button', { name: /remove/i })
    await user.click(removeButtons[0])
    expect(screen.queryByRole('button', { name: /remove/i })).not.toBeInTheDocument()
  })

  it('Discard resets the name input to its original value', async () => {
    const user = userEvent.setup()
    vi.mocked(clientApi.getClient).mockResolvedValueOnce({ data: makeClient() } as never)
    setup()
    const nameInput = await screen.findByDisplayValue('My App')
    await user.clear(nameInput)
    await user.type(nameInput, 'Changed Name')
    await user.click(screen.getByRole('button', { name: /discard/i }))
    expect(screen.getByDisplayValue('My App')).toBeInTheDocument()
  })
})

describe('ClientDetailPage — mutation pending states', () => {
  beforeEach(() => { vi.clearAllMocks() })

  it('shows "Saving…" while the update mutation is in flight', async () => {
    const user = userEvent.setup()
    vi.mocked(clientApi.getClient).mockResolvedValueOnce({ data: makeClient() } as never)
    vi.mocked(clientApi.updateClient).mockReturnValue(new Promise(() => {}) as never)
    setup()
    const nameInput = await screen.findByDisplayValue('My App')
    await user.clear(nameInput)
    await user.type(nameInput, 'Updated App')
    await user.click(screen.getByRole('button', { name: /save changes/i }))
    expect(await screen.findByRole('button', { name: /saving/i })).toBeInTheDocument()
  })
})

describe('ClientDetailPage — update API error edge cases', () => {
  beforeEach(() => { vi.clearAllMocks() })

  it('shows "Something went wrong." for a non-Axios update error', async () => {
    const user = userEvent.setup()
    vi.mocked(clientApi.getClient).mockResolvedValueOnce({ data: makeClient() } as never)
    vi.mocked(clientApi.updateClient).mockRejectedValueOnce(new Error('Network failure'))
    setup()
    const nameInput = await screen.findByDisplayValue('My App')
    await user.clear(nameInput)
    await user.type(nameInput, 'Updated App')
    await user.click(screen.getByRole('button', { name: /save changes/i }))
    expect(await screen.findByText('Something went wrong.')).toBeVisible()
  })

  it('shows "Failed to update client." when the Axios error has no message body', async () => {
    const user = userEvent.setup()
    const error = Object.assign(new Error('Server Error'), {
      isAxiosError: true,
      response: { status: 500, data: {} },
    })
    vi.mocked(clientApi.getClient).mockResolvedValueOnce({ data: makeClient() } as never)
    vi.mocked(clientApi.updateClient).mockRejectedValueOnce(error)
    setup()
    const nameInput = await screen.findByDisplayValue('My App')
    await user.clear(nameInput)
    await user.type(nameInput, 'Updated App')
    await user.click(screen.getByRole('button', { name: /save changes/i }))
    expect(await screen.findByText('Failed to update client.')).toBeVisible()
  })
})
