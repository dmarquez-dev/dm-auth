# Audit: test (T6)
Ref: 5b7d9a6

## Scope

`find <R>/src/DMAuth.Client/tests <R>/src/DMAuth.Client/src <R>/src/DMAuth.Client/playwright.config.ts <R>/src/DMAuth.Client/vitest.config.ts <R>/src/DMAuth.Client/tsconfig.test.json -type f ( -path '<R>/src/DMAuth.Client/tests/*' -o -path '<R>/src/DMAuth.Client/src/test/*' -o -name '*.test.ts' -o -name '*.test.tsx' -o -name playwright.config.ts -o -name vitest.config.ts -o -name tsconfig.test.json )` (parentheses escaped for the shell)

Evidence lines below are `grep -n` output with the file name (`grep -Hn`), paths relative to the worktree root. Every scoped file is LF-terminated in the worktree. Census commands run from the worktree root.

## Files

- `src/DMAuth.Client/tests/e2e/auth.setup.ts` — CT-017, CT-018
- `src/DMAuth.Client/tests/e2e/auth.spec.ts` — CT-015, CT-017, CT-020
- `src/DMAuth.Client/tests/e2e/clients.spec.ts` — CT-012, CT-014, CT-017
- `src/DMAuth.Client/tests/e2e/consent.spec.ts` — CT-011, CT-017
- `src/DMAuth.Client/tests/e2e/fixtures/auth.ts` — CT-013, CT-017
- `src/DMAuth.Client/tests/e2e/profile.spec.ts` — CT-013
- `src/DMAuth.Client/src/api/authApi.test.ts` — CT-004
- `src/DMAuth.Client/src/api/client.test.ts` — CT-002, CT-005, CT-006
- `src/DMAuth.Client/src/api/clientApi.test.ts` — CT-004
- `src/DMAuth.Client/src/api/userApi.test.ts` — CT-004
- `src/DMAuth.Client/src/auth/ProtectedRoute.test.tsx` — CT-001, CT-007, CT-009
- `src/DMAuth.Client/src/auth/useAuth.test.ts` — CT-008
- `src/DMAuth.Client/src/components/AppLayout.test.tsx` — CT-007, CT-009, CT-010
- `src/DMAuth.Client/src/components/PublicLayout.test.tsx` — CT-009
- `src/DMAuth.Client/src/pages/ClientCreatePage.test.tsx` — CT-003, CT-004, CT-005, CT-007, CT-009
- `src/DMAuth.Client/src/pages/ClientDetailPage.test.tsx` — CT-001, CT-003, CT-004, CT-005, CT-006, CT-007, CT-009
- `src/DMAuth.Client/src/pages/ClientsPage.test.tsx` — CT-001, CT-003, CT-007
- `src/DMAuth.Client/src/pages/ConsentPage.test.tsx` — CT-004, CT-007, CT-009
- `src/DMAuth.Client/src/pages/DashboardPage.test.tsx` — CT-001, CT-003, CT-007
- `src/DMAuth.Client/src/pages/LoginPage.test.tsx` — CT-009
- `src/DMAuth.Client/src/pages/ProfilePage.test.tsx` — CT-005, CT-006, CT-007
- `src/DMAuth.Client/src/pages/RegisterPage.test.tsx` — CT-009
- `src/DMAuth.Client/src/test/renderWithProviders.tsx` — CT-009
- `src/DMAuth.Client/src/test/setup.ts` — no finding
- `src/DMAuth.Client/playwright.config.ts` — CT-016, CT-018
- `src/DMAuth.Client/vitest.config.ts` — CT-019
- `src/DMAuth.Client/tsconfig.test.json` — CT-019

## Rules checked

- `typescript-style.md` § File naming — conforms
- `typescript-style.md` § Component style — conforms
- `typescript-style.md` § Exports — conforms
- `typescript-style.md` § Props — conforms
- `typescript-style.md` § Styling — n/a: no scoped file declares styles; CSS-class assertions are judged under `client-tests.md`
- `typescript-style.md` § Import organization — CT-009
- `client-tests.md` § Vitest: stack and configuration — conforms
- `client-tests.md` § Vitest: files — CT-007
- `client-tests.md` § Vitest: structure — CT-004, CT-005, CT-006
- `client-tests.md` § Vitest: rendering — conforms
- `client-tests.md` § Vitest: querying the DOM — CT-001
- `client-tests.md` § Vitest: interactions and async — conforms
- `client-tests.md` § Vitest: mocking — CT-003
- `client-tests.md` § Vitest: snapshots and coverage — conforms
- `client-tests.md` § Vitest: what to test — CT-001, CT-002, CT-008
- `client-tests.md` § Playwright: stack and configuration — conforms
- `client-tests.md` § Playwright: files — conforms
- `client-tests.md` § Playwright: isolation and authentication — CT-011, CT-012
- `client-tests.md` § Playwright: locators — CT-013
- `client-tests.md` § Playwright: assertions — conforms
- `client-tests.md` § Playwright: Page Object Models — conforms
- `client-tests.md` § Playwright: network — conforms
- `client-tests.md` § Playwright: debugging and CI — CT-016
- `client-tests.md` § Playwright: what to test — CT-015

## Findings

