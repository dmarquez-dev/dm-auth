export interface User {
  userId: string
  username: string
  email: string
  displayName: string
  emailVerified: boolean
}

export interface LoginRequest {
  email: string
  password: string
}

export interface LoginResponse {
  userId: string
  username: string
  email: string
  displayName: string
}

export interface RegisterRequest {
  email: string
  username: string
  displayName: string
  password: string
}

export interface RegisterResponse {
  userId: string
}
