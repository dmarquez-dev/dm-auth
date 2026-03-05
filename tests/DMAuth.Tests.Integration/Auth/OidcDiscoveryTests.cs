using System.Text.Json;
using DMAuth.Tests.Integration.Common;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;
using System.Net.Http.Json;

namespace DMAuth.Tests.Integration.Auth;

/// <summary>
///		Integration tests for GET /.well-known/openid-configuration.
/// </summary>
public class OidcDiscoveryTests(IntegrationTestFactory factory)
	: IntegrationTestBase, IClassFixture<IntegrationTestFactory>
{
	[Fact]
	public async Task Discovery_Returns200Ok()
	{
		var client = factory.CreateClient(new WebApplicationFactoryClientOptions
		{
			AllowAutoRedirect = false
		});

		var response = await client.GetAsync(
			"/.well-known/openid-configuration",
			TestCancellationToken);

		response.StatusCode.Should().Be(HttpStatusCode.OK);
	}

	[Fact]
	public async Task Discovery_Issuer_MatchesTestConfiguration()
	{
		var client = factory.CreateClient(new WebApplicationFactoryClientOptions
		{
			AllowAutoRedirect = false
		});

		var json = await client.GetFromJsonAsync<JsonElement>(
			"/.well-known/openid-configuration",
			TestCancellationToken);

		json.GetProperty("issuer").GetString()
			.Should().Be("https://test.dmauth.local");
	}

	[Fact]
	public async Task Discovery_AllEndpoints_AreRelativeToIssuer()
	{
		var client = factory.CreateClient(new WebApplicationFactoryClientOptions
		{
			AllowAutoRedirect = false
		});

		var json = await client.GetFromJsonAsync<JsonElement>(
			"/.well-known/openid-configuration",
			TestCancellationToken);

		const string issuer = "https://test.dmauth.local";

		json.GetProperty("authorization_endpoint").GetString()
			.Should().Be($"{issuer}/connect/authorize");
		json.GetProperty("token_endpoint").GetString()
			.Should().Be($"{issuer}/connect/token");
		json.GetProperty("userinfo_endpoint").GetString()
			.Should().Be($"{issuer}/connect/userinfo");
		json.GetProperty("jwks_uri").GetString()
			.Should().Be($"{issuer}/.well-known/jwks.json");
		json.GetProperty("revocation_endpoint").GetString()
			.Should().Be($"{issuer}/connect/revoke");
	}

	[Fact]
	public async Task Discovery_ResponseTypes_GrantTypes_AndMethods_MatchSupportedFlows()
	{
		var client = factory.CreateClient(new WebApplicationFactoryClientOptions
		{
			AllowAutoRedirect = false
		});

		var json = await client.GetFromJsonAsync<JsonElement>(
			"/.well-known/openid-configuration",
			TestCancellationToken);

		json.GetProperty("response_types_supported")
			.EnumerateArray().Select(e => e.GetString())
			.Should().BeEquivalentTo(["code"]);

		json.GetProperty("grant_types_supported")
			.EnumerateArray().Select(e => e.GetString())
			.Should().BeEquivalentTo(["authorization_code", "refresh_token"]);

		json.GetProperty("subject_types_supported")
			.EnumerateArray().Select(e => e.GetString())
			.Should().BeEquivalentTo(["public"]);

		json.GetProperty("id_token_signing_alg_values_supported")
			.EnumerateArray().Select(e => e.GetString())
			.Should().BeEquivalentTo(["RS256"]);

		json.GetProperty("token_endpoint_auth_methods_supported")
			.EnumerateArray().Select(e => e.GetString())
			.Should().BeEquivalentTo(["none"]);

		json.GetProperty("code_challenge_methods_supported")
			.EnumerateArray().Select(e => e.GetString())
			.Should().BeEquivalentTo(["S256"]);
	}

	[Fact]
	public async Task Discovery_ScopesSupported_ContainsAllOidcScopes()
	{
		var client = factory.CreateClient(new WebApplicationFactoryClientOptions
		{
			AllowAutoRedirect = false
		});

		var json = await client.GetFromJsonAsync<JsonElement>(
			"/.well-known/openid-configuration",
			TestCancellationToken);

		var scopes = json.GetProperty("scopes_supported")
			.EnumerateArray()
			.Select(e => e.GetString())
			.ToList();

		scopes.Should()
			.Contain("openid")
			.And.Contain("profile")
			.And.Contain("email")
			.And.Contain("offline_access");
	}
}
