# Audit: test (T5)
Ref: 5b7d9a6

## Scope

`find <R>/tests -type f -not -path '*/bin/*' -not -path '*/obj/*'`

All 56 printed files are CRLF-terminated in the worktree checkout (`file` on each); Evidence lines below quote the text without the trailing CR.

## Files

Suite-wide restatement findings NT-006, NT-007, NT-008, NT-014 and NT-015 (`Moves: discipline`) describe a pattern every test file shares; they are listed only on the files their Locations cite. Every other ID is listed on each file its Locations or its Census population contain.

- `tests/DMAuth.Tests.Integration/Auth/AuthorizeTests.cs` — NT-011, NT-012, NT-018, NT-019
- `tests/DMAuth.Tests.Integration/Auth/ExchangeTokenTests.cs` — NT-009, NT-011, NT-018, NT-019
- `tests/DMAuth.Tests.Integration/Auth/GetAuthorizationDetailsTests.cs` — NT-011, NT-012, NT-017, NT-018, NT-019
- `tests/DMAuth.Tests.Integration/Auth/GrantConsentTests.cs` — NT-011, NT-018, NT-019
- `tests/DMAuth.Tests.Integration/Auth/JwksTests.cs` — NT-009, NT-011, NT-013, NT-016, NT-018
- `tests/DMAuth.Tests.Integration/Auth/OidcDiscoveryTests.cs` — NT-010, NT-011, NT-013, NT-016, NT-019
- `tests/DMAuth.Tests.Integration/Auth/RevokeTokenTests.cs` — NT-003, NT-009, NT-011, NT-012, NT-018, NT-019
- `tests/DMAuth.Tests.Integration/Auth/RotateTokenTests.cs` — NT-009, NT-011, NT-012, NT-018, NT-019
- `tests/DMAuth.Tests.Integration/Auth/UserInfoTests.cs` — NT-009, NT-011, NT-012, NT-018
- `tests/DMAuth.Tests.Integration/Clients/DeleteClientTests.cs` — NT-011, NT-018, NT-019
- `tests/DMAuth.Tests.Integration/Clients/GetClientsTests.cs` — NT-009, NT-011, NT-018, NT-019
- `tests/DMAuth.Tests.Integration/Clients/GetClientTests.cs` — NT-009, NT-011, NT-018, NT-019
- `tests/DMAuth.Tests.Integration/Clients/RegisterClientTests.cs` — NT-011, NT-019
- `tests/DMAuth.Tests.Integration/Clients/UpdateClientTests.cs` — NT-009, NT-011, NT-018, NT-019
- `tests/DMAuth.Tests.Integration/Common/IntegrationTestBase.cs` — NT-007
- `tests/DMAuth.Tests.Integration/Common/IntegrationTestFactory.cs` — NT-002, NT-009, NT-012, NT-013
- `tests/DMAuth.Tests.Integration/DMAuth.Tests.Integration.csproj` — NT-001
- `tests/DMAuth.Tests.Integration/Users/ChangeUserPasswordTests.cs` — NT-002, NT-003, NT-011, NT-019
- `tests/DMAuth.Tests.Integration/Users/GetUserProfileTests.cs` — NT-011, NT-019
- `tests/DMAuth.Tests.Integration/Users/LoginUserTests.cs` — NT-011, NT-019
- `tests/DMAuth.Tests.Integration/Users/LogoutUserTests.cs` — NT-003, NT-011, NT-019
- `tests/DMAuth.Tests.Integration/Users/RegisterUserTests.cs` — NT-011, NT-019
- `tests/DMAuth.Tests.Integration/Users/UpdateUserProfileTests.cs` — NT-003, NT-011, NT-019
- `tests/DMAuth.Tests.Unit/Application/Auth/AuthorizeHandlerTests.cs` — NT-005, NT-008, NT-015
- `tests/DMAuth.Tests.Unit/Application/Auth/ExchangeTokenHandlerTests.cs` — NT-005, NT-009, NT-014
- `tests/DMAuth.Tests.Unit/Application/Auth/GetUserInfoHandlerTests.cs` — NT-009
- `tests/DMAuth.Tests.Unit/Application/Auth/GrantConsentHandlerTests.cs` — NT-005
- `tests/DMAuth.Tests.Unit/Application/Auth/RevokeTokenHandlerTests.cs` — no finding
- `tests/DMAuth.Tests.Unit/Application/Auth/RotateTokenHandlerTests.cs` — NT-005
- `tests/DMAuth.Tests.Unit/Application/Clients/DeleteClientHandlerTests.cs` — NT-014
- `tests/DMAuth.Tests.Unit/Application/Clients/GetClientByIdHandlerTests.cs` — no finding
- `tests/DMAuth.Tests.Unit/Application/Clients/GetClientsByOwnerHandlerTests.cs` — no finding
- `tests/DMAuth.Tests.Unit/Application/Clients/RegisterClientHandlerTests.cs` — no finding
- `tests/DMAuth.Tests.Unit/Application/Clients/UpdateClientRegistrationHandlerTests.cs` — no finding
- `tests/DMAuth.Tests.Unit/Application/Users/ChangeUserPasswordHandlerTests.cs` — no finding
- `tests/DMAuth.Tests.Unit/Application/Users/GetUserProfileHandlerTests.cs` — no finding
- `tests/DMAuth.Tests.Unit/Application/Users/LoginUserHandlerTests.cs` — no finding
- `tests/DMAuth.Tests.Unit/Application/Users/RegisterUserHandlerTests.cs` — NT-006, NT-007
- `tests/DMAuth.Tests.Unit/Application/Users/UpdateUserProfileHandlerTests.cs` — no finding
- `tests/DMAuth.Tests.Unit/Common/UnitTestBase.cs` — NT-007
- `tests/DMAuth.Tests.Unit/DMAuth.Tests.Unit.csproj` — NT-001, NT-017
- `tests/DMAuth.Tests.Unit/Domain/Entities/AuthorizationCodeTests.cs` — NT-009, NT-013, NT-016, NT-020
- `tests/DMAuth.Tests.Unit/Domain/Entities/ClientTests.cs` — NT-009, NT-013, NT-016, NT-020
- `tests/DMAuth.Tests.Unit/Domain/Entities/ConsentTests.cs` — NT-004, NT-009, NT-016, NT-020
- `tests/DMAuth.Tests.Unit/Domain/Entities/RefreshTokenTests.cs` — NT-009, NT-013, NT-016, NT-020
- `tests/DMAuth.Tests.Unit/Domain/Entities/UserTests.cs` — NT-009, NT-013, NT-016, NT-020
- `tests/DMAuth.Tests.Unit/Domain/Policies/CodeChallengePolicyTests.cs` — NT-013
- `tests/DMAuth.Tests.Unit/Domain/Policies/EmailPolicyTests.cs` — NT-013
- `tests/DMAuth.Tests.Unit/Domain/Policies/PasswordPolicyTests.cs` — no finding
- `tests/DMAuth.Tests.Unit/Domain/Policies/RedirectUriPolicyTests.cs` — NT-013
- `tests/DMAuth.Tests.Unit/Domain/Policies/ScopePolicyTests.cs` — no finding
- `tests/DMAuth.Tests.Unit/Domain/ValueObjects/CodeChallengeTests.cs` — NT-013, NT-016
- `tests/DMAuth.Tests.Unit/Domain/ValueObjects/EmailTests.cs` — NT-013, NT-016
- `tests/DMAuth.Tests.Unit/Domain/ValueObjects/HashedPasswordTests.cs` — NT-013, NT-016
- `tests/DMAuth.Tests.Unit/Domain/ValueObjects/RedirectUriTests.cs` — NT-013, NT-016
- `tests/DMAuth.Tests.Unit/Domain/ValueObjects/ScopeTests.cs` — NT-013, NT-016

