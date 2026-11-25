# External SSO readiness

Make the live DM Auth authorization-code + PKCE flow usable and safe for an external application
(a .NET API + React SPA such as DM Fitness) signing in through DM Auth SSO, after first restoring
a solution that builds and test projects that actually run tests.

## Context

- The user is clearing backlog items while preparing DM Auth to serve an external application
  (DM Fitness: .NET API + React frontend) that signs in through the live DM Auth service.
- The build is broken at base `ba33897`: `dotnet build src/DM-Auth.sln` exits with 1 error,
  `NU1605` (downgrade of `Microsoft.Extensions.DependencyInjection.Abstractions`) from
  `src/DMAuth.Infrastructure/DMAuth.Infrastructure.csproj`, measured in this session's discuss
  phase. Backlog row `.sir/backlog.md:24` (IW-001) records the cause.
- Both test projects run zero tests under `dotnet test` (xUnit v3, VSTest mode, no
  `Microsoft.NET.Test.Sdk` / `xunit.runner.visualstudio`), per backlog row `.sir/backlog.md:30`
  (NT-001), recorded by session `20261123-0703-codebase-audit`. Its expected totals (234 unit,
  107 integration at 5b7d9a6) are that session's figures, not re-measured here.
- Confirmed at `ba33897` in this phase:
  - `src/DMAuth.Client/src/pages/ConsentPage.tsx:121` — `<form method="post" action="/connect/consent">`, relative (FE-003, `.sir/backlog.md:16`).
  - `src/DMAuth.Client/src/pages/LoginPage.tsx:20` reads `returnUrl`, `:33` calls `navigate(returnUrl, { replace: true })` (FE-004, `.sir/backlog.md:17`).
  - `src/DMAuth.Web/Auth/AuthController.cs:290-302` — `BuildCodeRedirectUrl` returns `$"{redirectUri}{query}"` with `QueryString.Create` (IW-004, `.sir/backlog.md:26`).
  - `ClientSecretHash` is read only by `Client.cs` and the EF configuration/migrations; nothing in Application verifies it (DA-001, `.sir/backlog.md:5`).
- CORS is configured from `Cors:AllowedOrigins` (`src/DMAuth.Web/Program.cs:118-128`) and access
  tokens use one `Jwt:Audience`; both are configuration and not in scope here.
- PR mode, `gh` ready, no `CLAUDE.md` line overrides the PR closeout (preflight output).

## Decisions

- D1. Track: Standard (Gate 1).
- D2. Scope (Gate 1, E1, answered "7 rows"): exactly these backlog rows —
  IW-001 (`:24`), NT-001 (`:30`), FE-003 (`:16`), FE-004 (`:17`), IW-004 (`:26`),
  DA-001 (`:5`), DA-002 (`:6`). Each row's own Verify step is its acceptance criterion's basis.
- D3. Build/test unblock (IW-001, NT-001) comes first; every other task's verification depends on it.
- D4. Confidential-client authentication reads `client_secret` from the token request form
  (client_secret_post); HTTP Basic (client_secret_basic) is optional, only if the spec finds it trivial.
  Applies to both the authorization-code exchange and refresh-token rotation.
- D5. Public clients (no secret, PKCE) keep working unchanged; a confidential client that omits or
  mismatches its secret gets `invalid_client` (Unauthorized), checked before any code or refresh
  token is looked up.
- D6. NT-001 runner default: add `Microsoft.NET.Test.Sdk` + `xunit.runner.visualstudio` (VSTest mode)
  to both test projects, unless the spec finds the MTP `global.json` route cleaner; one runner for both.
- D7. Each fix gets a test that fails before it and passes after.
- D8. Out of scope, left in the backlog: FE-002 consent CSRF (`:15`), FE-001 deny redirect (`:14`),
  DA-004 revoke client binding (`:8`), IW-003 token no-store (`:25`), RP-003 CI test gate (`:33`).
- D9. Closed rows are removed from `.sir/backlog.md` at Gate 3.
- D10. Branch `fix/external-sso-readiness`, branch type `fix`.

## Escalations

- E1 (asked inside Gate 1, reason `ambiguous`): "How wide should this unit be?" Options:
  (1) the 7 rows — recommended; (2) 5 rows, unblock + flow only; (3) 10 rows, + FE-002, FE-001, IW-003.
  Answer: the 7 rows. To be recorded in `state.json`'s `escalations` at spec setup.
