export type ClientType = 'Public' | 'Confidential'

export interface OAuthClient {
  clientId: string
  oAuthClientId: string
  clientName: string
  clientType: ClientType
  isActive: boolean
  redirectUris: string[]
  allowedScopes: string[]
  createdAt: string
  updatedAt: string | null
}

export interface OAuthClientDetail extends OAuthClient {
  ownerId: string
}

export interface RegisterClientRequest {
  clientName: string
  clientType: ClientType
  redirectUris: string[]
  allowedScopes: string[]
}

export interface RegisterClientResponse {
  clientId: string
  oAuthClientId: string
  clientSecret: string | null
}

export interface UpdateClientRequest {
  clientName: string
  redirectUris: string[]
  allowedScopes: string[]
}
