# DM Auth — Architecture Decision Records

This document captures key architectural decisions, their context, and rationale.

---

## ADR-001: CQRS-Lite for Project Structure

**Status:** Accepted

**Context:**
DM Auth needs a clear, maintainable project structure that organizes code by feature and enforces separation between read and write operations. Full CQRS (separate read/write databases, event sourcing, eventual consistency) introduces significant infrastructure complexity that is not justified for this project's scale.

**Decision:**
Use CQRS as a structural pattern only — commands and queries are separated into distinct types with dedicated handlers, dispatched via MediatR. A single shared database serves both reads and writes through the same EF Core DbContext.

**Consequences:**
- Feature-organized folder structure (Features/{Feature}/Commands/ and Queries/) makes the codebase navigable and enforces single-responsibility at the handler level
- MediatR pipeline behaviors (validation, logging) apply uniformly to all commands and queries
- No eventual consistency concerns — reads always reflect the latest writes
- No infrastructure overhead of separate databases, projections, or event stores
- If separate read/write stores become necessary in the future, the command/query separation already in place makes that migration straightforward

**Alternatives Considered:**
- **Full CQRS with event sourcing:** Rejected — adds event store, projections, and eventual consistency complexity without clear benefit at this scale
- **Traditional service layer (no CQRS):** Rejected — leads to bloated service classes mixing read and write logic, harder to maintain as the feature set grows
- **Vertical slice architecture:** Similar in spirit but less structured; CQRS-lite with MediatR provides a more consistent pattern across features

---

## ADR-002: JWT Access Tokens

**Status:** Accepted

**Context:**
Access tokens are issued to client applications to authorize API requests. The two primary options are self-contained JWTs (verifiable without calling back to the auth server) and opaque tokens (require an introspection endpoint call for every validation).

**Decision:**
Use JWTs (RS256-signed) for access tokens. OIDC ID tokens are JWTs by specification.

**Consequences:**
- Client applications and resource servers can validate tokens locally by fetching the public key from the JWKS endpoint — no per-request call to the auth server
- Better latency and availability for downstream services
- Token contents (claims) are readable by clients, so no sensitive PII should be included beyond what the granted scopes permit
- JWTs cannot be revoked before expiry once issued — mitigated by short expiry (15 minutes) combined with refresh token rotation
- Token size is larger than opaque tokens (~800 bytes vs ~32 bytes) but negligible for HTTP headers

**Alternatives Considered:**
- **Opaque tokens with introspection:** Rejected — every API call would require a round-trip to the auth server, creating a single point of failure and adding latency. The revocation benefit is marginal given short-lived JWTs with refresh rotation.
- **Encrypted JWTs (JWE):** Not needed at MVP — claim contents are non-sensitive (sub, email, name). Can be added later if claim confidentiality becomes a requirement.

---

## ADR-003: Mandatory PKCE for All Clients

**Status:** Accepted

**Context:**
PKCE (Proof Key for Code Exchange, RFC 7636) was originally designed to protect public clients (SPAs, mobile apps) from authorization code interception attacks. OAuth 2.1 draft makes PKCE mandatory for all clients, including confidential ones.

**Decision:**
Require PKCE (S256 method only) for all authorization requests, regardless of client type. Requests without `code_challenge` and `code_challenge_method=S256` are rejected.

**Consequences:**
- Defense in depth: protects against authorization code interception even for confidential clients where the code could be leaked through logs, referrer headers, or browser history
- Aligns with OAuth 2.1 draft and current best practice (RFC 9126)
- Simplifies server logic — one code path for all clients rather than conditional PKCE validation
- The `plain` code challenge method is not supported (S256 only) — `plain` offers no security benefit over no PKCE
- All client applications must implement PKCE, which is a trivial addition to any modern OAuth library

**Alternatives Considered:**
- **PKCE optional for confidential clients:** Rejected — reduces security posture without meaningful developer convenience benefit, as all modern OAuth libraries support PKCE out of the box
- **Support both `plain` and `S256` methods:** Rejected — `plain` provides no cryptographic protection and exists only for clients that cannot perform SHA-256, which is not a realistic constraint

