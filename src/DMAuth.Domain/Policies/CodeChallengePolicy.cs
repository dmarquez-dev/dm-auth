namespace DMAuth.Domain.Policies;

/// <summary>
///		Enforces validation rules for PKCE code challenges per RFC 7636.
/// </summary>
public static class CodeChallengePolicy
{
	private const int MinimumLength = 43;
	private const int MaximumLength = 128;

	/// <summary>
	///		Validates a code challenge string against all PKCE format rules.
	/// </summary>
	/// <param name="value">
	///		The code challenge string to validate.
	/// </param>
	/// <returns>
	///		A <see cref="PolicyResult"/> containing any violations found.
	/// </returns>
	public static PolicyResult Validate(string value)
	{
		if (string.IsNullOrWhiteSpace(value))
		{
			return PolicyResult.NonCompliant(["Code challenge cannot be empty."]);
		}

		var violations = new List<string>();

		if (value.Length is < MinimumLength or > MaximumLength)
		{
			violations.Add(
				$"Code challenge must be between {MinimumLength} and {MaximumLength} characters.");
		}

		if (!IsValidBase64Url(value))
		{
			violations.Add(
				"Code challenge must use only Base64url characters (A-Z, a-z, 0-9, '-', '_', '.').");
		}

		return violations.Count is 0
			? PolicyResult.Compliant()
			: PolicyResult.NonCompliant(violations);
	}

	private static bool IsValidBase64Url(string value) =>
		value.All(character =>
			char.IsAsciiLetterOrDigit(character)
			|| character is '-' or '_' or '.');
}
