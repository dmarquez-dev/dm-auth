# DM Auth — Work Breakdown

> Each Epic maps to a Jira Epic. Each Task maps to a Jira Task under that Epic.
> Tasks are scoped to be independently assignable and completable.

## Implementation Order

1. **Epic 1** (Tasks 1.1–1.9) — Foundation; everything depends on this
2. **Epic 2** (Tasks 2.1–2.8) — User registration + login; needed before OAuth flows
3. **Epic 3** (Tasks 3.1–3.4) — Client registration; needed before OAuth flows
4. **Epic 6** (Task 6.1) — Dev signing key; needed before token generation
5. **Epic 4** (Tasks 4.1–4.11) — OAuth flow; the core feature
6. **Epic 5** (Tasks 5.1–5.3) — OIDC discovery, JWKS, UserInfo
7. **Epic 2** (Tasks 2.9–2.10) — Profile management; can parallel with Epic 5
8. **Epic 3** (Tasks 3.5–3.6) — Client CRUD; can parallel with Epic 5
9. **Epic 7** (Tasks 7.1–7.7) — React SPA; after API endpoints stabilize
10. **Epic 8** (Tasks 8.1–8.5) — Comprehensive tests; runs throughout, finalized after APIs
11. **Epic 9** (Tasks 9.1–9.5) — Documentation and DevOps; continuous, finalized last
12. **Epic 6** (Task 6.2) — Azure Key Vault; before production deployment

Unit tests (Tasks 2.11–2.13, 3.7–3.8, 4.12–4.14, 5.4) should be written alongside their respective feature tasks.

---

## Epic 1: Project Foundation and Infrastructure

| ID | Task | Description | Status | Dependencies |
|----|------|-------------|--------|--------------|
| 1.1 | Create .NET solution and project structure | Create `DM-Auth.sln` with four src projects (DMAuth.Domain, DMAuth.Application, DMAuth.Infrastructure, DMAuth.Web) and two test projects (DMAuth.Tests.Unit, DMAuth.Tests.Integration). Add `Directory.Build.props` (net10.0, nullable enable, implicit usings). Configure project references: Web → Application → Domain; Infrastructure → Application → Domain. | TODO | — |
| 1.2 | Add .gitignore and .editorconfig | Add Visual Studio .gitignore (include node_modules, keys/ directory). Add .editorconfig with C# coding conventions (naming rules, formatting, severity). | TODO | — |
| 1.3 | Install NuGet dependencies | Domain: none. Application: MediatR, FluentValidation, FluentValidation.DependencyInjectionExtensions. Infrastructure: EF Core (SqlServer + Tools), BCrypt.Net-Next, System.IdentityModel.Tokens.Jwt, Microsoft.IdentityModel.Tokens. Web: Serilog (+ Sinks.Console, Sinks.File), Swashbuckle.AspNetCore, Microsoft.ApplicationInsights.AspNetCore. Unit Tests: xUnit, Moq or NSubstitute, FluentAssertions. Integration Tests: Microsoft.EntityFrameworkCore.InMemory, Microsoft.AspNetCore.Mvc.Testing. | TODO | 1.1 |
| 1.4 | Configure application startup and DI | Create `DependencyInjection.cs` in Application (register MediatR, FluentValidation, pipeline behaviors). Create `DependencyInjection.cs` in Infrastructure (register DbContext, repositories, services). Configure `Program.cs` in Web (call AddApplication/AddInfrastructure, set up middleware pipeline). Add CORS policy for React SPA origin. | TODO | 1.1, 1.3 |
| 1.5 | Add application configuration files | Create `appsettings.json` and `appsettings.Development.json` with connection string, JWT settings (issuer, expiry values, signing key path), CORS origins, Key Vault config (disabled for dev), authorization code expiry. | TODO | 1.1 |
| 1.6 | Configure Swagger/OpenAPI | Set up Swashbuckle in Program.cs for development API documentation. Configure Bearer token auth in Swagger UI. | TODO | 1.4 |
| 1.7 | Set up EF Core DbContext and entity configurations | Create `DmAuthDbContext` with DbSets for User, Client, RefreshToken, AuthorizationCode, Consent. Create fluent API entity configurations for each entity (keys, indexes, constraints, value object mappings). Use `NEWSEQUENTIALID()` for Guid PKs. | TODO | 1.3 |
| 1.8 | Generate initial EF Core migration | Create the initial database migration from entity configurations. Add database seeding logic for dev (test user + test client). | TODO | 1.7 |
| 1.9 | Implement cross-cutting concerns | Create `ExceptionHandlingMiddleware` (maps domain exceptions to HTTP status codes, logs errors). Create `Result<T>` type (success/failure discriminated union). Create `ValidationBehavior<TRequest, TResponse>` MediatR pipeline behavior. Configure Serilog for structured logging (console + file sinks for dev). | TODO | 1.4 |

