# Audit: database (T3)
Ref: 5b7d9a6

## Scope

`find <R>/src/DMAuth.Infrastructure/Persistence <R>/src/DMAuth.Infrastructure/Migrations -type f`

## Files

- `src/DMAuth.Infrastructure/Persistence/Configurations/AuthorizationCodeConfiguration.cs` — DB-001, DB-003, DB-007
- `src/DMAuth.Infrastructure/Persistence/Configurations/ClientConfiguration.cs` — no finding
- `src/DMAuth.Infrastructure/Persistence/Configurations/ConsentConfiguration.cs` — DB-003
- `src/DMAuth.Infrastructure/Persistence/Configurations/RefreshTokenConfiguration.cs` — DB-001, DB-003
- `src/DMAuth.Infrastructure/Persistence/Configurations/UserConfiguration.cs` — DB-007
- `src/DMAuth.Infrastructure/Persistence/DmAuthDbContext.cs` — DB-006
- `src/DMAuth.Infrastructure/Persistence/Repositories/AuthorizationCodeRepository.cs` — no finding
- `src/DMAuth.Infrastructure/Persistence/Repositories/ClientRepository.cs` — DB-004, DB-005
- `src/DMAuth.Infrastructure/Persistence/Repositories/ConsentRepository.cs` — no finding
- `src/DMAuth.Infrastructure/Persistence/Repositories/RefreshTokenRepository.cs` — DB-007, DB-008, DB-009
- `src/DMAuth.Infrastructure/Persistence/Repositories/Repository.cs` — DB-009
- `src/DMAuth.Infrastructure/Persistence/Repositories/UserRepository.cs` — no finding
- `src/DMAuth.Infrastructure/Migrations/20260218234932_InitialSchema.cs` — DB-001, DB-010
- `src/DMAuth.Infrastructure/Migrations/20260218234932_InitialSchema.Designer.cs` — DB-010
- `src/DMAuth.Infrastructure/Migrations/20260302194656_AddNonceAndTokenFamily.cs` — DB-002, DB-010
- `src/DMAuth.Infrastructure/Migrations/20260302194656_AddNonceAndTokenFamily.Designer.cs` — DB-010
- `src/DMAuth.Infrastructure/Migrations/20260302213715_AddScopesToRefreshTokens.cs` — DB-002
- `src/DMAuth.Infrastructure/Migrations/20260302213715_AddScopesToRefreshTokens.Designer.cs` — DB-010
- `src/DMAuth.Infrastructure/Migrations/20260506171955_AddDataProtectionKeys.cs` — DB-010
- `src/DMAuth.Infrastructure/Migrations/20260506171955_AddDataProtectionKeys.Designer.cs` — DB-010
- `src/DMAuth.Infrastructure/Migrations/DmAuthDbContextModelSnapshot.cs` — DB-010

## Rules checked

- `csharp-style.md` § Naming — DB-010
- `csharp-style.md` § File organization — conforms
- `csharp-style.md` § Parameter layout — DB-008
- `csharp-style.md` § Declaration layout — conforms
- `csharp-style.md` § Lambdas, expression bodies and chaining — DB-007, DB-009
- `csharp-style.md` § XML documentation comments — DB-006
- `efcore.md` § Queries — DB-004, DB-005

## Findings

