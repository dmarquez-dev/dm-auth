# Backend Conventions — DMAuth

> Domain-level, language-agnostic patterns for the DMAuth backend.
> For ASP.NET Core best practices, see [dotnet-guidelines.md](dotnet-guidelines.md).
> For C# naming, formatting, and code style rules, see [dotnet-format.md](dotnet-format.md).

---

## Table of Contents

1. [Project Structure](#1-project-structure)
2. [CQRS Patterns](#2-cqrs-patterns)
3. [Entity Conventions](#3-entity-conventions)
4. [Value Object Conventions](#4-value-object-conventions)
5. [Policy Conventions](#5-policy-conventions)
6. [Async / Cancellation](#6-async--cancellation)
7. [Dependency Injection](#7-dependency-injection)
8. [Error Handling](#8-error-handling)
9. [Testing Conventions](#9-testing-conventions)

---

## 1. Project Structure

Each layer uses a different organizing principle suited to its responsibilities:

- **Domain** — organized by technical category (entities, value objects, policies, events, exceptions, interfaces). Domain concepts are cross-cutting and not feature-owned.
- **Application** — organized by feature. Each feature folder owns its commands, queries, handlers, and validators.
- **Infrastructure** — organized by concern (persistence, security, logging, email). Each folder encapsulates one external system or resource.
- **Web** — organized by feature, mirroring Application. Contains controllers, request models, and middleware.

```
DMAuth.Domain/
  Entities/
    {Entity}/              # Per-entity subdirectory
  ValueObjects/
  Policies/
  Events/
  Exceptions/
  Interfaces/

DMAuth.Application/
  Common/
    Interfaces/            # Application service interfaces
    Behaviors/             # Pipeline behaviors (validation)
    Results/               # Result and error types
  Features/
    {Feature}/
      Commands/
        {CommandName}/
          {CommandName}Command
          {CommandName}CommandHandler
          {CommandName}CommandValidator
      Queries/
        {QueryName}/
          {QueryName}Query
          {QueryName}QueryHandler
          {QueryName}Dto
      DTOs/
  DependencyInjection

DMAuth.Infrastructure/
  Persistence/
    Configurations/
    Repositories/
    Migrations/
  Security/
  Logging/
  Email/
  DependencyInjection

DMAuth.Web/
  Common/
  {Feature}/
    {Feature}Controller
    Requests/              # Web-layer input models
  Middleware/
  Program
```

### Layer Dependencies

```
Application → Domain
Infrastructure → Domain
Web → Application, Infrastructure
```

The Domain layer has zero external dependencies. Web is the composition root.

### Web Layer Mapping

Web-layer input models capture HTTP-supplied fields and are mapped to Application commands. This keeps HTTP concerns (form fields, multipart data) separate from application-level concerns (current user identity, server-assigned IDs). Application response types flow directly to HTTP response bodies — no parallel Web-layer response models.

---

## 2. CQRS Patterns

This project uses CQRS for structural organization, not for separate read/write databases. All commands and queries are dispatched through a pipeline that runs validation before the handler executes.

### Commands

Commands represent operations that change state. A command:
- Is named in the imperative: `RegisterUser`, `DeleteClient`, `ChangePassword`
- Carries only the data supplied by the caller — never server-assigned values (IDs, timestamps)
- Returns a typed result wrapper indicating success or failure with an error type
- Has a corresponding validator that runs before the handler

For C# implementation shapes, see [dotnet-format.md § 8](dotnet-format.md#8-cqrs-in-c).

### Queries

Queries represent read operations that do not change state. A query:
- Is named as a noun phrase: `GetUserProfile`, `ListClients`
- Returns a DTO directly — no result wrapper, because a missing record is an error surfaced via HTTP, not a domain failure
- Has no validator (input is validated at the HTTP layer via model binding)

### Result Type

Expected failures (not found, conflict, validation error, unauthorized) are communicated via a result type, not via exceptions. The result type carries:
- A success flag
- The return value (on success)
- An error message (on failure)
- An error category (Validation, NotFound, Conflict, Unauthorized, Forbidden)

The Web layer maps each error category to the appropriate HTTP status code.

---

## 3. Entity Conventions

- Each aggregate root has a stable, unique identifier assigned at creation
- All entities record a creation timestamp set in the constructor and never modified
- Mutable entities record a last-modified timestamp updated whenever state changes
- Domain behavior lives in entity methods — handlers orchestrate, entities enforce invariants
- Constructors validate invariants and raise a domain exception if violated
- Properties that may only change through domain methods have restricted write access

---

## 4. Value Object Conventions

- Value objects are immutable — once constructed, their state never changes
- Construction delegates to the corresponding policy class; if validation fails, a domain exception is raised
- Any normalization (case folding, trimming) is applied after the policy check passes, not before
- Equality is based on value, not identity — two value objects with the same value are equal

For C# implementation shapes, see [dotnet-format.md § 9](dotnet-format.md#9-domain-building-blocks-in-c).

---

## 5. Policy Conventions

Each domain concept with non-trivial validation has two separate classes: a **policy** and a **value object**.

| Class | Role | On failure |
|-------|------|------------|
| Policy | Owns the validation rules; returns a structured result | Returns a result — never throws |
| Value object | Owns the state; delegates validation to the policy | Throws a domain exception |

This separation allows the same rules to serve two distinct contexts:
- **Construction** — the value object constructor calls the policy and throws if non-compliant
- **Pre-validation** — command validators call the policy directly to surface user-facing field errors before attempting to construct the value object

**Policies validate input, not derived output.** A policy exists for raw user-supplied or external input. It does not exist for values that are derived from already-validated input (e.g., a hashed password is always structurally valid by construction and needs no policy).

For C# implementation shapes, see [dotnet-format.md § 9](dotnet-format.md#9-domain-building-blocks-in-c).

---

## 6. Async / Cancellation

- All I/O operations are asynchronous — never block a thread waiting for a database call, network call, or file operation
- Every async operation accepts a cancellation signal so in-flight work can be abandoned when a caller disconnects
- Async methods are named with an `Async` suffix to distinguish them from synchronous counterparts. Exception: handler methods defined by a framework interface contract use the interface's prescribed name regardless of whether they are async

---

## 7. Dependency Injection

- Each layer registers its own services via a dedicated extension method on the service collection
- Prefer constructor injection — dependencies are declared as constructor parameters
- Services that hold request-scoped state (repositories, database context) use a scoped lifetime
- Stateless services (cryptography, token generation, configuration) use a singleton lifetime

---

## 8. Error Handling

- Business rule violations raise a domain exception, which is caught by a global error-handling middleware and mapped to a structured HTTP error response
- Expected failure paths (not found, conflict, wrong credentials) use the result type — not exceptions — to communicate failure
- Stack traces are never included in HTTP responses in any environment
- All exceptions are logged with a correlation ID for traceability

---

## 9. Testing Conventions

### Tools

The backend test suite uses:
- A unit test framework for isolated handler and domain tests
- An assertion library for expressive, readable assertions
- A mocking library for substituting dependencies
- An in-memory database provider for integration tests that exercise the persistence layer
- A test server factory for HTTP-level integration tests that exercise the full request pipeline

For specific tool names and configuration, see [dotnet-format.md § 10](dotnet-format.md#10-testing-in-c).

### Naming

Test method names follow the pattern: `MethodName_Scenario_ExpectedResult`

```
Handle_WithValidCommand_ReturnsSuccessWithUserId
Handle_WithDuplicateEmail_ReturnsConflictError
Validate_WithShortPassword_ReturnsViolation
```

### Test Structure

All tests follow Arrange-Act-Assert:

```
// Arrange — set up inputs, mocks, and expected values
// Act     — invoke the unit under test
// Assert  — verify the result
```

### Test Organization

Unit tests mirror the source structure: a test file per source file, grouped by layer. Integration test classes are grouped by feature and share a test server fixture.

### Isolation

- Each test class gets its own database instance — no shared state between classes
- Tests within a class run sequentially to allow safe shared fixtures
- Each test seeds its own data — no reliance on data left by other tests
