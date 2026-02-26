using DMAuth.Domain.Entities.Client;
using DMAuth.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DMAuth.Infrastructure.Persistence.Repositories;

/// <summary>
///		EF Core repository for OAuth 2.0 client registration persistence.
/// </summary>
public sealed class ClientRepository(
	DmAuthDbContext dbContext)
		: Repository<Client>(dbContext), IClientRepository
{
	/// <inheritdoc />
	public async Task<Client?> FindByClientIdAsync(
		string clientId,
		CancellationToken cancellationToken) =>
		await DbContext.Clients
			.FirstOrDefaultAsync(
				client =>
					client.ClientId == clientId,
				cancellationToken);

	/// <inheritdoc />
	public async Task<List<Client>> FindByOwnerIdAsync(
		Guid ownerId,
		CancellationToken cancellationToken) =>
		await DbContext.Clients
			.Where(client =>
				client.OwnerId == ownerId)
			.ToListAsync(cancellationToken);

	/// <inheritdoc />
	public async Task<bool> ExistsByClientIdAsync(
		string clientId,
		CancellationToken cancellationToken) =>
		await DbContext.Clients
			.AnyAsync(
				client =>
					client.ClientId == clientId,
				cancellationToken);
}
