using DMAuth.Application.Common.Results;
using DMAuth.Domain.Interfaces;
using MediatR;

namespace DMAuth.Application.Features.Clients.GetById;

/// <summary>
///		Handles client retrieval by fetching the client record, enforcing owner-only
///		access, and projecting it into a <see cref="GetClientByIdResponse"/>.
/// </summary>
public sealed class GetClientByIdHandler(
	IClientRepository clientRepository)
		: IRequestHandler<GetClientByIdQuery, TypedResult<GetClientByIdResponse>>
{
	/// <inheritdoc />
	public async Task<TypedResult<GetClientByIdResponse>> Handle(
		GetClientByIdQuery request,
		CancellationToken cancellationToken)
	{
		var client = await clientRepository.FindByIdAsync(
			request.ClientId,
			cancellationToken);

		if (client is null)
		{
			return TypedResult<GetClientByIdResponse>.NotFound("Client not found.");
		}

		if (client.OwnerId != request.RequestingUserId)
		{
			return TypedResult<GetClientByIdResponse>.Forbidden("You do not have access to this client.");
		}

		return TypedResult<GetClientByIdResponse>.Success(new GetClientByIdResponse(
			client.Id,
			client.ClientId,
			client.ClientName,
			client.ClientType,
			client.OwnerId,
			client.IsActive,
			client.RedirectUris,
			client.AllowedScopes,
			client.CreatedAt,
			client.UpdatedAt));
	}
}
