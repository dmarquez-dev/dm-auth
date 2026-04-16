import type {
  OAuthClient,
  OAuthClientDetail,
  RegisterClientRequest,
  RegisterClientResponse,
  UpdateClientRequest,
} from '../types/client'
import { apiClient } from './client'

export const clientApi = {
  listClients: () =>
    apiClient.get<OAuthClient[]>('/api/clients'),

  getClient: (id: string) =>
    apiClient.get<OAuthClientDetail>(`/api/clients/${id}`),

  createClient: (request: RegisterClientRequest) =>
    apiClient.post<RegisterClientResponse>('/api/clients', request),

  updateClient: (id: string, request: UpdateClientRequest) =>
    apiClient.put<void>(`/api/clients/${id}`, request),

  deleteClient: (id: string) =>
    apiClient.delete<void>(`/api/clients/${id}`),
}
