import { useQueryClient, useQuery } from '@tanstack/react-query'
import type { ReactNode } from 'react'
import { authApi } from '../api/authApi'
import { AuthContext } from './AuthContext'

interface AuthProviderProps {
  children: ReactNode
}

export function AuthProvider({ children }: AuthProviderProps) {
  const queryClient = useQueryClient()

  const { data: user = null, isLoading } = useQuery({
    queryKey: ['auth', 'me'],
    queryFn: () => authApi.me().then(response => response.data),
    retry: false,
    staleTime: 5 * 60 * 1000,
  })

  const logout = async () => {
    await authApi.logout()
    queryClient.clear()
    window.location.href = '/login'
  }

  return (
    <AuthContext value={{
      user,
      isAuthenticated: user !== null,
      isLoading,
      logout,
    }}>
      {children}
    </AuthContext>
  )
}
