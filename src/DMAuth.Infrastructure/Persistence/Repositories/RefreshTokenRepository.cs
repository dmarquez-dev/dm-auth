using DMAuth.Domain.Entities.RefreshToken;
using DMAuth.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DMAuth.Infrastructure.Persistence.Repositories;

/// <summary>
///		EF Core implementation of <see cref="IRefreshTokenRepository"/>.
/// </summary>
public sealed class RefreshTokenRepository(DmAuthDbContext dbContext)
	: Repository<RefreshToken>(dbContext), IRefreshTokenRepository
{
	/// <inheritdoc />
	public async Task<RefreshToken?> FindByTokenHashAsync(
		string tokenHash,
		CancellationToken cancellationToken) =>
		await DbContext.RefreshTokens.FirstOrDefaultAsync(
			token => token.TokenHash == tokenHash,
			cancellationToken);

	/// <inheritdoc />
	public async Task RevokeByTokenFamilyAsync(
		Guid familyId,
		CancellationToken cancellationToken)
	{
		var tokens = await DbContext.RefreshTokens
			.Where(token =>
				token.FamilyId == familyId
				&& token.RevokedAt == null)
			.ToListAsync(cancellationToken);

		foreach (var token in tokens)
		{
			token.Revoke();
		}
	}
}