## Rules checked

- `csharp-style.md` § Naming — NT-009, NT-010
- `csharp-style.md` § File organization — conforms
- `csharp-style.md` § Parameter layout — NT-011, NT-012
- `csharp-style.md` § Declaration layout — NT-011
- `csharp-style.md` § Lambdas, expression bodies and chaining — NT-013, NT-014
- `csharp-style.md` § XML documentation comments — NT-015
- `dotnet-tests.md` § Stack — NT-001
- `dotnet-tests.md` § Naming — NT-016
- `dotnet-tests.md` § Organization — NT-017
- `dotnet-tests.md` § Structure — NT-006, NT-007
- `dotnet-tests.md` § Test class setup and isolation — NT-008
- `dotnet-tests.md` § Assertions — NT-005, NT-014
- `dotnet-tests.md` § Substitutes (NSubstitute) — conforms

## Findings

### NT-001 dotnet test discovers and runs no tests in either test project
- Severity: correctness/security risk
- Layer: test
- Location: `tests/DMAuth.Tests.Unit/DMAuth.Tests.Unit.csproj:15`; `tests/DMAuth.Tests.Unit/DMAuth.Tests.Unit.csproj:5`; `tests/DMAuth.Tests.Integration/DMAuth.Tests.Integration.csproj:17`
- Rule: discipline .sir/disciplines/dotnet-tests.md § Stack; external Testing with 'dotnet test' (VSTest mode and MTP mode) https://learn.microsoft.com/dotnet/core/testing/unit-testing-with-dotnet-test; external dotnet test with VSTest https://learn.microsoft.com/dotnet/core/tools/dotnet-test-vstest
- Moves: code
- What: Make both test projects runnable by `dotnet test`: either opt the repository into the .NET 10 SDK's Microsoft.Testing.Platform mode of `dotnet test` (a repo-root `global.json` with `"test": { "runner": "Microsoft.Testing.Platform" }`, then run `dotnet test --project <csproj>`), or add `Microsoft.NET.Test.Sdk` and `xunit.runner.visualstudio` package references (which set `IsTestProject=true` and supply the VSTest adapter) to `tests/DMAuth.Tests.Unit/DMAuth.Tests.Unit.csproj` and `tests/DMAuth.Tests.Integration/DMAuth.Tests.Integration.csproj`; pick one runner for both projects.
- Why: In the spec-phase probe `dotnet test tests/DMAuth.Tests.Unit/DMAuth.Tests.Unit.csproj` printed only `Determining projects to restore...` and `All projects are up-to-date for restore.`, no test results, and exited 0, so the 307 test methods in `tests/` (193 [Fact] and 18 [Theory] in the unit project) cannot fail that command at 5b7d9a6. Cause, read from the project files: the unit csproj references only `xunit.v3` 3.2.2 (with `OutputType` Exe, FluentAssertions, NSubstitute, coverlet.collector); its restored `obj/DMAuth.Tests.Unit.csproj.nuget.g.props` imports Microsoft.Testing.Platform 1.9.1 and `xunit.v3.core.mtp-v1`, which set `TestProject` (not `IsTestProject`) and leave `TestingPlatformDotnetTestSupport` at its default `False`; no `Microsoft.NET.Test.Sdk`, `xunit.runner.visualstudio`, `IsTestProject` or `TestingPlatformDotnetTestSupport` appears in either csproj or in the repo-root `Directory.Build.props` (which sets only `TargetFramework` net10.0, `Nullable` and `ImplicitUsings`), and there is no `global.json`, so `dotnet test` runs in VSTest mode, whose SDK `VSTest` target (`Microsoft.TestPlatform.targets`, as read in the locally installed SDK 10.0.403) builds and runs a project only when `IsTestProject` is `true` and otherwise logs a low-importance info message and succeeds. `tests/DMAuth.Tests.Integration/DMAuth.Tests.Integration.csproj` shares the cause exactly: the same `xunit.v3` 3.2.2 reference with `OutputType` Exe, the same absence of all four, and its restore output also holds no Test.Sdk or runner.visualstudio package; once it is fixed it will additionally need the NU1605 restore error recorded for `src/DMAuth.Infrastructure` resolved, because it references that project.
- Verify: `dotnet test tests/DMAuth.Tests.Unit/DMAuth.Tests.Unit.csproj` (or `dotnet test --project tests/DMAuth.Tests.Unit/DMAuth.Tests.Unit.csproj` in MTP mode) prints a test summary reporting a non-zero total of 234 tests (193 [Fact] + 41 [InlineData] rows at 5b7d9a6), and the same command for `tests/DMAuth.Tests.Integration/DMAuth.Tests.Integration.csproj` reports 107 tests (93 [Fact] + 14 [InlineData] rows).
- Evidence:
  ```text
  15:    <PackageReference Include="xunit.v3" Version="3.2.2" />
  5:    <OutputType>Exe</OutputType>
  17:    <PackageReference Include="xunit.v3" Version="3.2.2" />
  $ grep -n -E 'Microsoft.NET.Test.Sdk|xunit.runner.visualstudio|IsTestProject|TestingPlatformDotnetTestSupport' tests/DMAuth.Tests.Unit/DMAuth.Tests.Unit.csproj tests/DMAuth.Tests.Integration/DMAuth.Tests.Integration.csproj Directory.Build.props
  (no output, exit 1)
  ```

