# Audit: backend (T1)
Ref: 5b7d9a6

## Scope

`find <R>/src/DMAuth.Domain <R>/src/DMAuth.Application -type f -not -path '*/bin/*' -not -path '*/obj/*'`

## Files

- `src/DMAuth.Application/Common/Behaviors/ValidationBehavior.cs` — DA-009, DA-023
- `src/DMAuth.Application/Common/Interfaces/IPasswordHasher.cs` — no finding
- `src/DMAuth.Application/Common/Interfaces/ITokenService.cs` — no finding
- `src/DMAuth.Application/Common/Interfaces/IUnitOfWork.cs` — no finding
- `src/DMAuth.Application/Common/Results/Extensions/ResultExtensions.cs` — no finding
- `src/DMAuth.Application/Common/Results/Extensions/TypedResultExtensions.cs` — no finding
- `src/DMAuth.Application/Common/Results/Factories/ResultFactories.cs` — DA-018
- `src/DMAuth.Application/Common/Results/Factories/TypedResultFactories.cs` — DA-018
- `src/DMAuth.Application/Common/Results/Result.cs` — no finding
- `src/DMAuth.Application/Common/Results/ResultError.cs` — no finding
- `src/DMAuth.Application/Common/Results/TypedResult.cs` — no finding
- `src/DMAuth.Application/Common/Settings/JwtSettings.cs` — DA-019, DA-026
- `src/DMAuth.Application/Common/Settings/OAuthSettings.cs` — DA-006
- `src/DMAuth.Application/DMAuth.Application.csproj` — DA-024
- `src/DMAuth.Application/DependencyInjection.cs` — no finding
- `src/DMAuth.Application/Features/Auth/Authorize/AuthorizeCommand.cs` — DA-022
- `src/DMAuth.Application/Features/Auth/Authorize/AuthorizeHandler.cs` — DA-007
- `src/DMAuth.Application/Features/Auth/Authorize/AuthorizeResponse.cs` — DA-022
- `src/DMAuth.Application/Features/Auth/Authorize/AuthorizeValidator.cs` — DA-006, DA-021
- `src/DMAuth.Application/Features/Auth/ExchangeToken/ExchangeTokenCommand.cs` — no finding
- `src/DMAuth.Application/Features/Auth/ExchangeToken/ExchangeTokenHandler.cs` — DA-001, DA-003, DA-007, DA-025, DA-026
- `src/DMAuth.Application/Features/Auth/ExchangeToken/ExchangeTokenResponse.cs` — DA-022
- `src/DMAuth.Application/Features/Auth/ExchangeToken/ExchangeTokenValidator.cs` — DA-008, DA-021
- `src/DMAuth.Application/Features/Auth/GetAuthorizationDetails/GetAuthorizationDetailsHandler.cs` — DA-007
- `src/DMAuth.Application/Features/Auth/GetAuthorizationDetails/GetAuthorizationDetailsQuery.cs` — no finding
- `src/DMAuth.Application/Features/Auth/GetAuthorizationDetails/GetAuthorizationDetailsResponse.cs` — no finding
- `src/DMAuth.Application/Features/Auth/GetAuthorizationDetails/GetAuthorizationDetailsValidator.cs` — DA-021
- `src/DMAuth.Application/Features/Auth/GetUserInfo/GetUserInfoHandler.cs` — no finding
- `src/DMAuth.Application/Features/Auth/GetUserInfo/GetUserInfoQuery.cs` — DA-013
- `src/DMAuth.Application/Features/Auth/GetUserInfo/UserInfoResponse.cs` — DA-013
- `src/DMAuth.Application/Features/Auth/GrantConsent/GrantConsentCommand.cs` — DA-022
- `src/DMAuth.Application/Features/Auth/GrantConsent/GrantConsentHandler.cs` — DA-002, DA-007, DA-010, DA-021, DA-025, DA-026
- `src/DMAuth.Application/Features/Auth/GrantConsent/GrantConsentResponse.cs` — DA-020
- `src/DMAuth.Application/Features/Auth/GrantConsent/GrantConsentValidator.cs` — DA-021
- `src/DMAuth.Application/Features/Auth/RevokeToken/RevokeTokenCommand.cs` — DA-004, DA-014, DA-020
- `src/DMAuth.Application/Features/Auth/RevokeToken/RevokeTokenHandler.cs` — DA-004, DA-014, DA-025
- `src/DMAuth.Application/Features/Auth/RevokeToken/RevokeTokenValidator.cs` — no finding
- `src/DMAuth.Application/Features/Auth/RotateToken/RotateTokenCommand.cs` — no finding
- `src/DMAuth.Application/Features/Auth/RotateToken/RotateTokenHandler.cs` — DA-001, DA-003, DA-025, DA-026
- `src/DMAuth.Application/Features/Auth/RotateToken/RotateTokenValidator.cs` — DA-021
- `src/DMAuth.Application/Features/Clients/Delete/DeleteClientCommand.cs` — no finding
- `src/DMAuth.Application/Features/Clients/Delete/DeleteClientHandler.cs` — DA-011
- `src/DMAuth.Application/Features/Clients/Delete/DeleteClientValidator.cs` — no finding
- `src/DMAuth.Application/Features/Clients/GetById/GetClientByIdHandler.cs` — no finding
- `src/DMAuth.Application/Features/Clients/GetById/GetClientByIdQuery.cs` — no finding
- `src/DMAuth.Application/Features/Clients/GetById/GetClientByIdResponse.cs` — no finding
- `src/DMAuth.Application/Features/Clients/GetById/GetClientByIdValidator.cs` — no finding
- `src/DMAuth.Application/Features/Clients/GetByOwner/GetClientsByOwnerHandler.cs` — no finding
- `src/DMAuth.Application/Features/Clients/GetByOwner/GetClientsByOwnerQuery.cs` — DA-012
- `src/DMAuth.Application/Features/Clients/GetByOwner/GetClientsByOwnerResponse.cs` — no finding
- `src/DMAuth.Application/Features/Clients/GetByOwner/GetClientsByOwnerValidator.cs` — no finding
- `src/DMAuth.Application/Features/Clients/Register/RegisterClientCommand.cs` — no finding
- `src/DMAuth.Application/Features/Clients/Register/RegisterClientHandler.cs` — DA-006, DA-007, DA-010, DA-025
- `src/DMAuth.Application/Features/Clients/Register/RegisterClientResponse.cs` — no finding
- `src/DMAuth.Application/Features/Clients/Register/RegisterClientValidator.cs` — no finding
- `src/DMAuth.Application/Features/Clients/UpdateRegistration/UpdateClientRegistrationCommand.cs` — no finding
- `src/DMAuth.Application/Features/Clients/UpdateRegistration/UpdateClientRegistrationHandler.cs` — DA-007, DA-010
- `src/DMAuth.Application/Features/Clients/UpdateRegistration/UpdateClientRegistrationValidator.cs` — no finding
- `src/DMAuth.Application/Features/Users/ChangePassword/ChangeUserPasswordCommand.cs` — no finding
- `src/DMAuth.Application/Features/Users/ChangePassword/ChangeUserPasswordHandler.cs` — DA-003, DA-010
- `src/DMAuth.Application/Features/Users/ChangePassword/ChangeUserPasswordValidator.cs` — no finding
- `src/DMAuth.Application/Features/Users/GetProfile/GetUserProfileHandler.cs` — no finding
- `src/DMAuth.Application/Features/Users/GetProfile/GetUserProfileQuery.cs` — no finding
- `src/DMAuth.Application/Features/Users/GetProfile/GetUserProfileResponse.cs` — no finding
- `src/DMAuth.Application/Features/Users/GetProfile/GetUserProfileValidator.cs` — no finding
- `src/DMAuth.Application/Features/Users/Login/LoginUserCommand.cs` — no finding
- `src/DMAuth.Application/Features/Users/Login/LoginUserHandler.cs` — DA-003, DA-005, DA-010
- `src/DMAuth.Application/Features/Users/Login/LoginUserResponse.cs` — no finding
- `src/DMAuth.Application/Features/Users/Login/LoginUserValidator.cs` — no finding
- `src/DMAuth.Application/Features/Users/Register/RegisterUserCommand.cs` — no finding
- `src/DMAuth.Application/Features/Users/Register/RegisterUserHandler.cs` — DA-010
- `src/DMAuth.Application/Features/Users/Register/RegisterUserResponse.cs` — no finding
- `src/DMAuth.Application/Features/Users/Register/RegisterUserValidator.cs` — no finding
- `src/DMAuth.Application/Features/Users/UpdateProfile/UpdateUserProfileCommand.cs` — no finding
- `src/DMAuth.Application/Features/Users/UpdateProfile/UpdateUserProfileHandler.cs` — no finding
- `src/DMAuth.Application/Features/Users/UpdateProfile/UpdateUserProfileValidator.cs` — no finding
- `src/DMAuth.Domain/DMAuth.Domain.csproj` — no finding
- `src/DMAuth.Domain/Entities/AuditableEntity.cs` — no finding
- `src/DMAuth.Domain/Entities/AuthorizationCode/AuthorizationCode.cs` — DA-016
- `src/DMAuth.Domain/Entities/AuthorizationCode/AuthorizationCodeMethods.cs` — DA-018
- `src/DMAuth.Domain/Entities/Client/Client.cs` — DA-001, DA-015
- `src/DMAuth.Domain/Entities/Client/ClientMethods.cs` — DA-011, DA-018
- `src/DMAuth.Domain/Entities/Consent/Consent.cs` — DA-016
- `src/DMAuth.Domain/Entities/Consent/ConsentMethods.cs` — DA-018
- `src/DMAuth.Domain/Entities/Entity.cs` — no finding
- `src/DMAuth.Domain/Entities/RefreshToken/RefreshToken.cs` — DA-016
- `src/DMAuth.Domain/Entities/RefreshToken/RefreshTokenMethods.cs` — DA-018
- `src/DMAuth.Domain/Entities/User/User.cs` — DA-015
- `src/DMAuth.Domain/Entities/User/UserMethods.cs` — DA-018
- `src/DMAuth.Domain/Enums/ClientType.cs` — DA-019
- `src/DMAuth.Domain/Enums/CodeChallengeMethod.cs` — no finding
- `src/DMAuth.Domain/Enums/ScopeType.cs` — no finding
- `src/DMAuth.Domain/Exceptions/DomainException.cs` — no finding
- `src/DMAuth.Domain/Interfaces/IAuthorizationCodeRepository.cs` — no finding
- `src/DMAuth.Domain/Interfaces/IClientRepository.cs` — DA-012, DA-022
- `src/DMAuth.Domain/Interfaces/IConsentRepository.cs` — no finding
- `src/DMAuth.Domain/Interfaces/IRefreshTokenRepository.cs` — DA-022
- `src/DMAuth.Domain/Interfaces/IRepository.cs` — DA-022
- `src/DMAuth.Domain/Interfaces/IUserRepository.cs` — DA-022
- `src/DMAuth.Domain/Policies/CodeChallengePolicy.cs` — DA-008
- `src/DMAuth.Domain/Policies/EmailPolicy.cs` — no finding
- `src/DMAuth.Domain/Policies/PasswordPolicy.cs` — no finding
- `src/DMAuth.Domain/Policies/PolicyResult.cs` — no finding
- `src/DMAuth.Domain/Policies/RedirectUriPolicy.cs` — no finding
- `src/DMAuth.Domain/Policies/ScopePolicy.cs` — DA-007
- `src/DMAuth.Domain/ValueObjects/CodeChallenge.cs` — DA-017
- `src/DMAuth.Domain/ValueObjects/Email.cs` — DA-017
- `src/DMAuth.Domain/ValueObjects/HashedPassword.cs` — DA-017
- `src/DMAuth.Domain/ValueObjects/RedirectUri.cs` — DA-017
- `src/DMAuth.Domain/ValueObjects/Scope.cs` — DA-017

