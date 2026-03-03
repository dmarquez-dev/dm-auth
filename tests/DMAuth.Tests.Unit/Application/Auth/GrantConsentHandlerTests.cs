using DMAuth.Application.Common.Interfaces;
using DMAuth.Application.Common.Results;
using DMAuth.Application.Features.Auth.GrantConsent;
using DMAuth.Domain.Entities.AuthorizationCode;
using DMAuth.Domain.Entities.Client;
using DMAuth.Domain.Entities.Consent;
using DMAuth.Domain.Enums;
using DMAuth.Domain.Interfaces;
using DMAuth.Tests.Unit.Common;
using FluentAssertions;
using NSubstitute;

namespace DMAuth.Tests.Unit.Application.Auth;

public class GrantConsentHandlerTests
	: UnitTestBase
{
	private readonly IClientRepository _clientRepository = Substitute.For<IClientRepository>();
	private readonly IConsentRepository _consentRepository = Substitute.For<IConsentRepository>();
	private readonly IAuthorizationCodeRepository _authorizationCodeRepository = Substitute.For<IAuthorizationCodeRepository>();
	private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
	private readonly GrantConsentHandler _handler;

	public GrantConsentHandlerTests()
	{
		_handler = new GrantConsentHandler(
			_clientRepository,
			_consentRepository,
			_authorizationCodeRepository,
			_unitOfWork);
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

		_consentRepository.FindByUserAndClientAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>())
			.Returns((Consent?)null);

		var result = await _handler.Handle(ValidCommand(), TestCancellationToken);

		result.ErrorType.Should().Be(ResultError.Forbidden);
	}

	[Fact]
	public async Task Handle_WhenNoExistingConsent_AddsNewConsent()
	{
		SetupActiveClient();

		_consentRepository.FindByUserAndClientAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>())
			.Returns((Consent?)null);

		await _handler.Handle(ValidCommand(), TestCancellationToken);

		_consentRepository.Received(1).Add(Arg.Any<Consent>());
	}

	[Fact]
	public async Task Handle_WhenExistingConsentCoversAllScopes_DoesNotUpdateConsent()
	{
		SetupActiveClient();

		var existingConsent = new Consent(Guid.NewGuid(), Guid.NewGuid(), "openid profile");

		_consentRepository.FindByUserAndClientAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>())
			.Returns(existingConsent);

		await _handler.Handle(ValidCommand(), TestCancellationToken);

		_consentRepository.DidNotReceive().Update(Arg.Any<Consent>());
	}

	[Fact]
	public async Task Handle_WhenExistingConsentMissingScopes_UpdatesConsent()
	{
		SetupActiveClient();

		var existingConsent = new Consent(Guid.NewGuid(), Guid.NewGuid(), "openid");

		_consentRepository.FindByUserAndClientAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>())
			.Returns(existingConsent);

		var command = ValidCommand() with { GrantedScopes = ["openid", "profile"] };

		await _handler.Handle(command, TestCancellationToken);

		_consentRepository.Received(1).Update(existingConsent);
	}

	[Fact]
	public async Task Handle_WithValidRequest_AddsAuthorizationCode()
	{
		SetupActiveClient();

		_consentRepository.FindByUserAndClientAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>())
			.Returns((Consent?)null);

		await _handler.Handle(ValidCommand(), TestCancellationToken);

		_authorizationCodeRepository.Received(1).Add(Arg.Any<AuthorizationCode>());
	}

	[Fact]
	public async Task Handle_WithValidRequest_SavesChanges()
	{
		SetupActiveClient();

		_consentRepository.FindByUserAndClientAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>())
			.Returns((Consent?)null);

		await _handler.Handle(ValidCommand(), TestCancellationToken);

		await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
	}

	[Fact]
	public async Task Handle_WithValidRequest_ReturnsSuccessWithPlainCode()
	{
		SetupActiveClient();

		_consentRepository.FindByUserAndClientAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>())
			.Returns((Consent?)null);

		var result = await _handler.Handle(ValidCommand(), TestCancellationToken);

		result.IsSuccess.Should().BeTrue();
		result.Value!.PlainCode.Should().NotBeNullOrEmpty();
	}

	private void SetupActiveClient()
	{
		_clientRepository.FindByClientIdAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
			.Returns(CreateActiveClient());
	}

	private static Client CreateActiveClient() =>
		new(
			"dmauth_test_client",
			"Test Client",
			ClientType.Public,
			Guid.NewGuid(),
			["https://example.com/callback"],
			["openid", "profile"]);

	private static GrantConsentCommand ValidCommand() =>
		new(
			Guid.NewGuid(),
			"dmauth_test_client",
			["openid", "profile"],
			"https://example.com/callback",
			"state_value",
			new string('A', 43),
			"S256");
}
