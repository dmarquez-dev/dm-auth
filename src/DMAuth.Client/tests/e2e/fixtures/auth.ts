import { test as base, expect } from '@playwright/test'
import { randomBytes } from 'node:crypto'

type FreshUser = {
  email: string
  username: string
  displayName: string
  password: string
}

type AuthFixtures = {
  /** A newly-registered, logged-in user. Isolated per test — safe for mutations. */
  freshUser: FreshUser
}

/**
 * Extends Playwright's base `test` with a `freshUser` fixture.
 *
 * Use this instead of the shared storageState when a test modifies auth-related
 * data (display name, password) and must not affect other tests.
 *
 * The fixture registers a unique user via the API, logs them in via the UI, and
 * leaves `page` on `/dashboard` ready for assertions.
 */
export const test = base.extend<AuthFixtures>({
  freshUser: async ({ page, request }, use) => {
    const id = randomBytes(4).toString('hex')
    const user: FreshUser = {
      email: `e2e-fresh-${id}@dmauth.test`,
      username: `e2efresh${id}`,
      displayName: `E2E Fresh ${id}`,
      password: 'FreshPass1!',
    }

    await request.post('/api/users/register', { data: user })

    await page.goto('/login')
    await page.getByLabel(/email/i).fill(user.email)
    await page.getByLabel(/password/i).fill(user.password)
    await page.getByRole('button', { name: /sign in/i }).click()
    await expect(page).toHaveURL('/dashboard')
    // Wait for the sidebar to show the user's email — confirms /api/users/me resolved
    // and the React Query cache is populated before the test body navigates away.
    await expect(page.locator('nav').getByText(user.email)).toBeVisible()

    await use(user)
  },
})

export { expect } from '@playwright/test'
