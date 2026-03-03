using System.Buffers.Text;
using System.Security.Cryptography;
using System.Text;
using DMAuth.Application.Common.Interfaces;
using DMAuth.Application.Common.Results;
using DMAuth.Domain.Entities.AuthorizationCode;
using DMAuth.Domain.Entities.Consent;
using DMAuth.Domain.Enums;
using DMAuth.Domain.Interfaces;
using DMAuth.Domain.ValueObjects;
using MediatR;

namespace DMAuth.Application.Features.Auth.GrantConsent;

/// <summary>
///		Handles consent grant by recording or updating the user's consent, generating a
///		cryptographically random authorization code, and persisting its hash for later exchange.
/// </summary>
public sealed class GrantConsentHandler(
	IClientRepository clientRepository,
	IConsentRepository consentRepository,
	IAuthorizationCodeRepository authorizationCodeRepository,
	IUnitOfWork unitOfWork)
		: IRequestHandler<GrantConsentCommand, TypedResult<GrantConsentResponse>>
{
	/// <inheritdoc />
	public async Task<TypedResult<GrantConsentResponse>> Handle(
		GrantConsentCommand request,
		CancellationToken cancellationToken)
	{
		var client = await clientRepository.FindByClientIdAsync(
			request.OAuthClientId,
			cancellationToken);

		if (client is null)
		{
			return TypedResult<GrantConsentResponse>.NotFound(
				$"No client with client_id '{request.OAuthClientId}' was found.");
		}

		if (!client.IsActive)
		{
			return TypedResult<GrantConsentResponse>.Forbidden(
				"This client is inactive and cannot initiate authorization flows.");
		}

		var scopeString = string.Join(" ", request.GrantedScopes);

		var existingConsent = await consentRepository.FindByUserAndClientAsync(
			request.UserId,
			client.Id,
			cancellationToken);

		if (existingConsent is null)
		{
			consentRepository.Add(new Consent(request.UserId, client.Id, scopeString));
		}
		else
		{
			var alreadyGranted = existingConsent.GrantedScopes
				.Split(' ', StringSplitOptions.RemoveEmptyEntries)
				.ToHashSet();

			var allScopesCovered = request.GrantedScopes
				.All(scope => alreadyGranted.Contains(scope));

			if (!allScopesCovered)
			{
				existingConsent.UpdateGrantedScopes(scopeString);
				consentRepository.Update(existingConsent);
			}
		}

		var plainCode = Base64Url.EncodeToString(RandomNumberGenerator.GetBytes(32));
		var codeHash = Base64Url.EncodeToString(SHA256.HashData(Encoding.UTF8.GetBytes(plainCode)));

		var authorizationCode = new AuthorizationCode(
			codeHash,
			request.UserId,
			client.Id,
			request.RedirectUri,
			scopeString,
			new CodeChallenge(request.CodeChallenge),
			CodeChallengeMethod.S256,
			DateTimeOffset.UtcNow.AddMinutes(5),
			request.Nonce);

		authorizationCodeRepository.Add(authorizationCode);
		await unitOfWork.SaveChangesAsync(cancellationToken);

		return TypedResult<GrantConsentResponse>.Success(
			new GrantConsentResponse(plainCode));
	}
}
