namespace DMAuth.Application.Common.Interfaces;

/// <summary>
///		Represents a unit of work that atomically commits all tracked changes to the store.
/// </summary>
public interface IUnitOfWork
{
	/// <summary>
	///		Commits all pending changes to the underlying store.
	/// </summary>
	/// <param name="cancellationToken">
	///		A token to cancel the operation.
	/// </param>
	public Task SaveChangesAsync(CancellationToken cancellationToken);
}
