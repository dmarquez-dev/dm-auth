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