### DB-001 Authorization code and refresh token single-use is not enforced by the database
- Severity: correctness/security risk
- Layer: database
- Location: `src/DMAuth.Infrastructure/Persistence/Configurations/AuthorizationCodeConfiguration.cs`; `src/DMAuth.Infrastructure/Migrations/20260218234932_InitialSchema.cs:73`; `src/DMAuth.Infrastructure/Persistence/Configurations/RefreshTokenConfiguration.cs:45`
- Rule: external EF Core: Handling Concurrency Conflicts https://learn.microsoft.com/en-us/ef/core/saving/concurrency; external RFC 6749 §4.1.2 (code MUST NOT be used more than once) https://datatracker.ietf.org/doc/html/rfc6749#section-4.1.2; external RFC 9700 OAuth 2.0 Security Best Current Practice (refresh token rotation) https://datatracker.ietf.org/doc/html/rfc9700
- Moves: n/a
- What: Make the single-use transitions atomic in the database: configure a concurrency token (a `rowversion` via `IsRowVersion()`, or `IsConcurrencyToken()` on `AuthorizationCode.UsedAt` and `RefreshToken.RevokedAt`) in a new migration, and have the exchange and rotate paths treat `DbUpdateConcurrencyException` as `invalid_grant` (or use a conditional `ExecuteUpdateAsync` with `WHERE UsedAt IS NULL` / `WHERE RevokedAt IS NULL` and check the affected row count).
- Why: No concurrency token exists in the model, so EF Core issues `UPDATE ... WHERE Id = @id` for `MarkAsUsed()` and `Rotate()`; two concurrent token requests that both read the code or refresh token before either saves both pass the `UsedAt.HasValue` / `RevokedAt.HasValue` checks in the Application handlers and both receive tokens, defeating code single-use and refresh-token reuse detection.
- Verify: `grep -rnE "IsRowVersion|IsConcurrencyToken|ExecuteUpdateAsync" src/DMAuth.Infrastructure/Persistence` prints a line for both AuthorizationCode and RefreshToken, and an integration test firing two parallel exchanges of one code gets exactly one success.
- Evidence:
  ```
  src/DMAuth.Infrastructure/Migrations/20260218234932_InitialSchema.cs:73:                    UsedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
  src/DMAuth.Infrastructure/Persistence/Configurations/RefreshTokenConfiguration.cs:45:			token.RevokedAt);
  $ grep -rnE "IsConcurrencyToken|IsRowVersion|ConcurrencyCheck|\[Timestamp\]|DbUpdateConcurrencyException" src; echo "exit=$?"
  exit=1
  ```

### DB-002 Legacy refresh tokens were backfilled with a shared empty FamilyId and empty Scopes
- Severity: hygiene
- Layer: database
- Location: `src/DMAuth.Infrastructure/Migrations/20260302194656_AddNonceAndTokenFamily.cs:19`; `src/DMAuth.Infrastructure/Migrations/20260302213715_AddScopesToRefreshTokens.cs:19`
- Rule: external EF Core: Managing Migrations, customize migration code https://learn.microsoft.com/en-us/ef/core/managing-schemas/migrations/managing#customize-migration-code
- Moves: n/a
- What: Without editing the released migrations, confirm against each environment that no unrevoked `RefreshTokens` row has `FamilyId = '00000000-0000-0000-0000-000000000000'` or `Scopes = ''`; if any exist, add a new data-only migration that sets their `RevokedAt`.
- Why: Every row existing when `FamilyId` was added received the same empty GUID, so reuse detection on any one legacy token (`RevokeByTokenFamilyAsync(token.FamilyId)`) would revoke every other user's legacy token, and legacy rows rotate into tokens with no scopes; with a 30-day lifetime these rows are likely expired, which is why this is hygiene and not a live defect.
- Verify: `SELECT COUNT(*) FROM RefreshTokens WHERE RevokedAt IS NULL AND (FamilyId = '00000000-0000-0000-0000-000000000000' OR Scopes = '')` returns 0 in every environment.
- Evidence:
  ```
  src/DMAuth.Infrastructure/Migrations/20260302194656_AddNonceAndTokenFamily.cs:19:                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));
  src/DMAuth.Infrastructure/Migrations/20260302213715_AddScopesToRefreshTokens.cs:19:                defaultValue: "");
  ```

