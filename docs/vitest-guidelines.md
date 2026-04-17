# Vitest Guidelines — DMAuth.Client

> Canonical conventions for unit and component testing in DMAuth.Client (Vite + React + TypeScript).
> All new test files must follow these guidelines.

---

## Table of Contents

1. [Stack and Configuration](#1-stack-and-configuration)
2. [File and Folder Conventions](#2-file-and-folder-conventions)
3. [Test Structure](#3-test-structure)
4. [Rendering Components](#4-rendering-components)
5. [Querying the DOM](#5-querying-the-dom)
6. [User Interactions](#6-user-interactions)
7. [Mocking](#7-mocking)
8. [Async Testing](#8-async-testing)
9. [Parameterised Tests](#9-parameterised-tests)
10. [Snapshot Testing](#10-snapshot-testing)
11. [Coverage](#11-coverage)
12. [What to Test (and What Not To)](#12-what-to-test-and-what-not-to)

---

## 1. Stack and Configuration

| Package | Purpose |
|---|---|
| `vitest` | Test runner (native Vite integration, no Babel overhead) |
| `@vitest/coverage-v8` | Coverage via V8 (fast, no instrumentation) |
| `@testing-library/react` | Component rendering and DOM queries |
| `@testing-library/user-event` | Realistic user interaction simulation |
| `@testing-library/jest-dom` | Extended DOM matchers (`toBeVisible`, `toHaveValue`, etc.) |
| `jsdom` | Browser-like DOM environment |

**`vitest.config.ts`** — extend the existing `vite.config.ts` rather than duplicating it:

```ts
import { defineConfig, mergeConfig } from 'vitest/config';
import viteConfig from './vite.config';

export default mergeConfig(
  viteConfig,
  defineConfig({
    test: {
      environment: 'jsdom',
      globals: false,
      setupFiles: ['./src/test/setup.ts'],
      include: ['src/**/*.{test,spec}.{ts,tsx}'],
      coverage: {
        provider: 'v8',
        reporter: ['text', 'html', 'lcov'],
        include: ['src/**/*.{ts,tsx}'],
        exclude: ['src/main.tsx', 'src/test/**', 'src/types/**'],
      },
    },
  })
);
```

**`src/test/setup.ts`**:

```ts
import '@testing-library/jest-dom/vitest';
```

**`package.json` scripts**:

```json
{
  "scripts": {
    "test": "vitest run",
    "test:watch": "vitest",
    "test:coverage": "vitest run --coverage"
  }
}
```

---

## 2. File and Folder Conventions

- Place test files **alongside the source file** they test: `LoginPage.tsx` → `LoginPage.test.tsx`.
- Shared test utilities live in `src/test/` and are never imported by production code.
- Name test files `<Subject>.test.tsx` (components) or `<subject>.test.ts` (non-JSX).

```
src/
  pages/
    LoginPage.tsx
    LoginPage.test.tsx
  auth/
    useAuth.ts
    useAuth.test.ts
  test/
    setup.ts
    renderWithProviders.tsx   ← shared helper
    factories.ts              ← test data builders
```

---

## 3. Test Structure

Use `describe` to group by subject, and name each `it` block as a sentence that completes "it …":

```ts
describe('LoginPage', () => {
  it('renders email and password fields', () => { ... });
  it('shows a validation error when the form is submitted empty', async () => { ... });
  it('redirects to /dashboard after a successful login', async () => { ... });
});
```

**Avoid** grouping tests by lifecycle (`beforeEach` heavy setups are a smell — prefer explicit arrangement in each test).

---

## 4. Rendering Components

Always use the `renderWithProviders` helper so components get the full provider tree (Router, QueryClient, AuthProvider) without per-test boilerplate:

```tsx
// src/test/renderWithProviders.tsx
import { MemoryRouter } from 'react-router-dom';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { AuthProvider } from '../auth/AuthProvider';
import { render } from '@testing-library/react';
import type { RenderOptions } from '@testing-library/react';
import type { ReactNode } from 'react';

interface Options extends RenderOptions {
  initialEntries?: string[];
}

export function renderWithProviders(ui: ReactNode, { initialEntries = ['/'], ...options }: Options = {}) {
  const queryClient = new QueryClient({
    defaultOptions: { queries: { retry: false } },
  });

  return render(
    <MemoryRouter initialEntries={initialEntries}>
      <QueryClientProvider client={queryClient}>
        <AuthProvider>{ui}</AuthProvider>
      </QueryClientProvider>
    </MemoryRouter>,
    options
  );
}
```

Usage:

```tsx
import { renderWithProviders } from '../test/renderWithProviders';
import { LoginPage } from './LoginPage';

it('renders the login form', () => {
  renderWithProviders(<LoginPage />);
  expect(screen.getByRole('button', { name: /sign in/i })).toBeInTheDocument();
});
```

---

## 5. Querying the DOM

**Priority order (highest to lowest):**

1. **Role** — `getByRole('button', { name: /submit/i })` — mirrors how assistive technology sees the page.
2. **Label text** — `getByLabelText(/email/i)` — for form fields.
3. **Placeholder / display text** — `getByPlaceholderText`, `getByText`.
4. **Test ID** — `getByTestId('consent-scope-list')` — last resort; use `data-testid` when no semantic query fits.

**Never** query by CSS class, tag name, or DOM structure — these break on any refactor.

```tsx
// Correct
const submitButton = screen.getByRole('button', { name: /sign in/i });

// Avoid
const submitButton = document.querySelector('.btn-primary');
```

---

## 6. User Interactions

Use `@testing-library/user-event` v14+ (not `fireEvent`) for realistic event sequences:

```tsx
import userEvent from '@testing-library/user-event';

it('shows a validation error when submitting an empty form', async () => {
  const user = userEvent.setup();
  renderWithProviders(<LoginPage />);

  await user.click(screen.getByRole('button', { name: /sign in/i }));

  expect(screen.getByText(/email is required/i)).toBeVisible();
});
```

`userEvent.setup()` creates an instance that properly simulates focus, keyboard, and pointer events in sequence.

---

## 7. Mocking

### Module mocking with `vi.mock`

Mock at the module boundary, not inside components:

```ts
import { vi } from 'vitest';
import * as userApi from '../api/userApi';

vi.mock('../api/userApi');

it('calls loginUser with the submitted credentials', async () => {
  const user = userEvent.setup();
  vi.mocked(userApi.loginUser).mockResolvedValueOnce({ id: '1', email: 'a@b.com' });

  renderWithProviders(<LoginPage />);
  await user.type(screen.getByLabelText(/email/i), 'a@b.com');
  await user.type(screen.getByLabelText(/password/i), 'Secure1!');
  await user.click(screen.getByRole('button', { name: /sign in/i }));

  expect(userApi.loginUser).toHaveBeenCalledWith({ email: 'a@b.com', password: 'Secure1!' });
});
```

### Mocking hooks

Prefer providing real context values through `renderWithProviders` over mocking `useAuth`. Only mock when testing the hook itself:

```ts
// Testing the hook in isolation
import { renderHook, act } from '@testing-library/react';
import { useAuth } from './useAuth';

it('starts as unauthenticated', () => {
  const { result } = renderHook(() => useAuth(), { wrapper: AuthProvider });
  expect(result.current.isAuthenticated).toBe(false);
});
```

### Spy functions

Use `vi.fn()` with descriptive names when passing handlers as props:

```ts
const handleApprove = vi.fn();
renderWithProviders(<ConsentPage onApprove={handleApprove} />);
```

---

## 8. Async Testing

Always `await` interactions and assertions on async state:

```ts
it('shows a server error when login fails', async () => {
  const user = userEvent.setup();
  vi.mocked(userApi.loginUser).mockRejectedValueOnce(new Error('Invalid credentials'));

  renderWithProviders(<LoginPage />);
  await user.click(screen.getByRole('button', { name: /sign in/i }));

  expect(await screen.findByText(/invalid credentials/i)).toBeVisible();
});
```

Use `findBy*` queries (which return Promises) rather than `waitFor` + `getBy*` for elements that appear asynchronously — it is more concise and has the same retry behaviour.

For timers, use `vi.useFakeTimers()` / `vi.runAllTimers()` rather than real `setTimeout` delays.

---

## 9. Parameterised Tests

Use `it.each` (or `describe.each`) to reduce duplication across similar input scenarios:

```ts
it.each([
  ['', 'Email is required'],
  ['notanemail', 'Enter a valid email'],
])('shows "%s" validation error for email input "%s"', async (input, expectedError) => {
  const user = userEvent.setup();
  renderWithProviders(<RegisterPage />);

  if (input) await user.type(screen.getByLabelText(/email/i), input);
  await user.click(screen.getByRole('button', { name: /register/i }));

  expect(screen.getByText(expectedError)).toBeVisible();
});
```

---

## 10. Snapshot Testing

Snapshot tests are **not recommended** for page-level components — they are brittle and produce noisy diffs. Prefer explicit assertions on visible text and ARIA roles.

The only acceptable use of `toMatchSnapshot` is for pure data-transformation functions (e.g. serialising a type to a display string) where the output is stable and not tied to markup.

---

## 11. Coverage

Run coverage locally before pushing: `npm run test:coverage`.

**Targets (not enforced in CI yet, aim for these):**

| Metric | Target |
|---|---|
| Statements | ≥ 80% |
| Branches | ≥ 75% |
| Functions | ≥ 80% |

Coverage does **not** measure test quality — a test that renders a component and asserts nothing inflates coverage without value. Focus on meaningful assertions.

---

## 12. What to Test (and What Not To)

### Test

- User-visible behaviour: what text appears, what routes are navigated, what API calls are made.
- Error states: validation messages, server error banners, empty states.
- Access control: `ProtectedRoute` redirects when unauthenticated.
- Hook state transitions: initial → loading → success / error.

### Do not test

- Implementation details: internal state variable names, private methods, component re-render counts.
- Third-party library internals: React Router's redirect logic, React Query's retry behaviour.
- Styles or CSS classes — test visibility and layout semantics via ARIA, not class names.
- `main.tsx` bootstrap code.
