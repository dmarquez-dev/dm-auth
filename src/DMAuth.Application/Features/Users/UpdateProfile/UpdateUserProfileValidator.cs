using FluentValidation;

namespace DMAuth.Application.Features.Users.UpdateProfile;

/// <summary>
///		Validates structural constraints on <see cref="UpdateUserProfileCommand"/> before the handler runs.
/// </summary>
public sealed class UpdateUserProfileValidator
	: AbstractValidator<UpdateUserProfileCommand>
{
	/// <summary>
	///		Configures validation rules for each field.
	/// </summary>
	public UpdateUserProfileValidator()
	{
		RuleFor(command =>
			command.DisplayName)
			.NotEmpty()
			.MaximumLength(200);
	}
}
