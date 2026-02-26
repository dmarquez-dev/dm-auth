using DMAuth.Application.Common.Interfaces;
using DMAuth.Application.Common.Results;
using DMAuth.Domain.Interfaces;
using MediatR;

namespace DMAuth.Application.Features.Clients.Delete;

/// <summary>
///		Handles client deletion by enforcing owner-only access and deactivating the client.
/// </summary>
public sealed class DeleteClientHandler(
	IClientRepository clientRepository,
	IUnitOfWork unitOfWork)
		: IRequestHandler<DeleteClientCommand, Result>
{
	/// <inheritdoc />
	public async Task<Result> Handle(
		DeleteClientCommand request,
		CancellationToken cancellationToken)
	{
		var client = await clientRepository.FindByIdAsync(
			request.ClientId,
			cancellationToken);

		if (client is null)
		{
			return Result.NotFound("Client not found.");
		}

		if (client.OwnerId != request.RequestingUserId)
		{
			return Result.Forbidden("You do not have access to this client.");
		}

		client.Deactivate();

		await unitOfWork.SaveChangesAsync(cancellationToken);

		return Result.Success();
	}
}
