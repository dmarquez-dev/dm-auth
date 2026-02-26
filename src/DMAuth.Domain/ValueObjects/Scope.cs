using DMAuth.Domain.Exceptions;
using DMAuth.Domain.Policies;

namespace DMAuth.Domain.ValueObjects;

/// <summary>
///		Represents a validated OAuth 2.0 / OIDC scope, enforced against <see cref="ScopePolicy"/>.
/// </summary>
public record Scope
{
	/// <summary>
	///		The lowercase scope string.
	/// </summary>
	public string Value { get; }

	/// <summary>
	///		Creates a new scope value object after validating it against <see cref="ScopePolicy"/>.
	/// </summary>
	/// <param name="value">
	///		The scope string to validate.
	/// </param>
	/// <exception cref="DomainException">
	///		Thrown when the scope violates any scope policy rule.
	/// </exception>
	public Scope(string value)
	{
		var result = ScopePolicy.Validate(value);

		if (!result.IsCompliant)
		{
			throw new DomainException(result.ViolationSummary);
		}

		Value = value.ToLowerInvariant();
	}
}
