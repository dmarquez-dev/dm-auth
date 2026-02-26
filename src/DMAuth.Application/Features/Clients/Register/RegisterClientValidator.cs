using FluentValidation;

namespace DMAuth.Application.Features.Clients.Register;

/// <summary>
///		Validates structural constraints on <see cref="RegisterClientCommand"/> before the handler runs.
/// </summary>
public sealed class RegisterClientValidator
	: AbstractValidator<RegisterClientCommand>
{
	/// <summary>
	///		Configures validation rules for each field.
	/// </summary>
	public RegisterClientValidator()
	{
		RuleFor(command =>
			command.OwnerId)
			.NotEmpty();

		RuleFor(command =>
			command.ClientName)
			.NotEmpty()
			.MaximumLength(200);

		RuleFor(command =>
			command.ClientType)
			.IsInEnum();

		RuleFor(command =>
			command.RedirectUris)
			.NotEmpty();

		RuleForEach(command =>
			command.RedirectUris)
			.NotEmpty();

		RuleFor(command =>
			command.AllowedScopes)
			.NotEmpty();

		RuleForEach(command =>
			command.AllowedScopes)
			.NotEmpty();
	}
}
