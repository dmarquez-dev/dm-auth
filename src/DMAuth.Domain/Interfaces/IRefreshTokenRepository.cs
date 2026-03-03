using DMAuth.Domain.Entities.RefreshToken;

namespace DMAuth.Domain.Interfaces;

/// <summary>
///		Provides access to refresh token persistence.
/// </summary>
public interface IRefreshTokenRepository
	: IRepository<RefreshToken>
{
	/// <summary>
	///		Returns the refresh token with the given hash, or null if not found.
	/// </summary>
	public Task<RefreshToken?> FindByTokenHashAsync(
		string tokenHash,
		CancellationToken cancellationToken);

	/// <summary>
	///		Revokes all non-revoked tokens belonging to the given family.
	/// </summary>
	public Task RevokeByTokenFamilyAsync(
		Guid familyId,
		CancellationToken cancellationToken);
}
