using System.Text.Json.Serialization;

namespace DMAuth.Application.Features.Auth.GetUserInfo;

/// <summary>
///		The claims returned by the UserInfo endpoint.
///		Only fields covered by the token's granted scopes are populated;
///		all others are omitted from the JSON response.
/// </summary>
public record UserInfoResponse
{
	/// <summary>
	///		The subject identifier — the user's ID. Always present (requires <c>openid</c> scope).
	/// </summary>
	[JsonPropertyName("sub")]
	public string Sub { get; init; } = string.Empty;

	/// <summary>
	///		The user's display name. Present when the token includes the <c>profile</c> scope.
	/// </summary>
	[JsonPropertyName("name")]
	[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	public string? Name { get; init; }

	/// <summary>
	///		The user's username. Present when the token includes the <c>profile</c> scope.
	/// </summary>
	[JsonPropertyName("preferred_username")]
	[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	public string? PreferredUsername { get; init; }

	/// <summary>
	///		The user's email address. Present when the token includes the <c>email</c> scope.
	/// </summary>
	[JsonPropertyName("email")]
	[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	public string? Email { get; init; }

	/// <summary>
	///		Whether the user's email address has been verified.
	///		Present when the token includes the <c>email</c> scope.
	/// </summary>
	[JsonPropertyName("email_verified")]
	[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	public bool? EmailVerified { get; init; }
}
