# DM Auth — Coding Conventions

## C# Conventions

### Naming

| Element | Convention | Example |
|---------|-----------|---------|
| Namespace | PascalCase, match folder structure | `DMAuth.Application.Features.Users.Commands.RegisterUser` |
| Class / Record | PascalCase | `RegisterUserCommand` |
| Interface | `I` prefix + PascalCase | `IUserRepository` |
| Method | PascalCase | `FindByEmail()` |
| Property | PascalCase | `CreatedAt` |
| Private field | `_camelCase` | `_userRepository` |
| Parameter | camelCase | `cancellationToken` |
| Constant | PascalCase | `MaxLoginAttempts` |
| Enum member | PascalCase | `ClientType.Confidential` |

### File Organization

- One type per file (class, record, interface, enum)
- File name matches type name: `RegisterUserCommand.cs`
- Folder structure mirrors namespace hierarchy

### Formatting

#### Primary Constructors

Use primary constructors where possible. This applies to classes, records, and any type that supports them.

#### Parameter Formatting

**Primary constructor parameters**: always one per line, regardless of count.
Primary constructors are structural type declarations and benefit from vertical clarity at any arity.

**Regular (body) constructor and method parameters**: one per line when there are 2 or more; inline when there is exactly 1.
Regular constructors and methods are implementation — the 2+ threshold avoids noise for simple single-argument signatures.

```csharp
// Primary constructor — always wrap (even 1 param)
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

// Method call/expression body, 1 argument — inline
public static Result NotFound(string error) =>
	Failure(error, ResultError.NotFound);

// Method call, 2+ arguments — wrap
public static Result Failure(
	string error,
	ResultError errorType) =>
		new(false, error, errorType);
```

#### Inheritance, Interface Implementation, and Generic Constraints

Base classes, interface implementations, and `where` clauses each go on their own line. Each successive element type in the signature gets +1 tab of indentation from the previous element type that is present. The indentation is cumulative and relative — if an element type is absent, it doesn't consume a level.

**Element types in order:**

1. **Declaration** (class/record name) — base indentation (0 tabs)
2. **Parameters** — +1 tab from declaration
3. **Inheritance / interfaces** (`: BaseClass, IFoo`) — +1 tab from previous element
4. **`where` clauses** — +1 tab from previous element

```csharp
// All elements: params(+1), inheritance(+2), where(+3)
public class TestClass(
	IService service,
	IOtherService otherService)
		: BaseClass, ITestClass<T>
			where T : class
{
}

// Params(+1) + inheritance(+2), no where
public record TestCommand(
	string Param1,
	ICollection<int> Param2)
		: IRequest<Result<Guid>>;

// Params(+1) only
public record TestRecord(
	int Param1,
	string Param2);

// No params, where(+1) directly after declaration
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

// Method signatures follow the same parameter-per-line rule
public async Task<Result<Guid>> Handle(
	RegisterUserCommand request,
	CancellationToken cancellationToken)
{
}
```

#### Summary

| Element | Indentation | Rule |
|---------|-------------|------|
| Primary constructors | — | Use where possible |
| Primary constructor parameters | +1 tab from declaration | Always one per line, closing paren after last param |
| Regular constructor parameters | +1 tab from declaration | One per line when 2+; inline when exactly 1 |
| Method parameters | +1 tab from declaration | One per line when 2+; inline when exactly 1 |
| Base class / interfaces | +1 tab from previous element | New line |
| `where` clauses | +1 tab from previous element | New line |

#### Lambda Expressions

Everything after `=>` goes on a new line, indented one level.

```csharp
// Single-line body
var activeUsers = users
	.Where(u =>
		u.IsActive)
	.ToList();

// Multi-line body
var result = items
	.Select(x =>
		new ItemDto
		{
			Id = x.Id,
			Name = x.Name
		})
	.ToList();

// Expression-bodied members
public string FullName =>
	$"{FirstName} {LastName}";

public bool IsExpired =>
	ExpiresAt < DateTimeOffset.UtcNow;
```

