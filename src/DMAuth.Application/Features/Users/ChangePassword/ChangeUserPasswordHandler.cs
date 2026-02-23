using DMAuth.Application.Common.Interfaces;
using DMAuth.Application.Common.Results;
using DMAuth.Domain.Interfaces;
using DMAuth.Domain.Policies;
using MediatR;

namespace DMAuth.Application.Features.Users.ChangePassword;

/// <summary>
///		Handles password changes by verifying the current password, validating
///		the new password against policy, then hashing and persisting the update.
/// </summary>
public sealed class ChangeUserPasswordHandler(
	IUserRepository userRepository,
	IPasswordHasher passwordHasher,
	IUnitOfWork unitOfWork)
		: IRequestHandler<ChangeUserPasswordCommand, Result>
{
	/// <inheritdoc />
	public async Task<Result> Handle(
		ChangeUserPasswordCommand request,
		CancellationToken cancellationToken)
	{
		var user = await userRepository.FindByIdAsync(
			request.UserId,
			cancellationToken);

		if (user is null)
		{
			return Result.NotFound("User not found.");
		}

		if (!passwordHasher.Verify(request.CurrentPassword, user.HashedPassword))
		{
			return Result.Unauthorized("Current password is incorrect.");
		}

		var policyResult = PasswordPolicy.Validate(request.NewPassword);
		if (!policyResult.IsCompliant)
		{
			return Result.Invalid(policyResult.ViolationSummary);
		}

		user.ChangePassword(passwordHasher.Hash(request.NewPassword));

		await unitOfWork.SaveChangesAsync(cancellationToken);

		return Result.Success();
	}
}