## Rules checked

- `csharp-style.md` § Naming — conforms
- `csharp-style.md` § File organization — DA-018, DA-019
- `csharp-style.md` § Parameter layout — DA-020
- `csharp-style.md` § Declaration layout — conforms
- `csharp-style.md` § Lambdas, expression bodies and chaining — DA-021
- `csharp-style.md` § XML documentation comments — DA-022
- `dotnet.md` § Platform — n/a: no `<TargetFramework>` is declared in the two scoped csproj files (it is inherited from `Directory.Build.props`, a T7 path) and the Web layer is outside T1
- `dotnet.md` § Solution and layers — DA-013, DA-019
- `dotnet.md` § CQRS — DA-013, DA-014
- `dotnet.md` § Result type — DA-009
- `dotnet.md` § Entities — DA-015, DA-016
- `dotnet.md` § Value objects and policies — DA-007, DA-010, DA-017
- `dotnet.md` § Async and cancellation — conforms
- `dotnet.md` § Dependency injection — conforms
- `dotnet.md` § Error handling and exceptions — DA-009, DA-010, DA-011
- `dotnet.md` § HttpContext — n/a: `grep -rlnE "HttpContext|HttpClient|MustAsync|DbContext|Request\.Form" src/DMAuth.Domain src/DMAuth.Application` prints nothing
- `dotnet.md` § Requests and responses — n/a: no request or response stream is read in Domain or Application (same grep as HttpContext)
- `dotnet.md` § Performance and memory — conforms
- `dotnet.md` § HTTP clients — n/a: no `HttpClient` in Domain or Application (same grep as HttpContext)
- `dotnet.md` § Configuration and secrets — n/a: no appsettings file or connection string is in the T1 population
- `efcore.md` § Queries — DA-012

