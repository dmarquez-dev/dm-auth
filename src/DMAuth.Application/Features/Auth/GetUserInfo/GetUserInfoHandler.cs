using DMAuth.Application.Common.Results;
using DMAuth.Domain.Interfaces;
using MediatR;

namespace DMAuth.Application.Features.Auth.GetUserInfo;

/// <summary>
///		Returns OIDC UserInfo claims for the authenticated user,
///		scoped to the permissions granted in the access token.
/// </summary>
public sealed class GetUserInfoHandler(
	IUserRepository userRepository)
		: IRequestHandler<GetUserInfoQuery, TypedResult<UserInfoResponse>>
{
	/// <inheritdoc />
	public async Task<TypedResult<UserInfoResponse>> Handle(
		GetUserInfoQuery request,
		CancellationToken cancellationToken)
	{
		var user = await userRepository.FindByIdAsync(
			request.UserId,
			cancellationToken);

		if (user is null)
		{
			return TypedResult<UserInfoResponse>.NotFound(
				$"No user with ID '{request.UserId}' was found.");
		}

		var scopes = request.Scope.Split(
			' ',
			StringSplitOptions.RemoveEmptyEntries);

		var hasProfile = scopes.Contains("profile");
		var hasEmail   = scopes.Contains("email");

		return TypedResult<UserInfoResponse>.Success(new UserInfoResponse
		{
			Sub  = user.Id.ToString(),
			Name = hasProfile ? user.DisplayName : null,
			PreferredUsername = hasProfile ? user.Username    : null,
			Email = hasEmail   ? user.Email.Value : null,
			EmailVerified = hasEmail   ? user.EmailVerified : null,
		});
	}
}
