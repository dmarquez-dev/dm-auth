import { describe, it, expect, vi, beforeEach, afterEach } from 'vitest'
import { screen } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { Route, Routes } from 'react-router-dom'
import { ClientCreatePage } from './ClientCreatePage'
import { renderWithProviders } from '../test/renderWithProviders'
import { clientApi } from '../api/clientApi'
import type { User } from '../types/auth'

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

const setup = () =>
  renderWithProviders(
    <Routes>
      <Route path="/clients/new" element={<ClientCreatePage />} />
      <Route path="/clients" element={<p>Clients page</p>} />
      <Route path="/clients/:id" element={<p>Client detail page</p>} />
    </Routes>,
    { initialEntries: ['/clients/new'], user: testUser }
  )

const fillValidForm = async (user: ReturnType<typeof userEvent.setup>) => {
  await user.type(screen.getByLabelText(/^name$/i), 'My App')
  await user.type(screen.getByPlaceholderText(/https:\/\/example.com\/callback/i), 'https://example.com/cb')
}

describe('ClientCreatePage — rendering', () => {
  beforeEach(() => { vi.clearAllMocks() })

  it('renders the name input', () => {
    setup()
    expect(screen.getByLabelText(/^name$/i)).toBeInTheDocument()
  })

  it('renders Public and Confidential client type radio buttons', () => {
    setup()
    expect(screen.getByRole('radio', { name: /public/i })).toBeInTheDocument()
    expect(screen.getByRole('radio', { name: /confidential/i })).toBeInTheDocument()
  })

  it('renders scope checkboxes for all available scopes', () => {
    setup()
    expect(screen.getByRole('checkbox', { name: /openid/i })).toBeInTheDocument()
    expect(screen.getByRole('checkbox', { name: /profile/i })).toBeInTheDocument()
    expect(screen.getByRole('checkbox', { name: /email/i })).toBeInTheDocument()
    expect(screen.getByRole('checkbox', { name: /offline_access/i })).toBeInTheDocument()
  })

  it('renders the Register client and Cancel buttons', () => {
    setup()
    expect(screen.getByRole('button', { name: /register client/i })).toBeInTheDocument()
    expect(screen.getByRole('button', { name: /cancel/i })).toBeInTheDocument()
  })

  it('openid scope is pre-selected by default', () => {
    setup()
    expect(screen.getByRole('checkbox', { name: /openid/i })).toBeChecked()
  })

  it('Public is the default client type', () => {
    setup()
    expect(screen.getByRole('radio', { name: /public/i })).toBeChecked()
  })
})

describe('ClientCreatePage — validation', () => {
  beforeEach(() => { vi.clearAllMocks() })

  it('shows "Name is required" when submitted without a name', async () => {
    const user = userEvent.setup()
    setup()
    await user.click(screen.getByRole('button', { name: /register client/i }))
    expect(await screen.findByText('Name is required')).toBeVisible()
  })

  it('shows a URL validation error for a non-URL redirect URI', async () => {
    const user = userEvent.setup()
    setup()
    await user.type(screen.getByLabelText(/^name$/i), 'My App')
    await user.type(screen.getByPlaceholderText(/https:\/\/example.com\/callback/i), 'not-a-url')
    await user.click(screen.getByRole('button', { name: /register client/i }))
    expect(await screen.findByText(/must be a valid url/i)).toBeVisible()
  })

  it('shows "Select at least one scope" when all scopes are deselected', async () => {
    const user = userEvent.setup()
    setup()
    // openid is pre-checked — uncheck it
    await user.click(screen.getByRole('checkbox', { name: /openid/i }))
    await user.type(screen.getByLabelText(/^name$/i), 'My App')
    await user.type(screen.getByPlaceholderText(/https:\/\/example.com\/callback/i), 'https://example.com/cb')
    await user.click(screen.getByRole('button', { name: /register client/i }))
    expect(await screen.findByText(/select at least one scope/i)).toBeVisible()
  })
})

