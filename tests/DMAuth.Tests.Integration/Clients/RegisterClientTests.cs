using DMAuth.Application.Features.Clients.Register;
using DMAuth.Tests.Integration.Common;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;
using System.Net.Http.Json;

namespace DMAuth.Tests.Integration.Clients;

/// <summary>
///		Integration tests for POST /api/clients.
/// </summary>
public class RegisterClientTests(IntegrationTestFactory factory)
	: IntegrationTestBase, IClassFixture<IntegrationTestFactory>
{
	[Fact]
	public async Task Register_WithValidPublicClient_Returns201Created()
	{
		var suffix = Guid.NewGuid().ToString("N")[..8];
		var httpClient = await factory.CreateAuthenticatedClientAsync(
			$"regclient-{suffix}@example.com",
			$"regclient-{suffix}",
			cancellationToken: TestCancellationToken);

		var response = await httpClient.PostAsJsonAsync(
			"/api/clients",
			new
			{
				clientName = "Test Client",
				clientType = "Public",
				redirectUris = new[] { "https://example.com/callback" },
				allowedScopes = new[] { "openid" }
			},
			TestCancellationToken);

		response.StatusCode.Should().Be(HttpStatusCode.Created);
	}

	[Fact]
	public async Task Register_WithValidPublicClient_ReturnsClientData()
	{
		var suffix = Guid.NewGuid().ToString("N")[..8];
		var httpClient = await factory.CreateAuthenticatedClientAsync(
			$"regdata-{suffix}@example.com",
			$"regdata-{suffix}",
			cancellationToken: TestCancellationToken);

		var response = await httpClient.PostAsJsonAsync(
			"/api/clients",
			new
			{
				clientName = "My App",
				clientType = "Public",
				redirectUris = new[] { "https://example.com/callback" },
				allowedScopes = new[] { "openid" }
			},
			TestCancellationToken);

		var body = await response.Content.ReadFromJsonAsync<RegisterClientResponse>(TestCancellationToken);

		body!.ClientId.Should().NotBeEmpty();
		body.OAuthClientId.Should().StartWith("dma_");
		body.ClientSecret.Should().BeNull();
	}

	[Fact]
	public async Task Register_WithValidConfidentialClient_ReturnsClientSecret()
	{
		var suffix = Guid.NewGuid().ToString("N")[..8];
		var httpClient = await factory.CreateAuthenticatedClientAsync(
			$"confidential-{suffix}@example.com",
			$"confidential-{suffix}",
			cancellationToken: TestCancellationToken);

		var response = await httpClient.PostAsJsonAsync(
			"/api/clients",
			new
			{
				clientName = "Confidential App",
				clientType = "Confidential",
				redirectUris = new[] { "https://example.com/callback" },
				allowedScopes = new[] { "openid" }
			},
			TestCancellationToken);

		var body = await response.Content.ReadFromJsonAsync<RegisterClientResponse>(TestCancellationToken);

		body!.ClientSecret.Should().NotBeNullOrEmpty();
	}

	[Fact]
	public async Task Register_WithMissingClientName_Returns400BadRequest()
	{
		var suffix = Guid.NewGuid().ToString("N")[..8];
		var httpClient = await factory.CreateAuthenticatedClientAsync(
			$"regnoname-{suffix}@example.com",
			$"regnoname-{suffix}",
			cancellationToken: TestCancellationToken);

		var response = await httpClient.PostAsJsonAsync(
			"/api/clients",
			new
			{
				clientName = "",
				clientType = "Public",
				redirectUris = new[] { "https://example.com/callback" },
				allowedScopes = new[] { "openid" }
			},
			TestCancellationToken);

		response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
	}

	[Fact]
	public async Task Register_WithInvalidRedirectUri_Returns400BadRequest()
	{
		var suffix = Guid.NewGuid().ToString("N")[..8];
		var httpClient = await factory.CreateAuthenticatedClientAsync(
			$"regbaduri-{suffix}@example.com",
			$"regbaduri-{suffix}",
			cancellationToken: TestCancellationToken);

		var response = await httpClient.PostAsJsonAsync(
			"/api/clients",
			new
			{
				clientName = "Test Client",
				clientType = "Public",
				redirectUris = new[] { "http://example.com/callback" },  // non-HTTPS, non-localhost
				allowedScopes = new[] { "openid" }
			},
			TestCancellationToken);

		response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
	}

	[Fact]
	public async Task Register_WithInvalidScope_Returns400BadRequest()
	{
		var suffix = Guid.NewGuid().ToString("N")[..8];
		var httpClient = await factory.CreateAuthenticatedClientAsync(
			$"regbadscope-{suffix}@example.com",
			$"regbadscope-{suffix}",
			cancellationToken: TestCancellationToken);

		var response = await httpClient.PostAsJsonAsync(
			"/api/clients",
			new
			{
				clientName = "Test Client",
				clientType = "Public",
				redirectUris = new[] { "https://example.com/callback" },
				allowedScopes = new[] { "unknown_scope" }
			},
			TestCancellationToken);

		response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
	}

	[Fact]
	public async Task Register_WhenUnauthenticated_Returns401Unauthorized()
	{
		var httpClient = factory.CreateClient(new WebApplicationFactoryClientOptions
		{
			AllowAutoRedirect = false
		});

		var response = await httpClient.PostAsJsonAsync(
			"/api/clients",
			new
			{
				clientName = "Test Client",
				clientType = "Public",
				redirectUris = new[] { "https://example.com/callback" },
				allowedScopes = new[] { "openid" }
			},
			TestCancellationToken);

		response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
	}
}
