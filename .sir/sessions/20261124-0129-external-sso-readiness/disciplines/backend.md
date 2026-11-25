<!-- source: sir-sdlc disciplines/backend.md -->

# Backend discipline

Rules are binding. Where one conflicts with a frozen contract, the contract wins.

## Layering and boundaries

- A layer may depend inward only. Nothing in the domain layer imports from transport, persistence, or presentation.
- Types that cross a boundary are declared at that boundary. A persistence type must not appear in a transport signature.
- A module exposes behavior, not its internal state. Public mutable fields are not an interface.

## Error handling

- Distinguish expected failure from defect. Expected failure is a value the caller must handle; a defect terminates the operation.
- Never swallow an error to keep a code path quiet. If it is genuinely ignorable, say why in a comment at the catch site.
- An error crossing a boundary is translated to that boundary's vocabulary. Internal failure detail does not leak outward.
- Include the operation and its inputs in the failure, never a secret or a credential.

## Dependency injection

- Dependencies are declared in the constructor and are immutable thereafter.
- A type resolves nothing at runtime from a global container. Service location hides the dependency graph the constructor would have shown.
- Lifetime is declared where the dependency is registered, not inferred at the call site.

## Logging

- Log at boundaries and at decisions, not at every step. A log line per statement is noise that hides the one line that mattered.
- A log message states what happened and the identifiers needed to find it again. It does not restate the code.
- Never log a secret, a credential, or an entire request body.

## Async discipline

- Async is contagious at the boundary it starts from. Do not block on an asynchronous call to satisfy a synchronous signature; change the signature.
- Every asynchronous operation crossing a process boundary carries a cancellation mechanism and a timeout.
- Concurrent work whose results are all required is awaited together, not sequentially in a loop.

## Configuration

- Configuration is read once, at startup, into a typed shape, and validated there. A missing or malformed value fails startup, not the first request that needs it.
- No secret is committed. A default that works locally must not be a working default anywhere else.
- Behavior differing by environment is a declared configuration value, never a branch on an environment name.

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

<!-- source: .sir/disciplines/dotnet.md -->

# .NET server

## Platform

- Target .NET 10; never lower `<TargetFramework>` without a documented reason.
- The Web layer is a controller-based ASP.NET Core API; do not add Razor or MVC server-rendered views.

## Solution and layers

- `DM-Auth.sln` lives in `src/`; run build and test from the repository root with the explicit path (`dotnet build src/DM-Auth.sln`, `dotnet test src/DM-Auth.sln`).
- Keep `Directory.Build.props` at the repository root.
- Dependencies point only this way: Application → Domain; Infrastructure → Domain; Web → Application, Infrastructure. Domain has zero external dependencies. Web is the composition root.
- Organize each layer by its own principle:
  - Domain by technical category: `Entities/{Entity}/`, `ValueObjects/`, `Policies/`, `Events/`, `Exceptions/`, `Interfaces/`.
  - Application by feature: `Common/Interfaces/` (application service interfaces), `Common/Behaviors/` (pipeline behaviors), `Common/Results/` (result and error types), `Features/{Feature}/`, and a `DependencyInjection` class.
  - Infrastructure by concern, one external system or resource per folder: `Persistence/` (`Configurations/`, `Repositories/`, `Migrations/`), `Security/`, `Logging/`, `Email/`, and a `DependencyInjection` class.
  - Web by feature, mirroring Application: `Common/`, `{Feature}/{Feature}Controller` with `{Feature}/Requests/` for web-layer input models, `Middleware/`, `Program`.
- Inside `Features/{Feature}/`, give each command or query its own folder named for the operation, `Features/{Feature}/{Name}/` (`Clients/GetById/`, `Users/Register/`), holding `{Operation}Command` or `{Operation}Query`, `{Operation}Handler`, `{Operation}Validator`, and `{Operation}Response` when the operation returns its own payload (`GetClientByIdQuery`, `GetClientByIdHandler`, `GetClientByIdValidator`, `GetClientByIdResponse`); never add `Commands/`, `Queries/` or `DTOs/` folders, and never name a type `…CommandHandler`, `…QueryHandler`, `…CommandValidator` or `…Dto`.
- Web-layer input models carry only HTTP-supplied fields (form fields, multipart data) and are mapped to Application commands; the controller supplies application-level values (current user identity, server-assigned IDs).
- Application response types are returned directly as HTTP response bodies; do not declare parallel Web-layer response models.

