using DMAuth.Application.Features.Clients.GetById;
using DMAuth.Application.Features.Clients.Register;
using DMAuth.Domain.Enums;
using DMAuth.Tests.Integration.Common;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace DMAuth.Tests.Integration.Clients;

/// <summary>
///		Integration tests for GET /api/clients/{id}.
/// </summary>
public class GetClientTests(IntegrationTestFactory factory)
	: IntegrationTestBase, IClassFixture<IntegrationTestFactory>
{
	private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
	{
		Converters = { new JsonStringEnumConverter() }
	};

	[Fact]
	public async Task GetById_WhenClientExists_Returns200WithClientData()
	{
		var suffix = Guid.NewGuid().ToString("N")[..8];
		var httpClient = await factory.CreateAuthenticatedClientAsync(
			$"getbyid-{suffix}@example.com",
			$"getbyid-{suffix}",
			cancellationToken: TestCancellationToken);

		var registration = await CreateClientRegistrationAsync(httpClient);

		var response = await httpClient.GetAsync(
			$"/api/clients/{registration.ClientId}",
			TestCancellationToken);

		response.StatusCode.Should().Be(HttpStatusCode.OK);

		var body = await response.Content.ReadFromJsonAsync<GetClientByIdResponse>(
			JsonOptions,
			TestCancellationToken);

		body!.ClientId.Should().Be(registration.ClientId);
		body.OAuthClientId.Should().Be(registration.OAuthClientId);
		body.ClientName.Should().Be("Test Client");
		body.ClientType.Should().Be(ClientType.Public);
		body.IsActive.Should().BeTrue();
	}

	[Fact]
	public async Task GetById_WhenClientBelongsToDifferentOwner_Returns403Forbidden()
	{
		var suffixA = Guid.NewGuid().ToString("N")[..8];
		var httpClientA = await factory.CreateAuthenticatedClientAsync(
			$"getbyid-ownerA-{suffixA}@example.com",
			$"getbyid-ownerA-{suffixA}",
			cancellationToken: TestCancellationToken);

		var registration = await CreateClientRegistrationAsync(httpClientA);

		var suffixB = Guid.NewGuid().ToString("N")[..8];
		var httpClientB = await factory.CreateAuthenticatedClientAsync(
			$"getbyid-ownerB-{suffixB}@example.com",
			$"getbyid-ownerB-{suffixB}",
			cancellationToken: TestCancellationToken);

		var response = await httpClientB.GetAsync(
			$"/api/clients/{registration.ClientId}",
			TestCancellationToken);

		response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
	}

	[Fact]
	public async Task GetById_WhenClientNotFound_Returns404NotFound()
	{
		var suffix = Guid.NewGuid().ToString("N")[..8];
		var httpClient = await factory.CreateAuthenticatedClientAsync(
			$"getbyid-notfound-{suffix}@example.com",
			$"getbyid-notfound-{suffix}",
			cancellationToken: TestCancellationToken);

		var response = await httpClient.GetAsync(
			$"/api/clients/{Guid.NewGuid()}",
			TestCancellationToken);

		response.StatusCode.Should().Be(HttpStatusCode.NotFound);
	}

	[Fact]
	public async Task GetById_WhenUnauthenticated_Returns401Unauthorized()
	{
		var httpClient = factory.CreateClient(new WebApplicationFactoryClientOptions
		{
			AllowAutoRedirect = false
		});

		var response = await httpClient.GetAsync(
			$"/api/clients/{Guid.NewGuid()}",
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
