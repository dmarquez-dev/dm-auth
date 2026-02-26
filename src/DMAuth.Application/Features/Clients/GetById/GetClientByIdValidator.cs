using FluentValidation;

namespace DMAuth.Application.Features.Clients.GetById;

/// <summary>
///		Validates structural constraints on <see cref="GetClientByIdQuery"/> before the handler runs.
/// </summary>
public sealed class GetClientByIdValidator
	: AbstractValidator<GetClientByIdQuery>
{
	/// <summary>
	///		Configures validation rules for each field.
	/// </summary>
	public GetClientByIdValidator()
	{
		RuleFor(query =>
			query.ClientId)
			.NotEmpty();

		RuleFor(query =>
			query.RequestingUserId)
			.NotEmpty();
	}
}
