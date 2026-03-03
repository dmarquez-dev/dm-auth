using System.Buffers.Text;
using System.Security.Cryptography;
using System.Text;
using DMAuth.Application.Common.Interfaces;
using DMAuth.Application.Common.Results;
using DMAuth.Domain.Interfaces;
using MediatR;

namespace DMAuth.Application.Features.Auth.RevokeToken;

/// <summary>
///		Handles refresh token revocation. Finds the token by its hash and revokes it if active.
///		Always returns success regardless of whether the token existed, per RFC 7009.
/// </summary>
public sealed class RevokeTokenHandler(
	IRefreshTokenRepository refreshTokenRepository,
	IUnitOfWork unitOfWork)
		: IRequestHandler<RevokeTokenCommand, TypedResult<bool>>
{
	/// <inheritdoc />
	public async Task<TypedResult<bool>> Handle(
		RevokeTokenCommand request,
		CancellationToken cancellationToken)
	{
		var tokenHash = Base64Url.EncodeToString(
			SHA256.HashData(Encoding.UTF8.GetBytes(request.Token)));

		var token = await refreshTokenRepository.FindByTokenHashAsync(
			tokenHash,
			cancellationToken);

		if (token is not null && !token.RevokedAt.HasValue)
		{
			token.Revoke();
			await unitOfWork.SaveChangesAsync(cancellationToken);
		}

		return TypedResult<bool>.Success(true);
	}
}
