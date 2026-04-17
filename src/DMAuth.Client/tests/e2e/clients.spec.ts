import { test, expect } from '@playwright/test'

test.describe('Clients list', () => {
  test('shows the clients page with a "New client" link', async ({ page }) => {
    await page.goto('/clients')
    await expect(page.getByRole('link', { name: /new client/i })).toBeVisible()
  })
})

test.describe('Client CRUD', () => {
  test('creates a new public client and shows the secret modal', async ({ page }) => {
    await page.goto('/clients/new')

    await page.getByLabel('Name').fill('E2E Test App')
    // Public is selected by default; leave it as-is

    await page.getByPlaceholder('https://example.com/callback').fill('http://localhost:9999/cb')
    // openid scope is pre-selected by default

    await page.getByRole('button', { name: /register client/i }).click()

    // Secret modal appears for confidential; for public it shows "Your client has been registered."
    await expect(page.getByText(/client registered/i)).toBeVisible()

    // Close modal → navigates to the new client detail page
    await page.getByRole('button', { name: /done/i }).click()
    await expect(page).toHaveURL(/\/clients\//)
  })

  test('updates a client name and shows the success banner', async ({ page }) => {
    // Create a client via API then navigate to its detail page
    const createRes = await page.request.post('/api/clients', {
      data: {
        clientName: 'E2E Update Target',
        clientType: 'Public',
        redirectUris: ['http://localhost:9999/cb'],
        allowedScopes: ['openid'],
      },
    })
    const { clientId } = await createRes.json() as { clientId: string }

    await page.goto(`/clients/${clientId}`)
    await expect(page.getByText('E2E Update Target')).toBeVisible()

    const nameInput = page.getByLabel('Name')
    await nameInput.fill('E2E Updated Name')
    await page.getByRole('button', { name: /save changes/i }).click()

    await expect(page.getByText('Client updated.')).toBeVisible()
  })

  test('deletes a client and navigates back to the clients list', async ({ page }) => {
    const createRes = await page.request.post('/api/clients', {
      data: {
        clientName: 'E2E Delete Target',
        clientType: 'Public',
        redirectUris: ['http://localhost:9999/cb'],
        allowedScopes: ['openid'],
      },
    })
    const { clientId } = await createRes.json() as { clientId: string }

    await page.goto(`/clients/${clientId}`)
    await page.getByRole('button', { name: /delete client/i }).click()

    // Confirm in the modal
    await page.getByRole('button', { name: /^delete$/i }).click()

    await expect(page).toHaveURL('/clients')
  })
})
