using FluentValidation;

namespace DMAuth.Application.Features.Auth.RevokeToken;

/// <summary>
///		Validates structural constraints on <see cref="RevokeTokenCommand"/> before the handler runs.
/// </summary>
public sealed class RevokeTokenValidator
	: AbstractValidator<RevokeTokenCommand>
{
	/// <summary>
	///		Configures validation rules for each field.
	/// </summary>
	public RevokeTokenValidator()
	{
		RuleFor(command =>
			command.Token)
			.NotEmpty();
	}
}
