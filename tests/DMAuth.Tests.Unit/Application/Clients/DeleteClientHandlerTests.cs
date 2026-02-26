using DMAuth.Application.Common.Interfaces;
using DMAuth.Application.Common.Results;
using DMAuth.Application.Features.Clients.Delete;
using DMAuth.Domain.Entities.Client;
using DMAuth.Domain.Enums;
using DMAuth.Domain.Interfaces;
using DMAuth.Tests.Unit.Common;
using FluentAssertions;
using NSubstitute;

namespace DMAuth.Tests.Unit.Application.Clients;

public class DeleteClientHandlerTests
	: UnitTestBase
{
	private readonly IClientRepository _clientRepository = Substitute.For<IClientRepository>();
	private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
	private readonly DeleteClientHandler _handler;

	public DeleteClientHandlerTests()
	{
		_handler = new DeleteClientHandler(_clientRepository, _unitOfWork);
	}

	[Fact]
	public async Task Handle_WhenClientNotFound_ReturnsNotFound()
	{
		_clientRepository.FindByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
			.Returns((Client?)null);

		var command = new DeleteClientCommand(Guid.NewGuid(), Guid.NewGuid());

		var result = await _handler.Handle(command, TestCancellationToken);

		result.IsSuccess.Should().BeFalse();
		result.ErrorType.Should().Be(ResultError.NotFound);
	}

	[Fact]
	public async Task Handle_WhenClientBelongsToDifferentOwner_ReturnsForbidden()
	{
		var client = CreateClient(ownerId: Guid.NewGuid());
		_clientRepository.FindByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
			.Returns(client);

		var command = new DeleteClientCommand(Guid.NewGuid(), Guid.NewGuid());

		var result = await _handler.Handle(command, TestCancellationToken);

		result.IsSuccess.Should().BeFalse();
		result.ErrorType.Should().Be(ResultError.Forbidden);
	}

	[Fact]
	public async Task Handle_WithValidRequest_DeactivatesClient()
	{
		var ownerId = Guid.NewGuid();
		var client = CreateClient(ownerId);
		_clientRepository.FindByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
			.Returns(client);

		await _handler.Handle(new DeleteClientCommand(Guid.NewGuid(), ownerId), TestCancellationToken);

		client.IsActive.Should().BeFalse();
	}

	[Fact]
	public async Task Handle_WithValidRequest_SavesChanges()
	{
		var ownerId = Guid.NewGuid();
		_clientRepository.FindByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
			.Returns(CreateClient(ownerId));

		await _handler.Handle(new DeleteClientCommand(Guid.NewGuid(), ownerId), TestCancellationToken);

		await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
	}

	[Fact]
	public async Task Handle_WithValidRequest_ReturnsSuccess()
	{
		var ownerId = Guid.NewGuid();
		_clientRepository.FindByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
			.Returns(CreateClient(ownerId));

		var result = await _handler.Handle(new DeleteClientCommand(Guid.NewGuid(), ownerId), TestCancellationToken);

		result.IsSuccess.Should().BeTrue();
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