## Findings

### DA-001 Token endpoint never authenticates confidential clients
- Severity: correctness/security risk
- Layer: backend
- Location: `src/DMAuth.Application/Features/Auth/ExchangeToken/ExchangeTokenHandler.cs:36`; `src/DMAuth.Application/Features/Auth/RotateToken/RotateTokenHandler.cs:29`; `src/DMAuth.Domain/Entities/Client/Client.cs:25`
- Rule: external RFC 6749 §3.2.1 Client Authentication https://www.rfc-editor.org/rfc/rfc6749#section-3.2.1
- Moves: n/a
- What: Add a client secret to ExchangeTokenCommand and RotateTokenCommand and, when the client is `ClientType.Confidential`, verify it against `Client.ClientSecretHash` through `IPasswordHasher.Verify` before any code or refresh token is looked up, returning `Unauthorized("invalid_client")` on mismatch.
- Why: Both handlers resolve the client from the public `client_id` alone; `ClientSecretHash` is written at registration and read by nothing in Application or Web, so a confidential client's secret protects nothing and a stolen code or refresh token is redeemable by anyone.
- Verify: a handler unit test sends a confidential client's code with a wrong secret and expects `ResultError.Unauthorized`; `grep -rn "ClientSecretHash" src/DMAuth.Application --include=*.cs` prints at least one line.
- Census: 0 via `grep -rn "ClientSecretHash" src/DMAuth.Application src/DMAuth.Web --include=*.cs | wc -l`
- Evidence:
  ```
  36:		var client = await clientRepository.FindByClientIdAsync(
  29:		var client = await clientRepository.FindByClientIdAsync(
  25:	public string? ClientSecretHash { get; private set; }
  ```

### DA-002 Consent grant trusts the form's redirect_uri and scopes
- Severity: correctness/security risk
- Layer: backend
- Location: `src/DMAuth.Application/Features/Auth/GrantConsent/GrantConsentHandler.cs:47`; `src/DMAuth.Application/Features/Auth/GrantConsent/GrantConsentHandler.cs:81`
- Rule: external RFC 9700 §4.1 Insufficient Redirect URI Validation https://www.rfc-editor.org/rfc/rfc9700#section-4.1
- Moves: n/a
- What: In GrantConsentHandler, reject the command unless `request.RedirectUri` is in `client.RedirectUris` (exact match) and every `request.GrantedScopes` entry is in `client.AllowedScopes`, as AuthorizeHandler already does, returning `Invalid` before the authorization code is created.
- Why: `POST /consent` builds GrantConsentCommand straight from form fields, and the handler stores and issues a code for whatever redirect URI and scopes arrive, so the only redirect-URI check (AuthorizeHandler) is bypassable by posting the consent form directly.
- Verify: a GrantConsentHandler unit test with an unregistered redirect URI expects a failed result and no `authorizationCodeRepository.Add` call.
- Evidence:
  ```
  47:		var scopeString = string.Join(" ", request.GrantedScopes);
  81:			request.RedirectUri,
  ```

### DA-003 Refresh tokens survive user deactivation and password change
- Severity: correctness/security risk
- Layer: backend
- Location: `src/DMAuth.Application/Features/Auth/RotateToken/RotateTokenHandler.cs:80`; `src/DMAuth.Application/Features/Auth/ExchangeToken/ExchangeTokenHandler.cs:115`; `src/DMAuth.Application/Features/Users/ChangePassword/ChangeUserPasswordHandler.cs:44`; `src/DMAuth.Application/Features/Users/Login/LoginUserHandler.cs:36`
- Rule: external RFC 9700 §4.14.2 Refresh Token Protection Recommendations https://www.rfc-editor.org/rfc/rfc9700#section-4.14.2
- Moves: n/a
- What: Load the user in RotateTokenHandler and ExchangeTokenHandler (not only when `openid` is granted) and refuse with `Unauthorized("invalid_grant")` when `!user.IsActive`; in ChangeUserPasswordHandler revoke the user's outstanding refresh tokens after `ChangePassword`.
- Why: Only LoginUserHandler checks the user's `IsActive` (`grep -rn "user.IsActive" src/DMAuth.Application` prints only LoginUserHandler.cs:36); rotation never loads the user, so a deactivated account keeps minting access tokens for the 30-day refresh lifetime, and a password change leaves every previously issued refresh token valid.
- Verify: a RotateTokenHandler unit test with an inactive user expects `ResultError.Unauthorized`; a ChangeUserPasswordHandler test asserts the user's refresh tokens are revoked.
- Evidence:
  ```
  80:		var (plainToken, newTokenHash) = tokenService.GenerateRefreshToken();
  115:		if (scopes.Contains("openid"))
  44:		user.ChangePassword(passwordHasher.Hash(request.NewPassword));
  36:		if (!user.IsActive)
  ```

### DA-004 Revocation is not bound to the requesting client
- Severity: correctness/security risk
- Layer: backend
- Location: `src/DMAuth.Application/Features/Auth/RevokeToken/RevokeTokenCommand.cs:13`; `src/DMAuth.Application/Features/Auth/RevokeToken/RevokeTokenHandler.cs:32`
- Rule: external RFC 7009 §2.1 Revocation Request https://www.rfc-editor.org/rfc/rfc7009#section-2.1
- Moves: n/a
- What: Add the client identifier (and secret for confidential clients) to RevokeTokenCommand, resolve the client, and revoke only when `token.ClientId == client.Id`, still returning success otherwise.
- Why: The command carries only the token, so the handler revokes any refresh token for any caller; RFC 7009 requires the server to authenticate the client and check the token was issued to it.
- Verify: a RevokeTokenHandler unit test presenting another client's token expects the token to stay unrevoked.
- Evidence:
  ```
  13:public record RevokeTokenCommand(string Token)
  32:		if (token is not null && !token.RevokedAt.HasValue)
  ```

