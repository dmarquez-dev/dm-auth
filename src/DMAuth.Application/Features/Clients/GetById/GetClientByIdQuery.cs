using DMAuth.Application.Common.Results;
using MediatR;

namespace DMAuth.Application.Features.Clients.GetById;

/// <summary>
///		Query to retrieve a single client registration by its identifier.
/// </summary>
/// <param name="ClientId">
///		The identifier of the client to retrieve.
/// </param>
/// <param name="RequestingUserId">
///		The identifier of the user making the request, used to enforce owner-only access.
/// </param>
public record GetClientByIdQuery(
	Guid ClientId,
	Guid RequestingUserId)
		: IRequest<TypedResult<GetClientByIdResponse>>;