---

## Epic 2: User Account Management

| ID | Task | Description | Status | Dependencies |
|----|------|-------------|--------|--------------|
| 2.1 | Create User entity and value objects | Create `User` aggregate root entity with Id (Guid), Email, Username, HashedPassword, DisplayName, IsActive, EmailVerified, CreatedAt, UpdatedAt. Create `Email` value object (normalized, validated). Create `HashedPassword` value object. Add domain methods: ChangePassword(), UpdateProfile(), VerifyPassword(), Deactivate(). | TODO | 1.1 |
| 2.2 | Implement password policy domain service | Create `PasswordPolicy` domain service enforcing: minimum 8 characters, at least one number, at least one special character. Return descriptive validation errors for each failed rule. | TODO | 1.1 |
| 2.3 | Create User repository interface and implementation | Define `IUserRepository` in Domain (FindByEmail, FindByUsername, FindById, Add, Update, ExistsByEmail, ExistsByUsername). Implement `UserRepository` in Infrastructure using EF Core. | TODO | 2.1, 1.7 |
| 2.4 | Implement password hasher service | Define `IPasswordHasher` interface in Application (Hash, Verify). Implement using BCrypt with work factor 12 in Infrastructure. | TODO | 1.3 |
| 2.5 | Implement user registration command | Create `RegisterUserCommand` (email, username, password, displayName). Create `RegisterUserCommandHandler` (validate uniqueness, hash password, persist user). Create `RegisterUserCommandValidator` (FluentValidation rules). | TODO | 2.1, 2.2, 2.3, 2.4, 1.9 |
| 2.6 | Create user registration API endpoint | Create `UserController` with `POST /api/users/register`. Map command result to HTTP response (201 Created on success, 409 Conflict on duplicate, 400 on validation failure). | TODO | 2.5, 1.4 |
| 2.7 | Implement user login with session cookies | Create `LoginCommand` (email, password) and handler (verify credentials, issue session cookie). Configure ASP.NET Core cookie authentication scheme (HttpOnly, Secure, SameSite=Strict). Create `POST /api/users/login` and `POST /api/users/logout` endpoints. | TODO | 2.3, 2.4, 1.4 |
| 2.8 | Implement current user service | Create `ICurrentUserService` interface in Application (GetUserId, IsAuthenticated). Implement in Web project reading from HttpContext.User claims. | TODO | 2.7 |
| 2.9 | Implement user profile query and update | Create `GetUserProfileQuery` + handler returning `UserProfileDto`. Create `UpdateProfileCommand` + handler (update display name). Create `ChangePasswordCommand` + handler (validate old password, apply policy to new, hash and persist). | TODO | 2.1, 2.2, 2.3, 2.4, 2.8 |
| 2.10 | Create user profile API endpoints | Create `GET /api/users/me`, `PUT /api/users/me`, `POST /api/users/me/change-password` endpoints. All require authentication (cookie auth). | TODO | 2.9 |
| 2.11 | Write user domain unit tests | Test User entity creation and validation, Email value object, HashedPassword value object, PasswordPolicy (all rule combinations). | TODO | 2.1, 2.2 |
| 2.12 | Write user command handler unit tests | Test RegisterUserCommandHandler (success, duplicate email, duplicate username, weak password). Test LoginCommandHandler (success, invalid email, wrong password). Test ChangePasswordCommandHandler (success, wrong old password, weak new password). | TODO | 2.5, 2.7, 2.9 |
| 2.13 | Write user registration integration test | End-to-end test: POST /api/users/register → verify 201, verify user in DB, verify password hashed. Test duplicate registration returns 409. | TODO | 2.6, 8.1 |

