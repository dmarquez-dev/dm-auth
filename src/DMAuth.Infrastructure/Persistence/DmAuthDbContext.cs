using DMAuth.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace DMAuth.Infrastructure.Persistence;

/// <summary>
///		EF Core database context for the DM Auth authorization server.
/// </summary>
/// <param name="options">
///		The database context options configured via dependency injection.
/// </param>
public class DmAuthDbContext(
	DbContextOptions<DmAuthDbContext> options)
		: DbContext(options)
{
	/// <summary>
	///	User accounts.
	/// </summary>
	public DbSet<User> Users =>
		Set<User>();

	/// <summary>
	///	OAuth 2.0 client registrations.
	/// </summary>
	public DbSet<Client> Clients =>
		Set<Client>();

	/// <summary>
	///	Refresh tokens for token rotation and revocation.
	/// </summary>
	public DbSet<RefreshToken> RefreshTokens =>
		Set<RefreshToken>();

	/// <summary>
	///	Short-lived authorization codes for the authorization code flow.
	/// </summary>
	public DbSet<AuthorizationCode> AuthorizationCodes =>
		Set<AuthorizationCode>();

	/// <summary>
	///	User consent grants for client applications.
	/// </summary>
	public DbSet<Consent> Consents =>
		Set<Consent>();

	/// <inheritdoc />
	protected override void OnModelCreating(
		ModelBuilder modelBuilder)
	{
		modelBuilder.ApplyConfigurationsFromAssembly(
			typeof(DmAuthDbContext).Assembly);
	}
}
