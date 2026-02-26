using FluentValidation;

namespace DMAuth.Application.Features.Clients.GetByOwner;

/// <summary>
///		Validates structural constraints on <see cref="GetClientsByOwnerQuery"/> before the handler runs.
/// </summary>
public sealed class GetClientsByOwnerValidator
	: AbstractValidator<GetClientsByOwnerQuery>
{
	/// <summary>
	///		Configures validation rules for each field.
	/// </summary>
	public GetClientsByOwnerValidator()
	{
		RuleFor(query =>
			query.OwnerId)
			.NotEmpty();
	}
}
