import { describe, it, expect, vi, afterEach } from 'vitest'
import { apiClient } from './client'

// Reach into the interceptor manager to get the registered error handler.
// Axios stores handlers in an array; our interceptor is the first (index 0).
const getErrorHandler = () => {
  const handlers = (apiClient.interceptors.response as any).handlers as Array<{
    fulfilled: ((r: unknown) => unknown) | null
    rejected: ((e: unknown) => unknown) | null
  }>
  return handlers[0].rejected!
}

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
    const handlers = (apiClient.interceptors.response as any).handlers as Array<{
      fulfilled: ((r: unknown) => unknown) | null
    }>
    expect(handlers[0].fulfilled!(response)).toBe(response)
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