### DB-003 Scope columns have inconsistent maximum lengths
- Severity: hygiene
- Layer: database
- Location: `src/DMAuth.Infrastructure/Persistence/Configurations/RefreshTokenConfiguration.cs:57`; `src/DMAuth.Infrastructure/Persistence/Configurations/AuthorizationCodeConfiguration.cs:48`; `src/DMAuth.Infrastructure/Persistence/Configurations/ConsentConfiguration.cs:37`
- Rule: external EF Core: Entity Properties, maximum length https://learn.microsoft.com/en-us/ef/core/modeling/entity-properties#maximum-length
- Moves: n/a
- What: Give `RefreshTokens.Scopes` the same maximum length as `AuthorizationCodes.Scopes` and `Consents.GrantedScopes` (1000) in a new migration, or define the length once as a shared constant used by all three configurations.
- Why: The token exchange copies `authCode.Scopes` (up to 1000) into a new `RefreshToken` whose column holds 512, so a long scope string would fail at `SaveChangesAsync` with a truncation error; today `ScopePolicy` allows only four short scopes, so the gap is latent.
- Verify: `grep -n "HasMaxLength" src/DMAuth.Infrastructure/Persistence/Configurations/RefreshTokenConfiguration.cs` shows the Scopes length equal to the AuthorizationCode Scopes length.
- Evidence:
  ```
  src/DMAuth.Infrastructure/Persistence/Configurations/RefreshTokenConfiguration.cs:57:			.HasMaxLength(512)
  src/DMAuth.Infrastructure/Persistence/Configurations/AuthorizationCodeConfiguration.cs:48:			.HasMaxLength(1000)
  src/DMAuth.Infrastructure/Persistence/Configurations/ConsentConfiguration.cs:37:			.HasMaxLength(1000)
  ```

### DB-004 Client list query is unbounded, tracked and loads full entities
- Severity: convention drift
- Layer: database
- Location: `src/DMAuth.Infrastructure/Persistence/Repositories/ClientRepository.cs:25`; `src/DMAuth.Infrastructure/Persistence/Repositories/ClientRepository.cs:31`
- Rule: discipline .sir/disciplines/efcore.md § Queries
- Moves: code
- What: Give `FindByOwnerIdAsync` page and page-size parameters and apply `AsNoTracking()`, `.OrderBy()`, `.Skip((page - 1) * pageSize).Take(pageSize)` and a `.Select()` projection to the fields `GetClientsByOwnerHandler` returns, inside the query.
- Why: Its only caller, `GetClientsByOwnerHandler`, backs a list endpoint, reads the result without saving it back, and projects it in memory, so the query returns an unbounded, change-tracked set of full `Client` rows (including `ClientSecretHash`) against three `efcore.md` § Queries rules.
- Verify: `grep -n "AsNoTracking\|Skip\|Take\|OrderBy" src/DMAuth.Infrastructure/Persistence/Repositories/ClientRepository.cs` shows all four in `FindByOwnerIdAsync`.
- Evidence:
  ```
  src/DMAuth.Infrastructure/Persistence/Repositories/ClientRepository.cs:25:	public async Task<List<Client>> FindByOwnerIdAsync(
  src/DMAuth.Infrastructure/Persistence/Repositories/ClientRepository.cs:31:			.ToListAsync(cancellationToken);
  ```

### DB-005 Read-only client lookup is tracked
- Severity: convention drift
- Layer: database
- Location: `src/DMAuth.Infrastructure/Persistence/Repositories/ClientRepository.cs:15`
- Rule: discipline .sir/disciplines/efcore.md § Queries
- Moves: code
- What: Add `AsNoTracking()` to `FindByClientIdAsync`.
- Why: Its five callers (Authorize, ExchangeToken, GetAuthorizationDetails, GrantConsent, RotateToken handlers) only read the client and never modify it, so the query should be untracked per `efcore.md` § Queries; no query anywhere in the codebase uses `AsNoTracking()`.
- Verify: `grep -n "AsNoTracking" src/DMAuth.Infrastructure/Persistence/Repositories/ClientRepository.cs` prints a line inside `FindByClientIdAsync`.
- Census: 0 via `grep -rn "AsNoTracking" src tests | wc -l`
- Evidence:
  ```
  src/DMAuth.Infrastructure/Persistence/Repositories/ClientRepository.cs:15:	public async Task<Client?> FindByClientIdAsync(
  ```

