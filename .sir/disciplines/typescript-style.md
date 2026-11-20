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
