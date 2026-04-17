import { test, expect } from '@playwright/test'
import { createHash, randomBytes } from 'node:crypto'

// PKCE helpers
function base64URLEncode(buf: Buffer): string {
  return buf.toString('base64').replace(/\+/g, '-').replace(/\//g, '_').replace(/=/g, '')
}

function pkce() {
  const verifier = base64URLEncode(randomBytes(32))
  const challenge = base64URLEncode(createHash('sha256').update(verifier).digest())
  return { verifier, challenge }
}

const REDIRECT_URI = 'http://localhost:5173'

test.describe('Consent page', () => {
  let oAuthClientId: string
  let clientId: string

  test.beforeAll(async ({ request }) => {
    const res = await request.post('/api/clients', {
      data: {
        clientName: 'E2E Consent Client',
        clientType: 'Public',
        redirectUris: [REDIRECT_URI],
        allowedScopes: ['openid', 'profile'],
      },
    })
    const body = await res.json() as { clientId: string; oAuthClientId: string }
    clientId = body.clientId
    oAuthClientId = body.oAuthClientId
  })

  test.afterAll(async ({ request }) => {
    await request.delete(`/api/clients/${clientId}`)
  })

  function consentURL(challenge: string) {
    const params = new URLSearchParams({
      client_id: oAuthClientId,
      redirect_uri: REDIRECT_URI,
      scope: 'openid profile',
      state: 'e2estate',
      code_challenge: challenge,
      code_challenge_method: 'S256',
    })
    return `/consent?${params.toString()}`
  }

  test('renders the client ID and requested scopes', async ({ page }) => {
    const { challenge } = pkce()
    await page.goto(consentURL(challenge))

    await expect(page.getByText(oAuthClientId)).toBeVisible()
    await expect(page.getByText('OpenID')).toBeVisible()
    await expect(page.getByText('Profile')).toBeVisible()
    await expect(page.getByRole('button', { name: /allow access/i })).toBeVisible()
    await expect(page.getByRole('button', { name: /deny/i })).toBeVisible()
  })

  test('deny redirects to redirect_uri with error=access_denied', async ({ page }) => {
    const { challenge } = pkce()
    await page.goto(consentURL(challenge))

    await page.getByRole('button', { name: /deny/i }).click()

    await expect(page).toHaveURL(/error=access_denied/)
  })

  test('allow redirects to redirect_uri with an authorization code', async ({ page }) => {
    const { challenge } = pkce()
    await page.goto(consentURL(challenge))

    await page.getByRole('button', { name: /allow access/i }).click()

    await expect(page).toHaveURL(/[?&]code=/, { timeout: 10_000 })
  })
})