### DB-006 DbSet XML descriptions are indented one tab instead of two
- Severity: convention drift
- Layer: database
- Location: `src/DMAuth.Infrastructure/Persistence/DmAuthDbContext.cs:29`; `src/DMAuth.Infrastructure/Persistence/DmAuthDbContext.cs:35`; `src/DMAuth.Infrastructure/Persistence/DmAuthDbContext.cs:41`; `src/DMAuth.Infrastructure/Persistence/DmAuthDbContext.cs:47`; `src/DMAuth.Infrastructure/Persistence/DmAuthDbContext.cs:53`
- Rule: discipline .sir/disciplines/csharp-style.md § XML documentation comments
- Moves: code
- What: Indent each of these `<summary>` descriptions two tabs after `///`, as the `DataProtectionKeys` summary in the same file already is.
- Why: The fragment requires two tabs and the rest of the file follows it, so these five lines are inconsistent with the rule and their neighbours.
- Verify: `awk '/^\t\/\/\/\t[^\t]/ {print NR}' src/DMAuth.Infrastructure/Persistence/DmAuthDbContext.cs` prints nothing.
- Evidence:
  ```
  src/DMAuth.Infrastructure/Persistence/DmAuthDbContext.cs:29:	///	User accounts.
  src/DMAuth.Infrastructure/Persistence/DmAuthDbContext.cs:35:	///	OAuth 2.0 client registrations.
  src/DMAuth.Infrastructure/Persistence/DmAuthDbContext.cs:41:	///	Refresh tokens for token rotation and revocation.
  src/DMAuth.Infrastructure/Persistence/DmAuthDbContext.cs:47:	///	Short-lived authorization codes for the authorization code flow.
  src/DMAuth.Infrastructure/Persistence/DmAuthDbContext.cs:53:	///	User consent grants for client applications.
  ```

### DB-007 Lambda bodies are not on a new line after the arrow
- Severity: convention drift
- Layer: database
- Location: `src/DMAuth.Infrastructure/Persistence/Configurations/UserConfiguration.cs:29`; `src/DMAuth.Infrastructure/Persistence/Configurations/UserConfiguration.cs:30`; `src/DMAuth.Infrastructure/Persistence/Configurations/UserConfiguration.cs:42`; `src/DMAuth.Infrastructure/Persistence/Configurations/UserConfiguration.cs:43`; `src/DMAuth.Infrastructure/Persistence/Configurations/UserConfiguration.cs:65`; `src/DMAuth.Infrastructure/Persistence/Configurations/AuthorizationCodeConfiguration.cs:54`; `src/DMAuth.Infrastructure/Persistence/Configurations/AuthorizationCodeConfiguration.cs:55`; `src/DMAuth.Infrastructure/Persistence/Repositories/RefreshTokenRepository.cs:18`
- Rule: discipline .sir/disciplines/csharp-style.md § Lambdas, expression bodies and chaining
- Moves: code
- What: Put `=>` at the end of the parameter line and the lambda body on the next line, indented one level, for the value-converter lambdas, the `UpdatedAt` property lambda (whose arrow starts its line) and the `FindByTokenHashAsync` predicate.
- Why: Every other lambda in the Persistence folder follows the fragment's layout; these eight sites are the exceptions.
- Verify: `grep -rnE "=> *[^ ]|^\s*=>" src/DMAuth.Infrastructure/Persistence | grep -vE "=>\s*$"` prints nothing.
- Evidence:
  ```
  src/DMAuth.Infrastructure/Persistence/Configurations/UserConfiguration.cs:29:				email => email.Value,
  src/DMAuth.Infrastructure/Persistence/Configurations/UserConfiguration.cs:30:				value => new Email(value))
  src/DMAuth.Infrastructure/Persistence/Configurations/UserConfiguration.cs:42:				hash => hash.Value,
  src/DMAuth.Infrastructure/Persistence/Configurations/UserConfiguration.cs:43:				value => new HashedPassword(value))
  src/DMAuth.Infrastructure/Persistence/Configurations/UserConfiguration.cs:65:			=> user.UpdatedAt);
  src/DMAuth.Infrastructure/Persistence/Configurations/AuthorizationCodeConfiguration.cs:54:				challenge => challenge.Value,
  src/DMAuth.Infrastructure/Persistence/Configurations/AuthorizationCodeConfiguration.cs:55:				value => new CodeChallenge(value))
  src/DMAuth.Infrastructure/Persistence/Repositories/RefreshTokenRepository.cs:18:			token => token.TokenHash == tokenHash,
  ```

