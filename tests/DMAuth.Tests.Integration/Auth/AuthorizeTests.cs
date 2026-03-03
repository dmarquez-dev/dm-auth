using DMAuth.Application.Features.Clients.Register;
using DMAuth.Tests.Integration.Common;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;
using System.Net.Http.Json;

namespace DMAuth.Tests.Integration.Auth;

/// <summary>
///		Integration tests for GET /connect/authorize.
/// </summary>
public class AuthorizeTests(IntegrationTestFactory factory)
	: IntegrationTestBase, IClassFixture<IntegrationTestFactory>
{
	[Fact]
	public async Task Authorize_WithMissingClientId_Returns400BadRequest()
	{
		var httpClient = factory.CreateClient(new WebApplicationFactoryClientOptions
		{
			AllowAutoRedirect = false
		});

		var response = await httpClient.GetAsync(
			"/connect/authorize?redirect_uri=https%3A%2F%2Fexample.com%2Fcallback&response_type=code&scope=openid&state=s&code_challenge=AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA&code_challenge_method=S256",
			TestCancellationToken);

		response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
	}

	[Fact]
	public async Task Authorize_WithInvalidClientIdPrefix_Returns400BadRequest()
	{
		var httpClient = factory.CreateClient(new WebApplicationFactoryClientOptions
		{
			AllowAutoRedirect = false
		});

		var response = await httpClient.GetAsync(
			"/connect/authorize?client_id=bad_prefix_client&redirect_uri=https%3A%2F%2Fexample.com%2Fcallback&response_type=code&scope=openid&state=s&code_challenge=AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA&code_challenge_method=S256",
			TestCancellationToken);

		response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
	}

	[Fact]
	public async Task Authorize_WhenClientNotFound_Returns404NotFound()
	{
		var httpClient = factory.CreateClient(new WebApplicationFactoryClientOptions
		{
			AllowAutoRedirect = false
		});

		var response = await httpClient.GetAsync(
			BuildAuthorizeUrl("dmauth_nonexistent_client", "https://example.com/callback"),
			TestCancellationToken);

		response.StatusCode.Should().Be(HttpStatusCode.NotFound);
	}

	[Fact]
	public async Task Authorize_WhenUnauthenticated_RedirectsToLoginPage()
	{
		var suffix = Guid.NewGuid().ToString("N")[..8];
		var authenticatedClient = await factory.CreateAuthenticatedClientAsync(
			$"authlogin-{suffix}@example.com",
			$"authlogin-{suffix}",
			cancellationToken: TestCancellationToken);

		var (oauthClientId, redirectUri) = await RegisterClientAsync(authenticatedClient);

		var unauthClient = factory.CreateClient(new WebApplicationFactoryClientOptions
		{
			AllowAutoRedirect = false
		});

		var response = await unauthClient.GetAsync(
			BuildAuthorizeUrl(oauthClientId, redirectUri),
			TestCancellationToken);

		response.StatusCode.Should().Be(HttpStatusCode.Redirect);
		response.Headers.Location!.ToString().Should().Contain("/login");
	}

	[Fact]
	public async Task Authorize_WhenAuthenticatedWithNoConsent_RedirectsToConsentPage()
	{
		var suffix = Guid.NewGuid().ToString("N")[..8];
		var httpClient = await factory.CreateAuthenticatedClientAsync(
			$"noconsent-{suffix}@example.com",
			$"noconsent-{suffix}",
			cancellationToken: TestCancellationToken);

		var (oauthClientId, redirectUri) = await RegisterClientAsync(httpClient);

		var response = await httpClient.GetAsync(
			BuildAuthorizeUrl(oauthClientId, redirectUri),
			TestCancellationToken);

		response.StatusCode.Should().Be(HttpStatusCode.Redirect);
		response.Headers.Location!.ToString().Should().Contain("/consent");
	}

	[Fact]
	public async Task Authorize_WhenAuthenticatedWithFullConsent_RedirectsToRedirectUriWithCode()
	{
		var suffix = Guid.NewGuid().ToString("N")[..8];
		var httpClient = await factory.CreateAuthenticatedClientAsync(
			$"fullconsent-{suffix}@example.com",
			$"fullconsent-{suffix}",
			cancellationToken: TestCancellationToken);

		var (oauthClientId, redirectUri) = await RegisterClientAsync(httpClient);

		await GrantConsentAsync(httpClient, oauthClientId, redirectUri);

		var response = await httpClient.GetAsync(
			BuildAuthorizeUrl(oauthClientId, redirectUri),
			TestCancellationToken);

		response.StatusCode.Should().Be(HttpStatusCode.Redirect);
		response.Headers.Location!.ToString().Should().Contain("code=");
		response.Headers.Location!.ToString().Should().Contain("state=");
	}

	private async Task<(string OAuthClientId, string RedirectUri)> RegisterClientAsync(HttpClient httpClient)
	{
		const string redirectUri = "https://example.com/callback";

		var response = await httpClient.PostAsJsonAsync(
			"/api/clients",
			new
			{
				clientName = "Test Client",
				clientType = "Public",
				redirectUris = new[] { redirectUri },
				allowedScopes = new[] { "openid" }
			},
			TestCancellationToken);

		var body = await response.Content.ReadFromJsonAsync<RegisterClientResponse>(TestCancellationToken);

		return (body!.OAuthClientId, redirectUri);
	}

	private async Task GrantConsentAsync(HttpClient httpClient, string oauthClientId, string redirectUri)
	{
		var formData = new FormUrlEncodedContent(
		[
			new KeyValuePair<string, string>("client_id", oauthClientId),
			new KeyValuePair<string, string>("scope", "openid"),
			new KeyValuePair<string, string>("redirect_uri", redirectUri),
			new KeyValuePair<string, string>("state", "state123"),
			new KeyValuePair<string, string>("code_challenge", new string('A', 43)),
			new KeyValuePair<string, string>("code_challenge_method", "S256"),
		]);

		await httpClient.PostAsync("/connect/consent", formData, TestCancellationToken);
	}

	private static string BuildAuthorizeUrl(string clientId, string redirectUri) =>
		$"/connect/authorize" +
		$"?client_id={Uri.EscapeDataString(clientId)}" +
		$"&redirect_uri={Uri.EscapeDataString(redirectUri)}" +
		$"&response_type=code" +
		$"&scope=openid" +
		$"&state=state123" +
		$"&code_challenge={new string('A', 43)}" +
		$"&code_challenge_method=S256";
}
