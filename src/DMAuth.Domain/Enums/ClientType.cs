namespace DMAuth.Domain.Enums;

/// <summary>
///		Identifies the OAuth 2.0 client type.
/// </summary>
public enum ClientType
{
	/// <summary>
	///		A server-side client that can securely store credentials.
	/// </summary>
	Confidential,

	/// <summary>
	///		A client-side application (e.g., SPA or mobile) that cannot securely store credentials.
	/// </summary>
	Public
}
