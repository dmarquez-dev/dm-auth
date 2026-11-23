# Audit: frontend (T4)
Ref: 5b7d9a6

## Scope

`find <R>/src/DMAuth.Client -type f -not -path '*/node_modules/*' -not -path '<R>/src/DMAuth.Client/coverage/*' -not -path '<R>/src/DMAuth.Client/tests/*' -not -path '<R>/src/DMAuth.Client/src/test/*' -not -name '*.test.ts' -not -name '*.test.tsx' -not -path '<R>/src/DMAuth.Client/playwright.config.ts' -not -path '<R>/src/DMAuth.Client/vitest.config.ts' -not -path '<R>/src/DMAuth.Client/tsconfig.test.json' -not -path '<R>/src/DMAuth.Client/.gitignore'`

Reference standards, in order: the wired fragments `.sir/disciplines/typescript-style.md` and `.sir/disciplines/react.md`; the sir-sdlc base frontend discipline composed into the session's frontend discipline; then cited external best practice. Line numbers are `grep -n` positions in the worktree at `5b7d9a6`; the scoped text files are LF-terminated.

## Files

- `src/DMAuth.Client/README.md` — FE-024
- `src/DMAuth.Client/eslint.config.js` — no finding
- `src/DMAuth.Client/index.html` — FE-024
- `src/DMAuth.Client/package-lock.json` — no finding
- `src/DMAuth.Client/package.json` — no finding
- `src/DMAuth.Client/public/favicon.svg` — no finding
- `src/DMAuth.Client/public/icons.svg` — FE-023
- `src/DMAuth.Client/src/App.tsx` — FE-014, FE-015, FE-020, FE-022
- `src/DMAuth.Client/src/api/authApi.ts` — FE-015
- `src/DMAuth.Client/src/api/client.ts` — FE-003, FE-006
- `src/DMAuth.Client/src/api/clientApi.ts` — FE-015
- `src/DMAuth.Client/src/api/userApi.ts` — FE-015, FE-023
- `src/DMAuth.Client/src/assets/hero.png` — FE-023
- `src/DMAuth.Client/src/assets/react.svg` — FE-023
- `src/DMAuth.Client/src/assets/vite.svg` — FE-023
- `src/DMAuth.Client/src/auth/AuthContext.tsx` — FE-015, FE-016
- `src/DMAuth.Client/src/auth/AuthProvider.tsx` — FE-009, FE-015, FE-021
- `src/DMAuth.Client/src/auth/ProtectedRoute.tsx` — FE-015, FE-022
- `src/DMAuth.Client/src/auth/useAuth.ts` — FE-015
- `src/DMAuth.Client/src/components/AppLayout.tsx` — FE-009, FE-015, FE-022
- `src/DMAuth.Client/src/components/PublicLayout.tsx` — no finding
- `src/DMAuth.Client/src/index.css` — no finding
- `src/DMAuth.Client/src/main.tsx` — FE-015
- `src/DMAuth.Client/src/pages/ClientCreatePage.tsx` — FE-010, FE-011, FE-012, FE-013, FE-015, FE-018, FE-019
- `src/DMAuth.Client/src/pages/ClientDetailPage.tsx` — FE-008, FE-011, FE-012, FE-013, FE-015, FE-018, FE-019
- `src/DMAuth.Client/src/pages/ClientsPage.tsx` — FE-007, FE-015
- `src/DMAuth.Client/src/pages/ConsentPage.tsx` — FE-001, FE-002, FE-003, FE-014, FE-015, FE-022
- `src/DMAuth.Client/src/pages/DashboardPage.tsx` — FE-007, FE-015
- `src/DMAuth.Client/src/pages/LoginPage.tsx` — FE-004, FE-011, FE-015
- `src/DMAuth.Client/src/pages/ProfilePage.tsx` — FE-011, FE-012, FE-013, FE-015, FE-019
- `src/DMAuth.Client/src/pages/RegisterPage.tsx` — FE-011, FE-012, FE-015
- `src/DMAuth.Client/src/types/auth.ts` — no finding
- `src/DMAuth.Client/src/types/client.ts` — no finding
- `src/DMAuth.Client/src/types/oauth.ts` — FE-023
- `src/DMAuth.Client/staticwebapp.config.json` — FE-003, FE-005
- `src/DMAuth.Client/tsconfig.app.json` — FE-017
- `src/DMAuth.Client/tsconfig.json` — no finding
- `src/DMAuth.Client/tsconfig.node.json` — FE-017
- `src/DMAuth.Client/vite.config.ts` — FE-003

