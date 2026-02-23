using DMAuth.Application.Features.Users.GetProfile;
using DMAuth.Tests.Integration.Common;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;
using System.Net.Http.Json;

namespace DMAuth.Tests.Integration.Users;

/// <summary>
///		Integration tests for PUT /api/users/me.
/// </summary>
public class UpdateUserProfileTests(IntegrationTestFactory factory)
	: IntegrationTestBase, IClassFixture<IntegrationTestFactory>
{
	[Fact]
	public async Task UpdateProfile_WithValidDisplayName_Returns204NoContent()
	{
		var suffix = Guid.NewGuid().ToString("N")[..8];
		var client = await factory.CreateAuthenticatedClientAsync(
			$"update-{suffix}@example.com",
			$"update-{suffix}",
			cancellationToken: TestCancellationToken);

		var response = await client.PutAsJsonAsync(
			"/api/users/me",
			new { displayName = "Updated Name" },
			TestCancellationToken);

		response.StatusCode.Should().Be(HttpStatusCode.NoContent);
	}

	[Fact]
	public async Task UpdateProfile_WithValidDisplayName_PersistsChange()
	{
		var suffix = Guid.NewGuid().ToString("N")[..8];
		var client = await factory.CreateAuthenticatedClientAsync(
			$"persist-{suffix}@example.com",
			$"persist-{suffix}",
			cancellationToken: TestCancellationToken);

		await client.PutAsJsonAsync(
			"/api/users/me",
			new { displayName = "Persisted Name" },
			TestCancellationToken);

		var response = await client.GetAsync("/api/users/me", TestCancellationToken);
		var body = await response.Content.ReadFromJsonAsync<GetUserProfileResponse>(TestCancellationToken);

		body!.DisplayName.Should().Be("Persisted Name");
	}

	[Fact]
	public async Task UpdateProfile_WithEmptyDisplayName_Returns400BadRequest()
	{
		var suffix = Guid.NewGuid().ToString("N")[..8];
		var client = await factory.CreateAuthenticatedClientAsync(
			$"badupdate-{suffix}@example.com",
			$"badupdate-{suffix}",
			cancellationToken: TestCancellationToken);

		var response = await client.PutAsJsonAsync(
			"/api/users/me",
			new { displayName = "" },
			TestCancellationToken);

		response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
	}

	[Fact]
	public async Task UpdateProfile_WhenUnauthenticated_Returns401Unauthorized()
	{
		var client = factory.CreateClient(new WebApplicationFactoryClientOptions
		{
			AllowAutoRedirect = false
		});

		var response = await client.PutAsJsonAsync(
			"/api/users/me",
			new { displayName = "Should Fail" },
			TestCancellationToken);

		response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
	}
}
