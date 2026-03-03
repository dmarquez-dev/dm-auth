using DMAuth.Domain.Entities.AuthorizationCode;

namespace DMAuth.Domain.Interfaces;

/// <summary>
///		Provides access to OAuth 2.0 authorization code persistence.
/// </summary>
public interface IAuthorizationCodeRepository
	: IRepository<AuthorizationCode>
{
	/// <summary>
	///		Returns the authorization code with the given code hash, or null if not found.
	/// </summary>
	public Task<AuthorizationCode?> FindByCodeHashAsync(
		string codeHash,
		CancellationToken cancellationToken);
}
