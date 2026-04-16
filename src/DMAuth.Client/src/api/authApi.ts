import type { LoginRequest, LoginResponse, RegisterRequest, RegisterResponse, User } from '../types/auth'
import { apiClient } from './client'

export const authApi = {
  login: (request: LoginRequest) =>
    apiClient.post<LoginResponse>('/api/users/login', request),

  register: (request: RegisterRequest) =>
    apiClient.post<RegisterResponse>('/api/users/register', request),

  logout: () =>
    apiClient.post<void>('/api/users/logout'),

  me: () =>
    apiClient.get<User>('/api/users/me'),
}
