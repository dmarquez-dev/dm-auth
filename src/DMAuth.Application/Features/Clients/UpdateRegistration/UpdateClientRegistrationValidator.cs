using FluentValidation;

namespace DMAuth.Application.Features.Clients.UpdateRegistration;

/// <summary>
///		Validates structural constraints on <see cref="UpdateClientRegistrationCommand"/> before the handler runs.
/// </summary>
public sealed class UpdateClientRegistrationValidator
	: AbstractValidator<UpdateClientRegistrationCommand>
{
	/// <summary>
	///		Configures validation rules for each field.
	/// </summary>
	public UpdateClientRegistrationValidator()
	{
		RuleFor(command =>
			command.ClientId)
			.NotEmpty();

		RuleFor(command =>
			command.RequestingUserId)
			.NotEmpty();

		RuleFor(command =>
			command.ClientName)
			.NotEmpty()
			.MaximumLength(200);

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
