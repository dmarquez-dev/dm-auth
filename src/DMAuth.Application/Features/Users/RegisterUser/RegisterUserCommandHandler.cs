using DMAuth.Application.Common.Interfaces;
using DMAuth.Application.Common.Results;
using DMAuth.Domain.Entities.User;
using DMAuth.Domain.Interfaces;
using DMAuth.Domain.Policies;
using DMAuth.Domain.ValueObjects;
using MediatR;

namespace DMAuth.Application.Features.Users.RegisterUser;

/// <summary>
///		Handles user registration by validating uniqueness, hashing the password, and persisting the new account.
/// </summary>
public sealed class RegisterUserCommandHandler(
	IUserRepository userRepository,
	IPasswordHasher passwordHasher,
	IUnitOfWork unitOfWork)
		: IRequestHandler<RegisterUserCommand, TypedResult<RegisterUserResponse>>
{
	/// <inheritdoc />
	public async Task<TypedResult<RegisterUserResponse>> Handle(
		RegisterUserCommand request,
		CancellationToken cancellationToken)
	{
		var policyResult = PasswordPolicy.Validate(request.Password);
		if (!policyResult.IsCompliant)
		{
			return TypedResult<RegisterUserResponse>.Invalid(policyResult.ViolationSummary);
		}

		var email = new Email(request.Email);

		if (await userRepository.ExistsByEmailAsync(
				email,
				cancellationToken))
		{
			return TypedResult<RegisterUserResponse>.Conflict("An account with this email address already exists.");
		}

		if (await userRepository.ExistsByUsernameAsync(
				request.Username,
				cancellationToken))
		{
			return TypedResult<RegisterUserResponse>.Conflict("An account with this username already exists.");
		}

		var user = new User(
			email,
			request.Username,
			passwordHasher.Hash(request.Password),
			request.DisplayName);

		userRepository.Add(user);
		await unitOfWork.SaveChangesAsync(cancellationToken);

		return TypedResult<RegisterUserResponse>.Success(new RegisterUserResponse(user.Id));
	}
}
