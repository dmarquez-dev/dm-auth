# EF Core

## Queries

- Use the async form of every EF Core database call (`ToListAsync`, `FirstOrDefaultAsync`, `SaveChangesAsync`, ...) and pass the `CancellationToken`; never call the synchronous `ToList()`, `FirstOrDefault()` or `SaveChanges()`.
- Add `AsNoTracking()` to every query whose results will not be modified and saved back.
- Project with `.Select()` to a DTO inside the query whenever the use case needs only a subset of fields; do not load full entities to read a few columns.
- Load related data in bulk with `.Include()` or a split query; never access a navigation property per item in a loop over a materialized collection.
- Apply `.Where()`, `.OrderBy()` and `.Select()` to the `IQueryable<T>` before materializing; never filter, sort or project a collection already loaded into memory.
- Every query backing a list endpoint takes pagination parameters and applies `.OrderBy()` before `.Skip((page - 1) * pageSize).Take(pageSize)`; never return an unbounded collection.
