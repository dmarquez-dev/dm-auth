<!-- source: sir-sdlc disciplines/frontend.md -->

# Frontend discipline

Rules are binding. Where one conflicts with a frozen contract, the contract wins.

## State ownership

- Every piece of state has exactly one owner. Two components holding the same fact will disagree.
- State lives at the lowest point that can serve every reader. Lifting further is a cost paid by everything in between.
- Derived values are computed, never stored. Stored derivations go stale silently.
- Server data and client interaction state are separate concerns and do not share a store.

## Data fetching

- A component states what data it needs; it does not orchestrate how the fetch happens.
- Every fetch has a declared loading state, a declared error state, and a declared empty state. A component that renders only the success path is unfinished.
- Requests whose results are independent are issued together. A dependent chain is a waterfall and is justified explicitly or removed.
- Nothing is fetched in response to a render. Fetching belongs to an explicit lifecycle or event.

## Component boundaries

- A component either arranges other components or renders content. One that does both is two components.
- Props are an interface. A component reaching around its props into global state cannot be reused or tested.
- Presentation components take data and callbacks; they do not know where either came from.
- A component file that has to be scrolled to be understood is doing too much.

## Accessibility floor

- Every interactive element is reachable and operable by keyboard alone.
- Semantic elements are used for their meaning. A clickable non-interactive element must carry an explicit role and keyboard handling.
- Every form control has a programmatically associated label.
- Color is never the only carrier of meaning.
- Focus is visible, and it moves predictably after an action that changes what is on screen.

<!-- source: .sir/disciplines/typescript-style.md -->

# TypeScript style

Binds every `.ts` and `.tsx` file in `src/DMAuth.Client/`, source and test alike.

## File naming

- Name a component file in PascalCase, matching the component it holds: `ProfilePage.tsx`, `AppLayout.tsx`.
- Name every non-component TypeScript file in camelCase: `authApi.ts`, `renderWithProviders.tsx`.
- Name a test file after the file it tests plus a `.test` suffix: `ProfilePage.test.tsx`, `client.test.ts`.
- Put one component, or one logical unit, in a file; do not co-locate unrelated exports.

## Component style

- Write function components only; never a class component.
- Name every component function in PascalCase.
- Declare a top-level exported component with a function declaration (`export function ProfilePage() {}`), not an arrow function assigned to a `const`.
- A small sub-component used only inside its own file may use arrow-function syntax.

## Exports

- Use named exports only; never `export default`.
- Exception: a tool configuration file whose tool requires a default export (`vite.config.ts`, `vitest.config.ts`, `playwright.config.ts`, `eslint.config.js`).

## Props

- Declare a component's props as an inline type or an interface in the same file as the component.
- Name a props interface `{ComponentName}Props`.
- Export a props type only when another component consumes it.

## Styling

- Style with Tailwind CSS utility classes only; no CSS Modules, styled-components, or emotion.
- Do not pass an inline `style` prop; use Tailwind classes, or add the project-specific value to the Tailwind theme (`@theme` in `src/index.css`).
- Do not use an arbitrary Tailwind value (e.g. `w-[137px]`) for a spacing or color that has a Tailwind equivalent.
- Do not add `dark:` variants; dark mode is not supported.

## Import organization

- Order imports in these groups, with one blank line between groups:
  1. React and its ecosystem: `react`, `react-dom`, `react-router-dom`.
  2. Third-party libraries: `@tanstack/react-query`, `axios`, `zod`, `react-hook-form`, and the rest.
  3. Internal modules, in this order: `types/`, `api/`, `auth/`, `components/`, `pages/`.
  4. Relative imports of siblings and parents (`./`, `../`) that are not one of the internal module folders above.
- Import type-only symbols with `import type`.
- Apply the order by hand; no lint rule enforces it.

<!-- source: .sir/disciplines/react.md -->

# React

Binds all React and SPA code in `src/DMAuth.Client/`.

## Project context

- DMAuth.Client is a client-side-only single-page app built with Vite, React and TypeScript.
- Not applicable here: SSR, hydration, React Server Components, server actions, async function components, and every Next.js API (`next/dynamic`, `next/script`, output file tracing). Do not introduce any of them.

## Project structure

- `src/api/` — HTTP client modules, one per resource.
- `src/auth/` — the auth provider, the route guard, and the auth hook.
- `src/components/` — shared, reusable components.
- `src/pages/` — page components, one per route.
- `src/test/` — unit and component test helpers.
- `src/types/` — shared type definitions.
- `src/App.tsx` — root component holding the router and the providers; `src/main.tsx` — application entry point.
- `tests/e2e/` — end-to-end specs organized by feature; `tests/e2e/fixtures/` — custom test fixtures.
- Tool configuration (`vite.config.ts`, `vitest.config.ts`, `playwright.config.ts`, `tsconfig*.json`) and `index.html` live at the client root.

