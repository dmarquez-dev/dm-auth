# Frontend Conventions — DMAuth.Client

> Domain-level patterns for the DMAuth React SPA.
> For React performance best practices, see [react-guidelines.md](react-guidelines.md).
> For TypeScript/React code style and formatting, see [react-format.md](react-format.md).
> For Vitest unit/component testing conventions, see [vitest-guidelines.md](vitest-guidelines.md).
> For Playwright e2e testing conventions, see [playwright-guidelines.md](playwright-guidelines.md).

**Project context:** DMAuth.Client is a pure client-side SPA. There is no SSR and no server components.

---

## Table of Contents

1. [Project Structure](#1-project-structure)
2. [State Management](#2-state-management)
3. [API Client](#3-api-client)
4. [Form Validation](#4-form-validation)
5. [Testing Conventions](#5-testing-conventions)

---

## 1. Project Structure

```
src/DMAuth.Client/
  src/
    api/              # HTTP client modules (one per resource)
    auth/             # Auth provider, route guard, and auth hook
    components/       # Shared/reusable components
    pages/            # Page components (one per route)
    test/             # Unit/component test helpers
    types/            # Shared type definitions
    App               # Root component with router and providers
    main              # Application entry point
  tests/
    e2e/              # End-to-end tests
      fixtures/       # Custom test fixtures
      *.spec          # Spec files organized by feature
  index.html
  vite.config
  vitest.config
  playwright.config
  tailwind.config
  tsconfig.json
  package.json
```

---

## 2. State Management

| State type | Tool |
|------------|------|
| Server state (API data) | TanStack Query |
| Auth state | Shared context via auth provider and hook |
| Form state | react-hook-form + zod schemas |
| Local UI state | Component-local state |

**Server state rules:**
- All API calls go through the server state library — no ad-hoc side effects with raw fetch or HTTP calls
- Use consistent query key conventions: `['resource', id]` (e.g., `['clients', clientId]`)
- Invalidate related queries on mutation success

**Auth state rules:**
- Consume auth state via the auth accessor — never import the auth context directly
- The route guard component protects all authenticated routes; page components do not check auth themselves

---

## 3. API Client

- One HTTP module per resource: `authApi`, `userApi`, `clientApi`
- All modules use the shared HTTP client instance configured with the base URL and credentials
- Type all request and response payloads — no untyped values
- Central 401 handling lives in the HTTP client response interceptor:
  - Redirects to the login page with a return URL on session expiry
  - Does **not** redirect for the session check endpoint or the change-password endpoint (business-meaning 401)

For implementation shapes, see [react-format.md § 7](react-format.md#7-api-client-module-structure).

---

## 4. Form Validation

- Schemas define all validation rules — one schema per form
- The schema resolver connects validation schemas to the form library
- Display field-level error messages below the relevant input
- Display API-level (root) errors in an alert banner above the submit button

For implementation shapes, see [react-format.md § 8](react-format.md#8-form-validation-patterns).

---

## 5. Testing Conventions

### Unit / Component Tests

See [vitest-guidelines.md](vitest-guidelines.md) for the full reference. Key conventions:

- Use the shared render helper for all component tests — never call the base render function directly
- Mock API modules at the top of each test file
- Inject a pre-authenticated user via the render helper options to skip auth loading states
- Use async queries for elements that appear after data loads; use synchronous queries for immediately-present elements
- Prefer role-based queries over test-id selectors

### E2E Tests

See [playwright-guidelines.md](playwright-guidelines.md) for the full reference. Key conventions:

- All e2e tests use the isolated user fixture — never share user state between tests
- Navigate within tests using sidebar nav links (client-side routing), not full-page navigation — full navigation discards the query cache and can cause flakiness
- Use exact or anchored patterns in locators to avoid strict-mode violations from substring matches
- Scope locators to the main content area when the same text appears in both the sidebar and the main content

For implementation shapes, see [react-format.md § 9](react-format.md#9-testing-patterns).
