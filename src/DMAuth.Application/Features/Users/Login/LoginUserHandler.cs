using DMAuth.Application.Common.Interfaces;
using DMAuth.Application.Common.Results;
using DMAuth.Domain.Interfaces;
using DMAuth.Domain.ValueObjects;
using MediatR;

namespace DMAuth.Application.Features.Users.Login;

/// <summary>
///		Handles user authentication by verifying credentials and returning user details
///		for the caller to establish a session.
/// </summary>
public sealed class LoginUserHandler(
	IUserRepository userRepository,
	IPasswordHasher passwordHasher)
		: IRequestHandler<LoginUserCommand, TypedResult<LoginUserResponse>>
{
	private const string InvalidCredentialsMessage = "Invalid email or password.";

	/// <inheritdoc />
	public async Task<TypedResult<LoginUserResponse>> Handle(
		LoginUserCommand request,
		CancellationToken cancellationToken)
	{
		var user = await userRepository.FindByEmailAsync(
			new Email(request.Email),
			cancellationToken);

		if (user is null || !passwordHasher.Verify(
				request.Password,
				user.HashedPassword))
		{
			return TypedResult<LoginUserResponse>.Unauthorized(InvalidCredentialsMessage);
		}

		if (!user.IsActive)
		{
			return TypedResult<LoginUserResponse>.Unauthorized(InvalidCredentialsMessage);
		}

		return TypedResult<LoginUserResponse>.Success(new LoginUserResponse(
			user.Id,
			user.Username,
			user.Email.Value,
			user.DisplayName));
	}
}