## CQRS

- CQRS organizes code only; there is one database for reads and writes.
- Dispatch every command and query through the pipeline, which runs validation before the handler.
- Name a command in the imperative (`RegisterUser`, `DeleteClient`, `ChangeUserPassword`).
- A command carries only caller-supplied data; never put server-assigned values (IDs, timestamps) on it.
- A command returns the result wrapper (`Result`, or `TypedResult<T>` when it returns a value, usually `{Operation}Response`) and has a validator.
- Name a query `Get` + what it reads (`GetUserProfile`, `GetClientById`, `GetClientsByOwner`).
- A query returns the result wrapper too, `TypedResult<{Operation}Response>` (`TypedResult<List<{Operation}Response>>` for a collection); a missing record is a `NotFound` failure result, never an exception or a bare `null`.
- A query has a validator, exactly like a command; the pipeline runs it before the handler.
- Declare commands and queries as records with positional properties implementing `IRequest<TResponse>`.
- Declare handlers as `sealed` classes with a primary constructor; every handler returns the result wrapper its request declares.
- Declare validators as `sealed` classes extending `AbstractValidator<TRequest>` for the command or query; write each rule as a method chain whose lambda parameter is named for the request kind: `command =>` in a command validator, `query =>` in a query validator.

```csharp
public sealed class RegisterUserValidator
	: AbstractValidator<RegisterUserCommand>
{
	public RegisterUserValidator()
	{
		RuleFor(command =>
			command.Email)
			.NotEmpty()
			.EmailAddress()
			.MaximumLength(256);
	}
}
```

## Result type

- Communicate expected failures (not found, conflict, validation error, unauthorized, forbidden) with the result type, never with an exception.
- Use the existing `Result` (no value) and `TypedResult<T>` (value on success) in `DMAuth.Application/Common/Results`; never declare another result type.
- A result carries a success flag, the value on success, an error message on failure, and a `ResultError` category.
- The Web layer maps each `ResultError` category to its HTTP status code.

## Entities

- Assign each aggregate root a stable, unique identifier at creation.
- Set every entity's creation timestamp in the constructor and never modify it.
- Give mutable entities a last-modified timestamp, updated on every state change.
- Put domain behavior in entity methods: handlers orchestrate, entities enforce invariants.
- Validate invariants in the constructor and throw a domain exception when one is violated.
- Restrict write access to properties that may change only through domain methods.

## Value objects and policies

- Give each domain concept with non-trivial validation a policy class and a separate value object.
- A policy is a `static` class with a single `Validate` method that returns a `PolicyResult`; it never throws.
- A value object is an immutable record whose state is set once through a private setter or `init`-only property.
- The value object constructor calls the policy and throws a `DomainException` when the input is non-compliant.
- Apply normalization (trimming, case folding) after the policy check passes, never before.
- Value objects compare by value, never by identity.
- Command validators call the policy directly to surface field errors before the value object is constructed.
- Write a policy only for raw user-supplied or external input; never for a value derived from already-validated input (e.g. a hashed password).

## Async and cancellation

- Make every I/O operation (database, network, file) asynchronous; never block a thread waiting on one.
- Never call `.Result`, `.Wait()` or `.GetAwaiter().GetResult()` on a `Task`.
- Never wrap synchronous code in `Task.Run`; call it directly, or use the native async API for I/O. `Task.Run` is allowed only for long CPU-bound work (e.g. cryptographic key generation).
- Controller actions that perform any I/O return `Task<IActionResult>` or `Task<ActionResult<T>>`.
- Never declare an `async void` method; return `Task`.
- Every async method accepts a `CancellationToken` and passes it to every awaited call.
- Suffix async method names with `Async`, except methods whose name a framework interface prescribes (e.g. `Handle`).

## Dependency injection

