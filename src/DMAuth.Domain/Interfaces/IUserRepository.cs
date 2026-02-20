using DMAuth.Domain.Entities.User;
using DMAuth.Domain.ValueObjects;

namespace DMAuth.Domain.Interfaces;

/// <summary>
///		Provides access to user account persistence.
/// </summary>
public interface IUserRepository
	: IRepository<User>
{
	/// <summary>
	///		Returns the user with the given email address, or null if not found.
	/// </summary>
	public Task<User?> FindByEmailAsync(
		Email email,
		CancellationToken cancellationToken);

	/// <summary>
	///		Returns the user with the given username, or null if not found.
	/// </summary>
	public Task<User?> FindByUsernameAsync(
		string username,
		CancellationToken cancellationToken);

	/// <summary>
	///		Returns true if a user with the given email address already exists.
	/// </summary>
	public Task<bool> ExistsByEmailAsync(
		Email email,
		CancellationToken cancellationToken);

	/// <summary>
	///		Returns true if a user with the given username already exists.
	/// </summary>
	public Task<bool> ExistsByUsernameAsync(
		string username,
		CancellationToken cancellationToken);
}
