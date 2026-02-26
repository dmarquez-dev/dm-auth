using DMAuth.Domain.Exceptions;
using DMAuth.Domain.Policies;

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
	///		Creates a new email value object after validating it against <see cref="EmailPolicy"/>
	///		and normalizing to lowercase.
	/// </summary>
	/// <param name="value">
	///		The raw email address string to validate.
	/// </param>
	/// <exception cref="DomainException">
	///		Thrown when the email violates any email policy rule.
	/// </exception>
	public Email(string value)
	{
		var result = EmailPolicy.Validate(value);

		if (!result.IsCompliant)
		{
			throw new DomainException(result.ViolationSummary);
		}

		Value = value.ToLowerInvariant();
	}
}
