using DMAuth.Application.Features.Users.Register;
using DMAuth.Tests.Integration.Common;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;
using System.Net.Http.Json;

namespace DMAuth.Tests.Integration.Users;

/// <summary>
///		Integration tests for POST /api/users/register.
/// </summary>
public class RegisterUserTests(IntegrationTestFactory factory)
	: IntegrationTestBase, IClassFixture<IntegrationTestFactory>
{
	[Fact]
	public async Task Register_WithValidData_Returns201Created()
	{
		var client = factory.CreateClient(new WebApplicationFactoryClientOptions
		{
			AllowAutoRedirect = false
		});

		var suffix = Guid.NewGuid().ToString("N")[..8];

		var response = await client.PostAsJsonAsync(
			"/api/users/register",
			new
			{
				email = $"user-{suffix}@example.com",
				username = $"user-{suffix}",
				password = "Secure1!",
				displayName = "Test User"
			},
			TestCancellationToken);

		response.StatusCode.Should().Be(HttpStatusCode.Created);

		var body = await response.Content.ReadFromJsonAsync<RegisterUserResponse>(TestCancellationToken);
		body!.UserId.Should().NotBeEmpty();
	}

	[Theory]
	[InlineData("", "validuser", "Secure1!", "Test User")]             // missing email
	[InlineData("not-an-email", "validuser", "Secure1!", "Test User")] // invalid email format
	[InlineData("user@example.com", "", "Secure1!", "Test User")]      // missing username
	[InlineData("user@example.com", "ab", "Secure1!", "Test User")]    // username too short
	[InlineData("user@example.com", "validuser", "", "Test User")]     // missing password
	[InlineData("user@example.com", "validuser", "weak", "Test User")] // password too short
	[InlineData("user@example.com", "validuser", "Secure1!", "")]      // missing display name
	public async Task Register_WithInvalidData_Returns400BadRequest(
		string email,
		string username,
		string password,
		string displayName)
	{
		var client = factory.CreateClient(new WebApplicationFactoryClientOptions
		{
			AllowAutoRedirect = false
		});

		var response = await client.PostAsJsonAsync(
			"/api/users/register",
			new { email, username, password, displayName },
			TestCancellationToken);

		response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
	}

	[Fact]
	public async Task Register_WithDuplicateEmail_Returns409Conflict()
	{
		var client = factory.CreateClient(new WebApplicationFactoryClientOptions
		{
			AllowAutoRedirect = false
		});

		var suffix = Guid.NewGuid().ToString("N")[..8];
		var email = $"dup-{suffix}@example.com";

		await client.PostAsJsonAsync(
			"/api/users/register",
			new
			{
				email,
				username = $"first-{suffix}",
				password = "Secure1!",
				displayName = "First"
			},
			TestCancellationToken);

		var response = await client.PostAsJsonAsync(
			"/api/users/register",
			new
			{
				email,
				username = $"second-{suffix}",
				password = "Secure1!",
				displayName = "Second"
			},
			TestCancellationToken);

		response.StatusCode.Should().Be(HttpStatusCode.Conflict);
	}

	[Fact]
	public async Task Register_WithDuplicateUsername_Returns409Conflict()
	{
		var client = factory.CreateClient(new WebApplicationFactoryClientOptions
		{
			AllowAutoRedirect = false
		});

		var suffix = Guid.NewGuid().ToString("N")[..8];
		var username = $"dup-{suffix}";

		await client.PostAsJsonAsync(
			"/api/users/register",
			new
			{
				email = $"first-{suffix}@example.com",
				username,
				password = "Secure1!",
				displayName = "First"
			},
			TestCancellationToken);

		var response = await client.PostAsJsonAsync(
			"/api/users/register",
			new
			{
				email = $"second-{suffix}@example.com",
				username,
				password = "Secure1!",
				displayName = "Second"
			},
			TestCancellationToken);

		response.StatusCode.Should().Be(HttpStatusCode.Conflict);
	}
}
