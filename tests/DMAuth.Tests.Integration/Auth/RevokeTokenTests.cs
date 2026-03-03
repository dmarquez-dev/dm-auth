using System.Buffers.Text;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using DMAuth.Application.Features.Clients.Register;
using DMAuth.Tests.Integration.Common;
using FluentAssertions;
using System.Net;
using System.Net.Http.Json;

namespace DMAuth.Tests.Integration.Auth;

/// <summary>
///		Integration tests for POST /connect/revoke.
///		Per RFC 7009, the endpoint always returns 200 OK after structural validation
///		regardless of whether the token existed or was already revoked.
/// </summary>
public class RevokeTokenTests(IntegrationTestFactory factory)
	: IntegrationTestBase, IClassFixture<IntegrationTestFactory>
{
	private const string TestRedirectUri = "https://example.com/callback";
	private static readonly string CodeVerifier = new('v', 43);

	[Fact]
	public async Task RevokeToken_WithMissingToken_Returns400BadRequest()
	{
		var suffix = Guid.NewGuid().ToString("N")[..8];
		var httpClient = await factory.CreateAuthenticatedClientAsync(
			$"rvnone-{suffix}@example.com",
			$"rvnone-{suffix}",
			cancellationToken: TestCancellationToken);

		var formData = new FormUrlEncodedContent([]);

		var response = await httpClient.PostAsync("/connect/revoke", formData, TestCancellationToken);

		response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
	}

	[Fact]
	public async Task RevokeToken_WithUnknownToken_Returns200Ok()
	{
		var suffix = Guid.NewGuid().ToString("N")[..8];
		var httpClient = await factory.CreateAuthenticatedClientAsync(
			$"rvunk-{suffix}@example.com",
			$"rvunk-{suffix}",
			cancellationToken: TestCancellationToken);

		var formData = new FormUrlEncodedContent(
		[
			new("token", "unknown_refresh_token_value"),
		]);

		var response = await httpClient.PostAsync("/connect/revoke", formData, TestCancellationToken);

		response.StatusCode.Should().Be(HttpStatusCode.OK);
	}

	[Fact]
	public async Task RevokeToken_WithActiveToken_Returns200Ok()
	{
		var suffix = Guid.NewGuid().ToString("N")[..8];
		var httpClient = await factory.CreateAuthenticatedClientAsync(
			$"rvactive-{suffix}@example.com",
			$"rvactive-{suffix}",
			cancellationToken: TestCancellationToken);

		var refreshToken = await GetRefreshTokenAsync(httpClient);

		var formData = new FormUrlEncodedContent(
		[
			new("token", refreshToken),
		]);

		var response = await httpClient.PostAsync("/connect/revoke", formData, TestCancellationToken);

		response.StatusCode.Should().Be(HttpStatusCode.OK);
	}

	[Fact]
	public async Task RevokeToken_WhenCalledTwiceWithSameToken_Returns200OkBothTimes()
	{
		var suffix = Guid.NewGuid().ToString("N")[..8];
		var httpClient = await factory.CreateAuthenticatedClientAsync(
			$"rvtwice-{suffix}@example.com",
			$"rvtwice-{suffix}",
			cancellationToken: TestCancellationToken);

		var refreshToken = await GetRefreshTokenAsync(httpClient);

		var formData = new FormUrlEncodedContent(
		[
			new("token", refreshToken),
		]);

		await httpClient.PostAsync("/connect/revoke", formData, TestCancellationToken);

		// Second call with the same (now revoked) token should still succeed per RFC 7009
		var secondResponse = await httpClient.PostAsync("/connect/revoke", formData, TestCancellationToken);

		secondResponse.StatusCode.Should().Be(HttpStatusCode.OK);
	}

	private async Task<string> GetRefreshTokenAsync(HttpClient httpClient)
	{
		var (oauthClientId, _) = await RegisterClientAsync(httpClient);
		var code = await GrantConsentAndGetCodeAsync(httpClient, oauthClientId);
		var json = await ExchangeCodeAsync(httpClient, oauthClientId, code);

		return json.GetProperty("refresh_token").GetString()!;
	}

	private async Task<(string OAuthClientId, string RedirectUri)> RegisterClientAsync(HttpClient httpClient)
	{
		var response = await httpClient.PostAsJsonAsync(
			"/api/clients",
			new
			{
				clientName = "Test Client",
				clientType = "Public",
				redirectUris = new[] { TestRedirectUri },
				allowedScopes = new[] { "openid", "offline_access" }
			},
			TestCancellationToken);

		var body = await response.Content.ReadFromJsonAsync<RegisterClientResponse>(TestCancellationToken);

		return (body!.OAuthClientId, TestRedirectUri);
	}

	private async Task<string> GrantConsentAndGetCodeAsync(HttpClient httpClient, string oauthClientId)
	{
		var codeChallenge = Base64Url.EncodeToString(
			SHA256.HashData(Encoding.UTF8.GetBytes(CodeVerifier)));

		var formData = new FormUrlEncodedContent(
		[
			new("client_id", oauthClientId),
			new("redirect_uri", TestRedirectUri),
			new("state", "state123"),
			new("code_challenge", codeChallenge),
			new("code_challenge_method", "S256"),
			new("scope", "openid"),
			new("scope", "offline_access"),
		]);

		var response = await httpClient.PostAsync("/connect/consent", formData, TestCancellationToken);

		var location = response.Headers.Location!.ToString();

		return location.Split("code=")[1].Split('&')[0];
	}

	private async Task<JsonElement> ExchangeCodeAsync(
		HttpClient httpClient,
		string oauthClientId,
		string code)
	{
		var formData = new FormUrlEncodedContent(
		[
			new("grant_type", "authorization_code"),
			new("code", code),
			new("client_id", oauthClientId),
			new("redirect_uri", TestRedirectUri),
			new("code_verifier", CodeVerifier),
		]);

		var response = await httpClient.PostAsync("/connect/token", formData, TestCancellationToken);

		return await response.Content.ReadFromJsonAsync<JsonElement>(TestCancellationToken);
	}
}
