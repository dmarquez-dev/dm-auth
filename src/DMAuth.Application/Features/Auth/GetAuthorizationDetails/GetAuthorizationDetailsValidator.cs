using DMAuth.Application.Common.Settings;
using FluentValidation;

namespace DMAuth.Application.Features.Auth.GetAuthorizationDetails;

/// <summary>
///		Validates structural constraints on <see cref="GetAuthorizationDetailsQuery"/> before the handler runs.
/// </summary>
public sealed class GetAuthorizationDetailsValidator
	: AbstractValidator<GetAuthorizationDetailsQuery>
{
	/// <summary>
	///		Configures validation rules for each field.
	/// </summary>
	/// <param name="oauthSettings">
	///		OAuth 2.0 configuration, including the required client ID prefix.
	/// </param>
	public GetAuthorizationDetailsValidator(OAuthSettings oauthSettings)
	{
		var prefix = oauthSettings.ClientIdPrefix;

		RuleFor(query =>
			query.UserId)
			.NotEmpty();

		RuleFor(query =>
			query.OAuthClientId)
			.NotEmpty()
			.Must(id => id.StartsWith(prefix, StringComparison.Ordinal))
			.WithMessage($"client_id must begin with '{prefix}'.");

		RuleFor(query =>
			query.RequestedScopes)
			.NotEmpty();

		RuleForEach(query =>
			query.RequestedScopes)
			.NotEmpty();
	}
}
