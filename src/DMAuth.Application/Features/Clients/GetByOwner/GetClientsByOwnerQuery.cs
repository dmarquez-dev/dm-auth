using DMAuth.Application.Common.Results;
using MediatR;

namespace DMAuth.Application.Features.Clients.GetByOwner;

/// <summary>
///		Query to retrieve all client registrations belonging to a given owner.
/// </summary>
/// <param name="OwnerId">
///		The identifier of the user whose clients to retrieve.
/// </param>
public record GetClientsByOwnerQuery(
	Guid OwnerId)
		: IRequest<TypedResult<List<GetClientsByOwnerResponse>>>;
