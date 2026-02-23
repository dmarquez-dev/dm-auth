using DMAuth.Application.Common.Results;
using DMAuth.Domain.Interfaces;
using MediatR;

namespace DMAuth.Application.Features.Users.GetProfile;

/// <summary>
///		Handles profile retrieval by fetching the user record and projecting
///		it into a <see cref="GetUserProfileResponse"/>.
/// </summary>
public sealed class GetUserProfileHandler(
	IUserRepository userRepository)
		: IRequestHandler<GetUserProfileQuery, TypedResult<GetUserProfileResponse>>
{
	/// <inheritdoc />
	public async Task<TypedResult<GetUserProfileResponse>> Handle(
		GetUserProfileQuery request,
		CancellationToken cancellationToken)
	{
		var user = await userRepository.FindByIdAsync(
			request.UserId,
			cancellationToken);

		if (user is null)
		{
			return TypedResult<GetUserProfileResponse>.NotFound("User not found.");
		}

		return TypedResult<GetUserProfileResponse>.Success(new GetUserProfileResponse(
			user.Id,
			user.Username,
			user.Email.Value,
			user.DisplayName,
			user.EmailVerified));
	}
}