## Rules checked

- `typescript-style.md` § File naming — FE-016
- `typescript-style.md` § Component style — conforms
- `typescript-style.md` § Exports — conforms
- `typescript-style.md` § Props — conforms
- `typescript-style.md` § Styling — conforms
- `typescript-style.md` § Import organization — FE-015
- `react.md` § Project context — conforms
- `react.md` § Project structure — conforms
- `react.md` § State management — FE-014
- `react.md` § API client modules — FE-006, FE-012, FE-017
- `react.md` § TanStack Query integration — FE-013
- `react.md` § Form validation — FE-011, FE-012, FE-013
- `react.md` § Async and waterfalls — conforms
- `react.md` § Bundle size — conforms
- `react.md` § Browser events and storage — n/a: no scoped file registers a touch/wheel/global listener or reads localStorage, sessionStorage or document.cookie
- `react.md` § Re-renders — conforms
- `react.md` § Rendering — FE-020
- `react.md` § JavaScript performance — conforms
- `react.md` § Effects and initialization — FE-021

## Findings

### FE-001 Consent "Deny" redirects to an unvalidated redirect_uri
- Severity: correctness/security risk
- Layer: frontend
- Location: `src/DMAuth.Client/src/pages/ConsentPage.tsx:61`; `src/DMAuth.Client/src/pages/ConsentPage.tsx:65`
- Rule: external RFC 6749 §4.1.2.1 Error Response https://www.rfc-editor.org/rfc/rfc6749#section-4.1.2.1; external OWASP Unvalidated Redirects and Forwards Cheat Sheet https://cheatsheetseries.owasp.org/cheatsheets/Unvalidated_Redirects_and_Forwards_Cheat_Sheet.html
- Moves: n/a
- What: Stop building the denial redirect in the browser from the `redirect_uri` query parameter; submit the denial to the server (e.g. the consent POST with a deny action) so it redirects only to a URI validated against the client's registered redirect URIs, and handle a malformed URI without an uncaught `new URL` exception.
- Why: `/consent?...&redirect_uri=https://attacker.example` is reachable directly, and clicking Deny sends the user (with `state`) to any URL in the query string; RFC 6749 says the server must not redirect to an unvalidated redirect URI.
- Verify: Open `/consent?client_id=x&redirect_uri=https://example.org/&scope=openid&state=s&code_challenge=c&code_challenge_method=S256` while signed in, click Deny, and confirm the browser is not sent to example.org.
- Evidence:
  ```
  61:    const url = new URL(redirectUri)
  65:    window.location.href = url.toString()
  ```

### FE-002 Consent approval form carries no anti-CSRF token
- Severity: correctness/security risk
- Layer: frontend
- Location: `src/DMAuth.Client/src/pages/ConsentPage.tsx:121`
- Rule: external OWASP Cross-Site Request Forgery Prevention Cheat Sheet https://cheatsheetseries.owasp.org/cheatsheets/Cross-Site_Request_Forgery_Prevention_Cheat_Sheet.html
- Moves: n/a
- What: Add a synchronizer/antiforgery token (fetched from the API and posted as a hidden field) to the consent form, paired with server-side validation on `POST /connect/consent` (backend counterpart belongs to the T2 layer).
- Why: The form is a plain cookie-authenticated cross-origin-capable POST whose fields are all attacker-choosable, and the session cookie is configured `SameSite=None` (`src/DMAuth.Web/Program.cs`, outside this scope), so any site can auto-submit a consent grant for a victim and receive an authorization code at a redirect URI registered by the attacker's own client.
- Verify: From a page on another origin, auto-submit a form to `/connect/consent` with a signed-in victim session and confirm the server rejects it (400/403) instead of issuing a 302 with a code.
- Evidence:
  ```
  121:          <form method="post" action="/connect/consent" className="mb-3">
  ```

