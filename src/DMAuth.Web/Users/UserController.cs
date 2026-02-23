using DMAuth.Application.Features.Users.ChangePassword;
using DMAuth.Application.Features.Users.GetProfile;
using DMAuth.Application.Features.Users.Login;
using DMAuth.Application.Features.Users.Register;
using DMAuth.Application.Features.Users.UpdateProfile;
using DMAuth.Web.Common;
using DMAuth.Web.Common.CurrentUser;
using DMAuth.Web.Users.Requests;
using MediatR;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DMAuth.Web.Users;

/// <summary>
///		Handles HTTP requests for user account operations.
/// </summary>
[Route("api/users")]
public sealed class UserController(
	IMediator mediator,
	ICurrentUserService currentUserService)
		: ApiControllerBase(mediator)
{
	/// <summary>
	///		Registers a new user account.
	/// </summary>
	/// <param name="request">
	///		The registration details.
	/// </param>
	/// <param name="cancellationToken">
	///		A token to cancel the operation.
	/// </param>
	[HttpPost("register")]
	[ProducesResponseType<RegisterUserResponse>(StatusCodes.Status201Created)]
	[ProducesResponseType(StatusCodes.Status400BadRequest)]
	[ProducesResponseType(StatusCodes.Status409Conflict)]
	public async Task<IActionResult> RegisterAsync(
		[FromBody] RegisterUserRequest request,
		CancellationToken cancellationToken)
	{
		var command = new RegisterUserCommand(
			request.Email,
			request.Username,
			request.Password,
			request.DisplayName);

		var result = await Mediator.Send(
			command,
			cancellationToken);

		return result.IsSuccess
			? Created(
				$"/api/users/{result.Value.UserId}",
				result.Value)
			: MapError(result);
	}

	/// <summary>
	///		Authenticates a user and establishes a session cookie.
	/// </summary>
	/// <param name="request">
	///		The login credentials.
	/// </param>
	/// <param name="cancellationToken">
	///		A token to cancel the operation.
	/// </param>
	[HttpPost("login")]
	[ProducesResponseType<LoginUserResponse>(StatusCodes.Status200OK)]
	[ProducesResponseType(StatusCodes.Status400BadRequest)]
	[ProducesResponseType(StatusCodes.Status401Unauthorized)]
	public async Task<IActionResult> LoginAsync(
		[FromBody] LoginRequest request,
		CancellationToken cancellationToken)
	{
		var command = new LoginUserCommand(
			request.Email,
			request.Password);

		var result = await Mediator.Send(
			command,
			cancellationToken);

		if (!result.IsSuccess)
		{
			return MapError(result);
		}

		await HttpContext.SignInAsync(
			CookieAuthenticationDefaults.AuthenticationScheme,
			ClaimsPrincipalFactory.FromLoginResponse(result.Value));

		return Ok(result.Value);
	}

	/// <summary>
	///		Terminates the current user session.
	/// </summary>
	[Authorize]
	[HttpPost("logout")]
	[ProducesResponseType(StatusCodes.Status204NoContent)]
	public async Task<IActionResult> LogoutAsync()
	{
		await HttpContext.SignOutAsync(
			CookieAuthenticationDefaults.AuthenticationScheme);

		return NoContent();
	}

	/// <summary>
	///		Returns the authenticated user's profile.
	/// </summary>
	/// <param name="cancellationToken">
	///		A token to cancel the operation.
	/// </param>
	[Authorize]
	[HttpGet("me")]
	[ProducesResponseType<GetUserProfileResponse>(StatusCodes.Status200OK)]
	[ProducesResponseType(StatusCodes.Status404NotFound)]
	public async Task<IActionResult> GetProfileAsync(CancellationToken cancellationToken)
	{
		var query = new GetUserProfileQuery(currentUserService.UserId);

		var result = await Mediator.Send(query, cancellationToken);

		return result.IsSuccess
			? Ok(result.Value)
			: MapError(result);
	}

	/// <summary>
	///		Updates the authenticated user's display name.
	/// </summary>
	/// <param name="request">
	///		The updated profile details.
	/// </param>
	/// <param name="cancellationToken">
	///		A token to cancel the operation.
	/// </param>
	[Authorize]
	[HttpPut("me")]
	[ProducesResponseType(StatusCodes.Status204NoContent)]
	[ProducesResponseType(StatusCodes.Status400BadRequest)]
	[ProducesResponseType(StatusCodes.Status404NotFound)]
	public async Task<IActionResult> UpdateProfileAsync(
		[FromBody] UpdateUserProfileRequest request,
		CancellationToken cancellationToken)
	{
		var command = new UpdateUserProfileCommand(
			currentUserService.UserId,
			request.DisplayName);

		var result = await Mediator.Send(command, cancellationToken);

		return result.IsSuccess
			? NoContent()
			: MapError(result);
	}

	/// <summary>
	///		Changes the authenticated user's password.
	/// </summary>
	/// <param name="request">
	///		The current and new password.
	/// </param>
	/// <param name="cancellationToken">
	///		A token to cancel the operation.
	/// </param>
	[Authorize]
	[HttpPost("me/change-password")]
	[ProducesResponseType(StatusCodes.Status204NoContent)]
	[ProducesResponseType(StatusCodes.Status400BadRequest)]
	[ProducesResponseType(StatusCodes.Status401Unauthorized)]
	[ProducesResponseType(StatusCodes.Status404NotFound)]
	public async Task<IActionResult> ChangePasswordAsync(
		[FromBody] ChangeUserPasswordRequest request,
		CancellationToken cancellationToken)
	{
		var command = new ChangeUserPasswordCommand(
			currentUserService.UserId,
			request.CurrentPassword,
			request.NewPassword);

		var result = await Mediator.Send(command, cancellationToken);

		return result.IsSuccess
			? NoContent()
			: MapError(result);
	}
}