---

## Epic 3: Client Application Management

| ID | Task | Description | Status | Dependencies |
|----|------|-------------|--------|--------------|
| 3.1 | Create Client entity and value objects | Create `Client` aggregate root with Id (Guid), ClientId (public string), ClientName, ClientSecretHash, ClientType (enum: Confidential/Public), RedirectUris (List\<RedirectUri\>), AllowedScopes (List\<Scope\>), OwnerId, IsActive, CreatedAt, UpdatedAt. Create `RedirectUri` value object (validates format, rejects fragments, requires HTTPS except localhost). Create `Scope` value object (validates against allowed set: openid, profile, email, offline_access). | TODO | 1.1 |
| 3.2 | Create Client repository interface and implementation | Define `IClientRepository` in Domain (FindById, FindByClientId, FindByOwnerId, Add, Update). Implement in Infrastructure with EF Core, including eager loading of RedirectUris and Scopes. | TODO | 3.1, 1.7 |
| 3.3 | Implement client registration command | Create `RegisterClientCommand` (name, type, redirectUris, scopes) + handler. Generate `ClientId` (prefixed format, e.g., "dma_abc123def456"). Generate `ClientSecret` for confidential clients — return plaintext in response (only time it's shown), store as BCrypt hash. | TODO | 3.1, 3.2, 2.4, 2.8 |
| 3.4 | Create client registration API endpoint | Create `ClientController` with `POST /api/clients`. Requires authenticated user (cookie auth). Response includes client_id and client_secret (confidential only, shown once). | TODO | 3.3, 1.4 |
| 3.5 | Implement client CRUD queries and commands | Create `GetClientByIdQuery` + handler (owner-only access check). Create `GetClientsByOwnerQuery` + handler (returns list of ClientSummaryDto). Create `UpdateClientCommand` + handler (name, redirect URIs, scopes — NOT secret). Create `DeleteClientCommand` + handler (soft delete: sets IsActive=false). | TODO | 3.1, 3.2, 2.8 |
| 3.6 | Create client CRUD API endpoints | Create `GET /api/clients` (list by owner), `GET /api/clients/{id}`, `PUT /api/clients/{id}`, `DELETE /api/clients/{id}`. Enforce owner-only authorization on all endpoints. | TODO | 3.5, 1.4 |
| 3.7 | Write client domain and handler unit tests | Test Client entity creation, RedirectUri validation (reject fragments, require HTTPS), Scope validation. Test RegisterClientCommandHandler, UpdateClientCommandHandler, DeleteClientCommandHandler. | TODO | 3.1, 3.3, 3.5 |
| 3.8 | Write client management integration tests | End-to-end: register client → list → update → delete. Verify owner-only access (403 for non-owners). | TODO | 3.4, 3.6, 8.1 |

---

## Epic 4: OAuth 2.0 Authorization Code Flow + PKCE

| ID | Task | Description | Status | Dependencies |
|----|------|-------------|--------|--------------|
| 4.1 | Implement authorization endpoint validation | Create `AuthorizeCommand` to validate request params: client_id (exists, active), redirect_uri (exact match against registered URIs), response_type=code, scope (subset of client's allowed scopes), state (required), code_challenge + code_challenge_method=S256 (required — PKCE mandatory). Return descriptive errors for each invalid param. | TODO | 3.2 |
| 4.2 | Implement authorization flow orchestration | Create `GET /connect/authorize` in `AuthController`. If user not authenticated → redirect to login page with return URL. If authenticated → check existing consent via `GetAuthorizationDetailsQuery`. If consent covers requested scopes → skip consent, generate code, redirect. If insufficient consent → redirect to consent page. | TODO | 4.1, 4.3, 2.7 |
| 4.3 | Create Consent entity and repository | Create `Consent` entity (Id, UserId, ClientId, GrantedScopes, GrantedAt). Unique constraint on (UserId, ClientId). Create `IConsentRepository` (FindByUserAndClient, Add, Update). Implement repository in Infrastructure. | TODO | 1.7 |
| 4.4 | Create AuthorizationCode entity and repository | Create `AuthorizationCode` entity (Id, CodeHash, UserId, ClientId, RedirectUri, Scopes, CodeChallenge, CodeChallengeMethod, ExpiresAt, UsedAt, CreatedAt). Store code as SHA-256 hash. 5-minute expiry. Create `IAuthorizationCodeRepository` (FindByCodeHash, Add, Update). | TODO | 1.7 |
| 4.5 | Implement consent grant and code generation | Create `GrantConsentCommand` + handler: store/update consent record, generate cryptographically random authorization code, hash and persist code, redirect to redirect_uri?code=CODE&state=STATE. | TODO | 4.3, 4.4 |
| 4.6 | Implement token endpoint — authorization code exchange | Create `ExchangeTokenCommand` for grant_type=authorization_code. Validate: code exists, not expired, UsedAt is null, matches client_id and redirect_uri. Validate PKCE: compute SHA256(code_verifier), compare to stored code_challenge. Set UsedAt timestamp. If code reuse detected (UsedAt already set), revoke all tokens issued for that authorization. | TODO | 4.4, 4.7, 4.8 |
| 4.7 | Implement TokenService — JWT generation | Create `ITokenService` interface (GenerateAccessToken, GenerateIdToken, GenerateRefreshToken). Implement JWT access tokens signed with RSA key. Access token claims: sub, iss, aud, exp, iat, scope, client_id. ID token claims per OIDC: sub, iss, aud, exp, iat, auth_time, nonce (if provided), plus profile/email claims based on granted scopes. | TODO | 6.1 |
| 4.8 | Implement refresh token generation and storage | Generate cryptographically random opaque refresh tokens. Store as SHA-256 hash in RefreshTokens table with UserId, ClientId, ExpiresAt (30 days). Create `IRefreshTokenRepository` (FindByTokenHash, FindByUserId, Add, Update, RevokeByTokenFamily). | TODO | 1.7 |
| 4.9 | Create POST /connect/token endpoint | Create token endpoint in AuthController. Route by grant_type: "authorization_code" → ExchangeTokenCommand, "refresh_token" → RefreshTokenCommand. Return JSON: access_token, id_token, refresh_token, token_type="Bearer", expires_in. | TODO | 4.6, 4.10 |
| 4.10 | Implement refresh token rotation | Create `RefreshTokenCommand` + handler. Validate: token exists (by hash), not expired, not revoked. Issue new access + refresh tokens. Revoke old refresh token, link via ReplacedByToken. Detect reuse: if revoked token presented, revoke entire token family (all tokens linked via ReplacedByToken chain). | TODO | 4.7, 4.8 |
| 4.11 | Implement token revocation endpoint | Create `RevokeTokenCommand` + handler. Accept refresh token, find by hash, set RevokedAt. Create `POST /connect/revoke` per RFC 7009. Always return 200 OK regardless of whether token was found (prevents enumeration). | TODO | 4.8 |
| 4.12 | Write authorization flow unit tests | Test AuthorizeCommand validation (invalid client, bad redirect URI, missing PKCE, invalid scope). Test GrantConsentCommandHandler (code generation, consent storage). Test ExchangeTokenCommandHandler (valid exchange, expired code, PKCE failure, code reuse). | TODO | 4.1, 4.5, 4.6 |
| 4.13 | Write token flow unit tests | Test TokenService (JWT claims, expiry, signing). Test RefreshTokenCommandHandler (rotation, reuse detection). Test RevokeTokenCommandHandler. Test PKCE S256 challenge/verifier computation. | TODO | 4.7, 4.10, 4.11 |
| 4.14 | Write full OAuth flow integration test | End-to-end: authorize request → login → consent → code exchange → receive tokens → refresh → revoke. Verify tokens are valid JWTs with correct claims. Verify refresh rotation issues new tokens. | TODO | 4.9, 4.11, 8.1 |

---

## Epic 5: OIDC Discovery and UserInfo

| ID | Task | Description | Status | Dependencies |
|----|------|-------------|--------|--------------|
| 5.1 | Implement OIDC discovery endpoint | Create `GET /.well-known/openid-configuration` in DiscoveryController. Return JSON with: issuer, authorization_endpoint, token_endpoint, userinfo_endpoint, jwks_uri, revocation_endpoint, response_types_supported (["code"]), grant_types_supported (["authorization_code", "refresh_token"]), subject_types_supported (["public"]), id_token_signing_alg_values_supported (["RS256"]), scopes_supported, token_endpoint_auth_methods_supported, code_challenge_methods_supported (["S256"]). | TODO | 1.4 |
| 5.2 | Implement JWKS endpoint | Create `GET /.well-known/jwks.json`. Return public RSA key(s) in JWK format (kid, kty, use=sig, alg=RS256, n, e). Support multiple keys for rotation (tokens include kid header). Cache response. | TODO | 6.1 |
| 5.3 | Implement UserInfo endpoint | Create `GetUserInfoQuery` + handler. Create `GET /connect/userinfo` requiring Bearer access token. Validate token, extract sub claim. Return claims based on token's scope: openid → sub; profile → name, preferred_username; email → email, email_verified. | TODO | 4.7, 2.3 |
| 5.4 | Write OIDC endpoint tests | Test discovery document contains all required fields per OIDC spec. Test JWKS returns valid JWK. Test UserInfo returns correct claims per scope. Test UserInfo rejects invalid/expired tokens. | TODO | 5.1, 5.2, 5.3 |

---

## Epic 6: JWT Signing Key Management

| ID | Task | Description | Status | Dependencies |
|----|------|-------------|--------|--------------|
| 6.1 | Implement development key management | On first startup (dev only), auto-generate RSA 2048-bit key pair if not present. Store in local JSON file (keys/ directory, gitignored). Load at startup for token signing and JWKS endpoint. | TODO | 1.4 |
| 6.2 | Implement Azure Key Vault integration | Create `KeyVaultService` to retrieve RSA signing key from Azure Key Vault. Cache key in memory with configurable TTL. Support key rotation via Key Vault key versioning. Configuration toggle: use local key (dev) vs Key Vault (prod) based on `KeyVault:Enabled` setting. | TODO | 6.1, 1.3 |

---

## Epic 7: React SPA — DM Auth Dashboard

| ID | Task | Description | Status | Dependencies |
|----|------|-------------|--------|--------------|
| 7.1 | Initialize React project | Scaffold Vite + React + TypeScript project in `dmauth-web/`. Install dependencies: react-router-dom, @tanstack/react-query, axios, tailwindcss, react-hook-form, @hookform/resolvers, zod. Configure Tailwind, Vite proxy to backend API. | TODO | — |
| 7.2 | Set up auth context and routing | Create AuthProvider context (manages login state, user info). Create useAuth hook. Create ProtectedRoute component (redirects to /login if unauthenticated). Set up React Router with public routes (/login, /register) and protected routes (/dashboard, /profile, /clients). | TODO | 7.1 |
| 7.3 | Create base layout and API client | Build Layout component (Navbar with user menu, main content area). Create Axios instance configured with base URL and withCredentials for cookie auth. Create typed API modules (authApi, userApi, clientApi). | TODO | 7.1 |
| 7.4 | Build login and registration pages | LoginPage: email/password form, error display, redirect support (returnUrl for OAuth flow). RegisterPage: email, username, password, confirm password, password strength meter component. Both use react-hook-form + zod validation. | TODO | 7.2, 7.3 |
| 7.5 | Build OAuth consent page | ConsentPage: displays client name, client logo placeholder, list of requested scopes with human-readable descriptions, Approve/Deny buttons. Receives auth request params from URL. On approve → calls GrantConsent API → redirects with code. On deny → redirects with error=access_denied. | TODO | 7.2, 7.3 |
| 7.6 | Build dashboard and profile pages | DashboardPage: welcome message, quick links to clients and profile. ProfilePage: display name edit form, password change form (current password, new password, confirm). | TODO | 7.2, 7.3 |
| 7.7 | Build client management pages | ClientListPage: table of user's registered clients, "Register New Client" button. ClientCreatePage: form with name, type (public/confidential), redirect URIs (multi-input), scopes (checkboxes). ClientDetailPage: view/edit details, display client_id, regenerate secret option. Modal to show client_secret once on creation. | TODO | 7.2, 7.3 |

---

## Epic 8: Comprehensive Testing

| ID | Task | Description | Status | Dependencies |
|----|------|-------------|--------|--------------|
| 8.1 | Set up test infrastructure | Create `TestDatabaseFixture` with EF Core InMemory provider (isolated per test class). Create custom `WebApplicationFactory<Program>` for integration tests (override DI to use InMemory DB, mock KeyVault). Add shared test helpers (user creation, client registration, token generation). | TODO | 1.7, 1.4 |
| 8.2 | Write domain entity and value object unit tests | User: creation, validation, ChangePassword, UpdateProfile. Client: creation, RedirectUri validation, scope validation. Value objects: Email normalization/validation, RedirectUri format rules, Scope allowed values. PasswordPolicy: all rule combinations. | TODO | 2.1, 2.2, 3.1 |
| 8.3 | Write command handler unit tests | All command handlers with mocked repositories. Cover success paths, validation failures, business rule violations, and edge cases. Target handlers: RegisterUser, Login, UpdateProfile, ChangePassword, RegisterClient, UpdateClient, DeleteClient, Authorize, GrantConsent, ExchangeToken, RefreshToken, RevokeToken. | TODO | 2.5, 2.7, 2.9, 3.3, 3.5, 4.1, 4.5, 4.6, 4.10, 4.11 |
| 8.4 | Write OAuth integration tests | Full authorization code flow (authorize → login → consent → token exchange). Refresh token rotation and reuse detection. Token revocation. Invalid PKCE rejection. Expired code rejection. Scope filtering on UserInfo. | TODO | 8.1, 4.9, 5.3 |
| 8.5 | Write API endpoint integration tests | Client registration and CRUD with owner-only enforcement. User registration (success + duplicate). Discovery document validation. JWKS endpoint validation. | TODO | 8.1, 2.6, 3.4, 3.6, 5.1, 5.2 |

---

## Epic 9: Documentation and DevOps

| ID | Task | Description | Status | Dependencies |
|----|------|-------------|--------|--------------|
| 9.1 | Write README.md | Overview, prerequisites (.NET 10, SQL Server/Docker, Node.js 18+), local dev setup steps, configuration reference, architecture overview, API reference (link to Swagger), OAuth flow explanation with diagram, testing instructions, deployment guide (Azure App Service, SQL Database, Key Vault, App Insights), security considerations, roadmap (MFA, external IdPs, rate limiting, email verification). | DONE | — |
| 9.2 | Create docs/work-breakdown.md | Port the full Epic → Task breakdown into a standalone document for project tracking. Include task IDs, descriptions, status tracking columns, and dependency notes. | DONE | — |
| 9.3 | Create docs/coding-conventions.md | Document C# conventions (naming, file organization, CQRS patterns, Result\<T\> usage). Document React conventions (component structure, state management, API integration patterns). Document testing conventions (naming, arrangement, assertion style). | TODO | — |
| 9.4 | Create architecture decision records | Create `docs/architecture-decisions.md` with ADRs: 001-cqrs-lite (why CQRS for structure not separate DBs), 002-jwt-access-tokens (why JWTs over opaque), 003-pkce-mandatory (why PKCE for all clients), 004-bcrypt-passwords (why BCrypt, work factor choice), 005-refresh-token-rotation (rotation + reuse detection rationale). | DONE | — |
| 9.5 | Create local development Docker setup | Create `docker-compose.yml` with SQL Server container (mcr.microsoft.com/mssql/server). Add database initialization script. Document setup steps in README. | TODO | — |
