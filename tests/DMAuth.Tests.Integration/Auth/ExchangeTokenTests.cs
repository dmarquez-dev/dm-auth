using System.Buffers.Text;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using DMAuth.Application.Features.Clients.Register;
using DMAuth.Tests.Integration.Common;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;
using System.Net.Http.Json;

namespace DMAuth.Tests.Integration.Auth;

/// <summary>
///		Integration tests for POST /connect/token with grant_type=authorization_code.
/// </summary>
public class ExchangeTokenTests(IntegrationTestFactory factory)
	: IntegrationTestBase, IClassFixture<IntegrationTestFactory>
{
	private const string TestRedirectUri = "https://example.com/callback";
	private static readonly string CodeVerifier = new('v', 43);

	[Fact]
	public async Task ExchangeToken_WithMissingGrantType_Returns400BadRequest()
	{
		var suffix = Guid.NewGuid().ToString("N")[..8];
		var httpClient = await factory.CreateAuthenticatedClientAsync(
			$"etnogt-{suffix}@example.com",
			$"etnogt-{suffix}",
			cancellationToken: TestCancellationToken);

		var formData = new FormUrlEncodedContent(
		[
			new("code", "some_code"),
			new("client_id", "dmauth_some_client"),
			new("redirect_uri", TestRedirectUri),
			new("code_verifier", CodeVerifier),
		]);

		var response = await httpClient.PostAsync("/connect/token", formData, TestCancellationToken);

		response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
	}

	[Fact]
	public async Task ExchangeToken_WithMissingClientId_Returns400BadRequest()
	{
		var suffix = Guid.NewGuid().ToString("N")[..8];
		var httpClient = await factory.CreateAuthenticatedClientAsync(
			$"etnocid-{suffix}@example.com",
			$"etnocid-{suffix}",
			cancellationToken: TestCancellationToken);

		var formData = new FormUrlEncodedContent(
		[
			new("grant_type", "authorization_code"),
			new("code", "some_code"),
			new("redirect_uri", TestRedirectUri),
			new("code_verifier", CodeVerifier),
		]);

		var response = await httpClient.PostAsync("/connect/token", formData, TestCancellationToken);

		response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
	}

	[Fact]
	public async Task ExchangeToken_WithInvalidClientIdPrefix_Returns400BadRequest()
	{
		var suffix = Guid.NewGuid().ToString("N")[..8];
		var httpClient = await factory.CreateAuthenticatedClientAsync(
			$"etbadpfx-{suffix}@example.com",
			$"etbadpfx-{suffix}",
			cancellationToken: TestCancellationToken);

		var formData = new FormUrlEncodedContent(
		[
			new("grant_type", "authorization_code"),
			new("code", "some_code"),
			new("client_id", "bad_prefix_client"),
			new("redirect_uri", TestRedirectUri),
			new("code_verifier", CodeVerifier),
		]);

		var response = await httpClient.PostAsync("/connect/token", formData, TestCancellationToken);

		response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
	}

	[Fact]
	public async Task ExchangeToken_WhenClientNotFound_Returns404NotFound()
	{
		var suffix = Guid.NewGuid().ToString("N")[..8];
		var httpClient = await factory.CreateAuthenticatedClientAsync(
			$"etnoent-{suffix}@example.com",
			$"etnoent-{suffix}",
			cancellationToken: TestCancellationToken);

		var formData = new FormUrlEncodedContent(
		[
			new("grant_type", "authorization_code"),
			new("code", "some_code"),
			new("client_id", "dmauth_nonexistent_client"),
			new("redirect_uri", TestRedirectUri),
			new("code_verifier", CodeVerifier),
		]);

		var response = await httpClient.PostAsync("/connect/token", formData, TestCancellationToken);

		response.StatusCode.Should().Be(HttpStatusCode.NotFound);
	}

	[Fact]
	public async Task ExchangeToken_WhenCodeNotFound_Returns400BadRequest()
	{
		var suffix = Guid.NewGuid().ToString("N")[..8];
		var httpClient = await factory.CreateAuthenticatedClientAsync(
			$"etbadcode-{suffix}@example.com",
			$"etbadcode-{suffix}",
			cancellationToken: TestCancellationToken);

		var (oauthClientId, _) = await RegisterClientAsync(httpClient);

		var formData = new FormUrlEncodedContent(
		[
			new("grant_type", "authorization_code"),
			new("code", "nonexistent_code"),
			new("client_id", oauthClientId),
			new("redirect_uri", TestRedirectUri),
			new("code_verifier", CodeVerifier),
		]);

		var response = await httpClient.PostAsync("/connect/token", formData, TestCancellationToken);

		response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
	}

	[Fact]
	public async Task ExchangeToken_WithValidRequest_Returns200Ok()
	{
		var suffix = Guid.NewGuid().ToString("N")[..8];
		var httpClient = await factory.CreateAuthenticatedClientAsync(
			$"etvalid-{suffix}@example.com",
			$"etvalid-{suffix}",
			cancellationToken: TestCancellationToken);

		var (oauthClientId, _) = await RegisterClientAsync(httpClient);
		var code = await GrantConsentAndGetCodeAsync(httpClient, oauthClientId, ["openid"]);

		var response = await ExchangeCodeRawAsync(httpClient, oauthClientId, code);

		response.StatusCode.Should().Be(HttpStatusCode.OK);
	}

	[Fact]
	public async Task ExchangeToken_WithValidRequest_ReturnsAccessToken()
	{
		var suffix = Guid.NewGuid().ToString("N")[..8];
		var httpClient = await factory.CreateAuthenticatedClientAsync(
			$"etaccess-{suffix}@example.com",
			$"etaccess-{suffix}",
			cancellationToken: TestCancellationToken);

		var (oauthClientId, _) = await RegisterClientAsync(httpClient);
		var code = await GrantConsentAndGetCodeAsync(httpClient, oauthClientId, ["openid"]);

		var json = await ExchangeCodeAsync(httpClient, oauthClientId, code);

		json.GetProperty("access_token").GetString().Should().NotBeNullOrEmpty();
	}

	[Fact]
	public async Task ExchangeToken_WhenOpenIdScopeGranted_ReturnsIdToken()
	{
		var suffix = Guid.NewGuid().ToString("N")[..8];
		var httpClient = await factory.CreateAuthenticatedClientAsync(
			$"etidtok-{suffix}@example.com",
			$"etidtok-{suffix}",
			cancellationToken: TestCancellationToken);

		var (oauthClientId, _) = await RegisterClientAsync(httpClient);
		var code = await GrantConsentAndGetCodeAsync(httpClient, oauthClientId, ["openid"]);

		var json = await ExchangeCodeAsync(httpClient, oauthClientId, code);

		json.GetProperty("id_token").GetString().Should().NotBeNullOrEmpty();
	}

	[Fact]
	public async Task ExchangeToken_WhenOfflineAccessScopeGranted_ReturnsRefreshToken()
	{
		var suffix = Guid.NewGuid().ToString("N")[..8];
		var httpClient = await factory.CreateAuthenticatedClientAsync(
			$"etrefresh-{suffix}@example.com",
			$"etrefresh-{suffix}",
			cancellationToken: TestCancellationToken);

		var (oauthClientId, _) = await RegisterClientAsync(httpClient, ["openid", "offline_access"]);
		var code = await GrantConsentAndGetCodeAsync(httpClient, oauthClientId, ["openid", "offline_access"]);

		var json = await ExchangeCodeAsync(httpClient, oauthClientId, code);

		json.GetProperty("refresh_token").GetString().Should().NotBeNullOrEmpty();
	}

	[Fact]
	public async Task ExchangeToken_WhenOfflineAccessScopeNotGranted_DoesNotReturnRefreshToken()
	{
		var suffix = Guid.NewGuid().ToString("N")[..8];
		var httpClient = await factory.CreateAuthenticatedClientAsync(
			$"etnort-{suffix}@example.com",
			$"etnort-{suffix}",
			cancellationToken: TestCancellationToken);

		var (oauthClientId, _) = await RegisterClientAsync(httpClient);
		var code = await GrantConsentAndGetCodeAsync(httpClient, oauthClientId, ["openid"]);

		var json = await ExchangeCodeAsync(httpClient, oauthClientId, code);

		json.TryGetProperty("refresh_token", out var refreshToken);
		refreshToken.ValueKind.Should().BeOneOf(JsonValueKind.Null, JsonValueKind.Undefined);
	}

	private async Task<(string OAuthClientId, string RedirectUri)> RegisterClientAsync(
		HttpClient httpClient,
		string[]? allowedScopes = null)
	{
		allowedScopes ??= ["openid"];
		var response = await httpClient.PostAsJsonAsync(
			"/api/clients",
			new
			{
				clientName = "Test Client",
				clientType = "Public",
				redirectUris = new[] { TestRedirectUri },
				allowedScopes
			},
			TestCancellationToken);

		var body = await response.Content.ReadFromJsonAsync<RegisterClientResponse>(TestCancellationToken);

		return (body!.OAuthClientId, TestRedirectUri);
	}

	private async Task<string> GrantConsentAndGetCodeAsync(
		HttpClient httpClient,
		string oauthClientId,
		string[] scopes)
	{
		var codeChallenge = ComputeCodeChallenge();

		var formFields = new List<KeyValuePair<string, string>>
		{
			new("client_id", oauthClientId),
			new("redirect_uri", TestRedirectUri),
			new("state", "state123"),
			new("code_challenge", codeChallenge),
			new("code_challenge_method", "S256"),
		};

		foreach (var scope in scopes)
		{
			formFields.Add(new("scope", scope));
		}

		var response = await httpClient.PostAsync(
			"/connect/consent",
			new FormUrlEncodedContent(formFields),
			TestCancellationToken);

		var location = response.Headers.Location!.ToString();

		return location.Split("code=")[1].Split('&')[0];
	}

	private async Task<HttpResponseMessage> ExchangeCodeRawAsync(
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

		return await httpClient.PostAsync("/connect/token", formData, TestCancellationToken);
	}

	private async Task<JsonElement> ExchangeCodeAsync(
		HttpClient httpClient,
		string oauthClientId,
		string code)
	{
		var response = await ExchangeCodeRawAsync(httpClient, oauthClientId, code);

		return await response.Content.ReadFromJsonAsync<JsonElement>(TestCancellationToken);
	}

	private static string ComputeCodeChallenge() =>
		Base64Url.EncodeToString(SHA256.HashData(Encoding.UTF8.GetBytes(CodeVerifier)));
}
