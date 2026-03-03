namespace DMAuth.Application.Features.Auth.GrantConsent;

/// <summary>
///		The result of a successful consent grant, containing the plain authorization code
///		to be appended to the redirect URI.
/// </summary>
/// <param name="PlainCode">
///		The unhashed authorization code. The controller appends this to the redirect URI as
///		the "code" query parameter.
/// </param>
public record GrantConsentResponse(string PlainCode);
