# DM Auth

OAuth 2.0 + OpenID Connect Authorization Server built with .NET 9 and React.

DM Auth provides user identity management and enables external client applications to integrate "Sign in with DM Auth" functionality using the Authorization Code + PKCE grant type.

## Overview

- **Protocol**: OAuth 2.0 + OpenID Connect (OIDC)
- **Grant Type**: Authorization Code with PKCE (S256, mandatory for all clients)
- **Token Format**: JWT access tokens (RSA-signed), OIDC ID tokens, database-backed refresh tokens with rotation
- **Architecture**: Clean Architecture with CQRS-lite (MediatR), Domain-Driven Design
- **Frontend**: React SPA (Vite + TypeScript + Tailwind CSS)

## Prerequisites

- [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- [SQL Server](https://www.microsoft.com/en-us/sql-server/) (local instance or Docker)
- [Node.js 18+](https://nodejs.org/) (for the React SPA)
- [Docker](https://www.docker.com/) (optional, for local SQL Server)

## Getting Started

### 1. Clone the Repository

```bash
git clone <repo-url>
cd dm-auth
```

### 2. Start SQL Server

Using Docker:

```bash
docker-compose up -d
```

Or use a local SQL Server instance.

### 3. Configure the API

Update the connection string in `src/DMAuth.Web/appsettings.Development.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=DMAuth;Trusted_Connection=True;TrustServerCertificate=True;"
  }
}
```

### 4. Run Database Migrations

```bash
dotnet ef database update --project src/DMAuth.Infrastructure --startup-project src/DMAuth.Web
```

### 5. Start the API

```bash
dotnet run --project src/DMAuth.Web
```

The API will be available at `https://localhost:5001`. Swagger UI is available at `https://localhost:5001/swagger` in development.

### 6. Start the React SPA

```bash
cd dmauth-web
npm install
npm run dev
```

The SPA will be available at `http://localhost:5173`.

## Project Structure

```
dm-auth/
├── DM-Auth.sln
├── Directory.Build.props              # Shared build settings (net9.0, nullable, implicit usings)
├── docs/
│   ├── work-breakdown.md              # Epic/task tracking for Jira import
│   ├── coding-conventions.md          # C#, React, and testing standards
│   └── architecture-decisions.md      # Architecture Decision Records (ADRs)
├── src/
│   ├── DMAuth.Domain/                 # Entities, value objects, domain services, interfaces
│   ├── DMAuth.Application/            # CQRS commands/queries/handlers, validators, DTOs
│   ├── DMAuth.Infrastructure/         # EF Core, repositories, JWT service, Key Vault
│   └── DMAuth.Web/                    # API controllers, middleware, DI/startup
├── tests/
│   ├── DMAuth.Tests.Unit/             # xUnit unit tests
│   └── DMAuth.Tests.Integration/      # xUnit integration tests (EF Core InMemory)
└── dmauth-web/                        # React SPA (Vite + TypeScript + Tailwind)
```

### Layer Dependencies

```
Web → Application → Domain
Infrastructure → Application → Domain
```

The Domain project has zero external dependencies. Application defines interfaces; Infrastructure implements them.

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

Key settings in `appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=...;Database=DMAuth;..."
  },
  "Jwt": {
    "Issuer": "https://localhost:5001",
    "AccessTokenExpiryMinutes": 15,
    "RefreshTokenExpiryDays": 30,
    "IdTokenExpiryMinutes": 60,
    "SigningKeyPath": "./keys/signing-key.json"
  },
  "AuthorizationCode": {
    "ExpiryMinutes": 5
  },
  "Cors": {
    "AllowedOrigins": ["http://localhost:5173"]
  },
  "KeyVault": {
    "Enabled": false,
    "VaultUri": "https://dmauth-kv.vault.azure.net/"
  }
}
```

For production, set `KeyVault:Enabled` to `true` and configure the vault URI. The signing key will be retrieved from Azure Key Vault instead of the local file.

## Testing

### Unit Tests

```bash
dotnet test tests/DMAuth.Tests.Unit
```

### Integration Tests

Integration tests use EF Core InMemory provider for isolated, parallel-safe testing.

```bash
dotnet test tests/DMAuth.Tests.Integration
```

## Deployment

### Azure Resources Required

- **Azure App Service** — hosts the .NET API
- **Azure SQL Database** — production database
- **Azure Key Vault** — RSA signing key storage and rotation
- **Application Insights** — production logging and telemetry

### Environment Variables (Production)

| Variable | Description |
|----------|-------------|
| `ConnectionStrings__DefaultConnection` | Azure SQL connection string |
| `KeyVault__Enabled` | `true` |
| `KeyVault__VaultUri` | Key Vault URI |
| `Jwt__Issuer` | Production issuer URL |
| `Cors__AllowedOrigins__0` | Production SPA URL |

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