#### Method Chaining

Each chained call goes on its own line, indented one level from the initial call.

```csharp
// LINQ chains
var results = dbContext.Users
	.Where(u =>
		u.IsActive)
	.OrderBy(u =>
		u.CreatedAt)
	.Select(u =>
		new UserDto
		{
			Id = u.Id,
			Email = u.Email
		})
	.ToListAsync(cancellationToken);

// Builder/fluent API chains
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

// String/general chains
var normalized = input
	.Trim()
	.ToLowerInvariant()
	.Replace(" ", "-");
```

### XML Documentation Comments

All public types, members, and parameters should have XML documentation comments.

#### Formatting Rules

- Tag descriptions go on a **new line**, indented one level from their tag.
- All descriptions are proper sentences ending with a period.
- Multi-sentence descriptions are allowed; each sentence ends with a period.

```csharp
/// <summary>
///	Represents a user account in the system.
/// </summary>
public class User
	: AuditableEntity
{
}

/// <summary>
///	Base exception for domain rule violations.
/// </summary>
/// <param name="message">
///	Description of the domain rule that was violated.
/// </param>
public class DomainException(
	string message)
		: Exception(message);
```

### Project Structure

Each layer uses a different organizing principle suited to its responsibilities:

- **Domain** — technical categorization (`Entities/`, `ValueObjects/`, `Policies/`, etc.). Domain concepts are cross-cutting and not feature-owned.
- **Application** — feature-based (`Features/{Feature}/`). Each feature folder owns its commands, queries, handlers, and validators.
- **Infrastructure** — concern-based (`Persistence/`, `Security/`, `Logging/`, `Email/`, etc.). Each folder contains the DbContext, client, or service that communicates with that external system or resource.

```
DMAuth.Domain/
  Entities/
    {Entity}/              # Per-entity subdirectory (User/, Client/, etc.)
      {Entity}.cs          # Properties and constructor
      {Entity}Methods.cs   # Domain behavior (partial class)
  ValueObjects/            # Immutable value objects
  Policies/                # Domain policy classes (PasswordPolicy, etc.)
  Events/                  # Domain events
  Exceptions/              # Domain-specific exceptions
  Interfaces/              # Repository interfaces

DMAuth.Application/
  Common/
    Interfaces/      # Application service interfaces (ITokenService, IPasswordHasher)
    Behaviors/       # MediatR pipeline behaviors (validation)
    Results/         # Result<T>, TypedResult<T>, extensions, factories
  Features/
    {Feature}/
      Commands/
        {CommandName}/
          {CommandName}Command.cs
          {CommandName}CommandHandler.cs
          {CommandName}CommandValidator.cs
      Queries/
        {QueryName}/
          {QueryName}Query.cs
          {QueryName}QueryHandler.cs
          {QueryName}Dto.cs
      DTOs/           # Shared DTOs for the feature
  DependencyInjection.cs

DMAuth.Infrastructure/
  Persistence/
    DmAuthDbContext.cs
    Configurations/   # EF Core entity type configurations
    Repositories/     # IRepository implementations
    Migrations/       # EF Core migrations (auto-generated)
  Security/           # Password hashing, token generation
  Logging/            # Logging configuration
  Email/              # Email service
  DependencyInjection.cs

DMAuth.Web/
  Common/                    # Shared Web infrastructure (ApiControllerBase, etc.)
  {Feature}/                 # Feature folder (Users/, Clients/, OAuth/, etc.)
    {Feature}Controller.cs
    Requests/                # Web-layer input records mapped to Application commands/queries
  Middleware/                # Custom middleware
  Program.cs
```

Web-layer input records are defined per feature and mapped to Application commands/queries in the controller action. This prevents HTTP-supplied fields from mixing with context-supplied fields (e.g., `CurrentUserId` from auth).

