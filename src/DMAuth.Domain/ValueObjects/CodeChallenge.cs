using DMAuth.Domain.Exceptions;

namespace DMAuth.Domain.ValueObjects;

/// <summary>
///		Represents a PKCE code challenge value used during the authorization code flow.
/// </summary>
public record CodeChallenge
{
	/// <summary>
	///		The code challenge string.
	/// </summary>
	public string Value { get; }

	/// <summary>
	///		Creates a new code challenge value object.
	/// </summary>
	/// <param name="value">
	///		The code challenge string to validate.
	/// </param>
	/// <exception cref="DomainException">
	///		Thrown when the code challenge is empty.
	/// </exception>
	public CodeChallenge(
		string value)
	{
		if (string.IsNullOrWhiteSpace(value))
		{
			throw new DomainException("Code challenge cannot be empty.");
		}

		Value = value;
	}
}
