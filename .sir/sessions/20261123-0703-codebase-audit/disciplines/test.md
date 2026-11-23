<!-- source: sir-sdlc disciplines/test.md -->

# Test discipline

Rules are binding. Where one conflicts with a frozen contract, the contract wins.

## Behavior over implementation

- A test asserts what the unit does, not how it does it. A test that breaks on a rename that changed no behavior was testing the wrong thing.
- Assert on returned values and observable effects, never on private state.
- A mock stands in for a boundary the test cannot cross. Mocking the thing under test asserts only that the mock was configured.
- A test that would still pass with the implementation deleted is not a test.

## Naming

- A test name states the condition and the expected outcome, so a failure is legible without opening the file.
- A name that says "works" or "is correct" names nothing.
- The name must not claim more than the test checks. A name broader than its assertions misleads every later reader.

## Isolation

- Tests do not share mutable state, and they pass in any order.
- Each test creates what it needs and leaves nothing behind. A test depending on another's leftovers fails alone.
- No test depends on the wall clock, the ambient time zone, the network, or a fixed port.

## Determinism

- No sleeping. Wait on the condition, with a timeout, or make the operation synchronous for the test.
- Randomness is seeded, and the seed is reported on failure.
- A test that fails intermittently is treated as a failing test, not as noise to re-run.

## Coverage

- Every acceptance criterion has a test that would fail if the criterion were unmet.
- A test is shown able to fail — seen failing before it is made to pass, or shown to catch the defect it exists for when that defect is injected. Either establishes it; a test never observed failing has not been shown to test anything, and one whose failing input cannot occur tests nothing at all.
- The failure path is tested, not only the success path.
- A bug fix arrives with the test that reproduces it.

<!-- source: .sir/disciplines/csharp-style.md -->
# C# style

Binds every C# file, production and test alike.

## Naming

| Element | Convention | Example |
|---|---|---|
| Namespace | PascalCase, matches the folder structure | `DMAuth.Application.Features.Users.Register` |
| Class / record | PascalCase | `RegisterUserCommand` |
| Interface | `I` prefix + PascalCase | `IUserRepository` |
| Method | PascalCase | `FindByEmail()` |
| Property | PascalCase | `CreatedAt` |
| Private field, static or instance | `_camelCase` | `_userRepository`, `_allowedScopes` |
| Parameter | camelCase | `cancellationToken` |
| Constant | PascalCase | `MaxLoginAttempts` |
| Enum member | PascalCase | `ClientType.Confidential` |

- Private static fields are `_camelCase`, exactly like every other private field.
- Name lambda parameters descriptively (`user =>`, `client =>`, `token =>`); never use a single-letter lambda parameter.

## File organization

- Declare one type (class, record, interface, enum) per file.
- Name the file exactly after the type: `RegisterUserCommand.cs`.
- Mirror the namespace hierarchy in the folder structure.

## Parameter layout

- Use a primary constructor wherever the type supports one.
- Primary constructor parameters: always one per line, at any count, including one.
- Regular constructor, method and expression-bodied member parameters: inline when there is exactly one; one per line when there are two or more.

```csharp
public class DomainException(
	string message)
		: Exception(message);

public Email(string value)
{
}

public async Task<Result> Handle(
	RegisterUserCommand request,
	CancellationToken cancellationToken)
{
}
```

## Declaration layout

- Put parameters, the base class / interface list, and each `where` clause on their own lines.
- Indent each element type +1 tab from the previous element type that is present, in the order declaration (0), parameters, inheritance, `where`. An absent element type consumes no level.

```csharp
public class TestClass<T>(
	IService service,
	IOtherService otherService)
		: BaseClass, ITestClass<T>
			where T : class
{
}

public class OtherClass<T>
	where T : class
{
}
```

## Lambdas, expression bodies and chaining

- Put everything after `=>` on a new line, indented one level; this applies to lambdas and expression-bodied members alike.
- Put each chained call on its own line, indented one level from the initial target (LINQ, fluent builders, FluentValidation rules, string operations).

