using DMAuth.Domain.Entities.Consent;

namespace DMAuth.Domain.Interfaces;

/// <summary>
///		Provides access to user consent record persistence.
/// </summary>
public interface IConsentRepository
	: IRepository<Consent>
{
	/// <summary>
	///		Returns the consent record for the given user and client pair, or null if no consent exists.
	/// </summary>
	public Task<Consent?> FindByUserAndClientAsync(
		Guid userId,
		Guid clientId,
		CancellationToken cancellationToken);
}
