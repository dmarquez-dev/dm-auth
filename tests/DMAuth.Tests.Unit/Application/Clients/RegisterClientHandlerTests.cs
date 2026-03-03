using DMAuth.Application.Common.Interfaces;
using DMAuth.Application.Common.Results;
using DMAuth.Application.Features.Clients.Register;
using DMAuth.Domain.Entities.Client;
using DMAuth.Domain.Enums;
using DMAuth.Domain.Interfaces;
using DMAuth.Domain.ValueObjects;
using DMAuth.Tests.Unit.Common;
using FluentAssertions;
using NSubstitute;

namespace DMAuth.Tests.Unit.Application.Clients;

public class RegisterClientHandlerTests
	: UnitTestBase
{
	private readonly IClientRepository _clientRepository = Substitute.For<IClientRepository>();
	private readonly IPasswordHasher _passwordHasher = Substitute.For<IPasswordHasher>();
	private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
	private readonly RegisterClientHandler _handler;

	public RegisterClientHandlerTests()
	{
		_handler = new RegisterClientHandler(_clientRepository, _passwordHasher, _unitOfWork);

		_clientRepository.ExistsByClientIdAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
			.Returns(false);
	}

	[Fact]
	public async Task Handle_WhenRedirectUriIsInvalid_ReturnsInvalid()
	{
		var command = new RegisterClientCommand(
			Guid.NewGuid(),
			"Test Client",
			ClientType.Public,
			["http://example.com/callback"],  // non-HTTPS, non-localhost
			["openid"]);

		var result = await _handler.Handle(command, TestCancellationToken);

		result.IsSuccess.Should().BeFalse();
		result.ErrorType.Should().Be(ResultError.Invalid);
	}

	[Fact]
	public async Task Handle_WhenScopeIsInvalid_ReturnsInvalid()
	{
		var command = new RegisterClientCommand(
			Guid.NewGuid(),
			"Test Client",
			ClientType.Public,
			["https://example.com/callback"],
			["unknown_scope"]);

		var result = await _handler.Handle(command, TestCancellationToken);

		result.IsSuccess.Should().BeFalse();
		result.ErrorType.Should().Be(ResultError.Invalid);
	}

	[Fact]
	public async Task Handle_WithValidPublicClient_ReturnsSuccessWithNullSecret()
	{
		var command = CreatePublicClientCommand();

		var result = await _handler.Handle(command, TestCancellationToken);

		result.IsSuccess.Should().BeTrue();
		result.Value!.ClientSecret.Should().BeNull();
	}

	[Fact]
	public async Task Handle_WithValidConfidentialClient_ReturnsSuccessWithSecret()
	{
		_passwordHasher.Hash(Arg.Any<string>())
			.Returns(new HashedPassword("$2a$12$hashed"));

		var command = new RegisterClientCommand(
			Guid.NewGuid(),
			"Test Client",
			ClientType.Confidential,
			["https://example.com/callback"],
			["openid"]);

		var result = await _handler.Handle(command, TestCancellationToken);

		result.IsSuccess.Should().BeTrue();
		result.Value!.ClientSecret.Should().NotBeNull();
	}

	[Fact]
	public async Task Handle_WithValidRequest_ReturnsSuccessWithClientId()
	{
		var command = CreatePublicClientCommand();

		var result = await _handler.Handle(command, TestCancellationToken);

		result.IsSuccess.Should().BeTrue();
		result.Value!.ClientId.Should().NotBeEmpty();
		result.Value.OAuthClientId.Should().StartWith("dmauth_");
	}

	[Fact]
	public async Task Handle_WithValidRequest_AddsClientToRepository()
	{
		var command = CreatePublicClientCommand();

		await _handler.Handle(command, TestCancellationToken);

		_clientRepository.Received(1).Add(Arg.Any<Client>());
	}

	[Fact]
	public async Task Handle_WithValidRequest_SavesChanges()
	{
		var command = CreatePublicClientCommand();

		await _handler.Handle(command, TestCancellationToken);

		await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
	}

	private static RegisterClientCommand CreatePublicClientCommand() =>
		new(
			Guid.NewGuid(),
			"Test Client",
			ClientType.Public,
			["https://example.com/callback"],
			["openid"]);
}
