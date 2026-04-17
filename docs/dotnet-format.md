# .NET Format — DMAuth

> C# code style, naming, and formatting rules for this project.
> For ASP.NET Core best practices, see [dotnet-guidelines.md](dotnet-guidelines.md).
> For domain-level patterns, see [backend-conventions.md](backend-conventions.md).

---

## Table of Contents

1. [Naming](#1-naming)
2. [File Organization](#2-file-organization)
3. [Parameter Formatting](#3-parameter-formatting)
4. [Inheritance, Interfaces, and Generic Constraints](#4-inheritance-interfaces-and-generic-constraints)
5. [Lambda Expressions](#5-lambda-expressions)
6. [Method Chaining](#6-method-chaining)
7. [XML Documentation Comments](#7-xml-documentation-comments)
8. [CQRS in C#](#8-cqrs-in-c)
9. [Domain Building Blocks in C#](#9-domain-building-blocks-in-c)
10. [Testing in C#](#10-testing-in-c)

---

## 1. Naming

| Element | Convention | Example |
|---------|-----------|---------|
| Namespace | PascalCase, matches folder structure | `DMAuth.Application.Features.Users.Commands.RegisterUser` |
| Class / Record | PascalCase | `RegisterUserCommand` |
| Interface | `I` prefix + PascalCase | `IUserRepository` |
| Method | PascalCase | `FindByEmail()` |
| Property | PascalCase | `CreatedAt` |
| Private field | `_camelCase` | `_userRepository` |
| Parameter | camelCase | `cancellationToken` |
| Constant | PascalCase | `MaxLoginAttempts` |
| Enum member | PascalCase | `ClientType.Confidential` |

### Lambda Variable Names

Use descriptive names in lambda expressions — never single-letter variables.

```csharp
// Incorrect
users.Where(u => u.IsActive)

// Correct
users.Where(user => user.IsActive)
```

---

## 2. File Organization

- One type per file (class, record, interface, enum)
- File name matches the type name exactly: `RegisterUserCommand.cs`
- Folder structure mirrors the namespace hierarchy

---

## 3. Parameter Formatting

### Primary Constructors

Use primary constructors where supported. They are preferred for classes, records, and any type where they apply.

**Primary constructor parameters**: always one per line, regardless of count. Primary constructors are structural type declarations and benefit from vertical clarity at any arity.

**Regular (body) constructor and method parameters**: one per line when there are 2 or more; inline when exactly 1. Regular constructors and methods are implementation — the 2+ threshold avoids noise for simple single-argument signatures.

```csharp
// Primary constructor — always wrap, even with 1 param
public class DomainException(
	string message)
		: Exception(message);

// Primary constructor — always wrap
public sealed class RegisterUserCommandHandler(
	IUserRepository userRepository,
	IPasswordHasher passwordHasher)
		: IRequestHandler<RegisterUserCommand, Result<Guid>>
{
}

// Regular constructor, 1 param — inline
public Email(string value)
{
	Value = value.ToLowerInvariant();
}

// Regular constructor, 2+ params — wrap
public User(
	Email email,
	string username,
	HashedPassword password,
	string displayName)
{
	Email = email;
	Username = username;
}

// Method, 1 parameter — inline
public async Task InvokeAsync(HttpContext context)
{
}

// Method, 2+ parameters — wrap
public async Task<Result<Guid>> Handle(
	RegisterUserCommand request,
	CancellationToken cancellationToken)
{
}

// Expression-body, 1 argument — inline
public static Result NotFound(string error) =>
	Failure(error, ResultError.NotFound);

// Expression-body, 2+ arguments — wrap
public static Result Failure(
	string error,
	ResultError errorType) =>
		new(false, error, errorType);
```

---

## 4. Inheritance, Interfaces, and Generic Constraints

Base classes, interface implementations, and `where` clauses each go on their own line. Each successive element type gets **+1 tab** of indentation from the previous element type that is present. Indentation is cumulative and relative — absent element types do not consume a level.

**Element types in declaration order:**

1. **Declaration** (type name) — base indentation (0 tabs)
2. **Parameters** — +1 tab from declaration
3. **Inheritance / interfaces** (`: BaseClass, IFoo`) — +1 tab from previous element
4. **`where` clauses** — +1 tab from previous element

```csharp
// All elements present: params(+1), inheritance(+2), where(+3)
public class TestClass(
	IService service,
	IOtherService otherService)
		: BaseClass, ITestClass<T>
			where T : class
{
}

// Params(+1) + inheritance(+2), no where clause
public record TestCommand(
	string Param1,
	ICollection<int> Param2)
		: IRequest<Result<Guid>>;

// Params(+1) only
public record TestRecord(
	int Param1,
	string Param2);

// No params — where(+1) directly after declaration
public class OtherClass<T>
	where T : class
{
}

// Params(+1), no inheritance, where(+2)
public class AnotherClass<T>(
	IService service)
		where T : class
{
}
```

**Summary:**

| Element | Indentation | Rule |
|---------|-------------|------|
| Primary constructor parameters | +1 tab from declaration | Always one per line |
| Regular constructor / method parameters | +1 tab from declaration | One per line when 2+; inline when exactly 1 |
| Base class / interfaces | +1 tab from previous element | New line |
| `where` clauses | +1 tab from previous element | New line |

---

## 5. Lambda Expressions

Everything after `=>` goes on a new line, indented one level.

```csharp
// Single-line body
var activeUsers = users
	.Where(user =>
		user.IsActive)
	.ToList();

// Multi-line body
var result = items
	.Select(item =>
		new ItemDto
		{
			Id = item.Id,
			Name = item.Name
		})
	.ToList();

// Expression-bodied members
public string FullName =>
	$"{FirstName} {LastName}";

public bool IsExpired =>
	ExpiresAt < DateTimeOffset.UtcNow;
```

---

## 6. Method Chaining

Each chained call goes on its own line, indented one level from the initial target.

```csharp
// LINQ chains
var results = dbContext.Users
	.Where(user =>
		user.IsActive)
	.OrderBy(user =>
		user.CreatedAt)
	.Select(user =>
		new UserDto
		{
			Id = user.Id,
			Email = user.Email
		})
	.ToListAsync(cancellationToken);

// Builder / fluent API chains
services
	.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
	.AddCookie(options =>
	{
		options.HttpOnly = true;
		options.SecurePolicy = CookieSecurePolicy.Always;
	});

// FluentValidation chains
RuleFor(x =>
	x.Email)
	.NotEmpty()
	.EmailAddress()
	.MaximumLength(256);

// String / general chains
var normalized = input
	.Trim()
	.ToLowerInvariant()
	.Replace(" ", "-");
```

---

## 7. XML Documentation Comments

All public types, members, and parameters must have XML documentation comments.

**Rules:**
- Tag descriptions go on a **new line**, indented with **two tabs** from the `///` marker
- All descriptions are complete sentences ending with a period
- Multi-sentence descriptions are allowed; each sentence ends with a period

```csharp
/// <summary>
///		Represents a user account in the system.
/// </summary>
public class User
	: AuditableEntity
{
}

/// <summary>
///		Base exception for domain rule violations.
/// </summary>
/// <param name="message">
///		Description of the domain rule that was violated.
/// </param>
public class DomainException(
	string message)
		: Exception(message);

/// <summary>
///		Handles the <see cref="RegisterUserCommand"/> by creating a new user account.
///		Returns a conflict result if the email or username is already in use.
/// </summary>
/// <param name="request">
///		The command containing the registration details.
/// </param>
/// <param name="cancellationToken">
///		Token used to cancel the operation.
/// </param>
public async Task<TypedResult<RegisterUserResponse>> Handle(
	RegisterUserCommand request,
	CancellationToken cancellationToken)
{
}
```

---

## 8. CQRS in C#

### Command

Commands are records with positional properties. The return type is always a `Result<T>` wrapper.

```csharp
public record RegisterUserCommand(
	string Email,
	string Username,
	string Password,
	string DisplayName)
		: IRequest<Result<Guid>>;
```

### Query

Queries are records with positional properties. The return type is a DTO directly — no result wrapper.

```csharp
public record GetUserProfileQuery(
	Guid UserId)
		: IRequest<UserProfileDto>;
```

### Handler

Handlers are sealed classes with a primary constructor. Command handlers return `Result<T>`; query handlers return the DTO directly.

```csharp
// Command handler
public sealed class RegisterUserCommandHandler(
	IUserRepository userRepository,
	IPasswordHasher passwordHasher)
		: IRequestHandler<RegisterUserCommand, Result<Guid>>
{
	/// <summary>
	///		Handles the <see cref="RegisterUserCommand"/> by creating a new user account.
	/// </summary>
	/// <param name="request">
	///		The command containing the registration details.
	/// </param>
	/// <param name="cancellationToken">
	///		Token used to cancel the operation.
	/// </param>
	public async Task<Result<Guid>> Handle(
		RegisterUserCommand request,
		CancellationToken cancellationToken)
	{
		// ...
	}
}

// Query handler
public sealed class GetUserProfileQueryHandler(
	IUserRepository userRepository)
		: IRequestHandler<GetUserProfileQuery, UserProfileDto>
{
	/// <summary>
	///		Handles the <see cref="GetUserProfileQuery"/> by returning the user's profile.
	/// </summary>
	/// <param name="request">
	///		The query identifying the user to retrieve.
	/// </param>
	/// <param name="cancellationToken">
	///		Token used to cancel the operation.
	/// </param>
	public async Task<UserProfileDto> Handle(
		GetUserProfileQuery request,
		CancellationToken cancellationToken)
	{
		// ...
	}
}
```

### Validator

Validators are sealed classes that extend `AbstractValidator<TCommand>`. Rules use method chaining. Lambda variable names are descriptive (matches the field name, lowercased).

```csharp
public sealed class RegisterUserCommandValidator
	: AbstractValidator<RegisterUserCommand>
{
	/// <summary>
	///		Initializes a new instance of <see cref="RegisterUserCommandValidator"/>
	///		with rules for all command fields.
	/// </summary>
	public RegisterUserCommandValidator()
	{
		RuleFor(command =>
			command.Email)
			.NotEmpty()
			.EmailAddress()
			.MaximumLength(256);

		RuleFor(command =>
			command.Password)
			.NotEmpty()
			.MinimumLength(8);
	}
}
```

### Result Type

```csharp
public record Result<T>
{
	public bool IsSuccess { get; init; }
	public T? Value { get; init; }
	public string? Error { get; init; }
	public ResultError? ErrorType { get; init; }

	public static Result<T> Success(T value) =>
		new() { IsSuccess = true, Value = value };

	public static Result<T> Failure(
		string error,
		ResultError errorType) =>
			new() { IsSuccess = false, Error = error, ErrorType = errorType };
}

public enum ResultError
{
	Validation,
	NotFound,
	Conflict,
	Unauthorized,
	Forbidden
}
```

---

## 9. Domain Building Blocks in C#

### Value Object

Value objects are records with a private setter or `init`-only property. The constructor calls the policy and throws on failure. Normalization is applied after the policy check.

```csharp
/// <summary>
///		Represents a validated, normalized email address.
/// </summary>
public record Email
{
	/// <summary>
	///		The normalized email address value.
	/// </summary>
	public string Value { get; }

	/// <summary>
	///		Initializes a new <see cref="Email"/> by validating and normalizing the input.
	/// </summary>
	/// <param name="value">
	///		The raw email address string to validate.
	/// </param>
	/// <exception cref="DomainException">
	///		Thrown when the provided value fails policy validation.
	/// </exception>
	public Email(string value)
	{
		var result = EmailPolicy.Validate(value);
		if (!result.IsValid)
			throw new DomainException(result.Error);
		Value = value.Trim().ToLowerInvariant();
	}
}
```

### Policy

Policies are static classes with a single `Validate` method that returns a `PolicyResult`. They never throw.

```csharp
/// <summary>
///		Defines and enforces the validation rules for an email address.
/// </summary>
public static class EmailPolicy
{
	/// <summary>
	///		Validates the provided email address string against all rules.
	/// </summary>
	/// <param name="value">
	///		The raw email address string to validate.
	/// </param>
	public static PolicyResult Validate(string value)
	{
		if (string.IsNullOrWhiteSpace(value))
			return PolicyResult.Invalid("Email is required.");

		if (!value.Contains('@'))
			return PolicyResult.Invalid("Email must be a valid address.");

		if (value.Length > 256)
			return PolicyResult.Invalid("Email must not exceed 256 characters.");

		return PolicyResult.Valid();
	}
}
```

---

## 10. Testing in C#

### Test Class Structure

Each test class creates its own mocks and system under test in the constructor. No shared state between test classes.

```csharp
public class RegisterUserCommandHandlerTests
{
	private readonly IUserRepository _userRepository;
	private readonly IPasswordHasher _passwordHasher;
	private readonly RegisterUserCommandHandler _handler;

	public RegisterUserCommandHandlerTests()
	{
		_userRepository = Substitute.For<IUserRepository>();
		_passwordHasher = Substitute.For<IPasswordHasher>();
		_handler = new RegisterUserCommandHandler(_userRepository, _passwordHasher);
	}

	[Fact]
	public async Task Handle_WithValidCommand_ReturnsSuccessWithUserId()
	{
		// Arrange
		var command = new RegisterUserCommand(
			"test@example.com",
			"testuser",
			"Password123!",
			"Test User");
		_userRepository
			.FindByEmailAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
			.Returns((User?)null);

		// Act
		var result = await _handler.Handle(command, CancellationToken.None);

		// Assert
		result.IsSuccess.Should().BeTrue();
		result.Value.Should().NotBeEmpty();
	}

	[Fact]
	public async Task Handle_WithDuplicateEmail_ReturnsConflictError()
	{
		// Arrange
		var command = new RegisterUserCommand(
			"existing@example.com",
			"newuser",
			"Password123!",
			"New User");
		_userRepository
			.FindByEmailAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
			.Returns(new User(...));

		// Act
		var result = await _handler.Handle(command, CancellationToken.None);

		// Assert
		result.IsSuccess.Should().BeFalse();
		result.ErrorType.Should().Be(ResultError.Conflict);
	}
}
```

### Assertions

Use FluentAssertions for all assertions. Chain `.Should()` directly off the subject.

```csharp
// Success / failure
result.IsSuccess.Should().BeTrue();
result.IsSuccess.Should().BeFalse();

// Value assertions
result.Value.Should().NotBeEmpty();
result.Value.Should().Be(expectedId);

// Error assertions
result.ErrorType.Should().Be(ResultError.Conflict);
result.Error.Should().Contain("already in use");

// Collection assertions
users.Should().HaveCount(3);
users.Should().ContainSingle(user => user.Email == "test@example.com");
```

### Mock Setup (NSubstitute)

```csharp
// Return a value
_userRepository
	.FindByEmailAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
	.Returns(existingUser);

// Return null
_userRepository
	.FindByEmailAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
	.Returns((User?)null);

// Verify a call was made
await _userRepository
	.Received(1)
	.AddAsync(Arg.Any<User>(), Arg.Any<CancellationToken>());

// Verify no calls
_userRepository
	.DidNotReceive()
	.AddAsync(Arg.Any<User>(), Arg.Any<CancellationToken>());
```
