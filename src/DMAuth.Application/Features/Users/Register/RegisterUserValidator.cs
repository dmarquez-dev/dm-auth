using FluentValidation;

namespace DMAuth.Application.Features.Users.Register;

/// <summary>
///		Validates structural constraints on <see cref="RegisterUserCommand"/> before the handler runs.
/// </summary>
public sealed class RegisterUserValidator
	: AbstractValidator<RegisterUserCommand>
{
	/// <summary>
	///		Configures validation rules for each field.
	/// </summary>
	public RegisterUserValidator()
	{
		RuleFor(command =>
			command.Email)
			.NotEmpty()
			.EmailAddress()
			.MaximumLength(256);

		RuleFor(command =>
			command.Username)
			.NotEmpty()
			.MinimumLength(3)
			.MaximumLength(100);

		RuleFor(command =>
			command.Password)
			.NotEmpty()
			.MinimumLength(8);

		RuleFor(command =>
			command.DisplayName)
			.NotEmpty()
			.MaximumLength(200);
	}
}