### DA-005 Login skips password hashing for unknown emails
- Severity: correctness/security risk
- Layer: backend
- Location: `src/DMAuth.Application/Features/Users/Login/LoginUserHandler.cs:29`
- Rule: external OWASP Authentication Cheat Sheet, Authentication and Error Messages https://cheatsheetseries.owasp.org/cheatsheets/Authentication_Cheat_Sheet.html
- Moves: n/a
- What: When `user is null`, still run `passwordHasher.Verify` against a fixed dummy `HashedPassword` before returning the invalid-credentials result, so both branches cost one hash verification.
- Why: The short-circuit `user is null || ...` returns without hashing, so response time distinguishes a registered email from an unregistered one even though the message is identical.
- Verify: a LoginUserHandler unit test with an unknown email asserts `IPasswordHasher.Verify` was called once.
- Evidence:
  ```
  29:		if (user is null || !passwordHasher.Verify(
  ```

### DA-006 Client registration hard-codes the configurable client_id prefix
- Severity: correctness/security risk
- Layer: backend
- Location: `src/DMAuth.Application/Features/Clients/Register/RegisterClientHandler.cs:82`; `src/DMAuth.Application/Common/Settings/OAuthSettings.cs:12`; `src/DMAuth.Application/Features/Auth/Authorize/AuthorizeValidator.cs:25`
- Rule: base backend § Configuration
- Moves: code
- What: Inject `OAuthSettings` into RegisterClientHandler and build the identifier from `oauthSettings.ClientIdPrefix` instead of the literal `"dmauth_"`.
- Why: Five validators (the five `.Must(id => id.StartsWith(prefix, ...))` sites listed in DA-021) reject any `client_id` not starting with the configured `ClientIdPrefix`, but registration always issues `dmauth_…`; setting a different prefix makes every newly registered client fail authorization and token requests.
- Verify: `grep -n '"dmauth_"' src/DMAuth.Application/Features/Clients/Register/RegisterClientHandler.cs` prints nothing, and a handler test with a non-default prefix asserts the generated id starts with it.
- Evidence:
  ```
  82:			clientId = "dmauth_" + Base64Url.EncodeToString(RandomNumberGenerator.GetBytes(16));
  12:	public string ClientIdPrefix { get; set; } = "dmauth_";
  25:			.Must(id => id.StartsWith(prefix, StringComparison.Ordinal))
  ```

### DA-007 Scopes are validated case-insensitively but stored and compared case-sensitively
- Severity: correctness/security risk
- Layer: backend
- Location: `src/DMAuth.Domain/Policies/ScopePolicy.cs:38`; `src/DMAuth.Application/Features/Clients/Register/RegisterClientHandler.cs:63`; `src/DMAuth.Application/Features/Clients/UpdateRegistration/UpdateClientRegistrationHandler.cs:58`; `src/DMAuth.Application/Features/Auth/Authorize/AuthorizeHandler.cs:54`; `src/DMAuth.Application/Features/Auth/GetAuthorizationDetails/GetAuthorizationDetailsHandler.cs:44`; `src/DMAuth.Application/Features/Auth/GrantConsent/GrantConsentHandler.cs:62`; `src/DMAuth.Application/Features/Auth/ExchangeToken/ExchangeTokenHandler.cs:115`
- Rule: discipline .sir/disciplines/dotnet.md § Value objects and policies
- Moves: code
- What: Construct the `Scope` value object (which lowercases after the policy check) for each allowed scope in RegisterClientHandler and UpdateClientRegistrationHandler and store its `Value`, and use one comparer (ordinal on the normalized value) in every scope comparison.
- Why: ScopePolicy accepts `OpenID`, the handlers store the raw string, and AuthorizeHandler, GrantConsentHandler and ExchangeTokenHandler compare ordinally while GetAuthorizationDetailsHandler ignores case, so a client registered with `OpenID` cannot request `openid` and never gets an ID token; `grep -rn "new Scope(" src --include=*.cs` prints nothing, so the normalizing value object is never used.
- Verify: a RegisterClientHandler unit test registering `OpenID` asserts the stored scope is `openid`; `grep -rn "new Scope(" src/DMAuth.Application` prints at least one line.
- Evidence:
  ```
  38:		if (!_allowedScopes.Contains(value.ToLowerInvariant()))
  63:			request.AllowedScopes,
  58:			request.AllowedScopes);
  54:				!client.AllowedScopes.Contains(requestedScope))
  44:			.ToHashSet(StringComparer.OrdinalIgnoreCase);
  62:				.ToHashSet();
  115:		if (scopes.Contains("openid"))
  ```

### DA-008 PKCE challenge and verifier formats are not validated per RFC 7636
- Severity: correctness/security risk
- Layer: backend
- Location: `src/DMAuth.Domain/Policies/CodeChallengePolicy.cs:29`; `src/DMAuth.Domain/Policies/CodeChallengePolicy.cs:38`; `src/DMAuth.Domain/Policies/CodeChallengePolicy.cs:49`; `src/DMAuth.Application/Features/Auth/ExchangeToken/ExchangeTokenValidator.cs:42`
- Rule: external RFC 7636 §4.1 and §4.2 https://www.rfc-editor.org/rfc/rfc7636#section-4.2
- Moves: n/a
- What: Require an S256 challenge to be exactly 43 base64url characters (`A-Z a-z 0-9 - _`, no `.`), fix the message that calls `.` a Base64url character, and validate `code_verifier` as 43-128 characters of `A-Z a-z 0-9 - . _ ~` in ExchangeTokenValidator.
- Why: The policy accepts 43-128 characters including `.`, which no S256 challenge can contain, so malformed challenges are stored and only fail later as a verifier mismatch, and any non-empty verifier reaches the hash comparison.
- Verify: CodeChallengePolicy unit tests reject a 44-character value and a value containing `.`; an ExchangeTokenValidator test rejects a 42-character verifier.
- Evidence:
  ```
  29:		if (value.Length is < MinimumLength or > MaximumLength)
  38:				"Code challenge must use only Base64url characters (A-Z, a-z, 0-9, '-', '_', '.').");
  49:			|| character is '-' or '_' or '.');
  42:			.NotEmpty();
  ```

