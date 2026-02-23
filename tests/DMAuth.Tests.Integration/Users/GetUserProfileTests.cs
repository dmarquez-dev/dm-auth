using DMAuth.Application.Features.Users.GetProfile;
using DMAuth.Tests.Integration.Common;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;
using System.Net.Http.Json;

namespace DMAuth.Tests.Integration.Users;

/// <summary>
///		Integration tests for GET /api/users/me.
/// </summary>
public class GetUserProfileTests(IntegrationTestFactory factory)
	: IntegrationTestBase, IClassFixture<IntegrationTestFactory>
{
	[Fact]
	public async Task GetProfile_WhenAuthenticated_Returns200WithProfileData()
	{
		var suffix = Guid.NewGuid().ToString("N")[..8];
		var email = $"profile-{suffix}@example.com";
		var username = $"profile-{suffix}";
		const string displayName = "Profile User";

		var client = await factory.CreateAuthenticatedClientAsync(
			email,
			username,
			displayName: displayName,
			cancellationToken: TestCancellationToken);

		var response = await client.GetAsync("/api/users/me", TestCancellationToken);

		response.StatusCode.Should().Be(HttpStatusCode.OK);

		var body = await response.Content.ReadFromJsonAsync<GetUserProfileResponse>(TestCancellationToken);
		body!.UserId.Should().NotBeEmpty();
		body.Email.Should().Be(email);
		body.Username.Should().Be(username);
		body.DisplayName.Should().Be(displayName);
		body.EmailVerified.Should().BeFalse();
	}

	[Fact]
	public async Task GetProfile_WhenUnauthenticated_Returns401Unauthorized()
	{
		var client = factory.CreateClient(new WebApplicationFactoryClientOptions
		{
			AllowAutoRedirect = false
		});

		var response = await client.GetAsync("/api/users/me", TestCancellationToken);

		response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
	}
}
