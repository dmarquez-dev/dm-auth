using DMAuth.Domain.Entities;
using DMAuth.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DMAuth.Infrastructure.Persistence.Repositories;

/// <summary>
///		Generic EF Core repository providing common persistence operations for aggregate roots.
/// </summary>
/// <typeparam name="T">
///		The aggregate root entity type.
/// </typeparam>
public class Repository<T>(
	DmAuthDbContext dbContext)
		: IRepository<T>
			where T : Entity
{
	/// <summary>
	///		The database context used by this repository and its subclasses.
	/// </summary>
	protected readonly DmAuthDbContext DbContext = dbContext;

	/// <inheritdoc />
	public async Task<T?> FindByIdAsync(
		Guid id,
		CancellationToken cancellationToken) =>
		await DbContext.Set<T>()
			.FirstOrDefaultAsync(
				entity =>
					entity.Id == id,
				cancellationToken);

	/// <inheritdoc />
	public void Add(T entity) =>
		DbContext.Set<T>().Add(entity);

	/// <inheritdoc />
	public void Update(T entity) =>
		DbContext.Set<T>().Update(entity);
}
