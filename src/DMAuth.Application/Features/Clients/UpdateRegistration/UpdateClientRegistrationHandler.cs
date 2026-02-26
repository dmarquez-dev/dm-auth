using DMAuth.Application.Common.Interfaces;
using DMAuth.Application.Common.Results;
using DMAuth.Domain.Interfaces;
using DMAuth.Domain.Policies;
using MediatR;

namespace DMAuth.Application.Features.Clients.UpdateRegistration;

/// <summary>
///		Handles client registration updates by enforcing owner-only access, validating
///		the new URIs and scopes, and persisting the changes.
/// </summary>
public sealed class UpdateClientRegistrationHandler(
	IClientRepository clientRepository,
	IUnitOfWork unitOfWork)
		: IRequestHandler<UpdateClientRegistrationCommand, Result>
{
	/// <inheritdoc />
	public async Task<Result> Handle(
		UpdateClientRegistrationCommand request,
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

		foreach (var uri in request.RedirectUris)
		{
			var uriResult = RedirectUriPolicy.Validate(uri);
			if (!uriResult.IsCompliant)
			{
				return Result.Invalid(uriResult.ViolationSummary);
			}
		}

		foreach (var scope in request.AllowedScopes)
		{
			var scopeResult = ScopePolicy.Validate(scope);
			if (!scopeResult.IsCompliant)
			{
				return Result.Invalid(scopeResult.ViolationSummary);
			}
		}

		client.UpdateRegistration(
			request.ClientName,
			request.RedirectUris,
			request.AllowedScopes);

		await unitOfWork.SaveChangesAsync(cancellationToken);

		return Result.Success();
	}
}
