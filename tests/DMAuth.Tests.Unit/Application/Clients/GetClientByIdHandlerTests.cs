using DMAuth.Application.Common.Results;
using DMAuth.Application.Features.Clients.GetById;
using DMAuth.Domain.Entities.Client;
using DMAuth.Domain.Enums;
using DMAuth.Domain.Interfaces;
using DMAuth.Tests.Unit.Common;
using FluentAssertions;
using NSubstitute;

namespace DMAuth.Tests.Unit.Application.Clients;

public class GetClientByIdHandlerTests
	: UnitTestBase
{
	private readonly IClientRepository _clientRepository = Substitute.For<IClientRepository>();
	private readonly GetClientByIdHandler _handler;

	public GetClientByIdHandlerTests()
	{
		_handler = new GetClientByIdHandler(_clientRepository);
	}

	[Fact]
	public async Task Handle_WhenClientNotFound_ReturnsNotFound()
	{
		_clientRepository.FindByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
			.Returns((Client?)null);

		var query = new GetClientByIdQuery(Guid.NewGuid(), Guid.NewGuid());

		var result = await _handler.Handle(query, TestCancellationToken);

		result.IsSuccess.Should().BeFalse();
		result.ErrorType.Should().Be(ResultError.NotFound);
	}

	[Fact]
	public async Task Handle_WhenClientBelongsToDifferentOwner_ReturnsForbidden()
	{
		var client = CreateClient(ownerId: Guid.NewGuid());
		_clientRepository.FindByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
			.Returns(client);

		var query = new GetClientByIdQuery(Guid.NewGuid(), Guid.NewGuid());

		var result = await _handler.Handle(query, TestCancellationToken);

		result.IsSuccess.Should().BeFalse();
		result.ErrorType.Should().Be(ResultError.Forbidden);
	}

	[Fact]
	public async Task Handle_WhenClientBelongsToOwner_ReturnsSuccess()
	{
		var ownerId = Guid.NewGuid();
		var client = CreateClient(ownerId);
		_clientRepository.FindByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
			.Returns(client);

		var query = new GetClientByIdQuery(Guid.NewGuid(), ownerId);

		var result = await _handler.Handle(query, TestCancellationToken);

		result.IsSuccess.Should().BeTrue();
	}

	[Fact]
	public async Task Handle_WhenClientExists_ReturnsClientData()
	{
		var ownerId = Guid.NewGuid();
		var client = CreateClient(ownerId);
		_clientRepository.FindByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
			.Returns(client);

		var query = new GetClientByIdQuery(Guid.NewGuid(), ownerId);

		var result = await _handler.Handle(query, TestCancellationToken);

		result.Value!.OAuthClientId.Should().Be("dma_testclientid");
		result.Value.ClientName.Should().Be("Test Client");
		result.Value.ClientType.Should().Be(ClientType.Public);
		result.Value.OwnerId.Should().Be(ownerId);
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
