namespace DMAuth.Domain.Policies;

/// <summary>
///		Enforces validation rules for email addresses.
/// </summary>
public static class EmailPolicy
{
	private const int MaximumLength = 256;

	/// <summary>
	///		Validates an email address string against all email rules.
	/// </summary>
	/// <param name="value">
	///		The raw email address string to validate.
	/// </param>
	/// <returns>
	///		A <see cref="PolicyResult"/> containing any violations found.
	/// </returns>
	public static PolicyResult Validate(string value)
	{
		if (string.IsNullOrWhiteSpace(value))
		{
			return PolicyResult.NonCompliant(["Email cannot be empty."]);
		}

		var violations = new List<string>();

		if (value.Length > MaximumLength)
		{
			violations.Add($"Email must not exceed {MaximumLength} characters.");
		}

		if (!value.Contains('@'))
		{
			violations.Add("Email format is invalid.");
		}

		return violations.Count is 0
			? PolicyResult.Compliant()
			: PolicyResult.NonCompliant(violations);
	}
}