Application response records are used directly as HTTP response bodies — response types contain no internal fields that need to be hidden from the wire, so a parallel Web-layer response type would add mapping with no benefit.

### CQRS Patterns

#### Commands

Commands represent actions that change state. They return `TypedResult<TResponse>` where `TResponse` is a dedicated response record defined in the same feature folder. Even simple results (e.g., a single ID) are wrapped in a response record for consistency, extensibility, and correct JSON serialization.

```csharp
// Response record — lives in the same feature folder
public record RegisterUserResponse(Guid UserId);

// Command definition — immutable record
public record RegisterUserCommand(
	string Email,
	string Username,
	string Password,
	string DisplayName)
		: IRequest<TypedResult<RegisterUserResponse>>;

// Handler — primary constructor + interface
public class RegisterUserCommandHandler(
	IUserRepository userRepository,
	IPasswordHasher passwordHasher)
		: IRequestHandler<RegisterUserCommand, TypedResult<RegisterUserResponse>>
{
	public async Task<TypedResult<RegisterUserResponse>> Handle(
		RegisterUserCommand request,
		CancellationToken cancellationToken)
	{
		// 2+ params — wrapped
	}
}

// Validator — primary constructor + inheritance
public class RegisterUserCommandValidator()
		: AbstractValidator<RegisterUserCommand>
{
	RuleFor(x =>
		x.Email)
		.NotEmpty()
		.EmailAddress();

	RuleFor(x =>
		x.Username)
		.NotEmpty()
		.MinimumLength(3)
		.MaximumLength(100);

	RuleFor(x =>
		x.Password)
		.NotEmpty()
		.MinimumLength(8);
}
```

#### Queries

Queries represent read operations. They return DTOs directly (no Result wrapper).

```csharp
public record GetUserProfileQuery(
	Guid UserId)
		: IRequest<UserProfileDto>;

public class GetUserProfileQueryHandler(
	IUserRepository userRepository)
		: IRequestHandler<GetUserProfileQuery, UserProfileDto>
{
	public async Task<UserProfileDto> Handle(
		GetUserProfileQuery request,
		CancellationToken cancellationToken)
	{
		// 2+ params — wrapped
	}
}
```

#### Result Type

```csharp
public class Result<T>
{
    public bool IsSuccess { get; }
    public T? Value { get; }
    public string? Error { get; }
    public ResultErrorType? ErrorType { get; }

    public static Result<T> Success(T value) => new(value);
    public static Result<T> Failure(string error, ResultErrorType errorType) => new(error, errorType);
}

public enum ResultErrorType
{
    Validation,
    NotFound,
    Conflict,
    Unauthorized,
    Forbidden
}
```

### Entity Conventions

- Aggregate roots inherit from a base entity or implement an interface with `Id` (Guid)
- All entities have `CreatedAt` (DateTimeOffset) set in constructor
- Mutable entities have `UpdatedAt` (DateTimeOffset) updated on modification
- Domain logic lives in entity methods, not in handlers
- Constructors validate invariants; throw `DomainException` on violation
- Use private setters for properties modified only through domain methods

### Value Object Conventions

- Immutable (record types or classes with readonly properties)
- Validate in constructor; throw `DomainException` on invalid input
- Override equality based on value, not reference

### Async/Await

- All I/O operations are async
- Use `CancellationToken` in all async method signatures
- All async methods must have the `Async` suffix (e.g., `FindByEmailAsync`, `SaveChangesAsync`). **Exception:** MediatR handlers implement `Handle()` as required by the `IRequestHandler` interface contract. The async nature is communicated by the `Task<>` return type; the `Async` suffix is not used.

### Dependency Injection

- Register services via `AddApplication()` and `AddInfrastructure()` extension methods
- Prefer constructor injection
- Use `Scoped` lifetime for repositories and DbContext
- Use `Singleton` for stateless services (password hasher, token service configuration)

