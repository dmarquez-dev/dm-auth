using System.Buffers.Text;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using DMAuth.Application.Features.Clients.Register;
using DMAuth.Tests.Integration.Common;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;

namespace DMAuth.Tests.Integration.Auth;

/// <summary>
///		Integration tests for GET /.well-known/jwks.json.
/// </summary>
public class JwksTests(IntegrationTestFactory factory)
	: IntegrationTestBase, IClassFixture<IntegrationTestFactory>
{
	private const string TestRedirectUri = "https://example.com/callback";
	private static readonly string CodeVerifier = new('v', 43);

	[Fact]
	public async Task Jwks_Returns200Ok()
	{
		var client = factory.CreateClient(new WebApplicationFactoryClientOptions
		{
			AllowAutoRedirect = false
		});

		var response = await client.GetAsync("/.well-known/jwks.json", TestCancellationToken);

		response.StatusCode.Should().Be(HttpStatusCode.OK);
	}

	[Fact]
	public async Task Jwks_ContainsOneRsaKey_WithAllRequiredFields()
	{
		var client = factory.CreateClient(new WebApplicationFactoryClientOptions
		{
			AllowAutoRedirect = false
		});

		var json = await client.GetFromJsonAsync<JsonElement>(
			"/.well-known/jwks.json",
			TestCancellationToken);

		var keys = json.GetProperty("keys").EnumerateArray().ToList();
		keys.Should().HaveCount(1);

		var key = keys[0];
		key.GetProperty("kty").GetString().Should().Be("RSA");
		key.GetProperty("use").GetString().Should().Be("sig");
		key.GetProperty("alg").GetString().Should().Be("RS256");
		key.GetProperty("kid").GetString().Should().NotBeNullOrWhiteSpace();
		key.GetProperty("n").GetString().Should().NotBeNullOrWhiteSpace();
		key.GetProperty("e").GetString().Should().NotBeNullOrWhiteSpace();
	}

	[Fact]
	public async Task Jwks_HasPublicCacheControlHeader()
	{
		var client = factory.CreateClient(new WebApplicationFactoryClientOptions
		{
			AllowAutoRedirect = false
		});

		var response = await client.GetAsync("/.well-known/jwks.json", TestCancellationToken);

		response.Headers.CacheControl.Should().NotBeNull();
		response.Headers.CacheControl!.MaxAge.Should().Be(TimeSpan.FromSeconds(86400));
		response.Headers.CacheControl.Public.Should().BeTrue();
	}

	[Fact]
	public async Task Jwks_KeyId_MatchesKidHeaderInIssuedAccessTokens()
	{
		var suffix = Guid.NewGuid().ToString("N")[..8];
		var httpClient = await factory.CreateAuthenticatedClientAsync(
			$"jwks-{suffix}@example.com",
			$"jwks-{suffix}",
			cancellationToken: TestCancellationToken);

		var (oauthClientId, _) = await RegisterClientAsync(httpClient);
		var accessToken = await GetAccessTokenAsync(httpClient, oauthClientId, ["openid"]);

		// Extract the kid from the issued JWT header
		var jwt = new JwtSecurityTokenHandler().ReadJwtToken(accessToken);
		var tokenKid = jwt.Header.Kid;

		// Verify a JWKS entry exists with that kid
		var jwks = await httpClient.GetFromJsonAsync<JsonElement>(
			"/.well-known/jwks.json",
			TestCancellationToken);

		var jwksKids = jwks.GetProperty("keys")
			.EnumerateArray()
			.Select(key => key.GetProperty("kid").GetString())
			.ToList();

		jwksKids.Should().Contain(tokenKid,
			"the JWKS must contain the key used to sign the access token");
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
				clientName = "JWKS Test Client",
				clientType = "Public",
				redirectUris = new[] { TestRedirectUri },
				allowedScopes
			},
			TestCancellationToken);

		var body = await response.Content.ReadFromJsonAsync<RegisterClientResponse>(TestCancellationToken);
		return (body!.OAuthClientId, TestRedirectUri);
	}

	private async Task<string> GetAccessTokenAsync(
		HttpClient httpClient,
		string oauthClientId,
		string[] scopes)
	{
		var code = await GrantConsentAndGetCodeAsync(httpClient, oauthClientId, scopes);

		var tokenResponse = await httpClient.PostAsync(
			"/connect/token",
			new FormUrlEncodedContent(
			[
				new("grant_type", "authorization_code"),
				new("code", code),
				new("client_id", oauthClientId),
				new("redirect_uri", TestRedirectUri),
				new("code_verifier", CodeVerifier),
			]),
			TestCancellationToken);

		var json = await tokenResponse.Content.ReadFromJsonAsync<JsonElement>(TestCancellationToken);
		return json.GetProperty("access_token").GetString()!;
	}

	private async Task<string> GrantConsentAndGetCodeAsync(
		HttpClient httpClient,
		string oauthClientId,
		string[] scopes)
	{
		var codeChallenge = Base64Url.EncodeToString(
			SHA256.HashData(Encoding.UTF8.GetBytes(CodeVerifier)));

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
}
