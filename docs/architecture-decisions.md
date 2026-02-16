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
