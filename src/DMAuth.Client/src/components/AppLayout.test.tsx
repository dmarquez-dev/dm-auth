import { describe, it, expect, vi, beforeEach } from 'vitest'
import { screen } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { Route, Routes } from 'react-router-dom'
import { Outlet } from 'react-router-dom'
import { AppLayout } from './AppLayout'
import { renderWithProviders } from '../test/renderWithProviders'
import { authApi } from '../api/authApi'
import type { User } from '../types/auth'

vi.mock('../api/authApi', () => ({
  authApi: {
    me: vi.fn(() => new Promise(() => {})),
    login: vi.fn(),
    logout: vi.fn(() => Promise.resolve({ data: undefined })),
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
      <Route element={<AppLayout />}>
        <Route path="/dashboard" element={<Outlet />} />
      </Route>
    </Routes>,
    { initialEntries: ['/dashboard'], user: testUser }
  )

describe('AppLayout', () => {
  beforeEach(() => { vi.clearAllMocks() })

  it('renders the DM Auth brand name', () => {
    setup()
    expect(screen.getByText('DM Auth')).toBeInTheDocument()
  })

  it('renders nav links for Dashboard, Clients, and Profile', () => {
    setup()
    expect(screen.getByRole('link', { name: /dashboard/i })).toBeInTheDocument()
    expect(screen.getByRole('link', { name: /clients/i })).toBeInTheDocument()
    expect(screen.getByRole('link', { name: /profile/i })).toBeInTheDocument()
  })

  it('shows the authenticated user email in the sidebar', () => {
    setup()
    expect(screen.getByText(testUser.email)).toBeInTheDocument()
  })

  it('calls authApi.logout when the Sign out button is clicked', async () => {
    const user = userEvent.setup()
    vi.mocked(authApi.logout).mockResolvedValueOnce({ data: undefined } as never)
    setup()
    await user.click(screen.getByRole('button', { name: /sign out/i }))
    expect(authApi.logout).toHaveBeenCalledOnce()
  })
})
