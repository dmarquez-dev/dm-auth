using DMAuth.Domain.ValueObjects;

namespace DMAuth.Application.Common.Interfaces;

/// <summary>
///		Provides password hashing and verification operations.
/// </summary>
public interface IPasswordHasher
{
	/// <summary>
	///		Hashes a plain-text password and returns a typed value object.
	/// </summary>
	/// <param name="password">
	///		The plain-text password to hash.
	/// </param>
	public HashedPassword Hash(string password);

	/// <summary>
	///		Verifies a plain-text password against a stored hashed password.
	/// </summary>
	/// <param name="password">
	///		The plain-text password to verify.
	/// </param>
	/// <param name="hashedPassword">
	///		The stored hashed password to verify against.
	/// </param>
	public bool Verify(
		string password,
		HashedPassword hashedPassword);
}
