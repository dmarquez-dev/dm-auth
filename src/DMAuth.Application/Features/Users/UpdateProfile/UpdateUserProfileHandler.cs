using DMAuth.Application.Common.Interfaces;
using DMAuth.Application.Common.Results;
using DMAuth.Domain.Interfaces;
using MediatR;

namespace DMAuth.Application.Features.Users.UpdateProfile;

/// <summary>
///		Handles profile updates by applying the new display name to the user
///		aggregate and persisting the change.
/// </summary>
public sealed class UpdateUserProfileHandler(
	IUserRepository userRepository,
	IUnitOfWork unitOfWork)
		: IRequestHandler<UpdateUserProfileCommand, Result>
{
	/// <inheritdoc />
	public async Task<Result> Handle(
		UpdateUserProfileCommand request,
		CancellationToken cancellationToken)
	{
		var user = await userRepository.FindByIdAsync(
			request.UserId,
			cancellationToken);

		if (user is null)
		{
			return Result.NotFound("User not found.");
		}

		user.UpdateProfile(request.DisplayName);

		await unitOfWork.SaveChangesAsync(cancellationToken);

		return Result.Success();
	}
}
