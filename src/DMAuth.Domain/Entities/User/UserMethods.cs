using DMAuth.Domain.Exceptions;
using DMAuth.Domain.ValueObjects;

namespace DMAuth.Domain.Entities.User;

public partial class User
{
	/// <summary>
	///		Replaces the user's password with a new pre-hashed value.
	/// </summary>
	/// <param name="newPassword">
	///		The new hashed password to set.
	/// </param>
	public void ChangePassword(HashedPassword newPassword)
	{
		HashedPassword = newPassword;
		UpdatedAt = DateTimeOffset.UtcNow;
	}

	/// <summary>
	///		Updates the user's display name.
	/// </summary>
	/// <param name="displayName">
	///		The new display name to set.
	/// </param>
	public void UpdateProfile(string displayName)
	{
		DisplayName = displayName;
		UpdatedAt = DateTimeOffset.UtcNow;
	}

	/// <summary>
	///		Deactivates the user account, preventing further authentication.
	/// </summary>
	/// <exception cref="DomainException">
	///		Thrown when the account is already inactive.
	/// </exception>
	public void Deactivate()
	{
		DomainException.ThrowIf(
			!IsActive,
			"User account is already inactive.");

		IsActive = false;
		UpdatedAt = DateTimeOffset.UtcNow;
	}
}
