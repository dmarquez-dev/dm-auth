using System.Buffers.Text;
using System.Security.Cryptography;
using DMAuth.Application.Common.Interfaces;
using DMAuth.Application.Common.Results;
using DMAuth.Domain.Entities.Client;
using DMAuth.Domain.Enums;
using DMAuth.Domain.Interfaces;
using DMAuth.Domain.Policies;
using MediatR;

namespace DMAuth.Application.Features.Clients.Register;

/// <summary>
///		Handles client registration by generating credentials, validating URIs and scopes,
///		and persisting the new client application.
/// </summary>
public sealed class RegisterClientHandler(
	IClientRepository clientRepository,
	IPasswordHasher passwordHasher,
	IUnitOfWork unitOfWork)
		: IRequestHandler<RegisterClientCommand, TypedResult<RegisterClientResponse>>
{
	/// <inheritdoc />
	public async Task<TypedResult<RegisterClientResponse>> Handle(
		RegisterClientCommand request,
		CancellationToken cancellationToken)
	{
		foreach (var uri in request.RedirectUris)
		{
			var uriResult = RedirectUriPolicy.Validate(uri);
			if (!uriResult.IsCompliant)
			{
				return TypedResult<RegisterClientResponse>.Invalid(uriResult.ViolationSummary);
			}
		}

		foreach (var scope in request.AllowedScopes)
		{
			var scopeResult = ScopePolicy.Validate(scope);
			if (!scopeResult.IsCompliant)
			{
				return TypedResult<RegisterClientResponse>.Invalid(scopeResult.ViolationSummary);
			}
		}

		var oauthClientId = await GenerateUniqueClientIdAsync(cancellationToken);

		string? plainTextSecret = null;
		string? secretHash = null;

		if (request.ClientType is ClientType.Confidential)
		{
			plainTextSecret = Base64Url.EncodeToString(RandomNumberGenerator.GetBytes(32));
			secretHash = passwordHasher.Hash(plainTextSecret).Value;
		}

		var client = new Client(
			oauthClientId,
			request.ClientName,
			request.ClientType,
			request.OwnerId,
			request.RedirectUris,
			request.AllowedScopes,
			secretHash);

		clientRepository.Add(client);
		await unitOfWork.SaveChangesAsync(cancellationToken);

		return TypedResult<RegisterClientResponse>.Success(
			new RegisterClientResponse(
				client.Id,
				oauthClientId,
				plainTextSecret));
	}

	private async Task<string> GenerateUniqueClientIdAsync(CancellationToken cancellationToken)
	{
		string clientId;

		do
		{
			clientId = "dma_" + Base64Url.EncodeToString(RandomNumberGenerator.GetBytes(16));
		}
		while (await clientRepository.ExistsByClientIdAsync(
			clientId,
			cancellationToken));

		return clientId;
	}
}
