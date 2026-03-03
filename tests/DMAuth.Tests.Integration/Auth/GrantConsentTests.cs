using DMAuth.Application.Features.Clients.Register;
using DMAuth.Tests.Integration.Common;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;
using System.Net.Http.Json;

namespace DMAuth.Tests.Integration.Auth;

/// <summary>
///		Integration tests for POST /connect/consent.
/// </summary>
public class GrantConsentTests(IntegrationTestFactory factory)
	: IntegrationTestBase, IClassFixture<IntegrationTestFactory>
{
	[Fact]
	public async Task GrantConsent_WhenUnauthenticated_Returns401Unauthorized()
	{
		var httpClient = factory.CreateClient(new WebApplicationFactoryClientOptions
		{
			AllowAutoRedirect = false
		});

		var formData = new FormUrlEncodedContent(
		[
			new KeyValuePair<string, string>("client_id", "dmauth_any_client"),
			new KeyValuePair<string, string>("scope", "openid"),
			new KeyValuePair<string, string>("redirect_uri", "https://example.com/callback"),
			new KeyValuePair<string, string>("state", "state123"),
			new KeyValuePair<string, string>("code_challenge", new string('A', 43)),
			new KeyValuePair<string, string>("code_challenge_method", "S256"),
		]);

		var response = await httpClient.PostAsync("/connect/consent", formData, TestCancellationToken);

		response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
	}

	[Fact]
	public async Task GrantConsent_WithMissingClientId_Returns400BadRequest()
	{
		var suffix = Guid.NewGuid().ToString("N")[..8];
		var httpClient = await factory.CreateAuthenticatedClientAsync(
			$"grantmissing-{suffix}@example.com",
			$"grantmissing-{suffix}",
			cancellationToken: TestCancellationToken);

		var formData = new FormUrlEncodedContent(
		[
			new KeyValuePair<string, string>("scope", "openid"),
			new KeyValuePair<string, string>("redirect_uri", "https://example.com/callback"),
			new KeyValuePair<string, string>("state", "state123"),
			new KeyValuePair<string, string>("code_challenge", new string('A', 43)),
			new KeyValuePair<string, string>("code_challenge_method", "S256"),
		]);

		var response = await httpClient.PostAsync("/connect/consent", formData, TestCancellationToken);

		response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
	}

	[Fact]
	public async Task GrantConsent_WithInvalidClientIdPrefix_Returns400BadRequest()
	{
		var suffix = Guid.NewGuid().ToString("N")[..8];
		var httpClient = await factory.CreateAuthenticatedClientAsync(
			$"grantprefix-{suffix}@example.com",
			$"grantprefix-{suffix}",
			cancellationToken: TestCancellationToken);

		var formData = new FormUrlEncodedContent(
		[
			new KeyValuePair<string, string>("client_id", "bad_prefix_client"),
			new KeyValuePair<string, string>("scope", "openid"),
			new KeyValuePair<string, string>("redirect_uri", "https://example.com/callback"),
			new KeyValuePair<string, string>("state", "state123"),
			new KeyValuePair<string, string>("code_challenge", new string('A', 43)),
			new KeyValuePair<string, string>("code_challenge_method", "S256"),
		]);

		var response = await httpClient.PostAsync("/connect/consent", formData, TestCancellationToken);

		response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
	}

	[Fact]
	public async Task GrantConsent_WhenClientNotFound_Returns404NotFound()
	{
		var suffix = Guid.NewGuid().ToString("N")[..8];
		var httpClient = await factory.CreateAuthenticatedClientAsync(
			$"grantnotfound-{suffix}@example.com",
			$"grantnotfound-{suffix}",
			cancellationToken: TestCancellationToken);

		var formData = new FormUrlEncodedContent(
		[
			new KeyValuePair<string, string>("client_id", "dmauth_nonexistent_client"),
			new KeyValuePair<string, string>("scope", "openid"),
			new KeyValuePair<string, string>("redirect_uri", "https://example.com/callback"),
			new KeyValuePair<string, string>("state", "state123"),
			new KeyValuePair<string, string>("code_challenge", new string('A', 43)),
			new KeyValuePair<string, string>("code_challenge_method", "S256"),
		]);

		var response = await httpClient.PostAsync("/connect/consent", formData, TestCancellationToken);

		response.StatusCode.Should().Be(HttpStatusCode.NotFound);
	}

	[Fact]
	public async Task GrantConsent_WithValidRequest_RedirectsToRedirectUriWithCode()
	{
		var suffix = Guid.NewGuid().ToString("N")[..8];
		var httpClient = await factory.CreateAuthenticatedClientAsync(
			$"grantvalid-{suffix}@example.com",
			$"grantvalid-{suffix}",
			cancellationToken: TestCancellationToken);

		var (oauthClientId, redirectUri) = await RegisterClientAsync(httpClient);

		var formData = new FormUrlEncodedContent(
		[
			new KeyValuePair<string, string>("client_id", oauthClientId),
			new KeyValuePair<string, string>("scope", "openid"),
			new KeyValuePair<string, string>("redirect_uri", redirectUri),
			new KeyValuePair<string, string>("state", "state123"),
			new KeyValuePair<string, string>("code_challenge", new string('A', 43)),
			new KeyValuePair<string, string>("code_challenge_method", "S256"),
		]);

		var response = await httpClient.PostAsync("/connect/consent", formData, TestCancellationToken);

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
}
