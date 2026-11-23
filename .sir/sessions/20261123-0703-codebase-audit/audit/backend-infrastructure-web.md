# Audit: backend (T2)
Ref: 5b7d9a6

## Scope

`find <R>/src/DMAuth.Infrastructure <R>/src/DMAuth.Web <R>/src/DM-Auth.sln -type f -not -path '*/bin/*' -not -path '*/obj/*' -not -path '<R>/src/DMAuth.Infrastructure/Persistence/*' -not -path '<R>/src/DMAuth.Infrastructure/Migrations/*'`

Every scoped file is CRLF-terminated in the worktree; Evidence quotes each line without its
trailing CR.

## Files

- `src/DMAuth.Infrastructure/DependencyInjection.cs` — IW-006, IW-008, IW-010, IW-014, IW-015
- `src/DMAuth.Infrastructure/DMAuth.Infrastructure.csproj` — IW-001, IW-009
- `src/DMAuth.Infrastructure/Security/BcryptPasswordHasher.cs` — no finding
- `src/DMAuth.Infrastructure/Services/TokenService.cs` — IW-008, IW-010, IW-013, IW-014
- `src/DMAuth.Web/appsettings.Development.json` — IW-017
- `src/DMAuth.Web/appsettings.json` — IW-017
- `src/DMAuth.Web/appsettings.Production.json` — IW-017
- `src/DMAuth.Web/Auth/AuthController.cs` — IW-002, IW-003, IW-004, IW-005, IW-019
- `src/DMAuth.Web/Auth/Requests/AuthorizeRequest.cs` — no finding
- `src/DMAuth.Web/Auth/Requests/ExchangeTokenRequest.cs` — IW-021
- `src/DMAuth.Web/Auth/Requests/GrantConsentRequest.cs` — no finding
- `src/DMAuth.Web/Auth/Requests/RevokeTokenRequest.cs` — no finding
- `src/DMAuth.Web/Clients/ClientController.cs` — IW-019
- `src/DMAuth.Web/Clients/Requests/RegisterClientRequest.cs` — no finding
- `src/DMAuth.Web/Clients/Requests/UpdateClientRequest.cs` — no finding
- `src/DMAuth.Web/Common/ApiControllerBase.cs` — IW-019
- `src/DMAuth.Web/Common/ClaimsPrincipalFactory.cs` — no finding
- `src/DMAuth.Web/Common/CurrentUser/CurrentUserService.cs` — IW-011, IW-019
- `src/DMAuth.Web/Common/CurrentUser/ICurrentUserService.cs` — no finding
- `src/DMAuth.Web/Common/Middleware/ExceptionHandlingMiddleware.cs` — IW-007, IW-011
- `src/DMAuth.Web/Common/SigningKey/ISigningKeyProvider.cs` — no finding
- `src/DMAuth.Web/Common/SigningKey/SigningKeyProvider.cs` — IW-008, IW-011, IW-013
- `src/DMAuth.Web/Discovery/DiscoveryController.cs` — IW-019
- `src/DMAuth.Web/Discovery/Documents/DiscoveryDocument.cs` — IW-011
- `src/DMAuth.Web/Discovery/Documents/JwksDocument.cs` — IW-012
- `src/DMAuth.Web/Discovery/JwksController.cs` — IW-019
- `src/DMAuth.Web/DMAuth.Web.csproj` — IW-020
- `src/DMAuth.Web/Program.cs` — IW-002, IW-005, IW-016
- `src/DMAuth.Web/Properties/launchSettings.json` — no finding
- `src/DMAuth.Web/Users/Requests/ChangeUserPasswordRequest.cs` — no finding
- `src/DMAuth.Web/Users/Requests/LoginRequest.cs` — no finding
- `src/DMAuth.Web/Users/Requests/RegisterUserRequest.cs` — no finding
- `src/DMAuth.Web/Users/Requests/UpdateUserProfileRequest.cs` — no finding
- `src/DMAuth.Web/Users/UserController.cs` — IW-005, IW-018, IW-019
- `src/DM-Auth.sln` — no finding

## Rules checked

