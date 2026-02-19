namespace DMAuth.Domain.Entities;

/// <summary>
///		Abstract base class for entities that track modification timestamps.
/// </summary>
public abstract class AuditableEntity
	: Entity
{
	/// <summary>
	///		Timestamp of the last modification, or null if never modified.
	/// </summary>
	public DateTimeOffset? UpdatedAt { get; protected set; }
}
