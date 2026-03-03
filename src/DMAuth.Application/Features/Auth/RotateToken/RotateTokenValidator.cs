using DMAuth.Application.Common.Settings;
using FluentValidation;

namespace DMAuth.Application.Features.Auth.RotateToken;

/// <summary>
///		Validates structural constraints on <see cref="RotateTokenCommand"/> before the handler runs.
/// </summary>
public sealed class RotateTokenValidator
	: AbstractValidator<RotateTokenCommand>
{
	/// <summary>
	///		Configures validation rules for each field.
	/// </summary>
	/// <param name="oauthSettings">
	///		OAuth 2.0 configuration, including the required client ID prefix.
	/// </param>
	public RotateTokenValidator(OAuthSettings oauthSettings)
	{
		var prefix = oauthSettings.ClientIdPrefix;

		RuleFor(command =>
			command.ClientId)
			.NotEmpty()
			.Must(id => id.StartsWith(prefix, StringComparison.Ordinal))
			.WithMessage($"client_id must begin with '{prefix}'.");

		RuleFor(command =>
			command.Token)
			.NotEmpty();
	}
}
