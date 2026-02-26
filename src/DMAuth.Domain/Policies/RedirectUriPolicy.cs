namespace DMAuth.Domain.Policies;

/// <summary>
///		Enforces validation rules for OAuth 2.0 redirect URIs.
/// </summary>
public static class RedirectUriPolicy
{
	/// <summary>
	///		Validates a redirect URI string against all redirect URI rules.
	/// </summary>
	/// <param name="value">
	///		The URI string to validate.
	/// </param>
	/// <returns>
	///		A <see cref="PolicyResult"/> containing any violations found.
	/// </returns>
	public static PolicyResult Validate(string value)
	{
		if (string.IsNullOrWhiteSpace(value))
		{
			return PolicyResult.NonCompliant(["Redirect URI cannot be empty."]);
		}

		if (!Uri.TryCreate(value, UriKind.Absolute, out var uri))
		{
			return PolicyResult.NonCompliant(["Redirect URI must be a valid absolute URI."]);
		}

		var violations = new List<string>();

		if (!string.IsNullOrEmpty(uri.Fragment))
		{
			violations.Add("Redirect URI must not contain a fragment (#).");
		}

		var isLocalhost = uri.Host.Equals("localhost", StringComparison.OrdinalIgnoreCase)
			|| uri.Host.Equals("127.0.0.1", StringComparison.OrdinalIgnoreCase);

		if (!isLocalhost && uri.Scheme != Uri.UriSchemeHttps)
		{
			violations.Add("Redirect URI must use HTTPS unless the host is localhost.");
		}

		return violations.Count is 0
			? PolicyResult.Compliant()
			: PolicyResult.NonCompliant(violations);
	}
}
