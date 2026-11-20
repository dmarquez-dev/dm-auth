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
