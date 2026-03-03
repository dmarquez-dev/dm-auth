using DMAuth.Application.Features.Clients.Register;
using DMAuth.Tests.Integration.Common;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;
using System.Net.Http.Json;

namespace DMAuth.Tests.Integration.Auth;

/// <summary>
///		Integration tests for the consent-checking behavior within GET /connect/authorize.
///		Exercises the GetAuthorizationDetails query, which determines whether existing consent
///		covers the requested scopes and controls whether the user is redirected to the consent
///		page or automatically granted an authorization code.
/// </summary>
public class GetAuthorizationDetailsTests(IntegrationTestFactory factory)
	: IntegrationTestBase, IClassFixture<IntegrationTestFactory>
{
	private const string TestRedirectUri = "https://example.com/callback";

	[Fact]
	public async Task Authorize_WhenUserHasNoConsent_RedirectsToConsentPage()
	{
		var suffix = Guid.NewGuid().ToString("N")[..8];
		var httpClient = await factory.CreateAuthenticatedClientAsync(
			$"gadnone-{suffix}@example.com",
			$"gadnone-{suffix}",
			cancellationToken: TestCancellationToken);

		var (oauthClientId, _) = await RegisterClientAsync(httpClient, ["openid", "profile"]);

		var response = await httpClient.GetAsync(
			BuildAuthorizeUrl(oauthClientId, "openid profile"),
			TestCancellationToken);

		response.StatusCode.Should().Be(HttpStatusCode.Redirect);
		response.Headers.Location!.ToString().Should().Contain("/consent");
	}

	[Fact]
	public async Task Authorize_WhenUserHasFullConsent_RedirectsWithCode()
	{
		var suffix = Guid.NewGuid().ToString("N")[..8];
		var httpClient = await factory.CreateAuthenticatedClientAsync(
			$"gadfull-{suffix}@example.com",
			$"gadfull-{suffix}",
			cancellationToken: TestCancellationToken);

		var (oauthClientId, _) = await RegisterClientAsync(httpClient, ["openid", "profile"]);

		await GrantConsentAsync(httpClient, oauthClientId, ["openid", "profile"]);

		var response = await httpClient.GetAsync(
			BuildAuthorizeUrl(oauthClientId, "openid profile"),
			TestCancellationToken);

		response.StatusCode.Should().Be(HttpStatusCode.Redirect);
		response.Headers.Location!.ToString().Should().Contain("code=");
	}

	[Fact]
	public async Task Authorize_WhenUserHasConsentForSuperset_DoesNotRequireConsentForSubset()
	{
		var suffix = Guid.NewGuid().ToString("N")[..8];
		var httpClient = await factory.CreateAuthenticatedClientAsync(
			$"gadsup-{suffix}@example.com",
			$"gadsup-{suffix}",
			cancellationToken: TestCancellationToken);

		var (oauthClientId, _) = await RegisterClientAsync(httpClient, ["openid", "profile"]);

		// Grant consent for the full set of scopes
		await GrantConsentAsync(httpClient, oauthClientId, ["openid", "profile"]);

		// Authorize requesting only a subset
		var response = await httpClient.GetAsync(
			BuildAuthorizeUrl(oauthClientId, "openid"),
			TestCancellationToken);

		response.StatusCode.Should().Be(HttpStatusCode.Redirect);
		response.Headers.Location!.ToString().Should().Contain("code=");
	}

	[Fact]
	public async Task Authorize_WhenUserHasPartialConsent_RedirectsToConsentPageForExpansion()
	{
		var suffix = Guid.NewGuid().ToString("N")[..8];
		var httpClient = await factory.CreateAuthenticatedClientAsync(
			$"gadpart-{suffix}@example.com",
			$"gadpart-{suffix}",
			cancellationToken: TestCancellationToken);

		var (oauthClientId, _) = await RegisterClientAsync(httpClient, ["openid", "profile"]);

		// Grant consent for a partial set of scopes
		await GrantConsentAsync(httpClient, oauthClientId, ["openid"]);

		// Authorize requesting more scopes than consented
		var response = await httpClient.GetAsync(
			BuildAuthorizeUrl(oauthClientId, "openid profile"),
			TestCancellationToken);

		response.StatusCode.Should().Be(HttpStatusCode.Redirect);
		response.Headers.Location!.ToString().Should().Contain("/consent");
	}

	private async Task<(string OAuthClientId, string RedirectUri)> RegisterClientAsync(
		HttpClient httpClient,
		string[] allowedScopes)
	{
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

	private async Task GrantConsentAsync(
		HttpClient httpClient,
		string oauthClientId,
		string[] scopes)
	{
		var formFields = new List<KeyValuePair<string, string>>
		{
			new("client_id", oauthClientId),
			new("redirect_uri", TestRedirectUri),
			new("state", "state123"),
			new("code_challenge", new string('A', 43)),
			new("code_challenge_method", "S256"),
		};

		foreach (var scope in scopes)
		{
			formFields.Add(new("scope", scope));
		}

		await httpClient.PostAsync(
			"/connect/consent",
			new FormUrlEncodedContent(formFields),
			TestCancellationToken);
	}

	private static string BuildAuthorizeUrl(string clientId, string scope) =>
		$"/connect/authorize" +
		$"?client_id={Uri.EscapeDataString(clientId)}" +
		$"&redirect_uri={Uri.EscapeDataString(TestRedirectUri)}" +
		$"&response_type=code" +
		$"&scope={Uri.EscapeDataString(scope)}" +
		$"&state=state123" +
		$"&code_challenge={new string('A', 43)}" +
		$"&code_challenge_method=S256";
}
