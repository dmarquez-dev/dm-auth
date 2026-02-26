using DMAuth.Application.Features.Clients.GetById;
using DMAuth.Application.Features.Clients.Register;
using DMAuth.Tests.Integration.Common;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace DMAuth.Tests.Integration.Clients;

/// <summary>
///		Integration tests for PUT /api/clients/{id}.
/// </summary>
public class UpdateClientTests(IntegrationTestFactory factory)
	: IntegrationTestBase, IClassFixture<IntegrationTestFactory>
{
	private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
	{
		Converters = { new JsonStringEnumConverter() }
	};

	[Fact]
	public async Task Update_WithValidData_Returns204NoContent()
	{
		var suffix = Guid.NewGuid().ToString("N")[..8];
		var httpClient = await factory.CreateAuthenticatedClientAsync(
			$"update-ok-{suffix}@example.com",
			$"update-ok-{suffix}",
			cancellationToken: TestCancellationToken);

		var registration = await CreateClientRegistrationAsync(httpClient);

		var response = await httpClient.PutAsJsonAsync(
			$"/api/clients/{registration.ClientId}",
			new
			{
				clientName = "Updated Name",
				redirectUris = new[] { "https://example.com/callback" },
				allowedScopes = new[] { "openid" }
			},
			TestCancellationToken);

		response.StatusCode.Should().Be(HttpStatusCode.NoContent);
	}

	[Fact]
	public async Task Update_WithValidData_PersistsChange()
	{
		var suffix = Guid.NewGuid().ToString("N")[..8];
		var httpClient = await factory.CreateAuthenticatedClientAsync(
			$"update-persist-{suffix}@example.com",
			$"update-persist-{suffix}",
			cancellationToken: TestCancellationToken);

		var registration = await CreateClientRegistrationAsync(httpClient);

		await httpClient.PutAsJsonAsync(
			$"/api/clients/{registration.ClientId}",
			new
			{
				clientName = "Persisted Name",
				redirectUris = new[] { "https://example.com/callback" },
				allowedScopes = new[] { "openid" }
			},
			TestCancellationToken);

		var getResponse = await httpClient.GetAsync(
			$"/api/clients/{registration.ClientId}",
			TestCancellationToken);

		var body = await getResponse.Content.ReadFromJsonAsync<GetClientByIdResponse>(
			JsonOptions,
			TestCancellationToken);

		body!.ClientName.Should().Be("Persisted Name");
	}

	[Fact]
	public async Task Update_WhenClientBelongsToDifferentOwner_Returns403Forbidden()
	{
		var suffixA = Guid.NewGuid().ToString("N")[..8];
		var httpClientA = await factory.CreateAuthenticatedClientAsync(
			$"update-ownerA-{suffixA}@example.com",
			$"update-ownerA-{suffixA}",
			cancellationToken: TestCancellationToken);

		var registration = await CreateClientRegistrationAsync(httpClientA);

		var suffixB = Guid.NewGuid().ToString("N")[..8];
		var httpClientB = await factory.CreateAuthenticatedClientAsync(
			$"update-ownerB-{suffixB}@example.com",
			$"update-ownerB-{suffixB}",
			cancellationToken: TestCancellationToken);

		var response = await httpClientB.PutAsJsonAsync(
			$"/api/clients/{registration.ClientId}",
			new
			{
				clientName = "Unauthorized Update",
				redirectUris = new[] { "https://example.com/callback" },
				allowedScopes = new[] { "openid" }
			},
			TestCancellationToken);

		response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
	}

	[Fact]
	public async Task Update_WhenClientNotFound_Returns404NotFound()
	{
		var suffix = Guid.NewGuid().ToString("N")[..8];
		var httpClient = await factory.CreateAuthenticatedClientAsync(
			$"update-notfound-{suffix}@example.com",
			$"update-notfound-{suffix}",
			cancellationToken: TestCancellationToken);

		var response = await httpClient.PutAsJsonAsync(
			$"/api/clients/{Guid.NewGuid()}",
			new
			{
				clientName = "Updated Name",
				redirectUris = new[] { "https://example.com/callback" },
				allowedScopes = new[] { "openid" }
			},
			TestCancellationToken);

		response.StatusCode.Should().Be(HttpStatusCode.NotFound);
	}

	[Fact]
	public async Task Update_WithInvalidRedirectUri_Returns400BadRequest()
	{
		var suffix = Guid.NewGuid().ToString("N")[..8];
		var httpClient = await factory.CreateAuthenticatedClientAsync(
			$"update-baduri-{suffix}@example.com",
			$"update-baduri-{suffix}",
			cancellationToken: TestCancellationToken);

		var registration = await CreateClientRegistrationAsync(httpClient);

		var response = await httpClient.PutAsJsonAsync(
			$"/api/clients/{registration.ClientId}",
			new
			{
				clientName = "Updated Name",
				redirectUris = new[] { "http://example.com/callback" },  // non-HTTPS, non-localhost
				allowedScopes = new[] { "openid" }
			},
			TestCancellationToken);

		response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
	}

	[Fact]
	public async Task Update_WhenUnauthenticated_Returns401Unauthorized()
	{
		var httpClient = factory.CreateClient(new WebApplicationFactoryClientOptions
		{
			AllowAutoRedirect = false
		});

		var response = await httpClient.PutAsJsonAsync(
			$"/api/clients/{Guid.NewGuid()}",
			new
			{
				clientName = "Updated Name",
				redirectUris = new[] { "https://example.com/callback" },
				allowedScopes = new[] { "openid" }
			},
			TestCancellationToken);

		response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
	}

	private async Task<RegisterClientResponse> CreateClientRegistrationAsync(HttpClient httpClient)
	{
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

		return (await response.Content.ReadFromJsonAsync<RegisterClientResponse>(TestCancellationToken))!;
	}
}
