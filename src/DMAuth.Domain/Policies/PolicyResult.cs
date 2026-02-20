namespace DMAuth.Domain.Policies;

/// <summary>
///		Represents the outcome of evaluating a domain policy.
/// </summary>
public sealed class PolicyResult
{
	/// <summary>
	///		Whether all policy rules were satisfied.
	/// </summary>
	public bool IsCompliant { get; }

	/// <summary>
	///		The list of violation messages when the policy is not compliant.
	///		Empty when <see cref="IsCompliant"/> is <see langword="true"/>.
	/// </summary>
	public IReadOnlyList<string> Violations { get; }

	/// <summary>
	///		A single space-separated string of all violation messages, suitable for use as an error message.
	///		Empty when <see cref="IsCompliant"/> is <see langword="true"/>.
	/// </summary>
	public string ViolationSummary =>
		string.Join(
			" ",
			Violations);

	private PolicyResult(
		bool isCompliant,
		IReadOnlyList<string> violations)
	{
		IsCompliant = isCompliant;
		Violations = violations;
	}

	/// <summary>
	///		Creates a compliant result with no violations.
	/// </summary>
	public static PolicyResult Compliant() =>
		new(
			true,
			[]);

	/// <summary>
	///		Creates a non-compliant result with the given violations.
	/// </summary>
	/// <param name="violations">
	///		The list of policy rules that were violated.
	/// </param>
	public static PolicyResult NonCompliant(IReadOnlyList<string> violations) =>
		new(
			false,
			violations);
}
