using DMAuth.Domain.Exceptions;
using DMAuth.Domain.Policies;

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
	///		Creates a new redirect URI value object after validating it against <see cref="RedirectUriPolicy"/>.
	/// </summary>
	/// <param name="value">
	///		The URI string to validate.
	/// </param>
	/// <exception cref="DomainException">
	///		Thrown when the URI violates any redirect URI policy rule.
	/// </exception>
	public RedirectUri(string value)
	{
		var result = RedirectUriPolicy.Validate(value);

		if (!result.IsCompliant)
		{
			throw new DomainException(result.ViolationSummary);
		}

		Value = value;
	}
}
