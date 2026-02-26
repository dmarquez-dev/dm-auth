using DMAuth.Application.Common.Results;
using DMAuth.Domain.Interfaces;
using MediatR;

namespace DMAuth.Application.Features.Clients.GetByOwner;

/// <summary>
///		Handles retrieval of all client registrations belonging to the requesting owner.
/// </summary>
public sealed class GetClientsByOwnerHandler(
	IClientRepository clientRepository)
		: IRequestHandler<GetClientsByOwnerQuery, TypedResult<List<GetClientsByOwnerResponse>>>
{
	/// <inheritdoc />
	public async Task<TypedResult<List<GetClientsByOwnerResponse>>> Handle(
		GetClientsByOwnerQuery request,
		CancellationToken cancellationToken)
	{
		var clients = await clientRepository.FindByOwnerIdAsync(
			request.OwnerId,
			cancellationToken);

		var response = clients
			.Select(client =>
				new GetClientsByOwnerResponse(
					client.Id,
					client.ClientId,
					client.ClientName,
					client.ClientType,
					client.IsActive,
					client.RedirectUris,
					client.AllowedScopes,
					client.CreatedAt,
					client.UpdatedAt))
			.ToList();

		return TypedResult<List<GetClientsByOwnerResponse>>.Success(response);
	}
}
