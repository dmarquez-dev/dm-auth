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
