using DMAuth.Application.Common.Results;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace DMAuth.Web.Common;

/// <summary>
///		Abstract base controller providing shared MediatR dispatch and result-to-HTTP mapping
///		for all API controllers in the application.
/// </summary>
[ApiController]
public abstract class ApiControllerBase(
	IMediator mediator)
		: ControllerBase
{
	/// <summary>
	///		The MediatR mediator used to dispatch commands and queries.
	/// </summary>
	protected IMediator Mediator { get; } = mediator;

	/// <summary>
	///		Maps a failed <see cref="Result"/> to the appropriate <see cref="IActionResult"/>
	///		based on its <see cref="ResultError"/> category.
	/// </summary>
	/// <param name="result">
	///		The failed result to map. Should only be called when <see cref="Result.IsSuccess"/> is false.
	/// </param>
	protected IActionResult MapError(Result result) =>
		result.ErrorType switch
		{
			ResultError.NotFound => NotFound(new { message = result.Error }),
			ResultError.Conflict => Conflict(new { message = result.Error }),
			ResultError.Unauthorized => Unauthorized(new { message = result.Error }),
			ResultError.Forbidden => StatusCode(
				StatusCodes.Status403Forbidden,
				new { message = result.Error }),
			ResultError.Invalid => BadRequest(new { message = result.Error }),
			_ => StatusCode(
				StatusCodes.Status500InternalServerError,
				new { message = "An unexpected error occurred." })
		};
}