### DB-008 RefreshTokenRepository primary constructor parameter is inline
- Severity: convention drift
- Layer: database
- Location: `src/DMAuth.Infrastructure/Persistence/Repositories/RefreshTokenRepository.cs:10`
- Rule: discipline .sir/disciplines/csharp-style.md § Parameter layout
- Moves: code
- What: Put `DmAuthDbContext dbContext` on its own line and move the base list to +2 tabs, matching the other four repositories.
- Why: The fragment requires primary constructor parameters one per line at any count, and every sibling repository already does so.
- Verify: `grep -n "RefreshTokenRepository(DmAuthDbContext" src/DMAuth.Infrastructure/Persistence/Repositories/RefreshTokenRepository.cs` prints nothing.
- Evidence:
  ```
  src/DMAuth.Infrastructure/Persistence/Repositories/RefreshTokenRepository.cs:10:public sealed class RefreshTokenRepository(DmAuthDbContext dbContext)
  ```

### DB-009 Chained calls share a line
- Severity: convention drift
- Layer: database
- Location: `src/DMAuth.Infrastructure/Persistence/Repositories/RefreshTokenRepository.cs:17`; `src/DMAuth.Infrastructure/Persistence/Repositories/Repository.cs:35`; `src/DMAuth.Infrastructure/Persistence/Repositories/Repository.cs:39`
- Rule: discipline .sir/disciplines/csharp-style.md § Lambdas, expression bodies and chaining
- Moves: code
- What: Put each chained call on its own line indented one level from the target (`DbContext.RefreshTokens` / `.FirstOrDefaultAsync(`, `DbContext.Set<T>()` / `.Add(entity)` / `.Update(entity)`).
- Why: The fragment requires one chained call per line, and the other repository queries already break `DbContext.<Set>` and the query operator onto separate lines.
- Verify: `grep -nE "\)\.[A-Z]|[a-z]s\.[A-Z][a-zA-Z]*Async\(" src/DMAuth.Infrastructure/Persistence/Repositories/*.cs` prints nothing.
- Evidence:
  ```
  src/DMAuth.Infrastructure/Persistence/Repositories/RefreshTokenRepository.cs:17:		await DbContext.RefreshTokens.FirstOrDefaultAsync(
  src/DMAuth.Infrastructure/Persistence/Repositories/Repository.cs:35:		DbContext.Set<T>().Add(entity);
  src/DMAuth.Infrastructure/Persistence/Repositories/Repository.cs:39:		DbContext.Set<T>().Update(entity);
  ```

### DB-010 csharp-style binds EF-generated migration code it cannot govern
- Severity: convention drift
- Layer: database
- Location: `src/DMAuth.Infrastructure/Migrations/20260218234932_InitialSchema.cs:30`; `src/DMAuth.Infrastructure/Migrations/DmAuthDbContextModelSnapshot.cs:25`; `.sir/disciplines/csharp-style.md:3`; `.sir/disciplines/csharp-style.md:20`
- Rule: discipline .sir/disciplines/csharp-style.md § Naming
- Moves: discipline
- What: Restate the scope line of `csharp-style.md` to exempt EF Core tool-generated files under `src/DMAuth.Infrastructure/Migrations/` (migrations, `*.Designer.cs`, the model snapshot), and optionally mark them `generated_code = true` in `.editorconfig`.
- Why: The generated files use single-letter lambdas (`x =>`, `b =>`), four-space indentation and block namespaces; the code is the evidence here, because released migrations must not be hand-edited and the tool regenerates this shape on every `dotnet ef migrations add`, so the rule rather than the code moves.
- Verify: `grep -n "Migrations" .sir/disciplines/csharp-style.md` prints the exemption line.
- Census: 60 via `grep -rnoE "\b[a-z] =>" src/DMAuth.Infrastructure/Migrations | wc -l`
- Evidence:
  ```
  src/DMAuth.Infrastructure/Migrations/20260218234932_InitialSchema.cs:30:                    table.PrimaryKey("PK_Users", x => x.Id);
  src/DMAuth.Infrastructure/Migrations/DmAuthDbContextModelSnapshot.cs:25:            modelBuilder.Entity("DMAuth.Domain.Entities.AuthorizationCode.AuthorizationCode", b =>
  .sir/disciplines/csharp-style.md:3:Binds every C# file, production and test alike.
  .sir/disciplines/csharp-style.md:20:- Name lambda parameters descriptively (`user =>`, `client =>`, `token =>`); never use a single-letter lambda parameter.
  ```
