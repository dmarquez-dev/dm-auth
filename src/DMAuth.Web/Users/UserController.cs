using DMAuth.Application.Features.Users.RegisterUser;
using DMAuth.Web.Common;
using DMAuth.Web.Users.Requests;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace DMAuth.Web.Users;

/// <summary>
///		Handles HTTP requests for user account operations.
/// </summary>
[Route("api/users")]
public sealed class UserController(
	IMediator mediator)
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
				$"/api/users/{result.Value!.UserId}",
				result.Value)
			: MapError(result);
	}
}
