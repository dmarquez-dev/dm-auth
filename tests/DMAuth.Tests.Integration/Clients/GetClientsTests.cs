using DMAuth.Application.Features.Clients.GetByOwner;
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
///		Integration tests for GET /api/clients.
/// </summary>
public class GetClientsTests(IntegrationTestFactory factory)
	: IntegrationTestBase, IClassFixture<IntegrationTestFactory>
{
	private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
	{
		Converters = { new JsonStringEnumConverter() }
	};

	[Fact]
	public async Task GetAll_WhenNoClientsRegistered_Returns200WithEmptyList()
	{
		var suffix = Guid.NewGuid().ToString("N")[..8];
		var httpClient = await factory.CreateAuthenticatedClientAsync(
			$"getall-empty-{suffix}@example.com",
			$"getall-empty-{suffix}",
			cancellationToken: TestCancellationToken);

		var response = await httpClient.GetAsync("/api/clients", TestCancellationToken);

		response.StatusCode.Should().Be(HttpStatusCode.OK);

		var body = await response.Content.ReadFromJsonAsync<List<GetClientsByOwnerResponse>>(
			JsonOptions,
			TestCancellationToken);

		body.Should().BeEmpty();
	}

	[Fact]
	public async Task GetAll_WhenClientsExist_ReturnsAllOwnedClients()
	{
		var suffix = Guid.NewGuid().ToString("N")[..8];
		var httpClient = await factory.CreateAuthenticatedClientAsync(
			$"getall-owned-{suffix}@example.com",
			$"getall-owned-{suffix}",
			cancellationToken: TestCancellationToken);

		await CreateClientRegistrationAsync(httpClient);
		await CreateClientRegistrationAsync(httpClient);

		var response = await httpClient.GetAsync("/api/clients", TestCancellationToken);

		var body = await response.Content.ReadFromJsonAsync<List<GetClientsByOwnerResponse>>(
			JsonOptions,
			TestCancellationToken);

		body!.Should().HaveCount(2);
	}

	[Fact]
	public async Task GetAll_WhenUnauthenticated_Returns401Unauthorized()
	{
		var httpClient = factory.CreateClient(new WebApplicationFactoryClientOptions
		{
			AllowAutoRedirect = false
		});

		var response = await httpClient.GetAsync("/api/clients", TestCancellationToken);

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
