using DMAuth.Tests.Integration.Common;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;
using System.Net.Http.Json;

namespace DMAuth.Tests.Integration.Users;

/// <summary>
///		Integration tests for POST /api/users/me/change-password.
/// </summary>
public class ChangeUserPasswordTests(IntegrationTestFactory factory)
	: IntegrationTestBase, IClassFixture<IntegrationTestFactory>
{
	[Fact]
	public async Task ChangePassword_WithValidCredentials_Returns204NoContent()
	{
		var suffix = Guid.NewGuid().ToString("N")[..8];
		var client = await factory.CreateAuthenticatedClientAsync(
			$"changepw-{suffix}@example.com",
			$"changepw-{suffix}",
			cancellationToken: TestCancellationToken);

		var response = await client.PostAsJsonAsync(
			"/api/users/me/change-password",
			new { currentPassword = "Secure1!", newPassword = "NewSecure2@" },
			TestCancellationToken);

		response.StatusCode.Should().Be(HttpStatusCode.NoContent);
	}

	[Fact]
	public async Task ChangePassword_WithWrongCurrentPassword_Returns401Unauthorized()
	{
		var suffix = Guid.NewGuid().ToString("N")[..8];
		var client = await factory.CreateAuthenticatedClientAsync(
			$"wrongpw-{suffix}@example.com",
			$"wrongpw-{suffix}",
			cancellationToken: TestCancellationToken);

		var response = await client.PostAsJsonAsync(
			"/api/users/me/change-password",
			new { currentPassword = "WrongPassword1!", newPassword = "NewSecure2@" },
			TestCancellationToken);

		response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
	}

	[Fact]
	public async Task ChangePassword_WhenUnauthenticated_Returns401Unauthorized()
	{
		var client = factory.CreateClient(new WebApplicationFactoryClientOptions
		{
			AllowAutoRedirect = false
		});

		var response = await client.PostAsJsonAsync(
			"/api/users/me/change-password",
			new { currentPassword = "Secure1!", newPassword = "NewSecure2@" },
			TestCancellationToken);

		response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
	}
}
