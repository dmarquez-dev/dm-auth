namespace DMAuth.Application.Features.Auth.GetAuthorizationDetails;

/// <summary>
///		The result of checking whether a user's existing consent covers a set of requested scopes.
///		The client is identified by its public OAuth 2.0 client identifier throughout; no internal
///		database identifiers are exposed.
/// </summary>
/// <param name="IsConsentRequired">
///		True when the user must visit the consent page; false when existing consent already covers
///		all requested scopes and the authorization code can be issued directly (task 4.5).
/// </param>
public record GetAuthorizationDetailsResponse(
	bool IsConsentRequired);
