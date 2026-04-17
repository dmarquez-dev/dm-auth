# Playwright Guidelines — DMAuth.Client

> Canonical conventions for end-to-end testing of DMAuth.Client with Playwright.
> All new e2e test files must follow these guidelines.

---

## Table of Contents

1. [Stack and Configuration](#1-stack-and-configuration)
2. [File and Folder Conventions](#2-file-and-folder-conventions)
3. [Test Isolation](#3-test-isolation)
4. [Locator Strategy](#4-locator-strategy)
5. [Assertions](#5-assertions)
6. [Authentication Fixture](#6-authentication-fixture)
7. [Page Object Models](#7-page-object-models)
8. [Mocking the Network](#8-mocking-the-network)
9. [Debugging](#9-debugging)
10. [CI Integration](#10-ci-integration)
11. [What to Test (and What Not To)](#11-what-to-test-and-what-not-to)

---

## 1. Stack and Configuration

**Package:** `@playwright/test` (dev dependency). Install the default browser with `npx playwright install chromium --with-deps`.

**`playwright.config.ts`**:

```ts
import { defineConfig, devices } from '@playwright/test';

export default defineConfig({
  testDir: './tests/e2e',
  fullyParallel: true,
  forbidOnly: !!process.env.CI,
  retries: process.env.CI ? 2 : 0,
  reporter: process.env.CI ? [['html'], ['github']] : [['html']],
  use: {
    baseURL: process.env.PLAYWRIGHT_BASE_URL ?? 'http://localhost:5173',
    trace: 'on-first-retry',
  },
  projects: [
    { name: 'chromium', use: { ...devices['Desktop Chrome'] } },
  ],
  webServer: {
    command: 'npm run dev',
    url: 'http://localhost:5173',
    reuseExistingServer: !process.env.CI,
  },
});
```

**`package.json` scripts**:

```json
{
  "scripts": {
    "e2e": "playwright test",
    "e2e:ui": "playwright test --ui",
    "e2e:debug": "playwright test --debug"
  }
}
```

---

## 2. File and Folder Conventions

```
tests/
  e2e/
    fixtures/
      auth.ts          ← authenticated page fixture
    pages/             ← Page Object Models
      LoginPage.ts
      ConsentPage.ts
    auth.spec.ts
    clients.spec.ts
    consent.spec.ts
    profile.spec.ts
```

- One spec file per feature area — keep files focused.
- Page Object Models go in `tests/e2e/pages/`.
- Fixtures go in `tests/e2e/fixtures/`.
- No production source code in `tests/e2e/` — the folder is test-only.

---

## 3. Test Isolation

Each test must be fully independent. Never share state between tests via module-level variables or test ordering.

**Each test gets its own user** — create a fresh account (or use the `authenticatedPage` fixture which does this automatically) so tests never collide on shared database rows:

```ts
// Correct — unique email per test prevents conflicts
const email = `user-${Date.now()}@test.local`;
```

**Storage state per suite** — authenticate once per spec file using `storageState`, not on every test:

```ts
// auth.setup.ts
import { test as setup } from '@playwright/test';

setup('authenticate', async ({ page }) => {
  await page.goto('/register');
  // ... register and log in
  await page.context().storageState({ path: 'tests/e2e/.auth/user.json' });
});
```

Then reference it in `playwright.config.ts` for the authenticated project:

```ts
{
  name: 'authenticated',
  use: { storageState: 'tests/e2e/.auth/user.json' },
  dependencies: ['setup'],
}
```

---

## 4. Locator Strategy

**Priority order (highest to lowest):**

1. **Role** — `page.getByRole('button', { name: /sign in/i })` — most resilient to markup changes.
2. **Label** — `page.getByLabel('Email address')` — for form fields.
3. **Text** — `page.getByText('Dashboard')` — for headings and static content.
4. **Test ID** — `page.getByTestId('scope-list')` — last resort; add `data-testid` when no semantic locator fits.

**Never** use CSS selectors (`.btn-primary`) or XPath — they break on any styling or structural change.

### Chaining and filtering

Narrow to the right element when multiple similar elements exist:

```ts
// Click "Delete" only for the client named "My App"
await page
  .getByRole('listitem')
  .filter({ hasText: 'My App' })
  .getByRole('button', { name: /delete/i })
  .click();
```

### Generating locators

Use `npx playwright codegen http://localhost:5173` to auto-generate role/label-based locators. Always review and simplify the generated output — codegen is a starting point, not a final answer.

---

## 5. Assertions

Always use **web-first assertions** — they automatically retry until the condition is met or the timeout expires:

```ts
// Correct — retries until visible
await expect(page.getByText('Dashboard')).toBeVisible();

// Avoid — does not retry; will fail on slow renders
expect(await page.getByText('Dashboard').isVisible()).toBe(true);
```

Use `expect.soft()` when you want to collect multiple failures in one test run without stopping early:

```ts
await expect.soft(page.getByTestId('client-name')).toHaveText('My App');
await expect.soft(page.getByTestId('client-id')).not.toBeEmpty();
```

**Common assertions:**

```ts
await expect(page).toHaveURL('/dashboard');
await expect(page.getByRole('heading')).toHaveText('Welcome');
await expect(page.getByRole('button', { name: /delete/i })).toBeDisabled();
await expect(page.getByRole('alert')).toContainText('Invalid credentials');
```

---

## 6. Authentication Fixture

Extend Playwright's base `test` with an `authenticatedPage` fixture so tests that need a logged-in user get one without repeating login steps:

```ts
// tests/e2e/fixtures/auth.ts
import { test as base, expect } from '@playwright/test';

type AuthFixtures = {
  authenticatedPage: Page;
};

export const test = base.extend<AuthFixtures>({
  authenticatedPage: async ({ page }, use) => {
    const email = `user-${Date.now()}@test.local`;
    const password = 'Secure1!';

    await page.goto('/register');
    await page.getByLabel(/email/i).fill(email);
    await page.getByLabel(/username/i).fill(`user${Date.now()}`);
    await page.getByLabel(/^password$/i).fill(password);
    await page.getByRole('button', { name: /register/i }).click();

    await expect(page).toHaveURL('/dashboard');
    await use(page);
  },
});

export { expect } from '@playwright/test';
```

Usage in specs:

```ts
import { test, expect } from '../fixtures/auth';

test('creates a new client', async ({ authenticatedPage: page }) => {
  await page.goto('/clients/new');
  // ...
});
```

---

## 7. Page Object Models

Use Page Object Models (POMs) for pages that appear in multiple spec files. A POM encapsulates locators and actions so they are defined once and reused:

```ts
// tests/e2e/pages/LoginPage.ts
import type { Page } from '@playwright/test';

export class LoginPage {
  constructor(private readonly page: Page) {}

  async goto() {
    await this.page.goto('/login');
  }

  async login(email: string, password: string) {
    await this.page.getByLabel(/email/i).fill(email);
    await this.page.getByLabel(/password/i).fill(password);
    await this.page.getByRole('button', { name: /sign in/i }).click();
  }

  get errorMessage() {
    return this.page.getByRole('alert');
  }
}
```

Usage:

```ts
import { LoginPage } from '../pages/LoginPage';

test('shows an error on wrong password', async ({ page }) => {
  const loginPage = new LoginPage(page);
  await loginPage.goto();
  await loginPage.login('a@b.com', 'wrongpassword');
  await expect(loginPage.errorMessage).toContainText('Invalid credentials');
});
```

**Guidance:** only create a POM when a page's interactions are shared across multiple spec files. A page used in one spec does not need a POM.

---

## 8. Mocking the Network

Avoid calling the real backend from e2e tests when possible for third-party or non-owned services. For the DMAuth backend (which we own and control), prefer hitting the real dev server. Mock the network only when:

- Testing error states that are hard to reproduce (500, network timeout).
- The dependency is a third-party service outside our control.

```ts
// Mock a 500 on the clients endpoint
await page.route('**/api/clients', route =>
  route.fulfill({ status: 500, body: JSON.stringify({ error: 'Internal Server Error' }) })
);
```

---

## 9. Debugging

**Local debugging:**

```bash
# Step through a test interactively
npm run e2e:debug

# Run a single test file
npx playwright test auth.spec.ts

# Run a single test by line number
npx playwright test auth.spec.ts:12
```

**UI mode** (`npm run e2e:ui`) provides a visual test runner with time-travel through each test step — use this for authoring and diagnosing failures locally.

**Traces** are captured on the first retry (`trace: 'on-first-retry'` in config). After a CI failure, download the HTML report and open the trace viewer:

```bash
npx playwright show-report
```

The trace viewer shows a full timeline of network requests, DOM snapshots, and console output — use it instead of adding `console.log` statements.

**Screenshots and videos** are generally not needed when traces are enabled. Avoid enabling them by default as they slow down the test run.

---

## 10. CI Integration

Tests run on Linux in CI (cost-effective, consistent). The GitHub Actions workflow should:

1. Install only the needed browser: `npx playwright install chromium --with-deps`.
2. Run tests in parallel (default).
3. Upload the Playwright HTML report as a build artifact on failure.

```yaml
- name: Install Playwright browsers
  run: npx playwright install chromium --with-deps

- name: Run e2e tests
  run: npm run e2e

- name: Upload Playwright report
  if: failure()
  uses: actions/upload-artifact@v4
  with:
    name: playwright-report
    path: playwright-report/
```

**Do not commit `.auth/` files** — add `tests/e2e/.auth/` to `.gitignore`.

---

## 11. What to Test (and What Not To)

### Test

- Full user journeys across multiple pages (register → dashboard, create client → see in list).
- Auth boundaries: unauthenticated access is blocked; authenticated access succeeds.
- OAuth consent flow end-to-end.
- Error recovery: wrong password shows error; user can retry.
- Form validation feedback in the browser (not just in unit tests).

### Do not test

- Third-party services (external OAuth providers, payment gateways) — mock them.
- Implementation details: API request payloads belong in Vitest unit tests.
- Every permutation of form validation — cover the happy path and one error path in e2e; exhaustive validation belongs in Vitest.
- Static content that doesn't change (exact copyright text, footer links).
