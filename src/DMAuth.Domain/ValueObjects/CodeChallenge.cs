using DMAuth.Domain.Exceptions;
using DMAuth.Domain.Policies;

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
	///		Creates a new code challenge value object after validating it against <see cref="CodeChallengePolicy"/>.
	/// </summary>
	/// <param name="value">
	///		The code challenge string to validate.
	/// </param>
	/// <exception cref="DomainException">
	///		Thrown when the code challenge violates any PKCE policy rule.
	/// </exception>
	public CodeChallenge(string value)
	{
		var result = CodeChallengePolicy.Validate(value);

		if (!result.IsCompliant)
		{
			throw new DomainException(result.ViolationSummary);
		}

		Value = value;
	}
}
