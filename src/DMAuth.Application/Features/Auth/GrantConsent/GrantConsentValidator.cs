using DMAuth.Application.Common.Settings;
using FluentValidation;

namespace DMAuth.Application.Features.Auth.GrantConsent;

/// <summary>
///		Validates structural constraints on <see cref="GrantConsentCommand"/> before the handler runs.
/// </summary>
public sealed class GrantConsentValidator
	: AbstractValidator<GrantConsentCommand>
{
	/// <summary>
	///		Configures validation rules for each field.
	/// </summary>
	/// <param name="oauthSettings">
	///		OAuth 2.0 configuration, including the required client ID prefix.
	/// </param>
	public GrantConsentValidator(OAuthSettings oauthSettings)
	{
		var prefix = oauthSettings.ClientIdPrefix;

		RuleFor(command =>
			command.OAuthClientId)
			.NotEmpty()
			.Must(id => id.StartsWith(prefix, StringComparison.Ordinal))
			.WithMessage($"client_id must begin with '{prefix}'.");

		RuleFor(command =>
			command.GrantedScopes)
			.NotEmpty();

		RuleForEach(command =>
			command.GrantedScopes)
			.NotEmpty();

		RuleFor(command =>
			command.RedirectUri)
			.NotEmpty();

		RuleFor(command =>
			command.State)
			.NotEmpty();

		RuleFor(command =>
			command.CodeChallenge)
			.NotEmpty();

		RuleFor(command =>
			command.CodeChallengeMethod)
			.NotEmpty();
	}
}
