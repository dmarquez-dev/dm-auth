using DMAuth.Domain.Entities;

namespace DMAuth.Domain.Interfaces;

/// <summary>
///		Defines common persistence operations shared by all aggregate root repositories.
/// </summary>
/// <typeparam name="T">
///		The aggregate root entity type.
/// </typeparam>
public interface IRepository<T>
	where T : Entity
{
	/// <summary>
	///		Returns the entity with the given identifier, or null if not found.
	/// </summary>
	public Task<T?> FindByIdAsync(
		Guid id,
		CancellationToken cancellationToken);

	/// <summary>
	///		Queues a new entity for insertion on the next unit of work commit.
	/// </summary>
	public void Add(T entity);

	/// <summary>
	///		Marks an existing entity as modified on the next unit of work commit.
	/// </summary>
	public void Update(T entity);
}
