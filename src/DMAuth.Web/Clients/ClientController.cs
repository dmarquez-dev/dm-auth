using DMAuth.Application.Features.Clients.Delete;
using DMAuth.Application.Features.Clients.GetById;
using DMAuth.Application.Features.Clients.GetByOwner;
using DMAuth.Application.Features.Clients.Register;
using DMAuth.Application.Features.Clients.UpdateRegistration;
using DMAuth.Web.Clients.Requests;
using DMAuth.Web.Common;
using DMAuth.Web.Common.CurrentUser;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DMAuth.Web.Clients;

/// <summary>
///		Handles HTTP requests for OAuth 2.0 client application management.
/// </summary>
[Authorize]
[Route("api/clients")]
public sealed class ClientController(
	IMediator mediator,
	ICurrentUserService currentUserService)
		: ApiControllerBase(mediator)
{
	/// <summary>
	///		Registers a new OAuth 2.0 client application for the authenticated user.
	/// </summary>
	/// <param name="request">
	///		The client registration details.
	/// </param>
	/// <param name="cancellationToken">
	///		A token to cancel the operation.
	/// </param>
	[HttpPost]
	[ProducesResponseType<RegisterClientResponse>(StatusCodes.Status201Created)]
	[ProducesResponseType(StatusCodes.Status400BadRequest)]
	[ProducesResponseType(StatusCodes.Status401Unauthorized)]
	public async Task<IActionResult> RegisterAsync(
		[FromBody] RegisterClientRequest request,
		CancellationToken cancellationToken)
	{
		var command = new RegisterClientCommand(
			currentUserService.UserId,
			request.ClientName,
			request.ClientType,
			request.RedirectUris,
			request.AllowedScopes);

		var result = await Mediator.Send(command, cancellationToken);

		return result.IsSuccess
			? Created(
				$"/api/clients/{result.Value.ClientId}",
				result.Value)
			: MapError(result);
	}

	/// <summary>
	///		Returns all client registrations belonging to the authenticated user.
	/// </summary>
	/// <param name="cancellationToken">
	///		A token to cancel the operation.
	/// </param>
	[HttpGet]
	[ProducesResponseType<List<GetClientsByOwnerResponse>>(StatusCodes.Status200OK)]
	[ProducesResponseType(StatusCodes.Status401Unauthorized)]
	public async Task<IActionResult> GetAllAsync(CancellationToken cancellationToken)
	{
		var query = new GetClientsByOwnerQuery(currentUserService.UserId);

		var result = await Mediator.Send(query, cancellationToken);

		return result.IsSuccess
			? Ok(result.Value)
			: MapError(result);
	}

	/// <summary>
	///		Returns a single client registration by its identifier.
	/// </summary>
	/// <param name="id">
	///		The identifier of the client to retrieve.
	/// </param>
	/// <param name="cancellationToken">
	///		A token to cancel the operation.
	/// </param>
	[HttpGet("{id:guid}")]
	[ProducesResponseType<GetClientByIdResponse>(StatusCodes.Status200OK)]
	[ProducesResponseType(StatusCodes.Status401Unauthorized)]
	[ProducesResponseType(StatusCodes.Status403Forbidden)]
	[ProducesResponseType(StatusCodes.Status404NotFound)]
	public async Task<IActionResult> GetByIdAsync(
		Guid id,
		CancellationToken cancellationToken)
	{
		var query = new GetClientByIdQuery(id, currentUserService.UserId);

		var result = await Mediator.Send(query, cancellationToken);

		return result.IsSuccess
			? Ok(result.Value)
			: MapError(result);
	}

	/// <summary>
	///		Updates the name, redirect URIs, and allowed scopes of an existing client registration.
	/// </summary>
	/// <param name="id">
	///		The identifier of the client to update.
	/// </param>
	/// <param name="request">
	///		The updated registration details.
	/// </param>
	/// <param name="cancellationToken">
	///		A token to cancel the operation.
	/// </param>
	[HttpPut("{id:guid}")]
	[ProducesResponseType(StatusCodes.Status204NoContent)]
	[ProducesResponseType(StatusCodes.Status400BadRequest)]
	[ProducesResponseType(StatusCodes.Status401Unauthorized)]
	[ProducesResponseType(StatusCodes.Status403Forbidden)]
	[ProducesResponseType(StatusCodes.Status404NotFound)]
	public async Task<IActionResult> UpdateRegistrationAsync(
		Guid id,
		[FromBody] UpdateClientRequest request,
		CancellationToken cancellationToken)
	{
		var command = new UpdateClientRegistrationCommand(
			id,
			currentUserService.UserId,
			request.ClientName,
			request.RedirectUris,
			request.AllowedScopes);

		var result = await Mediator.Send(command, cancellationToken);

		return result.IsSuccess
			? NoContent()
			: MapError(result);
	}

	/// <summary>
	///		Deactivates a client registration, preventing it from initiating further OAuth 2.0 flows.
	/// </summary>
	/// <param name="id">
	///		The identifier of the client to deactivate.
	/// </param>
	/// <param name="cancellationToken">
	///		A token to cancel the operation.
	/// </param>
	[HttpDelete("{id:guid}")]
	[ProducesResponseType(StatusCodes.Status204NoContent)]
	[ProducesResponseType(StatusCodes.Status401Unauthorized)]
	[ProducesResponseType(StatusCodes.Status403Forbidden)]
	[ProducesResponseType(StatusCodes.Status404NotFound)]
	public async Task<IActionResult> DeleteAsync(
		Guid id,
		CancellationToken cancellationToken)
	{
		var command = new DeleteClientCommand(id, currentUserService.UserId);

		var result = await Mediator.Send(command, cancellationToken);

		return result.IsSuccess
			? NoContent()
			: MapError(result);
	}
}