### DA-009 Validation failures are thrown, contrary to the result-type rule
- Severity: convention drift
- Layer: backend
- Location: `src/DMAuth.Application/Common/Behaviors/ValidationBehavior.cs:51`; `.sir/disciplines/dotnet.md:53`; `.sir/disciplines/dotnet.md:100`
- Rule: discipline .sir/disciplines/dotnet.md § Result type; discipline .sir/disciplines/dotnet.md § Error handling and exceptions
- Moves: discipline
- What: Restate dotnet.md:53 and dotnet.md:100 to carve out pipeline validation: FluentValidation failures raised by `ValidationBehavior` as `ValidationException` and mapped to HTTP 400 by the exception middleware are the sanctioned path, and handlers still return results for every other expected failure.
- Why: Every request's validator failures reach the caller through the thrown `ValidationException`, a deliberate, documented design that the two rules forbid outright, so the rules misdescribe the code every validator relies on.
- Verify: `sed -n '53p;100p' .sir/disciplines/dotnet.md` names the ValidationBehavior exception path as allowed.
- Evidence:
  ```
  51:			throw new ValidationException(failures);
  53:- Communicate expected failures (not found, conflict, validation error, unauthorized, forbidden) with the result type, never with an exception.
  100:- Never throw for user validation errors, not-found lookups, wrong credentials, or business checks that are expected to fail; return a failed result.
  ```

### DA-010 Policies are checked in handlers instead of validators
- Severity: convention drift
- Layer: backend
- Location: `src/DMAuth.Application/Features/Users/Register/RegisterUserHandler.cs:25`; `src/DMAuth.Application/Features/Users/Register/RegisterUserHandler.cs:31`; `src/DMAuth.Application/Features/Users/ChangePassword/ChangeUserPasswordHandler.cs:38`; `src/DMAuth.Application/Features/Clients/Register/RegisterClientHandler.cs:30`; `src/DMAuth.Application/Features/Clients/Register/RegisterClientHandler.cs:39`; `src/DMAuth.Application/Features/Clients/UpdateRegistration/UpdateClientRegistrationHandler.cs:39`; `src/DMAuth.Application/Features/Clients/UpdateRegistration/UpdateClientRegistrationHandler.cs:48`; `src/DMAuth.Application/Features/Auth/GrantConsent/GrantConsentHandler.cs:83`; `src/DMAuth.Application/Features/Users/Login/LoginUserHandler.cs:26`
- Rule: discipline .sir/disciplines/dotnet.md § Value objects and policies; discipline .sir/disciplines/dotnet.md § Error handling and exceptions
- Moves: code
- What: Move each `PasswordPolicy`, `RedirectUriPolicy`, `ScopePolicy`, `EmailPolicy` and `CodeChallengePolicy` check into the matching validator as a `.Must(...)` rule carrying the policy's violation summary, and delete the handler-side loops.
- Why: Policy failures surface as one `Invalid` result instead of per-field validation errors, and where no validator runs the policy (GrantConsent's code challenge, Login's email length) user input reaches the value-object constructor and is reported by a thrown `DomainException`.
- Verify: `grep -rn "Policy.Validate" src/DMAuth.Application/Features --include=*Handler.cs` prints only AuthorizeHandler (or nothing), and each listed validator references the policy.
- Evidence:
  ```
  25:		var policyResult = PasswordPolicy.Validate(request.Password);
  31:		var email = new Email(request.Email);
  38:		var policyResult = PasswordPolicy.Validate(request.NewPassword);
  30:			var uriResult = RedirectUriPolicy.Validate(uri);
  39:			var scopeResult = ScopePolicy.Validate(scope);
  39:			var uriResult = RedirectUriPolicy.Validate(uri);
  48:			var scopeResult = ScopePolicy.Validate(scope);
  83:			new CodeChallenge(request.CodeChallenge),
  26:			new Email(request.Email),
  ```

### DA-011 Deleting an already-inactive client throws instead of returning a result
- Severity: convention drift
- Layer: backend
- Location: `src/DMAuth.Application/Features/Clients/Delete/DeleteClientHandler.cs:35`; `src/DMAuth.Domain/Entities/Client/ClientMethods.cs:42`
- Rule: discipline .sir/disciplines/dotnet.md § Error handling and exceptions
- Moves: code
- What: In DeleteClientHandler, return a failed `Result` (for example `Conflict`) when `!client.IsActive` before calling `client.Deactivate()`, as RevokeTokenHandler checks `RevokedAt` before `Revoke()`.
- Why: A repeated delete is an expected outcome, yet it reaches `Deactivate()`'s `DomainException.ThrowIf(!IsActive, ...)` and is reported through an exception.
- Verify: a DeleteClientHandler unit test on an inactive client expects a failed result and no exception.
- Evidence:
  ```
  35:		client.Deactivate();
  42:			!IsActive,
  ```

### DA-012 Client list query is unbounded
- Severity: convention drift
- Layer: backend
- Location: `src/DMAuth.Application/Features/Clients/GetByOwner/GetClientsByOwnerQuery.cs:12`; `src/DMAuth.Domain/Interfaces/IClientRepository.cs:21`
- Rule: discipline .sir/disciplines/efcore.md § Queries
- Moves: code
- What: Add page and page-size parameters to GetClientsByOwnerQuery (validated in GetClientsByOwnerValidator) and to `IClientRepository.FindByOwnerIdAsync`, ordering before `Skip`/`Take`.
- Why: The query backing the owner's client list returns every registration in one call, which the rule forbids for list endpoints.
- Verify: `grep -n "page" src/DMAuth.Application/Features/Clients/GetByOwner/GetClientsByOwnerQuery.cs` prints the new parameters.
- Evidence:
  ```
  12:public record GetClientsByOwnerQuery(
  21:	public Task<List<Client>> FindByOwnerIdAsync(
  ```

### DA-013 GetUserInfo lacks a validator and its response is misnamed
- Severity: convention drift
- Layer: backend
- Location: `src/DMAuth.Application/Features/Auth/GetUserInfo/GetUserInfoQuery.cs:17`; `src/DMAuth.Application/Features/Auth/GetUserInfo/UserInfoResponse.cs:10`
- Rule: discipline .sir/disciplines/dotnet.md § CQRS; discipline .sir/disciplines/dotnet.md § Solution and layers
- Moves: code
- What: Add `GetUserInfoValidator` (`AbstractValidator<GetUserInfoQuery>`, `query =>`) covering `UserId` and `Scope`, and rename `UserInfoResponse` to `GetUserInfoResponse` with its usages.
- Why: Of the five `*Query.cs` files the T1 scope command prints, it is the one whose folder holds no `*Validator.cs`, and its response is the one `*Response.cs` in Features not named `{Operation}Response`.
- Verify: `ls src/DMAuth.Application/Features/Auth/GetUserInfo` lists `GetUserInfoHandler.cs`, `GetUserInfoQuery.cs`, `GetUserInfoResponse.cs` and `GetUserInfoValidator.cs`.
- Evidence:
  ```
  17:public record GetUserInfoQuery(
  10:public record UserInfoResponse
  ```

