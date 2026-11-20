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
