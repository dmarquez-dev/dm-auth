import { test, expect } from './fixtures/auth'

// All tests use the Profile nav link rather than page.goto('/profile').
// page.goto() triggers a full browser navigation that discards the React app's
// in-memory state (including the React Query cache). Using the sidebar link keeps
// the app mounted so the ['auth', 'me'] result is already cached and ProtectedRoute
// never needs to recheck auth — eliminating the flakiness window.
const goToProfile = async (page: Parameters<typeof test>[1]['page']) => {
  await page.getByRole('link', { name: 'Profile', exact: true }).click()
  // Wait for the page heading to confirm the ProfilePage has rendered
  await expect(page.getByRole('heading', { name: /^profile$/i, level: 1 })).toBeVisible()
}

test.describe('Profile page', () => {
  test('shows account info (username, email, email-verified badge)', async ({ page, freshUser }) => {
    await goToProfile(page)

    // Scope to main to avoid matching the nav sidebar which also shows the email
    await expect(page.locator('main').getByText(freshUser.email)).toBeVisible()
    await expect(page.locator('main').getByText(freshUser.username)).toBeVisible()
    // Use anchored pattern to avoid matching the "Email verified" label text
    await expect(page.getByText(/^(Verified|Not verified)$/i)).toBeVisible()
  })

  test('updates the display name and shows success banner', async ({ page, freshUser }) => {
    await goToProfile(page)

    const displayNameInput = page.locator('input[autocomplete="name"]')
    await displayNameInput.fill('Updated Display Name')
    await page.getByRole('button', { name: /^save$/i }).click()

    await expect(page.getByText('Display name updated.')).toBeVisible()
  })

  test('changes the password successfully', async ({ page, freshUser }) => {
    await goToProfile(page)

    await page.getByLabel('Current password', { exact: true }).fill(freshUser.password)
    await page.getByLabel('New password', { exact: true }).fill('NewPass1!')
    await page.getByLabel('Confirm new password', { exact: true }).fill('NewPass1!')
    await page.getByRole('button', { name: /change password/i }).click()

    await expect(page.getByText('Password changed successfully.')).toBeVisible()
  })

  test('shows an error when the current password is wrong', async ({ page, freshUser }) => {
    await goToProfile(page)

    await page.getByLabel('Current password', { exact: true }).fill('WrongCurrent1!')
    await page.getByLabel('New password', { exact: true }).fill('NewPass1!')
    await page.getByLabel('Confirm new password', { exact: true }).fill('NewPass1!')
    await page.getByRole('button', { name: /change password/i }).click()

    await expect(page.getByText('Incorrect password.')).toBeVisible()
  })
})