### FE-003 Consent form posts to the SPA origin, bypassing the configured API base URL
- Severity: correctness/security risk
- Layer: frontend
- Location: `src/DMAuth.Client/src/pages/ConsentPage.tsx:121`; `src/DMAuth.Client/src/api/client.ts:4`; `src/DMAuth.Client/vite.config.ts:15`; `src/DMAuth.Client/staticwebapp.config.json:2`
- Rule: external Vite Env Variables and Modes https://vite.dev/guide/env-and-mode; external Configure Azure Static Web Apps https://learn.microsoft.com/en-us/azure/static-web-apps/configuration
- Moves: n/a
- What: Build the consent form `action` from `import.meta.env.VITE_API_BASE_URL` (the same base `apiClient` uses), or add an explicit `/connect/*` route to the API in `staticwebapp.config.json`; do not rely on the dev-server proxy.
- Why: Every XHR call goes to `VITE_API_BASE_URL` (set to the App Service host in the SWA workflow), but the consent POST is relative, so it only reaches the API under `vite dev`'s `/connect` proxy; on the deployed Static Web App (`api_location: ""`, no routes) it posts to the static host.
- Verify: `grep -n 'action=' src/DMAuth.Client/src/pages/ConsentPage.tsx` shows an action derived from the API base URL, and a consent approval on the deployed site returns the API's 302 to the client redirect URI.
- Evidence:
  ```
  121:          <form method="post" action="/connect/consent" className="mb-3">
  4:  baseURL: import.meta.env.VITE_API_BASE_URL || '/',
  15:      '/connect': {
  2:  "navigationFallback": {
  ```

### FE-004 Login returnUrl is passed unchecked to client-side navigate
- Severity: correctness/security risk
- Layer: frontend
- Location: `src/DMAuth.Client/src/pages/LoginPage.tsx:20`; `src/DMAuth.Client/src/pages/LoginPage.tsx:33`
- Rule: external React Router useNavigate https://reactrouter.com/api/hooks/useNavigate; external OWASP Unvalidated Redirects and Forwards Cheat Sheet https://cheatsheetseries.owasp.org/cheatsheets/Unvalidated_Redirects_and_Forwards_Cheat_Sheet.html
- Moves: n/a
- What: Classify `returnUrl` before use: an app-relative path (starts with `/`, not `//`) goes to `navigate`; an allow-listed API URL (the `/connect/authorize` resume URL) is loaded with `window.location.assign`; anything else falls back to `/dashboard`.
- Why: `AuthController.AuthorizeAsync` (outside this scope) redirects unauthenticated users to `/login?returnUrl=<absolute display URL of /connect/authorize>`, and `navigate` performs a client-side history push, so after sign-in the OAuth flow is not resumed against the server; nothing restricts the parameter to this app's paths either.
- Verify: Start an authorization at `/connect/authorize?...` signed out, sign in, and confirm the browser reaches the consent page or the client's redirect URI; `/login?returnUrl=//example.org` lands on `/dashboard`.
- Evidence:
  ```
  20:  const returnUrl = searchParams.get('returnUrl') ?? '/dashboard'
  33:      navigate(returnUrl, { replace: true })
  ```

### FE-005 Static Web App sends no security headers (CSP, frame-ancestors)
- Severity: correctness/security risk
- Layer: frontend
- Location: `src/DMAuth.Client/staticwebapp.config.json`
- Rule: external OWASP Clickjacking Defense Cheat Sheet https://cheatsheetseries.owasp.org/cheatsheets/Clickjacking_Defense_Cheat_Sheet.html; external Azure Static Web Apps global headers https://learn.microsoft.com/en-us/azure/static-web-apps/configuration#global-headers
- Moves: n/a
- What: Add a `globalHeaders` block to the whole file's configuration with at least `Content-Security-Policy` (including `frame-ancestors 'none'`), `X-Content-Type-Options: nosniff` and `Referrer-Policy`.
- Why: The app hosts the OAuth consent screen; without `frame-ancestors`/`X-Frame-Options` it can be framed for clickjacking of "Allow access", and without a CSP there is no defence-in-depth against script injection on an auth origin.
- Verify: `curl -sI https://<swa-host>/consent` shows `Content-Security-Policy` with `frame-ancestors`.
- Evidence:
  ```
  $ grep -c 'globalHeaders\|Content-Security-Policy\|frame-ancestors\|X-Frame-Options' src/DMAuth.Client/staticwebapp.config.json
  0
  ```

### FE-006 401 interceptor exempts every /api/users/me request, not only the session check
- Severity: correctness/security risk
- Layer: frontend
- Location: `src/DMAuth.Client/src/api/client.ts:16`; `src/DMAuth.Client/src/api/client.ts:17`
- Rule: discipline .sir/disciplines/react.md § API client modules
- Moves: code
- What: Match the exemption on method and URL (`GET /api/users/me` and `POST /api/users/me/change-password`) so `PUT /api/users/me` (profile update) gets the session-expiry redirect.
- Why: The rule exempts only the session-check and change-password endpoints; comparing the URL alone also exempts `userApi.updateProfile`, so an expired session during a profile save shows "Failed to update profile." instead of redirecting to login.
- Verify: A unit test in `client.test.ts` that rejects `PUT /api/users/me` with 401 expects `window.location.href` to become `/login?returnUrl=...`.
- Evidence:
  ```
  16:      error.config?.url === '/api/users/me' ||
  17:      error.config?.url === '/api/users/me/change-password'
  ```

