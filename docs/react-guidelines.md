# React Guidelines — DMAuth.Client

> This document is the canonical React conventions reference for DMAuth.Client. All React work should follow these guidelines.

**Project context:** DMAuth Dashboard is a pure client-side SPA (Vite + React + TypeScript). There is no SSR, no Next.js, and no React Server Components. Rules that are exclusively Next.js or RSC-specific (server actions, `server-*` APIs, Next.js `dynamic()`, `next/script`, output file tracing) are **not applicable** and are omitted or noted as N/A below.

---

## Table of Contents

1. [Eliminating Waterfalls (async)](#1-eliminating-waterfalls-async) — CRITICAL
2. [Bundle Size Optimization (bundle)](#2-bundle-size-optimization-bundle) — CRITICAL
3. [Client-Side Data Fetching (client)](#3-client-side-data-fetching-client) — MEDIUM-HIGH
4. [Re-render Optimization (rerender)](#4-re-render-optimization-rerender) — MEDIUM
5. [Rendering Performance (rendering)](#5-rendering-performance-rendering) — MEDIUM
6. [JavaScript Performance (js)](#6-javascript-performance-js) — LOW-MEDIUM
7. [Advanced Patterns (advanced)](#7-advanced-patterns-advanced) — LOW

---

## 1. Eliminating Waterfalls (async)

> **Impact: CRITICAL** — Each sequential await adds full network latency. Eliminating waterfalls yields the largest performance gains.

### 1.1 Parallel Async Operations

**Rule:** Use `Promise.all()` for independent async calls instead of sequential `await`.

| Incorrect | Correct |
|---|---|
| Sequential awaits (3 round trips) | `Promise.all()` (1 round trip) |

```tsx
// Incorrect — 3 sequential round trips
const user = await fetchUser()
const config = await fetchConfig()
const notifications = await fetchNotifications()

// Correct — all start immediately
const [user, config, notifications] = await Promise.all([
  fetchUser(),
  fetchConfig(),
  fetchNotifications(),
])
```

### 1.2 Dependency-Based Parallelization

**Rule:** For operations with partial dependencies, start independent promises immediately and chain dependent ones. Don't serialize operations that don't need to wait on each other.

```tsx
// Incorrect — profile waits for config unnecessarily
const [user, config] = await Promise.all([fetchUser(), fetchConfig()])
const profile = await fetchProfile(user.id)

// Correct — config and profile run in parallel
const userPromise = fetchUser()
const profilePromise = userPromise.then(user => fetchProfile(user.id))

const [user, config, profile] = await Promise.all([
  userPromise,
  fetchConfig(),
  profilePromise,
])
```

For complex dependency chains, consider `better-all` which automatically maximizes parallelism.

### 1.3 Defer Await Until Needed

**Rule:** Move `await` into the branch where the value is actually used. Don't block code paths that don't need the result.

```tsx
// Incorrect — always fetches even when skipping
async function handleRequest(userId: string, skip: boolean) {
  const userData = await fetchUserData(userId)
  if (skip) return { skipped: true }
  return processUserData(userData)
}

// Correct — fetch only when needed
async function handleRequest(userId: string, skip: boolean) {
  if (skip) return { skipped: true }
  const userData = await fetchUserData(userId)
  return processUserData(userData)
}
```

### 1.4 Check Cheap Conditions Before Async Calls

**Rule:** When a branch needs both a cheap synchronous condition and an async value (e.g., a feature flag), evaluate the cheap condition first to avoid unnecessary async work.

```tsx
// Incorrect — always awaits the flag even when someCondition is false
const someFlag = await getFlag()
if (someFlag && someCondition) { /* ... */ }

// Correct — skip the async call when condition is already false
if (someCondition) {
  const someFlag = await getFlag()
  if (someFlag) { /* ... */ }
}
```

### 1.5 Suspense Boundaries for Parallel Data + UI

**Rule:** Use `<Suspense>` to render page chrome immediately while async data loads in a nested component. This prevents the entire page from blocking on one data fetch.

```tsx
// Correct — layout renders immediately, only the data section waits
function Page() {
  return (
    <div>
      <Sidebar />
      <Header />
      <Suspense fallback={<Skeleton />}>
        <DataDisplay />
      </Suspense>
      <Footer />
    </div>
  )
}

async function DataDisplay() {
  const data = await fetchData() // only blocks this component
  return <div>{data.content}</div>
}
```

**To share a promise across siblings (avoids double fetch):**

```tsx
function Page() {
  const dataPromise = fetchData() // start immediately, don't await

  return (
    <Suspense fallback={<Skeleton />}>
      <DataDisplay dataPromise={dataPromise} />
      <DataSummary dataPromise={dataPromise} />
    </Suspense>
  )
}

function DataDisplay({ dataPromise }: { dataPromise: Promise<Data> }) {
  const data = use(dataPromise) // React's use() hook unwraps the promise
  return <div>{data.content}</div>
}
```

**Trade-off:** Faster initial paint vs. potential layout shift. Avoid Suspense for critical above-the-fold content or when layout shift is unacceptable.

---

## 2. Bundle Size Optimization (bundle)

> **Impact: CRITICAL** — Reducing initial bundle size improves Time to Interactive (TTI) and Largest Contentful Paint (LCP).

### 2.1 Prefer Statically Analyzable Import Paths

**Rule:** Use explicit import maps with literal paths so Vite/Rollup can statically analyze and tree-shake your bundle. Dynamic path composition prevents the bundler from knowing which modules to include.

```tsx
// Incorrect — bundler cannot determine what is imported
const PAGE_MODULES = {
  home: './pages/home',
  settings: './pages/settings',
} as const
const Page = await import(PAGE_MODULES[pageName])

// Correct — explicit import functions, bundler knows every possibility
const PAGE_MODULES = {
  home: () => import('./pages/home'),
  settings: () => import('./pages/settings'),
} as const
const Page = await PAGE_MODULES[pageName]()
```

### 2.2 Avoid Barrel File Imports

**Rule:** Import directly from source paths rather than barrel re-export files. Popular libraries (icon packs, component libraries) can have 10,000+ re-exports — importing the barrel pulls them all in.

- **Impact:** 15–70% faster dev startup, 28% faster builds, 40% faster cold starts

```tsx
// Incorrect — pulls in entire library barrel
import { Button, Input, Dialog } from '@mui/material'

// Correct — import from specific module paths
import Button from '@mui/material/Button'
import Input from '@mui/material/Input'
import Dialog from '@mui/material/Dialog'
```

> For Vite projects, configure `optimizeDeps` or use the `vite-plugin-import-optimizer` to handle this automatically while preserving TypeScript types.

### 2.3 Dynamic Imports for Heavy Components

**Rule:** Lazy-load large components that are not needed on initial render using `React.lazy()` and `import()`.

```tsx
import { lazy, Suspense } from 'react'

// Correct — Monaco editor (~300KB) loads only when CodePanel mounts
const MonacoEditor = lazy(() => import('./MonacoEditor'))

function CodePanel() {
  return (
    <Suspense fallback={<div>Loading editor...</div>}>
      <MonacoEditor />
    </Suspense>
  )
}
```

### 2.4 Conditional Module Loading

**Rule:** Load large data or feature modules only when a feature is actually activated, not eagerly on startup.

```tsx
function AnimationPlayer({ enabled, setEnabled }: Props) {
  const [frames, setFrames] = useState<Frame[] | null>(null)

  useEffect(() => {
    if (enabled && !frames) {
      import('./animation-frames.js')
        .then(mod => setFrames(mod.frames))
        .catch(() => setEnabled(false))
    }
  }, [enabled, frames, setEnabled])

  if (!frames) return <Skeleton />
  return <Canvas frames={frames} />
}
```

### 2.5 Preload Based on User Intent

**Rule:** Preload heavy bundles when the user signals intent (hover, focus) — before they actually click — to reduce perceived latency.

```tsx
function EditorButton({ onClick }: { onClick: () => void }) {
  const preload = () => {
    void import('./monaco-editor')
  }

  return (
    <button onMouseEnter={preload} onFocus={preload} onClick={onClick}>
      Open Editor
    </button>
  )
}
```

### 2.6 Defer Non-Critical Third-Party Libraries

**Rule:** Analytics, logging, and error-tracking scripts should load after the app is interactive. In a Vite SPA, use dynamic `import()` inside a `useEffect` to defer them.

```tsx
// Correct — analytics loads after initial render
useEffect(() => {
  import('@vercel/analytics').then(({ inject }) => inject())
}, [])
```

> Note: `next/dynamic` with `ssr: false` is Next.js-specific (N/A). In this SPA, use `React.lazy()` + `Suspense` or deferred `useEffect` imports instead.

---

## 3. Client-Side Data Fetching (client)

> **Impact: MEDIUM-HIGH** — Automatic deduplication and efficient patterns reduce redundant network requests.

### 3.1 Use SWR for Automatic Deduplication

**Rule:** Use SWR (or React Query) for data fetching. Multiple component instances sharing the same key will share one request, with automatic caching and revalidation.

```tsx
// Incorrect — no deduplication; multiple instances each fetch independently
function UserList() {
  const [users, setUsers] = useState([])
  useEffect(() => {
    fetch('/api/users').then(r => r.json()).then(setUsers)
  }, [])
}

// Correct — multiple instances share one request
import useSWR from 'swr'

function UserList() {
  const { data: users, isLoading } = useSWR('/api/users', fetcher)
  if (isLoading) return <Spinner />
  return <ul>{users?.map(renderUser)}</ul>
}
```

**For immutable/static data:**
```tsx
import useSWRImmutable from 'swr/immutable'

function Config() {
  const { data } = useSWRImmutable('/api/config', fetcher)
}
```

**For mutations:**
```tsx
import { useSWRMutation } from 'swr/mutation'

function UpdateButton() {
  const { trigger } = useSWRMutation('/api/user', updateUser)
  return <button onClick={() => trigger()}>Update</button>
}
```

Reference: [https://swr.vercel.app](https://swr.vercel.app)

### 3.2 Use Passive Event Listeners

**Rule:** Add `{ passive: true }` to `touchstart`, `touchmove`, and `wheel` event listeners. This lets the browser scroll immediately without waiting for `preventDefault()` checks.

```tsx
// Incorrect — browser waits before scrolling
useEffect(() => {
  document.addEventListener('touchstart', handleTouch)
  document.addEventListener('wheel', handleWheel)
  return () => {
    document.removeEventListener('touchstart', handleTouch)
    document.removeEventListener('wheel', handleWheel)
  }
}, [])

// Correct — browser scrolls immediately
useEffect(() => {
  document.addEventListener('touchstart', handleTouch, { passive: true })
  document.addEventListener('wheel', handleWheel, { passive: true })
  return () => {
    document.removeEventListener('touchstart', handleTouch)
    document.removeEventListener('wheel', handleWheel)
  }
}, [])
```

Only omit `passive: true` when you need to call `preventDefault()` (e.g., custom zoom/gesture controls).

### 3.3 Deduplicate Global Event Listeners

**Rule:** Use `useSWRSubscription()` or a module-level subscriber pattern to share global event listeners across multiple component instances. N instances should register 1 listener, not N.

```tsx
// Incorrect — each hook usage adds a new window listener
function useKeyboardShortcut(key: string, callback: () => void) {
  useEffect(() => {
    const handler = (e: KeyboardEvent) => {
      if (e.metaKey && e.key === key) callback()
    }
    window.addEventListener('keydown', handler)
    return () => window.removeEventListener('keydown', handler)
  }, [key, callback])
}
```

Use `useSWRSubscription` to ensure only one listener is registered globally regardless of how many components call the hook.

### 3.4 Version and Minimize localStorage Data

**Rule:** When persisting state to `localStorage`, use versioned keys, store only essential fields (not full server responses), and always wrap access in `try/catch` (throws in Safari/Firefox private mode and when quota is exceeded).

```tsx
const STORAGE_KEY = 'userPrefs:v2'

function savePrefs(prefs: UserPrefs) {
  try {
    localStorage.setItem(STORAGE_KEY, JSON.stringify({
      theme: prefs.theme,
      notifications: prefs.notifications,
      // Never persist tokens or PII here
    }))
  } catch {
    // Ignore storage errors (private browsing, quota exceeded)
  }
}
```

---

## 4. Re-render Optimization (rerender)

> **Impact: MEDIUM** — Reducing unnecessary re-renders minimizes wasted computation and improves UI responsiveness.

### 4.1 Never Define Components Inside Components

**Rule:** Do not define a component function inside another component's render. React sees a new component type every render and fully remounts it, destroying all state and DOM.

```tsx
// Incorrect — Avatar is a new type on every render; state is destroyed
function UserProfile({ user, theme }) {
  const Avatar = () => <img src={user.avatarUrl} className={theme} />
  return <Avatar />
}

// Correct — define outside, pass props
function Avatar({ src, theme }: { src: string; theme: string }) {
  return <img src={src} className={theme} />
}

function UserProfile({ user, theme }) {
  return <Avatar src={user.avatarUrl} theme={theme} />
}
```

**Symptoms of this bug:** input fields lose focus on every keystroke, animations restart, effects re-run on every parent render, scroll position resets.

### 4.2 Calculate Derived State During Render (No Effect for Derived Values)

**Rule:** If a value can be computed from existing props or state, compute it inline during render. Do not store it in state or update it via `useEffect`.

```tsx
// Incorrect — extra render cycle and state drift risk
function Form() {
  const [firstName, setFirstName] = useState('First')
  const [lastName, setLastName] = useState('Last')
  const [fullName, setFullName] = useState('')

  useEffect(() => {
    setFullName(firstName + ' ' + lastName)
  }, [firstName, lastName])

  return <p>{fullName}</p>
}

// Correct — derived inline, zero extra renders
function Form() {
  const [firstName, setFirstName] = useState('First')
  const [lastName, setLastName] = useState('Last')
  const fullName = firstName + ' ' + lastName

  return <p>{fullName}</p>
}
```

### 4.3 Subscribe to Derived Boolean State

**Rule:** Subscribe to derived boolean values (e.g., `isMobile`) rather than continuous numeric values (e.g., `windowWidth`). This limits re-renders to actual threshold crossings instead of every pixel change.

```tsx
// Incorrect — re-renders on every pixel of window resize
function Sidebar() {
  const width = useWindowWidth()
  const isMobile = width < 768
  return <nav className={isMobile ? 'mobile' : 'desktop'} />
}

// Correct — re-renders only when breakpoint is crossed
function Sidebar() {
  const isMobile = useMediaQuery('(max-width: 767px)')
  return <nav className={isMobile ? 'mobile' : 'desktop'} />
}
```

### 4.4 Put Interaction Logic in Event Handlers

**Rule:** If a side effect is triggered by a user action (click, submit), run it in the event handler — not in a `useEffect` that watches a state flag. State + effect patterns cause effects to re-run on unrelated changes.

```tsx
// Incorrect — useEffect re-runs whenever theme changes too
function Form() {
  const [submitted, setSubmitted] = useState(false)
  const theme = useContext(ThemeContext)

  useEffect(() => {
    if (submitted) {
      post('/api/register')
      showToast('Registered', theme)
    }
  }, [submitted, theme])

  return <button onClick={() => setSubmitted(true)}>Submit</button>
}

// Correct — side effect runs exactly once, on click
function Form() {
  const theme = useContext(ThemeContext)

  function handleSubmit() {
    post('/api/register')
    showToast('Registered', theme)
  }

  return <button onClick={handleSubmit}>Submit</button>
}
```

### 4.5 Narrow Effect Dependencies

**Rule:** Pass the primitive value your effect actually uses, not the whole object. This limits re-runs to changes in that specific field.

```tsx
// Incorrect — re-runs when any user field changes
useEffect(() => {
  console.log(user.id)
}, [user])

// Correct — re-runs only when id changes
useEffect(() => {
  console.log(user.id)
}, [user.id])

// Also correct — compute derived boolean outside effect
const isMobile = width < 768
useEffect(() => {
  if (isMobile) enableMobileMode()
}, [isMobile]) // runs only on boolean transition
```

### 4.6 Use Functional setState Updates

**Rule:** When new state depends on the current state value, use the functional form of `setState`. This prevents stale closure bugs and keeps callbacks stable.

```tsx
// Incorrect — stale closure if items changes; requires items in deps
const addItems = useCallback((newItems: Item[]) => {
  setItems([...items, ...newItems])
}, [items])

// Correct — always operates on latest state, callback stays stable
const addItems = useCallback((newItems: Item[]) => {
  setItems(curr => [...curr, ...newItems])
}, [])
```

### 4.7 Lazy State Initialization

**Rule:** Pass a function (not a value) to `useState` for expensive initial computations. Without the function form, the initializer runs on every render even though React only uses it once.

```tsx
// Incorrect — parses localStorage on every render
const [prefs, setPrefs] = useState(JSON.parse(localStorage.getItem('prefs') ?? '{}'))

// Correct — runs only on initial mount
const [prefs, setPrefs] = useState(() => JSON.parse(localStorage.getItem('prefs') ?? '{}'))
```

Apply lazy init for: reading from localStorage/sessionStorage, building data structures (Maps, indexes), DOM reads, heavy transformations.

### 4.8 Use useRef for Transient / High-Frequency Values

**Rule:** Use `useRef` for values that change frequently but don't need to trigger re-renders (mouse position, scroll offset, timers, transient flags). Updating a ref does not cause a re-render.

```tsx
// Incorrect — re-renders on every mouse move
function DragLayer() {
  const [mousePos, setMousePos] = useState({ x: 0, y: 0 })
  // ...
}

// Correct — updates the DOM directly, no re-render
function DragLayer() {
  const nodeRef = useRef<HTMLDivElement>(null)
  const posRef = useRef({ x: 0, y: 0 })

  useEffect(() => {
    const handler = (e: MouseEvent) => {
      posRef.current = { x: e.clientX, y: e.clientY }
      if (nodeRef.current) {
        nodeRef.current.style.transform = `translate(${e.clientX}px, ${e.clientY}px)`
      }
    }
    window.addEventListener('mousemove', handler)
    return () => window.removeEventListener('mousemove', handler)
  }, [])

  return <div ref={nodeRef} />
}
```

### 4.9 Defer State Reads to Usage Point

**Rule:** Don't subscribe to dynamic state (search params, localStorage) if you only read it inside a callback. Read it on demand instead to avoid unnecessary re-renders.

```tsx
// Incorrect — re-renders whenever searchParams changes
function ShareButton({ chatId }: { chatId: string }) {
  const searchParams = useSearchParams()

  const handleShare = () => {
    const ref = searchParams.get('ref')
    shareChat(chatId, { ref })
  }

  return <button onClick={handleShare}>Share</button>
}

// Correct — reads on demand, no subscription
function ShareButton({ chatId }: { chatId: string }) {
  const handleShare = () => {
    const params = new URLSearchParams(window.location.search)
    const ref = params.get('ref')
    shareChat(chatId, { ref })
  }

  return <button onClick={handleShare}>Share</button>
}
```

### 4.10 Extract to Memoized Components

**Rule:** Extract expensive computations into separate `memo()`-wrapped components. This enables early returns before computation and isolates re-renders.

```tsx
// Incorrect — avatar is computed even when loading=true
function Profile({ user, loading }: Props) {
  const avatar = useMemo(() => {
    const id = computeAvatarId(user)
    return <Avatar id={id} />
  }, [user])

  if (loading) return <Skeleton />
  return <div>{avatar}</div>
}

// Correct — UserAvatar only renders (and computes) when Profile needs it
const UserAvatar = memo(function UserAvatar({ user }: { user: User }) {
  const id = useMemo(() => computeAvatarId(user), [user])
  return <Avatar id={id} />
})

function Profile({ user, loading }: Props) {
  if (loading) return <Skeleton />
  return <div><UserAvatar user={user} /></div>
}
```

> If React Compiler is enabled in this project, manual `memo()` and `useMemo()` wrappers become unnecessary — the compiler handles this automatically.

### 4.11 Default Non-Primitive Props in Memoized Components

**Rule:** When a memoized component has a non-primitive optional prop (object, array, function) with a default value, extract that default to a module-level constant. Inline defaults create new instances each render and break `memo()`.

```tsx
// Incorrect — new () => {} on every render breaks memo
const UserAvatar = memo(function UserAvatar({ onClick = () => {} }: { onClick?: () => void }) {
  // ...
})

// Correct — stable reference
const NOOP = () => {}

const UserAvatar = memo(function UserAvatar({ onClick = NOOP }: { onClick?: () => void }) {
  // ...
})
```

### 4.12 Don't Wrap Simple Primitive Expressions in useMemo

**Rule:** Don't wrap simple expressions (a few operators, primitive result) in `useMemo`. The hook overhead costs more than the expression itself.

```tsx
// Incorrect — useMemo overhead > boolean expression cost
const isLoading = useMemo(
  () => user.isLoading || notifications.isLoading,
  [user.isLoading, notifications.isLoading]
)

// Correct — just compute it
const isLoading = user.isLoading || notifications.isLoading
```

Use `useMemo` for: expensive transformations, large array operations, reference stability for child components.

### 4.13 Split Combined Hooks

**Rule:** Split `useMemo` or `useEffect` hooks that do multiple independent things with different dependencies. A combined hook reruns all tasks when any dependency changes.

```tsx
// Incorrect — sort recalculates when category changes and vice versa
const displayProducts = useMemo(() => {
  return products
    .filter(p => p.category === category)
    .sort((a, b) => a[sortKey] - b[sortKey])
}, [products, category, sortKey])

// Correct — filter and sort are independent memos
const filteredProducts = useMemo(
  () => products.filter(p => p.category === category),
  [products, category]
)
const sortedProducts = useMemo(
  () => filteredProducts.toSorted((a, b) => a[sortKey] - b[sortKey]),
  [filteredProducts, sortKey]
)
```

### 4.14 Use useDeferredValue for Expensive Derived Renders

**Rule:** When user input triggers expensive computations (large list filtering, chart rendering), use `useDeferredValue` to keep the input responsive. The deferred value lags behind while React prioritizes input updates.

```tsx
function Search({ items }: { items: Item[] }) {
  const [query, setQuery] = useState('')
  const deferredQuery = useDeferredValue(query)

  const filtered = useMemo(
    () => items.filter(item => fuzzyMatch(item, deferredQuery)),
    [items, deferredQuery]
  )

  const isStale = query !== deferredQuery

  return (
    <>
      <input value={query} onChange={e => setQuery(e.target.value)} />
      <div style={{ opacity: isStale ? 0.7 : 1 }}>
        <ResultsList results={filtered} />
      </div>
    </>
  )
}
```

> Always wrap the expensive computation in `useMemo` keyed to the deferred value, otherwise it still runs eagerly on every render.

### 4.15 Use Transitions for Non-Urgent Updates

**Rule:** Wrap non-urgent state updates (e.g., scroll tracking, background processing) in `startTransition` to keep the UI responsive.

```tsx
import { startTransition } from 'react'

function ScrollTracker() {
  const [scrollY, setScrollY] = useState(0)

  useEffect(() => {
    const handler = () => {
      startTransition(() => setScrollY(window.scrollY))
    }
    window.addEventListener('scroll', handler, { passive: true })
    return () => window.removeEventListener('scroll', handler)
  }, [])
}
```

### 4.16 Use useTransition Over Manual Loading States

**Rule:** Prefer `useTransition` to manual `isLoading` state for async operations. It provides built-in `isPending`, handles errors correctly, and supports interruption.

```tsx
// Incorrect — manual loading flag, prone to not resetting on error
function SearchResults() {
  const [isLoading, setIsLoading] = useState(false)

  const handleSearch = async (value: string) => {
    setIsLoading(true)
    const data = await fetchResults(value)
    setResults(data)
    setIsLoading(false) // won't run if fetchResults throws
  }
}

// Correct — isPending resets automatically even on error
import { useTransition, useState } from 'react'

function SearchResults() {
  const [results, setResults] = useState([])
  const [isPending, startTransition] = useTransition()

  const handleSearch = (value: string) => {
    startTransition(async () => {
      const data = await fetchResults(value)
      setResults(data)
    })
  }

  return (
    <>
      <input onChange={e => handleSearch(e.target.value)} />
      {isPending && <Spinner />}
      <ResultsList results={results} />
    </>
  )
}
```

---

## 5. Rendering Performance (rendering)

> **Impact: MEDIUM** — Optimizing the rendering process reduces browser work.

### 5.1 Use Activity Component for Show/Hide

**Rule:** Use React's `<Activity>` component (experimental) to toggle visibility of expensive components. Unlike unmounting, `Activity` preserves the component's state and DOM, avoiding expensive remount work.

```tsx
import { Activity } from 'react'

function Dropdown({ isOpen }: { isOpen: boolean }) {
  return (
    <Activity mode={isOpen ? 'visible' : 'hidden'}>
      <ExpensiveMenu />
    </Activity>
  )
}
```

### 5.2 Use Explicit Conditional Rendering

**Rule:** Use ternary operators instead of `&&` for conditional rendering when the condition might be `0` or `NaN`. These render as visible text with `&&`.

```tsx
// Incorrect — renders "0" when count is 0
{count && <span className="badge">{count}</span>}

// Correct — renders nothing when count is 0
{count > 0 ? <span className="badge">{count}</span> : null}
```

### 5.3 CSS content-visibility for Long Lists

**Rule:** Apply `content-visibility: auto` to list items to defer off-screen rendering. For 1000 items, the browser skips layout and paint for ~990 off-screen items — up to 10x faster initial render.

```css
/* styles.css */
.message-item {
  content-visibility: auto;
  contain-intrinsic-size: 0 80px; /* estimated item height */
}
```

```tsx
function MessageList({ messages }: { messages: Message[] }) {
  return (
    <div className="overflow-y-auto h-screen">
      {messages.map(msg => (
        <div key={msg.id} className="message-item">
          <Avatar user={msg.author} />
          <div>{msg.content}</div>
        </div>
      ))}
    </div>
  )
}
```

### 5.4 Hoist Static JSX Elements

**Rule:** Extract static JSX (elements with no props that change) to module-level constants to avoid recreating them on every render.

```tsx
// Incorrect — new JSX object created every render
function Container({ loading }: { loading: boolean }) {
  return <div>{loading && <div className="animate-pulse h-20 bg-gray-200" />}</div>
}

// Correct — reuses same element reference
const loadingSkeleton = <div className="animate-pulse h-20 bg-gray-200" />

function Container({ loading }: { loading: boolean }) {
  return <div>{loading && loadingSkeleton}</div>
}
```

> If React Compiler is enabled, it hoists static JSX automatically — manual hoisting becomes unnecessary.

### 5.5 Animate SVG via Wrapper Div

**Rule:** Apply CSS animations/transforms to a wrapper `<div>` rather than directly to an `<svg>` element. Many browsers do not GPU-accelerate CSS animations on SVG elements directly.

```tsx
// Incorrect — no hardware acceleration on SVG
function LoadingSpinner() {
  return <svg className="animate-spin" width="24" height="24" viewBox="0 0 24 24">...</svg>
}

// Correct — wrapper div gets GPU acceleration
function LoadingSpinner() {
  return (
    <div className="animate-spin">
      <svg width="24" height="24" viewBox="0 0 24 24">...</svg>
    </div>
  )
}
```

### 5.6 Optimize SVG Precision

**Rule:** Reduce SVG coordinate decimal precision to shrink file size. Use SVGO to automate this.

```bash
npx svgo --precision=1 --multipass icon.svg
```

```svg
<!-- Incorrect — excessive precision -->
<path d="M 10.293847 20.847362 L 30.938472 40.192837" />

<!-- Correct — 1 decimal place is visually identical for most icons -->
<path d="M 10.3 20.8 L 30.9 40.2" />
```

### 5.7 Use React DOM Resource Hints

**Rule:** Use React DOM's resource hint APIs to inform the browser about resources it will need. Call these at the top of your app or layout component.

```tsx
import { preconnect, prefetchDNS, preload } from 'react-dom'

function App() {
  prefetchDNS('https://analytics.example.com')    // DNS resolution only
  preconnect('https://api.example.com')            // DNS + TCP + TLS
  preload('/fonts/inter.woff2', {
    as: 'font',
    type: 'font/woff2',
    crossOrigin: 'anonymous',
  })

  return <main>{/* ... */}</main>
}
```

| API | Use case |
|---|---|
| `prefetchDNS` | Third-party domains you'll connect to later |
| `preconnect` | APIs or CDNs you'll fetch from immediately |
| `preload` | Critical resources (fonts, CSS) needed for current page |
| `preloadModule` | JS modules for likely next navigation |
| `preinit` | Stylesheets/scripts that must execute early |

Reference: [React DOM Resource Preloading APIs](https://react.dev/reference/react-dom#resource-preloading-apis)

### 5.8 Use defer / async on Script Tags

**Rule:** Never inject `<script>` tags without `defer` or `async` — they block HTML parsing. In a Vite SPA this is mostly handled by Vite itself, but applies to any manually injected scripts.

- **`defer`**: Downloads in parallel, executes after HTML is parsed, maintains order
- **`async`**: Downloads in parallel, executes immediately when ready, no order guarantee

```html
<!-- Independent script (analytics) — use async -->
<script src="https://example.com/analytics.js" async></script>

<!-- DOM-dependent script — use defer -->
<script src="/scripts/utils.js" defer></script>
```

> `next/script` is Next.js-specific (N/A). Use standard `defer`/`async` attributes.

### 5.9 Hydration Notes (N/A for This SPA)

Rules `rendering-hydration-no-flicker` and `rendering-hydration-suppress-warning` are SSR-specific and do not apply to this CSR-only SPA. There is no server-generated HTML to hydrate.

---

## 6. JavaScript Performance (js)

> **Impact: LOW-MEDIUM** — Micro-optimizations for hot paths that add up in aggregate.

### 6.1 Batch DOM/CSS Reads and Writes

**Rule:** Never interleave DOM style writes with layout reads. Reads between writes force the browser to perform synchronous layout (layout thrashing), which is expensive.

```tsx
// Incorrect — write, read, write forces two synchronous reflows
element.style.width = '100px'
const height = element.offsetHeight // forces reflow
element.style.height = height + 'px'

// Correct — batch writes, then reads
element.style.width = '100px'
element.style.height = '200px'
const height = element.offsetHeight // one reflow at end

// Better — use CSS classes instead of inline styles in React
element.classList.add('expanded') // single class toggle
```

### 6.2 Cache Repeated Function Results

**Rule:** Use a module-level `Map` to cache results of expensive functions called repeatedly with the same inputs (e.g., during list rendering).

```tsx
const slugifyCache = new Map<string, string>()

function cachedSlugify(text: string): string {
  if (slugifyCache.has(text)) return slugifyCache.get(text)!
  const result = slugify(text)
  slugifyCache.set(text, result)
  return result
}

function ProjectList({ projects }: { projects: Project[] }) {
  return projects.map(project => (
    <ProjectCard key={project.id} slug={cachedSlugify(project.name)} />
  ))
}
```

Use a `Map` (not a hook) so it works in utilities, event handlers, and outside React components.

### 6.3 Cache Property Access in Loops

**Rule:** Hoist deeply nested property lookups out of loop bodies to avoid repeated chain traversals.

```tsx
// Incorrect — 3 property lookups × N iterations
for (let i = 0; i < arr.length; i++) {
  process(obj.config.settings.value)
}

// Correct — 1 lookup total
const value = obj.config.settings.value
const len = arr.length
for (let i = 0; i < len; i++) {
  process(value)
}
```

### 6.4 Cache Storage API Calls

**Rule:** `localStorage`, `sessionStorage`, and `document.cookie` are synchronous and relatively expensive. Cache reads in memory.

```tsx
const storageCache = new Map<string, string | null>()

function getLocalStorage(key: string) {
  if (!storageCache.has(key)) {
    storageCache.set(key, localStorage.getItem(key))
  }
  return storageCache.get(key)
}

function setLocalStorage(key: string, value: string) {
  localStorage.setItem(key, value)
  storageCache.set(key, value)
}

// Invalidate on external changes (other tabs)
window.addEventListener('storage', e => {
  if (e.key) storageCache.delete(e.key)
})
```

### 6.5 Combine Multiple Array Iterations

**Rule:** Replace multiple `.filter()` / `.map()` passes over the same array with a single loop.

```tsx
// Incorrect — 3 passes through the users array
const admins = users.filter(user => user.isAdmin)
const testers = users.filter(user => user.isTester)
const inactive = users.filter(user => !user.isActive)

// Correct — 1 pass
const admins: User[] = []
const testers: User[] = []
const inactive: User[] = []

for (const user of users) {
  if (user.isAdmin) admins.push(user)
  if (user.isTester) testers.push(user)
  if (!user.isActive) inactive.push(user)
}
```

### 6.6 Early Return from Functions

**Rule:** Return as soon as the result is known to skip unnecessary processing.

```tsx
// Incorrect — continues checking all users after finding first error
function validateUsers(users: User[]) {
  let hasError = false
  for (const user of users) {
    if (!user.email) hasError = true
  }
  return hasError ? { valid: false } : { valid: true }
}

// Correct — returns immediately on first error
function validateUsers(users: User[]) {
  for (const user of users) {
    if (!user.email) return { valid: false, error: 'Email required' }
    if (!user.name) return { valid: false, error: 'Name required' }
  }
  return { valid: true }
}
```

### 6.7 Use flatMap to Map and Filter in One Pass

**Rule:** Replace `.map().filter(Boolean)` chains with `.flatMap()` to avoid an intermediate array and a second iteration.

```tsx
// Incorrect — 2 iterations, intermediate array
const activeNames = users
  .map(user => user.isActive ? user.name : null)
  .filter(Boolean)

// Correct — 1 iteration, no intermediate array
const activeNames = users.flatMap(user =>
  user.isActive ? [user.name] : []
)
```

### 6.8 Hoist RegExp Out of Render

**Rule:** Don't create `RegExp` objects inside component render functions or render-called helpers. Hoist static patterns to module scope; memoize dynamic patterns with `useMemo`.

```tsx
// Incorrect — new RegExp on every render
function Highlighter({ text, query }: Props) {
  const regex = new RegExp(`(${query})`, 'gi')
  // ...
}

// Correct — static: hoist to module; dynamic: useMemo
const EMAIL_REGEX = /^[^\s@]+@[^\s@]+\.[^\s@]+$/

function Highlighter({ text, query }: Props) {
  const regex = useMemo(
    () => new RegExp(`(${escapeRegex(query)})`, 'gi'),
    [query]
  )
  // ...
}
```

> Warning: global regexes (`/g`) have mutable `lastIndex` state. Be careful sharing them across calls.

### 6.9 Build Index Maps for Repeated Lookups

**Rule:** When you need to look up items from an array repeatedly by key, build a `Map` index first. `Array.find()` is O(n) per call; `Map.get()` is O(1).

```tsx
// Incorrect — O(n) per order = O(n²) total
function processOrders(orders: Order[], users: User[]) {
  return orders.map(order => ({
    ...order,
    user: users.find(user => user.id === order.userId),
  }))
}

// Correct — O(n) to build map, O(1) per lookup
function processOrders(orders: Order[], users: User[]) {
  const userById = new Map(users.map(user => [user.id, user]))
  return orders.map(order => ({
    ...order,
    user: userById.get(order.userId),
  }))
}
```

### 6.10 Early Length Check Before Expensive Comparisons

**Rule:** When comparing arrays with expensive operations (sort, deep equality), check `.length` first. Arrays with different lengths cannot be equal.

```tsx
function hasChanges(current: string[], original: string[]) {
  if (current.length !== original.length) return true // O(1) early exit

  const currentSorted = current.toSorted()
  const originalSorted = original.toSorted()
  for (let i = 0; i < currentSorted.length; i++) {
    if (currentSorted[i] !== originalSorted[i]) return true
  }
  return false
}
```

### 6.11 Use a Loop for Min/Max (Not Sort)

**Rule:** Finding the minimum or maximum only requires O(n) — a single pass. Sorting is O(n log n) and wasteful when you only need one extreme value.

```tsx
// Incorrect — O(n log n)
function getLatestProject(projects: Project[]) {
  return [...projects].sort((a, b) => b.updatedAt - a.updatedAt)[0]
}

// Correct — O(n)
function getLatestProject(projects: Project[]) {
  if (projects.length === 0) return null
  let latest = projects[0]
  for (let i = 1; i < projects.length; i++) {
    if (projects[i].updatedAt > latest.updatedAt) latest = projects[i]
  }
  return latest
}
```

> `Math.min(...numbers)` / `Math.max(...numbers)` work for small arrays but can fail or be slow for very large arrays (spread operator limitations). Use the loop approach for reliability.

### 6.12 Defer Non-Critical Work with requestIdleCallback

**Rule:** Schedule analytics, telemetry, localStorage writes, and other non-critical work during browser idle periods using `requestIdleCallback`. This keeps the main thread free for user interactions.

```tsx
function handleSearch(query: string) {
  const results = searchItems(query)
  setResults(results)

  // Defer non-critical work
  requestIdleCallback(() => analytics.track('search', { query }))
  requestIdleCallback(() => saveToRecentSearches(query))
}

// With timeout to guarantee execution within 2 seconds
requestIdleCallback(
  () => analytics.track('page_view', { path: location.pathname }),
  { timeout: 2000 }
)

// Fallback for older browsers
const scheduleIdleWork = window.requestIdleCallback ?? ((cb: () => void) => setTimeout(cb, 1))
```

### 6.13 Use Set/Map for O(1) Membership Checks

**Rule:** When checking membership in a collection repeatedly, convert the array to a `Set` first.

```tsx
// Incorrect — O(n) per check
const allowedIds = ['a', 'b', 'c']
items.filter(item => allowedIds.includes(item.id))

// Correct — O(1) per check
const allowedIds = new Set(['a', 'b', 'c'])
items.filter(item => allowedIds.has(item.id))
```

### 6.14 Use toSorted() for Immutable Sorts

**Rule:** Use `.toSorted()` instead of `.sort()` to avoid mutating props or state arrays. `.sort()` mutates in place and breaks React's immutability model.

```tsx
// Incorrect — mutates the users prop!
const sorted = useMemo(
  () => users.sort((a, b) => a.name.localeCompare(b.name)),
  [users]
)

// Correct — returns new array, original unchanged
const sorted = useMemo(
  () => users.toSorted((a, b) => a.name.localeCompare(b.name)),
  [users]
)
```

Similarly: `.toReversed()`, `.toSpliced()`, `.with()` are the immutable counterparts to `reverse()`, `splice()`, and index assignment.

> Browser support: Chrome 110+, Safari 16+, Firefox 115+, Node 20+. Fallback: `[...items].sort(...)`.

---

## 7. Advanced Patterns (advanced)

> **Impact: LOW** — Specific patterns for edge cases requiring careful implementation.

### 7.1 Initialize App Once, Not Per Mount

**Rule:** Do not put app-wide one-time initialization (auth checks, storage loading) inside `useEffect([])`. Components can remount and effects re-run. Use a module-level guard instead.

```tsx
// Incorrect — runs twice in React StrictMode dev, re-runs on remount
function App() {
  useEffect(() => {
    loadFromStorage()
    checkAuthToken()
  }, [])
}

// Correct — guaranteed to run once per app load
let didInit = false

function App() {
  useEffect(() => {
    if (didInit) return
    didInit = true
    loadFromStorage()
    checkAuthToken()
  }, [])
}
```

Reference: [Initializing the Application](https://react.dev/learn/you-might-not-need-an-effect#initializing-the-application)

### 7.2 Store Event Handlers in Refs for Stable Subscriptions

**Rule:** When an event listener's callback changes frequently but the subscription itself should remain stable, store the latest callback in a ref rather than re-subscribing.

```tsx
// Incorrect — re-subscribes to window event on every render
function useWindowEvent(event: string, handler: (e: Event) => void) {
  useEffect(() => {
    window.addEventListener(event, handler)
    return () => window.removeEventListener(event, handler)
  }, [event, handler]) // handler changes every render
}

// Correct — stable subscription, always calls latest handler
function useWindowEvent(event: string, handler: (e: Event) => void) {
  const handlerRef = useRef(handler)

  useEffect(() => {
    handlerRef.current = handler
  }, [handler])

  useEffect(() => {
    const listener = (e: Event) => handlerRef.current(e)
    window.addEventListener(event, listener)
    return () => window.removeEventListener(event, listener)
  }, [event])
}
```

**Alternative using `useEffectEvent` (React 19+):**

```tsx
import { useEffectEvent } from 'react'

function useWindowEvent(event: string, handler: (e: Event) => void) {
  const onEvent = useEffectEvent(handler)

  useEffect(() => {
    window.addEventListener(event, onEvent)
    return () => window.removeEventListener(event, onEvent)
  }, [event])
}
```

### 7.3 Do Not Put Effect Events in Dependency Arrays

**Rule:** Functions returned by `useEffectEvent` have intentionally unstable identity — their reference changes every render. Never include them in a `useEffect` dependency array.

```tsx
// Incorrect — handleConnected in deps causes re-run every render + lint error
function ChatRoom({ roomId, onConnected }: { roomId: string; onConnected: () => void }) {
  const handleConnected = useEffectEvent(onConnected)

  useEffect(() => {
    const connection = createConnection(roomId)
    connection.on('connected', handleConnected)
    connection.connect()
    return () => connection.disconnect()
  }, [roomId, handleConnected]) // wrong
}

// Correct — only reactive values in deps; call Effect Event from inside
function ChatRoom({ roomId, onConnected }: { roomId: string; onConnected: () => void }) {
  const handleConnected = useEffectEvent(onConnected)

  useEffect(() => {
    const connection = createConnection(roomId)
    connection.on('connected', handleConnected) // called inside, not a dep
    connection.connect()
    return () => connection.disconnect()
  }, [roomId]) // correct
}
```

Reference: [React useEffectEvent](https://react.dev/reference/react/useEffectEvent#effect-event-in-deps)

### 7.4 Use useEffectEvent for Stable Callback Refs

**Rule:** Use `useEffectEvent` to access the latest version of a callback inside an effect without adding it to the dependency array. This prevents effect re-runs when the callback reference changes.

```tsx
// Incorrect — effect re-runs every time onSearch prop changes
function SearchInput({ onSearch }: { onSearch: (q: string) => void }) {
  const [query, setQuery] = useState('')

  useEffect(() => {
    const timeout = setTimeout(() => onSearch(query), 300)
    return () => clearTimeout(timeout)
  }, [query, onSearch]) // onSearch forces re-runs
}

// Correct — only re-runs when query changes
import { useEffectEvent } from 'react'

function SearchInput({ onSearch }: { onSearch: (q: string) => void }) {
  const [query, setQuery] = useState('')
  const onSearchEvent = useEffectEvent(onSearch)

  useEffect(() => {
    const timeout = setTimeout(() => onSearchEvent(query), 300)
    return () => clearTimeout(timeout)
  }, [query]) // stable
}
```

---

## Quick Reference Summary

| Category | Key Rules |
|---|---|
| **Async** | `Promise.all()` for parallel, `await` only where needed, Suspense for streaming |
| **Bundle** | Direct imports (no barrels), `React.lazy()` for heavy components, preload on hover |
| **Client** | Use SWR for deduplication, passive listeners, version localStorage data |
| **Re-render** | No inline components, derive state in render, functional setState, narrow deps |
| **Rendering** | Explicit conditionals (no `&&` with numbers), `content-visibility`, hoist static JSX |
| **JS** | `Promise.all()`, Map for O(1) lookups, `toSorted()`, `flatMap`, early return |
| **Advanced** | Module-level init guard, handler refs for stable subscriptions, `useEffectEvent` |
