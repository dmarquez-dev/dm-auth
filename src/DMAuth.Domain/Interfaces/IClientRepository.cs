using DMAuth.Domain.Entities.Client;

namespace DMAuth.Domain.Interfaces;

/// <summary>
///		Provides access to OAuth 2.0 client registration persistence.
/// </summary>
public interface IClientRepository
	: IRepository<Client>
{
	/// <summary>
	///		Returns the client with the given OAuth 2.0 client identifier, or null if not found.
	/// </summary>
	public Task<Client?> FindByClientIdAsync(
		string clientId,
		CancellationToken cancellationToken);

	/// <summary>
	///		Returns all clients registered by the given owner.
	/// </summary>
	public Task<List<Client>> FindByOwnerIdAsync(
		Guid ownerId,
		CancellationToken cancellationToken);

	/// <summary>
	///		Returns true if a client with the given OAuth 2.0 client identifier already exists.
	/// </summary>
	public Task<bool> ExistsByClientIdAsync(
		string clientId,
		CancellationToken cancellationToken);
}