### CT-001 Vitest specs query loading indicators by CSS class with document.querySelector
- Severity: convention drift
- Layer: test
- Location: `src/DMAuth.Client/src/auth/ProtectedRoute.test.tsx:58`; `src/DMAuth.Client/src/pages/ClientDetailPage.test.tsx:66`; `src/DMAuth.Client/src/pages/ClientsPage.test.tsx:59`; `src/DMAuth.Client/src/pages/DashboardPage.test.tsx:70`
- Rule: discipline .sir/disciplines/client-tests.md § Vitest: querying the DOM; discipline .sir/disciplines/client-tests.md § Vitest: what to test
- Moves: code
- What: Give the spinner (`ProtectedRoute.tsx`) and the loading skeletons an accessible role and name (for example `role="status"` with `aria-label="Loading"`) and assert them with `screen.getByRole('status', ...)` instead of `document.querySelector('.animate-*')`.
- Why: The rule forbids `document.querySelector` and class queries and forbids asserting CSS classes; these tests break on a Tailwind class rename that changes no behaviour, and the loading state stays invisible to assistive technology.
- Verify: `grep -rn 'querySelector' src/DMAuth.Client/src --include='*.test.ts' --include='*.test.tsx' | wc -l` prints 0 and `npm --prefix src/DMAuth.Client run test` passes.
- Census: 4 via `grep -rn 'querySelector' src/DMAuth.Client/src --include='*.test.ts' --include='*.test.tsx' | wc -l`
- Evidence:
  ````
  src/DMAuth.Client/src/auth/ProtectedRoute.test.tsx:58:    expect(document.querySelector('.animate-spin')).toBeInTheDocument()
  src/DMAuth.Client/src/pages/ClientDetailPage.test.tsx:66:    expect(document.querySelector('.animate-pulse')).toBeInTheDocument()
  src/DMAuth.Client/src/pages/ClientsPage.test.tsx:59:    expect(document.querySelectorAll('.animate-pulse')).toHaveLength(3)
  src/DMAuth.Client/src/pages/DashboardPage.test.tsx:70:    expect(document.querySelector('.animate-pulse')).toBeInTheDocument()
  ````

