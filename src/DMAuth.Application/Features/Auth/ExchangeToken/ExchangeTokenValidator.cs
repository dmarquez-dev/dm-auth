using DMAuth.Application.Common.Settings;
using FluentValidation;

namespace DMAuth.Application.Features.Auth.ExchangeToken;

/// <summary>
///		Validates structural constraints on <see cref="ExchangeTokenCommand"/> before the handler runs.
/// </summary>
public sealed class ExchangeTokenValidator
	: AbstractValidator<ExchangeTokenCommand>
{
	/// <summary>
	///		Configures validation rules for each field.
	/// </summary>
	/// <param name="oauthSettings">
	///		OAuth 2.0 configuration, including the required client ID prefix.
	/// </param>
	public ExchangeTokenValidator(OAuthSettings oauthSettings)
	{
		var prefix = oauthSettings.ClientIdPrefix;

		RuleFor(command =>
			command.GrantType)
			.NotEmpty();

		RuleFor(command =>
			command.Code)
			.NotEmpty();

		RuleFor(command =>
			command.ClientId)
			.NotEmpty()
			.Must(id => id.StartsWith(prefix, StringComparison.Ordinal))
			.WithMessage($"client_id must begin with '{prefix}'.");

		RuleFor(command =>
			command.RedirectUri)
			.NotEmpty();

		RuleFor(command =>
			command.CodeVerifier)
			.NotEmpty();
	}
}
