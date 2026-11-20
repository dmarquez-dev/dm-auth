# Client tests (DMAuth.Client)

Every new unit, component, and e2e test file in `src/DMAuth.Client` follows these rules.

## Vitest: stack and configuration

- Unit and component tests use `vitest`, `@testing-library/react`, `@testing-library/user-event`, `@testing-library/jest-dom`, and `jsdom`; coverage uses `@vitest/coverage-v8`. Do not add another runner, DOM environment, or matcher library.
- `vitest.config.ts` extends `vite.config.ts` through `mergeConfig`; never duplicate Vite configuration in it.
- Keep these `test` settings: `environment: 'jsdom'`, `globals: false`, `setupFiles: ['./src/test/setup.ts']`, `include: ['src/**/*.{test,spec}.{ts,tsx}']`, and the `coverage` block `provider: 'v8'`, `reporter: ['text', 'html', 'lcov']`, `include: ['src/**/*.{ts,tsx}']`, `exclude: ['src/main.tsx', 'src/test/**', 'src/types/**']`.
- `src/test/setup.ts` imports `@testing-library/jest-dom/vitest`.
- `globals` is `false`: import `describe`, `it`, `expect`, `vi`, and hooks from `vitest` in every test file.
- The scripts are `test` (`vitest run`), `test:watch` (`vitest`), and `test:coverage` (`vitest run --coverage`).

## Vitest: files

- Place a test file beside the source file it tests: `LoginPage.tsx` → `LoginPage.test.tsx`.
- Shared test utilities (render helper, test data factories) live in `src/test/`. Production code never imports from `src/test/`.

## Vitest: structure

- Group tests with one `describe` per subject, named after the subject (`describe('LoginPage', ...)`).
- Name each `it` as a sentence completing "it …": `it('redirects to /dashboard after a successful login', ...)`.
- Arrange each test explicitly inside its own `it`. Do not build heavy shared setup in `beforeEach`; `beforeEach` holds at most the default resolved values of module mocks shared by every test in the block.
- Use `it.each` or `describe.each` for scenarios that differ only by input and expected output, instead of copying the test.

## Vitest: rendering

- Render every component through `renderWithProviders` from `src/test/renderWithProviders.tsx`. Never call Testing Library's `render` directly in a test.
- Set the starting route with the `initialEntries` option, not by wrapping the component in another router.
- Pass the `user` option to render as an authenticated user without an auth loading state; pass `user: null` to render as unauthenticated.
- Provide real context values through `renderWithProviders` instead of mocking `useAuth`. Mock or isolate a hook only in that hook's own test, using `renderHook` with the provider as `wrapper`.

## Vitest: querying the DOM

- Query in this priority order: `getByRole` (with `name`), then `getByLabelText` for form fields, then `getByPlaceholderText` / `getByText`, then `getByTestId` as the last resort.
- Add a `data-testid` only when no role, label, or text query fits.
- Never query by CSS class, tag name, or DOM structure; never use `document.querySelector` or `container.querySelector`.
- Use `findBy*` for an element that appears after data loads or another async step; use `getBy*` for an element present immediately.
- Use `findBy*` instead of `waitFor` wrapped around `getBy*`.

## Vitest: interactions and async

- Simulate user input with `@testing-library/user-event`: create `const user = userEvent.setup()` at the start of the test and drive it with `await user.click(...)`, `await user.type(...)`. Never use `fireEvent`.
- `await` every user-event call and every assertion on async state.
- Control time with `vi.useFakeTimers()` and `vi.runAllTimers()`; never wait on a real `setTimeout` delay.

## Vitest: mocking

- Mock API modules at the module boundary with `vi.mock('<path to api module>')` at the top of the test file, never inside a component or a test body.
- Configure a mocked function through `vi.mocked(...)`: `mockResolvedValueOnce` / `mockRejectedValueOnce` for a single test, `mockResolvedValue` for a shared default.
- Assert the API call a user action produces with `toHaveBeenCalledWith(...)` on the mocked function.
- A spy passed as a handler prop is a `vi.fn()` bound to a name describing the handler it replaces (`const handleApprove = vi.fn()`).

## Vitest: snapshots and coverage

- Do not snapshot page-level or other markup-producing components; assert visible text and ARIA roles instead.
- Use `toMatchSnapshot` only for pure data-transformation functions whose output is stable and not markup.
- Coverage targets are statements ≥ 80%, branches ≥ 75%, functions ≥ 80%; they are not enforced in CI. Run `npm run test:coverage` before pushing.
- Every test makes a meaningful assertion on behavior; a test that only renders a component asserts nothing.

## Vitest: what to test