---

## ADR-004: BCrypt for Password Hashing

**Status:** Accepted

**Context:**
Password hashing must be resistant to brute-force attacks, including GPU-accelerated cracking. The primary candidates are BCrypt, Argon2, and PBKDF2.

**Decision:**
Use BCrypt with a work factor of 12 for all password hashing.

**Consequences:**
- BCrypt is a well-established, widely audited algorithm with proven resistance to GPU-based attacks due to its memory-hard properties
- Work factor 12 produces ~250ms hash time on modern hardware — fast enough for login UX, slow enough to make brute-force impractical
- The work factor can be increased over time as hardware improves; existing hashes remain valid and can be rehashed on next login
- BCrypt has a 72-byte input limit — passwords longer than 72 bytes are truncated. This is acceptable given the password policy (8+ characters with complexity requirements) and the extremely rare occurrence of passwords exceeding 72 bytes
- BCrypt.Net-Next is a mature, well-maintained .NET library

**Alternatives Considered:**
- **Argon2 (Argon2id):** Winner of the Password Hashing Competition and theoretically superior to BCrypt. Rejected for MVP due to less mature .NET library support and configuration complexity (memory, parallelism, iterations must all be tuned). Can be adopted later if needed.
- **PBKDF2:** Available natively in ASP.NET Core Identity. Rejected — PBKDF2-SHA256 is more vulnerable to GPU acceleration than BCrypt. While usable with high iteration counts, BCrypt provides better security with simpler configuration.

---

## ADR-005: Refresh Token Rotation with Reuse Detection

**Status:** Accepted

**Context:**
Refresh tokens are long-lived credentials (30 days) that allow clients to obtain new access tokens without re-authentication. If a refresh token is stolen, an attacker can maintain access for the token's full lifetime. This risk must be mitigated.

**Decision:**
Implement refresh token rotation: every time a refresh token is used, a new refresh token is issued and the old one is revoked. Additionally, implement reuse detection: if a revoked refresh token is presented, revoke the entire token family (all tokens linked via the `ReplacedByToken` chain).

**Consequences:**
- A stolen refresh token can only be used once — either the legitimate client or the attacker uses it first
- If the attacker uses it first, the legitimate client's next refresh attempt uses a revoked token, triggering family-wide revocation and effectively logging out the attacker
- If the legitimate client uses it first, the attacker's stolen token is already revoked
- Refresh tokens are stored as SHA-256 hashes in the database — a database breach does not expose usable tokens
- Each refresh operation is a database write (revoke old, create new) — acceptable overhead for a security-critical operation
- Clients must handle the new refresh token on every token refresh response and discard the old one

**Alternatives Considered:**
- **Long-lived non-rotating refresh tokens:** Rejected — a stolen token grants persistent access for the full lifetime with no detection mechanism
- **Short-lived refresh tokens without rotation:** Rejected — forces frequent re-authentication, degrading UX without the security benefit of reuse detection
- **Refresh token binding (DPoP):** A stronger mechanism that binds tokens to a specific client key pair. Deferred to post-MVP due to implementation complexity and limited client library support

---

## ADR-006: OIDC Discovery Endpoint as a Web-Layer-Only Concern

**Status:** Accepted

**Context:**
The OIDC Discovery endpoint (`GET /.well-known/openid-configuration`) returns a provider metadata document defined by OpenID Connect Discovery 1.0 and RFC 8414. This document is pure protocol metadata — issuer URL, endpoint locations, supported algorithms, scopes, and grant types — with no domain logic, no database interaction, and no user-specific data.

**Decision:**
Implement the discovery endpoint entirely in the Web layer (`DiscoveryController`), with no Application-layer handler or MediatR dispatch. The controller reads directly from `JwtSettings` (issuer URL) and `OAuthSettings` (supported scopes) to construct the response.