- `csharp-style.md` § Naming — conforms
- `csharp-style.md` § File organization — IW-012
- `csharp-style.md` § Parameter layout — conforms
- `csharp-style.md` § Declaration layout — IW-013
- `csharp-style.md` § Lambdas, expression bodies and chaining — IW-014
- `csharp-style.md` § XML documentation comments — IW-019
- `dotnet.md` § Platform — conforms
- `dotnet.md` § Solution and layers — IW-009, IW-010, IW-011
- `dotnet.md` § CQRS — conforms
- `dotnet.md` § Result type — conforms
- `dotnet.md` § Entities — n/a: no entity is declared in the T2 scope (entities are in DMAuth.Domain, T1)
- `dotnet.md` § Value objects and policies — n/a: no value object or policy is declared in the T2 scope (DMAuth.Domain, T1)
- `dotnet.md` § Async and cancellation — IW-018
- `dotnet.md` § Dependency injection — IW-008
- `dotnet.md` § Error handling and exceptions — conforms
- `dotnet.md` § HttpContext — conforms
- `dotnet.md` § Requests and responses — IW-007
- `dotnet.md` § Performance and memory — IW-008
- `dotnet.md` § HTTP clients — n/a: the T2 scope makes no outbound HTTP call and constructs no HttpClient
- `dotnet.md` § Configuration and secrets — IW-017
- `efcore.md` § Queries — n/a: the T2 scope issues no EF Core query; DependencyInjection.cs only registers the context, and the repositories are under Persistence/ (T3)

## Findings