- Each layer registers its own services through a dedicated `IServiceCollection` extension method.
- Inject dependencies through the constructor.
- Register services holding request-scoped state (repositories, the database context) as scoped.
- Register stateless services (cryptography, token generation, configuration) as singletons.
- Never capture a scoped service in `Task.Run` or a hosted service; create a scope with `IServiceScopeFactory.CreateAsyncScope()` inside the background work and resolve the service from it.

## Error handling and exceptions

- Throw a domain exception for a business rule violation; the global error-handling middleware catches it and maps it to a structured HTTP error response.
- Throw only for unexpected conditions: infrastructure failures, programming errors (e.g. a null argument), domain invariant violations (`DomainException`).
- Never throw for user validation errors, not-found lookups, wrong credentials, or business checks that are expected to fail; return a failed result.
- Never include a stack trace in an HTTP response, in any environment.
- Log every exception with a correlation ID.

## HttpContext

- Never store `HttpContext` in a field; inject `IHttpContextAccessor` and read `HttpContext` at the call site, handling `null`.
- Never read `HttpContext` inside `Task.Run`, `Parallel.ForEach` or parallel LINQ; copy the values you need on the request thread first.
- Never access `HttpContext` from fire-and-forget work or after the response completes.

## Requests and responses

- Read forms with `await Request.ReadFormAsync(cancellationToken)`; never use `Request.Form`.
- Deserialize JSON bodies with `JsonSerializer.DeserializeAsync` over `Request.Body`; never buffer the whole body into a `string` or `byte[]`.
- Never modify response headers after the response has started; set late headers through `Response.OnStarting`.
- Never call `next()` after the middleware has started writing the response body.

## Performance and memory

- On hot paths (token endpoint, auth middleware, `GET /api/users/me`), do not allocate large strings, `byte[]` or `List<T>` per call when a cached or pooled alternative exists.
- Use `StringBuilder` pooling or `string.Create()` for dynamically built strings.
- Return lazy `IEnumerable<T>` rather than materializing a `List<T>` when the caller iterates once.
- Keep middleware fast: no long-running operations, database calls or synchronous I/O in middleware registered early in the pipeline; the authentication and exception-handling middleware stay lean.
- Compute frequently read, rarely changed data (OIDC discovery document, JWKS, allowed scopes, signing credentials, RSA keys) once at startup, cache it (e.g. as a singleton) and refresh it on a schedule; never re-read it from the database or Key Vault per request.
- Hand work that cannot finish within an HTTP timeout (sending email, slow external APIs) to a background service or queue and return `202 Accepted`.

## HTTP clients

- Never construct `HttpClient` directly; resolve clients from `IHttpClientFactory`.
- Register each outbound client as a named client in `Program.cs` with an explicit `Timeout`.

## Configuration and secrets

- The connection string is named `DmAuth`: read it with `GetConnectionString("DmAuth")`; its Key Vault secret is `ConnectionStrings--DmAuth`.
- Set sensitive settings (credentials, private keys) to `null` in `appsettings.json` and in every `appsettings.{Environment}.json`; supply them through user secrets or Key Vault.
- Give non-secret settings in appsettings files a safe default (an empty string or a number).

<!-- source: .sir/disciplines/efcore.md -->

# EF Core

## Queries

- Use the async form of every EF Core database call (`ToListAsync`, `FirstOrDefaultAsync`, `SaveChangesAsync`, ...) and pass the `CancellationToken`; never call the synchronous `ToList()`, `FirstOrDefault()` or `SaveChanges()`.
- Add `AsNoTracking()` to every query whose results will not be modified and saved back.
- Project with `.Select()` inside the query to a record holding only the fields the use case needs whenever it needs only a subset; do not load full entities to read a few columns.
- Load related data in bulk with `.Include()` or a split query; never access a navigation property per item in a loop over a materialized collection.
- Apply `.Where()`, `.OrderBy()` and `.Select()` to the `IQueryable<T>` before materializing; never filter, sort or project a collection already loaded into memory.
- Every query backing a list endpoint takes pagination parameters and applies `.OrderBy()` before `.Skip((page - 1) * pageSize).Take(pageSize)`; never return an unbounded collection.