**Consequences:**
- No unnecessary MediatR overhead for a static metadata response
- Endpoint URL derivation is always consistent: every URL in the document is constructed as `{Issuer.TrimEnd('/')}/connect/{path}`, so the document can never advertise endpoints that point to a different host than the configured issuer
- `ScopePolicy` (domain) and `OAuthSettings.SupportedScopes` (protocol) are kept intentionally separate — `ScopePolicy` enforces what the server *accepts*; the discovery document advertises what the server *supports publicly*. These sets are expected to match in production but are not coupled in code, allowing a scope to be accepted for backward compatibility without being re-advertised, or to be staged gradually before being published

**Keeping the discovery document accurate — changes required for future capabilities:**

| Capability | Field(s) to update | Notes |
|---|---|---|
| **Add a new scope** | `scopes_supported` (via `OAuthSettings.SupportedScopes`) | Also add the scope to `ScopePolicy._scopeNames` so domain validation accepts it |
| **Retire a scope** | Remove from `OAuthSettings.SupportedScopes` | Keep in `ScopePolicy` for backward compatibility until existing tokens expire |
| **Confidential client support** | `token_endpoint_auth_methods_supported` | Add `"client_secret_post"` and/or `"client_secret_basic"`. Also expose `"client_secret_jwt"` or `"private_key_jwt"` if mTLS or JWT-based auth is supported |
| **Token introspection (RFC 7662)** | Add `introspection_endpoint` field to `DiscoveryDocument` | Requires a new `POST /connect/introspect` endpoint and an introspection handler |
| **Pushed Authorization Requests (PAR, RFC 9126)** | Add `pushed_authorization_request_endpoint`, `require_pushed_authorization_requests` | Requires a new `POST /connect/par` endpoint |
| **JWKS key rotation** | `jwks_uri` already advertised — no document change needed | The JWKS endpoint (task 5.2) must support multiple keys; tokens must include a `kid` header matching a key in the set |
| **New response types (implicit, hybrid)** | `response_types_supported` | Not recommended — these flows are deprecated in OAuth 2.1; add only if a specific legacy client requires them |
| **New ID token signing algorithms** | `id_token_signing_alg_values_supported` | Add the algorithm string (e.g., `"ES256"`) when a new key type is introduced in the signing key infrastructure |

**Alternatives Considered:**
- **Application-layer handler via MediatR:** Rejected — the discovery document contains no domain logic, no validation, and requires no database access. Routing it through the MediatR pipeline adds ceremony with no benefit
- **Expose `ScopePolicy.AllowedScopes` and use it directly in the controller:** Rejected — `ScopePolicy` is a domain enforcer, not a configuration source. Coupling the discovery document to the domain policy blurs the boundary between what the server *accepts* (a domain invariant) and what it *advertises* (a protocol concern). These sets are expected to be equal but are conceptually distinct and should be maintained independently

---

## ADR-007: Dual Authentication Schemes — Cookie Sessions and JWT Bearer

**Status:** Accepted

**Context:**
The server has two distinct classes of API consumers: the first-party management dashboard (DMAuth.Client) and third-party resource servers calling the `/connect/userinfo` endpoint with access tokens. These consumers require fundamentally different authentication mechanisms. Forcing the dashboard through the OAuth flow would create a circular dependency where the auth server must use itself as its own authorization provider.

**Decision:**
Register two authentication schemes simultaneously:
- **Cookie authentication** (default scheme) — the dashboard authenticates via username/password to `/api/users/login` and receives an `HttpOnly`, `SameSite=Strict`, `Secure` session cookie valid for 24 hours with sliding expiration
- **JWT Bearer** — the `/connect/userinfo` endpoint uses `[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]` to accept RS256-signed access tokens issued by the server itself

All cookie auth redirect events (`OnRedirectToLogin`, `OnRedirectToAccessDenied`) return 401/403 status codes instead of performing HTTP redirects — the API should never redirect; the SPA handles navigation.

**Consequences:**
- No circular dependency — the dashboard authenticates directly, not via the OAuth flow the server provides to third parties
- Clear, explicit scheme selection: OAuth protocol endpoints opt in to JWT Bearer via the `[Authorize]` attribute; all other authenticated endpoints use cookies by default
- Cookie security properties (HttpOnly, SameSite=Strict, Secure) provide CSRF protection and prevent JavaScript access to the session token
- The `/connect/userinfo` endpoint correctly requires a Bearer token, conforming to the OIDC UserInfo spec (RFC 5749)
- Two auth schemes means two middleware paths — developers must understand which scheme applies when adding new endpoints