- Test user-visible behavior: the text that appears, the route navigated to, and the API calls made.
- Test error states: validation messages, server error banners, and empty states.
- Test access control: `ProtectedRoute` redirects when unauthenticated.
- Test hook state transitions: initial → loading → success or error.
- Do not test implementation details: internal state variable names, private functions, or re-render counts.
- Do not test third-party library internals (React Router redirect logic, TanStack Query retry behavior).
- Do not assert styles or CSS classes; assert visibility and semantics through ARIA.
- Do not test the `main.tsx` bootstrap.

## Playwright: stack and configuration

- E2e tests use `@playwright/test` against Chromium only. Install the browser with `npx playwright install chromium --with-deps`.
- Keep these `playwright.config.ts` settings: `testDir: './tests/e2e'`, `fullyParallel: true`, `forbidOnly: !!process.env.CI`, `retries: process.env.CI ? 2 : 0`, `workers: process.env.CI ? 1 : undefined`, `reporter: [['html', { open: 'never' }]]`, `use.baseURL` from `PLAYWRIGHT_BASE_URL` falling back to `http://localhost:5173`, `trace: 'on-first-retry'`, and a `webServer` running `npm run dev` with `reuseExistingServer: !process.env.CI`.
- The `chromium` project depends on the `setup` project (`*.setup.ts`) and reads `storageState` from `tests/e2e/.auth/user.json`.
- Run e2e through the `e2e` (`playwright test`) and `e2e:ui` (`playwright test --ui`) scripts.

## Playwright: files

- Specs live in `tests/e2e/`, one `<feature>.spec.ts` per feature area.
- Fixtures live in `tests/e2e/fixtures/`; Page Object Models live in `tests/e2e/pages/`.
- `tests/e2e/` holds test code only; never put production source there.
- Never commit `tests/e2e/.auth/`; it stays listed in `src/DMAuth.Client/.gitignore`.

## Playwright: isolation and authentication

- Each test is fully independent. Never share state between tests through module-level variables or test order.
- Authenticate the shared user once in `auth.setup.ts` and save it with `storageState`; never log in at the start of every test.
- A test that mutates the signed-in user's own account data (display name, password) takes the `freshUser` fixture, imported with `test` and `expect` from `tests/e2e/fixtures/auth.ts`; the fixture registers a unique user, signs in, and leaves `page` on `/dashboard`.
- A new user's email and username carry a unique suffix drawn per test (`randomBytes(4)` on every `freshUser` fixture call) so tests never collide on database rows.
- After sign-in, navigate by clicking sidebar nav links (client-side routing), not `page.goto`; a full-page navigation discards the query cache.

## Playwright: locators

- Locate in this priority order: `getByRole` (with `name`), then `getByLabel` for form fields, then `getByText` for headings and static content, then `getByTestId` as the last resort.
- Add a `data-testid` only when no semantic locator fits.
- Never use CSS selectors or XPath.
- Match names exactly with `{ exact: true }` or an anchored regex (`/^Create$/i`) wherever a substring would match more than one element.
- Scope locators to `page.locator('main')` when the same text also appears in the sidebar.
- Narrow among similar elements with `.filter({ hasText: ... })` chained from a role locator.
- Treat `npx playwright codegen` output as a draft: review and simplify every generated locator before committing.

## Playwright: assertions

- Assert with web-first assertions only: `await expect(locator).toBeVisible()`, `toHaveText`, `toContainText`, `toBeDisabled`, `await expect(page).toHaveURL(...)`.
- Never assert on a resolved value such as `expect(await locator.isVisible()).toBe(true)`.
- Use `expect.soft()` only to collect several independent failures in one test.

## Playwright: Page Object Models

- Create a Page Object Model only for a page whose interactions are shared by more than one spec file.
- A Page Object Model is a class in `tests/e2e/pages/<Page>Page.ts` taking `Page` in its constructor and exposing navigation and action methods and locator getters.

## Playwright: network

- Run e2e tests against the real DMAuth backend dev server.
- Mock with `page.route(...)` only to produce error states that are hard to reproduce (HTTP 500, timeouts) or to replace a third-party service outside this repository.

## Playwright: debugging and CI

- Diagnose failures with the trace viewer (`npx playwright show-report`) and UI mode (`npm run e2e:ui`); do not add `console.log` statements to a spec to diagnose it.
- Do not enable screenshots or video by default.
- In CI, install only Chromium, run tests serially on the single worker `workers` sets in CI (`fullyParallel: true` parallelizes only local runs), and upload `playwright-report/` as an artifact when the run fails.

## Playwright: what to test

- Test full user journeys across pages (register → dashboard, create client → see it in the list).
- Test auth boundaries: unauthenticated access is blocked, authenticated access succeeds.
- Test the OAuth consent flow end to end.
- Test error recovery: a wrong password shows an error and the user can retry.
- Test form validation feedback in the browser for the happy path and one error path; exhaustive validation belongs in Vitest.
- Do not call third-party services; mock them.
- Do not assert API request payloads in e2e; they belong in Vitest.
- Do not test static content (copyright text, footer links).
