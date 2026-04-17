import { describe, it, expect, vi, beforeEach } from 'vitest'
import { screen } from '@testing-library/react'
import { Route, Routes } from 'react-router-dom'
import { ProtectedRoute } from './ProtectedRoute'
import { renderWithProviders } from '../test/renderWithProviders'
import type { User } from '../types/auth'

// Never-resolving mock so the auth query stays in loading state when we need it
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

describe('ProtectedRoute', () => {
  beforeEach(() => { vi.clearAllMocks() })

  it('renders children when the user is authenticated', () => {
    renderWithProviders(
      <Routes>
        <Route path="/" element={<ProtectedRoute><p>Secret content</p></ProtectedRoute>} />
      </Routes>,
      { user: testUser }
    )
    expect(screen.getByText('Secret content')).toBeInTheDocument()
  })

  it('redirects to /login when the user is not authenticated', () => {
    renderWithProviders(
      <Routes>
        <Route path="/" element={<ProtectedRoute><p>Secret content</p></ProtectedRoute>} />
        <Route path="/login" element={<p>Login page</p>} />
      </Routes>,
      { user: null }
    )
    expect(screen.getByText('Login page')).toBeInTheDocument()
    expect(screen.queryByText('Secret content')).not.toBeInTheDocument()
  })

  it('shows a loading spinner while the auth state is being determined', () => {
    // No user seed — authApi.me is mocked to never resolve, so isLoading stays true
    renderWithProviders(
      <Routes>
        <Route path="/" element={<ProtectedRoute><p>Secret content</p></ProtectedRoute>} />
      </Routes>
    )
    expect(document.querySelector('.animate-spin')).toBeInTheDocument()
    expect(screen.queryByText('Secret content')).not.toBeInTheDocument()
  })
})