### CT-002 apiClient interceptor test reaches into Axios internals through an `as any` cast
- Severity: convention drift
- Layer: test
- Location: `src/DMAuth.Client/src/api/client.test.ts:7`; `src/DMAuth.Client/src/api/client.test.ts:30`
- Rule: discipline .sir/disciplines/client-tests.md § Vitest: what to test; base test § Behavior over implementation; external typescript-eslint no-explicit-any (in the recommended config) https://typescript-eslint.io/rules/no-explicit-any
- Moves: code
- What: Drive the interceptor through the public `apiClient` (install a stub `adapter` that resolves or rejects with the wanted status and call `apiClient.get(...)`), and delete the `(apiClient.interceptors.response as any).handlers[0]` access.
- Why: The test depends on Axios's private handler array and on the interceptor being index 0, which is a third-party internal and an implementation detail; the `as any` casts are also reported by `@typescript-eslint/no-explicit-any`, which `tseslint.configs.recommended` in `eslint.config.js` enables, so `npm run lint` is expected to report them (lint could not run in this session).
- Verify: `grep -n 'interceptors.response as any' src/DMAuth.Client/src/api/client.test.ts` prints nothing, and `npm --prefix src/DMAuth.Client run lint` and `run test` pass.
- Evidence:
  ````
  src/DMAuth.Client/src/api/client.test.ts:7:  const handlers = (apiClient.interceptors.response as any).handlers as Array<{
  src/DMAuth.Client/src/api/client.test.ts:30:    const handlers = (apiClient.interceptors.response as any).handlers as Array<{
  ````

### CT-003 Persistent never-resolving mockReturnValue leaks into later tests of the same file
- Severity: convention drift
- Layer: test
- Location: `src/DMAuth.Client/src/pages/ClientCreatePage.test.tsx:221`; `src/DMAuth.Client/src/pages/ClientDetailPage.test.tsx:64`; `src/DMAuth.Client/src/pages/ClientDetailPage.test.tsx:201`; `src/DMAuth.Client/src/pages/ClientDetailPage.test.tsx:274`; `src/DMAuth.Client/src/pages/ClientsPage.test.tsx:56`; `src/DMAuth.Client/src/pages/DashboardPage.test.tsx:56`; `src/DMAuth.Client/src/pages/DashboardPage.test.tsx:62`; `src/DMAuth.Client/src/pages/DashboardPage.test.tsx:68`; `src/DMAuth.Client/src/pages/DashboardPage.test.tsx:82`; `src/DMAuth.Client/src/pages/DashboardPage.test.tsx:88`
- Rule: discipline .sir/disciplines/client-tests.md § Vitest: mocking; base test § Isolation; external Vitest mockClear does not reset implementations https://vitest.dev/api/mock.html#mockclear
- Moves: code
- What: Use `mockReturnValueOnce(new Promise(() => {}))` for a single test's pending state (or set `mockReset: true` in `vitest.config.ts`), so `vi.clearAllMocks()` in `beforeEach` is not relied on to undo it.
- Why: `vi.clearAllMocks()` clears calls but keeps the implementation, so the pending promise set in one test becomes the default for every later test in the file; for example the update-success test in `ClientDetailPage.test.tsx` invalidates `['clients', id]` (`ClientDetailPage.tsx` line 106) and its refetch of `getClient` receives the promise leaked from the test at line 64, while run alone it receives `undefined` — the outcome depends on test order.
- Verify: `grep -rn 'mockReturnValue(' src/DMAuth.Client/src --include='*.test.ts' --include='*.test.tsx' | wc -l` prints 0 (or `vitest.config.ts` sets `mockReset: true`), and each spec file passes when run with `-t` on any single test.
- Census: 10 via `grep -rn 'mockReturnValue(' src/DMAuth.Client/src --include='*.test.ts' --include='*.test.tsx' | wc -l`
- Evidence:
  ````
  src/DMAuth.Client/src/pages/ClientCreatePage.test.tsx:221:    vi.mocked(clientApi.createClient).mockReturnValue(new Promise(() => {}) as never)
  src/DMAuth.Client/src/pages/ClientDetailPage.test.tsx:64:    vi.mocked(clientApi.getClient).mockReturnValue(new Promise(() => {}) as never)
  src/DMAuth.Client/src/pages/ClientDetailPage.test.tsx:201:    vi.mocked(clientApi.deleteClient).mockReturnValue(new Promise(() => {}) as never)
  src/DMAuth.Client/src/pages/ClientDetailPage.test.tsx:274:    vi.mocked(clientApi.updateClient).mockReturnValue(new Promise(() => {}) as never)
  src/DMAuth.Client/src/pages/ClientsPage.test.tsx:56:    vi.mocked(clientApi.listClients).mockReturnValue(new Promise(() => {}) as never)
  src/DMAuth.Client/src/pages/DashboardPage.test.tsx:56:    vi.mocked(clientApi.listClients).mockReturnValue(new Promise(() => {}) as never)
  src/DMAuth.Client/src/pages/DashboardPage.test.tsx:62:    vi.mocked(clientApi.listClients).mockReturnValue(new Promise(() => {}) as never)
  src/DMAuth.Client/src/pages/DashboardPage.test.tsx:68:    vi.mocked(clientApi.listClients).mockReturnValue(new Promise(() => {}) as never)
  src/DMAuth.Client/src/pages/DashboardPage.test.tsx:82:    vi.mocked(clientApi.listClients).mockReturnValue(new Promise(() => {}) as never)
  src/DMAuth.Client/src/pages/DashboardPage.test.tsx:88:    vi.mocked(clientApi.listClients).mockReturnValue(new Promise(() => {}) as never)
  ````

### CT-004 Test names that do not complete the sentence "it …"
- Severity: convention drift
- Layer: test
- Location: `src/DMAuth.Client/src/api/authApi.test.ts:17`; `src/DMAuth.Client/src/api/authApi.test.ts:26`; `src/DMAuth.Client/src/api/authApi.test.ts:33`; `src/DMAuth.Client/src/api/authApi.test.ts:39`; `src/DMAuth.Client/src/api/clientApi.test.ts:17`; `src/DMAuth.Client/src/api/userApi.test.ts:17`; `src/DMAuth.Client/src/pages/ClientCreatePage.test.tsx:80`; `src/DMAuth.Client/src/pages/ClientCreatePage.test.tsx:85`; `src/DMAuth.Client/src/pages/ClientCreatePage.test.tsx:125`; `src/DMAuth.Client/src/pages/ConsentPage.test.tsx:81`
- Rule: discipline .sir/disciplines/client-tests.md § Vitest: structure
- Moves: code
- What: Rename each listed `it` (and the rest of the census) to a verb phrase completing "it …", e.g. `it('POSTs the credentials to /api/users/login')`, `it('selects Public as the default client type')`, `it('redirects to redirect_uri with error=access_denied when Deny is clicked')`.
- Why: 101 of the 123 `it` names already follow the rule (123 counted by `grep -rnE "^\s*it\(" src/DMAuth.Client/src --include='*.test.ts' --include='*.test.tsx' | wc -l`); the rest start with a method call, a noun, a quoted control, or "clicking", so failure output reads inconsistently.
- Verify: the Census command prints 0.
- Census: 22 via `grep -rnE "^\s*it\('([^a-z]|[a-zA-Z]+\(|clicking |openid )" src/DMAuth.Client/src --include='*.test.ts' --include='*.test.tsx' | wc -l`
- Evidence:
  ````
  src/DMAuth.Client/src/api/authApi.test.ts:17:  it('login() POSTs to /api/users/login with the supplied credentials', async () => {
  src/DMAuth.Client/src/api/authApi.test.ts:26:  it('register() POSTs to /api/users/register with the supplied data', async () => {
  src/DMAuth.Client/src/api/authApi.test.ts:33:  it('logout() POSTs to /api/users/logout', async () => {
  src/DMAuth.Client/src/api/authApi.test.ts:39:  it('me() GETs /api/users/me', async () => {
  src/DMAuth.Client/src/api/clientApi.test.ts:17:  it('listClients() GETs /api/clients', async () => {
  src/DMAuth.Client/src/api/userApi.test.ts:17:  it('getProfile() GETs /api/users/me', async () => {
  src/DMAuth.Client/src/pages/ClientCreatePage.test.tsx:80:  it('openid scope is pre-selected by default', () => {
  src/DMAuth.Client/src/pages/ClientCreatePage.test.tsx:85:  it('Public is the default client type', () => {
  src/DMAuth.Client/src/pages/ClientCreatePage.test.tsx:125:  it('"+ Add redirect URI" appends a new input', async () => {
  src/DMAuth.Client/src/pages/ConsentPage.test.tsx:81:  it('clicking Deny sets window.location.href to the redirect_uri with error=access_denied', async () => {
  ````

### CT-005 Large specs split one subject into several `<Subject> — <aspect>` describe blocks
- Severity: convention drift
- Layer: test
- Location: `.sir/disciplines/client-tests.md:21`; `src/DMAuth.Client/src/pages/ClientCreatePage.test.tsx:52`; `src/DMAuth.Client/src/pages/ClientDetailPage.test.tsx:60`; `src/DMAuth.Client/src/pages/ProfilePage.test.tsx:37`; `src/DMAuth.Client/src/api/client.test.ts:25`
- Rule: discipline .sir/disciplines/client-tests.md § Vitest: structure
- Moves: discipline
- What: Restate `.sir/disciplines/client-tests.md:21` to allow a spec to group one subject into sibling top-level blocks named `<Subject> — <aspect>` (the form the three largest page specs use), keeping "one subject per file".
- Why: The rule says one `describe` per subject, but four spec files consistently use 19 `<Subject> — <aspect>` blocks; the code is the evidence of the working convention, and a rule nobody follows invites silent drift either way.
- Verify: `.sir/disciplines/client-tests.md` § Vitest: structure names the `<Subject> — <aspect>` form, and the Census command's matches all follow it.
- Census: 19 via `grep -rnE "^describe\('[A-Za-z]+ — " src/DMAuth.Client/src --include='*.test.ts' --include='*.test.tsx' | wc -l`
- Evidence:
  ````
  .sir/disciplines/client-tests.md:21:- Group tests with one `describe` per subject, named after the subject (`describe('LoginPage', ...)`).
  src/DMAuth.Client/src/pages/ClientCreatePage.test.tsx:52:describe('ClientCreatePage — rendering', () => {
  src/DMAuth.Client/src/pages/ClientDetailPage.test.tsx:60:describe('ClientDetailPage — loading and error states', () => {
  src/DMAuth.Client/src/pages/ProfilePage.test.tsx:37:describe('ProfilePage — account info', () => {
  src/DMAuth.Client/src/api/client.test.ts:25:describe('apiClient — response interceptor', () => {
  ````

### CT-006 Input-only test variants copied instead of using it.each
- Severity: convention drift
- Layer: test
- Location: `src/DMAuth.Client/src/api/client.test.ts:52`; `src/DMAuth.Client/src/api/client.test.ts:60`; `src/DMAuth.Client/src/api/client.test.ts:68`; `src/DMAuth.Client/src/api/client.test.ts:76`; `src/DMAuth.Client/src/pages/ClientDetailPage.test.tsx:97`; `src/DMAuth.Client/src/pages/ClientDetailPage.test.tsx:103`; `src/DMAuth.Client/src/pages/ProfilePage.test.tsx:50`; `src/DMAuth.Client/src/pages/ProfilePage.test.tsx:55`
- Rule: discipline .sir/disciplines/client-tests.md § Vitest: structure
- Moves: code
- What: Collapse each group into one `it.each` table: the four "does NOT redirect" cases (pathname, url, status → no redirect), Active/Inactive (`isActive` → label), and Verified/Not verified (`emailVerified` → badge).
- Why: The rule asks for `it.each` where scenarios differ only by input and expected output; the copies differ only in their literals, and `grep -rn 'it.each' src/DMAuth.Client/src` prints nothing.
- Verify: `grep -rn 'it.each' src/DMAuth.Client/src --include='*.test.ts' --include='*.test.tsx' | wc -l` prints at least 3 and the listed copies are gone.
- Evidence:
  ````
  src/DMAuth.Client/src/api/client.test.ts:52:  it('does NOT redirect when the 401 is from /api/users/me (auth check)', async () => {
  src/DMAuth.Client/src/api/client.test.ts:60:  it('does NOT redirect when the 401 is from /api/users/me/change-password (wrong password)', async () => {
  src/DMAuth.Client/src/api/client.test.ts:68:  it('does NOT redirect when already on /login', async () => {
  src/DMAuth.Client/src/api/client.test.ts:76:  it('does NOT redirect for non-401 errors', async () => {
  src/DMAuth.Client/src/pages/ClientDetailPage.test.tsx:97:  it('renders Active status for an active client', async () => {
  src/DMAuth.Client/src/pages/ClientDetailPage.test.tsx:103:  it('renders Inactive status for an inactive client', async () => {
  src/DMAuth.Client/src/pages/ProfilePage.test.tsx:50:  it('shows "Verified" badge when email is verified', () => {
  src/DMAuth.Client/src/pages/ProfilePage.test.tsx:55:  it('shows "Not verified" badge when email is not verified', () => {
  ````

### CT-007 Test data factories duplicated per spec instead of living in src/test/
- Severity: convention drift
- Layer: test
- Location: `src/DMAuth.Client/src/auth/ProtectedRoute.test.tsx:18`; `src/DMAuth.Client/src/components/AppLayout.test.tsx:20`; `src/DMAuth.Client/src/pages/ClientCreatePage.test.tsx:29`; `src/DMAuth.Client/src/pages/ClientDetailPage.test.tsx:30`; `src/DMAuth.Client/src/pages/ClientsPage.test.tsx:28`; `src/DMAuth.Client/src/pages/ConsentPage.test.tsx:18`; `src/DMAuth.Client/src/pages/DashboardPage.test.tsx:28`; `src/DMAuth.Client/src/pages/ProfilePage.test.tsx:26`; `src/DMAuth.Client/src/pages/ClientsPage.test.tsx:36`; `src/DMAuth.Client/src/pages/DashboardPage.test.tsx:36`
- Rule: discipline .sir/disciplines/client-tests.md § Vitest: files
- Moves: code
- What: Move the `testUser` object and the `makeClient` factory into a shared module under `src/DMAuth.Client/src/test/` (e.g. `factories.ts` exporting `makeUser` and `makeClient`) and import it from every spec.
- Why: The rule places shared test data factories in `src/test/`; eight copies of `testUser` and three of `makeClient` (already diverging: `ClientDetailPage.test.tsx` uses other scopes and dates) must each be edited when `User` or `OAuthClient` changes.
- Verify: the Census command prints 0 and a factory module exists under `src/DMAuth.Client/src/test/`.
- Census: 11 via `grep -rnE "const (testUser: User|makeClient) = " src/DMAuth.Client/src | wc -l`
- Evidence:
  ````
  src/DMAuth.Client/src/auth/ProtectedRoute.test.tsx:18:const testUser: User = {
  src/DMAuth.Client/src/components/AppLayout.test.tsx:20:const testUser: User = {
  src/DMAuth.Client/src/pages/ClientCreatePage.test.tsx:29:const testUser: User = {
  src/DMAuth.Client/src/pages/ClientDetailPage.test.tsx:30:const testUser: User = {
  src/DMAuth.Client/src/pages/ClientsPage.test.tsx:28:const testUser: User = {
  src/DMAuth.Client/src/pages/ConsentPage.test.tsx:18:const testUser: User = {
  src/DMAuth.Client/src/pages/DashboardPage.test.tsx:28:const testUser: User = {
  src/DMAuth.Client/src/pages/ProfilePage.test.tsx:26:const testUser: User = {
  src/DMAuth.Client/src/pages/ClientsPage.test.tsx:36:const makeClient = (overrides: Partial<OAuthClient> = {}): OAuthClient => ({
  src/DMAuth.Client/src/pages/DashboardPage.test.tsx:36:const makeClient = (overrides: Partial<OAuthClient> = {}): OAuthClient => ({
  ````

### CT-008 Auth state transitions (loading, success, error, logout) have no test
- Severity: convention drift
- Layer: test
- Location: `src/DMAuth.Client/src/auth/useAuth.test.ts:6`
- Rule: discipline .sir/disciplines/client-tests.md § Vitest: what to test
- Moves: code
- What: Add `renderHook(() => useAuth(), { wrapper })` tests (wrapper = `QueryClientProvider` + `AuthProvider`) covering initial loading → authenticated when `authApi.me` resolves, → unauthenticated when it rejects, and `logout()` calling `authApi.logout` and clearing the user.
- Why: The rule requires hook state transitions initial → loading → success or error; the only hook test checks the outside-provider throw, and `src/DMAuth.Client/src/auth/` has no `AuthProvider` test file, so the `isLoading`/`isAuthenticated` derivation in `AuthProvider.tsx` is covered only indirectly by pre-seeded caches.
- Verify: `grep -c "it(" src/DMAuth.Client/src/auth/useAuth.test.ts` prints at least 4 and `npm --prefix src/DMAuth.Client run test` passes.
- Evidence:
  ````
  src/DMAuth.Client/src/auth/useAuth.test.ts:6:  it('throws when used outside an AuthProvider', () => {
  $ ls src/DMAuth.Client/src/auth
  AuthContext.tsx
  AuthProvider.tsx
  ProtectedRoute.test.tsx
  ProtectedRoute.tsx
  useAuth.test.ts
  useAuth.ts
  ````

### CT-009 Import grouping rule is followed by no test file
- Severity: convention drift
- Layer: test
- Location: `.sir/disciplines/typescript-style.md:39`; `src/DMAuth.Client/src/pages/LoginPage.test.tsx:4`; `src/DMAuth.Client/src/auth/ProtectedRoute.test.tsx:3`; `src/DMAuth.Client/src/test/renderWithProviders.tsx:4`
- Rule: discipline .sir/disciplines/typescript-style.md § Import organization
- Moves: discipline
- What: Restate `.sir/disciplines/typescript-style.md:39` to the order the code actually uses (no blank lines between groups; test files lead with `vitest` and Testing Library), or replace the hand-applied rule with a lint-enforced order (e.g. an import-sorting ESLint plugin) and reorder in one sweep.
- Why: Every scoped file importing `react-router-dom` places it after a third-party import, and no scoped file separates groups with blank lines (production files such as `LoginPage.tsx` do the same), so the rule as written describes no file in the client; "apply by hand" has not held.
- Verify: the restated rule matches the Census population, or the lint rule is enabled and `npm --prefix src/DMAuth.Client run lint` passes.
- Census: 9 via `grep -rl "from 'react-router-dom'" src/DMAuth.Client/src --include='*.test.tsx' --include='renderWithProviders.tsx' | wc -l`
- Evidence:
  ````
  .sir/disciplines/typescript-style.md:39:- Order imports in these groups, with one blank line between groups:
  src/DMAuth.Client/src/pages/LoginPage.test.tsx:4:import { Route, Routes } from 'react-router-dom'
  src/DMAuth.Client/src/auth/ProtectedRoute.test.tsx:3:import { Route, Routes } from 'react-router-dom'
  src/DMAuth.Client/src/test/renderWithProviders.tsx:4:import { MemoryRouter } from 'react-router-dom'
  ````

### CT-010 Duplicate import of react-router-dom in AppLayout.test.tsx
- Severity: hygiene
- Layer: test
- Location: `src/DMAuth.Client/src/components/AppLayout.test.tsx:4`; `src/DMAuth.Client/src/components/AppLayout.test.tsx:5`
- Rule: external ESLint no-duplicate-imports https://eslint.org/docs/latest/rules/no-duplicate-imports
- Moves: n/a
- What: Merge the two statements into `import { Outlet, Route, Routes } from 'react-router-dom'`.
- Why: Two import statements from one module are noise and the usual source of drift when one is edited and the other is not.
- Verify: `grep -c "from 'react-router-dom'" src/DMAuth.Client/src/components/AppLayout.test.tsx` prints 1.
- Evidence:
  ````
  src/DMAuth.Client/src/components/AppLayout.test.tsx:4:import { Route, Routes } from 'react-router-dom'
  src/DMAuth.Client/src/components/AppLayout.test.tsx:5:import { Outlet } from 'react-router-dom'
  ````

### CT-011 Consent e2e tests share a client through describe-scoped variables and beforeAll
- Severity: convention drift
- Layer: test
- Location: `src/DMAuth.Client/tests/e2e/consent.spec.ts:18`; `src/DMAuth.Client/tests/e2e/consent.spec.ts:19`; `src/DMAuth.Client/tests/e2e/consent.spec.ts:21`
- Rule: discipline .sir/disciplines/client-tests.md § Playwright: isolation and authentication; external Playwright fixtures https://playwright.dev/docs/test-fixtures
- Moves: code
- What: Replace the `let oAuthClientId` / `let clientId` + `beforeAll`/`afterAll` pair with a fixture (in `tests/e2e/fixtures/`) that creates the OAuth client, yields its ids, and deletes it in teardown.
- Why: The rule forbids sharing state between tests through module-level variables; with `fullyParallel: true` the hook runs once per worker, so the state's lifetime depends on how tests are distributed, and a failed create leaves every test with `undefined` ids.
- Verify: `grep -nE "^\s+let |beforeAll" src/DMAuth.Client/tests/e2e/consent.spec.ts` prints nothing and `npm --prefix src/DMAuth.Client run e2e` passes.
- Evidence:
  ````
  src/DMAuth.Client/tests/e2e/consent.spec.ts:18:  let oAuthClientId: string
  src/DMAuth.Client/tests/e2e/consent.spec.ts:19:  let clientId: string
  src/DMAuth.Client/tests/e2e/consent.spec.ts:21:  test.beforeAll(async ({ request }) => {
  ````

### CT-012 Clients e2e navigates with page.goto, which the sidebar-navigation rule forbids
- Severity: convention drift
- Layer: test
- Location: `.sir/disciplines/client-tests.md:92`; `src/DMAuth.Client/tests/e2e/clients.spec.ts:5`; `src/DMAuth.Client/tests/e2e/clients.spec.ts:12`; `src/DMAuth.Client/tests/e2e/clients.spec.ts:42`; `src/DMAuth.Client/tests/e2e/clients.spec.ts:63`
- Rule: discipline .sir/disciplines/client-tests.md § Playwright: isolation and authentication
- Moves: discipline
- What: Restate `.sir/disciplines/client-tests.md:92` so it binds only tests on the `freshUser` fixture (whose sign-in warms the query cache), and permits `page.goto` for a `storageState` test's first navigation and for deep links to a record created through the API.
- Why: A `storageState` test starts on a blank page, so its first navigation can only be `page.goto`, and no sidebar link reaches `/clients/<id>`; `clients.spec.ts` follows the only workable pattern, so the rule moves, not the code.
- Verify: the restated rule names its `freshUser` scope and the `page.goto` exceptions, and `clients.spec.ts` satisfies it unchanged.
- Evidence:
  ````
  .sir/disciplines/client-tests.md:92:- After sign-in, navigate by clicking sidebar nav links (client-side routing), not `page.goto`; a full-page navigation discards the query cache.
  src/DMAuth.Client/tests/e2e/clients.spec.ts:5:    await page.goto('/clients')
  src/DMAuth.Client/tests/e2e/clients.spec.ts:12:    await page.goto('/clients/new')
  src/DMAuth.Client/tests/e2e/clients.spec.ts:42:    await page.goto(`/clients/${clientId}`)
  src/DMAuth.Client/tests/e2e/clients.spec.ts:63:    await page.goto(`/clients/${clientId}`)
  ````

### CT-013 CSS selectors used as e2e locators
- Severity: convention drift
- Layer: test
- Location: `src/DMAuth.Client/tests/e2e/profile.spec.ts:28`; `src/DMAuth.Client/tests/e2e/fixtures/auth.ts:44`
- Rule: discipline .sir/disciplines/client-tests.md § Playwright: locators
- Moves: code
- What: Give the display-name input in `ProfilePage.tsx` a `<label htmlFor>` (the field at its `autoComplete="name"` line has none) and locate it with `page.getByLabel('Display name')`; replace `page.locator('nav')` with `page.getByRole('navigation')`.
- Why: The rule forbids CSS selectors and allows only `page.locator('main')` as a scope; an attribute selector breaks on markup changes, and the selector was needed only because the input lacks an accessible label.
- Verify: `grep -rnE "locator\('(input|nav)" src/DMAuth.Client/tests/e2e | wc -l` prints 0 and `npm --prefix src/DMAuth.Client run e2e` passes.
- Evidence:
  ````
  src/DMAuth.Client/tests/e2e/profile.spec.ts:28:    const displayNameInput = page.locator('input[autocomplete="name"]')
  src/DMAuth.Client/tests/e2e/fixtures/auth.ts:44:    await expect(page.locator('nav').getByText(user.email)).toBeVisible()
  ````

### CT-014 Clients e2e tests leave created clients behind on the shared user
- Severity: convention drift
- Layer: test
- Location: `src/DMAuth.Client/tests/e2e/clients.spec.ts:11`; `src/DMAuth.Client/tests/e2e/clients.spec.ts:30`
- Rule: base test § Isolation
- Moves: code
- What: Delete the client each test creates (via `page.request.delete('/api/clients/<id>')` in a fixture teardown), capturing the id from the URL in the create test.
- Why: The base rule says each test leaves nothing behind; both tests add a client to the shared `e2e-shared@dmauth.test` account on every run, so the list `clients.spec.ts` and `DashboardPage` render keeps growing across runs (the delete test and `consent.spec.ts` clean up; these two do not).
- Verify: after `npm --prefix src/DMAuth.Client run e2e`, `GET /api/clients` for the shared user returns the same count as before the run.
- Evidence:
  ````
  src/DMAuth.Client/tests/e2e/clients.spec.ts:11:  test('creates a new public client and shows the secret modal', async ({ page }) => {
  src/DMAuth.Client/tests/e2e/clients.spec.ts:30:  test('updates a client name and shows the success banner', async ({ page }) => {
  ````

### CT-015 Wrong-password e2e test stops at the error and never retries
- Severity: convention drift
- Layer: test
- Location: `src/DMAuth.Client/tests/e2e/auth.spec.ts:58`
- Rule: discipline .sir/disciplines/client-tests.md § Playwright: what to test
- Moves: code
- What: After asserting "Incorrect email or password.", fill the correct password, submit again, and assert `toHaveURL('/dashboard')`.
- Why: The rule's error-recovery journey is "a wrong password shows an error and the user can retry"; the test covers only the first half, so a form left disabled or stale after a 401 would pass.
- Verify: the test at this line ends with a successful sign-in assertion, and `npm --prefix src/DMAuth.Client run e2e` passes.
- Evidence:
  ````
  src/DMAuth.Client/tests/e2e/auth.spec.ts:58:  test('shows "Incorrect email or password" on wrong credentials', async ({ page, request }) => {
  ````

### CT-016 The Playwright CI rule describes a CI e2e job that does not exist
- Severity: convention drift
- Layer: test
- Location: `.sir/disciplines/client-tests.md:124`; `src/DMAuth.Client/playwright.config.ts:8`; `src/DMAuth.Client/playwright.config.ts:9`; `src/DMAuth.Client/playwright.config.ts:10`
- Rule: discipline .sir/disciplines/client-tests.md § Playwright: debugging and CI
- Moves: discipline
- What: Restate `.sir/disciplines/client-tests.md:124` to say e2e runs locally only and the `process.env.CI` branches are reserved for a future job, or move it to a backlog item that adds the job; do not leave the rule describing CI behaviour.
- Why: No workflow under `.github/workflows/` mentions Playwright, so the "install only Chromium, run serially, upload `playwright-report/`" rule and the CI-conditional `forbidOnly`/`retries`/`workers` settings describe nothing that runs — the same kind of drift as the project's lesson about the old docs claiming parallel e2e in CI.
- Verify: `.sir/disciplines/client-tests.md` § Playwright: debugging and CI matches the workflows, checked with `grep -rn -i playwright .github/workflows`.
- Evidence:
  ````
  .sir/disciplines/client-tests.md:124:- In CI, install only Chromium, run tests serially on the single worker `workers` sets in CI (`fullyParallel: true` parallelizes only local runs), and upload `playwright-report/` as an artifact when the run fails.
  src/DMAuth.Client/playwright.config.ts:8:  forbidOnly: !!process.env.CI,
  src/DMAuth.Client/playwright.config.ts:9:  retries: process.env.CI ? 2 : 0,
  src/DMAuth.Client/playwright.config.ts:10:  workers: process.env.CI ? 1 : undefined,
  $ grep -rn -i playwright .github/workflows; echo "exit=$?"
  exit=1
  ````

### CT-017 E2e API setup calls never check the response status
- Severity: hygiene
- Layer: test
- Location: `src/DMAuth.Client/tests/e2e/auth.setup.ts:11`; `src/DMAuth.Client/tests/e2e/fixtures/auth.ts:35`; `src/DMAuth.Client/tests/e2e/auth.spec.ts:46`; `src/DMAuth.Client/tests/e2e/auth.spec.ts:62`; `src/DMAuth.Client/tests/e2e/auth.spec.ts:85`; `src/DMAuth.Client/tests/e2e/clients.spec.ts:32`; `src/DMAuth.Client/tests/e2e/clients.spec.ts:53`; `src/DMAuth.Client/tests/e2e/consent.spec.ts:22`
- Rule: external Playwright API testing, `expect(response).toBeOK()` https://playwright.dev/docs/api-testing
- Moves: n/a
- What: Assert each setup response with `await expect(response).toBeOK()` (in `auth.setup.ts`, accept 409 explicitly as the documented idempotent case).
- Why: When the backend is down or rejects the payload, the tests fail later at an unrelated UI step (sign-in or `createRes.json()` destructuring), hiding the real cause; `auth.setup.ts` swallows every non-409 failure the comment does not mention.
- Verify: `grep -rn "toBeOK" src/DMAuth.Client/tests/e2e | wc -l` prints at least 8.
- Evidence:
  ````
  src/DMAuth.Client/tests/e2e/auth.setup.ts:11:  await request.post('/api/users/register', {
  src/DMAuth.Client/tests/e2e/fixtures/auth.ts:35:    await request.post('/api/users/register', { data: user })
  src/DMAuth.Client/tests/e2e/auth.spec.ts:46:    await request.post('/api/users/register', {
  src/DMAuth.Client/tests/e2e/auth.spec.ts:62:    await request.post('/api/users/register', {
  src/DMAuth.Client/tests/e2e/auth.spec.ts:85:    await request.post('/api/users/register', {
  src/DMAuth.Client/tests/e2e/clients.spec.ts:32:    const createRes = await page.request.post('/api/clients', {
  src/DMAuth.Client/tests/e2e/clients.spec.ts:53:    const createRes = await page.request.post('/api/clients', {
  src/DMAuth.Client/tests/e2e/consent.spec.ts:22:    const res = await request.post('/api/clients', {
  ````

### CT-018 Storage-state file path is relative to the process working directory
- Severity: hygiene
- Layer: test
- Location: `src/DMAuth.Client/tests/e2e/auth.setup.ts:3`; `src/DMAuth.Client/playwright.config.ts:25`
- Rule: external Playwright authentication guide (resolves the auth file with `path.join(__dirname, ...)`) https://playwright.dev/docs/auth
- Moves: n/a
- What: Build the auth file path from the config/spec directory (`path.join(__dirname, '.auth/user.json')` or an `import.meta.dirname` equivalent) and share one constant between `auth.setup.ts` and `playwright.config.ts`.
- Why: `page.context().storageState({ path })` resolves a relative path against the working directory, so running Playwright from the repository root (`npx playwright test -c src/DMAuth.Client`) writes the session outside `src/DMAuth.Client/tests/e2e/.auth/`, the path `src/DMAuth.Client/.gitignore` ignores; the literal is also duplicated in two files.
- Verify: `grep -rn "'tests/e2e/.auth/user.json'" src/DMAuth.Client/tests src/DMAuth.Client/playwright.config.ts | wc -l` prints 0.
- Evidence:
  ````
  src/DMAuth.Client/tests/e2e/auth.setup.ts:3:const AUTH_FILE = 'tests/e2e/.auth/user.json'
  src/DMAuth.Client/playwright.config.ts:25:        storageState: 'tests/e2e/.auth/user.json',
  ````

### CT-019 E2e specs and test configs are type-checked by nothing; tsconfig.test.json is inert
- Severity: hygiene
- Layer: test
- Location: `src/DMAuth.Client/tsconfig.test.json:4`; `src/DMAuth.Client/tsconfig.test.json:6`; `src/DMAuth.Client/vitest.config.ts:9`
- Rule: external Playwright TypeScript (Playwright transpiles without type checking, so run `tsc --noEmit` separately) https://playwright.dev/docs/test-typescript; external Vitest `typecheck` options apply only when typecheck is enabled https://vitest.dev/config/#typecheck
- Moves: n/a
- What: Make `tsconfig.test.json` the test type-check project: include `tests/e2e`, `playwright.config.ts` and `vitest.config.ts` with `node` types, reference it from `tsconfig.json` (so `tsc -b` covers it), and drop the `typecheck` block from `vitest.config.ts` unless a script enables `--typecheck`.
- Why: `tsconfig.json` references only `tsconfig.app.json` (`include: ["src"]`) and `tsconfig.node.json` (`include: ["vite.config.ts"]`), so no project compiles `tests/e2e/**`, `playwright.config.ts` or `vitest.config.ts`; `tsconfig.test.json` is read only by Vitest's typecheck mode, which no script enables, and its `src` include duplicates the app project.
- Verify: `npx --prefix src/DMAuth.Client tsc -p src/DMAuth.Client/tsconfig.test.json --noEmit` type-checks `tests/e2e/auth.spec.ts` (introduce a type error there and see it fail), and `npm --prefix src/DMAuth.Client run build` still passes.
- Evidence:
  ````
  src/DMAuth.Client/tsconfig.test.json:4:    "types": ["vite/client", "@testing-library/jest-dom"]
  src/DMAuth.Client/tsconfig.test.json:6:  "include": ["src"]
  src/DMAuth.Client/vitest.config.ts:9:      typecheck: { tsconfig: './tsconfig.test.json' },
  ````

### CT-020 Unused `request` fixture in the registration e2e test
- Severity: hygiene
- Layer: test
- Location: `src/DMAuth.Client/tests/e2e/auth.spec.ts:12`
- Rule: external ESLint no-unused-vars https://eslint.org/docs/latest/rules/no-unused-vars
- Moves: n/a
- What: Drop `request` from the destructured fixtures of this test.
- Why: The test registers through the UI and never uses `request`; the dead parameter suggests an API setup step that does not exist.
- Verify: `sed -n '12p' src/DMAuth.Client/tests/e2e/auth.spec.ts` shows `async ({ page })`.
- Evidence:
  ````
  src/DMAuth.Client/tests/e2e/auth.spec.ts:12:  test('registers a new account and redirects to login with success banner', async ({ page, request }) => {
  ````
