using DMAuth.Domain.Entities.AuthorizationCode;
using DMAuth.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DMAuth.Infrastructure.Persistence.Repositories;

/// <summary>
///		EF Core repository for OAuth 2.0 authorization code persistence.
/// </summary>
public sealed class AuthorizationCodeRepository(
	DmAuthDbContext dbContext)
		: Repository<AuthorizationCode>(dbContext), IAuthorizationCodeRepository
{
	/// <inheritdoc />
	public async Task<AuthorizationCode?> FindByCodeHashAsync(
		string codeHash,
		CancellationToken cancellationToken) =>
		await DbContext.AuthorizationCodes
			.FirstOrDefaultAsync(
				authCode =>
					authCode.CodeHash == codeHash,
				cancellationToken);
}
