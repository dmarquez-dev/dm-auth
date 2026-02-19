using DMAuth.Domain.Exceptions;

namespace DMAuth.Domain.ValueObjects;

/// <summary>
///		Represents a validated OAuth 2.0 redirect URI.
/// </summary>
public record RedirectUri
{
	/// <summary>
	///		The validated absolute URI string.
	/// </summary>
	public string Value { get; }

	/// <summary>
	///		Creates a new redirect URI value object after validating it is a valid absolute URI.
	/// </summary>
	/// <param name="value">
	///		The URI string to validate.
	/// </param>
	/// <exception cref="DomainException">
	///		Thrown when the URI is empty or not a valid absolute URI.
	/// </exception>
	public RedirectUri(string value)
	{
		if (string.IsNullOrWhiteSpace(value))
		{
			throw new DomainException("Redirect URI cannot be empty.");
		}

		if (!Uri.TryCreate(
				value,
				UriKind.Absolute, out _))
		{
			throw new DomainException("Redirect URI must be a valid absolute URI.");
		}

		Value = value;
	}
}
