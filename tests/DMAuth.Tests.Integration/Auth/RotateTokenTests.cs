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
///		Integration tests for POST /connect/token with grant_type=refresh_token.
/// </summary>
public class RotateTokenTests(IntegrationTestFactory factory)
	: IntegrationTestBase, IClassFixture<IntegrationTestFactory>
{
	private const string TestRedirectUri = "https://example.com/callback";
	private static readonly string CodeVerifier = new('v', 43);

	[Fact]
	public async Task RotateToken_WithMissingClientId_Returns400BadRequest()
	{
		var suffix = Guid.NewGuid().ToString("N")[..8];
		var httpClient = await factory.CreateAuthenticatedClientAsync(
			$"rotnocid-{suffix}@example.com",
			$"rotnocid-{suffix}",
			cancellationToken: TestCancellationToken);

		var formData = new FormUrlEncodedContent(
		[
			new("grant_type", "refresh_token"),
			new("refresh_token", "some_refresh_token"),
		]);

		var response = await httpClient.PostAsync("/connect/token", formData, TestCancellationToken);

		response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
	}

	[Fact]
	public async Task RotateToken_WithInvalidClientIdPrefix_Returns400BadRequest()
	{
		var suffix = Guid.NewGuid().ToString("N")[..8];
		var httpClient = await factory.CreateAuthenticatedClientAsync(
			$"rotbadpfx-{suffix}@example.com",
			$"rotbadpfx-{suffix}",
			cancellationToken: TestCancellationToken);

		var formData = new FormUrlEncodedContent(
		[
			new("grant_type", "refresh_token"),
			new("client_id", "bad_prefix_client"),
			new("refresh_token", "some_refresh_token"),
		]);

		var response = await httpClient.PostAsync("/connect/token", formData, TestCancellationToken);

		response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
	}

	[Fact]
	public async Task RotateToken_WhenTokenNotFound_Returns401Unauthorized()
	{
		var suffix = Guid.NewGuid().ToString("N")[..8];
		var httpClient = await factory.CreateAuthenticatedClientAsync(
			$"rotnoent-{suffix}@example.com",
			$"rotnoent-{suffix}",
			cancellationToken: TestCancellationToken);

		var (oauthClientId, _) = await RegisterClientAsync(httpClient);

		var formData = new FormUrlEncodedContent(
		[
			new("grant_type", "refresh_token"),
			new("client_id", oauthClientId),
			new("refresh_token", "nonexistent_refresh_token"),
		]);

		var response = await httpClient.PostAsync("/connect/token", formData, TestCancellationToken);

		response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
	}

	[Fact]
	public async Task RotateToken_WithValidRequest_Returns200Ok()
	{
		var suffix = Guid.NewGuid().ToString("N")[..8];
		var httpClient = await factory.CreateAuthenticatedClientAsync(
			$"rotvalid-{suffix}@example.com",
			$"rotvalid-{suffix}",
			cancellationToken: TestCancellationToken);

		var (oauthClientId, refreshToken) = await GetInitialTokensAsync(httpClient);

		var response = await RotateTokenRawAsync(httpClient, oauthClientId, refreshToken);

		response.StatusCode.Should().Be(HttpStatusCode.OK);
	}

	[Fact]
	public async Task RotateToken_WithValidRequest_ReturnsNewAccessToken()
	{
		var suffix = Guid.NewGuid().ToString("N")[..8];
		var httpClient = await factory.CreateAuthenticatedClientAsync(
			$"rotaccess-{suffix}@example.com",
			$"rotaccess-{suffix}",
			cancellationToken: TestCancellationToken);

		var (oauthClientId, refreshToken) = await GetInitialTokensAsync(httpClient);

		var json = await RotateTokenAsync(httpClient, oauthClientId, refreshToken);

		json.GetProperty("access_token").GetString().Should().NotBeNullOrEmpty();
	}

	[Fact]
	public async Task RotateToken_WithValidRequest_ReturnsNewRefreshToken()
	{
		var suffix = Guid.NewGuid().ToString("N")[..8];
		var httpClient = await factory.CreateAuthenticatedClientAsync(
			$"rotrt-{suffix}@example.com",
			$"rotrt-{suffix}",
			cancellationToken: TestCancellationToken);

		var (oauthClientId, refreshToken) = await GetInitialTokensAsync(httpClient);

		var json = await RotateTokenAsync(httpClient, oauthClientId, refreshToken);

		json.GetProperty("refresh_token").GetString().Should().NotBeNullOrEmpty();
	}

	[Fact]
	public async Task RotateToken_WhenPresentingRotatedToken_Returns401Unauthorized()
	{
		var suffix = Guid.NewGuid().ToString("N")[..8];
		var httpClient = await factory.CreateAuthenticatedClientAsync(
			$"rotreuse-{suffix}@example.com",
			$"rotreuse-{suffix}",
			cancellationToken: TestCancellationToken);

		var (oauthClientId, originalRefreshToken) = await GetInitialTokensAsync(httpClient);

		// Rotate once — this revokes the original token
		await RotateTokenAsync(httpClient, oauthClientId, originalRefreshToken);

		// Present the original (now revoked) token again — should be rejected as reuse
		var response = await RotateTokenRawAsync(httpClient, oauthClientId, originalRefreshToken);

		response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
	}

	private async Task<(string OAuthClientId, string RefreshToken)> GetInitialTokensAsync(
		HttpClient httpClient)
	{
		var (oauthClientId, _) = await RegisterClientAsync(httpClient);
		var code = await GrantConsentAndGetCodeAsync(httpClient, oauthClientId);
		var json = await ExchangeCodeAsync(httpClient, oauthClientId, code);
		var refreshToken = json.GetProperty("refresh_token").GetString()!;

		return (oauthClientId, refreshToken);
	}

	private async Task<(string OAuthClientId, string RedirectUri)> RegisterClientAsync(
		HttpClient httpClient)
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

	private async Task<HttpResponseMessage> RotateTokenRawAsync(
		HttpClient httpClient,
		string oauthClientId,
		string refreshToken)
	{
		var formData = new FormUrlEncodedContent(
		[
			new("grant_type", "refresh_token"),
			new("client_id", oauthClientId),
			new("refresh_token", refreshToken),
		]);

		return await httpClient.PostAsync("/connect/token", formData, TestCancellationToken);
	}

	private async Task<JsonElement> RotateTokenAsync(
		HttpClient httpClient,
		string oauthClientId,
		string refreshToken)
	{
		var response = await RotateTokenRawAsync(httpClient, oauthClientId, refreshToken);

		return await response.Content.ReadFromJsonAsync<JsonElement>(TestCancellationToken);
	}
}