### IW-001 Package downgrade NU1605 breaks restore of the solution
- Severity: correctness/security risk
- Layer: backend
- Location: `src/DMAuth.Infrastructure/DMAuth.Infrastructure.csproj:11`; `src/DMAuth.Infrastructure/DMAuth.Infrastructure.csproj:12`; `src/DMAuth.Infrastructure/DMAuth.Infrastructure.csproj:5`
- Rule: external NuGet Error NU1605 (detected package downgrade) https://learn.microsoft.com/en-us/nuget/reference/errors-and-warnings/nu1605
- Moves: n/a
- What: Remove the pinned `Microsoft.Extensions.DependencyInjection.Abstractions` and `Microsoft.Extensions.Options.ConfigurationExtensions` `10.0.7` references (they are supplied transitively), or raise them to the version the EF Core / DataProtection packages require, and pin those floating `10.*` references to an exact version so restore is reproducible.
- Why: `dotnet build src/DM-Auth.sln` fails restore with NU1605 (downgrade of Microsoft.Extensions.DependencyInjection.Abstractions from 10.0.15 to 10.0.7) under warnings-as-errors, as observed by the discuss-phase probe (`error NU1605: Warning As Error: Detected package downgrade ... 1 Error(s), exit=1`); the floating `Version="10.*"` on Microsoft.AspNetCore.DataProtection.EntityFrameworkCore (and EntityFrameworkCore.SqlServer/Tools) now resolves to 10.0.15, whose chain DataProtection.EntityFrameworkCore 10.0.15 -> Microsoft.AspNetCore.DataProtection 10.0.15 requires DependencyInjection.Abstractions >= 10.0.15, while line 11 pins it directly to 10.0.7 (path recorded in the worktree's `src/DMAuth.Infrastructure/obj/project.assets.json` NU1605 log entry); NuGet treats NU1605 as an error by default, so the whole solution cannot build or test, and the break appeared without any repo change because the floating versions moved.
- Verify: `dotnet restore src/DM-Auth.sln` and `dotnet build src/DM-Auth.sln` both exit 0 with no NU1605 in the output.
- Evidence:
  ```text
  src/DMAuth.Infrastructure/DMAuth.Infrastructure.csproj:11:    <PackageReference Include="Microsoft.Extensions.DependencyInjection.Abstractions" Version="10.0.7" />
  src/DMAuth.Infrastructure/DMAuth.Infrastructure.csproj:12:    <PackageReference Include="Microsoft.Extensions.Options.ConfigurationExtensions" Version="10.0.7" />
  src/DMAuth.Infrastructure/DMAuth.Infrastructure.csproj:5:    <PackageReference Include="Microsoft.AspNetCore.DataProtection.EntityFrameworkCore" Version="10.*" />
  ```

### IW-002 Cookie-authenticated consent form POST has no CSRF protection
- Severity: correctness/security risk
- Layer: backend
- Location: `src/DMAuth.Web/Auth/AuthController.cs:115`; `src/DMAuth.Web/Auth/AuthController.cs:121`; `src/DMAuth.Web/Program.cs:59`
- Rule: external Prevent Cross-Site Request Forgery (XSRF/CSRF) attacks in ASP.NET Core https://learn.microsoft.com/en-us/aspnet/core/security/anti-request-forgery; external RFC 6749 §10.12 Cross-Site Request Forgery https://datatracker.ietf.org/doc/html/rfc6749#section-10.12
- Moves: n/a
- What: Protect `POST /connect/consent` against cross-site submission: require an antiforgery token (or a custom request header the SPA sets, which forces a CORS preflight) and reject requests without it.
- Why: The session cookie is `SameSite=None`, so a browser attaches it to a cross-site form POST; the consent action binds `[FromForm]` (a CORS simple request, no preflight) and the code base registers no antiforgery service, so an attacker page can auto-submit consent for the attacker's own client, redirect URI and PKCE challenge on behalf of a signed-in victim and receive an authorization code it can exchange for the victim's tokens.
- Verify: A cross-origin `application/x-www-form-urlencoded` POST to `/connect/consent` carrying a valid session cookie but no antiforgery token / custom header is rejected with 400; `grep -rn --include=*.cs -i 'antiforgery' src/DMAuth.Web` prints the registration and validation sites.
- Evidence:
  ```text
  src/DMAuth.Web/Auth/AuthController.cs:115:	[HttpPost("consent")]
  src/DMAuth.Web/Auth/AuthController.cs:121:		[FromForm] GrantConsentRequest request,
  src/DMAuth.Web/Program.cs:59:		options.Cookie.SameSite = SameSiteMode.None;
  $ grep -rn --include=*.cs -i 'antiforgery' src/DMAuth.Web src/DMAuth.Infrastructure
  (no output, exit 1)
  ```

### IW-003 Token endpoint responses are not marked Cache-Control: no-store
- Severity: correctness/security risk
- Layer: backend
- Location: `src/DMAuth.Web/Auth/AuthController.cs:187`; `src/DMAuth.Web/Auth/AuthController.cs:204`
- Rule: external RFC 6749 §5.1 Successful Response https://datatracker.ietf.org/doc/html/rfc6749#section-5.1
- Moves: n/a
- What: Set `Cache-Control: no-store` (and `Pragma: no-cache`) on every `/connect/token` response, for example with `[ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]` on `TokenAsync`.
- Why: RFC 6749 §5.1 says the authorization server MUST include `Cache-Control: no-store` in any response containing tokens; without it, access, ID and refresh tokens may be retained by intermediaries or the browser cache.
- Verify: `curl -i -X POST .../connect/token ...` on a successful exchange shows `Cache-Control: no-store`; `grep -n 'NoStore\|no-store' src/DMAuth.Web/Auth/AuthController.cs` prints a line.
- Evidence:
  ```text
  src/DMAuth.Web/Auth/AuthController.cs:187:			return Ok(result.Value);
  src/DMAuth.Web/Auth/AuthController.cs:204:		return Ok(codeResult.Value);
  ```

### IW-004 Code redirect concatenates the query onto a redirect URI that may already have one
- Severity: correctness/security risk
- Layer: backend
- Location: `src/DMAuth.Web/Auth/AuthController.cs:301`
- Rule: external RFC 6749 §3.1.2 Redirection Endpoint https://datatracker.ietf.org/doc/html/rfc6749#section-3.1.2
- Moves: n/a
- What: Build the code redirect by merging `code` and `state` into the redirect URI's existing query (for example `QueryHelpers.AddQueryString(redirectUri, ...)`), not by appending `?code=...` to the raw string.
- Why: RFC 6749 §3.1.2 allows a redirect URI with a query component, which MUST be retained when parameters are added; `src/DMAuth.Domain/Policies/RedirectUriPolicy.cs` rejects only a fragment, so a registered `https://app/cb?tenant=a` yields `https://app/cb?tenant=a?code=...&state=...`, corrupting the last existing parameter and breaking the client's code handling.
- Verify: A unit or integration test registering a redirect URI with a query component asserts the `Location` header is `https://app/cb?tenant=a&code=...&state=...`.
- Evidence:
  ```text
  src/DMAuth.Web/Auth/AuthController.cs:301:		return $"{redirectUri}{query}";
  ```

### IW-005 No rate limiting on the login and token endpoints
- Severity: correctness/security risk
- Layer: backend
- Location: `src/DMAuth.Web/Program.cs`; `src/DMAuth.Web/Users/UserController.cs:69`; `src/DMAuth.Web/Auth/AuthController.cs:164`
- Rule: external Rate limiting middleware in ASP.NET Core https://learn.microsoft.com/en-us/aspnet/core/performance/rate-limit; external OWASP Authentication Cheat Sheet https://cheatsheetseries.owasp.org/cheatsheets/Authentication_Cheat_Sheet.html
- Moves: n/a
- What: Add the ASP.NET Core rate limiter in `Program.cs` (`AddRateLimiter` / `UseRateLimiter`) and apply a partitioned policy to `POST /api/users/login` and `POST /connect/token`; `Program.cs` currently registers none (whole-file absence).
- Why: Both endpoints verify secrets (password, authorization code / refresh token) and are anonymous, so without throttling they are open to credential stuffing and brute force, which OWASP lists as a primary authentication control.
- Verify: `grep -n 'AddRateLimiter\|UseRateLimiter' src/DMAuth.Web/Program.cs` prints both, and repeated login attempts beyond the limit return 429.
- Evidence:
  ```text
  src/DMAuth.Web/Users/UserController.cs:69:	[HttpPost("login")]
  src/DMAuth.Web/Auth/AuthController.cs:164:	[HttpPost("token")]
  $ grep -rn --include=*.cs -i 'ratelimit' src/DMAuth.Web src/DMAuth.Infrastructure
  (no output, exit 1)
  ```

### IW-006 Data Protection keys are persisted to the database without encryption at rest
- Severity: correctness/security risk
- Layer: backend
- Location: `src/DMAuth.Infrastructure/DependencyInjection.cs:55`
- Rule: external Key encryption at rest in Windows and Azure using ASP.NET Core https://learn.microsoft.com/en-us/aspnet/core/security/data-protection/implementation/key-encryption-at-rest
- Moves: n/a
- What: Chain a key encryptor after `PersistKeysToDbContext` (for example `ProtectKeysWithAzureKeyVault(...)`, since Key Vault is already bootstrapped in `Program.cs`) and set an explicit application name.
- Why: With no `ProtectKeysWith*` call the key ring is stored in plain XML in the database, so anyone with read access to the DataProtectionKeys table can forge or decrypt the `dm_auth_session` cookie; ASP.NET Core logs a "No XML encryptor configured" warning for exactly this setup.
- Verify: `grep -n 'ProtectKeysWith' src/DMAuth.Infrastructure/DependencyInjection.cs` prints a line, and the startup log no longer contains "No XML encryptor configured".
- Evidence:
  ```text
  src/DMAuth.Infrastructure/DependencyInjection.cs:55:			.PersistKeysToDbContext<DmAuthDbContext>();
  ```

### IW-007 Exception middleware sets the status code without checking whether the response has started
- Severity: correctness/security risk
- Layer: backend
- Location: `src/DMAuth.Web/Common/Middleware/ExceptionHandlingMiddleware.cs:53`; `src/DMAuth.Web/Common/Middleware/ExceptionHandlingMiddleware.cs:70`; `src/DMAuth.Web/Common/Middleware/ExceptionHandlingMiddleware.cs:86`
- Rule: discipline .sir/disciplines/dotnet.md § Requests and responses; external Handle errors in ASP.NET Core https://learn.microsoft.com/en-us/aspnet/core/fundamentals/error-handling
- Moves: code
- What: In each catch block, check `context.Response.HasStarted` first; when it has started, log and rethrow (or abort) instead of setting `StatusCode` and writing a JSON body.
- Why: An exception thrown after a downstream component began writing the body makes `context.Response.StatusCode = ...` throw `InvalidOperationException` from inside the catch, which replaces the original exception and produces a truncated response; the discipline forbids modifying response headers after the response has started.
- Verify: `grep -n 'HasStarted' src/DMAuth.Web/Common/Middleware/ExceptionHandlingMiddleware.cs` prints a line, and a test whose endpoint throws after flushing the body observes the original exception logged.
- Evidence:
  ```text
  src/DMAuth.Web/Common/Middleware/ExceptionHandlingMiddleware.cs:53:			context.Response.StatusCode = StatusCodes.Status400BadRequest;
  src/DMAuth.Web/Common/Middleware/ExceptionHandlingMiddleware.cs:70:			context.Response.StatusCode = StatusCodes.Status400BadRequest;
  src/DMAuth.Web/Common/Middleware/ExceptionHandlingMiddleware.cs:86:			context.Response.StatusCode = StatusCodes.Status500InternalServerError;
  ```

### IW-008 TokenService re-imports the RSA private key per request
- Severity: convention drift
- Layer: backend
- Location: `src/DMAuth.Infrastructure/DependencyInjection.cs:58`; `src/DMAuth.Infrastructure/Services/TokenService.cs:35`; `src/DMAuth.Infrastructure/Services/TokenService.cs:36`; `src/DMAuth.Web/Common/SigningKey/SigningKeyProvider.cs:33`
- Rule: discipline .sir/disciplines/dotnet.md § Performance and memory; discipline .sir/disciplines/dotnet.md § Dependency injection
- Moves: code
- What: Register `TokenService` as a singleton (it holds no request state), or build the `SigningCredentials` once in a singleton key holder shared with `SigningKeyProvider`, so the PEM is parsed and the key ID computed once per process, and dispose the `RSA` with that holder.
- Why: `TokenService` is scoped and creates and imports a new `RSA` from PEM in its constructor, so every request that resolves it repeats the PEM parse and JWK thumbprint (duplicating `SigningKeyProvider`'s own import) and leaves an undisposed `RSA` instance behind; the discipline requires signing credentials and RSA keys to be computed once at startup and token generation services to be singletons.
- Verify: `grep -n 'ITokenService, TokenService' src/DMAuth.Infrastructure/DependencyInjection.cs` shows `AddSingleton`, and `grep -c 'ImportFromPem' src/DMAuth.Infrastructure/Services/TokenService.cs src/DMAuth.Web/Common/SigningKey/SigningKeyProvider.cs` totals 1.
- Evidence:
  ```text
  src/DMAuth.Infrastructure/DependencyInjection.cs:58:		services.AddScoped<ITokenService, TokenService>();
  src/DMAuth.Infrastructure/Services/TokenService.cs:35:		var rsa = RSA.Create();
  src/DMAuth.Infrastructure/Services/TokenService.cs:36:		rsa.ImportFromPem(settings.RsaPrivateKeyPem);
  src/DMAuth.Web/Common/SigningKey/SigningKeyProvider.cs:33:		rsa.ImportFromPem(settings.RsaPrivateKeyPem);
  ```

### IW-009 Infrastructure depends on Application, which the layer rule does not allow
- Severity: convention drift
- Layer: backend
- Location: `src/DMAuth.Infrastructure/DMAuth.Infrastructure.csproj:16`; `.sir/disciplines/dotnet.md:12`
- Rule: discipline .sir/disciplines/dotnet.md § Solution and layers
- Moves: discipline
- What: Restate the dependency rule at `.sir/disciplines/dotnet.md:12` as `Infrastructure → Application, Domain`, matching the code, where Infrastructure implements Application interfaces (`IPasswordHasher`, `ITokenService`) and binds Application settings (`JwtSettings`, `OAuthSettings`).
- Why: The code is the evidence: Infrastructure's only project reference is Application and its services implement Application-declared ports, so the rule's `Infrastructure → Domain` misdescribes the architecture and every audit against it reports a false violation.
- Verify: `grep -n 'Infrastructure → ' .sir/disciplines/dotnet.md` shows Infrastructure depending on Application and Domain.
- Evidence:
  ```text
  src/DMAuth.Infrastructure/DMAuth.Infrastructure.csproj:16:    <ProjectReference Include="..\DMAuth.Application\DMAuth.Application.csproj" />
  .sir/disciplines/dotnet.md:12:- Dependencies point only this way: Application → Domain; Infrastructure → Domain; Web → Application, Infrastructure. Domain has zero external dependencies. Web is the composition root.
  ```

### IW-010 TokenService lives in a generic Services folder instead of a concern folder
- Severity: convention drift
- Layer: backend
- Location: `src/DMAuth.Infrastructure/Services/TokenService.cs:10`; `src/DMAuth.Infrastructure/DependencyInjection.cs:7`
- Rule: discipline .sir/disciplines/dotnet.md § Solution and layers
- Moves: code
- What: Move `TokenService.cs` to `src/DMAuth.Infrastructure/Security/` with namespace `DMAuth.Infrastructure.Security`, beside `BcryptPasswordHasher`, and drop the `Services` using.
- Why: The discipline organizes Infrastructure by concern (`Persistence/`, `Security/`, `Logging/`, `Email/`); `Services/` names no concern and holds a single token-signing (security) type.
- Verify: `find src/DMAuth.Infrastructure/Services -type f` prints nothing and `grep -rn 'Infrastructure.Services' src tests --include=*.cs` prints nothing.
- Evidence:
  ```text
  src/DMAuth.Infrastructure/Services/TokenService.cs:10:namespace DMAuth.Infrastructure.Services;
  src/DMAuth.Infrastructure/DependencyInjection.cs:7:using DMAuth.Infrastructure.Services;
  ```

### IW-011 Web layout rule does not describe Common/ subfolders or Discovery documents
- Severity: convention drift
- Layer: backend
- Location: `src/DMAuth.Web/Common/Middleware/ExceptionHandlingMiddleware.cs:4`; `src/DMAuth.Web/Common/CurrentUser/CurrentUserService.cs:3`; `src/DMAuth.Web/Common/SigningKey/SigningKeyProvider.cs:6`; `src/DMAuth.Web/Discovery/Documents/DiscoveryDocument.cs:3`; `.sir/disciplines/dotnet.md:17`
- Rule: discipline .sir/disciplines/dotnet.md § Solution and layers
- Moves: discipline
- What: Restate the Web layout at `.sir/disciplines/dotnet.md:17` to the structure the code uses: cross-cutting Web types under `Common/{Concern}/` (`Common/Middleware/`, `Common/CurrentUser/`, `Common/SigningKey/`) rather than a root `Middleware/`, and protocol documents with no Application counterpart under `{Feature}/Documents/`.
- Why: The code is the evidence: middleware sits in `Common/Middleware/` and the OIDC discovery/JWKS payloads sit in `Discovery/Documents/`, neither of which the rule names, so the rule as written would move working code for no behavioural gain.
- Verify: `sed -n '17p' .sir/disciplines/dotnet.md` names `Common/{Concern}/` and `{Feature}/Documents/`.
- Evidence:
  ```text
  src/DMAuth.Web/Common/Middleware/ExceptionHandlingMiddleware.cs:4:namespace DMAuth.Web.Common.Middleware;
  src/DMAuth.Web/Common/CurrentUser/CurrentUserService.cs:3:namespace DMAuth.Web.Common.CurrentUser;
  src/DMAuth.Web/Common/SigningKey/SigningKeyProvider.cs:6:namespace DMAuth.Web.Common.SigningKey;
  src/DMAuth.Web/Discovery/Documents/DiscoveryDocument.cs:3:namespace DMAuth.Web.Discovery.Documents;
  .sir/disciplines/dotnet.md:17:  - Web by feature, mirroring Application: `Common/`, `{Feature}/{Feature}Controller` with `{Feature}/Requests/` for web-layer input models, `Middleware/`, `Program`.
  ```

### IW-012 JwksDocument.cs declares two types
- Severity: convention drift
- Layer: backend
- Location: `src/DMAuth.Web/Discovery/Documents/JwksDocument.cs:21`
- Rule: discipline .sir/disciplines/csharp-style.md § File organization
- Moves: code
- What: Move `JsonWebKeyParameters` into its own file `src/DMAuth.Web/Discovery/Documents/JsonWebKeyParameters.cs`.
- Why: The fragment requires one type per file, named after the type; `JsonWebKeyParameters` cannot be found by file name.
- Verify: `grep -c '^public record' src/DMAuth.Web/Discovery/Documents/JwksDocument.cs` prints 1.
- Evidence:
  ```text
  src/DMAuth.Web/Discovery/Documents/JwksDocument.cs:21:public record JsonWebKeyParameters
  ```

### IW-013 Base interface on the declaration line
- Severity: convention drift
- Layer: backend
- Location: `src/DMAuth.Infrastructure/Services/TokenService.cs:15`; `src/DMAuth.Web/Common/SigningKey/SigningKeyProvider.cs:12`
- Rule: discipline .sir/disciplines/csharp-style.md § Declaration layout
- Moves: code
- What: Put the interface list on its own line, indented one tab (`public sealed class TokenService` / `\t: ITokenService`), as `BcryptPasswordHasher` already does.
- Why: The fragment puts the base class / interface list on its own line; these are the two declarations in the T2 scope that keep it inline, against the in-scope precedent in `BcryptPasswordHasher.cs`.
- Verify: `grep -rnE '^public( sealed)? class [A-Za-z]+ : ' src/DMAuth.Web src/DMAuth.Infrastructure --include=*.cs` prints nothing.
- Evidence:
  ```text
  src/DMAuth.Infrastructure/Services/TokenService.cs:15:public sealed class TokenService : ITokenService
  src/DMAuth.Web/Common/SigningKey/SigningKeyProvider.cs:12:public sealed class SigningKeyProvider : ISigningKeyProvider
  ```

### IW-014 Inline lambda body and chained calls on one line
- Severity: convention drift
- Layer: backend
- Location: `src/DMAuth.Infrastructure/DependencyInjection.cs:40`; `src/DMAuth.Infrastructure/Services/TokenService.cs:69`; `src/DMAuth.Infrastructure/Services/TokenService.cs:100`; `src/DMAuth.Infrastructure/Services/TokenService.cs:101`; `src/DMAuth.Infrastructure/Services/TokenService.cs:120`
- Rule: discipline .sir/disciplines/csharp-style.md § Lambdas, expression bodies and chaining
- Moves: code
- What: Break the `sqlOptions =>` body onto a new indented line, and put each chained call (`.ToUnixTimeSeconds()`, `.ToString()`, `.ToLowerInvariant()`) on its own line, or extract the value into a local first.
- Why: The fragment puts everything after `=>` on a new line and each chained call (including string operations) on its own line; the rest of the T2 scope follows it.
- Verify: `grep -nE '=> [a-z]+\.|\)\.[A-Za-z]+\(\)\.' src/DMAuth.Infrastructure/DependencyInjection.cs src/DMAuth.Infrastructure/Services/TokenService.cs` prints nothing.
- Evidence:
  ```text
  src/DMAuth.Infrastructure/DependencyInjection.cs:40:				sqlOptions => sqlOptions.EnableRetryOnFailure(
  src/DMAuth.Infrastructure/Services/TokenService.cs:69:			new(JwtRegisteredClaimNames.Iat, new DateTimeOffset(now).ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64),
  src/DMAuth.Infrastructure/Services/TokenService.cs:100:			new(JwtRegisteredClaimNames.Iat, new DateTimeOffset(now).ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64),
  src/DMAuth.Infrastructure/Services/TokenService.cs:101:			new(JwtRegisteredClaimNames.AuthTime, authTime.ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64),
  src/DMAuth.Infrastructure/Services/TokenService.cs:120:				claims.Add(new Claim("email_verified", emailVerified.Value.ToString().ToLowerInvariant(), ClaimValueTypes.Boolean));
  ```

### IW-015 JWT and OAuth settings are not validated at startup
- Severity: convention drift
- Layer: backend
- Location: `src/DMAuth.Infrastructure/DependencyInjection.cs:60`; `src/DMAuth.Infrastructure/DependencyInjection.cs:64`
- Rule: base backend § Configuration
- Moves: code
- What: Register the settings with `AddOptions<JwtSettings>().Bind(...).ValidateDataAnnotations()` (or `.Validate(...)`) `.ValidateOnStart()`, and likewise for `OAuthSettings`, so an absent `Jwt:RsaPrivateKeyPem`, issuer or audience fails startup.
- Why: `Configure<T>` binds without validation; `RsaPrivateKeyPem` is `null` in every appsettings file, and the first consumer (`SigningKeyProvider` / `TokenService`, resolved lazily) only fails on the first request that needs a token, whereas the base discipline requires a missing or malformed value to fail startup.
- Verify: Starting the app with `Jwt:RsaPrivateKeyPem` unset exits during startup with an `OptionsValidationException`; `grep -n 'ValidateOnStart' src/DMAuth.Infrastructure/DependencyInjection.cs` prints a line.
- Evidence:
  ```text
  src/DMAuth.Infrastructure/DependencyInjection.cs:60:		services.Configure<JwtSettings>(configuration.GetSection("Jwt"));
  src/DMAuth.Infrastructure/DependencyInjection.cs:64:		services.Configure<OAuthSettings>(configuration.GetSection("OAuth"));
  ```

### IW-016 Swagger exposure branches on the environment name
- Severity: convention drift
- Layer: backend
- Location: `src/DMAuth.Web/Program.cs:174`
- Rule: base backend § Configuration
- Moves: code
- What: Replace the `IsDevelopment()` branch with a declared configuration flag (for example `Swagger:Enabled`, `false` in `appsettings.json`, `true` in `appsettings.Development.json`).
- Why: The base discipline requires behaviour that differs by environment to be a declared configuration value, never a branch on an environment name.
- Verify: `grep -n 'IsDevelopment' src/DMAuth.Web/Program.cs` prints nothing.
- Evidence:
  ```text
  src/DMAuth.Web/Program.cs:174:if (app.Environment.IsDevelopment())
  ```

### IW-017 DmAuth connection string defaults to an empty string instead of null
- Severity: convention drift
- Layer: backend
- Location: `src/DMAuth.Web/appsettings.json:5`; `src/DMAuth.Web/appsettings.Development.json:4`; `src/DMAuth.Web/appsettings.Production.json:4`
- Rule: discipline .sir/disciplines/dotnet.md § Configuration and secrets
- Moves: code
- What: Set `ConnectionStrings:DmAuth` to `null` in `appsettings.json` and each `appsettings.{Environment}.json`, as `Jwt:RsaPrivateKeyPem` already is.
- Why: The connection string is a credential-bearing setting supplied by Key Vault (`ConnectionStrings--DmAuth`); the fragment requires sensitive settings to be `null` in every appsettings file so that only a secret source can supply them.
- Verify: `grep -n '"DmAuth"' src/DMAuth.Web/appsettings*.json` shows `null` on every line.
- Evidence:
  ```text
  src/DMAuth.Web/appsettings.json:5:    "DmAuth": "",
  src/DMAuth.Web/appsettings.Development.json:4:    "DmAuth": "",
  src/DMAuth.Web/appsettings.Production.json:4:    "DmAuth": "",
  ```

### IW-018 Cancellation rule has no exemption for awaited calls that take no token
- Severity: convention drift
- Layer: backend
- Location: `src/DMAuth.Web/Users/UserController.cs:103`; `.sir/disciplines/dotnet.md:85`
- Rule: discipline .sir/disciplines/dotnet.md § Async and cancellation
- Moves: discipline
- What: Restate `.sir/disciplines/dotnet.md:85` so an async method accepts a `CancellationToken` when it awaits at least one call that accepts one, exempting methods such as `LogoutAsync` whose only awaited call (`HttpContext.SignOutAsync`) takes no token.
- Why: The code is the evidence: `LogoutAsync` awaits only `SignOutAsync`, which has no token parameter, so adding a `CancellationToken` parameter would be an unused argument; the rule as written flags it.
- Verify: `sed -n '85p' .sir/disciplines/dotnet.md` carries the exemption.
- Evidence:
  ```text
  src/DMAuth.Web/Users/UserController.cs:103:	public async Task<IActionResult> LogoutAsync()
  .sir/disciplines/dotnet.md:85:- Every async method accepts a `CancellationToken` and passes it to every awaited call.
  ```

### IW-019 Primary-constructor parameters of public Web types have no XML documentation
- Severity: convention drift
- Layer: backend
- Location: `src/DMAuth.Web/Auth/AuthController.cs:25`; `src/DMAuth.Web/Users/UserController.cs:21`; `src/DMAuth.Web/Clients/ClientController.cs:20`; `src/DMAuth.Web/Discovery/DiscoveryController.cs:13`; `src/DMAuth.Web/Discovery/JwksController.cs:13`; `src/DMAuth.Web/Common/CurrentUser/CurrentUserService.cs:9`; `src/DMAuth.Web/Common/ApiControllerBase.cs:12`
- Rule: discipline .sir/disciplines/csharp-style.md § XML documentation comments
- Moves: code
- What: Add a `<param>` tag for each primary-constructor parameter of these public types, as `ExceptionHandlingMiddleware` already does for `next` and `logger`.
- Why: The fragment requires an XML documentation comment for every public type, member and parameter; these types document their summary but none of their constructor parameters, against the in-scope precedent in `ExceptionHandlingMiddleware.cs`.
- Verify: Each listed file contains a `/// <param name="...">` line for every parameter of its primary constructor (for example `grep -c '<param name="mediator">' src/DMAuth.Web/Auth/AuthController.cs` prints 1).
- Evidence:
  ```text
  src/DMAuth.Web/Auth/AuthController.cs:25:public sealed class AuthController(
  src/DMAuth.Web/Users/UserController.cs:21:public sealed class UserController(
  src/DMAuth.Web/Clients/ClientController.cs:20:public sealed class ClientController(
  src/DMAuth.Web/Discovery/DiscoveryController.cs:13:public sealed class DiscoveryController(
  src/DMAuth.Web/Discovery/JwksController.cs:13:public sealed class JwksController(
  src/DMAuth.Web/Common/CurrentUser/CurrentUserService.cs:9:public sealed class CurrentUserService(
  src/DMAuth.Web/Common/ApiControllerBase.cs:12:public abstract class ApiControllerBase(
  ```

### IW-020 DMAuth.Web.csproj opens PropertyGroup on the Project line
- Severity: hygiene
- Layer: backend
- Location: `src/DMAuth.Web/DMAuth.Web.csproj:1`
- Rule: external MSBuild project file schema reference https://learn.microsoft.com/en-us/visualstudio/msbuild/msbuild-project-file-schema-reference
- Moves: n/a
- What: Reformat line 1 so `<Project>`, `<PropertyGroup>` and `<UserSecretsId>` each sit on their own line with consistent indentation, matching the other project files.
- Why: The run-on line (left by a tool edit) makes the file inconsistent with every other csproj in `src/` and obscures diffs on the property group.
- Verify: `sed -n '1p' src/DMAuth.Web/DMAuth.Web.csproj` prints only `<Project Sdk="Microsoft.NET.Sdk.Web">` (after the file's UTF-8 BOM, which the Evidence line below carries verbatim between `1:` and `<Project`).
- Evidence:
  ```text
  src/DMAuth.Web/DMAuth.Web.csproj:1:﻿<Project Sdk="Microsoft.NET.Sdk.Web"><PropertyGroup>  <UserSecretsId>c83e5f06-f255-46ce-b556-7c878ca5e5ae</UserSecretsId>
  ```

### IW-021 Stale XML doc says grant_type must be authorization_code
- Severity: hygiene
- Layer: backend
- Location: `src/DMAuth.Web/Auth/Requests/ExchangeTokenRequest.cs:13`
- Rule: external RFC 6749 §6 Refreshing an Access Token https://datatracker.ietf.org/doc/html/rfc6749#section-6
- Moves: n/a
- What: Change the `GrantType` summary to say it is `authorization_code` or `refresh_token`.
- Why: `AuthController.TokenAsync` dispatches `refresh_token` too, and the type's own summary says so; the property doc contradicts both.
- Verify: `grep -n 'Must be <c>authorization_code</c>' src/DMAuth.Web/Auth/Requests/ExchangeTokenRequest.cs` prints nothing.
- Evidence:
  ```text
  src/DMAuth.Web/Auth/Requests/ExchangeTokenRequest.cs:13:	///		The OAuth 2.0 grant type. Must be <c>authorization_code</c>.
  ```
