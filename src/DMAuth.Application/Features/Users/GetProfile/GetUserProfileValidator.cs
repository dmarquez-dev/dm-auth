using FluentValidation;

namespace DMAuth.Application.Features.Users.GetProfile;

/// <summary>
///		Validates structural constraints on <see cref="GetUserProfileQuery"/> before the handler runs.
/// </summary>
public sealed class GetUserProfileValidator
	: AbstractValidator<GetUserProfileQuery>
{
	/// <summary>
	///		Configures validation rules for each field.
	/// </summary>
	public GetUserProfileValidator()
	{
		RuleFor(query =>
			query.UserId)
			.NotEmpty();
	}
}
