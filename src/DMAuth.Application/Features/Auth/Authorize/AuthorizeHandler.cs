using DMAuth.Application.Common.Results;
using DMAuth.Domain.Interfaces;
using DMAuth.Domain.Policies;
using MediatR;

namespace DMAuth.Application.Features.Auth.Authorize;

/// <summary>
///		Handles authorization request validation by verifying the client, redirect URI, scopes,
///		and PKCE parameters before the authentication and consent flow proceeds.
/// </summary>
public sealed class AuthorizeHandler(
	IClientRepository clientRepository)
		: IRequestHandler<AuthorizeCommand, TypedResult<AuthorizeResponse>>
{
	/// <inheritdoc />
	public async Task<TypedResult<AuthorizeResponse>> Handle(
		AuthorizeCommand request,
		CancellationToken cancellationToken)
	{
		if (!request.ResponseType.Equals("code", StringComparison.OrdinalIgnoreCase))
		{
			return TypedResult<AuthorizeResponse>.Invalid(
				"response_type must be 'code'.");
		}

		var client = await clientRepository.FindByClientIdAsync(
			request.ClientId,
			cancellationToken);

		if (client is null)
		{
			return TypedResult<AuthorizeResponse>.NotFound(
				$"No client with client_id '{request.ClientId}' was found.");
		}

		if (!client.IsActive)
		{
			return TypedResult<AuthorizeResponse>.Forbidden(
				"This client is inactive and cannot initiate authorization flows.");
		}

		if (!client.RedirectUris.Contains(request.RedirectUri))
		{
			return TypedResult<AuthorizeResponse>.Invalid(
				$"redirect_uri '{request.RedirectUri}' is not registered for this client.");
		}

		var requestedScopes = request.Scope
			.Split(' ', StringSplitOptions.RemoveEmptyEntries);

		var unrecognizedScopes = requestedScopes
			.Where(requestedScope =>
				!client.AllowedScopes.Contains(requestedScope))
			.ToList();

		if (unrecognizedScopes.Count > 0)
		{
			return TypedResult<AuthorizeResponse>.Invalid(
				$"The following scopes are not permitted for this client: {string.Join(", ", unrecognizedScopes)}.");
		}

		if (!request.CodeChallengeMethod.Equals("S256", StringComparison.OrdinalIgnoreCase))
		{
			return TypedResult<AuthorizeResponse>.Invalid(
				"code_challenge_method must be 'S256'.");
		}

		var challengeResult = CodeChallengePolicy.Validate(request.CodeChallenge);

		if (!challengeResult.IsCompliant)
		{
			return TypedResult<AuthorizeResponse>.Invalid(
				challengeResult.ViolationSummary);
		}

		return TypedResult<AuthorizeResponse>.Success(
			new AuthorizeResponse(
				client.ClientId,
				client.ClientName,
				request.RedirectUri,
				requestedScopes,
				request.State,
				request.CodeChallenge,
				request.CodeChallengeMethod,
				request.Nonce));
	}
}
