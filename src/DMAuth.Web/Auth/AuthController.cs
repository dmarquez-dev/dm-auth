using System.IdentityModel.Tokens.Jwt;
using DMAuth.Application.Features.Auth.Authorize;
using DMAuth.Application.Features.Auth.ExchangeToken;
using DMAuth.Application.Features.Auth.GetAuthorizationDetails;
using DMAuth.Application.Features.Auth.GetUserInfo;
using DMAuth.Application.Features.Auth.GrantConsent;
using DMAuth.Application.Features.Auth.RotateToken;
using DMAuth.Application.Features.Auth.RevokeToken;
using DMAuth.Web.Auth.Requests;
using DMAuth.Web.Common;
using DMAuth.Web.Common.CurrentUser;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;

namespace DMAuth.Web.Auth;

/// <summary>
///		Handles OAuth 2.0 and OpenID Connect protocol endpoints.
/// </summary>
[Route("connect")]
public sealed class AuthController(
	IMediator mediator,
	ICurrentUserService currentUserService)
		: ApiControllerBase(mediator)
{
	/// <summary>
	///		Validates an OAuth 2.0 authorization request and orchestrates the redirect flow:
	///		unauthenticated users are sent to the login page, authenticated users with insufficient
	///		consent are sent to the consent page.
	/// </summary>
	/// <param name="request">
	///		The OAuth 2.0 authorization request parameters.
	/// </param>
	/// <param name="cancellationToken">
	///		A token to cancel the operation.
	/// </param>
	[HttpGet("authorize")]
	[ProducesResponseType(StatusCodes.Status302Found)]
	[ProducesResponseType(StatusCodes.Status400BadRequest)]
	[ProducesResponseType(StatusCodes.Status404NotFound)]
	public async Task<IActionResult> AuthorizeAsync(
		[FromQuery] AuthorizeRequest request,
		CancellationToken cancellationToken)
	{
		var command = new AuthorizeCommand(
			request.ClientId,
			request.RedirectUri,
			request.ResponseType,
			request.Scope,
			request.State,
			request.CodeChallenge,
			request.CodeChallengeMethod,
			request.Nonce);

		var validationResult = await Mediator.Send(command, cancellationToken);

		if (!validationResult.IsSuccess)
		{
			return MapError(validationResult);
		}

		var validated = validationResult.Value;

		if (!currentUserService.IsAuthenticated)
		{
			var returnUrl = QueryString.Create("returnUrl", Request.GetDisplayUrl());
			return Redirect($"/login{returnUrl}");
		}

		var detailsQuery = new GetAuthorizationDetailsQuery(
			currentUserService.UserId,
			validated.OAuthClientId,
			validated.RequestedScopes);

		var detailsResult = await Mediator.Send(detailsQuery, cancellationToken);

		if (!detailsResult.IsSuccess)
		{
			return MapError(detailsResult);
		}

		if (!detailsResult.Value.IsConsentRequired)
		{
			return await GrantConsentAsync(
				new GrantConsentCommand(
					currentUserService.UserId,
					validated.OAuthClientId,
					validated.RequestedScopes,
					validated.RedirectUri,
					validated.State,
					validated.CodeChallenge,
					validated.CodeChallengeMethod,
					validated.Nonce),
				cancellationToken);
		}

		return Redirect(BuildConsentUrl(validated));
	}

	/// <summary>
	///		Records the user's consent selection and issues an authorization code,
	///		redirecting to the client's redirect URI.
	/// </summary>
	/// <param name="request">
	///		The consent form fields submitted from the consent page.
	/// </param>
	/// <param name="cancellationToken">
	///		A token to cancel the operation.
	/// </param>
	[Authorize]
	[HttpPost("consent")]
	[ProducesResponseType(StatusCodes.Status302Found)]
	[ProducesResponseType(StatusCodes.Status400BadRequest)]
	[ProducesResponseType(StatusCodes.Status403Forbidden)]
	[ProducesResponseType(StatusCodes.Status404NotFound)]
	public async Task<IActionResult> GrantConsentAsync(
		[FromForm] GrantConsentRequest request,
		CancellationToken cancellationToken)
	{
		return await GrantConsentAsync(
			new GrantConsentCommand(
				currentUserService.UserId,
				request.OAuthClientId,
				request.GrantedScopes,
				request.RedirectUri,
				request.State,
				request.CodeChallenge,
				request.CodeChallengeMethod,
				request.Nonce),
			cancellationToken);
	}

	private async Task<IActionResult> GrantConsentAsync(
		GrantConsentCommand command,
		CancellationToken cancellationToken)
	{
		var result = await Mediator.Send(command, cancellationToken);

		if (!result.IsSuccess)
		{
			return MapError(result);
		}

		return Redirect(BuildCodeRedirectUrl(
			command.RedirectUri,
			result.Value.PlainCode,
			command.State));
	}

	/// <summary>
	///		Issues tokens by dispatching to the appropriate handler based on <c>grant_type</c>.
	///		Supports <c>authorization_code</c> (code exchange) and <c>refresh_token</c> (rotation).
	/// </summary>
	/// <param name="request">
	///		The token request form fields.
	/// </param>
	/// <param name="cancellationToken">
	///		A token to cancel the operation.
	/// </param>
	[HttpPost("token")]
	[Consumes("application/x-www-form-urlencoded")]
	[ProducesResponseType(StatusCodes.Status200OK)]
	[ProducesResponseType(StatusCodes.Status400BadRequest)]
	[ProducesResponseType(StatusCodes.Status401Unauthorized)]
	[ProducesResponseType(StatusCodes.Status404NotFound)]
	public async Task<IActionResult> TokenAsync(
		[FromForm] ExchangeTokenRequest request,
		CancellationToken cancellationToken)
	{
		if (request.GrantType == "refresh_token")
		{
			var command = new RotateTokenCommand(
				request.ClientId,
				request.RefreshToken ?? string.Empty);

			var result = await Mediator.Send(command, cancellationToken);

			if (!result.IsSuccess)
			{
				return MapError(result);
			}

			return Ok(result.Value);
		}

		var codeCommand = new ExchangeTokenCommand(
			request.GrantType,
			request.Code,
			request.ClientId,
			request.RedirectUri,
			request.CodeVerifier);

		var codeResult = await Mediator.Send(codeCommand, cancellationToken);

		if (!codeResult.IsSuccess)
		{
			return MapError(codeResult);
		}

		return Ok(codeResult.Value);
	}

	/// <summary>
	///		Revokes a refresh token per RFC 7009.
	///		Always returns 200 OK after structural validation, whether the token existed or not.
	/// </summary>
	/// <param name="request">
	///		The revocation request form fields.
	/// </param>
	/// <param name="cancellationToken">
	///		A token to cancel the operation.
	/// </param>
	[HttpPost("revoke")]
	[Consumes("application/x-www-form-urlencoded")]
	[ProducesResponseType(StatusCodes.Status200OK)]
	[ProducesResponseType(StatusCodes.Status400BadRequest)]
	public async Task<IActionResult> RevokeTokenAsync(
		[FromForm] RevokeTokenRequest request,
		CancellationToken cancellationToken)
	{
		var command = new RevokeTokenCommand(request.Token);

		var result = await Mediator.Send(command, cancellationToken);

		if (!result.IsSuccess)
		{
			return MapError(result);
		}

		return Ok();
	}

	/// <summary>
	///		Returns OIDC UserInfo claims for the authenticated user.
	///		Requires a valid Bearer access token; claims returned depend on the token's granted scopes.
	/// </summary>
	/// <param name="cancellationToken">
	///		A token to cancel the operation.
	/// </param>
	[HttpGet("userinfo")]
	[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
	[ProducesResponseType<UserInfoResponse>(StatusCodes.Status200OK)]
	[ProducesResponseType(StatusCodes.Status401Unauthorized)]
	[ProducesResponseType(StatusCodes.Status404NotFound)]
	public async Task<IActionResult> UserInfoAsync(CancellationToken cancellationToken)
	{
		var sub = User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;

		if (!Guid.TryParse(sub, out var userId))
		{
			return Unauthorized(new { message = "Invalid or missing sub claim." });
		}

		var scope = User.FindFirst("scope")?.Value ?? string.Empty;
		var query = new GetUserInfoQuery(userId, scope);
		var result = await Mediator.Send(query, cancellationToken);

		if (!result.IsSuccess)
		{
			return MapError(result);
		}

		return Ok(result.Value);
	}

	private static string BuildConsentUrl(AuthorizeResponse validated)
	{
		var pairs = new List<KeyValuePair<string, StringValues>>
		{
			new("client_id", validated.OAuthClientId),
			new("redirect_uri", validated.RedirectUri),
			new("scope", string.Join(" ", validated.RequestedScopes)),
			new("state", validated.State),
			new("code_challenge", validated.CodeChallenge),
			new("code_challenge_method", validated.CodeChallengeMethod),
		};

		if (!string.IsNullOrEmpty(validated.Nonce))
		{
			pairs.Add(new KeyValuePair<string, StringValues>("nonce", validated.Nonce));
		}

		return $"/consent{QueryString.Create(pairs)}";
	}

	private static string BuildCodeRedirectUrl(
		string redirectUri,
		string code,
		string state)
	{
		var query = QueryString.Create(
		[
			new KeyValuePair<string, StringValues>("code", code),
			new KeyValuePair<string, StringValues>("state", state),
		]);

		return $"{redirectUri}{query}";
	}
}
