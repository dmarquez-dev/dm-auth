import axios from 'axios'

// The one place the API base URL is read from the environment. Unset means same origin
// (the Vite dev proxy). Read on every call so tests can stub it per case.
const getApiBaseUrl = (): string => import.meta.env.VITE_API_BASE_URL || '/'

/**
 * Builds a URL for an API path the browser navigates to directly (a native form post),
 * joined the same way axios joins `baseURL` and a request path.
 */
export function apiUrl(path: string): string {
  return `${getApiBaseUrl().replace(/\/+$/, '')}/${path.replace(/^\/+/, '')}`
}

/**
 * True when `url` is an absolute URL on the API's origin (the base URL resolved against
 * the SPA's origin) whose path is exactly `/connect/authorize`.
 */
export function isApiAuthorizeUrl(url: string): boolean {
  let target: URL
  try {
    target = new URL(url) // no base: relative and protocol-relative URLs throw
  } catch {
    return false
  }
  const apiOrigin = new URL(getApiBaseUrl(), window.location.origin).origin
  return target.origin === apiOrigin && target.pathname === '/connect/authorize'
}

export const apiClient = axios.create({
  baseURL: getApiBaseUrl(),
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
