using DMAuth.Domain.Enums;
using DMAuth.Domain.Exceptions;

namespace DMAuth.Domain.ValueObjects;

/// <summary>
///		Represents a validated OAuth 2.0 / OIDC scope, enforced against the <see cref="ScopeType"/> enum.
/// </summary>
public record Scope
{
	private static readonly HashSet<string> _allowedScopes = Enum
		.GetValues<ScopeType>()
		.Select(scope =>
			scope
				.ToString()
				.ToLowerInvariant())
		.ToHashSet();

	/// <summary>
	///		The lowercase scope string.
	/// </summary>
	public string Value { get; }

	/// <summary>
	///		Creates a new scope value object after validating against recognized scope types.
	/// </summary>
	/// <param name="value">
	///		The scope string to validate.
	/// </param>
	/// <exception cref="DomainException">
	///		Thrown when the scope is empty or not a recognized scope type.
	/// </exception>
	public Scope(string value)
	{
		if (string.IsNullOrWhiteSpace(value))
		{
			throw new DomainException("Scope cannot be empty.");
		}

		if (!_allowedScopes.Contains(value.ToLowerInvariant()))
		{
			throw new DomainException(
				$"Scope '{value}' is not a recognized scope. " +
				$"Allowed scopes: {string.Join(", ", _allowedScopes)}.");
		}

		Value = value.ToLowerInvariant();
	}
}
