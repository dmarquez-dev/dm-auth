# External SSO readiness

Restore a solution that builds and test projects that run, then make DM Auth's authorization-code + PKCE flow usable and safe for an external client (a .NET API + React SPA) signing in through it.

## Context

- At base `ba33897`, `dotnet build src/DM-Auth.sln` fails restore with `NU1605` (Infrastructure pins `Microsoft.Extensions.DependencyInjection.Abstractions` 10.0.7 against a floating `10.*` chain that now needs 10.0.15). Probed in this worktree at session setup.
- Both test projects run nothing under `dotnet test`: xUnit v3 with no `Microsoft.NET.Test.Sdk` / `xunit.runner.visualstudio`, so VSTest mode skips them (unit probe: restore lines only, exit 0).
- With the runner fixed, every integration test fails on `DefaultAzureCredential` because `Program.cs` always loads Key Vault and the test factory overrides a stale key (`KeyVault:VaultUri`) too late. Measured by the architect on a scratch copy: unit 234/234 passed, integration 107/107 failed.
- On the flow an external client walks: the token endpoint never checks a confidential client's secret (`ClientSecretHash` is read by nothing in Application); `GrantConsentHandler` issues a code for whatever redirect URI and scopes the consent form posts; the code redirect appends `?code=` to a redirect URI that may already carry a query (`AuthController.cs:290-302`); the consent form posts to a relative `/connect/consent` (`ConsentPage.tsx:121`), which only works under the vite proxy; `LoginPage` sends the `/connect/authorize` resume URL to the client-side router.

## Definition of done

1. `dotnet restore src/DM-Auth.sln` and `dotnet build src/DM-Auth.sln` exit 0 with no `NU1605`.
2. No package reference in `DMAuth.Infrastructure.csproj` floats; the three formerly floating packages keep an exact version.
3. `dotnet test` on the unit project prints a summary with Total 234 and 0 failed.
4. `dotnet test` on the integration project prints a summary with 0 failed and a Total of 107 plus the tests T2 adds.
5. Both test projects use one runner, VSTest (D6).
6. Integration tests run with no Azure credentials, and production startup still fails fast on an empty Key Vault connection string.
7. A confidential client with a wrong `client_secret` gets `Unauthorized("invalid_client")` on the authorization-code exchange, before the code is looked up.
8. The same on refresh-token rotation, before the refresh token is looked up.
9. A confidential client that omits its secret gets `invalid_client` on both grants.
10. Public clients (no secret, PKCE) keep working unchanged on both grants.
11. `client_secret` is read from the token request form (client_secret_post), end to end through `POST /connect/token`.
12. `grep -rn "ClientSecretHash" src/DMAuth.Application --include=*.cs` prints at least one line.
13. A registered redirect URI with a query keeps it: Location is `https://example.com/callback?tenant=a&code=...&state=...`.
14. GrantConsent with an unregistered (or extended) redirect URI fails with `Invalid` and adds no authorization code.
15. GrantConsent with a scope outside the client's allowed scopes fails with `Invalid` and adds no authorization code.
16. The consent form's `action` is derived from the API base URL `apiClient` uses.
17. Login classifies `returnUrl`: app-relative path → router; the API's `/connect/authorize` URL → `window.location.assign`; anything else, including `//example.org` → `/dashboard`.
18. Every fix has a test that fails before it and passes after (D7).
19. `dotnet test src/DM-Auth.sln` is green; the client's `npm test`, `lint` and `build` exit 0.
20. On the deployed site, a consent approval returns the API's 302 to the client redirect URI (FE-003 Verify, second half).
21. Signed out, an authorization at `/connect/authorize` resumes after sign-in to the consent page or the client's redirect URI (FE-004 Verify, first half).

## Scope

The seven backlog rows signed at Gate 1: IW-001, NT-001, FE-003, FE-004, IW-004, DA-001, DA-002, plus a declared `KeyVault:Enabled` switch (default true) that the integration test factory turns off (Gate 2 D1, E2). `npm ci` is run in the worktree before T5 (Gate 2 D2).

### Not in scope

- `client_secret_basic` (HTTP Basic client authentication): deferred; `ExchangeTokenValidator` requires a form `client_id`, so it is not trivial (D4).
- FE-002 consent CSRF, FE-001 deny redirect, DA-004 revoke client binding, IW-003 token `no-store`, RP-003 CI test gate: stay in the backlog (D8).
- Item 20 (deployed consent approval): carried to the backlog at Gate 3 as a human check (Gate 2 D3).
- `AuthController`'s `/login` and `/consent` redirects being relative to the API host (architect finding F1), and with it item 21: deferred to a new backlog row appended at Gate 3 (Gate 2 D4).
- The floating `10.*` versions in `DMAuth.Web.csproj` and `DMAuth.Tests.Integration.csproj`: left as they are.

## Assumptions

| Question | Answer | Rung | Evidence |
|---|---|---|---|
| Which test runner? | VSTest: add `Microsoft.NET.Test.Sdk` and `xunit.runner.visualstudio` to both projects; no `global.json` | discussion D6 + repo | keeps `dotnet test src/DM-Auth.sln` (dotnet.md) working; measured 234/107 in scratch |
| How is IW-001 fixed? | Remove the `DependencyInjection.Abstractions` pin; raise `Options.ConfigurationExtensions` to 10.0.15 (removing it breaks `DependencyInjection.cs:60,64` with CS1503); pin the three `10.*` refs to 10.0.15 | repo, measured | architect scratch build; NU1605 message names 10.0.15 |
| What are the test totals at base? | Unit 234, integration 107 | repo, counted two ways | VSTest summaries in scratch; `[Fact]` + `[InlineData(` census 193+41 and 93+14, re-run at `ba33897` this phase (no `MemberData`/`ClassData`) |
| Include client_secret_basic? | No | judgment + repo | validator requires form `client_id` |
| Confidential client with a null `ClientSecretHash`? | `invalid_client`, never construct `HashedPassword` (its constructor throws on empty) | repo | `HashedPassword.cs:26-29` |
| Public client that sends a secret? | The secret is ignored | discussion D5 | — |
| Where do the DA-002 checks run? | Before any consent write and before the code is created, as `AuthorizeHandler` does | judgment, repo precedent | `AuthorizeHandler.cs:43,54` |
| What counts as the allow-listed returnUrl? | Origin equals `VITE_API_BASE_URL` resolved against `window.location.origin`, and path exactly `/connect/authorize` | backlog FE-004 + repo | `client.ts:4` |
| How is the FE-003 action built? | From the same base `apiClient` uses, not a Static Web Apps route; the env var stays read in one file | repo | `client.ts:4`; `staticwebapp.config.json` has only a navigation fallback |
| Can two dotnet tasks build at once? | No: one shared `obj/`/`bin/` per project, every build recompiles Domain and Application; T1→T2→T3→T4 is a chain | repo + judgment | `Directory.Build.props` sets no output paths |
| Can T5 run beside T3/T4? | Yes: npm and dotnet read disjoint files (`.tsx` vs `.cs`), and the solution does not include the client | repo | `src/DM-Auth.sln` |

## Contracts

None. Every shared shape (`ClientSecret` on `ExchangeTokenCommand` / `RotateTokenCommand` / `ExchangeTokenRequest`, the `client_secret` form field, the `IPasswordHasher.Verify` call) is inside T3, and the endpoints the client calls do not change shape.

## Open questions

None. Gate 2 resolved D1 (Key Vault switch), D2 (`npm ci` in the worktree), D3 (item 20 to the backlog as a human check) and D4 (item 21 behind a new backlog row).
