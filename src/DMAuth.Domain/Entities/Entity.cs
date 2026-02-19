namespace DMAuth.Domain.Entities;

/// <summary>
///		Abstract base class for all domain entities.
///		Provides identity and creation tracking.
/// </summary>
public abstract class Entity
{
	/// <summary>
	///		Unique identifier for this entity.
	/// </summary>
	public Guid Id { get; private init; }

	/// <summary>
	///		Timestamp of when this entity was created.
	/// </summary>
	public DateTimeOffset CreatedAt { get; private init; }

	/// <summary>
	///		Initializes a new entity with a generated identifier and the current UTC timestamp.
	/// </summary>
	protected Entity()
	{
		Id = Guid.NewGuid();
		CreatedAt = DateTimeOffset.UtcNow;
	}
}
