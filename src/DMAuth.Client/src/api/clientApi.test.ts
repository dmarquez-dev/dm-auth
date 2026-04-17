import { describe, it, expect, vi, beforeEach } from 'vitest'
import { clientApi } from './clientApi'
import { apiClient } from './client'

vi.mock('./client', () => ({
  apiClient: {
    get: vi.fn(),
    post: vi.fn(),
    put: vi.fn(),
    delete: vi.fn(),
  },
}))

describe('clientApi', () => {
  beforeEach(() => { vi.clearAllMocks() })

  it('listClients() GETs /api/clients', async () => {
    vi.mocked(apiClient.get).mockResolvedValueOnce({ data: [] })
    await clientApi.listClients()
    expect(apiClient.get).toHaveBeenCalledWith('/api/clients')
  })

  it('getClient(id) GETs /api/clients/:id', async () => {
    vi.mocked(apiClient.get).mockResolvedValueOnce({ data: {} })
    await clientApi.getClient('abc-123')
    expect(apiClient.get).toHaveBeenCalledWith('/api/clients/abc-123')
  })

  it('createClient() POSTs to /api/clients with the request payload', async () => {
    vi.mocked(apiClient.post).mockResolvedValueOnce({ data: {} })
    const payload = { clientName: 'My App', clientType: 'Public' as const, redirectUris: ['http://localhost/cb'], allowedScopes: ['openid'] }
    await clientApi.createClient(payload)
    expect(apiClient.post).toHaveBeenCalledWith('/api/clients', payload)
  })

  it('updateClient(id) PUTs to /api/clients/:id with the update payload', async () => {
    vi.mocked(apiClient.put).mockResolvedValueOnce({ data: undefined })
    const payload = { clientName: 'Updated', redirectUris: ['http://localhost/cb'], allowedScopes: ['openid'] }
    await clientApi.updateClient('abc-123', payload)
    expect(apiClient.put).toHaveBeenCalledWith('/api/clients/abc-123', payload)
  })

  it('deleteClient(id) DELETEs /api/clients/:id', async () => {
    vi.mocked(apiClient.delete).mockResolvedValueOnce({ data: undefined })
    await clientApi.deleteClient('abc-123')
    expect(apiClient.delete).toHaveBeenCalledWith('/api/clients/abc-123')
  })
})
