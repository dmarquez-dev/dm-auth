using DMAuth.Application.Common.Interfaces;
using DMAuth.Application.Common.Results;
using DMAuth.Application.Features.Clients.UpdateRegistration;
using DMAuth.Domain.Entities.Client;
using DMAuth.Domain.Enums;
using DMAuth.Domain.Interfaces;
using DMAuth.Tests.Unit.Common;
using FluentAssertions;
using NSubstitute;

namespace DMAuth.Tests.Unit.Application.Clients;

public class UpdateClientRegistrationHandlerTests
	: UnitTestBase
{
	private readonly IClientRepository _clientRepository = Substitute.For<IClientRepository>();
	private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
	private readonly UpdateClientRegistrationHandler _handler;

	public UpdateClientRegistrationHandlerTests()
	{
		_handler = new UpdateClientRegistrationHandler(_clientRepository, _unitOfWork);
	}

	[Fact]
	public async Task Handle_WhenClientNotFound_ReturnsNotFound()
	{
		_clientRepository.FindByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
			.Returns((Client?)null);

		var command = CreateCommand(Guid.NewGuid());

		var result = await _handler.Handle(command, TestCancellationToken);

		result.IsSuccess.Should().BeFalse();
		result.ErrorType.Should().Be(ResultError.NotFound);
	}

	[Fact]
	public async Task Handle_WhenClientBelongsToDifferentOwner_ReturnsForbidden()
	{
		var client = CreateClient(Guid.NewGuid());
		_clientRepository.FindByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
			.Returns(client);

		var command = CreateCommand(Guid.NewGuid());

		var result = await _handler.Handle(command, TestCancellationToken);

		result.IsSuccess.Should().BeFalse();
		result.ErrorType.Should().Be(ResultError.Forbidden);
	}

	[Fact]
	public async Task Handle_WhenRedirectUriIsInvalid_ReturnsInvalid()
	{
		var ownerId = Guid.NewGuid();
		_clientRepository.FindByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
			.Returns(CreateClient(ownerId));

		var command = new UpdateClientRegistrationCommand(
			Guid.NewGuid(),
			ownerId,
			"Updated Client",
			["http://example.com/callback"],  // non-HTTPS, non-localhost
			["openid"]);

		var result = await _handler.Handle(command, TestCancellationToken);

		result.IsSuccess.Should().BeFalse();
		result.ErrorType.Should().Be(ResultError.Invalid);
	}

	[Fact]
	public async Task Handle_WhenScopeIsInvalid_ReturnsInvalid()
	{
		var ownerId = Guid.NewGuid();
		_clientRepository.FindByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
			.Returns(CreateClient(ownerId));

		var command = new UpdateClientRegistrationCommand(
			Guid.NewGuid(),
			ownerId,
			"Updated Client",
			["https://example.com/callback"],
			["unknown_scope"]);

		var result = await _handler.Handle(command, TestCancellationToken);

		result.IsSuccess.Should().BeFalse();
		result.ErrorType.Should().Be(ResultError.Invalid);
	}

	[Fact]
	public async Task Handle_WithValidRequest_UpdatesClientRegistration()
	{
		var ownerId = Guid.NewGuid();
		var client = CreateClient(ownerId);
		_clientRepository.FindByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
			.Returns(client);

		var command = CreateCommand(ownerId);

		await _handler.Handle(command, TestCancellationToken);

		client.ClientName.Should().Be("Updated Client");
	}

	[Fact]
	public async Task Handle_WithValidRequest_SavesChanges()
	{
		var ownerId = Guid.NewGuid();
		_clientRepository.FindByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
			.Returns(CreateClient(ownerId));

		await _handler.Handle(CreateCommand(ownerId), TestCancellationToken);

		await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
	}

	[Fact]
	public async Task Handle_WithValidRequest_ReturnsSuccess()
	{
		var ownerId = Guid.NewGuid();
		_clientRepository.FindByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
			.Returns(CreateClient(ownerId));

		var result = await _handler.Handle(CreateCommand(ownerId), TestCancellationToken);

		result.IsSuccess.Should().BeTrue();
	}

	private static UpdateClientRegistrationCommand CreateCommand(Guid requestingUserId) =>
		new(
			Guid.NewGuid(),
			requestingUserId,
			"Updated Client",
			["https://example.com/callback"],
			["openid"]);

	private static Client CreateClient(Guid ownerId) =>
		new(
			"dma_testclientid",
			"Test Client",
			ClientType.Public,
			ownerId,
			["https://example.com/callback"],
			["openid"]);
}