describe('ClientCreatePage — redirect URI field array', () => {
  beforeEach(() => { vi.clearAllMocks() })

  it('"+ Add redirect URI" appends a new input', async () => {
    const user = userEvent.setup()
    setup()
    const before = screen.getAllByPlaceholderText(/https:\/\/example.com\/callback/i).length
    await user.click(screen.getByRole('button', { name: /\+ add redirect uri/i }))
    expect(screen.getAllByPlaceholderText(/https:\/\/example.com\/callback/i)).toHaveLength(before + 1)
  })

  it('Remove button removes a URI field when there are multiple', async () => {
    const user = userEvent.setup()
    setup()
    await user.click(screen.getByRole('button', { name: /\+ add redirect uri/i }))
    const removeButtons = screen.getAllByRole('button', { name: /remove/i })
    expect(removeButtons.length).toBeGreaterThanOrEqual(1)
    await user.click(removeButtons[0])
    expect(screen.getAllByPlaceholderText(/https:\/\/example.com\/callback/i)).toHaveLength(1)
  })
})

describe('ClientCreatePage — API interaction', () => {
  beforeEach(() => { vi.clearAllMocks() })

  it('shows the SecretModal with the client ID after a successful Public client creation', async () => {
    const user = userEvent.setup()
    vi.mocked(clientApi.createClient).mockResolvedValueOnce({
      data: { clientId: 'c1', oAuthClientId: 'oauth-c1', clientSecret: null },
    } as never)
    setup()
    await fillValidForm(user)
    await user.click(screen.getByRole('button', { name: /register client/i }))
    expect(await screen.findByText('Client registered')).toBeVisible()
    expect(await screen.findByText('oauth-c1')).toBeVisible()
    expect(screen.queryByText(/save your client secret/i)).not.toBeInTheDocument()
  })

  it('shows the client secret in the SecretModal for a Confidential client', async () => {
    const user = userEvent.setup()
    vi.mocked(clientApi.createClient).mockResolvedValueOnce({
      data: { clientId: 'c1', oAuthClientId: 'oauth-c1', clientSecret: 'super-secret-abc' },
    } as never)
    setup()
    await user.click(screen.getByRole('radio', { name: /confidential/i }))
    await fillValidForm(user)
    await user.click(screen.getByRole('button', { name: /register client/i }))
    expect(await screen.findByText(/save your client secret now/i)).toBeVisible()
    expect(await screen.findByText('super-secret-abc')).toBeVisible()
  })

  it('shows a root error message when the API call fails', async () => {
    const user = userEvent.setup()
    const error = Object.assign(new Error('Server error'), {
      isAxiosError: true,
      response: { status: 500, data: { message: 'Internal server error' } },
    })
    vi.mocked(clientApi.createClient).mockRejectedValueOnce(error)
    setup()
    await fillValidForm(user)
    await user.click(screen.getByRole('button', { name: /register client/i }))
    expect(await screen.findByText('Internal server error')).toBeVisible()
  })

  it('Cancel navigates to /clients', async () => {
    const user = userEvent.setup()
    setup()
    await user.click(screen.getByRole('button', { name: /cancel/i }))
    expect(screen.getByText('Clients page')).toBeInTheDocument()
  })
})

describe('ClientCreatePage — scope toggling', () => {
  beforeEach(() => { vi.clearAllMocks() })

  it('toggles a scope on when checked', async () => {
    const user = userEvent.setup()
    setup()
    const emailCheckbox = screen.getByRole('checkbox', { name: /^email$/i })
    expect(emailCheckbox).not.toBeChecked()
    await user.click(emailCheckbox)
    expect(emailCheckbox).toBeChecked()
  })

  it('toggles openid off when unchecked', async () => {
    const user = userEvent.setup()
    setup()
    const openidCheckbox = screen.getByRole('checkbox', { name: /openid/i })
    expect(openidCheckbox).toBeChecked()
    await user.click(openidCheckbox)
    expect(openidCheckbox).not.toBeChecked()
  })
})

