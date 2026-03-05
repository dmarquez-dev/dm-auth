using System.Buffers.Text;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using DMAuth.Application.Features.Clients.Register;
using DMAuth.Application.Features.Users.Login;
using DMAuth.Application.Features.Users.Register;
using DMAuth.Tests.Integration.Common;
using FluentAssertions;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.IdentityModel.Tokens;

namespace DMAuth.Tests.Integration.Auth;

/// <summary>
///		Integration tests for GET /connect/userinfo.
/// </summary>
public class UserInfoTests(IntegrationTestFactory factory)
	: IntegrationTestBase, IClassFixture<IntegrationTestFactory>
{
	private const string TestRedirectUri  = "https://example.com/callback";
	private const string TestPassword     = "Secure1!";
	private const string TestDisplayName  = "UserInfo Test User";
	private static readonly string CodeVerifier = new('v', 43);

	// -------------------------------------------------------------------------
	// Token rejection
	// -------------------------------------------------------------------------

	[Fact]
	public async Task UserInfo_WithNoAuthorizationHeader_Returns401Unauthorized()
	{
		var client = factory.CreateClient(new WebApplicationFactoryClientOptions
		{
			AllowAutoRedirect = false
		});

		var response = await client.GetAsync("/connect/userinfo", TestCancellationToken);

		response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
	}

	[Fact]
	public async Task UserInfo_WithTokenSignedByWrongKey_Returns401Unauthorized()
	{
		var client = factory.CreateClient(new WebApplicationFactoryClientOptions
		{
			AllowAutoRedirect = false
		});

		// Build a JWT using a completely different RSA key — signature will not verify
		var differentRsa = RSA.Create(2048);
		var credentials = new SigningCredentials(
			new RsaSecurityKey(differentRsa),
			SecurityAlgorithms.RsaSha256);

		var tokenString = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler().WriteToken(
			new System.IdentityModel.Tokens.Jwt.JwtSecurityToken(
				issuer: "https://test.dmauth.local",
				audience: "https://test.dmauth.local",
				claims: [new System.Security.Claims.Claim("sub", Guid.NewGuid().ToString())],
				notBefore: DateTime.UtcNow,
				expires: DateTime.UtcNow.AddMinutes(15),
				signingCredentials: credentials));

		differentRsa.Dispose();

		var response = await CallUserInfoAsync(client, tokenString);

		response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
	}

	[Fact]
	public async Task UserInfo_WithExpiredToken_Returns401Unauthorized()
	{
		var client = factory.CreateClient(new WebApplicationFactoryClientOptions
		{
			AllowAutoRedirect = false
		});

		var expiredToken = factory.CreateExpiredAccessToken(Guid.NewGuid());
		var response = await CallUserInfoAsync(client, expiredToken);

		response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
	}

	// -------------------------------------------------------------------------
	// Claim values
	// -------------------------------------------------------------------------

	[Fact]
	public async Task UserInfo_Sub_MatchesAuthenticatedUserId()
	{
		var suffix = Guid.NewGuid().ToString("N")[..8];
		var email = $"uisubmatch-{suffix}@example.com";

		var httpClient = factory.CreateClient(new WebApplicationFactoryClientOptions
		{
			AllowAutoRedirect = false
		});

		var registerResponse = await httpClient.PostAsJsonAsync(
			"/api/users/register",
			new { email, username = $"uisubmatch-{suffix}", password = TestPassword, displayName = TestDisplayName },
			TestCancellationToken);

		var registered = await registerResponse.Content
			.ReadFromJsonAsync<RegisterUserResponse>(TestCancellationToken);

		await httpClient.PostAsJsonAsync(
			"/api/users/login",
			new { email, password = TestPassword },
			TestCancellationToken);

		var (oauthClientId, _) = await RegisterClientAsync(
			httpClient, ["openid", "profile", "email"]);
		var accessToken = await GetAccessTokenAsync(
			httpClient, oauthClientId, ["openid", "profile", "email"]);

		var json = await GetUserInfoJsonAsync(httpClient, accessToken);

		json.GetProperty("sub").GetString()
			.Should().Be(registered!.UserId.ToString());
	}

	[Fact]
	public async Task UserInfo_WithAllScopes_ReturnsAllClaimsWithCorrectValues()
	{
		var suffix = Guid.NewGuid().ToString("N")[..8];
		var email = $"uiallscopes-{suffix}@example.com";
		var username = $"uiallscopes-{suffix}";

		var httpClient = await factory.CreateAuthenticatedClientAsync(
			email, username,
			password: TestPassword,
			displayName: TestDisplayName,
			cancellationToken: TestCancellationToken);

		var (oauthClientId, _) = await RegisterClientAsync(
			httpClient, ["openid", "profile", "email"]);
		var accessToken = await GetAccessTokenAsync(
			httpClient, oauthClientId, ["openid", "profile", "email"]);

		var json = await GetUserInfoJsonAsync(httpClient, accessToken);

		json.GetProperty("sub").GetString().Should().NotBeNullOrWhiteSpace();
		json.GetProperty("name").GetString().Should().Be(TestDisplayName);
		json.GetProperty("preferred_username").GetString().Should().Be(username);
		json.GetProperty("email").GetString().Should().Be(email);
		json.GetProperty("email_verified").GetBoolean().Should().BeFalse();
	}

	// -------------------------------------------------------------------------
	// Scope filtering
	// -------------------------------------------------------------------------

	[Theory]
	[InlineData("openid",                false, false)]
	[InlineData("openid profile",        true,  false)]
	[InlineData("openid email",          false, true)]
	[InlineData("openid profile email",  true,  true)]
	public async Task UserInfo_ScopeFiltering_IncludesOnlyGrantedClaimGroups(
		string scopeString,
		bool expectProfileClaims,
		bool expectEmailClaims)
	{
		var suffix = Guid.NewGuid().ToString("N")[..8];
		var scopes = scopeString.Split(' ');

		var httpClient = await factory.CreateAuthenticatedClientAsync(
			$"uifilter-{suffix}@example.com",
			$"uifilter-{suffix}",
			password: TestPassword,
			displayName: TestDisplayName,
			cancellationToken: TestCancellationToken);

		var (oauthClientId, _) = await RegisterClientAsync(
			httpClient, ["openid", "profile", "email"]);
		var accessToken = await GetAccessTokenAsync(httpClient, oauthClientId, scopes);

		var json = await GetUserInfoJsonAsync(httpClient, accessToken);

		// sub is always present
		json.TryGetProperty("sub", out _).Should().BeTrue();

		// profile claims present only when profile scope was granted
		json.TryGetProperty("name", out _).Should().Be(expectProfileClaims);
		json.TryGetProperty("preferred_username", out _).Should().Be(expectProfileClaims);

		// email claims present only when email scope was granted
		json.TryGetProperty("email", out _).Should().Be(expectEmailClaims);
		json.TryGetProperty("email_verified", out _).Should().Be(expectEmailClaims);
	}

	// -------------------------------------------------------------------------
	// Helpers
	// -------------------------------------------------------------------------

	private async Task<JsonElement> GetUserInfoJsonAsync(HttpClient client, string accessToken)
	{
		var response = await CallUserInfoAsync(client, accessToken);
		response.StatusCode.Should().Be(HttpStatusCode.OK);
		return await response.Content.ReadFromJsonAsync<JsonElement>(TestCancellationToken);
	}

	private async Task<HttpResponseMessage> CallUserInfoAsync(HttpClient client, string accessToken)
	{
		var request = new HttpRequestMessage(HttpMethod.Get, "/connect/userinfo");
		request.Headers.Authorization = new AuthenticationHeaderValue(
			JwtBearerDefaults.AuthenticationScheme,
			accessToken);
		return await client.SendAsync(request, TestCancellationToken);
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
				clientName = "UserInfo Test Client",
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