### Error Handling

- Domain exceptions for business rule violations (`DomainException`)
- `ExceptionHandlingMiddleware` catches and maps exceptions to HTTP responses
- Never expose stack traces in production responses
- Log all exceptions with correlation IDs

---

## React Conventions

### Project Structure

```
dmauth-web/src/
  api/              # API client modules (one per resource)
  auth/             # Auth context, hooks, protected route
  components/       # Shared/reusable components
  pages/            # Page components (one per route)
  types/            # TypeScript type definitions
  App.tsx           # Root component with router
  main.tsx          # Entry point
```

### Component Conventions

- Functional components only (no class components)
- One component per file; file name matches component name in PascalCase
- Props defined as TypeScript interfaces or inline types
- Use named exports (not default exports)

### State Management

- **Server state**: TanStack Query for all API data (caching, invalidation, loading states)
- **Auth state**: React Context via `AuthProvider`
- **Form state**: react-hook-form with zod schemas
- **Local UI state**: `useState` / `useReducer` as needed

### API Client

- Axios instance with `baseURL` and `withCredentials: true`
- One API module per resource (`authApi.ts`, `userApi.ts`, `clientApi.ts`)
- Type all request/response payloads
- Handle errors centrally via Axios interceptors (401 → redirect to login)

### Styling

- Tailwind CSS utility classes
- No CSS modules or styled-components
- Consistent spacing, color palette via Tailwind config

### Form Validation

- Zod schemas define validation rules
- react-hook-form `zodResolver` connects schemas to forms
- Display field-level error messages below inputs

---

## Testing Conventions

### Framework and Libraries

- **xUnit v3** — test framework
- **FluentAssertions** — assertion library (`result.Should().Be(...)`)
- **NSubstitute** — mocking library
- **EF Core InMemory** — integration test database
- **WebApplicationFactory** — API integration tests

### Naming

Test methods follow the pattern: `MethodName_Scenario_ExpectedResult`

```csharp
[Fact]
public async Task Handle_WithValidCommand_ReturnsSuccessWithUserId()

[Fact]
public async Task Handle_WithDuplicateEmail_ReturnsConflictError()

[Fact]
public void Email_WithInvalidFormat_ThrowsDomainException()
```

### Test Structure (Arrange-Act-Assert)

```csharp
[Fact]
public async Task Handle_WithValidCommand_ReturnsSuccessWithUserId()
{
    // Arrange
    var command = new RegisterUserCommand("test@example.com", "testuser", "P@ssw0rd!", "Test User");
    var handler = new RegisterUserCommandHandler(_mockUserRepo.Object, _mockPasswordHasher.Object);

    // Act
    var result = await handler.Handle(command, CancellationToken.None);

    // Assert
    result.IsSuccess.Should().BeTrue();
    result.Value.Should().NotBeEmpty();
}
```

### Test Organization

```
DMAuth.Tests.Unit/
  Domain/
    UserTests.cs
    ClientTests.cs
    PasswordPolicyTests.cs
    ValueObjects/
      EmailTests.cs
      RedirectUriTests.cs
  Application/
    Users/
      RegisterUserCommandHandlerTests.cs
      LoginCommandHandlerTests.cs
    Clients/
      RegisterClientCommandHandlerTests.cs
    Auth/
      ExchangeTokenCommandHandlerTests.cs

DMAuth.Tests.Integration/
  Fixtures/
    TestDatabaseFixture.cs
    CustomWebApplicationFactory.cs
  AuthFlowTests.cs
  UserRegistrationTests.cs
  ClientManagementTests.cs
  DiscoveryEndpointTests.cs
```

### Integration Test Isolation

- Each test class gets its own InMemory database instance via `IClassFixture<TestDatabaseFixture>`
- Tests within a class run sequentially; classes run in parallel
- No shared mutable state between tests
- Each test seeds its own data