### DA-014 RevokeToken returns a constant boolean payload
- Severity: convention drift
- Layer: backend
- Location: `src/DMAuth.Application/Features/Auth/RevokeToken/RevokeTokenCommand.cs:14`; `src/DMAuth.Application/Features/Auth/RevokeToken/RevokeTokenHandler.cs:38`
- Rule: discipline .sir/disciplines/dotnet.md § CQRS
- Moves: code
- What: Change RevokeTokenCommand to `IRequest<Result>` and return `Result.Success()`.
- Why: The handler always returns `TypedResult<bool>.Success(true)`; the value carries no information, and the rule reserves `TypedResult<T>` for commands that return a value.
- Verify: `grep -n "TypedResult<bool>" src/DMAuth.Application/Features/Auth/RevokeToken/*.cs` prints nothing.
- Evidence:
  ```
  14:	: IRequest<TypedResult<bool>>;
  38:		return TypedResult<bool>.Success(true);
  ```

### DA-015 Entity constructors do not enforce their stated invariants
- Severity: convention drift
- Layer: backend
- Location: `src/DMAuth.Domain/Entities/Client/Client.cs:81`; `src/DMAuth.Domain/Entities/Client/Client.cs:90`; `src/DMAuth.Domain/Entities/User/User.cs:61`
- Rule: discipline .sir/disciplines/dotnet.md § Entities
- Moves: code
- What: Throw a `DomainException` from the Client constructor when a confidential client has no secret hash or a public client has one, and when name, redirect URIs or scopes are empty; in User, reject an empty username and display name.
- Why: The documentation says the secret hash is required for confidential clients, but the constructor accepts any combination, so the invariant rests on each caller.
- Verify: Domain unit tests constructing a confidential Client with `clientSecretHash: null` expect `DomainException`.
- Evidence:
  ```
  81:	///		The hashed client secret, required for confidential clients.
  90:		string? clientSecretHash = null)
  61:		string username,
  ```

### DA-016 Single-transition entities have no last-modified timestamp
- Severity: convention drift
- Layer: backend
- Location: `src/DMAuth.Domain/Entities/AuthorizationCode/AuthorizationCode.cs:10`; `src/DMAuth.Domain/Entities/RefreshToken/RefreshToken.cs:7`; `src/DMAuth.Domain/Entities/Consent/Consent.cs:7`; `.sir/disciplines/dotnet.md:62`
- Rule: discipline .sir/disciplines/dotnet.md § Entities
- Moves: discipline
- What: Restate dotnet.md:62 so that entities whose state changes are recorded by their own transition timestamps (`UsedAt`, `RevokedAt`, `GrantedAt`) derive from `Entity`, and only entities with general updates derive from `AuditableEntity` with `UpdatedAt`.
- Why: AuthorizationCode, RefreshToken and Consent all have mutating methods yet derive from `Entity`, consistently using transition timestamps instead of `UpdatedAt`; the rule as written condemns a uniform, intentional design.
- Verify: `sed -n '62p' .sir/disciplines/dotnet.md` names the transition-timestamp case.
- Evidence:
  ```
  10:	: Entity
  7:	: Entity
  7:	: Entity
  62:- Give mutable entities a last-modified timestamp, updated on every state change.
  ```

### DA-017 Value objects use get-only properties, not the private setter or init the rule names
- Severity: convention drift
- Layer: backend
- Location: `src/DMAuth.Domain/ValueObjects/Email.cs:14`; `src/DMAuth.Domain/ValueObjects/Scope.cs:14`; `src/DMAuth.Domain/ValueObjects/RedirectUri.cs:14`; `src/DMAuth.Domain/ValueObjects/CodeChallenge.cs:14`; `src/DMAuth.Domain/ValueObjects/HashedPassword.cs:13`; `.sir/disciplines/dotnet.md:71`
- Rule: discipline .sir/disciplines/dotnet.md § Value objects and policies
- Moves: discipline
- What: Restate dotnet.md:71 to allow a get-only property assigned in the constructor alongside a private setter or `init`.
- Why: All five value objects use `{ get; }`, which is stricter than either form the rule names; the code is uniform and the rule's wording is what disagrees.
- Verify: `sed -n '71p' .sir/disciplines/dotnet.md` mentions get-only properties.
- Evidence:
  ```
  14:	public string Value { get; }
  14:	public string Value { get; }
  14:	public string Value { get; }
  14:	public string Value { get; }
  13:	public string Value { get; }
  71:- A value object is an immutable record whose state is set once through a private setter or `init`-only property.
  ```

### DA-018 Partial types are split across files named for their role
- Severity: convention drift
- Layer: backend
- Location: `src/DMAuth.Domain/Entities/AuthorizationCode/AuthorizationCodeMethods.cs:5`; `src/DMAuth.Domain/Entities/Client/ClientMethods.cs:6`; `src/DMAuth.Domain/Entities/Consent/ConsentMethods.cs:3`; `src/DMAuth.Domain/Entities/RefreshToken/RefreshTokenMethods.cs:5`; `src/DMAuth.Domain/Entities/User/UserMethods.cs:6`; `src/DMAuth.Application/Common/Results/Factories/ResultFactories.cs:3`; `src/DMAuth.Application/Common/Results/Factories/TypedResultFactories.cs:3`; `.sir/disciplines/csharp-style.md:25`; `.sir/disciplines/csharp-style.md:26`
- Rule: discipline .sir/disciplines/csharp-style.md § File organization
- Moves: discipline
- What: Restate csharp-style.md:25 and :26 to allow a partial type's additional parts in `{Type}{Role}.cs` files (`Methods`, `Factories`), with the namespace of the primary declaration even when the part sits in a subfolder.
- Why: Every entity and both result types use this split, so file names differ from the type name and `Factories/` holds types in the `DMAuth.Application.Common.Results` namespace; the pattern is uniform and the rules do not describe it.
- Verify: `sed -n '25,26p' .sir/disciplines/csharp-style.md` names the partial-part file pattern.
- Evidence:
  ```
  5:public partial class AuthorizationCode
  6:public partial class Client
  3:public partial class Consent
  5:public partial class RefreshToken
  6:public partial class User
  3:public partial class Result
  3:public sealed partial class TypedResult<T>
  25:- Name the file exactly after the type: `RegisterUserCommand.cs`.
  26:- Mirror the namespace hierarchy in the folder structure.
  ```

