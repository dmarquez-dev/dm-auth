import { test as setup, expect } from '@playwright/test'

const AUTH_FILE = 'tests/e2e/.auth/user.json'

/**
 * Registers the shared e2e test user (idempotent — 409 is silently ignored)
 * and saves their authenticated browser session to AUTH_FILE so all
 * "chromium" project tests start pre-logged-in.
 */
setup('authenticate shared test user', async ({ page, request }) => {
  await request.post('/api/users/register', {
    data: {
      email: 'e2e-shared@dmauth.test',
      username: 'e2eshared',
      displayName: 'E2E Shared User',
      password: 'E2ePass1!',
    },
  })

  await page.goto('/login')
  await page.getByLabel(/email/i).fill('e2e-shared@dmauth.test')
  await page.getByLabel(/password/i).fill('E2ePass1!')
  await page.getByRole('button', { name: /sign in/i }).click()
  await expect(page).toHaveURL('/dashboard')

  await page.context().storageState({ path: AUTH_FILE })
})
