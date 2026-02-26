namespace DMAuth.Application.Features.Clients.Register;

/// <summary>
///		The result of a successful client registration.
/// </summary>
/// <param name="ClientId">
///		The database identifier of the newly registered client.
/// </param>
/// <param name="OAuthClientId">
///		The generated OAuth 2.0 client identifier used in authorization requests.
/// </param>
/// <param name="ClientSecret">
///		The plaintext client secret for confidential clients, returned once at registration.
///		Always null for public clients.
/// </param>
public record RegisterClientResponse(
	Guid ClientId,
	string OAuthClientId,
	string? ClientSecret);
