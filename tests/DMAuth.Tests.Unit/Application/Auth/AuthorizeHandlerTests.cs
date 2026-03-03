using DMAuth.Application.Common.Results;
using DMAuth.Application.Features.Auth.Authorize;
using DMAuth.Domain.Entities.Client;
using DMAuth.Domain.Enums;
using DMAuth.Domain.Interfaces;
using DMAuth.Tests.Unit.Common;
using FluentAssertions;
using NSubstitute;

namespace DMAuth.Tests.Unit.Application.Auth;

public class AuthorizeHandlerTests
	: UnitTestBase
{
	private readonly IClientRepository _clientRepository = Substitute.For<IClientRepository>();
	private readonly AuthorizeHandler _handler;

	public AuthorizeHandlerTests()
	{
		_handler = new AuthorizeHandler(_clientRepository);
	}

	[Fact]
	public async Task Handle_WhenResponseTypeIsNotCode_ReturnsInvalid()
	{
		var command = ValidCommand() with { ResponseType = "token" };

		var result = await _handler.Handle(command, TestCancellationToken);

		result.ErrorType.Should().Be(ResultError.Invalid);
	}

	[Fact]
	public async Task Handle_WhenClientNotFound_ReturnsNotFound()
	{
		_clientRepository.FindByClientIdAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
			.Returns((Client?)null);

		var result = await _handler.Handle(ValidCommand(), TestCancellationToken);

		result.ErrorType.Should().Be(ResultError.NotFound);
	}

	[Fact]
	public async Task Handle_WhenClientIsInactive_ReturnsForbidden()
	{
		var client = CreateActiveClient();
		client.Deactivate();

		_clientRepository.FindByClientIdAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
			.Returns(client);

		var result = await _handler.Handle(ValidCommand(), TestCancellationToken);

		result.ErrorType.Should().Be(ResultError.Forbidden);
	}

	[Fact]
	public async Task Handle_WhenRedirectUriNotRegistered_ReturnsInvalid()
	{
		_clientRepository.FindByClientIdAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
			.Returns(CreateActiveClient());

		var command = ValidCommand() with { RedirectUri = "https://other.example.com/callback" };

		var result = await _handler.Handle(command, TestCancellationToken);

		result.ErrorType.Should().Be(ResultError.Invalid);
	}

	[Fact]
	public async Task Handle_WhenScopeNotPermitted_ReturnsInvalid()
	{
		_clientRepository.FindByClientIdAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
			.Returns(CreateActiveClient());

		var command = ValidCommand() with { Scope = "openid unknown_scope" };

		var result = await _handler.Handle(command, TestCancellationToken);

		result.ErrorType.Should().Be(ResultError.Invalid);
	}

	[Fact]
	public async Task Handle_WhenMultipleScopesNotPermitted_ErrorListsAllViolatingScopes()
	{
		_clientRepository.FindByClientIdAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
			.Returns(CreateActiveClient());

		var command = ValidCommand() with { Scope = "openid scope_a scope_b" };

		var result = await _handler.Handle(command, TestCancellationToken);

		result.ErrorType.Should().Be(ResultError.Invalid);
		result.Error.Should().Contain("scope_a").And.Contain("scope_b");
	}

	[Fact]
	public async Task Handle_WhenCodeChallengeMethodIsNotS256_ReturnsInvalid()
	{
		_clientRepository.FindByClientIdAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
			.Returns(CreateActiveClient());

		var command = ValidCommand() with { CodeChallengeMethod = "plain" };

		var result = await _handler.Handle(command, TestCancellationToken);

		result.ErrorType.Should().Be(ResultError.Invalid);
	}

	[Fact]
	public async Task Handle_WhenCodeChallengeIsTooShort_ReturnsInvalid()
	{
		_clientRepository.FindByClientIdAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
			.Returns(CreateActiveClient());

		var command = ValidCommand() with { CodeChallenge = "tooshort" };

		var result = await _handler.Handle(command, TestCancellationToken);

		result.ErrorType.Should().Be(ResultError.Invalid);
	}

	[Fact]
	public async Task Handle_WithValidRequest_ReturnsSuccess()
	{
		_clientRepository.FindByClientIdAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
			.Returns(CreateActiveClient());

		var result = await _handler.Handle(ValidCommand(), TestCancellationToken);

		result.IsSuccess.Should().BeTrue();
	}

	[Fact]
	public async Task Handle_WithValidRequest_ReturnsClientDetails()
	{
		_clientRepository.FindByClientIdAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
			.Returns(CreateActiveClient());

		var result = await _handler.Handle(ValidCommand(), TestCancellationToken);

		result.Value!.OAuthClientId.Should().Be("dmauth_test_client");
		result.Value.ClientName.Should().Be("Test Client");
		result.Value.RedirectUri.Should().Be("https://example.com/callback");
	}

	private static Client CreateActiveClient() =>
		new(
			"dmauth_test_client",
			"Test Client",
			ClientType.Public,
			Guid.NewGuid(),
			["https://example.com/callback"],
			["openid", "profile"]);

	private static AuthorizeCommand ValidCommand() =>
		new(
			"dmauth_test_client",
			"https://example.com/callback",
			"code",
			"openid",
			"state_value",
			new string('A', 43),
			"S256");
}
