<!-- source: sir-sdlc disciplines/database.md -->

# Database discipline

Rules are binding. Where one conflicts with a frozen contract, the contract wins.

## Migrations

- Migrations are forward-only. A released migration is never edited; a mistake is corrected by a new migration.
- Every migration is independently reversible or explicitly declares that it is not, with the reason.
- A migration that changes data is separate from one that changes shape. Mixing them makes the failure impossible to isolate.
- A destructive change is staged: stop writing, migrate readers, then drop. Never all at once.

## Naming

- Naming is consistent across the schema, and it follows what the schema already does rather than what the application layer does.
- A name says what the thing is, not what type it has.
- Keys and constraints are named explicitly, never left to a generator, so a failure message identifies them.

## Transaction scope

- A transaction spans one unit of work and is committed or rolled back at the boundary that opened it.
- Nothing that can block on the outside world happens inside a transaction.
- Isolation level is chosen deliberately where it matters and is otherwise left at the default.

## Indexing

- Every foreign key is indexed unless there is a stated reason not to.
- An index is justified by a query that exists. Speculative indexes cost writes and buy nothing.
- A uniqueness rule is enforced by a constraint, never by an application check alone.

## No logic in schema

- Business rules live in code, not in triggers or stored procedures. Logic in the schema is invisible to the tests and to the reader of the application.
- Referential integrity and uniqueness are schema concerns. Anything conditional on business meaning is not.

<!-- source: .sir/disciplines/csharp-style.md -->
# C# style

Binds every C# file, production and test alike.

## Naming

| Element | Convention | Example |
|---|---|---|
| Namespace | PascalCase, matches the folder structure | `DMAuth.Application.Features.Users.Register` |
| Class / record | PascalCase | `RegisterUserCommand` |
| Interface | `I` prefix + PascalCase | `IUserRepository` |
| Method | PascalCase | `FindByEmail()` |
| Property | PascalCase | `CreatedAt` |
| Private field, static or instance | `_camelCase` | `_userRepository`, `_allowedScopes` |
| Parameter | camelCase | `cancellationToken` |
| Constant | PascalCase | `MaxLoginAttempts` |
| Enum member | PascalCase | `ClientType.Confidential` |

- Private static fields are `_camelCase`, exactly like every other private field.
- Name lambda parameters descriptively (`user =>`, `client =>`, `token =>`); never use a single-letter lambda parameter.

## File organization

- Declare one type (class, record, interface, enum) per file.
- Name the file exactly after the type: `RegisterUserCommand.cs`.
- Mirror the namespace hierarchy in the folder structure.

## Parameter layout

- Use a primary constructor wherever the type supports one.
- Primary constructor parameters: always one per line, at any count, including one.
- Regular constructor, method and expression-bodied member parameters: inline when there is exactly one; one per line when there are two or more.

```csharp
public class DomainException(
	string message)
		: Exception(message);

public Email(string value)
{
}

public async Task<Result> Handle(
	RegisterUserCommand request,
	CancellationToken cancellationToken)
{
}
```

## Declaration layout

- Put parameters, the base class / interface list, and each `where` clause on their own lines.
- Indent each element type +1 tab from the previous element type that is present, in the order declaration (0), parameters, inheritance, `where`. An absent element type consumes no level.

```csharp
public class TestClass<T>(
	IService service,
	IOtherService otherService)
		: BaseClass, ITestClass<T>
			where T : class
{
}

public class OtherClass<T>
	where T : class
{
}
```

## Lambdas, expression bodies and chaining

- Put everything after `=>` on a new line, indented one level; this applies to lambdas and expression-bodied members alike.
- Put each chained call on its own line, indented one level from the initial target (LINQ, fluent builders, FluentValidation rules, string operations).

```csharp
var results = dbContext.Users
	.Where(user =>
		user.IsActive)
	.Select(user =>
		new UserSummary(
			user.Id,
			user.Username))
	.ToListAsync(cancellationToken);

public bool IsExpired =>
	ExpiresAt < DateTimeOffset.UtcNow;
```

## XML documentation comments

- Give every public type, member and parameter an XML documentation comment.
- Put each tag's description on a new line after the opening tag, indented two tabs after `///`.
- Wrap a long sentence onto a new `///` line indented two tabs, rather than running past line length.
- Write every description as complete sentences, each ending with a period.

```csharp
/// <summary>
///		Handles the <see cref="RegisterUserCommand"/> by creating a new user account.
///		Returns a conflict result if the email or username is already in use.
/// </summary>
/// <param name="request">
///		The command containing the registration details.
/// </param>
```

<!-- source: .sir/disciplines/efcore.md -->
# EF Core

## Queries

- Use the async form of every EF Core database call (`ToListAsync`, `FirstOrDefaultAsync`, `SaveChangesAsync`, ...) and pass the `CancellationToken`; never call the synchronous `ToList()`, `FirstOrDefault()` or `SaveChanges()`.
- Add `AsNoTracking()` to every query whose results will not be modified and saved back.
- Project with `.Select()` inside the query to a record holding only the fields the use case needs whenever it needs only a subset; do not load full entities to read a few columns.
- Load related data in bulk with `.Include()` or a split query; never access a navigation property per item in a loop over a materialized collection.
- Apply `.Where()`, `.OrderBy()` and `.Select()` to the `IQueryable<T>` before materializing; never filter, sort or project a collection already loaded into memory.
- Every query backing a list endpoint takes pagination parameters and applies `.OrderBy()` before `.Skip((page - 1) * pageSize).Take(pageSize)`; never return an unbounded collection.
