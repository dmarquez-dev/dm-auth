using DMAuth.Application.Features.Clients.Register;
using DMAuth.Tests.Integration.Common;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;
using System.Net.Http.Json;

namespace DMAuth.Tests.Integration.Clients;

/// <summary>
///		Integration tests for DELETE /api/clients/{id}.
/// </summary>
public class DeleteClientTests(IntegrationTestFactory factory)
	: IntegrationTestBase, IClassFixture<IntegrationTestFactory>
{
	[Fact]
	public async Task Delete_WhenClientExists_Returns204NoContent()
	{
		var suffix = Guid.NewGuid().ToString("N")[..8];
		var httpClient = await factory.CreateAuthenticatedClientAsync(
			$"delete-ok-{suffix}@example.com",
			$"delete-ok-{suffix}",
			cancellationToken: TestCancellationToken);

		var registration = await CreateClientRegistrationAsync(httpClient);

		var response = await httpClient.DeleteAsync(
			$"/api/clients/{registration.ClientId}",
			TestCancellationToken);

		response.StatusCode.Should().Be(HttpStatusCode.NoContent);
	}

	[Fact]
	public async Task Delete_WhenClientBelongsToDifferentOwner_Returns403Forbidden()
	{
		var suffixA = Guid.NewGuid().ToString("N")[..8];
		var httpClientA = await factory.CreateAuthenticatedClientAsync(
			$"delete-ownerA-{suffixA}@example.com",
			$"delete-ownerA-{suffixA}",
			cancellationToken: TestCancellationToken);

		var registration = await CreateClientRegistrationAsync(httpClientA);

		var suffixB = Guid.NewGuid().ToString("N")[..8];
		var httpClientB = await factory.CreateAuthenticatedClientAsync(
			$"delete-ownerB-{suffixB}@example.com",
			$"delete-ownerB-{suffixB}",
			cancellationToken: TestCancellationToken);

		var response = await httpClientB.DeleteAsync(
			$"/api/clients/{registration.ClientId}",
			TestCancellationToken);

		response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
	}

	[Fact]
	public async Task Delete_WhenClientNotFound_Returns404NotFound()
	{
		var suffix = Guid.NewGuid().ToString("N")[..8];
		var httpClient = await factory.CreateAuthenticatedClientAsync(
			$"delete-notfound-{suffix}@example.com",
			$"delete-notfound-{suffix}",
			cancellationToken: TestCancellationToken);

		var response = await httpClient.DeleteAsync(
			$"/api/clients/{Guid.NewGuid()}",
			TestCancellationToken);

		response.StatusCode.Should().Be(HttpStatusCode.NotFound);
	}

	[Fact]
	public async Task Delete_WhenClientAlreadyDeactivated_Returns400BadRequest()
	{
		var suffix = Guid.NewGuid().ToString("N")[..8];
		var httpClient = await factory.CreateAuthenticatedClientAsync(
			$"delete-twice-{suffix}@example.com",
			$"delete-twice-{suffix}",
			cancellationToken: TestCancellationToken);

		var registration = await CreateClientRegistrationAsync(httpClient);

		await httpClient.DeleteAsync($"/api/clients/{registration.ClientId}", TestCancellationToken);

		var response = await httpClient.DeleteAsync(
			$"/api/clients/{registration.ClientId}",
			TestCancellationToken);

		response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
	}

	[Fact]
	public async Task Delete_WhenUnauthenticated_Returns401Unauthorized()
	{
		var httpClient = factory.CreateClient(new WebApplicationFactoryClientOptions
		{
			AllowAutoRedirect = false
		});

		var response = await httpClient.DeleteAsync(
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