### FE-007 Client list queries have no error state
- Severity: correctness/security risk
- Layer: frontend
- Location: `src/DMAuth.Client/src/pages/ClientsPage.tsx:6`; `src/DMAuth.Client/src/pages/ClientsPage.tsx:32`; `src/DMAuth.Client/src/pages/DashboardPage.tsx:30`; `src/DMAuth.Client/src/pages/DashboardPage.tsx:47`
- Rule: base frontend § Data fetching
- Moves: code
- What: Read `isError` from both `['clients']` queries and render an error state (ClientsPage: an alert with retry; DashboardPage: a stat card error/placeholder) instead of the empty/zero rendering.
- Why: On a failed request `clients` defaults to `[]`/`undefined`, so ClientsPage tells the user "No clients yet." and the dashboard shows 0 registered clients: a failure is rendered as a true empty result.
- Verify: A component test mocking `clientApi.listClients` to reject expects an error message and no "No clients yet." text.
- Evidence:
  ```
  6:  const { data: clients = [], isLoading } = useQuery({
  32:      ) : clients.length === 0 ? (
  30:  const { data: clients, isLoading: clientsLoading } = useQuery({
  47:          value={clients?.length ?? 0}
  ```

### FE-008 Client delete failure is silently ignored
- Severity: correctness/security risk
- Layer: frontend
- Location: `src/DMAuth.Client/src/pages/ClientDetailPage.tsx:117`
- Rule: base frontend § Data fetching
- Moves: code
- What: Give the delete mutation an error state (`onError` or `remove_.isError`) and render the message inside the confirm modal.
- Why: When `DELETE /api/clients/{id}` fails the modal just re-enables its buttons with no feedback, so the user cannot tell whether the client was deleted.
- Verify: A component test rejecting `clientApi.deleteClient` expects an error message in the dialog.
- Evidence:
  ```
  117:  const remove_ = useMutation({
  ```

### FE-009 Logout has no failure handling
- Severity: correctness/security risk
- Layer: frontend
- Location: `src/DMAuth.Client/src/auth/AuthProvider.tsx:20`; `src/DMAuth.Client/src/auth/AuthProvider.tsx:21`; `src/DMAuth.Client/src/components/AppLayout.tsx:69`
- Rule: base frontend § Data fetching
- Moves: code
- What: Wrap `authApi.logout()` in `try`/`finally` (clear the query cache and leave the app either way) or surface an error, so the rejected promise is not dropped by the `onClick` handler.
- Why: If the logout request fails, `logout` rejects into an unhandled promise from the Sign out button, the cache is not cleared and the user stays on the signed-in screen with no indication.
- Verify: A test rejecting `authApi.logout` expects `queryClient.clear()` to be called and no unhandled rejection.
- Evidence:
  ```
  20:  const logout = async () => {
  21:    await authApi.logout()
  69:            onClick={logout}
  ```

### FE-010 Copy buttons report "Copied!" without awaiting the clipboard write
- Severity: correctness/security risk
- Layer: frontend
- Location: `src/DMAuth.Client/src/pages/ClientCreatePage.tsx:34`
- Rule: external MDN Clipboard.writeText() https://developer.mozilla.org/en-US/docs/Web/API/Clipboard/writeText
- Moves: n/a
- What: Await `navigator.clipboard.writeText` and set the copied state only on success; show a failure hint otherwise.
- Why: `writeText` returns a promise that rejects when permission or a secure context is missing; the modal shows the one-time client secret, and telling the user it was copied when it was not loses the secret.
- Verify: A test with `navigator.clipboard.writeText` rejecting expects no "Copied!" text.
- Evidence:
  ```
  34:    navigator.clipboard.writeText(value)
  ```

