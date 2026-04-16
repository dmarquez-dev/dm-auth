import type { User } from '../types/auth'
import { apiClient } from './client'

export interface UpdateProfileRequest {
  displayName: string
}

export interface ChangePasswordRequest {
  currentPassword: string
  newPassword: string
}

export const userApi = {
  getProfile: () =>
    apiClient.get<User>('/api/users/me'),

  updateProfile: (request: UpdateProfileRequest) =>
    apiClient.put<User>('/api/users/me', request),

  changePassword: (request: ChangePasswordRequest) =>
    apiClient.post<void>('/api/users/me/change-password', request),
}
