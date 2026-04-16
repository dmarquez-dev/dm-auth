import axios from 'axios'

export const apiClient = axios.create({
  baseURL: '/',
  withCredentials: true,
  headers: {
    'Content-Type': 'application/json',
  },
})

apiClient.interceptors.response.use(
  response => response,
  error => {
    const isAuthCheck = error.config?.url === '/api/users/me'
    const alreadyOnLogin = window.location.pathname === '/login'
    if (error.response?.status === 401 && !isAuthCheck && !alreadyOnLogin) {
      const returnUrl = encodeURIComponent(window.location.pathname + window.location.search)
      window.location.href = `/login?returnUrl=${returnUrl}`
    }
    return Promise.reject(error)
  }
)
