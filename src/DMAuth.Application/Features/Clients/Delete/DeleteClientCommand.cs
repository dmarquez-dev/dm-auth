using DMAuth.Application.Common.Results;
using MediatR;

namespace DMAuth.Application.Features.Clients.Delete;

/// <summary>
///		Command to deactivate a client registration, preventing it from initiating further OAuth 2.0 flows.
/// </summary>
/// <param name="ClientId">
///		The identifier of the client to deactivate.
/// </param>
/// <param name="RequestingUserId">
///		The identifier of the user making the request, used to enforce owner-only access.
/// </param>
public record DeleteClientCommand(
	Guid ClientId,
	Guid RequestingUserId)
		: IRequest<Result>;
