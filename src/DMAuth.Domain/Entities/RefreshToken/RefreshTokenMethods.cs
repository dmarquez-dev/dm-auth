using DMAuth.Domain.Exceptions;

namespace DMAuth.Domain.Entities.RefreshToken;

public partial class RefreshToken
{
	/// <summary>
	///		Revokes this token, preventing it from being used for further exchanges.
	/// </summary>
	/// <exception cref="DomainException">
	///		Thrown when the token has already been revoked.
	/// </exception>
	public void Revoke()
	{
		DomainException.ThrowIf(
			RevokedAt.HasValue,
			"Refresh token is already revoked.");

		RevokedAt = DateTimeOffset.UtcNow;
	}

	/// <summary>
	///		Marks this token as consumed by rotation, recording the hash of its replacement.
	/// </summary>
	/// <param name="replacementTokenHash">
	///		The SHA-256 hash of the new token that replaces this one.
	/// </param>
	/// <exception cref="DomainException">
	///		Thrown when the token has already been revoked.
	/// </exception>
	public void Rotate(string replacementTokenHash)
	{
		DomainException.ThrowIf(
			RevokedAt.HasValue,
			"Cannot rotate a revoked refresh token.");

		ReplacedByToken = replacementTokenHash;
		RevokedAt = DateTimeOffset.UtcNow;
	}
}
