using DMAuth.Domain.Entities.Client;
using DMAuth.Domain.Enums;
using DMAuth.Domain.Exceptions;
using FluentAssertions;

namespace DMAuth.Tests.Unit.Domain.Entities;

public class ClientTests
{
	private static readonly Guid TestOwnerId = Guid.NewGuid();

	private static Client CreateClient() =>
		new(
			"dma_testclientid",
			"Test Client",
			ClientType.Public,
			TestOwnerId,
			["https://example.com/callback"],
			["openid"]);

	[Fact]
	public void Constructor_WithValidArguments_SetsAllProperties()
	{
		var client = CreateClient();

		client.ClientId.Should().Be("dma_testclientid");
		client.ClientName.Should().Be("Test Client");
		client.ClientType.Should().Be(ClientType.Public);
		client.OwnerId.Should().Be(TestOwnerId);
		client.RedirectUris.Should().ContainSingle(uri => uri == "https://example.com/callback");
		client.AllowedScopes.Should().ContainSingle(scope => scope == "openid");
		client.ClientSecretHash.Should().BeNull();
	}

	[Fact]
	public void Constructor_SetsIsActiveTrue()
	{
		var client = CreateClient();

		client.IsActive.Should().BeTrue();
	}

	[Fact]
	public void Constructor_WithSecretHash_StoresSecretHash()
	{
		var client = new Client(
			"dma_testclientid",
			"Test Client",
			ClientType.Confidential,
			TestOwnerId,
			["https://example.com/callback"],
			["openid"],
			"$2a$12$somehashvalue");

		client.ClientSecretHash.Should().Be("$2a$12$somehashvalue");
	}

	[Fact]
	public void UpdateRegistration_ChangesClientName()
	{
		var client = CreateClient();

		client.UpdateRegistration(
			"Updated Client",
			["https://example.com/callback"],
			["openid"]);

		client.ClientName.Should().Be("Updated Client");
	}

	[Fact]
	public void UpdateRegistration_ChangesRedirectUris()
	{
		var client = CreateClient();

		client.UpdateRegistration(
			"Test Client",
			["https://new.example.com/callback"],
			["openid"]);

		client.RedirectUris.Should().ContainSingle(uri => uri == "https://new.example.com/callback");
	}

	[Fact]
	public void UpdateRegistration_ChangesAllowedScopes()
	{
		var client = CreateClient();

		client.UpdateRegistration(
			"Test Client",
			["https://example.com/callback"],
			["openid", "profile"]);

		client.AllowedScopes.Should().BeEquivalentTo(["openid", "profile"]);
	}

	[Fact]
	public void UpdateRegistration_SetsUpdatedAt()
	{
		var client = CreateClient();

		client.UpdateRegistration(
			"Updated Client",
			["https://example.com/callback"],
			["openid"]);

		client.UpdatedAt.Should().NotBeNull();
		client.UpdatedAt!.Value.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(1));
	}

	[Fact]
	public void Deactivate_SetsIsActiveFalse()
	{
		var client = CreateClient();

		client.Deactivate();

		client.IsActive.Should().BeFalse();
	}

	[Fact]
	public void Deactivate_SetsUpdatedAt()
	{
		var client = CreateClient();

		client.Deactivate();

		client.UpdatedAt.Should().NotBeNull();
		client.UpdatedAt!.Value.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(1));
	}

	[Fact]
	public void Deactivate_WhenAlreadyInactive_ThrowsDomainException()
	{
		var client = CreateClient();
		client.Deactivate();

		var act = () => client.Deactivate();

		act.Should().Throw<DomainException>()
			.WithMessage("Client is already inactive.");
	}
}