### FE-011 Form API-error banners lack role="alert"
- Severity: convention drift
- Layer: frontend
- Location: `src/DMAuth.Client/src/pages/LoginPage.tsx:89`; `src/DMAuth.Client/src/pages/RegisterPage.tsx:153`; `src/DMAuth.Client/src/pages/ClientDetailPage.tsx:274`; `src/DMAuth.Client/src/pages/ClientCreatePage.tsx:271`; `src/DMAuth.Client/src/pages/ProfilePage.tsx:136`; `src/DMAuth.Client/src/pages/ProfilePage.tsx:222`
- Rule: discipline .sir/disciplines/react.md § Form validation
- Moves: code
- What: Add `role="alert"` to each `errors.root` banner.
- Why: The rule requires the root API error in a `role="alert"` banner; without it screen readers do not announce a failed submit.
- Verify: `grep -rn 'errors.root &&' -A1 src/DMAuth.Client/src/pages --include=*.tsx | grep -v test | grep -c 'role="alert"'` equals the number of banners.
- Evidence:
  ```
  89:          <p className="rounded-md bg-red-50 px-3 py-2 text-sm text-red-700">
  153:          <p className="rounded-md bg-red-50 px-3 py-2 text-sm text-red-700">
  274:            <p className="rounded-md bg-red-50 px-3 py-2 text-sm text-red-700">
  271:            <p className="rounded-md bg-red-50 px-3 py-2 text-sm text-red-700">
  136:            <p className="rounded-md bg-red-50 px-3 py-2 text-sm text-red-700">
  222:            <p className="rounded-md bg-red-50 px-3 py-2 text-sm text-red-700">
  ```

### FE-012 API error payloads are read untyped, with no shared extractApiError
- Severity: convention drift
- Layer: frontend
- Location: `src/DMAuth.Client/src/pages/RegisterPage.tsx:56`; `src/DMAuth.Client/src/pages/ClientDetailPage.tsx:111`; `src/DMAuth.Client/src/pages/ClientCreatePage.tsx:130`; `src/DMAuth.Client/src/pages/ProfilePage.tsx:66`
- Rule: discipline .sir/disciplines/react.md § Form validation; discipline .sir/disciplines/react.md § API client modules
- Moves: code
- What: Add the `extractApiError(err)` helper the rule names (typed with an `ApiError`/ProblemDetails type in `src/types/`, using `isAxiosError<ApiError>`) and replace the inline `error.response?.data?.…` reads with it.
- Why: `error.response.data` is `any`, so `.message`/`.errors.Username[0]` are unchecked against the API's error shape, and four pages each re-implement extraction differently; the rule names a helper that does not exist in the code.
- Verify: `grep -rn 'response?.data?\|response.data?' src/DMAuth.Client/src/pages --include=*.tsx | grep -v test` prints nothing and `extractApiError` is defined once.
- Evidence:
  ```
  56:          const detail = error.response.data?.errors
  111:        ? (error.response?.data?.message ?? 'Failed to update client.')
  130:        ? (error.response?.data?.message ?? 'Failed to register client.')
  66:        ? (error.response?.data?.message ?? 'Failed to update profile.')
  ```

### FE-013 Mutation-backed forms disable on isPending, not formState.isSubmitting
- Severity: convention drift
- Layer: frontend
- Location: `.sir/disciplines/react.md:61`; `src/DMAuth.Client/src/pages/ClientDetailPage.tsx:288`; `src/DMAuth.Client/src/pages/ClientCreatePage.tsx:279`; `src/DMAuth.Client/src/pages/ProfilePage.tsx:150`; `src/DMAuth.Client/src/pages/ProfilePage.tsx:236`
- Rule: discipline .sir/disciplines/react.md § Form validation; discipline .sir/disciplines/react.md § TanStack Query integration
- Moves: discipline
- What: Restate react.md:61 so a form submitted through `useMutation(...).mutate` disables its submit button on the mutation's `isPending`, keeping `formState.isSubmitting` for forms whose `onSubmit` awaits the call (auth session calls).
- Why: The code is the evidence: `handleSubmit(values => mutation.mutate(values))` returns synchronously, so `isSubmitting` is never true for these forms and `isPending` is the only correct flag; the two rules conflict as written.
- Verify: `sed -n '61p' .sir/disciplines/react.md` names the `isPending` case.
- Evidence:
  ```
  61:- Disable the submit button while `formState.isSubmitting`.
  288:              disabled={update.isPending || !isDirty}
  279:              disabled={create.isPending}
  150:              disabled={updateProfile.isPending || !profileForm.formState.isDirty}
  236:              disabled={changePassword.isPending}
  ```

