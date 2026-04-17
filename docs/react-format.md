# React Format — DMAuth.Client

> TypeScript/React code style and formatting rules for this project.
> For React performance best practices, see [react-guidelines.md](react-guidelines.md).
> For domain-level patterns (state management, API client, forms), see [frontend-conventions.md](frontend-conventions.md).

---

## Table of Contents

1. [File Naming](#1-file-naming)
2. [Component Style](#2-component-style)
3. [Exports](#3-exports)
4. [Props](#4-props)
5. [Styling](#5-styling)
6. [Import Organization](#6-import-organization)
7. [API Client Module Structure](#7-api-client-module-structure)
8. [Form Validation Patterns](#8-form-validation-patterns)
9. [Testing Patterns](#9-testing-patterns)

---

## 1. File Naming

- Component files use **PascalCase** matching the component name: `ProfilePage.tsx`, `AppLayout.tsx`
- Non-component TypeScript files use **camelCase**: `authApi.ts`, `renderWithProviders.tsx`
- Test files mirror the file they test with a `.test` suffix: `ProfilePage.test.tsx`, `client.test.ts`
- One component (or one logical unit) per file — do not co-locate unrelated exports

---

## 2. Component Style

- **Functional components only** — no class components
- Component function names use **PascalCase**

```tsx
// Correct
function ProfilePage() { ... }

// Incorrect — arrow function assigned to const is acceptable but function declarations are preferred
const ProfilePage = () => { ... }
```

> Small, file-local sub-components may use arrow function syntax. Top-level exported components use function declarations.

---

## 3. Exports

- **Named exports only** — never use default exports

```tsx
// Correct
export function ProfilePage() { ... }
export function useAuth() { ... }

// Incorrect
export default function ProfilePage() { ... }
```

This makes imports predictable, keeps refactoring straightforward, and avoids name inconsistencies between the export site and import site.

---

## 4. Props

- Define props as an **inline type or interface in the same file** as the component
- Name the interface `{ComponentName}Props`
- Do not export props types unless they are consumed by another component

```tsx
interface UserCardProps {
  userId: string
  displayName: string
  email: string
  onEdit?: () => void
}

export function UserCard({ userId, displayName, email, onEdit }: UserCardProps) {
  ...
}
```

---

## 5. Styling

- **Tailwind CSS utility classes only** — no CSS Modules, no styled-components, no `emotion`
- No inline `style` props — use Tailwind classes or extend the Tailwind config for project-specific values
- Do not use arbitrary Tailwind values (e.g., `w-[137px]`) for spacing or colors that have a Tailwind equivalent
- Dark mode is not currently supported — do not add `dark:` variants

---

## 6. Import Organization

Imports are organized in the following order, with a blank line separating each group:

1. **React and React ecosystem** (`react`, `react-dom`, `react-router-dom`)
2. **Third-party libraries** (`@tanstack/react-query`, `axios`, `zod`, `react-hook-form`)
3. **Internal modules** — in order: `types/`, `api/`, `auth/`, `components/`, `pages/`
4. **Relative imports** (`./ ../`)

```tsx
import { useState } from 'react'
import { useNavigate } from 'react-router-dom'

import { useQuery, useMutation } from '@tanstack/react-query'
import { z } from 'zod'

import type { OAuthClient } from '../types/client'
import { clientApi } from '../api/clientApi'
import { useAuth } from '../auth/useAuth'
```

> Enforcing this order with an ESLint rule (`import/order`) is recommended but not currently configured — follow the convention manually.

---

## 7. API Client Module Structure

Each resource has a named export object with typed function signatures. All functions call through the shared `apiClient` instance.

```ts
// src/api/userApi.ts
import type { User, UpdateProfileRequest, ChangePasswordRequest } from '../types/user'

import { apiClient } from './client'

export const userApi = {
  me: () =>
    apiClient.get<User>('/api/users/me'),

  updateProfile: (data: UpdateProfileRequest) =>
    apiClient.patch<void>('/api/users/me', data),

  changePassword: (data: ChangePasswordRequest) =>
    apiClient.post<void>('/api/users/me/change-password', data),
}
```

### TanStack Query Integration

```tsx
// useQuery — read data
const { data: user, isLoading } = useQuery({
  queryKey: ['users', 'me'],
  queryFn: () => userApi.me().then(response => response.data),
})

// useMutation — write data, invalidate on success
const queryClient = useQueryClient()

const mutation = useMutation({
  mutationFn: (data: UpdateProfileRequest) => userApi.updateProfile(data),
  onSuccess: () => {
    queryClient.invalidateQueries({ queryKey: ['users', 'me'] })
  },
})
```

---

## 8. Form Validation Patterns

Zod schemas define validation rules. `zodResolver` wires the schema into react-hook-form. Field errors render below their input; API errors render in a root alert banner.

```tsx
import { useForm } from 'react-hook-form'
import { z } from 'zod'
import { zodResolver } from '@hookform/resolvers/zod'

import { extractApiError } from '../api/client'
import { authApi } from '../api/authApi'

const schema = z.object({
  email: z.string().email('Enter a valid email.'),
  password: z.string().min(8, 'Password must be at least 8 characters.'),
})

type FormValues = z.infer<typeof schema>

export function LoginPage() {
  const {
    register,
    handleSubmit,
    formState: { errors, isSubmitting },
    setError,
  } = useForm<FormValues>({ resolver: zodResolver(schema) })

  const onSubmit = async (data: FormValues) => {
    try {
      await authApi.login(data)
    } catch (err) {
      setError('root', { message: extractApiError(err) })
    }
  }

  return (
    <form onSubmit={handleSubmit(onSubmit)}>
      <div>
        <input {...register('email')} />
        {errors.email && <p>{errors.email.message}</p>}
      </div>
      <div>
        <input type="password" {...register('password')} />
        {errors.password && <p>{errors.password.message}</p>}
      </div>
      {errors.root && <div role="alert">{errors.root.message}</div>}
      <button type="submit" disabled={isSubmitting}>Sign in</button>
    </form>
  )
}
```

---

## 9. Testing Patterns

### Component Tests (Vitest)

Mock API modules at the top of the file with `vi.mock`. Use `renderWithProviders` for all component renders. Pass a `user` option to skip auth loading.

```tsx
import { vi, describe, it, expect, beforeEach } from 'vitest'
import { screen } from '@testing-library/react'
import userEvent from '@testing-library/user-event'

import { clientApi } from '../api/clientApi'
import { renderWithProviders } from '../test/renderWithProviders'
import { ClientsPage } from './ClientsPage'

vi.mock('../api/clientApi')

const mockClientApi = vi.mocked(clientApi)

describe('ClientsPage', () => {
  beforeEach(() => {
    mockClientApi.list.mockResolvedValue({ data: [] })
  })

  it('renders the empty state when no clients exist', async () => {
    renderWithProviders(<ClientsPage />, {
      user: { id: '1', email: 'test@example.com', username: 'testuser' },
    })

    expect(await screen.findByText(/no clients/i)).toBeInTheDocument()
  })
})
```

**Query selection rules:**
- `findBy*` — async; use for elements that appear after data loads
- `getBy*` — synchronous; use for elements that are immediately present
- Prefer role-based queries: `getByRole`, `getByLabelText`
- Avoid test-id selectors unless no semantic query fits

### E2E Tests (Playwright)

Use the `freshUser` fixture for all tests. Scope locators to `page.locator('main')` when the sidebar contains duplicate text. Use `{ exact: true }` or anchored regex to avoid strict-mode violations.

```ts
import { test, expect } from '../fixtures/auth'

test('creates a new client', async ({ page, freshUser }) => {
  await page.getByRole('link', { name: 'Clients' }).click()

  const main = page.locator('main')
  await main.getByRole('button', { name: 'New Client' }).click()
  await main.getByLabel('Client Name').fill('My App')
  await main.getByRole('button', { name: /^Create$/i }).click()

  await expect(main.getByText('My App', { exact: true })).toBeVisible()
})
```