describe('ClientCreatePage — mutation pending state', () => {
  beforeEach(() => { vi.clearAllMocks() })

  it('shows "Registering…" while the create mutation is in flight', async () => {
    const user = userEvent.setup()
    vi.mocked(clientApi.createClient).mockReturnValue(new Promise(() => {}) as never)
    setup()
    await fillValidForm(user)
    await user.click(screen.getByRole('button', { name: /register client/i }))
    expect(await screen.findByRole('button', { name: /registering/i })).toBeInTheDocument()
  })
})

describe('ClientCreatePage — API error edge cases', () => {
  beforeEach(() => { vi.clearAllMocks() })

  it('shows "Something went wrong." when the API throws a non-Axios error', async () => {
    const user = userEvent.setup()
    vi.mocked(clientApi.createClient).mockRejectedValueOnce(new Error('Network failure'))
    setup()
    await fillValidForm(user)
    await user.click(screen.getByRole('button', { name: /register client/i }))
    expect(await screen.findByText('Something went wrong.')).toBeVisible()
  })

  it('shows "Failed to register client." when Axios error has no message body', async () => {
    const user = userEvent.setup()
    const error = Object.assign(new Error('Server Error'), {
      isAxiosError: true,
      response: { status: 500, data: {} },
    })
    vi.mocked(clientApi.createClient).mockRejectedValueOnce(error)
    setup()
    await fillValidForm(user)
    await user.click(screen.getByRole('button', { name: /register client/i }))
    expect(await screen.findByText('Failed to register client.')).toBeVisible()
  })
})

describe('ClientCreatePage — SecretModal interactions', () => {
  beforeEach(() => {
    vi.clearAllMocks()
    vi.spyOn(navigator.clipboard, 'writeText').mockResolvedValue(undefined)
  })
  afterEach(() => { vi.restoreAllMocks() })

  it('clicking Copy on the client ID calls clipboard.writeText and shows "Copied!"', async () => {
    const user = userEvent.setup()
    vi.mocked(clientApi.createClient).mockResolvedValueOnce({
      data: { clientId: 'c1', oAuthClientId: 'oauth-c1', clientSecret: null },
    } as never)
    setup()
    await fillValidForm(user)
    await user.click(screen.getByRole('button', { name: /register client/i }))
    await screen.findByText('Client registered')

    await user.click(screen.getByRole('button', { name: /^copy$/i }))

    expect(navigator.clipboard.writeText).toHaveBeenCalledWith('oauth-c1')
    expect(screen.getByRole('button', { name: /copied!/i })).toBeInTheDocument()
  })

  it('clicking Copy on the client secret calls clipboard.writeText and shows "Copied!"', async () => {
    const user = userEvent.setup()
    vi.mocked(clientApi.createClient).mockResolvedValueOnce({
      data: { clientId: 'c1', oAuthClientId: 'oauth-c1', clientSecret: 'super-secret-abc' },
    } as never)
    setup()
    await user.click(screen.getByRole('radio', { name: /confidential/i }))
    await fillValidForm(user)
    await user.click(screen.getByRole('button', { name: /register client/i }))
    await screen.findByText('Client registered')

    // Two Copy buttons: [0] = client ID, [1] = client secret
    const copyButtons = screen.getAllByRole('button', { name: /^copy$/i })
    await user.click(copyButtons[1])

    expect(navigator.clipboard.writeText).toHaveBeenCalledWith('super-secret-abc')
    expect(screen.getByRole('button', { name: /copied!/i })).toBeInTheDocument()
  })

  it('clicking Done navigates to the client detail page', async () => {
    const user = userEvent.setup()
    vi.mocked(clientApi.createClient).mockResolvedValueOnce({
      data: { clientId: 'c1', oAuthClientId: 'oauth-c1', clientSecret: null },
    } as never)
    setup()
    await fillValidForm(user)
    await user.click(screen.getByRole('button', { name: /register client/i }))
    await screen.findByText('Client registered')

    await user.click(screen.getByRole('button', { name: /^done$/i }))

    expect(screen.getByText('Client detail page')).toBeInTheDocument()
  })
})
