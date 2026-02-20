namespace DMAuth.Domain.Policies;

/// <summary>
///		Enforces password complexity rules for the domain.
/// </summary>
public static class PasswordPolicy
{
	private const int MinimumLength = 8;

	/// <summary>
	///		Validates a plain-text password against all password complexity rules.
	/// </summary>
	/// <param name="password">
	///		The plain-text password to validate.
	/// </param>
	/// <returns>
	///		A <see cref="PolicyResult"/> containing any violations found.
	/// </returns>
	public static PolicyResult Validate(string password)
	{
		var violations = new List<string>();

		if (password.Length < MinimumLength)
		{
			violations.Add($"Password must be at least {MinimumLength} characters long.");
		}

		if (!password.Any(char.IsDigit))
		{
			violations.Add("Password must contain at least one digit.");
		}

		if (!password.Any(char.IsPunctuation) && !password.Any(char.IsSymbol))
		{
			violations.Add("Password must contain at least one special character.");
		}

		return violations.Count is 0
			? PolicyResult.Compliant()
			: PolicyResult.NonCompliant(violations);
	}
}
