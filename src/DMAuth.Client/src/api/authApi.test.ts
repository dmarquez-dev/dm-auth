import { describe, it, expect, vi, beforeEach } from 'vitest'
import { authApi } from './authApi'
import { apiClient } from './client'

vi.mock('./client', () => ({
  apiClient: {
    get: vi.fn(),
    post: vi.fn(),
    put: vi.fn(),
    delete: vi.fn(),
  },
}))

describe('authApi', () => {
  beforeEach(() => { vi.clearAllMocks() })

  it('login() POSTs to /api/users/login with the supplied credentials', async () => {
    vi.mocked(apiClient.post).mockResolvedValueOnce({ data: {} })
    await authApi.login({ email: 'a@test.com', password: 'Secret1!' })
    expect(apiClient.post).toHaveBeenCalledWith(
      '/api/users/login',
      { email: 'a@test.com', password: 'Secret1!' }
    )
  })

  it('register() POSTs to /api/users/register with the supplied data', async () => {
    vi.mocked(apiClient.post).mockResolvedValueOnce({ data: {} })
    const payload = { email: 'a@test.com', username: 'alice', displayName: 'Alice', password: 'Secret1!' }
    await authApi.register(payload)
    expect(apiClient.post).toHaveBeenCalledWith('/api/users/register', payload)
  })

  it('logout() POSTs to /api/users/logout', async () => {
    vi.mocked(apiClient.post).mockResolvedValueOnce({ data: undefined })
    await authApi.logout()
    expect(apiClient.post).toHaveBeenCalledWith('/api/users/logout')
  })

  it('me() GETs /api/users/me', async () => {
    vi.mocked(apiClient.get).mockResolvedValueOnce({ data: {} })
    await authApi.me()
    expect(apiClient.get).toHaveBeenCalledWith('/api/users/me')
  })
})
