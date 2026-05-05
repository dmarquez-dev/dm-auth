# DM Auth

OAuth 2.0 + OpenID Connect Authorization Server built with .NET 10 and React.

DM Auth provides user identity management and enables external client applications to integrate "Sign in with DM Auth" functionality using the Authorization Code + PKCE grant type.

## Overview

- **Protocol**: OAuth 2.0 + OpenID Connect (OIDC)
- **Grant Type**: Authorization Code with PKCE (S256, mandatory for all clients)
- **Token Format**: JWT access tokens (RSA-signed), OIDC ID tokens, database-backed refresh tokens with rotation
- **Architecture**: Clean Architecture with CQRS-lite (MediatR), Domain-Driven Design
- **Frontend**: React SPA (Vite + TypeScript + Tailwind CSS)

## Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- [PowerShell 7.1+](https://learn.microsoft.com/en-us/powershell/scripting/install/installing-powershell) — required by `setup.ps1` (`??` operator); the `SqlServer` module is installed automatically on first run
- [Azure CLI](https://learn.microsoft.com/en-us/cli/azure/install-azure-cli) — for Key Vault secret access in development
- [Node.js 18+](https://nodejs.org/) (for the React SPA)
- [Docker](https://www.docker.com/) (optional, for local SQL Server)

## Getting Started

### Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0) — verify with `dotnet --version` (should show `10.0.x`)
- [PowerShell 7.1+](https://learn.microsoft.com/en-us/powershell/scripting/install/installing-powershell) — verify with `pwsh --version`; the `SqlServer` module is installed automatically by `setup.ps1` on first run
- [Azure CLI](https://learn.microsoft.com/en-us/cli/azure/install-azure-cli) — authenticate with `az login`

### 1. Clone the Repository

```bash
git clone <repo-url>
cd dm-auth
```

### 2. Build the Solution

```bash
dotnet build src/DM-Auth.sln
```

Expected: `Build succeeded. 0 Error(s)`

### 3. Set Up the Development Environment

Choose one of three options based on your setup. Options A and B use the shared Azure dev database. Option C spins up a local SQL Server container — no Azure access required.

#### Option A — Setup script (recommended for team members)

The script verifies Azure CLI authentication, retrieves the DB connection string from Key Vault, applies the EF Core migration, and seeds the database.

```powershell
.\eng\dev\setup.ps1 -VaultName "dmauth-dev"
```

#### Option B — Manual steps

**a. Verify Azure CLI authentication**

```bash
az login
az account show
```

**b. Apply the migration**

```bash
CS=$(az keyvault secret show --vault-name dmauth-dev --name "ConnectionStrings--DmAuth" --query "value" -o tsv)
dotnet ef database update \
  --project src/DMAuth.Infrastructure \
  --startup-project src/DMAuth.Web \
  --connection "$CS"
```

**c. Seed the database**

```bash
sqlcmd -S <server> -d DMAuth -E -i eng/dev/seed-data.sql
```

This inserts a test user (`test@example.com` / `testpassword123`) and a public test client (`test_client`).

#### Option C — Local Docker (no Azure access required)

Use this if you don't have access to the dev Azure environment or want a fully isolated local database.

**Prerequisites:** [Docker Desktop](https://www.docker.com/products/docker-desktop/)

**a. Start the SQL Server container**

```bash
SA_PASSWORD=YourStrong!Pass123 docker compose -f eng/dev/docker-compose.yml up -d
```

The container exposes SQL Server on `localhost,1433`. Data is persisted in the `dmauth-db-data` Docker volume across restarts.

**b. Configure the connection string via user secrets**

```bash
cd src/DMAuth.Web
dotnet user-secrets set "ConnectionStrings:DmAuthConnection" \
  "Server=localhost,1433;Database=DMAuth;User Id=sa;Password=YourStrong!Pass123;TrustServerCertificate=True"
```

> Use the same password you passed to `SA_PASSWORD` above. The password must meet SQL Server complexity requirements: at least 8 characters, with uppercase, lowercase, digit, and special character.

**c. Apply migrations**

```bash
dotnet ef database update \
  --project src/DMAuth.Infrastructure \
  --startup-project src/DMAuth.Web
```

**d. (Optional) Seed the database**

```bash
sqlcmd -S localhost,1433 -U sa -P YourStrong!Pass123 -d DMAuth -i eng/dev/seed-data.sql
```

**Teardown:**

```bash
# Stop but keep data
docker compose -f eng/dev/docker-compose.yml down

# Stop and wipe the database volume
docker compose -f eng/dev/docker-compose.yml down -v
```

### 4. Start the API

```bash
dotnet run --project src/DMAuth.Web --launch-profile https
```

The API will be available at `https://localhost:7259`. Swagger UI is available at `https://localhost:7259/swagger` in development.

## Project Structure

```
dm-auth/
├── .editorconfig                      # C# and TypeScript code style rules
├── Directory.Build.props              # Shared build settings (net10.0, nullable, implicit usings)
├── docs/
│   ├── dotnet-guidelines.md           # ASP.NET Core best practices (async, EF Core, HttpContext)
│   ├── dotnet-format.md               # C# naming, formatting, and code style rules
│   ├── backend-conventions.md         # Domain patterns: project structure, CQRS, DDD, testing
│   ├── react-guidelines.md            # React performance and best practices
│   ├── react-format.md                # TypeScript/React naming, exports, and style rules
│   ├── frontend-conventions.md        # Domain patterns: state management, API client, forms
│   ├── vitest-guidelines.md           # Vitest unit/component testing conventions
│   ├── playwright-guidelines.md       # Playwright e2e testing conventions
│   └── architecture-decisions.md      # Architecture Decision Records (ADRs)
├── eng/
│   └── dev/
│       ├── docker-compose.yml         # Local SQL Server for contributors
│       └── setup.ps1                  # Dev environment setup script
├── src/
│   ├── DM-Auth.sln
│   ├── DMAuth.Domain/                 # Entities, value objects, domain services, interfaces
│   ├── DMAuth.Application/            # CQRS commands/queries/handlers, validators, DTOs
│   ├── DMAuth.Infrastructure/         # EF Core, repositories, JWT service, Key Vault
│   ├── DMAuth.Web/                    # API controllers, middleware, DI/startup
│   └── DMAuth.Client/                 # React SPA (Vite + TypeScript + Tailwind)
│       ├── src/                       # Application source
│       │   ├── api/                   # Axios API clients + unit tests
│       │   ├── auth/                  # AuthProvider, ProtectedRoute, useAuth
│       │   ├── pages/                 # Route-level page components + component tests
│       │   └── types/                 # Shared TypeScript types
│       └── tests/
│           └── e2e/                   # Playwright end-to-end tests
│               ├── fixtures/          # Custom test fixtures (freshUser)
│               ├── auth.setup.ts      # Global auth setup (saves storageState)
│               ├── auth.spec.ts       # Registration, login, logout flows
│               ├── clients.spec.ts    # Client CRUD flows
│               ├── consent.spec.ts    # OAuth consent flows
│               └── profile.spec.ts    # Profile update flows
└── tests/
    ├── DMAuth.Tests.Unit/             # xUnit v3 unit tests
    └── DMAuth.Tests.Integration/      # xUnit v3 integration tests (EF Core InMemory + WebApplicationFactory)
```

### Layer Dependencies

```
Application → Domain
Infrastructure → Domain
Web → Application, Infrastructure
```

The Domain project has zero external dependencies. Application contains business logic and service implementations (TokenService, PasswordHasher). Infrastructure handles data persistence and external service integrations (EF Core, Key Vault). Web is the composition root that wires everything together via DI.

## Architecture

### CQRS-Lite

The project uses CQRS for structural organization, not for separate read/write databases. MediatR dispatches commands and queries through a pipeline with FluentValidation. Each feature is organized into its own folder:

```
Application/Features/{Feature}/
  Commands/{CommandName}/
    {CommandName}Command.cs
    {CommandName}CommandHandler.cs
    {CommandName}CommandValidator.cs
  Queries/{QueryName}/
    {QueryName}Query.cs
    {QueryName}QueryHandler.cs
    {QueryName}Dto.cs
```

See [docs/architecture-decisions/001-cqrs-lite.md](docs/architecture-decisions/001-cqrs-lite.md) for the full rationale.

### Domain Model

| Entity | Role |
|--------|------|
| **User** | Aggregate root. Manages identity, credentials, and profile. |
| **Client** | Aggregate root. Represents an OAuth client application registered with DM Auth. |
| **RefreshToken** | Tracks issued refresh tokens with rotation and revocation. |
| **AuthorizationCode** | Short-lived code for the OAuth authorization flow. |
| **Consent** | Records user consent per client per scope. |

All entity IDs use `Guid` (UNIQUEIDENTIFIER) with `NEWSEQUENTIALID()` for clustered index performance.

## API Reference

Swagger UI is available at `/swagger` in development. Key endpoints:

### OAuth 2.0 / OIDC

| Method | Path | Description |
|--------|------|-------------|
| GET | `/.well-known/openid-configuration` | OIDC discovery document |
| GET | `/.well-known/jwks.json` | JSON Web Key Set (public signing keys) |
| GET | `/connect/authorize` | Authorization endpoint |
| POST | `/connect/token` | Token endpoint (code exchange, refresh) |
| POST | `/connect/revoke` | Token revocation (RFC 7009) |
| GET | `/connect/userinfo` | OIDC UserInfo endpoint |

### User Management

| Method | Path | Description |
|--------|------|-------------|
| POST | `/api/users/register` | Register a new account |
| POST | `/api/users/login` | Login (sets session cookie) |
| POST | `/api/users/logout` | Logout |
| GET | `/api/users/me` | Get current user profile |
| PUT | `/api/users/me` | Update profile |
| POST | `/api/users/me/change-password` | Change password |

### Client Management

| Method | Path | Description |
|--------|------|-------------|
| GET | `/api/clients` | List your registered clients |
| POST | `/api/clients` | Register a new OAuth client |
| GET | `/api/clients/{id}` | Get client details |
| PUT | `/api/clients/{id}` | Update client |
| DELETE | `/api/clients/{id}` | Deactivate client |

## OAuth 2.0 Flow

DM Auth implements the Authorization Code flow with PKCE (RFC 7636):

```
┌──────────┐                              ┌──────────┐                              ┌──────────┐
│  Client   │                              │  DM Auth │                              │  User    │
│  App      │                              │  Server  │                              │  Browser │
└─────┬────┘                              └─────┬────┘                              └─────┬────┘
      │                                         │                                         │
      │  1. Generate code_verifier +             │                                         │
      │     code_challenge = SHA256(verifier)    │                                         │
      │                                         │                                         │
      │  2. Redirect to /connect/authorize ───────────────────────────────────────────────>│
      │     ?response_type=code                  │                                         │
      │     &client_id=X                         │                                         │
      │     &redirect_uri=Y                      │                                         │
      │     &scope=openid profile email          │                                         │
      │     &state=Z                             │                                         │
      │     &code_challenge=CC                   │                                         │
      │     &code_challenge_method=S256          │                                         │
      │                                         │                                         │
      │                                         │  3. Show login page (if not logged in) ──>│
      │                                         │                                         │
      │                                         │  4. User authenticates <─────────────────│
      │                                         │                                         │
      │                                         │  5. Show consent screen ────────────────>│
      │                                         │     (requested scopes, client name)      │
      │                                         │                                         │
      │                                         │  6. User grants consent <────────────────│
      │                                         │                                         │
      │  7. Redirect to redirect_uri <───────────────────────────────────────────────────── │
      │     ?code=AUTH_CODE&state=Z              │                                         │
      │                                         │                                         │
      │  8. POST /connect/token ────────────────>│                                         │
      │     grant_type=authorization_code        │                                         │
      │     &code=AUTH_CODE                      │                                         │
      │     &redirect_uri=Y                      │                                         │
      │     &client_id=X                         │                                         │
      │     &code_verifier=CV                    │                                         │
      │                                         │                                         │
      │  9. Receive tokens <─────────────────────│                                         │
      │     { access_token, id_token,            │                                         │
      │       refresh_token, expires_in }        │                                         │
      │                                         │                                         │
```

### Supported Scopes

| Scope | Claims Returned |
|-------|----------------|
| `openid` | `sub` (required for OIDC) |
| `profile` | `name`, `preferred_username` |
| `email` | `email`, `email_verified` |
| `offline_access` | Enables refresh tokens |

## Configuration

All sensitive secrets are stored in Azure Key Vault using the `--` double-dash naming convention, which maps directly onto the ASP.NET Core `IConfiguration` hierarchy (e.g. `ConnectionStrings--DmAuth` → `ConnectionStrings:DmAuth`).

### Key Vault Secrets

| Key Vault secret name | Maps to `IConfiguration` key | Description |
|---|---|---|
| `Jwt--RsaPrivateKeyPem` | `Jwt:RsaPrivateKeyPem` | RSA 2048-bit private key (PEM-encoded) used to sign JWTs |
| `ConnectionStrings--DmAuth` | `ConnectionStrings:DmAuth` | Azure SQL connection string |
| `ConnectionStrings--ApplicationInsights` | `ConnectionStrings:ApplicationInsights` | Application Insights connection string |

### Non-secret settings

The only non-secret Key Vault setting is `KeyVault:VaultUri`, which tells the app where to find its vault. Set this in `appsettings.Development.json` per-environment and override it in production via an App Service Application Setting (`KeyVault__VaultUri`).

Non-sensitive settings kept in `appsettings.json`:

```json
{
  "Jwt": {
    "Issuer": "",
    "Audience": "",
    "AccessTokenExpiryMinutes": 15,
    "IdTokenExpiryMinutes": 60
  },
  "AuthorizationCode": {
    "ExpiryMinutes": 5
  },
  "Cors": {
    "AllowedOrigins": []
  },
  "KeyVault": {
    "VaultUri": ""
  }
}
```

## Testing

The project has three test tiers:

| Tier | Stack | Location | Requires live API? |
|------|-------|----------|--------------------|
| .NET unit | xUnit v3 + FluentAssertions + NSubstitute | `tests/DMAuth.Tests.Unit/` | No |
| .NET integration | xUnit v3 + EF Core InMemory + WebApplicationFactory | `tests/DMAuth.Tests.Integration/` | No |
| Frontend unit/component | Vitest + Testing Library | `src/DMAuth.Client/src/**/*.test.{ts,tsx}` | No |
| Frontend e2e | Playwright (Chromium) | `src/DMAuth.Client/tests/e2e/` | Yes |

---

### .NET Unit Tests

Tests domain logic, application handlers, and services in complete isolation using NSubstitute mocks.

```bash
dotnet test tests/DMAuth.Tests.Unit
```

**Validating results:** xUnit v3 prints a summary to the terminal (`X passed, 0 failed`). For a detailed breakdown by test class, add `--logger "console;verbosity=detailed"`.

---

### .NET Integration Tests

Tests HTTP endpoints and the full request pipeline using `WebApplicationFactory` with an EF Core InMemory database and a test RSA key. No external dependencies (SQL Server, Key Vault) required.

```bash
dotnet test tests/DMAuth.Tests.Integration
```

**Code coverage (.NET):**

```bash
dotnet test --collect:"XPlat Code Coverage"
# Coverage reports land in tests/*/TestResults/*/coverage.cobertura.xml
# To generate an HTML report, install reportgenerator first:
dotnet tool install -g dotnet-reportgenerator-globaltool
reportgenerator -reports:"tests/**/coverage.cobertura.xml" -targetdir:"coverage-report" -reporttypes:Html
```

Then open `coverage-report/index.html` in a browser.

---

### Frontend Unit & Component Tests (Vitest)

Tests React components, hooks, and API client functions using jsdom and Testing Library. No browser or live API needed.

#### First-time setup

```bash
cd src/DMAuth.Client
npm install
```

#### Running tests

```bash
# Single run (CI-style)
npm test

# Watch mode (re-runs on file save)
npm run test:watch

# With coverage report
npm run test:coverage
```

**Validating results:** Vitest prints a per-file summary. Pass/fail counts appear at the bottom:

```
Test Files  9 passed (9)
     Tests  43 passed (43)
```

**Coverage report:** After `npm run test:coverage`, open `src/DMAuth.Client/coverage/index.html`. Aim for ≥ 80 % statement coverage on `src/api/` and `src/pages/`.

See [docs/vitest-guidelines.md](docs/vitest-guidelines.md) for conventions, patterns, and anti-patterns used in this project.

---

### Frontend End-to-End Tests (Playwright)

Drives a real Chromium browser against the full running stack (Vite dev server + .NET API). Tests register/login, client CRUD, the OAuth consent flow, and profile management.

#### Prerequisites

1. **Node packages** — `cd src/DMAuth.Client && npm install` (installs `@playwright/test`).
2. **Chromium browser** — run once after installing:
   ```bash
   npx playwright install chromium
   ```
3. **Running .NET API** — start the backend before running e2e tests:
   ```bash
   dotnet run --project src/DMAuth.Web --launch-profile https
   ```
   The API must be reachable at `https://localhost:7259` (the default Vite proxy target).

#### Running tests

All commands run from `src/DMAuth.Client/`:

```bash
# Headless run (all tests)
npm run e2e

# Interactive UI mode — step through tests, inspect trace timeline
npm run e2e:ui

# Single spec file
npx playwright test tests/e2e/auth.spec.ts

# Keep the browser visible (headed mode)
npx playwright test --headed
```

**Validating results:** After a run, Playwright generates an HTML report:

```bash
npx playwright show-report
```

The report opens in your browser and shows pass/fail per test, screenshots on failure, and execution traces. Traces can be opened with the Playwright Trace Viewer for a step-by-step replay of what the browser did.

#### How auth is handled

On the first run, the `setup` project registers a shared e2e test account (`e2e-shared@dmauth.test`) and stores the session in `tests/e2e/.auth/user.json` (gitignored). All subsequent tests in the `chromium` project start pre-authenticated using that stored session, so they never repeat the login flow. Tests in `auth.spec.ts` override this with `test.use({ storageState: { cookies: [], origins: [] } })` so they run unauthenticated. The `profile.spec.ts` tests use a `freshUser` fixture that registers a new unique user per test for full isolation.

#### Overriding the base URL

If the Vite dev server runs on a different port, set `PLAYWRIGHT_BASE_URL`:

```bash
PLAYWRIGHT_BASE_URL=http://localhost:3000 npm run e2e
```

See [docs/playwright-guidelines.md](docs/playwright-guidelines.md) for conventions, fixture patterns, and CI setup guidance.

## Deployment

### Azure Resources Required

- **Azure App Service** — hosts the .NET API
- **Azure SQL Database** — production database
- **Azure Key Vault** — RSA signing key storage and rotation
- **Application Insights** — production logging and telemetry

### Environment Variables (Production)

All secrets are sourced from Key Vault at startup via `DefaultAzureCredential` (Managed Identity). The only App Service Application Setting needed to bootstrap the vault is:

| Variable | Description |
|----------|-------------|
| `KeyVault__VaultUri` | Production Key Vault URI (e.g. `https://dmauth-prod.vault.azure.net/`) |
| `Jwt__Issuer` | Production issuer URL |
| `Jwt__Audience` | Expected audience for access tokens |
| `Cors__AllowedOrigins__0` | Production SPA URL |

Ensure the App Service's system-assigned Managed Identity has the `Key Vault Secrets User` role on the production vault, and that the vault contains all three secrets listed in the [Key Vault Secrets](#key-vault-secrets) table above.

## Security

- **PKCE mandatory** for all clients (S256 only) — prevents authorization code interception
- **Authorization codes stored as SHA-256 hashes** — codes cannot be extracted from a database breach
- **Refresh tokens stored as SHA-256 hashes** — same protection as auth codes
- **Refresh token rotation enforced** — each refresh issues a new token and revokes the old one
- **Refresh token reuse detection** — if a revoked token is presented, the entire token family is revoked (indicates theft)
- **Passwords hashed with BCrypt** (work factor 12) — industry standard for password storage
- **Password policy** — minimum 8 characters, at least one number, at least one special character
- **Short-lived access tokens** (15 minutes) — limits window of misuse
- **Redirect URI exact match** — no wildcards or partial matches
- **Client secrets shown once** — displayed only at creation, never retrievable again
- **Session cookies** — HttpOnly, Secure, SameSite=Strict for the dashboard

See the [Architecture Decision Records](docs/architecture-decisions.md) for detailed rationale behind each security decision.

## Commit Message Convention

Commit messages follow the format:

```
type(scope) Summary

Optional body with additional context, reasoning, or details.
```

### Types

| Type | Usage |
|------|-------|
| `feat` | New feature or capability |
| `fix` | Bug fix |
| `refactor` | Code restructuring with no behavior change |
| `docs` | Documentation only |
| `test` | Adding or updating tests |
| `chore` | Build config, dependencies, tooling, CI/CD |
| `style` | Formatting, whitespace, naming (no logic change) |

### Scopes

Scope identifies the area of the codebase affected. Examples:

| Scope | Area |
|-------|------|
| `domain` | DMAuth.Domain (entities, value objects, domain services) |
| `application` | DMAuth.Application (commands, queries, handlers) |
| `infrastructure` | DMAuth.Infrastructure (EF Core, repositories, services) |
| `web` | DMAuth.Web (controllers, middleware, startup) |
| `spa` | dmauth-web React SPA |
| `auth` | OAuth/OIDC flow (cross-cutting across layers) |
| `users` | User account management feature |
| `clients` | Client application management feature |
| `tokens` | Token generation, refresh, revocation |
| `architecture` | Solution structure, project scaffolding |
| `testing` | Test infrastructure, fixtures, helpers |
| `deps` | Dependency changes |

### Examples

```
feat(architecture) Scaffolded .NET solution structure
feat(users) Implemented user registration command and endpoint
feat(auth) Added authorization code exchange with PKCE validation
fix(tokens) Fixed refresh token reuse detection skipping revoked families
refactor(application) Extracted shared validation behavior into pipeline
docs(architecture) Added ADRs for CQRS-lite and JWT decisions
test(auth) Added integration tests for full OAuth authorization flow
chore(deps) Updated MediatR to v13
style(domain) Applied primary constructor convention to entities
```

## Roadmap

- [ ] Multi-factor authentication (TOTP/WebAuthn)
- [ ] External identity providers (Google, GitHub, Microsoft)
- [ ] Rate limiting and throttling
- [ ] Email verification flow
- [ ] Admin dashboard
- [ ] Client Credentials grant type (service-to-service)
- [ ] Breach detection (HaveIBeenPwned integration)
