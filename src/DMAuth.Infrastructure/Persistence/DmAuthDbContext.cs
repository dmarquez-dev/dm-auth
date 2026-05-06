using DMAuth.Application.Common.Interfaces;
using DMAuth.Domain.Entities.AuthorizationCode;
using DMAuth.Domain.Entities.Client;
using DMAuth.Domain.Entities.Consent;
using DMAuth.Domain.Entities.RefreshToken;
using DMAuth.Domain.Entities.User;
using Microsoft.AspNetCore.DataProtection.EntityFrameworkCore;
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
		: DbContext(options), IUnitOfWork, IDataProtectionKeyContext
{
	/// <summary>
	///		Data Protection keys for session cookie encryption persistence.
	/// </summary>
	public DbSet<DataProtectionKey> DataProtectionKeys =>
		Set<DataProtectionKey>();

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
	Task IUnitOfWork.SaveChangesAsync(CancellationToken cancellationToken) =>
		base.SaveChangesAsync(cancellationToken);

	/// <inheritdoc />
	protected override void OnModelCreating(ModelBuilder modelBuilder)
	{
		modelBuilder.ApplyConfigurationsFromAssembly(typeof(DmAuthDbContext).Assembly);
	}
}
