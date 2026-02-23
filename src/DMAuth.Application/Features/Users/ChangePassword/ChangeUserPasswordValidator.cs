using FluentValidation;

namespace DMAuth.Application.Features.Users.ChangePassword;

/// <summary>
///		Validates structural constraints on <see cref="ChangeUserPasswordCommand"/> before the handler runs.
/// </summary>
public sealed class ChangeUserPasswordValidator
	: AbstractValidator<ChangeUserPasswordCommand>
{
	/// <summary>
	///		Configures validation rules for each field.
	/// </summary>
	public ChangeUserPasswordValidator()
	{
		RuleFor(command =>
			command.CurrentPassword)
			.NotEmpty();

		RuleFor(command =>
			command.NewPassword)
			.NotEmpty()
			.MinimumLength(8);
	}
}