### DA-019 Layer folder lists omit Enums/ and Common/Settings/
- Severity: convention drift
- Layer: backend
- Location: `src/DMAuth.Domain/Enums/ClientType.cs:1`; `src/DMAuth.Application/Common/Settings/JwtSettings.cs:1`; `.sir/disciplines/dotnet.md:14`; `.sir/disciplines/dotnet.md:15`
- Rule: discipline .sir/disciplines/dotnet.md § Solution and layers
- Moves: discipline
- What: Add `Enums/` to the Domain folder list at dotnet.md:14 and `Common/Settings/` (typed options classes) to the Application list at dotnet.md:15.
- Why: Domain holds three enums in `Enums/` and Application holds its options classes in `Common/Settings/`, neither of which the layer layout names, so the rule does not describe the existing tree.
- Verify: `sed -n '14,15p' .sir/disciplines/dotnet.md` names `Enums/` and `Common/Settings/`.
- Evidence:
  ```
  1:namespace DMAuth.Domain.Enums;
  1:namespace DMAuth.Application.Common.Settings;
  14:  - Domain by technical category: `Entities/{Entity}/`, `ValueObjects/`, `Policies/`, `Events/`, `Exceptions/`, `Interfaces/`.
  15:  - Application by feature: `Common/Interfaces/` (application service interfaces), `Common/Behaviors/` (pipeline behaviors), `Common/Results/` (result and error types), `Features/{Feature}/`, and a `DependencyInjection` class.
  ```

### DA-020 Single-parameter records keep the parameter inline
- Severity: convention drift
- Layer: backend
- Location: `src/DMAuth.Application/Features/Auth/RevokeToken/RevokeTokenCommand.cs:13`; `src/DMAuth.Application/Features/Auth/GrantConsent/GrantConsentResponse.cs:11`
- Rule: discipline .sir/disciplines/csharp-style.md § Parameter layout
- Moves: code
- What: Put the single primary-constructor parameter of RevokeTokenCommand and GrantConsentResponse on its own line, as the other single-parameter records (`RegisterUserResponse`, `GetUserProfileQuery`) do.
- Why: The rule requires primary-constructor parameters one per line at any count, including one.
- Verify: `grep -rnE "^public record [A-Za-z]+\([^)]" src/DMAuth.Application` prints nothing.
- Evidence:
  ```
  13:public record RevokeTokenCommand(string Token)
  11:public record GrantConsentResponse(string PlainCode);
  ```

### DA-021 Lambda bodies sit on the same line as =>
- Severity: convention drift
- Layer: backend
- Location: `src/DMAuth.Application/Features/Auth/Authorize/AuthorizeValidator.cs:25`; `src/DMAuth.Application/Features/Auth/ExchangeToken/ExchangeTokenValidator.cs:33`; `src/DMAuth.Application/Features/Auth/GetAuthorizationDetails/GetAuthorizationDetailsValidator.cs:29`; `src/DMAuth.Application/Features/Auth/GrantConsent/GrantConsentValidator.cs:25`; `src/DMAuth.Application/Features/Auth/RotateToken/RotateTokenValidator.cs:25`; `src/DMAuth.Application/Features/Auth/GrantConsent/GrantConsentHandler.cs:65`
- Rule: discipline .sir/disciplines/csharp-style.md § Lambdas, expression bodies and chaining
- Moves: code
- What: Break each listed lambda after `=>` and indent the body one level.
- Why: The rule puts every lambda body on the line after `=>`; the Census command finds these six sites in Domain and Application.
- Verify: the Census command prints 0.
- Census: 6 via `grep -rnE '[a-zA-Z]+ => [^ ]' src/DMAuth.Domain src/DMAuth.Application --include=*.cs | wc -l`
- Evidence:
  ```
  25:			.Must(id => id.StartsWith(prefix, StringComparison.Ordinal))
  33:			.Must(id => id.StartsWith(prefix, StringComparison.Ordinal))
  29:			.Must(id => id.StartsWith(prefix, StringComparison.Ordinal))
  25:			.Must(id => id.StartsWith(prefix, StringComparison.Ordinal))
  25:			.Must(id => id.StartsWith(prefix, StringComparison.Ordinal))
  65:				.All(scope => alreadyGranted.Contains(scope));
  ```

### DA-022 Public parameters without XML documentation
- Severity: convention drift
- Layer: backend
- Location: `src/DMAuth.Domain/Interfaces/IRepository.cs:17`; `src/DMAuth.Domain/Interfaces/IRepository.cs:24`; `src/DMAuth.Domain/Interfaces/IClientRepository.cs:14`; `src/DMAuth.Domain/Interfaces/IUserRepository.cs:15`; `src/DMAuth.Domain/Interfaces/IRefreshTokenRepository.cs:21`; `src/DMAuth.Application/Features/Auth/Authorize/AuthorizeCommand.cs:39`; `src/DMAuth.Application/Features/Auth/GrantConsent/GrantConsentCommand.cs:39`; `src/DMAuth.Application/Features/Auth/Authorize/AuthorizeResponse.cs:36`; `src/DMAuth.Application/Features/Auth/ExchangeToken/ExchangeTokenResponse.cs:27`
- Rule: discipline .sir/disciplines/csharp-style.md § XML documentation comments
- Moves: code
- What: Add `<param>` tags to every member of the Domain repository interfaces, a `Nonce` `<param>` to AuthorizeCommand, GrantConsentCommand and AuthorizeResponse, and a `TokenType` `<param>` to ExchangeTokenResponse.
- Why: The rule requires documentation for every public parameter; `grep -rn '<param' src/DMAuth.Domain/Interfaces` prints nothing across the 14 members the Census counts, and the four listed record parameters have no `<param>` tag in their files.
- Verify: `grep -rn '<param' src/DMAuth.Domain/Interfaces | wc -l` prints a non-zero count, and `grep -c 'name="Nonce"' src/DMAuth.Application/Features/Auth/Authorize/AuthorizeCommand.cs` prints 1.
- Census: 14 via `grep -rE '^\s*public .*\(' src/DMAuth.Domain/Interfaces | wc -l`
- Evidence:
  ```
  17:	public Task<T?> FindByIdAsync(
  24:	public void Add(T entity);
  14:	public Task<Client?> FindByClientIdAsync(
  15:	public Task<User?> FindByEmailAsync(
  21:	public Task RevokeByTokenFamilyAsync(
  39:	string? Nonce = null)
  39:	string? Nonce = null)
  36:	string? Nonce = null);
  27:	[property: JsonPropertyName("token_type")]    string TokenType,
  ```

