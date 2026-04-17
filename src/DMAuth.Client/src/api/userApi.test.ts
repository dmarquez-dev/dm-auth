import { describe, it, expect, vi, beforeEach } from 'vitest'
import { userApi } from './userApi'
import { apiClient } from './client'

vi.mock('./client', () => ({
  apiClient: {
    get: vi.fn(),
    post: vi.fn(),
    put: vi.fn(),
    delete: vi.fn(),
  },
}))

describe('userApi', () => {
  beforeEach(() => { vi.clearAllMocks() })

  it('getProfile() GETs /api/users/me', async () => {
    vi.mocked(apiClient.get).mockResolvedValueOnce({ data: {} })
    await userApi.getProfile()
    expect(apiClient.get).toHaveBeenCalledWith('/api/users/me')
  })

  it('updateProfile() PUTs to /api/users/me with the display name', async () => {
    vi.mocked(apiClient.put).mockResolvedValueOnce({ data: {} })
    await userApi.updateProfile({ displayName: 'Bob' })
    expect(apiClient.put).toHaveBeenCalledWith('/api/users/me', { displayName: 'Bob' })
  })

  it('changePassword() POSTs to /api/users/me/change-password', async () => {
    vi.mocked(apiClient.post).mockResolvedValueOnce({ data: undefined })
    await userApi.changePassword({ currentPassword: 'Old1!', newPassword: 'New1!' })
    expect(apiClient.post).toHaveBeenCalledWith(
      '/api/users/me/change-password',
      { currentPassword: 'Old1!', newPassword: 'New1!' }
    )
  })
})
