using DMAuth.Domain.Exceptions;

namespace DMAuth.Domain.ValueObjects;

/// <summary>
///		Represents a validated and normalized email address.
/// </summary>
public record Email
{
	/// <summary>
	///		The lowercase email address string.
	/// </summary>
	public string Value { get; }

	/// <summary>
	///		Creates a new email value object after validating format and normalizing to lowercase.
	/// </summary>
	/// <param name="value">
	///		The raw email address string to validate.
	/// </param>
	/// <exception cref="DomainException">
	///		Thrown when the email is empty, missing '@', or exceeds 256 characters.
	/// </exception>
	public Email(
		string value)
	{
		if (string.IsNullOrWhiteSpace(value))
		{
			throw new DomainException("Email cannot be empty.");
		}

		if (!value.Contains('@') || value.Length > 256)
		{
			throw new DomainException("Email format is invalid.");
		}

		Value = value.ToLowerInvariant();
	}
}
