using DMAuth.Domain.Exceptions;

namespace DMAuth.Domain.Entities.AuthorizationCode;

public partial class AuthorizationCode
{
	/// <summary>
	///		Marks this authorization code as consumed, preventing reuse.
	/// </summary>
	/// <exception cref="DomainException">
	///		Thrown when the code has already been used.
	/// </exception>
	public void MarkAsUsed()
	{
		DomainException.ThrowIf(
			UsedAt.HasValue,
			"Authorization code has already been used.");

		UsedAt = DateTimeOffset.UtcNow;
	}
}