### FE-014 ConsentPage checks authentication itself instead of the route guard
- Severity: convention drift
- Layer: frontend
- Location: `src/DMAuth.Client/src/pages/ConsentPage.tsx:45`; `src/DMAuth.Client/src/App.tsx:32`
- Rule: discipline .sir/disciplines/react.md § State management
- Moves: code
- What: Wrap the `/consent` route in `ProtectedRoute` (under a layout without the app sidebar, which could also replace the page's copy of the PublicLayout chrome) and delete the page's own loading/redirect branches.
- Why: The rule says every authenticated route uses `ProtectedRoute` and a page does not check auth itself; ConsentPage duplicates the guard's spinner and returnUrl logic.
- Verify: `grep -n 'isAuthenticated' src/DMAuth.Client/src/pages/ConsentPage.tsx` prints nothing and the `/consent` route element is inside `ProtectedRoute`.
- Evidence:
  ```
  45:  if (!isAuthenticated) {
  32:          <Route path="/consent" element={<ConsentPage />} />
  ```

### FE-015 Import groups are out of order and never blank-line separated
- Severity: convention drift
- Layer: frontend
- Location: `src/DMAuth.Client/src/App.tsx:3`; `src/DMAuth.Client/src/App.tsx:5`; `src/DMAuth.Client/src/auth/AuthProvider.tsx:1`; `src/DMAuth.Client/src/pages/LoginPage.tsx:4`; `src/DMAuth.Client/src/pages/RegisterPage.tsx:4`; `src/DMAuth.Client/src/pages/ClientCreatePage.tsx:6`; `src/DMAuth.Client/src/pages/ClientCreatePage.tsx:9`; `src/DMAuth.Client/src/pages/ProfilePage.tsx:7`; `src/DMAuth.Client/src/main.tsx:4`
- Rule: discipline .sir/disciplines/typescript-style.md § Import organization
- Moves: code
- What: Reorder each file's imports into React-ecosystem, third-party, internal (`types/`, `api/`, `auth/`, `components/`, `pages/`), relative groups with one blank line between groups.
- Why: React-ecosystem imports follow third-party ones (react-router-dom after zod/@tanstack), `components/` precedes `auth/`, `api/` follows `types/`/`auth/` out of order, and no non-test source file with two or more imports separates its groups.
- Verify: Re-run the Census command; it prints 0.
- Census: 18 via `find src/DMAuth.Client/src -type f \( -name '*.ts' -o -name '*.tsx' \) ! -name '*.test.*' ! -path '*/src/test/*' -exec awk '/^import/{n++; if(pb&&n>1)b=1} {pb=($0=="")} END{if(n>=2&&!b)print FILENAME}' {} \; | wc -l`
- Evidence:
  ```
  3:import { AppLayout } from './components/AppLayout'
  5:import { ProtectedRoute } from './auth/ProtectedRoute'
  1:import { useQueryClient, useQuery } from '@tanstack/react-query'
  4:import { Link, useNavigate, useSearchParams } from 'react-router-dom'
  4:import { Link, useNavigate } from 'react-router-dom'
  6:import { useNavigate } from 'react-router-dom'
  9:import type { RegisterClientResponse } from '../types/client'
  7:import { userApi } from '../api/userApi'
  4:import { App } from './App.tsx'
  ```

### FE-016 Non-component context module is named PascalCase .tsx
- Severity: convention drift
- Layer: frontend
- Location: `.sir/disciplines/typescript-style.md:8`; `src/DMAuth.Client/src/auth/AuthContext.tsx:11`
- Rule: discipline .sir/disciplines/typescript-style.md § File naming
- Moves: discipline
- What: Restate typescript-style.md:8 with an exception: a module exporting a React context object is named after the context (`AuthContext.ts`), and drop the `.tsx` extension where the file holds no JSX.
- Why: The code is the evidence: `AuthContext.tsx` holds no component and no JSX, only `createContext`, and its PascalCase name follows the common React convention for contexts; the rule as written would demand `authContext.ts` and a rename across its importers.
- Verify: `sed -n '8,9p' .sir/disciplines/typescript-style.md` states the context-module case.
- Evidence:
  ```
  8:- Name every non-component TypeScript file in camelCase: `authApi.ts`, `renderWithProviders.tsx`.
  11:export const AuthContext = createContext<AuthContextValue | null>(null)
  ```

### FE-017 TypeScript strict mode is not enabled
- Severity: convention drift
- Layer: frontend
- Location: `src/DMAuth.Client/tsconfig.app.json:18`; `src/DMAuth.Client/tsconfig.node.json:17`
- Rule: discipline .sir/disciplines/react.md § API client modules; external TypeScript TSConfig reference: strict https://www.typescriptlang.org/tsconfig/#strict
- Moves: code
- What: Add `"strict": true` to the linting options of both configs and fix the resulting errors.
- Why: Without `strict` (no `noImplicitAny`, no `strictNullChecks`) the compiler cannot enforce the "no untyped values" rule, and null/undefined flows (e.g. `user?.x` versus `user.x`) are unchecked; the Vite React-TS template enables it by default.
- Verify: `grep -c '"strict": true' src/DMAuth.Client/tsconfig.app.json src/DMAuth.Client/tsconfig.node.json` prints 1 for each and `npm --prefix src/DMAuth.Client run build` passes.
- Evidence:
  ```
  18:    /* Linting */
  17:    /* Linting */
  $ grep -c '"strict"' src/DMAuth.Client/tsconfig.app.json src/DMAuth.Client/tsconfig.node.json
  src/DMAuth.Client/tsconfig.app.json:0
  src/DMAuth.Client/tsconfig.node.json:0
  ```

### FE-018 Modals have no dialog semantics or focus management
- Severity: convention drift
- Layer: frontend
- Location: `src/DMAuth.Client/src/pages/ClientDetailPage.tsx:34`; `src/DMAuth.Client/src/pages/ClientCreatePage.tsx:40`
- Rule: base frontend § Accessibility floor; external WAI-ARIA APG Dialog (Modal) Pattern https://www.w3.org/WAI/ARIA/apg/patterns/dialog-modal/
- Moves: code
- What: Render DeleteConfirmModal and SecretModal as a native `<dialog>` opened with `showModal()` (or `role="dialog"` + `aria-modal` + `aria-labelledby`), move focus into it on open, close on Escape, and return focus to the trigger.
- Why: Focus stays behind the overlay, so keyboard and screen-reader users can tab into the obscured page; the base floor requires focus to move predictably after an action that changes the screen.
- Verify: A component test opening each modal expects `getByRole('dialog')` and focus inside it.
- Evidence:
  ```
  34:    <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/40 px-4">
  40:    <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/40 px-4">
  ```

### FE-019 Form controls without an associated label
- Severity: convention drift
- Layer: frontend
- Location: `src/DMAuth.Client/src/pages/ClientDetailPage.tsx:214`; `src/DMAuth.Client/src/pages/ClientCreatePage.tsx:210`; `src/DMAuth.Client/src/pages/ProfilePage.tsx:122`
- Rule: base frontend § Accessibility floor
- Moves: code
- What: Give each redirect-URI input an `aria-label` (e.g. "Redirect URI 1") and the profile display-name input an `id` with a `<label htmlFor>`.
- Why: The base floor requires every form control to have a programmatically associated label; these inputs have only a nearby heading or placeholder.
- Verify: `getByLabelText` finds each of these inputs in component tests.
- Evidence:
  ```
  214:                  <input
  210:                  <input
  122:            <input
  ```

### FE-020 No resource hint for the cross-origin API fetched at startup
- Severity: convention drift
- Layer: frontend
- Location: `src/DMAuth.Client/src/App.tsx:22`
- Rule: discipline .sir/disciplines/react.md § Rendering
- Moves: code
- What: Call React DOM's `preconnect(import.meta.env.VITE_API_BASE_URL, { crossOrigin: 'use-credentials' })` at the top of `App` when the base URL is absolute.
- Why: The rule asks for `preconnect` for APIs fetched immediately; `AuthProvider` calls `/api/users/me` on the cross-origin API on every load and no hint exists.
- Verify: `grep -n 'preconnect' src/DMAuth.Client/src/App.tsx` prints a line.
- Evidence:
  ```
  22:export function App() {
  $ grep -rn 'preconnect\|prefetchDNS' src/DMAuth.Client/src src/DMAuth.Client/index.html | wc -l
  0
  ```

### FE-021 didInit one-time-initialization rule contradicts the query-based auth check
- Severity: convention drift
- Layer: frontend
- Location: `.sir/disciplines/react.md:136`; `src/DMAuth.Client/src/auth/AuthProvider.tsx:13`
- Rule: discipline .sir/disciplines/react.md § Effects and initialization
- Moves: discipline
- What: Restate react.md:136 so the module-level `didInit` flag applies to effect-driven initialization only, and auth checks owned by TanStack Query (`['auth', 'me']`) are named as the sanctioned form.
- Why: The code is the evidence: the auth check is a `useQuery`, deduplicated and cached by TanStack Query per the State management table, with no `useEffect` in any scoped source file, so the rule's "auth checks" example describes a pattern the project deliberately does not use.
- Verify: `sed -n '136p' .sir/disciplines/react.md` no longer lists auth checks as a `didInit` case.
- Evidence:
  ```
  136:- Guard app-wide one-time initialization (storage loading, auth checks) with a module-level flag (`let didInit = false`), not an unguarded `useEffect(..., [])`.
  13:  const { data: user = null, isLoading } = useQuery({
  ```

### FE-022 Loading spinners are unannounced and duplicated
- Severity: hygiene
- Layer: frontend
- Location: `src/DMAuth.Client/src/App.tsx:18`; `src/DMAuth.Client/src/auth/ProtectedRoute.tsx:16`; `src/DMAuth.Client/src/components/AppLayout.tsx:8`; `src/DMAuth.Client/src/pages/ConsentPage.tsx:40`
- Rule: external WCAG 2.2 Understanding 4.1.3 Status Messages https://www.w3.org/WAI/WCAG22/Understanding/status-messages.html
- Moves: n/a
- What: Extract one `Spinner` component in `src/components/` with `role="status"` and a visually hidden "Loading…" label, and use it at all four sites.
- Why: The spinner is a bare animated `div` copied four times; assistive technology gets no indication that content is loading.
- Verify: `grep -rn 'animate-spin' src/DMAuth.Client/src --include=*.tsx | grep -v test` prints one line.
- Evidence:
  ```
  18:    <div className="h-8 w-8 animate-spin rounded-full border-4 border-indigo-600 border-t-transparent" />
  16:        <div className="h-8 w-8 animate-spin rounded-full border-4 border-indigo-600 border-t-transparent" />
  8:      <div className="h-8 w-8 animate-spin rounded-full border-4 border-indigo-600 border-t-transparent" />
  40:        <div className="h-8 w-8 animate-spin rounded-full border-4 border-indigo-600 border-t-transparent" />
  ```

### FE-023 Unused types, API function and template assets
- Severity: hygiene
- Layer: frontend
- Location: `src/DMAuth.Client/src/types/oauth.ts:1`; `src/DMAuth.Client/src/api/userApi.ts:14`; `src/DMAuth.Client/src/assets/hero.png`; `src/DMAuth.Client/src/assets/react.svg`; `src/DMAuth.Client/src/assets/vite.svg`; `src/DMAuth.Client/public/icons.svg`
- Rule: external Knip: find unused files, dependencies and exports https://knip.dev/
- Moves: n/a
- What: Delete `types/oauth.ts` (whole file, never imported), `userApi.getProfile` (only its own test calls it; `authApi.me` serves the same endpoint) and the four unreferenced Vite-template asset files.
- Why: Dead code and template assets mislead readers about what the app uses and ship in the repository for nothing.
- Verify: `npx knip` (run in `src/DMAuth.Client`) reports no unused files or exports.
- Evidence:
  ```
  1:export interface AuthorizationRequest {
  14:  getProfile: () =>
  $ grep -rlE 'hero\.png|react\.svg|vite\.svg|icons\.svg|types/oauth' src/DMAuth.Client/src src/DMAuth.Client/index.html src/DMAuth.Client/tests | wc -l
  0
  $ grep -rn 'getProfile' src/DMAuth.Client/src --include='*.ts' --include='*.tsx' | grep -v '\.test\.'
  src/DMAuth.Client/src/api/userApi.ts:14:  getProfile: () =>
  ```

### FE-024 Vite template README and page title left in place
- Severity: hygiene
- Layer: frontend
- Location: `src/DMAuth.Client/README.md:1`; `src/DMAuth.Client/index.html:7`
- Rule: external Make a README https://www.makeareadme.com/; external WCAG 2.2 Understanding 2.4.2 Page Titled https://www.w3.org/WAI/WCAG22/Understanding/page-titled.html
- Moves: n/a
- What: Replace the whole template README with the client's setup, scripts and env (`VITE_API_BASE_URL`), and give the page a product title ("DM Auth"), ideally set per route.
- Why: The README describes the generic Vite template, not this client, and the tab title is the npm package name.
- Verify: `sed -n '1p' src/DMAuth.Client/README.md` and `grep -n '<title>' src/DMAuth.Client/index.html` name DM Auth.
- Evidence:
  ```
  1:# React + TypeScript + Vite
  7:    <title>dmauth-client</title>
  ```
