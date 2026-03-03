using System.Buffers.Text;
using System.Security.Cryptography;
using System.Text;
using DMAuth.Application.Common.Interfaces;
using DMAuth.Application.Common.Results;
using DMAuth.Application.Features.Auth.ExchangeToken;
using DMAuth.Domain.Entities.RefreshToken;
using DMAuth.Domain.Interfaces;
using MediatR;

namespace DMAuth.Application.Features.Auth.RotateToken;

/// <summary>
///		Handles refresh token rotation: validates the presented token, detects reuse,
///		revokes the old token, and issues a new access token and refresh token.
/// </summary>
public sealed class RotateTokenHandler(
	IClientRepository clientRepository,
	IRefreshTokenRepository refreshTokenRepository,
	ITokenService tokenService,
	IUnitOfWork unitOfWork)
		: IRequestHandler<RotateTokenCommand, TypedResult<ExchangeTokenResponse>>
{
	/// <inheritdoc />
	public async Task<TypedResult<ExchangeTokenResponse>> Handle(
		RotateTokenCommand request,
		CancellationToken cancellationToken)
	{
		var client = await clientRepository.FindByClientIdAsync(
			request.ClientId,
			cancellationToken);

		if (client is null)
		{
			return TypedResult<ExchangeTokenResponse>.NotFound(
				$"No client with client_id '{request.ClientId}' was found.");
		}

		if (!client.IsActive)
		{
			return TypedResult<ExchangeTokenResponse>.Forbidden(
				"This client is inactive and cannot participate in token exchange.");
		}

		var tokenHash = Base64Url.EncodeToString(
			SHA256.HashData(Encoding.UTF8.GetBytes(request.Token)));

		var token = await refreshTokenRepository.FindByTokenHashAsync(
			tokenHash,
			cancellationToken);

		if (token is null)
		{
			return TypedResult<ExchangeTokenResponse>.Unauthorized("invalid_grant");
		}

		if (token.RevokedAt.HasValue)
		{
			await refreshTokenRepository.RevokeByTokenFamilyAsync(
				token.FamilyId,
				cancellationToken);

			await unitOfWork.SaveChangesAsync(cancellationToken);

			return TypedResult<ExchangeTokenResponse>.Unauthorized(
				"Refresh token has already been used.");
		}

		if (token.ClientId != client.Id)
		{
			return TypedResult<ExchangeTokenResponse>.Unauthorized("invalid_grant");
		}

		if (token.ExpiresAt <= DateTimeOffset.UtcNow)
		{
			return TypedResult<ExchangeTokenResponse>.Unauthorized(
				"Refresh token has expired.");
		}

		var (plainToken, newTokenHash) = tokenService.GenerateRefreshToken();

		token.Rotate(newTokenHash);

		refreshTokenRepository.Add(new RefreshToken(
			newTokenHash,
			token.UserId,
			client.Id,
			DateTimeOffset.UtcNow.AddDays(30),
			token.Scopes,
			token.FamilyId));

		var accessToken = tokenService.GenerateAccessToken(
			token.UserId,
			client.ClientId,
			token.Scopes);

		await unitOfWork.SaveChangesAsync(cancellationToken);

		return TypedResult<ExchangeTokenResponse>.Success(
			new ExchangeTokenResponse(
				accessToken,
				null,
				plainToken,
				"Bearer",
				tokenService.AccessTokenExpirySeconds,
				token.Scopes));
	}
}
