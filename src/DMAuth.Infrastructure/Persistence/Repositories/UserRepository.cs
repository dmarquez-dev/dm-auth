using DMAuth.Domain.Entities.User;
using DMAuth.Domain.Interfaces;
using DMAuth.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace DMAuth.Infrastructure.Persistence.Repositories;

/// <summary>
///		EF Core repository for user account persistence.
/// </summary>
public sealed class UserRepository(
	DmAuthDbContext dbContext)
		: Repository<User>(dbContext), IUserRepository
{
	/// <inheritdoc />
	public async Task<User?> FindByEmailAsync(
		Email email,
		CancellationToken cancellationToken) =>
		await DbContext.Users
			.FirstOrDefaultAsync(
				user =>
					user.Email == email,
				cancellationToken);

	/// <inheritdoc />
	public async Task<User?> FindByUsernameAsync(
		string username,
		CancellationToken cancellationToken) =>
		await DbContext.Users
			.FirstOrDefaultAsync(
				user =>
					user.Username == username,
				cancellationToken);

	/// <inheritdoc />
	public async Task<bool> ExistsByEmailAsync(
		Email email,
		CancellationToken cancellationToken) =>
		await DbContext.Users
			.AnyAsync(
				user =>
					user.Email == email,
				cancellationToken);

	/// <inheritdoc />
	public async Task<bool> ExistsByUsernameAsync(
		string username,
		CancellationToken cancellationToken) =>
		await DbContext.Users
			.AnyAsync(
				user =>
					user.Username == username,
				cancellationToken);
}
