import { describe, it, expect, vi, afterEach } from 'vitest'
import { apiClient, apiUrl, isApiAuthorizeUrl } from './client'

// Reach into the interceptor manager to get the registered error handler.
// Axios stores handlers in an array; our interceptor is the first (index 0).
type InterceptorHandlers = Array<{
  fulfilled: ((r: unknown) => unknown) | null
  rejected: ((e: unknown) => unknown) | null
}>

const getHandlers = () =>
  (apiClient.interceptors.response as unknown as { handlers: InterceptorHandlers }).handlers

const getErrorHandler = () => getHandlers()[0].rejected!

const stubLocation = (pathname: string, search = '') => {
  let href = ''
  vi.stubGlobal('location', {
    pathname,
    search,
    get href() { return href },
    set href(v: string) { href = v },
  })
  return () => href
}

describe('apiClient — response interceptor', () => {
  afterEach(() => { vi.unstubAllGlobals() })

  it('passes successful responses through unchanged', () => {
    const response = { data: { ok: true }, status: 200 }
    expect(getHandlers()[0].fulfilled!(response)).toBe(response)
  })

  it('redirects to /login with returnUrl on a 401 from a standard API endpoint', async () => {
    const getHref = stubLocation('/dashboard', '')
    const error = { config: { url: '/api/clients' }, response: { status: 401 } }

    await expect(getErrorHandler()(error)).rejects.toBe(error)
    expect(getHref()).toBe('/login?returnUrl=%2Fdashboard')
  })

  it('includes the query string in the returnUrl', async () => {
    const getHref = stubLocation('/clients', '?page=2')
    const error = { config: { url: '/api/clients' }, response: { status: 401 } }

    await expect(getErrorHandler()(error)).rejects.toBe(error)
    expect(getHref()).toBe('/login?returnUrl=%2Fclients%3Fpage%3D2')
  })

  it('does NOT redirect when the 401 is from /api/users/me (auth check)', async () => {
    const getHref = stubLocation('/dashboard')
    const error = { config: { url: '/api/users/me' }, response: { status: 401 } }

    await expect(getErrorHandler()(error)).rejects.toBe(error)
    expect(getHref()).toBe('')
  })

  it('does NOT redirect when the 401 is from /api/users/me/change-password (wrong password)', async () => {
    const getHref = stubLocation('/profile')
    const error = { config: { url: '/api/users/me/change-password' }, response: { status: 401 } }

    await expect(getErrorHandler()(error)).rejects.toBe(error)
    expect(getHref()).toBe('')
  })

  it('does NOT redirect when already on /login', async () => {
    const getHref = stubLocation('/login')
    const error = { config: { url: '/api/clients' }, response: { status: 401 } }

    await expect(getErrorHandler()(error)).rejects.toBe(error)
    expect(getHref()).toBe('')
  })

  it('does NOT redirect for non-401 errors', async () => {
    const getHref = stubLocation('/dashboard')
    const error = { config: { url: '/api/clients' }, response: { status: 500 } }

    await expect(getErrorHandler()(error)).rejects.toBe(error)
    expect(getHref()).toBe('')
  })
})

describe('apiUrl', () => {
  afterEach(() => { vi.unstubAllEnvs() })

  it.each([
    [undefined, '/connect/consent'],
    ['https://api.example.test/', 'https://api.example.test/connect/consent'],
    ['https://api.example.test', 'https://api.example.test/connect/consent'],
  ])('joins /connect/consent onto VITE_API_BASE_URL %s as %s', (apiBaseUrl, expected) => {
    vi.stubEnv('VITE_API_BASE_URL', apiBaseUrl)
    expect(apiUrl('/connect/consent')).toBe(expected)
  })
})

describe('isApiAuthorizeUrl', () => {
  afterEach(() => { vi.unstubAllEnvs() })

  it.each([
    ['https://api.example.test/connect/authorize?client_id=x', true],
    ['https://api.example.test/connect/authorize/extra', false],
    ['https://api.example.test/api/users/me', false],
    ['https://evil.example/connect/authorize', false],
    ['//api.example.test/connect/authorize', false],
    ['/connect/authorize', false],
    ['not a url', false],
  ])('classifies %s as %s when the API is https://api.example.test/', (url, expected) => {
    vi.stubEnv('VITE_API_BASE_URL', 'https://api.example.test/')
    expect(isApiAuthorizeUrl(url)).toBe(expected)
  })

  it('accepts an absolute /connect/authorize URL on the SPA origin when VITE_API_BASE_URL is unset', () => {
    vi.stubEnv('VITE_API_BASE_URL', undefined)
    expect(isApiAuthorizeUrl(`${window.location.origin}/connect/authorize?client_id=x`)).toBe(true)
  })
})
