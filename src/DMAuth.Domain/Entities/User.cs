using DMAuth.Domain.ValueObjects;

namespace DMAuth.Domain.Entities;

/// <summary>
///		Represents a user account.
///		Serves as an aggregate root for user-related operations.
/// </summary>
public class User
	: AuditableEntity
{
	/// <summary>
	///		The user's validated email address.
	/// </summary>
	public Email Email { get; private set; } = null!;

	/// <summary>
	///		The user's unique username.
	/// </summary>
	public string Username { get; private set; } = null!;

	/// <summary>
	///		The user's hashed password.
	/// </summary>
	public HashedPassword HashedPassword { get; private set; } = null!;

	/// <summary>
	///		The user's display name shown in profile and consent screens.
	/// </summary>
	public string DisplayName { get; private set; } = null!;

	/// <summary>
	///		Whether the user account is active and can authenticate.
	/// </summary>
	public bool IsActive { get; private set; }

	/// <summary>
	///		Whether the user's email address has been verified.
	/// </summary>
	public bool EmailVerified { get; private set; }

	private User() { }

	/// <summary>
	///		Creates a new active user with an unverified email.
	/// </summary>
	/// <param name="email">
	///		The user's email address.
	/// </param>
	/// <param name="username">
	///		The user's unique username.
	/// </param>
	/// <param name="password">
	///		The user's pre-hashed password.
	/// </param>
	/// <param name="displayName">
	///		The user's display name.
	/// </param>
	public User(
		Email email,
		string username,
		HashedPassword password,
		string displayName)
	{
		Email = email;
		Username = username;
		HashedPassword = password;
		DisplayName = displayName;
		IsActive = true;
		EmailVerified = false;
	}
}
