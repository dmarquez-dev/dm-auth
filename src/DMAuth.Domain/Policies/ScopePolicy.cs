using DMAuth.Domain.Enums;

namespace DMAuth.Domain.Policies;

/// <summary>
///		Enforces validation rules for OAuth 2.0 / OIDC scopes.
/// </summary>
public static class ScopePolicy
{
	private static readonly Dictionary<ScopeType, string> _scopeNames = new()
	{
		[ScopeType.OpenId] = "openid",
		[ScopeType.Profile] = "profile",
		[ScopeType.Email] = "email",
		[ScopeType.OfflineAccess] = "offline_access",
	};

	private static readonly HashSet<string> _allowedScopes = _scopeNames.Values.ToHashSet();

	/// <summary>
	///		Validates a scope string against all recognized scope types.
	/// </summary>
	/// <param name="value">
	///		The scope string to validate.
	/// </param>
	/// <returns>
	///		A <see cref="PolicyResult"/> containing any violations found.
	/// </returns>
	public static PolicyResult Validate(string value)
	{
		if (string.IsNullOrWhiteSpace(value))
		{
			return PolicyResult.NonCompliant(["Scope cannot be empty."]);
		}

		var violations = new List<string>();

		if (!_allowedScopes.Contains(value.ToLowerInvariant()))
		{
			violations.Add(
				$"Scope '{value}' is not a recognized scope. Allowed scopes: {string.Join(", ", _allowedScopes)}.");
		}

		return violations.Count is 0
			? PolicyResult.Compliant()
			: PolicyResult.NonCompliant(violations);
	}
}