## State management

| State type | Owner |
|---|---|
| Server state (API data) | TanStack Query, except the auth session calls below |
| Auth state | Shared context, exposed through the auth provider and `useAuth` |
| Form state | react-hook-form with a zod schema |
| Local UI state | Component-local state |

- Read resource data with `useQuery` and change it with `useMutation`; never call `fetch` or the HTTP client ad hoc from an effect or a component body.
- Exception, auth session calls: await `authApi.login`, `authApi.register` and `authApi.logout` directly in the event handler that triggers them (a form `onSubmit`, the auth provider's `logout`), then invalidate or clear the affected queries (`invalidateQueries({ queryKey: ['auth', 'me'] })`, `queryClient.clear()`) and navigate. Make no other API call outside a query or mutation.
- Shape query keys as `['resource', id]` (e.g. `['clients', clientId]`).
- Invalidate the related queries in a mutation's `onSuccess`.
- Read auth state through `useAuth`; never import the auth context directly.
- Protect every authenticated route with the route guard (`ProtectedRoute`); a page component does not check auth itself.

## API client modules

- Give each resource one module under `src/api/` exporting one named object of typed functions: `authApi`, `userApi`, `clientApi`.
- Call through the shared `apiClient` instance (`src/api/client.ts`), configured with the base URL and credentials; never create another HTTP client.
- Type every request and response payload; pass the response type to the client call (`apiClient.get<User>(...)`); no untyped values.
- Keep 401 handling in the `apiClient` response interceptor only:
  - on session expiry it redirects to the login page with a `returnUrl`;
  - it does not redirect for the session-check endpoint or the change-password endpoint, where a 401 carries business meaning.

## TanStack Query integration

- Read with `useQuery({ queryKey, queryFn })`, where `queryFn` calls the resource module and unwraps `response.data`.
- Write resource data with `useMutation({ mutationFn, onSuccess })` (auth session calls excepted, see State management), invalidating through `useQueryClient().invalidateQueries({ queryKey })` in `onSuccess`.
- Let components that need the same data share one query key, so TanStack Query deduplicates and caches the request; never fetch in `useEffect` into `useState`.
- For data that never changes during a session, set `staleTime: Infinity` rather than refetching.
- Do not add SWR or any second data-fetching library.

## Form validation

- Define every validation rule in one zod schema per form; derive the form's values type with `z.infer<typeof schema>`.
- Connect the schema with `useForm({ resolver: zodResolver(schema) })`.
- Render each field error directly below its input.
- Report an API error with `setError('root', { message: extractApiError(err) })` and render it in a `role="alert"` banner above the submit button.
- Disable the submit button while `formState.isSubmitting`.

## Async and waterfalls

- Start independent async calls together with `Promise.all()`; never `await` them in sequence.
- With partial dependencies, start each promise as soon as its inputs exist and chain the dependent one (`userPromise.then(user => fetchProfile(user.id))`); do not wait on work a later call does not need.
- Move an `await` into the branch that uses its result.
- Evaluate a cheap synchronous condition before awaiting an async value in the same check.
- Wrap a data-dependent section in `<Suspense>` so the surrounding page chrome renders immediately; to share one promise across siblings, start it once and unwrap it in each child with `use()`.
- Do not put a Suspense boundary around critical above-the-fold content where the layout shift is unacceptable.

## Bundle size

- Keep import paths statically analyzable: a lookup of dynamic modules maps keys to `() => import('./literal/path')` functions, never to path strings passed to `import()`.
- Import from a library's specific module path, not its barrel entry, when the barrel re-exports a large surface (icon packs, component libraries).
- Lazy-load a large component not needed on first render with `React.lazy()` inside `<Suspense>`; because modules have named exports only, map the export: `lazy(() => import('./MonacoEditor').then(module => ({ default: module.MonacoEditor })))`.
- Load a large data or feature module with `import()` only when the feature is activated, not at startup.
- Preload a heavy module on user intent: call `void import(...)` from `onMouseEnter` and `onFocus` of the control that opens it.
- Load analytics, logging and error-tracking libraries after the app is interactive, via `import()` inside a mount `useEffect`.

## Browser events and storage

- Pass `{ passive: true }` to `touchstart`, `touchmove` and `wheel` listeners, unless the handler calls `preventDefault()`.
- Share a global event listener across component instances through one module-level subscriber, so N instances register one listener.
- Persist to `localStorage` under a versioned key (`'userPrefs:v2'`), store only the fields needed (never a full server response, a token, or PII), and wrap every access in `try`/`catch`.
- Cache `localStorage`, `sessionStorage` and `document.cookie` reads in a module-level `Map`; update it on write and invalidate it from the `storage` event.

## Re-renders

- Never define a component inside another component; define it at module level and pass props.
- Compute a value derivable from props or state during render; never store it in state or sync it with `useEffect`.
- Subscribe to a derived boolean (`useMediaQuery('(max-width: 767px)')`), not a continuous value (window width) that is then compared.
- Run a side effect caused by a user action in that action's event handler, not in an effect watching a state flag.
- List the primitive a `useEffect` reads in its dependencies (`[user.id]`), not the whole object (`[user]`); derive a boolean outside the effect when the effect only cares about a threshold.
- Use the functional form `setState(current => ...)` whenever the next state depends on the current one.
- Pass an initializer function to `useState` for an expensive initial value (storage reads, building maps or indexes, DOM reads, heavy transforms).
- Hold frequently changing values that do not drive rendering (pointer position, scroll offset, timers, transient flags) in `useRef`, not state.
- Do not subscribe to dynamic state (`useSearchParams`, storage) that is read only inside a callback; read it on demand in the callback (`new URLSearchParams(window.location.search)`).
- Move expensive work into a `memo()`-wrapped child so the parent can return early before it runs.
- Give a non-primitive default prop of a memoized component a module-level constant (`const NOOP = () => {}`), never an inline literal.
- Do not wrap a simple expression with a primitive result in `useMemo`; reserve `useMemo` for expensive transforms, large array work, and reference stability for children.
- Split a `useMemo` or `useEffect` that does independent things with different dependencies into one hook per thing.
- Feed an expensive render driven by user input from `useDeferredValue(input)`, and memoize the expensive computation on the deferred value.
- Wrap non-urgent state updates (scroll tracking, background processing) in `startTransition`.
- Track an async action's pending state with `useTransition`'s `isPending`, not a manual `isLoading` state.

## Rendering

- Toggle an expensive component that should keep its state and DOM with `<Activity mode={visible ? 'visible' : 'hidden'}>` rather than unmounting it.
- Render conditionally with a ternary (`count > 0 ? <Badge /> : null`) whenever the condition can be `0` or `NaN`; never `count && ...`.
- Give the items of a long list `content-visibility: auto` with a `contain-intrinsic-size` estimate, applied as Tailwind arbitrary properties (`[content-visibility:auto]`).
- Hoist JSX with no changing props to a module-level constant.
- Apply CSS animations and transforms to a wrapper `<div>`, never to the `<svg>` element.
- Commit SVG assets with reduced coordinate precision (e.g. `npx svgo --precision=1 --multipass icon.svg`).
- Declare needed resources with React DOM's resource hints, at the top of the app or layout component: `prefetchDNS` for third-party domains used later, `preconnect` for APIs or CDNs fetched immediately, `preload` for critical fonts and CSS, `preloadModule` for likely next-navigation modules, `preinit` for stylesheets or scripts that must execute early.
- Give every manually injected `<script>` `async` (independent scripts) or `defer` (DOM-dependent or order-dependent scripts); never neither.

## JavaScript performance

- Do not interleave DOM style writes with layout reads; batch the writes, then read, or toggle a CSS class instead of writing styles.
- Cache an expensive pure function called repeatedly with the same inputs in a module-level `Map`, not a hook.
- Hoist a deep property lookup and the array length out of a loop body.
- Replace several `.filter()`/`.map()` passes over one array with a single loop.
- Return from a function as soon as its result is known.
- Replace `.map(...).filter(Boolean)` with one `.flatMap(...)`.
- Never construct a `RegExp` in render or a render-called helper; hoist a static pattern to module scope and memoize a dynamic one with `useMemo`. Do not share a `/g` regex across calls without resetting `lastIndex`.
- Build a `Map` index before repeated by-key lookups into an array; never call `Array.find()` inside a loop over another collection.
- Compare `.length` first before an expensive array comparison (sort, deep equality).
- Find a minimum or maximum with one loop, never by sorting, and not with `Math.min(...array)` on large arrays.
- Schedule non-critical work (analytics, telemetry, storage writes) with `requestIdleCallback`, with a `setTimeout` fallback where it is unavailable.
- Convert an array to a `Set` before repeated membership checks.
- Sort, reverse, splice or replace by index with `toSorted()`, `toReversed()`, `toSpliced()`, `with()`; never mutate a prop or state array with `sort()`, `reverse()` or `splice()`.

## Effects and initialization

- Guard app-wide one-time initialization (storage loading, auth checks) with a module-level flag (`let didInit = false`), not an unguarded `useEffect(..., [])`.
- Keep a subscription stable when its callback changes: read the latest callback through `useEffectEvent` (or a ref updated in an effect), and depend only on the subscription's own inputs.
- Never list a function returned by `useEffectEvent` in a dependency array; call it from inside the effect.
