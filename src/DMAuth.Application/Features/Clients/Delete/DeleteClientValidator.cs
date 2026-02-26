using FluentValidation;

namespace DMAuth.Application.Features.Clients.Delete;

/// <summary>
///		Validates structural constraints on <see cref="DeleteClientCommand"/> before the handler runs.
/// </summary>
public sealed class DeleteClientValidator
	: AbstractValidator<DeleteClientCommand>
{
	/// <summary>
	///		Configures validation rules for each field.
	/// </summary>
	public DeleteClientValidator()
	{
		RuleFor(command =>
			command.ClientId)
			.NotEmpty();

		RuleFor(command =>
			command.RequestingUserId)
			.NotEmpty();
	}
}
