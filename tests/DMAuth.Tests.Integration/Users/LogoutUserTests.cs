using DMAuth.Tests.Integration.Common;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;

namespace DMAuth.Tests.Integration.Users;

/// <summary>
///		Integration tests for POST /api/users/logout.
/// </summary>
public class LogoutUserTests(IntegrationTestFactory factory)
	: IntegrationTestBase, IClassFixture<IntegrationTestFactory>
{
	[Fact]
	public async Task Logout_WhenAuthenticated_Returns204NoContent()
	{
		var suffix = Guid.NewGuid().ToString("N")[..8];
		var client = await factory.CreateAuthenticatedClientAsync(
			$"logout-{suffix}@example.com",
			$"logout-{suffix}",
			cancellationToken: TestCancellationToken);

		var response = await client.PostAsync("/api/users/logout", null, TestCancellationToken);

		response.StatusCode.Should().Be(HttpStatusCode.NoContent);
	}

	[Fact]
	public async Task Logout_WhenUnauthenticated_Returns401Unauthorized()
	{
		var client = factory.CreateClient(new WebApplicationFactoryClientOptions
		{
			AllowAutoRedirect = false
		});

		var response = await client.PostAsync("/api/users/logout", null, TestCancellationToken);

		response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
	}
}