```csharp
var results = dbContext.Users
	.Where(user =>
		user.IsActive)
	.Select(user =>
		new UserSummary(
			user.Id,
			user.Username))
	.ToListAsync(cancellationToken);

public bool IsExpired =>
	ExpiresAt < DateTimeOffset.UtcNow;
```

## XML documentation comments

- Give every public type, member and parameter an XML documentation comment.
- Put each tag's description on a new line after the opening tag, indented two tabs after `///`.
- Wrap a long sentence onto a new `///` line indented two tabs, rather than running past line length.
- Write every description as complete sentences, each ending with a period.

```csharp
/// <summary>
///		Handles the <see cref="RegisterUserCommand"/> by creating a new user account.
///		Returns a conflict result if the email or username is already in use.
/// </summary>
/// <param name="request">
///		The command containing the registration details.
/// </param>
```

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

<!-- source: .sir/disciplines/dotnet-tests.md -->
# .NET tests

## Stack

- Write tests with xUnit v3 (`[Fact]`, `[Theory]`), assertions with FluentAssertions, and test doubles with NSubstitute. Do not add another test framework, assertion library, or mocking library.
- Put isolated handler and domain tests in `tests/DMAuth.Tests.Unit`.
- Put persistence and HTTP-pipeline tests in `tests/DMAuth.Tests.Integration`: exercise the persistence layer through the EF Core in-memory provider, and the full request pipeline through `IntegrationTestFactory` (`WebApplicationFactory<Program>`).

## Naming

- Name every test method `MethodName_Scenario_ExpectedResult`, e.g. `Handle_WithValidCommand_ReturnsSuccessWithUserId`, `Handle_WithDuplicateEmail_ReturnsConflictError`, `Validate_WithShortPassword_ReturnsViolation`.
- Name the test class `<SourceType>Tests`, e.g. `RegisterUserCommandHandlerTests`.

## Organization

- Mirror the source structure in unit tests: one test file per source file, grouped in folders by layer.
- Group integration test classes in folders by feature; each class receives the shared `IntegrationTestFactory` test server fixture.

## Structure

- Lay out every test body as Arrange, Act, Assert, each section introduced by a `// Arrange`, `// Act`, or `// Assert` comment line, in that order.
- Arrange sets up inputs, substitutes, and expected values; Act invokes the unit under test; Assert verifies the result.
- Pass `CancellationToken.None` when the unit under test takes a cancellation token.

## Test class setup and isolation

- Create every substitute and the system under test in the test class constructor, assigned to `private readonly` fields; never in a static field.
- Share no state between test classes. Each integration test class gets its own database instance.
- Tests within one class run sequentially; share a fixture between tests only within one class.
- Each test seeds the data it needs. Never rely on data left by another test.

## Assertions

- Use FluentAssertions for every assertion. Do not use xUnit `Assert.*`.
- Chain `.Should()` directly off the subject under assertion: `result.IsSuccess.Should().BeTrue()`, `result.Value.Should().Be(expectedId)`.
- Assert a `Result` failure on its flag and its kind: `result.IsSuccess.Should().BeFalse()` and `result.ErrorType.Should().Be(ResultError.Conflict)`; assert error text with `result.Error.Should().Contain(...)`.
- Assert collections with FluentAssertions collection assertions: `users.Should().HaveCount(3)`, `users.Should().ContainSingle(user => user.Email == "test@example.com")`.

## Substitutes (NSubstitute)

- Create each dependency's substitute with `Substitute.For<TInterface>()`.
- Match arguments the test does not care about with `Arg.Any<T>()`, including `Arg.Any<CancellationToken>()`.
- Configure a return with `.Returns(value)`; configure a null return with an explicit nullable cast, `.Returns((User?)null)`.
- Verify an expected call with `.Received(n)` (with `await` when the member is async); verify an absent call with `.DidNotReceive()`.

<!-- source: .sir/disciplines/client-tests.md -->
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
