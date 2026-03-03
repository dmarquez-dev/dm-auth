using DMAuth.Application.Common.Results;
using DMAuth.Domain.Interfaces;
using MediatR;

namespace DMAuth.Application.Features.Auth.GetAuthorizationDetails;

/// <summary>
///		Handles consent checking by resolving the client from its public identifier and comparing
///		the user's granted scopes against the requested scopes.
/// </summary>
public sealed class GetAuthorizationDetailsHandler(
	IClientRepository clientRepository,
	IConsentRepository consentRepository)
		: IRequestHandler<GetAuthorizationDetailsQuery, TypedResult<GetAuthorizationDetailsResponse>>
{
	/// <inheritdoc />
	public async Task<TypedResult<GetAuthorizationDetailsResponse>> Handle(
		GetAuthorizationDetailsQuery request,
		CancellationToken cancellationToken)
	{
		var client = await clientRepository.FindByClientIdAsync(
			request.OAuthClientId,
			cancellationToken);

		if (client is null)
		{
			return TypedResult<GetAuthorizationDetailsResponse>.NotFound(
				$"No client with client_id '{request.OAuthClientId}' was found.");
		}

		var consent = await consentRepository.FindByUserAndClientAsync(
			request.UserId,
			client.Id,
			cancellationToken);

		if (consent is null)
		{
			return TypedResult<GetAuthorizationDetailsResponse>.Success(
				new GetAuthorizationDetailsResponse(IsConsentRequired: true));
		}

		var grantedScopes = consent.GrantedScopes
			.Split(' ', StringSplitOptions.RemoveEmptyEntries)
			.ToHashSet(StringComparer.OrdinalIgnoreCase);

		var isConsentRequired = !request.RequestedScopes
			.All(scope =>
				grantedScopes.Contains(scope));

		return TypedResult<GetAuthorizationDetailsResponse>.Success(
			new GetAuthorizationDetailsResponse(isConsentRequired));
	}
}
