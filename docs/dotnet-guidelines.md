# .NET Guidelines — DMAuth.Web / DMAuth.Api

> Best practices for writing performant, reliable ASP.NET Core code in this project.
> Based on the [ASP.NET Core Best Practices](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/best-practices?view=aspnetcore-10.0) documentation.

**Project context:** DMAuth runs on .NET 10. The Web layer is a controller-based ASP.NET Core API using EF Core with SQL Server. All API calls are authenticated; there is no Razor/MVC server-rendered HTML.

---

## Table of Contents

1. [Async / Await](#1-async--await) — CRITICAL
2. [Data Access & EF Core](#2-data-access--ef-core) — HIGH
3. [HttpContext Safety](#3-httpcontext-safety) — HIGH
4. [Memory & Allocations](#4-memory--allocations) — MEDIUM
5. [HTTP Clients](#5-http-clients) — MEDIUM
6. [Exception Handling](#6-exception-handling) — MEDIUM
7. [Request & Response Handling](#7-request--response-handling) — MEDIUM
8. [Performance Patterns](#8-performance-patterns) — LOW-MEDIUM

---

## 1. Async / Await

> **Impact: CRITICAL** — Blocking calls starve the thread pool and degrade throughput under concurrent load. Every I/O operation in this API must be async.

### 1.1 Never Block on Async Code

Do not call `.Result`, `.Wait()`, or `.GetAwaiter().GetResult()` on a `Task`. These block the current thread while waiting, which can cause deadlocks and thread pool starvation.

```csharp
// Incorrect — blocks thread pool thread
public IActionResult Get()
{
    var user = _userRepository.FindByEmailAsync(email).Result;
    return Ok(user);
}

// Correct — async all the way
public async Task<IActionResult> Get()
{
    var user = await _userRepository.FindByEmailAsync(email);
    return Ok(user);
}
```

### 1.2 Do Not Wrap Synchronous Code in Task.Run

`Task.Run` offloads to a thread pool thread. In ASP.NET Core, the request is already on a thread pool thread. Wrapping synchronous work in `Task.Run` adds scheduling overhead without any benefit.

```csharp
// Incorrect — pointless thread pool hop
var result = await Task.Run(() => ComputeSomething());

// Correct — call directly if synchronous, or use a true async API
var result = ComputeSomething();
```

**Exception:** Use `Task.Run` for CPU-bound work that would otherwise block for a long time (e.g., cryptographic key generation). For I/O, always use the native async API.

### 1.3 Make Controller Actions Async

All controller actions that perform any I/O must return `Task<IActionResult>` (or `Task<ActionResult<T>>`). Synchronous actions that call async code are a hidden deadlock risk.

```csharp
// Incorrect — sync action calling async code via .Result
[HttpGet("{id}")]
public IActionResult GetClient(Guid id)
{
    var client = _mediator.Send(new GetClientQuery(id)).Result;
    return Ok(client);
}

// Correct
[HttpGet("{id}")]
public async Task<IActionResult> GetClient(Guid id)
{
    var client = await _mediator.Send(new GetClientQuery(id));
    return Ok(client);
}
```

### 1.4 Pass CancellationToken Through the Call Stack

Accept `CancellationToken` in every async method and propagate it to all awaited calls. This allows in-flight requests to be cancelled cleanly when a client disconnects.

```csharp
// Correct — token flows through every layer
public async Task<IActionResult> Register(
    RegisterRequest request,
    CancellationToken cancellationToken)
{
    var result = await _mediator.Send(
        new RegisterUserCommand(request.Email, request.Username, request.Password),
        cancellationToken);
    return result.IsSuccess ? Ok() : BadRequest(result.Error);
}
```

---

## 2. Data Access & EF Core

> **Impact: HIGH** — Database access is typically the slowest part of a request. Small mistakes multiply under concurrent load.

### 2.1 Always Use Async EF Core Methods

Every EF Core database call must use its async equivalent. `ToList()`, `FirstOrDefault()`, `SaveChanges()` — all have async counterparts.

```csharp
// Incorrect
var users = dbContext.Users.ToList();

// Correct
var users = await dbContext.Users.ToListAsync(cancellationToken);
```

### 2.2 Use AsNoTracking for Read-Only Queries

When reading data that will not be modified and saved back, use `AsNoTracking()`. EF Core skips change tracking, which reduces memory allocation and CPU overhead.

```csharp
// Incorrect — tracks entities that will never be modified
var clients = await dbContext.Clients
    .Where(client => client.IsActive)
    .ToListAsync(cancellationToken);

// Correct — no tracking for read-only projections
var clients = await dbContext.Clients
    .AsNoTracking()
    .Where(client => client.IsActive)
    .ToListAsync(cancellationToken);
```

### 2.3 Project Only What You Need

Never select full entities when the use case only needs a subset of fields. Use `.Select()` to project to a DTO directly in the query.

```csharp
// Incorrect — loads all columns + navigation properties
var user = await dbContext.Users.FirstOrDefaultAsync(u => u.Id == id, ct);

// Correct — projects only required fields, translated to SQL SELECT
var dto = await dbContext.Users
    .AsNoTracking()
    .Where(user => user.Id == id)
    .Select(user =>
        new UserProfileDto(user.Id, user.Email.Value, user.Username, user.DisplayName))
    .FirstOrDefaultAsync(cancellationToken);
```

### 2.4 Avoid the N+1 Query Problem

Loading a collection and then accessing a navigation property on each item in a loop executes one query per item. Use `.Include()` or split queries to load related data in bulk.

```csharp
// Incorrect — 1 query for clients + N queries for redirect URIs
var clients = await dbContext.Clients.ToListAsync(ct);
foreach (var client in clients)
{
    var uris = client.RedirectUris; // triggers a query per client
}

// Correct — one query with JOIN
var clients = await dbContext.Clients
    .Include(client => client.RedirectUris)
    .ToListAsync(cancellationToken);
```

### 2.5 Filter in the Database, Not In-Memory

Apply `.Where()`, `.OrderBy()`, and `.Select()` before materializing the query. LINQ expressions over `IQueryable<T>` are translated to SQL; the same expressions over `IEnumerable<T>` run in-process on a fully-loaded collection.

```csharp
// Incorrect — loads entire table into memory, then filters
var active = (await dbContext.Clients.ToListAsync(ct))
    .Where(client => client.IsActive)
    .ToList();

// Correct — WHERE clause is in SQL
var active = await dbContext.Clients
    .Where(client => client.IsActive)
    .ToListAsync(cancellationToken);
```

### 2.6 Return Paginated Results for Collections

Returning unbounded collections risks memory exhaustion and slow response times. All list endpoints must accept pagination parameters.

```csharp
// Correct — page size enforced, both SQL OFFSET/FETCH applied
var clients = await dbContext.Clients
    .AsNoTracking()
    .OrderBy(client => client.CreatedAt)
    .Skip((page - 1) * pageSize)
    .Take(pageSize)
    .Select(client => new ClientSummaryDto(...))
    .ToListAsync(cancellationToken);
```

---

## 3. HttpContext Safety

> **Impact: HIGH** — Incorrect `HttpContext` usage causes data corruption, crashes, and subtle bugs that only appear under concurrent load.

### 3.1 Do Not Store HttpContext in a Field

`HttpContext` is valid only for the duration of a single request. Storing it in a field or variable and using it later (e.g., in a background task or after an `await`) reads a stale or null context.

```csharp
// Incorrect — captures HttpContext in constructor; may be stale when CheckAdmin() is called
public class BadAuthService
{
    private readonly HttpContext _context;

    public BadAuthService(IHttpContextAccessor accessor)
    {
        _context = accessor.HttpContext; // captured too early
    }

    public bool IsAdmin() => _context.User.IsInRole("admin");
}

// Correct — access HttpContext at the call site, always check for null
public class GoodAuthService
{
    private readonly IHttpContextAccessor _accessor;

    public GoodAuthService(IHttpContextAccessor accessor)
    {
        _accessor = accessor;
    }

    public bool IsAdmin()
    {
        var context = _accessor.HttpContext;
        return context?.User.IsInRole("admin") ?? false;
    }
}
```

### 3.2 Do Not Access HttpContext from Multiple Threads

`HttpContext` is not thread-safe. Never read from it inside a `Task.Run`, `Parallel.ForEach`, or parallel LINQ expression without copying the values you need first.

```csharp
// Incorrect — HttpContext.Request.Path accessed from a background thread
public async Task<IActionResult> Search(string query)
{
    var task1 = Task.Run(() => SearchProviderA(query, HttpContext.Request.Path));
    var task2 = Task.Run(() => SearchProviderB(query, HttpContext.Request.Path));
    await Task.WhenAll(task1, task2);
    return Ok();
}

// Correct — copy what you need before spawning threads
public async Task<IActionResult> Search(string query)
{
    var path = HttpContext.Request.Path.Value; // copy on the request thread
    var task1 = Task.Run(() => SearchProviderA(query, path));
    var task2 = Task.Run(() => SearchProviderB(query, path));
    await Task.WhenAll(task1, task2);
    return Ok();
}
```

### 3.3 Do Not Use HttpContext After the Request Completes

`HttpContext` is recycled once the response is sent. Do not access it from fire-and-forget tasks or `async void` methods.

```csharp
// Incorrect — async void completes the request at the first await
[HttpPost("/token")]
public async void Exchange(TokenRequest request)
{
    await Task.Delay(100);
    await Response.WriteAsync("done"); // crashes — response already disposed
}

// Correct — return Task so ASP.NET Core keeps the request alive
[HttpPost("/token")]
public async Task<IActionResult> Exchange(TokenRequest request)
{
    await Task.Delay(100);
    return Ok();
}
```

### 3.4 Do Not Capture Scoped Services in Background Threads

Scoped services (repositories, `DbContext`) are tied to the request lifetime. Capturing them in a `Task.Run` or a hosted service means using a disposed service.

```csharp
// Incorrect — DbContext may be disposed before the task runs
[HttpPost("/audit")]
public IActionResult Audit([FromServices] DmAuthDbContext context)
{
    _ = Task.Run(async () =>
    {
        await context.AuditLog.AddAsync(new AuditEntry()); // ObjectDisposedException
        await context.SaveChangesAsync();
    });
    return Accepted();
}

// Correct — create a new scope in the background task
[HttpPost("/audit")]
public IActionResult Audit([FromServices] IServiceScopeFactory scopeFactory)
{
    _ = Task.Run(async () =>
    {
        await using var scope = scopeFactory.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<DmAuthDbContext>();
        await context.AuditLog.AddAsync(new AuditEntry());
        await context.SaveChangesAsync();
    });
    return Accepted();
}
```

---

## 4. Memory & Allocations

> **Impact: MEDIUM** — Large object allocations trigger full GC pauses. Avoid on hot paths (token endpoint, auth check).

### 4.1 Do Not Read Large Request Bodies into a Single Buffer

Buffering an entire request body into a `string` or `byte[]` allocates on the Large Object Heap (≥ 85 KB) and may trigger full GC pauses. For the DMAuth use case (JSON payloads), use `System.Text.Json`'s streaming deserialization instead.

```csharp
// Incorrect — reads entire body into string, LOH allocation risk
public async Task<IActionResult> Post()
{
    var json = await new StreamReader(Request.Body).ReadToEndAsync();
    var data = JsonSerializer.Deserialize<TokenRequest>(json);
    return Ok();
}

// Correct — streaming deserialization, no large buffer
public async Task<IActionResult> Post()
{
    var data = await JsonSerializer.DeserializeAsync<TokenRequest>(Request.Body);
    return Ok();
}
```

### 4.2 Avoid Frequent Large Object Allocations on Hot Paths

The token endpoint, auth middleware, and `GET /api/users/me` are called on every authenticated request. Do not allocate large strings, `byte[]`, or `List<T>` per-call when a cached or pooled alternative exists.

- Cache signing credentials and RSA keys at startup — do not reload from Key Vault per request.
- Use `StringBuilder` pooling or `string.Create()` for dynamically-built strings.
- Return `IEnumerable<T>` (lazy) rather than materializing to `List<T>` when the caller only iterates once.

---

## 5. HTTP Clients

> **Impact: MEDIUM** — Applies whenever DMAuth makes outbound HTTP calls (e.g., future webhook delivery or external IdP integration).

### 5.1 Use HttpClientFactory, Not New HttpClient

Creating and disposing `HttpClient` directly leaves sockets in `TIME_WAIT` and can exhaust available ports under load. Always resolve `HttpClient` instances from `IHttpClientFactory`.

```csharp
// Incorrect — socket exhaustion risk
public class WebhookService
{
    public async Task NotifyAsync(string url, object payload)
    {
        using var client = new HttpClient();
        await client.PostAsJsonAsync(url, payload);
    }
}

// Correct — pooled connections via IHttpClientFactory
public class WebhookService(IHttpClientFactory httpClientFactory)
{
    public async Task NotifyAsync(
        string url,
        object payload,
        CancellationToken cancellationToken)
    {
        var client = httpClientFactory.CreateClient("webhooks");
        await client.PostAsJsonAsync(url, payload, cancellationToken);
    }
}
```

Register named clients in `Program.cs`:

```csharp
builder.Services.AddHttpClient("webhooks", client =>
{
    client.Timeout = TimeSpan.FromSeconds(10);
});
```

---

## 6. Exception Handling

> **Impact: MEDIUM** — Exceptions are expensive. Using them for normal control flow (e.g., "user not found") degrades throughput.

### 6.1 Do Not Use Exceptions for Normal Control Flow

Throwing and catching an exception is orders of magnitude slower than a conditional check. Use `Result<T>` (already in this codebase) for expected failure paths.

```csharp
// Incorrect — throws for a predictable "not found" condition
public async Task<User> GetUserAsync(Guid id, CancellationToken ct)
{
    var user = await _repo.FindByIdAsync(id, ct);
    if (user is null) throw new UserNotFoundException(id); // expensive
    return user;
}

// Correct — Result<T> communicates failure without exception overhead
public async Task<Result<User>> GetUserAsync(Guid id, CancellationToken ct)
{
    var user = await _repo.FindByIdAsync(id, ct);
    return user is null
        ? Result<User>.Failure("User not found.", ResultErrorType.NotFound)
        : Result<User>.Success(user);
}
```

### 6.2 Reserve Exceptions for Truly Unexpected Conditions

Exceptions are appropriate for: infrastructure failures (DB unreachable), programming errors (null argument), domain invariant violations (`DomainException`). They are not appropriate for: user validation errors, not-found lookups, or business rule checks that are expected to fail occasionally.

---

## 7. Request & Response Handling

> **Impact: MEDIUM** — Middleware and response pipeline correctness.

### 7.1 Prefer ReadFormAsync Over Request.Form

`HttpContext.Request.Form` performs synchronous I/O, which can starve the thread pool. Always use `ReadFormAsync`.

```csharp
// Incorrect
var form = HttpContext.Request.Form;

// Correct
var form = await HttpContext.Request.ReadFormAsync(cancellationToken);
```

### 7.2 Do Not Modify Headers After the Response Has Started

Once the first byte of the response body is written, headers are flushed to the client and cannot be changed.

```csharp
// Incorrect — throws if next() already wrote the body
app.Use(async (context, next) =>
{
    await next();
    context.Response.Headers["X-Custom"] = "value"; // may throw
});

// Correct — register a callback that runs before headers are sent
app.Use(async (context, next) =>
{
    context.Response.OnStarting(() =>
    {
        context.Response.Headers["X-Custom"] = "value";
        return Task.CompletedTask;
    });
    await next();
});
```

### 7.3 Do Not Call next() After Starting to Write the Response

If your middleware has already begun writing the response body, calling `next()` hands off to the next middleware which will attempt to write headers that are already sent.

---

## 8. Performance Patterns

> **Impact: LOW-MEDIUM** — Applied selectively on hot code paths.

### 8.1 Keep Middleware Fast

Middleware runs on every request. Avoid long-running operations, database calls, or synchronous I/O in middleware registered early in the pipeline. The authentication and exception-handling middleware in DMAuth must remain lean.

### 8.2 Cache Aggressively

Frequently-read, rarely-changed data (OIDC discovery document, JWKS, allowed scopes) should be cached in memory at startup and refreshed on a schedule, not re-read from the database or Key Vault per request.

```csharp
// Discovery document — computed once at startup, cached as a singleton
builder.Services.AddSingleton<IDiscoveryDocumentProvider, DiscoveryDocumentProvider>();
```

### 8.3 Use the Latest Runtime

Each .NET major release includes measurable throughput and latency improvements. This project targets .NET 10. Do not downgrade the `<TargetFramework>` without a documented reason.

### 8.4 Long-Running Work Belongs Outside the Request

If a request triggers work that cannot complete within a reasonable HTTP timeout (e.g., sending an email, calling a slow external API), hand it off to a background service or message queue and return `202 Accepted` immediately.

```csharp
// Correct — accept-and-queue pattern
[HttpPost("verify-email")]
public async Task<IActionResult> RequestVerification(
    [FromServices] IEmailQueue emailQueue,
    CancellationToken cancellationToken)
{
    await emailQueue.EnqueueAsync(new VerificationEmail(User.GetUserId()), cancellationToken);
    return Accepted();
}
```

---

## Quick Reference

| Category | Key Rules |
|---|---|
| **Async** | No `.Result`/`.Wait()`, no `Task.Run` for I/O, always pass `CancellationToken` |
| **EF Core** | `AsNoTracking` for reads, project with `.Select()`, paginate collections, filter in SQL |
| **HttpContext** | Never store in field, never access across threads, copy values before `Task.Run` |
| **Memory** | Avoid LOH allocations on hot paths, cache keys/credentials at startup |
| **HTTP Clients** | Always use `IHttpClientFactory` |
| **Exceptions** | Use `Result<T>` for expected failures, throw only for unexpected conditions |
| **Middleware** | Keep fast, register callbacks via `OnStarting`, don't call `next()` after writing body |
| **Performance** | Cache discovery/JWKS, use `202 Accepted` for long-running work |
