using DMAuth.Application.Common.Interfaces;
using DMAuth.Domain.ValueObjects;

namespace DMAuth.Infrastructure.Security;

/// <summary>
///		BCrypt implementation of <see cref="IPasswordHasher"/> with a work factor of 12.
/// </summary>
public sealed class BcryptPasswordHasher
	: IPasswordHasher
{
	private const int WorkFactor = 12;

	/// <inheritdoc />
	public HashedPassword Hash(string password) =>
		new(BCrypt.Net.BCrypt.HashPassword(
			password,
			WorkFactor));

	/// <inheritdoc />
	public bool Verify(
		string password,
		HashedPassword hashedPassword) =>
		BCrypt.Net.BCrypt.Verify(
			password,
			hashedPassword.Value);
}
