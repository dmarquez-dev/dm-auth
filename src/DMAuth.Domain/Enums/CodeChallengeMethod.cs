namespace DMAuth.Domain.Enums;

/// <summary>
///		Supported PKCE code challenge methods.
/// </summary>
public enum CodeChallengeMethod
{
	/// <summary>
	///		SHA-256 hash of the code verifier.
	///		This is the only supported method per ADR-003.
	/// </summary>
	S256
}