**Alternatives Considered:**
- **OAuth authorization code + PKCE for the dashboard:** Rejected — the dashboard would need to initiate an OAuth flow against the same server it is managing, creating a bootstrap dependency. If the auth server is misconfigured, the dashboard cannot be used to fix it.
- **Single JWT Bearer scheme for all endpoints:** Rejected — the dashboard would need to store and refresh access tokens in JavaScript, exposing them to XSS. HttpOnly cookies are strictly more secure for same-origin first-party UI sessions.

---

## ADR-008: Azure Key Vault with DefaultAzureCredential for Secrets Management

**Status:** Accepted

**Context:**
The server requires several sensitive secrets at runtime: the RSA private key for signing JWTs and the database connection string (with credentials). These must never appear in source-controlled configuration files, but must be available at startup before any service registration reads `IConfiguration`.

**Decision:**
Bootstrap Azure Key Vault as an `IConfiguration` provider before any service registration:

```
builder.Configuration.AddAzureKeyVault(vaultUri, new DefaultAzureCredential());
```

`DefaultAzureCredential` tries a chain of credential sources in order — Visual Studio / `az login` in development, and Managed Identity in production — with no code change required between environments. All `appsettings.json` secret fields are set to `null` as placeholders; the Key Vault provider overlays the real values at startup.

**Consequences:**
- Secrets are never committed to source control — `appsettings.json` contains only null placeholders
- The same binary runs in development and production without environment-specific secret handling code
- Managed Identity in production means no credential rotation is needed for Key Vault access itself
- Key Vault secrets can be rotated without redeployment; the new value takes effect on next app restart
- If Key Vault is unreachable at startup, the application fails fast (the Key Vault URI check throws `InvalidOperationException` if absent)
- Local development requires an active `az login` session with permissions to the Key Vault; new contributors must be granted access

**Alternatives Considered:**
- **`dotnet user-secrets` only:** Sufficient for local development but does not scale to team environments or production. Secrets must be distributed to each developer manually.
- **Environment variables:** Works for CI/CD but requires secrets to be injected into the process environment, which is visible to all processes on the host. Key Vault keeps secrets out of the environment entirely.
- **Hardcoded or appsettings-committed secrets:** Rejected — immediate security risk and incompatible with any compliance posture.

---

## ADR-009: Docker Compose for Local SQL Server Development

**Status:** Accepted

**Context:**
The project's production and development databases are hosted on Azure. Contributors without an Azure account — or those who prefer a fully local environment — have no way to run the application without cloud access.

**Decision:**
Provide a `docker-compose.yml` at `eng/dev/` that runs SQL Server 2022 Developer edition on `localhost,1433`. The SA password is supplied via a `${SA_PASSWORD}` environment variable at `docker compose up` time and is never committed. A named volume (`dmauth-db-data`) persists data across container restarts. A health check polls the server every 10 seconds before the container is marked ready.

**Consequences:**
- Zero-configuration local database setup for any contributor with Docker installed
- Developer choice: use the local container or the Azure Dev DB — both are valid; the connection string in user secrets determines which
- SA password is never stored in the repository; developers set it once in their shell environment
- Named volume survives `docker compose down` — data is only lost on explicit `docker compose down -v`
- SQL Server 2022 Developer edition is functionally identical to Enterprise but licensed for development use only — not suitable for production

**Alternatives Considered:**
- **SQL Server LocalDB:** Windows-only and requires Visual Studio or a standalone installer. Excludes macOS and Linux contributors.
- **SQLite for local development:** Would require a second EF Core provider and migration path. Behavioral differences between SQLite and SQL Server could mask bugs. Rejected in favor of running the actual target database engine locally.
- **Require Azure access for all contributors:** Rejected — adds onboarding friction, requires Azure account provisioning, and creates a hard dependency on external infrastructure for local development.
