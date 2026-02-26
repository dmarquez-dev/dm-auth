using DMAuth.Application.Features.Clients.GetByOwner;
using DMAuth.Domain.Entities.Client;
using DMAuth.Domain.Enums;
using DMAuth.Domain.Interfaces;
using DMAuth.Tests.Unit.Common;
using FluentAssertions;
using NSubstitute;

namespace DMAuth.Tests.Unit.Application.Clients;

public class GetClientsByOwnerHandlerTests
	: UnitTestBase
{
	private readonly IClientRepository _clientRepository = Substitute.For<IClientRepository>();
	private readonly GetClientsByOwnerHandler _handler;

	public GetClientsByOwnerHandlerTests()
	{
		_handler = new GetClientsByOwnerHandler(_clientRepository);
	}

	[Fact]
	public async Task Handle_WhenOwnerHasNoClients_ReturnsSuccessWithEmptyList()
	{
		_clientRepository.FindByOwnerIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
			.Returns([]);

		var query = new GetClientsByOwnerQuery(Guid.NewGuid());

		var result = await _handler.Handle(query, TestCancellationToken);

		result.IsSuccess.Should().BeTrue();
		result.Value.Should().BeEmpty();
	}

	[Fact]
	public async Task Handle_WhenOwnerHasClients_ReturnsAllClients()
	{
		var ownerId = Guid.NewGuid();
		_clientRepository.FindByOwnerIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
			.Returns([CreateClient(ownerId), CreateClient(ownerId)]);

		var query = new GetClientsByOwnerQuery(ownerId);

		var result = await _handler.Handle(query, TestCancellationToken);

		result.IsSuccess.Should().BeTrue();
		result.Value!.Should().HaveCount(2);
	}

	[Fact]
	public async Task Handle_WhenOwnerHasClients_MapsClientDataCorrectly()
	{
		var ownerId = Guid.NewGuid();
		_clientRepository.FindByOwnerIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
			.Returns([CreateClient(ownerId)]);

		var query = new GetClientsByOwnerQuery(ownerId);

		var result = await _handler.Handle(query, TestCancellationToken);

		var client = result.Value!.Single();
		client.OAuthClientId.Should().Be("dma_testclientid");
		client.ClientName.Should().Be("Test Client");
		client.ClientType.Should().Be(ClientType.Public);
		client.IsActive.Should().BeTrue();
	}

	private static Client CreateClient(Guid ownerId) =>
		new(
			"dma_testclientid",
			"Test Client",
			ClientType.Public,
			ownerId,
			["https://example.com/callback"],
			["openid"]);
}
