using System.Buffers.Text;
using System.Security.Cryptography;
using System.Text;
using DMAuth.Application.Common.Interfaces;
using DMAuth.Application.Common.Results;
using DMAuth.Domain.Entities.RefreshToken;
using DMAuth.Domain.Interfaces;
using MediatR;

namespace DMAuth.Application.Features.Auth.ExchangeToken;

/// <summary>
///		Handles the OAuth 2.0 authorization code exchange, validating the code and PKCE verifier,
///		then issuing an access token, optional ID token, and optional refresh token.
/// </summary>
public sealed class ExchangeTokenHandler(
	IClientRepository clientRepository,
	IAuthorizationCodeRepository authorizationCodeRepository,
	IRefreshTokenRepository refreshTokenRepository,
	IUserRepository userRepository,
	ITokenService tokenService,
	IUnitOfWork unitOfWork)
		: IRequestHandler<ExchangeTokenCommand, TypedResult<ExchangeTokenResponse>>
{
	/// <inheritdoc />
	public async Task<TypedResult<ExchangeTokenResponse>> Handle(
		ExchangeTokenCommand request,
		CancellationToken cancellationToken)
	{
		if (request.GrantType != "authorization_code")
		{
			return TypedResult<ExchangeTokenResponse>.Invalid(
				"grant_type must be 'authorization_code'.");
		}

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

		var codeHash = Base64Url.EncodeToString(
			SHA256.HashData(Encoding.UTF8.GetBytes(request.Code)));

		var authCode = await authorizationCodeRepository.FindByCodeHashAsync(
			codeHash,
			cancellationToken);

		if (authCode is null)
		{
			return TypedResult<ExchangeTokenResponse>.Invalid("invalid_grant");
		}

		if (authCode.ClientId != client.Id)
		{
			return TypedResult<ExchangeTokenResponse>.Invalid("invalid_grant");
		}

		if (authCode.RedirectUri != request.RedirectUri)
		{
			return TypedResult<ExchangeTokenResponse>.Invalid(
				"redirect_uri does not match the original authorization request.");
		}

		if (authCode.ExpiresAt <= DateTimeOffset.UtcNow)
		{
			return TypedResult<ExchangeTokenResponse>.Invalid(
				"Authorization code has expired.");
		}

		if (authCode.UsedAt.HasValue)
		{
			await refreshTokenRepository.RevokeByTokenFamilyAsync(
				authCode.Id,
				cancellationToken);

			await unitOfWork.SaveChangesAsync(cancellationToken);

			return TypedResult<ExchangeTokenResponse>.Unauthorized(
				"Authorization code has already been used.");
		}

		var verifierHash = Base64Url.EncodeToString(
			SHA256.HashData(Encoding.UTF8.GetBytes(request.CodeVerifier)));

		if (verifierHash != authCode.CodeChallenge.Value)
		{
			return TypedResult<ExchangeTokenResponse>.Invalid(
				"code_verifier does not match code_challenge.");
		}

		authCode.MarkAsUsed();

		var scopes = authCode.Scopes
			.Split(' ', StringSplitOptions.RemoveEmptyEntries)
			.ToHashSet();

		var accessToken = tokenService.GenerateAccessToken(
			authCode.UserId,
			client.ClientId,
			authCode.Scopes);

		string? idToken = null;

		if (scopes.Contains("openid"))
		{
			var user = await userRepository.FindByIdAsync(
				authCode.UserId,
				cancellationToken);

			if (user is null)
			{
				return TypedResult<ExchangeTokenResponse>.NotFound(
					$"No user with ID '{authCode.UserId}' was found.");
			}

			idToken = tokenService.GenerateIdToken(
				authCode.UserId,
				client.ClientId,
				authCode.CreatedAt,
				authCode.Nonce,
				scopes.Contains("profile") ? user.DisplayName : null,
				scopes.Contains("email") ? user.Email.Value : null,
				scopes.Contains("email") ? user.EmailVerified : null);
		}

		string? refreshToken = null;

		if (scopes.Contains("offline_access"))
		{
			var (plainToken, tokenHash) = tokenService.GenerateRefreshToken();

			refreshTokenRepository.Add(new RefreshToken(
				tokenHash,
				authCode.UserId,
				client.Id,
				DateTimeOffset.UtcNow.AddDays(30),
				authCode.Scopes,
				authCode.Id));

			refreshToken = plainToken;
		}

		await unitOfWork.SaveChangesAsync(cancellationToken);

		return TypedResult<ExchangeTokenResponse>.Success(
			new ExchangeTokenResponse(
				accessToken,
				idToken,
				refreshToken,
				"Bearer",
				tokenService.AccessTokenExpirySeconds,
				authCode.Scopes));
	}
}
