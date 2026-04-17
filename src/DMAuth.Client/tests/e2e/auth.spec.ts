import { test, expect } from '@playwright/test'
import { randomBytes } from 'node:crypto'

// Run all auth-flow tests unauthenticated — clear the shared storageState.
test.use({ storageState: { cookies: [], origins: [] } })

function uid() {
  return randomBytes(4).toString('hex')
}

test.describe('Registration', () => {
  test('registers a new account and redirects to login with success banner', async ({ page, request }) => {
    const id = uid()
    const email = `e2e-reg-${id}@dmauth.test`

    await page.goto('/register')
    await page.getByLabel('Email').fill(email)
    await page.getByLabel('Username').fill(`e2ereg${id}`)
    await page.getByLabel('Display name').fill('Test Registrant')
    await page.getByLabel('Password', { exact: true }).fill('RegPass1!')
    await page.getByLabel('Confirm password').fill('RegPass1!')
    await page.getByRole('button', { name: /create account/i }).click()

    await expect(page).toHaveURL(/registered=1/)
    await expect(page.getByText(/account created/i)).toBeVisible()
  })

  test('shows a validation error when passwords do not match', async ({ page }) => {
    await page.goto('/register')
    await page.getByLabel('Email').fill('mismatch@dmauth.test')
    await page.getByLabel('Username').fill('mismatchuser')
    await page.getByLabel('Display name').fill('Mismatch')
    await page.getByLabel('Password', { exact: true }).fill('Pass1234!')
    await page.getByLabel('Confirm password').fill('Different1!')
    await page.getByRole('button', { name: /create account/i }).click()

    await expect(page.getByText('Passwords do not match')).toBeVisible()
  })
})

test.describe('Login', () => {
  test('signs in with valid credentials and reaches dashboard', async ({ page, request }) => {
    const id = uid()
    const email = `e2e-login-${id}@dmauth.test`

    await request.post('/api/users/register', {
      data: { email, username: `e2elogin${id}`, displayName: 'Login Test', password: 'LoginPass1!' },
    })

    await page.goto('/login')
    await page.getByLabel(/email/i).fill(email)
    await page.getByLabel(/password/i).fill('LoginPass1!')
    await page.getByRole('button', { name: /sign in/i }).click()

    await expect(page).toHaveURL('/dashboard')
  })

  test('shows "Incorrect email or password" on wrong credentials', async ({ page, request }) => {
    const id = uid()
    const email = `e2e-badpw-${id}@dmauth.test`

    await request.post('/api/users/register', {
      data: { email, username: `e2ebadpw${id}`, displayName: 'Bad PW', password: 'GoodPass1!' },
    })

    await page.goto('/login')
    await page.getByLabel(/email/i).fill(email)
    await page.getByLabel(/password/i).fill('WrongPass1!')
    await page.getByRole('button', { name: /sign in/i }).click()

    await expect(page.getByText('Incorrect email or password.')).toBeVisible()
  })

  test('redirects unauthenticated users from protected routes to /login', async ({ page }) => {
    await page.goto('/dashboard')
    await expect(page).toHaveURL(/\/login/)
  })
})

test.describe('Logout', () => {
  test('signs out and redirects to /login', async ({ page, request }) => {
    const id = uid()
    const email = `e2e-logout-${id}@dmauth.test`

    await request.post('/api/users/register', {
      data: { email, username: `e2elogout${id}`, displayName: 'Logout Test', password: 'LogoutPass1!' },
    })

    await page.goto('/login')
    await page.getByLabel(/email/i).fill(email)
    await page.getByLabel(/password/i).fill('LogoutPass1!')
    await page.getByRole('button', { name: /sign in/i }).click()
    await expect(page).toHaveURL('/dashboard')

    await page.getByRole('button', { name: /sign out/i }).click()
    await expect(page).toHaveURL('/login')
  })
})
