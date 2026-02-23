using FluentValidation;

namespace DMAuth.Application.Features.Users.Login;

/// <summary>
///		Validates structural constraints on <see cref="LoginUserCommand"/> before the handler runs.
/// </summary>
public sealed class LoginUserValidator
	: AbstractValidator<LoginUserCommand>
{
	/// <summary>
	///		Configures validation rules for each field.
	/// </summary>
	public LoginUserValidator()
	{
		RuleFor(command =>
			command.Email)
			.NotEmpty()
			.EmailAddress();

		RuleFor(command =>
			command.Password)
			.NotEmpty();
	}
}
