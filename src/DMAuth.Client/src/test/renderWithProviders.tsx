import { render } from '@testing-library/react'
import type { RenderOptions } from '@testing-library/react'
import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { MemoryRouter } from 'react-router-dom'
import type { ReactNode } from 'react'
import { AuthProvider } from '../auth/AuthProvider'
import type { User } from '../types/auth'

interface RenderWithProvidersOptions extends RenderOptions {
  /** Initial URL entries for MemoryRouter. Defaults to ['/']. */
  initialEntries?: string[]
  /**
   * If provided, pre-seeds the React Query cache with this user under the
   * ['auth', 'me'] key so AuthProvider reports an authenticated state
   * without making a real network request.
   * Pass null explicitly to simulate the unauthenticated state.
   */
  user?: User | null
}

function buildQueryClient(user?: User | null): QueryClient {
  const queryClient = new QueryClient({
    defaultOptions: {
      queries: { retry: false, gcTime: 0 },
      mutations: { retry: false },
    },
  })

  if (user !== undefined) {
    queryClient.setQueryData(['auth', 'me'], user)
  }

  return queryClient
}

export function renderWithProviders(
  ui: ReactNode,
  { initialEntries = ['/'], user, ...options }: RenderWithProvidersOptions = {}
) {
  const queryClient = buildQueryClient(user)

  function Wrapper({ children }: { children: ReactNode }) {
    return (
      <MemoryRouter initialEntries={initialEntries}>
        <QueryClientProvider client={queryClient}>
          <AuthProvider>{children}</AuthProvider>
        </QueryClientProvider>
      </MemoryRouter>
    )
  }

  return render(ui, { wrapper: Wrapper, ...options })
}
