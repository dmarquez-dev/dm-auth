using DMAuth.Domain.Entities.Consent;
using DMAuth.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DMAuth.Infrastructure.Persistence.Repositories;

/// <summary>
///		EF Core repository for user consent record persistence.
/// </summary>
public sealed class ConsentRepository(
	DmAuthDbContext dbContext)
		: Repository<Consent>(dbContext), IConsentRepository
{
	/// <inheritdoc />
	public async Task<Consent?> FindByUserAndClientAsync(
		Guid userId,
		Guid clientId,
		CancellationToken cancellationToken) =>
		await DbContext.Consents
			.FirstOrDefaultAsync(
				consent =>
					consent.UserId == userId && consent.ClientId == clientId,
				cancellationToken);
}