### DA-023 Validation behavior runs validators synchronously and sequentially
- Severity: hygiene
- Layer: backend
- Location: `src/DMAuth.Application/Common/Behaviors/ValidationBehavior.cs:42`
- Rule: base backend § Async discipline; external FluentValidation, Asynchronous Validation https://docs.fluentvalidation.net/en/latest/async.html
- Moves: code
- What: Replace `validator.Validate(context)` with `await Task.WhenAll(validators.Select(validator => validator.ValidateAsync(context, cancellationToken)))`.
- Why: FluentValidation throws when a validator with async rules is invoked through `Validate`, so the first `MustAsync` rule anyone adds breaks the pipeline, and the cancellation token is not passed to validation.
- Verify: `grep -n "ValidateAsync" src/DMAuth.Application/Common/Behaviors/ValidationBehavior.cs` prints a line.
- Evidence:
  ```
  42:				validator.Validate(context))
  ```

### DA-024 Application project carries unused JWT packages and stale project entries
- Severity: hygiene
- Layer: backend
- Location: `src/DMAuth.Application/DMAuth.Application.csproj:8`; `src/DMAuth.Application/DMAuth.Application.csproj:11`; `src/DMAuth.Application/DMAuth.Application.csproj:12`; `src/DMAuth.Application/DMAuth.Application.csproj:16`
- Rule: external Common web application architectures, Clean architecture https://learn.microsoft.com/en-us/dotnet/architecture/modern-web-apps-azure/common-web-application-architectures
- Moves: n/a
- What: Remove the `Microsoft.IdentityModel.Tokens` and `System.IdentityModel.Tokens.Jwt` references (Infrastructure and Web reference what they use), delete the `<Folder Include="Common\Results\" />` item, and indent the FluentValidation reference like its siblings.
- Why: No `.cs` file in Application uses IdentityModel, so the references only pull token libraries into the application core; the `Folder` item is a placeholder for a folder that now holds files.
- Verify: `grep -n "IdentityModel\|<Folder" src/DMAuth.Application/DMAuth.Application.csproj` prints nothing and `dotnet build src/DM-Auth.sln` succeeds.
- Census: 0 via `grep -rn "IdentityModel" src/DMAuth.Application --include=*.cs | wc -l`
- Evidence:
  ```
  8:<PackageReference Include="FluentValidation" Version="12.1.1" />
  11:    <PackageReference Include="Microsoft.IdentityModel.Tokens" Version="8.16.0" />
  12:    <PackageReference Include="System.IdentityModel.Tokens.Jwt" Version="8.16.0" />
  16:    <Folder Include="Common\Results\" />
  ```

### DA-025 Token hashing and secret generation are duplicated across handlers
- Severity: hygiene
- Layer: backend
- Location: `src/DMAuth.Application/Features/Auth/ExchangeToken/ExchangeTokenHandler.cs:53`; `src/DMAuth.Application/Features/Auth/ExchangeToken/ExchangeTokenHandler.cs:94`; `src/DMAuth.Application/Features/Auth/GrantConsent/GrantConsentHandler.cs:74`; `src/DMAuth.Application/Features/Auth/GrantConsent/GrantConsentHandler.cs:75`; `src/DMAuth.Application/Features/Auth/RevokeToken/RevokeTokenHandler.cs:26`; `src/DMAuth.Application/Features/Auth/RotateToken/RotateTokenHandler.cs:46`; `src/DMAuth.Application/Features/Clients/Register/RegisterClientHandler.cs:53`; `src/DMAuth.Application/Features/Clients/Register/RegisterClientHandler.cs:82`
- Rule: external Common web application architectures, Clean architecture https://learn.microsoft.com/en-us/dotnet/architecture/modern-web-apps-azure/common-web-application-architectures
- Moves: n/a
- What: Move SHA-256/base64url hashing and random code, secret and identifier generation behind one Application interface (or extend `ITokenService`, which already generates refresh tokens) and call it from the handlers.
- Why: The same hash expression is repeated in four handlers, so a change to the hashing scheme must be made in every copy and a missed copy silently stops matching stored hashes.
- Verify: the Census command prints 0.
- Census: 8 via `grep -rnE "SHA256\.HashData|RandomNumberGenerator\.GetBytes" src/DMAuth.Application --include=*.cs | wc -l`
- Evidence:
  ```
  53:			SHA256.HashData(Encoding.UTF8.GetBytes(request.Code)));
  94:			SHA256.HashData(Encoding.UTF8.GetBytes(request.CodeVerifier)));
  74:		var plainCode = Base64Url.EncodeToString(RandomNumberGenerator.GetBytes(32));
  75:		var codeHash = Base64Url.EncodeToString(SHA256.HashData(Encoding.UTF8.GetBytes(plainCode)));
  26:			SHA256.HashData(Encoding.UTF8.GetBytes(request.Token)));
  46:			SHA256.HashData(Encoding.UTF8.GetBytes(request.Token)));
  53:			plainTextSecret = Base64Url.EncodeToString(RandomNumberGenerator.GetBytes(32));
  82:			clientId = "dmauth_" + Base64Url.EncodeToString(RandomNumberGenerator.GetBytes(16));
  ```

### DA-026 Refresh-token and authorization-code lifetimes are hard-coded
- Severity: hygiene
- Layer: backend
- Location: `src/DMAuth.Application/Features/Auth/ExchangeToken/ExchangeTokenHandler.cs:147`; `src/DMAuth.Application/Features/Auth/RotateToken/RotateTokenHandler.cs:88`; `src/DMAuth.Application/Features/Auth/GrantConsent/GrantConsentHandler.cs:85`; `src/DMAuth.Application/Common/Settings/JwtSettings.cs:21`
- Rule: external Options pattern in .NET https://learn.microsoft.com/en-us/dotnet/core/extensions/options
- Moves: n/a
- What: Add refresh-token and authorization-code lifetimes to the typed settings (beside `AccessTokenExpiryMinutes`) and read them in the three handlers.
- Why: Access and ID token lifetimes are configuration, but the 30-day refresh lifetime is a literal in two handlers and the 5-minute code lifetime in a third, so changing either needs a code edit and the two refresh copies can drift.
- Verify: `grep -rn "AddDays(30)\|AddMinutes(5)" src/DMAuth.Application` prints nothing.
- Evidence:
  ```
  147:				DateTimeOffset.UtcNow.AddDays(30),
  88:			DateTimeOffset.UtcNow.AddDays(30),
  85:			DateTimeOffset.UtcNow.AddMinutes(5),
  21:	public int AccessTokenExpiryMinutes { get; set; } = 60;
  ```
