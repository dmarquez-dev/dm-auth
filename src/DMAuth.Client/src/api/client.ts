import axios from 'axios'

export const apiClient = axios.create({
  baseURL: import.meta.env.VITE_API_BASE_URL || '/',
  withCredentials: true,
  headers: {
    'Content-Type': 'application/json',
  },
})

apiClient.interceptors.response.use(
  response => response,
  error => {
    // Endpoints where 401 carries business meaning (not "session expired")
    const isSessionlessEndpoint =
      error.config?.url === '/api/users/me' ||
      error.config?.url === '/api/users/me/change-password'
    const alreadyOnLogin = window.location.pathname === '/login'
    if (error.response?.status === 401 && !isSessionlessEndpoint && !alreadyOnLogin) {
      const returnUrl = encodeURIComponent(window.location.pathname + window.location.search)
      window.location.href = `/login?returnUrl=${returnUrl}`
    }
    return Promise.reject(error)
  }
)