### NT-002 Authenticated-client helper ignores register and login failures, so 401 assertions can pass vacuously
- Severity: correctness/security risk
- Layer: test
- Location: `tests/DMAuth.Tests.Integration/Common/IntegrationTestFactory.cs:138`; `tests/DMAuth.Tests.Integration/Common/IntegrationTestFactory.cs:143`; `tests/DMAuth.Tests.Integration/Users/ChangeUserPasswordTests.cs:46`
- Rule: external Integration tests in ASP.NET Core (arrange calls checked with EnsureSuccessStatusCode) https://learn.microsoft.com/aspnet/core/test/integration-tests
- Moves: n/a
- What: In `CreateAuthenticatedClientAsync`, check the `/api/users/register` and `/api/users/login` responses (for example `response.EnsureSuccessStatusCode()`, or assert the expected 201/200) so the helper throws when it fails to establish a session.
- Why: The helper discards both responses, so a failed registration or login returns an unauthenticated client; `ChangePassword_WithWrongCurrentPassword_Returns401Unauthorized` then receives 401 because there is no session, not because the password was wrong, and passes while the behaviour it names goes untested; every other test built on the helper fails later with an unrelated assertion instead of at the arrange step.
- Verify: Make the login call in `CreateAuthenticatedClientAsync` use a wrong password and run `dotnet test tests/DMAuth.Tests.Integration/DMAuth.Tests.Integration.csproj`: `ChangePassword_WithWrongCurrentPassword_Returns401Unauthorized` must fail.
- Evidence:
  ```text
  138:		await client.PostAsJsonAsync(
  143:		await client.PostAsJsonAsync(
  46:		response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
  ```

### NT-003 Logout, token revocation and password change are asserted only on status code
- Severity: convention drift
- Layer: test
- Location: `tests/DMAuth.Tests.Integration/Users/LogoutUserTests.cs:25`; `tests/DMAuth.Tests.Integration/Auth/RevokeTokenTests.cs:77`; `tests/DMAuth.Tests.Integration/Users/ChangeUserPasswordTests.cs:29`; `tests/DMAuth.Tests.Integration/Users/UpdateUserProfileTests.cs:50`
- Rule: base test § Behavior over implementation; base test § Coverage
- Moves: code
- What: Add a follow-up observation to each state-changing security test: after logout, `GET /api/users/me` returns 401; after revoking a refresh token, `POST /connect/token` with `grant_type=refresh_token` and that token returns 401; after a password change, login with the old password returns 401 and with the new one 200.
- Why: Each test would still pass if the endpoint returned the status code without signing out, revoking or changing anything, so the security behaviour the endpoints exist for is unverified; the repository already follows the read-back pattern for non-security updates (`UpdateProfile_WithValidDisplayName_PersistsChange`, `Update_WithValidData_PersistsChange`).
- Verify: Remove the sign-out call from the logout endpoint and run `dotnet test tests/DMAuth.Tests.Integration/DMAuth.Tests.Integration.csproj`: a logout test must fail; likewise for revoke and change-password.
- Evidence:
  ```text
  25:		response.StatusCode.Should().Be(HttpStatusCode.NoContent);
  77:		response.StatusCode.Should().Be(HttpStatusCode.OK);
  29:		response.StatusCode.Should().Be(HttpStatusCode.NoContent);
  50:		body!.DisplayName.Should().Be("Persisted Name");
  ```

### NT-004 UpdateGrantedScopes_UpdatesGrantedAt cannot fail
- Severity: hygiene
- Layer: test
- Location: `tests/DMAuth.Tests.Unit/Domain/Entities/ConsentTests.cs:53`
- Rule: base test § Coverage; base test § Behavior over implementation
- Moves: code
- What: Make the assertion discriminate, for example capture `DateTimeOffset.UtcNow` before the call and assert `consent.GrantedAt.Should().BeOnOrAfter(before)` with `before` taken after construction (or inject a time source and assert the exact value).
- Why: `BeOnOrAfter(grantedAtBefore)` holds when `GrantedAt` is left unchanged, so the test passes with the `GrantedAt = DateTimeOffset.UtcNow;` line of `Consent.UpdateGrantedScopes` deleted; its name claims a behaviour it does not check.
- Verify: Delete the `GrantedAt` assignment in `src/DMAuth.Domain/Entities/Consent/ConsentMethods.cs` and run `dotnet test tests/DMAuth.Tests.Unit/DMAuth.Tests.Unit.csproj`: `UpdateGrantedScopes_UpdatesGrantedAt` must fail.
- Evidence:
  ```text
  53:		consent.GrantedAt.Should().BeOnOrAfter(grantedAtBefore);
  ```

### NT-005 Result failures asserted on kind only, without the IsSuccess flag
- Severity: convention drift
- Layer: test
- Location: `tests/DMAuth.Tests.Unit/Application/Auth/AuthorizeHandlerTests.cs:30`; `tests/DMAuth.Tests.Unit/Application/Auth/AuthorizeHandlerTests.cs:41`; `tests/DMAuth.Tests.Unit/Application/Auth/AuthorizeHandlerTests.cs:55`; `tests/DMAuth.Tests.Unit/Application/Auth/AuthorizeHandlerTests.cs:68`; `tests/DMAuth.Tests.Unit/Application/Auth/AuthorizeHandlerTests.cs:81`; `tests/DMAuth.Tests.Unit/Application/Auth/AuthorizeHandlerTests.cs:94`; `tests/DMAuth.Tests.Unit/Application/Auth/AuthorizeHandlerTests.cs:108`; `tests/DMAuth.Tests.Unit/Application/Auth/AuthorizeHandlerTests.cs:121`; `tests/DMAuth.Tests.Unit/Application/Auth/ExchangeTokenHandlerTests.cs:56`; `tests/DMAuth.Tests.Unit/Application/Auth/ExchangeTokenHandlerTests.cs:67`
- Rule: discipline .sir/disciplines/dotnet-tests.md § Assertions
- Moves: code
- What: Precede each failure-kind assertion with `result.IsSuccess.Should().BeFalse();`, as the Clients and Users handler tests already do; the 26 sites are in AuthorizeHandlerTests, ExchangeTokenHandlerTests, GrantConsentHandlerTests and RotateTokenHandlerTests.
- Why: 22 of the 48 `ErrorType` assertions in `tests/` are paired with the flag (each immediately preceded by `IsSuccess.Should().BeFalse()`), the other 26 are not, so the Auth handler tests diverge from both the rule and the rest of the suite.
- Verify: `grep -rh --include=*.cs 'ErrorType.Should()' tests | wc -l` equals `grep -rh --include=*.cs 'IsSuccess.Should().BeFalse()' tests | wc -l`.
- Census: 26 via `echo $(( $(grep -rh --include=*.cs 'ErrorType.Should()' tests | wc -l) - $(grep -rh --include=*.cs 'IsSuccess.Should().BeFalse()' tests | wc -l) ))`
- Evidence:
  ```text
  30:		result.ErrorType.Should().Be(ResultError.Invalid);
  41:		result.ErrorType.Should().Be(ResultError.NotFound);
  55:		result.ErrorType.Should().Be(ResultError.Forbidden);
  68:		result.ErrorType.Should().Be(ResultError.Invalid);
  81:		result.ErrorType.Should().Be(ResultError.Invalid);
  94:		result.ErrorType.Should().Be(ResultError.Invalid);
  108:		result.ErrorType.Should().Be(ResultError.Invalid);
  121:		result.ErrorType.Should().Be(ResultError.Invalid);
  56:		result.ErrorType.Should().Be(ResultError.Invalid);
  67:		result.ErrorType.Should().Be(ResultError.NotFound);
  ```

