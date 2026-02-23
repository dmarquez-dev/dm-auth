using DMAuth.Application.Features.Users.Login;
using DMAuth.Tests.Integration.Common;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;
using System.Net.Http.Json;

namespace DMAuth.Tests.Integration.Users;

/// <summary>
///		Integration tests for POST /api/users/login.
/// </summary>
public class LoginUserTests(IntegrationTestFactory factory)
	: IntegrationTestBase, IClassFixture<IntegrationTestFactory>
{
	[Fact]
	public async Task Login_WithValidCredentials_Returns200OkWithUserDetails()
	{
		var client = factory.CreateClient(new WebApplicationFactoryClientOptions
		{
			AllowAutoRedirect = false
		});

		var suffix = Guid.NewGuid().ToString("N")[..8];
		var email = $"login-{suffix}@example.com";
		var username = $"login-{suffix}";
		const string password = "Secure1!";
		const string displayName = "Login User";

		await client.PostAsJsonAsync(
			"/api/users/register",
			new { email, username, password, displayName },
			TestCancellationToken);

		var response = await client.PostAsJsonAsync(
			"/api/users/login",
			new { email, password },
			TestCancellationToken);

		response.StatusCode.Should().Be(HttpStatusCode.OK);

		var body = await response.Content.ReadFromJsonAsync<LoginUserResponse>(TestCancellationToken);
		body!.UserId.Should().NotBeEmpty();
		body.Email.Should().Be(email);
		body.Username.Should().Be(username);
		body.DisplayName.Should().Be(displayName);
	}

	[Fact]
	public async Task Login_WithValidCredentials_SetsSessionCookie()
	{
		var client = factory.CreateClient(new WebApplicationFactoryClientOptions
		{
			AllowAutoRedirect = false
		});

		var suffix = Guid.NewGuid().ToString("N")[..8];
		var email = $"cookie-{suffix}@example.com";
		const string password = "Secure1!";

		await client.PostAsJsonAsync(
			"/api/users/register",
			new { email, username = $"cookie-{suffix}", password, displayName = "Cookie User" },
			TestCancellationToken);

		var response = await client.PostAsJsonAsync(
			"/api/users/login",
			new { email, password },
			TestCancellationToken);

		response.Headers.TryGetValues("Set-Cookie", out var cookies);
		cookies.Should().Contain(cookie =>
			cookie.Contains("dm_auth_session"));
	}

	[Fact]
	public async Task Login_WithWrongPassword_Returns401Unauthorized()
	{
		var client = factory.CreateClient(new WebApplicationFactoryClientOptions
		{
			AllowAutoRedirect = false
		});

		var suffix = Guid.NewGuid().ToString("N")[..8];
		var email = $"wrong-{suffix}@example.com";

		await client.PostAsJsonAsync(
			"/api/users/register",
			new { email, username = $"wrong-{suffix}", password = "Secure1!", displayName = "Wrong Pass" },
			TestCancellationToken);

		var response = await client.PostAsJsonAsync(
			"/api/users/login",
			new { email, password = "BadPassword1!" },
			TestCancellationToken);

		response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
	}

	[Fact]
	public async Task Login_WithUnknownEmail_Returns401Unauthorized()
	{
		var client = factory.CreateClient(new WebApplicationFactoryClientOptions
		{
			AllowAutoRedirect = false
		});

		var response = await client.PostAsJsonAsync(
			"/api/users/login",
			new { email = $"nobody-{Guid.NewGuid():N}@example.com", password = "Secure1!" },
			TestCancellationToken);

		response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
	}

	[Theory]
	[InlineData("", "Secure1!")]             // missing email
	[InlineData("not-an-email", "Secure1!")] // invalid email format
	[InlineData("user@example.com", "")]     // missing password
	public async Task Login_WithInvalidFields_Returns400BadRequest(
		string email,
		string password)
	{
		var client = factory.CreateClient(new WebApplicationFactoryClientOptions
		{
			AllowAutoRedirect = false
		});

		var response = await client.PostAsJsonAsync(
			"/api/users/login",
			new { email, password },
			TestCancellationToken);

		response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
	}
}
