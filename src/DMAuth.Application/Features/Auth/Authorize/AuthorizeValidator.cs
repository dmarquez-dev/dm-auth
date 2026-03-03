using DMAuth.Application.Common.Settings;
using FluentValidation;

namespace DMAuth.Application.Features.Auth.Authorize;

/// <summary>
///		Validates structural constraints on <see cref="AuthorizeCommand"/> before the handler runs.
/// </summary>
public sealed class AuthorizeValidator
	: AbstractValidator<AuthorizeCommand>
{
	/// <summary>
	///		Configures validation rules for each field.
	/// </summary>
	/// <param name="oauthSettings">
	///		OAuth 2.0 configuration, including the required client ID prefix.
	/// </param>
	public AuthorizeValidator(OAuthSettings oauthSettings)
	{
		var prefix = oauthSettings.ClientIdPrefix;

		RuleFor(command =>
			command.ClientId)
			.NotEmpty()
			.Must(id => id.StartsWith(prefix, StringComparison.Ordinal))
			.WithMessage($"client_id must begin with '{prefix}'.");

		RuleFor(command =>
			command.RedirectUri)
			.NotEmpty();

		RuleFor(command =>
			command.ResponseType)
			.NotEmpty();

		RuleFor(command =>
			command.Scope)
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