### NT-006 No test uses Arrange/Act/Assert comment lines
- Severity: convention drift
- Layer: test
- Location: `.sir/disciplines/dotnet-tests.md:21`; `tests/DMAuth.Tests.Unit/Application/Users/RegisterUserHandlerTests.cs:28`; `tests/DMAuth.Tests.Unit/Application/Users/RegisterUserHandlerTests.cs:30`; `tests/DMAuth.Tests.Unit/Application/Users/RegisterUserHandlerTests.cs:32`
- Rule: discipline .sir/disciplines/dotnet-tests.md § Structure
- Moves: discipline
- What: Restate `.sir/disciplines/dotnet-tests.md:21` to the layout the suite uses: Arrange, Act and Assert sections in that order, separated by one blank line, with no `// Arrange` / `// Act` / `// Assert` comment lines.
- Why: None of the 307 test methods carries the comment lines and all separate the three sections with blank lines (as in the cited test), so the code is the evidence and the rule describes a convention the repository never adopted.
- Verify: `grep -rnE --include=*.cs '// (Arrange|Act|Assert)' tests | wc -l` still prints 0 and `.sir/disciplines/dotnet-tests.md` no longer requires the comment lines.
- Census: 307 via `grep -rhE --include=*.cs '^\s*\[(Fact|Theory)\]' tests | wc -l`
- Evidence:
  ```text
  21:- Lay out every test body as Arrange, Act, Assert, each section introduced by a `// Arrange`, `// Act`, or `// Assert` comment line, in that order.
  28:		var command = new RegisterUserCommand("user@example.com", "testuser", "weak", "Test User");
  30:		var result = await _handler.Handle(command, TestCancellationToken);
  32:		result.IsSuccess.Should().BeFalse();
  $ grep -rnE --include=*.cs '// (Arrange|Act|Assert)' tests | wc -l
  0
  ```

### NT-007 Tests pass the xUnit test-context token, not CancellationToken.None
- Severity: convention drift
- Layer: test
- Location: `.sir/disciplines/dotnet-tests.md:23`; `tests/DMAuth.Tests.Unit/Common/UnitTestBase.cs:14`; `tests/DMAuth.Tests.Integration/Common/IntegrationTestBase.cs:14`; `tests/DMAuth.Tests.Unit/Application/Users/RegisterUserHandlerTests.cs:30`
- Rule: discipline .sir/disciplines/dotnet-tests.md § Structure; external xUnit1051 analyzer rule (calls to methods which accept CancellationToken should use TestContext.Current.CancellationToken) https://xunit.net/xunit.analyzers/rules/xUnit1051
- Moves: discipline
- What: Restate `.sir/disciplines/dotnet-tests.md:23` to: pass `TestCancellationToken` (`TestContext.Current.CancellationToken`, exposed by `UnitTestBase` and `IntegrationTestBase`) wherever the unit under test or an HTTP call takes a cancellation token.
- Why: `CancellationToken.None` occurs nowhere in `tests/` while `TestCancellationToken` is passed on 319 lines; the code follows the xUnit v3 analyzer guidance (which lets a run cancel a hung test) and contradicts the rule, so the rule moves.
- Verify: `grep -rn --include=*.cs 'CancellationToken.None' tests | wc -l` prints 0 and the restated rule names `TestCancellationToken`.
- Census: 319 via `grep -rh --include=*.cs 'TestCancellationToken' tests | wc -l`
- Evidence:
  ```text
  23:- Pass `CancellationToken.None` when the unit under test takes a cancellation token.
  14:		TestContext.Current.CancellationToken;
  14:		TestContext.Current.CancellationToken;
  30:		var result = await _handler.Handle(command, TestCancellationToken);
  $ grep -rn --include=*.cs 'CancellationToken.None' tests | wc -l
  0
  ```

### NT-008 Substitutes are created in field initializers, not in the constructor
- Severity: convention drift
- Layer: test
- Location: `.sir/disciplines/dotnet-tests.md:27`; `tests/DMAuth.Tests.Unit/Application/Auth/AuthorizeHandlerTests.cs:15`; `tests/DMAuth.Tests.Unit/Application/Auth/AuthorizeHandlerTests.cs:20`
- Rule: discipline .sir/disciplines/dotnet-tests.md § Test class setup and isolation
- Moves: discipline
- What: Restate `.sir/disciplines/dotnet-tests.md:27` to: create each substitute in a `private readonly` instance field initializer and the system under test in the constructor; never in a static field.
- Why: All 38 `Substitute.For<...>()` calls in `tests/` are `private readonly` field initializers and the constructor builds only the handler from them; field initializers run per test-class instance exactly as constructor assignments do, so the rule's intent (fresh, non-static doubles per test) is met and only its wording disagrees with the code.
- Verify: `grep -rhE --include=*.cs 'Substitute\.For<' tests | wc -l` equals the count of `private readonly` initializers, and the restated rule matches that layout.
- Census: 38 via `grep -rhE --include=*.cs 'private readonly .* = Substitute\.For<' tests | wc -l`
- Evidence:
  ```text
  27:- Create every substitute and the system under test in the test class constructor, assigned to `private readonly` fields; never in a static field.
  15:	private readonly IClientRepository _clientRepository = Substitute.For<IClientRepository>();
  20:		_handler = new AuthorizeHandler(_clientRepository);
  ```

### NT-009 Private static readonly fields are PascalCase instead of _camelCase
- Severity: convention drift
- Layer: test
- Location: `tests/DMAuth.Tests.Integration/Common/IntegrationTestFactory.cs:36`; `tests/DMAuth.Tests.Unit/Application/Auth/GetUserInfoHandlerTests.cs:18`; `tests/DMAuth.Tests.Unit/Application/Auth/ExchangeTokenHandlerTests.cs:349`; `tests/DMAuth.Tests.Unit/Domain/Entities/AuthorizationCodeTests.cs:12`; `tests/DMAuth.Tests.Unit/Domain/Entities/ClientTests.cs:10`; `tests/DMAuth.Tests.Unit/Domain/Entities/ConsentTests.cs:8`; `tests/DMAuth.Tests.Unit/Domain/Entities/RefreshTokenTests.cs:9`; `tests/DMAuth.Tests.Unit/Domain/Entities/UserTests.cs:11`; `tests/DMAuth.Tests.Integration/Clients/GetClientTests.cs:20`; `tests/DMAuth.Tests.Integration/Auth/ExchangeTokenTests.cs:21`
- Rule: discipline .sir/disciplines/csharp-style.md § Naming
- Moves: code
- What: Rename every `private static readonly` field in `tests/` to `_camelCase` (`_codeVerifier`, `_jsonOptions`, `_testUserId`, `_testRsaPrivateKeyPem`, ...); while there, make `GetUserInfoHandlerTests.TestUser` (a mutable `User` entity shared by every test in the class) a per-test instance.
- Why: The rule says private static fields are `_camelCase` exactly like instance fields, and `.editorconfig` enforces `_camelCase` for all private fields at warning severity; all 21 private static readonly fields in `tests/` break it, so the test projects build with naming warnings the production code avoids.
- Verify: `grep -rhE --include=*.cs 'private static readonly [A-Za-z<>?,]+ [A-Z]' tests | wc -l` prints 0.
- Census: 21 via `grep -rhE --include=*.cs 'private static readonly [A-Za-z<>?,]+ [A-Z]' tests | wc -l`
- Evidence:
  ```text
  36:	private static readonly string TestRsaPrivateKeyPem = CreateTestRsaKey();
  18:	private static readonly User TestUser = new(
  349:	private static readonly string CodeVerifier = new('v', 43);
  12:	private static readonly Guid TestClientId = Guid.NewGuid();
  10:	private static readonly Guid TestOwnerId = Guid.NewGuid();
  8:	private static readonly Guid TestUserId = Guid.NewGuid();
  9:	private static readonly Guid TestUserId = Guid.NewGuid();
  11:	private static readonly HashedPassword TestPassword = new("$2a$12$somehashvalue");
  20:	private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
  21:	private static readonly string CodeVerifier = new('v', 43);
  ```

### NT-010 Single-letter lambda parameters in OidcDiscoveryTests
- Severity: convention drift
- Layer: test
- Location: `tests/DMAuth.Tests.Integration/Auth/OidcDiscoveryTests.cs:86`; `tests/DMAuth.Tests.Integration/Auth/OidcDiscoveryTests.cs:90`; `tests/DMAuth.Tests.Integration/Auth/OidcDiscoveryTests.cs:94`; `tests/DMAuth.Tests.Integration/Auth/OidcDiscoveryTests.cs:98`; `tests/DMAuth.Tests.Integration/Auth/OidcDiscoveryTests.cs:102`; `tests/DMAuth.Tests.Integration/Auth/OidcDiscoveryTests.cs:106`; `tests/DMAuth.Tests.Integration/Auth/OidcDiscoveryTests.cs:124`
- Rule: discipline .sir/disciplines/csharp-style.md § Naming
- Moves: code
- What: Rename the `e` lambda parameter to a descriptive name (`element =>`, `scope =>`) at each site.
- Why: The rule forbids single-letter lambda parameters; these seven are the only ones in `tests/` (`grep -rhE --include=*.cs '(^|[^A-Za-z0-9_])[a-z]\s*=>' tests | wc -l` prints 7), so the file diverges from the rest of the suite.
- Verify: `grep -rhE --include=*.cs '(^|[^A-Za-z0-9_])[a-z]\s*=>' tests | wc -l` prints 0.
- Evidence:
  ```text
  86:			.EnumerateArray().Select(e => e.GetString())
  90:			.EnumerateArray().Select(e => e.GetString())
  94:			.EnumerateArray().Select(e => e.GetString())
  98:			.EnumerateArray().Select(e => e.GetString())
  102:			.EnumerateArray().Select(e => e.GetString())
  106:			.EnumerateArray().Select(e => e.GetString())
  124:			.Select(e => e.GetString())
  ```

### NT-011 Integration test classes put the primary constructor parameter inline
- Severity: convention drift
- Layer: test
- Location: `tests/DMAuth.Tests.Integration/Auth/AuthorizeTests.cs:13`; `tests/DMAuth.Tests.Integration/Auth/AuthorizeTests.cs:14`; `tests/DMAuth.Tests.Integration/Auth/ExchangeTokenTests.cs:17`; `tests/DMAuth.Tests.Integration/Auth/GetAuthorizationDetailsTests.cs:16`; `tests/DMAuth.Tests.Integration/Auth/JwksTests.cs:18`; `tests/DMAuth.Tests.Integration/Auth/UserInfoTests.cs:22`; `tests/DMAuth.Tests.Integration/Users/ChangeUserPasswordTests.cs:12`; `tests/DMAuth.Tests.Integration/Users/LogoutUserTests.cs:11`
- Rule: discipline .sir/disciplines/csharp-style.md § Parameter layout; discipline .sir/disciplines/csharp-style.md § Declaration layout
- Moves: code
- What: In every integration test class, put `IntegrationTestFactory factory` on its own line indented one tab, and indent the `: IntegrationTestBase, IClassFixture<IntegrationTestFactory>` line two tabs.
- Why: The rule requires primary constructor parameters one per line at any count, including one, with the inheritance list one level deeper than the parameters; all 20 integration test classes declare `(IntegrationTestFactory factory)` inline with the base list at one tab.
- Verify: `grep -rhE --include=*.cs '^public class [A-Za-z]+\([A-Za-z]+ [a-z][A-Za-z]*\)' tests | wc -l` prints 0.
- Census: 20 via `grep -rhE --include=*.cs '^public class [A-Za-z]+\([A-Za-z]+ [a-z][A-Za-z]*\)' tests | wc -l`
- Evidence:
  ```text
  13:public class AuthorizeTests(IntegrationTestFactory factory)
  14:	: IntegrationTestBase, IClassFixture<IntegrationTestFactory>
  17:public class ExchangeTokenTests(IntegrationTestFactory factory)
  16:public class GetAuthorizationDetailsTests(IntegrationTestFactory factory)
  18:public class JwksTests(IntegrationTestFactory factory)
  22:public class UserInfoTests(IntegrationTestFactory factory)
  12:public class ChangeUserPasswordTests(IntegrationTestFactory factory)
  11:public class LogoutUserTests(IntegrationTestFactory factory)
  ```

### NT-012 Methods with two or more parameters declare them inline
- Severity: convention drift
- Layer: test
- Location: `tests/DMAuth.Tests.Integration/Auth/AuthorizeTests.cs:146`; `tests/DMAuth.Tests.Integration/Auth/AuthorizeTests.cs:161`; `tests/DMAuth.Tests.Integration/Auth/GetAuthorizationDetailsTests.cs:152`; `tests/DMAuth.Tests.Integration/Auth/RevokeTokenTests.cs:131`; `tests/DMAuth.Tests.Integration/Auth/RotateTokenTests.cs:184`; `tests/DMAuth.Tests.Integration/Auth/UserInfoTests.cs:203`; `tests/DMAuth.Tests.Integration/Auth/UserInfoTests.cs:210`; `tests/DMAuth.Tests.Integration/Common/IntegrationTestFactory.cs:96`
- Rule: discipline .sir/disciplines/csharp-style.md § Parameter layout
- Moves: code
- What: Put each parameter of these eight method declarations on its own line, as the neighbouring helpers (`GrantConsentAndGetCodeAsync` in ExchangeTokenTests, `RegisterClientAsync` in UserInfoTests) already do.
- Why: The rule allows inline parameters only when there is exactly one; these declarations take two or three inline while sibling helpers in the same files follow the rule.
- Verify: Each listed declaration spans one line per parameter.
- Evidence:
  ```text
  146:	private async Task GrantConsentAsync(HttpClient httpClient, string oauthClientId, string redirectUri)
  161:	private static string BuildAuthorizeUrl(string clientId, string redirectUri) =>
  152:	private static string BuildAuthorizeUrl(string clientId, string scope) =>
  131:	private async Task<string> GrantConsentAndGetCodeAsync(HttpClient httpClient, string oauthClientId)
  184:	private async Task<string> GrantConsentAndGetCodeAsync(HttpClient httpClient, string oauthClientId)
  203:	private async Task<JsonElement> GetUserInfoJsonAsync(HttpClient client, string accessToken)
  210:	private async Task<HttpResponseMessage> CallUserInfoAsync(HttpClient client, string accessToken)
  96:	public string CreateExpiredAccessToken(Guid userId, string scope = "openid")
  ```

### NT-013 Lambda bodies on the same line as =>
- Severity: convention drift
- Layer: test
- Location: `tests/DMAuth.Tests.Integration/Auth/JwksTests.cs:99`; `tests/DMAuth.Tests.Integration/Common/IntegrationTestFactory.cs:73`; `tests/DMAuth.Tests.Integration/Common/IntegrationTestFactory.cs:89`; `tests/DMAuth.Tests.Unit/Domain/Entities/AuthorizationCodeTests.cs:98`; `tests/DMAuth.Tests.Unit/Domain/Entities/ClientTests.cs:30`; `tests/DMAuth.Tests.Unit/Domain/Entities/ClientTests.cs:138`; `tests/DMAuth.Tests.Unit/Domain/Entities/RefreshTokenTests.cs:76`; `tests/DMAuth.Tests.Unit/Domain/Entities/RefreshTokenTests.cs:109`; `tests/DMAuth.Tests.Unit/Domain/ValueObjects/CodeChallengeTests.cs:24`; `tests/DMAuth.Tests.Unit/Domain/ValueObjects/EmailTests.cs:39`
- Rule: discipline .sir/disciplines/csharp-style.md § Lambdas, expression bodies and chaining
- Moves: code
- What: Move everything after `=>` onto a new line indented one level at each site (`var act = () =>` / `new Email(value);`, `ContainSingle(uri =>` / `uri == ...`), matching the policy tests that already do so.
- Why: The rule puts every lambda body on a new line; 37 lines in `tests/` keep it on the `=>` line while other lambdas in the same files (for example the `ContainSingle(violation =>` assertions in the policy tests) follow the rule.
- Verify: `grep -rhE --include=*.cs '=>[[:blank:]]*[^[:space:]]' tests | wc -l` prints 0.
- Census: 37 via `grep -rhE --include=*.cs '=>[[:blank:]]*[^[:space:]]' tests | wc -l`
- Evidence:
  ```text
  99:			.Select(key => key.GetProperty("kid").GetString())
  73:				options => options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest);
  89:		builder.ConfigureLogging(logging => logging.ClearProviders());
  98:		var act = () => authCode.MarkAsUsed();
  30:		client.RedirectUris.Should().ContainSingle(uri => uri == "https://example.com/callback");
  138:		var act = () => client.Deactivate();
  76:		var act = () => token.Revoke();
  109:		var act = () => token.Rotate("new_token_hash");
  24:		var act = () => new CodeChallenge(value);
  39:		var act = () => new Email("invalidemail.com");
  ```

### NT-014 Chaining rule contradicts the inline assertion and substitute-setup chains
- Severity: convention drift
- Layer: test
- Location: `.sir/disciplines/csharp-style.md:73`; `.sir/disciplines/dotnet-tests.md:35`; `tests/DMAuth.Tests.Unit/Application/Clients/DeleteClientHandlerTests.cs:50`; `tests/DMAuth.Tests.Unit/Application/Clients/DeleteClientHandlerTests.cs:51`; `tests/DMAuth.Tests.Unit/Application/Auth/ExchangeTokenHandlerTests.cs:62`
- Rule: discipline .sir/disciplines/csharp-style.md § Lambdas, expression bodies and chaining; discipline .sir/disciplines/dotnet-tests.md § Assertions
- Moves: discipline
- What: Restate `.sir/disciplines/csharp-style.md:73` to exempt FluentAssertions chains (`subject.Should().Be(...)` stays on one line, as `.sir/disciplines/dotnet-tests.md:35` shows) and to allow the first NSubstitute call on the substitute's line with `.Returns(...)` on the next.
- Why: Read literally, the chaining rule puts `.Should()` and its assertion on separate lines, but the test fragment's own examples keep them inline and 463 lines in `tests/` do so; the two wired fragments disagree and the code follows the test fragment, so csharp-style moves.
- Verify: `.sir/disciplines/csharp-style.md` names the assertion and substitute-setup exception, and `grep -rhE --include=*.cs '[^[:space:]]\.Should\(\)' tests | wc -l` needs no change.
- Census: 463 via `grep -rhE --include=*.cs '[^[:space:]]\.Should\(\)' tests | wc -l`
- Evidence:
  ```text
  73:- Put each chained call on its own line, indented one level from the initial target (LINQ, fluent builders, FluentValidation rules, string operations).
  35:- Chain `.Should()` directly off the subject under assertion: `result.IsSuccess.Should().BeTrue()`, `result.Value.Should().Be(expectedId)`.
  50:		result.IsSuccess.Should().BeFalse();
  51:		result.ErrorType.Should().Be(ResultError.Forbidden);
  62:		_clientRepository.FindByClientIdAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
  ```

### NT-015 Test methods and unit test classes carry no XML documentation
- Severity: convention drift
- Layer: test
- Location: `.sir/disciplines/csharp-style.md:91`; `tests/DMAuth.Tests.Unit/Application/Auth/AuthorizeHandlerTests.cs:12`; `tests/DMAuth.Tests.Unit/Application/Auth/AuthorizeHandlerTests.cs:24`
- Rule: discipline .sir/disciplines/csharp-style.md § XML documentation comments; discipline .sir/disciplines/dotnet-tests.md § Naming
- Moves: discipline
- What: Restate `.sir/disciplines/csharp-style.md:91` to exempt `[Fact]` and `[Theory]` methods (their `MethodName_Scenario_ExpectedResult` name is the documentation) and to require a `<summary>` on each test class, which the integration test classes already carry; the unit test classes then gain a class `<summary>`.
- Why: None of the 307 test methods has an XML comment and the integration classes document only the class, so applying "every public type, member and parameter" to tests contradicts the whole suite; the code is the evidence and the rule's scope moves.
- Verify: `.sir/disciplines/csharp-style.md` states the test-method exemption, and every `public class` in `tests/DMAuth.Tests.Unit` is preceded by `/// </summary>`.
- Census: 307 via `grep -rhE --include=*.cs '^\s*\[(Fact|Theory)\]' tests | wc -l`
- Evidence:
  ```text
  91:- Give every public type, member and parameter an XML documentation comment.
  12:public class AuthorizeHandlerTests
  24:	public async Task Handle_WhenResponseTypeIsNotCode_ReturnsInvalid()
  $ grep -rh --include=*.cs -B1 -E '^\s*\[(Fact|Theory)\]' tests | grep -c '///'
  0
  ```

### NT-016 Test names missing the Scenario segment or the method name
- Severity: convention drift
- Layer: test
- Location: `tests/DMAuth.Tests.Integration/Auth/JwksTests.cs:25`; `tests/DMAuth.Tests.Integration/Auth/JwksTests.cs:62`; `tests/DMAuth.Tests.Integration/Auth/OidcDiscoveryTests.cs:17`; `tests/DMAuth.Tests.Unit/Domain/Entities/AuthorizationCodeTests.cs:41`; `tests/DMAuth.Tests.Unit/Domain/Entities/ClientTests.cs:36`; `tests/DMAuth.Tests.Unit/Domain/Entities/ConsentTests.cs:46`; `tests/DMAuth.Tests.Unit/Domain/Entities/RefreshTokenTests.cs:60`; `tests/DMAuth.Tests.Unit/Domain/Entities/UserTests.cs:84`; `tests/DMAuth.Tests.Unit/Domain/ValueObjects/CodeChallengeTests.cs:58`; `tests/DMAuth.Tests.Unit/Domain/ValueObjects/EmailTests.cs:66`
- Rule: discipline .sir/disciplines/dotnet-tests.md § Naming
- Moves: code
- What: Rename the 29 two-segment tests to three segments (`Deactivate_WhenActive_SetsIsActiveFalse`, `Jwks_WhenRequested_Returns200Ok`) and the 12 `TwoX_...` value-object tests after the member they exercise (`Equals_WithSameValue_ReturnsTrue`).
- Why: The rule requires `MethodName_Scenario_ExpectedResult`; 266 of the 307 test methods follow it, these 41 either drop the scenario or start with a phrase that names no method.
- Verify: `grep -rhE --include=*.cs '^\s*public (async Task|void) ([A-Za-z0-9]+_[A-Za-z0-9]+|Two[A-Za-z]+_[A-Za-z0-9_]+)\(' tests | wc -l` prints 0.
- Census: 41 via `grep -rhE --include=*.cs '^\s*public (async Task|void) ([A-Za-z0-9]+_[A-Za-z0-9]+|Two[A-Za-z]+_[A-Za-z0-9_]+)\(' tests | wc -l`
- Evidence:
  ```text
  25:	public async Task Jwks_Returns200Ok()
  62:	public async Task Jwks_HasPublicCacheControlHeader()
  17:	public async Task Discovery_Returns200Ok()
  41:	public void Constructor_SetsExpiresAt()
  36:	public void Constructor_SetsIsActiveTrue()
  46:	public void UpdateGrantedScopes_UpdatesGrantedAt()
  60:	public void Revoke_SetsRevokedAt()
  84:	public void Deactivate_SetsIsActiveFalse()
  58:	public void TwoChallenges_WithSameValue_AreEqual()
  66:	public void TwoEmails_WithSameValueDifferentCase_AreEqual()
  ```

### NT-017 Validators and GetAuthorizationDetailsHandler have no unit test file
- Severity: convention drift
- Layer: test
- Location: `tests/DMAuth.Tests.Unit/DMAuth.Tests.Unit.csproj`; `tests/DMAuth.Tests.Integration/Auth/GetAuthorizationDetailsTests.cs:16`
- Rule: discipline .sir/disciplines/dotnet-tests.md § Organization; base test § Coverage
- Moves: code
- What: Add unit test files for `GetAuthorizationDetailsHandler`, the 16 `*Validator.cs` classes and `ValidationBehavior` under `tests/DMAuth.Tests.Unit/Application/`, mirroring the existing handler test layout (this record is about the absence of those files in the unit project).
- Why: The rule maps one test file to each source file; 16 of the 17 Application handlers have a `*HandlerTests.cs` file, but `GetAuthorizationDetailsHandler` is exercised only through HTTP in `GetAuthorizationDetailsTests`, and no unit test references any validator or the validation behaviour, so validation failure paths are covered only indirectly by integration 400 assertions.
- Verify: `grep -rlE 'Validator|ValidationBehavior|GetAuthorizationDetailsHandler' tests/DMAuth.Tests.Unit --include=*.cs | wc -l` prints at least 18.
- Census: 16 via `find src/DMAuth.Application -name '*Validator.cs' | wc -l`
- Evidence:
  ```text
  16:public class GetAuthorizationDetailsTests(IntegrationTestFactory factory)
  $ grep -rlE 'Validator|ValidationBehavior|GetAuthorizationDetailsHandler' tests/DMAuth.Tests.Unit --include=*.cs | wc -l
  0
  $ find src/DMAuth.Application -name '*Handler.cs' | wc -l
  17
  $ find tests/DMAuth.Tests.Unit -name '*HandlerTests.cs' | wc -l
  16
  ```

### NT-018 Integration arrange helpers are copied into each test class
- Severity: hygiene
- Layer: test
- Location: `tests/DMAuth.Tests.Integration/Auth/AuthorizeTests.cs:126`; `tests/DMAuth.Tests.Integration/Auth/ExchangeTokenTests.cs:224`; `tests/DMAuth.Tests.Integration/Auth/ExchangeTokenTests.cs:245`; `tests/DMAuth.Tests.Integration/Auth/GetAuthorizationDetailsTests.cs:107`; `tests/DMAuth.Tests.Integration/Auth/GrantConsentTests.cs:138`; `tests/DMAuth.Tests.Integration/Auth/JwksTests.cs:106`; `tests/DMAuth.Tests.Integration/Auth/JwksTests.cs:150`; `tests/DMAuth.Tests.Integration/Auth/RevokeTokenTests.cs:113`; `tests/DMAuth.Tests.Integration/Auth/UserInfoTests.cs:219`; `tests/DMAuth.Tests.Integration/Clients/DeleteClientTests.cs:109`
- Rule: external Unit testing best practices for .NET (prefer helper methods for shared arrange code) https://learn.microsoft.com/dotnet/core/testing/unit-testing-best-practices
- Moves: n/a
- What: Move `RegisterClientAsync`, `CreateClientRegistrationAsync`, `GrantConsentAndGetCodeAsync` (and the exchange/rotate helpers) into one shared helper in `tests/DMAuth.Tests.Integration/Common/` taking the scopes and redirect URI as parameters, and have it check each response status.
- Why: 17 near-identical private helper definitions across 12 integration files differ only in literals, so a change to the client-registration or consent contract has to be made in every copy, and the copies already drift (some register `offline_access`, some read the code without checking the redirect).
- Verify: The census command prints at most 3, one shared definition per helper, all under `tests/DMAuth.Tests.Integration/Common/`.
- Census: 17 via `grep -rhE --include=*.cs 'Task<\(string OAuthClientId, string RedirectUri\)> RegisterClientAsync|Task<string> GrantConsentAndGetCodeAsync|Task<RegisterClientResponse> CreateClientRegistrationAsync' tests | wc -l`
- Evidence:
  ```text
  126:	private async Task<(string OAuthClientId, string RedirectUri)> RegisterClientAsync(HttpClient httpClient)
  224:	private async Task<(string OAuthClientId, string RedirectUri)> RegisterClientAsync(
  245:	private async Task<string> GrantConsentAndGetCodeAsync(
  107:	private async Task<(string OAuthClientId, string RedirectUri)> RegisterClientAsync(
  138:	private async Task<(string OAuthClientId, string RedirectUri)> RegisterClientAsync(HttpClient httpClient)
  106:	private async Task<(string OAuthClientId, string RedirectUri)> RegisterClientAsync(
  150:	private async Task<string> GrantConsentAndGetCodeAsync(
  113:	private async Task<(string OAuthClientId, string RedirectUri)> RegisterClientAsync(HttpClient httpClient)
  219:	private async Task<(string OAuthClientId, string RedirectUri)> RegisterClientAsync(
  109:	private async Task<RegisterClientResponse> CreateClientRegistrationAsync(HttpClient httpClient)
  ```

### NT-019 System using directives placed after other usings
- Severity: convention drift
- Layer: test
- Location: `tests/DMAuth.Tests.Integration/Auth/AuthorizeTests.cs:5`; `tests/DMAuth.Tests.Integration/Auth/ExchangeTokenTests.cs:9`; `tests/DMAuth.Tests.Integration/Auth/RevokeTokenTests.cs:8`; `tests/DMAuth.Tests.Integration/Auth/RotateTokenTests.cs:8`; `tests/DMAuth.Tests.Integration/Clients/DeleteClientTests.cs:5`; `tests/DMAuth.Tests.Integration/Clients/GetClientsTests.cs:6`; `tests/DMAuth.Tests.Integration/Clients/GetClientTests.cs:7`; `tests/DMAuth.Tests.Integration/Users/ChangeUserPasswordTests.cs:4`; `tests/DMAuth.Tests.Integration/Users/LoginUserTests.cs:5`; `tests/DMAuth.Tests.Integration/Users/UpdateUserProfileTests.cs:5`
- Rule: external .NET formatting option dotnet_sort_system_directives_first https://learn.microsoft.com/dotnet/fundamentals/code-analysis/style-rules/dotnet-formatting-options
- Moves: n/a
- What: Sort the using directives in these 18 integration test files with `System.*` first (run `dotnet format style` or the IDE's sort usings).
- Why: The repository `.editorconfig` sets `dotnet_sort_system_directives_first = true` and the unit tests, `JwksTests` and `UserInfoTests` follow it; 18 integration files place `using System.Net;` (and siblings) after `DMAuth`, `FluentAssertions` or `Microsoft` usings.
- Verify: The census command prints 0.
- Census: 18 via `grep -rn --include=*.cs -B1 '^using System' tests | grep -cE -- '-[0-9]+-using (DMAuth|FluentAssertions|Microsoft)'`
- Evidence:
  ```text
  5:using System.Net;
  9:using System.Net;
  8:using System.Net;
  8:using System.Net;
  5:using System.Net;
  6:using System.Net;
  7:using System.Net;
  4:using System.Net;
  5:using System.Net;
  5:using System.Net;
  ```

### NT-020 Domain timestamp tests depend on the wall clock
- Severity: hygiene
- Layer: test
- Location: `tests/DMAuth.Tests.Unit/Domain/Entities/AuthorizationCodeTests.cs:89`; `tests/DMAuth.Tests.Unit/Domain/Entities/ClientTests.cs:108`; `tests/DMAuth.Tests.Unit/Domain/Entities/ClientTests.cs:129`; `tests/DMAuth.Tests.Unit/Domain/Entities/ConsentTests.cs:32`; `tests/DMAuth.Tests.Unit/Domain/Entities/RefreshTokenTests.cs:67`; `tests/DMAuth.Tests.Unit/Domain/Entities/RefreshTokenTests.cs:90`; `tests/DMAuth.Tests.Unit/Domain/Entities/UserTests.cs:58`; `tests/DMAuth.Tests.Unit/Domain/Entities/UserTests.cs:80`
- Rule: base test § Isolation; external TimeProvider and FakeTimeProvider https://learn.microsoft.com/dotnet/standard/datetime/timeprovider-overview
- Moves: code
- What: Give the domain entities a time source (`TimeProvider`) and have these tests pass a `FakeTimeProvider`, asserting the exact timestamp instead of `BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(1))`.
- Why: The rule forbids depending on the wall clock; the domain methods read `DateTimeOffset.UtcNow` directly, so these 8 assertions pass only when the call and the assertion land within one second, which a paused debugger or a loaded CI agent can exceed.
- Verify: `grep -rh --include=*.cs 'BeCloseTo(DateTimeOffset.UtcNow' tests | wc -l` prints 0.
- Evidence:
  ```text
  89:		authCode.UsedAt!.Value.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(1));
  108:		client.UpdatedAt!.Value.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(1));
  129:		client.UpdatedAt!.Value.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(1));
  32:		consent.GrantedAt.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(1));
  67:		token.RevokedAt!.Value.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(1));
  90:		token.RevokedAt!.Value.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(1));
  58:		user.UpdatedAt!.Value.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(1));
  80:		user.UpdatedAt!.Value.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(1));
  ```
