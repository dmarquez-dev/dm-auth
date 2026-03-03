using System.Buffers.Text;
using System.Security.Cryptography;
using System.Text;
using DMAuth.Application.Common.Interfaces;
using DMAuth.Application.Common.Results;
using DMAuth.Application.Features.Auth.ExchangeToken;
using DMAuth.Domain.Entities.AuthorizationCode;
using DMAuth.Domain.Entities.Client;
using DMAuth.Domain.Entities.RefreshToken;
using DMAuth.Domain.Entities.User;
using DMAuth.Domain.Enums;
using DMAuth.Domain.Interfaces;
using DMAuth.Domain.ValueObjects;
using DMAuth.Tests.Unit.Common;
using FluentAssertions;
using NSubstitute;

namespace DMAuth.Tests.Unit.Application.Auth;

public class ExchangeTokenHandlerTests
	: UnitTestBase
{
	private readonly IClientRepository _clientRepository = Substitute.For<IClientRepository>();
	private readonly IAuthorizationCodeRepository _authCodeRepository = Substitute.For<IAuthorizationCodeRepository>();
	private readonly IRefreshTokenRepository _refreshTokenRepository = Substitute.For<IRefreshTokenRepository>();
	private readonly IUserRepository _userRepository = Substitute.For<IUserRepository>();
	private readonly ITokenService _tokenService = Substitute.For<ITokenService>();
	private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
	private readonly ExchangeTokenHandler _handler;
	private readonly Client _activeClient = CreateActiveClient();

	public ExchangeTokenHandlerTests()
	{
		_tokenService.GenerateAccessToken(Arg.Any<Guid>(), Arg.Any<string>(), Arg.Any<string>())
			.Returns("access_token");

		_tokenService.GenerateRefreshToken()
			.Returns(("plain_refresh_token", "hashed_refresh_token"));

		_handler = new ExchangeTokenHandler(
			_clientRepository,
			_authCodeRepository,
			_refreshTokenRepository,
			_userRepository,
			_tokenService,
			_unitOfWork);
	}

	[Fact]
	public async Task Handle_WhenGrantTypeIsNotAuthorizationCode_ReturnsInvalid()
	{
		var command = ValidCommand() with { GrantType = "implicit" };

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
	public async Task Handle_WhenCodeNotFound_ReturnsInvalid()
	{
		SetupActiveClient();

		_authCodeRepository.FindByCodeHashAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
			.Returns((AuthorizationCode?)null);

		var result = await _handler.Handle(ValidCommand(), TestCancellationToken);

		result.ErrorType.Should().Be(ResultError.Invalid);
	}

	[Fact]
	public async Task Handle_WhenCodeClientIdMismatch_ReturnsInvalid()
	{
		SetupActiveClient();

		var authCode = CreateAuthCode(clientId: Guid.NewGuid());

		_authCodeRepository.FindByCodeHashAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
			.Returns(authCode);

		var result = await _handler.Handle(ValidCommand(), TestCancellationToken);

		result.ErrorType.Should().Be(ResultError.Invalid);
	}

	[Fact]
	public async Task Handle_WhenRedirectUriMismatch_ReturnsInvalid()
	{
		SetupActiveClient();

		var authCode = CreateAuthCode(redirectUri: "https://other.example.com/callback");

		_authCodeRepository.FindByCodeHashAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
			.Returns(authCode);

		var result = await _handler.Handle(ValidCommand(), TestCancellationToken);

		result.ErrorType.Should().Be(ResultError.Invalid);
	}

	[Fact]
	public async Task Handle_WhenCodeExpired_ReturnsInvalid()
	{
		SetupActiveClient();

		var authCode = CreateAuthCode(expiresAt: DateTimeOffset.UtcNow.AddMinutes(-1));

		_authCodeRepository.FindByCodeHashAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
			.Returns(authCode);

		var result = await _handler.Handle(ValidCommand(), TestCancellationToken);

		result.ErrorType.Should().Be(ResultError.Invalid);
	}

	[Fact]
	public async Task Handle_WhenCodeAlreadyUsed_RevokesTokenFamilyAndReturnsUnauthorized()
	{
		SetupActiveClient();

		var authCode = CreateAuthCode();
		authCode.MarkAsUsed();

		_authCodeRepository.FindByCodeHashAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
			.Returns(authCode);

		var result = await _handler.Handle(ValidCommand(), TestCancellationToken);

		await _refreshTokenRepository.Received(1).RevokeByTokenFamilyAsync(
			authCode.Id,
			Arg.Any<CancellationToken>());

		result.ErrorType.Should().Be(ResultError.Unauthorized);
	}

	[Fact]
	public async Task Handle_WhenCodeVerifierMismatch_ReturnsInvalid()
	{
		SetupActiveClient();

		var authCode = CreateAuthCode();

		_authCodeRepository.FindByCodeHashAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
			.Returns(authCode);

		var command = ValidCommand() with { CodeVerifier = new string('B', 43) };

		var result = await _handler.Handle(command, TestCancellationToken);

		result.ErrorType.Should().Be(ResultError.Invalid);
	}

	[Fact]
	public async Task Handle_WithValidRequest_ReturnsSuccessWithAccessToken()
	{
		SetupActiveClient();
		SetupValidAuthCode();

		var result = await _handler.Handle(ValidCommand(), TestCancellationToken);

		result.IsSuccess.Should().BeTrue();
		result.Value!.AccessToken.Should().Be("access_token");
	}

	[Fact]
	public async Task Handle_WhenOpenIdScopeGranted_ReturnsIdToken()
	{
		SetupActiveClient();
		SetupValidAuthCode(scopes: "openid profile");
		SetupUser();

		_tokenService.GenerateIdToken(
			Arg.Any<Guid>(),
			Arg.Any<string>(),
			Arg.Any<DateTimeOffset>(),
			Arg.Any<string?>(),
			Arg.Any<string?>(),
			Arg.Any<string?>(),
			Arg.Any<bool?>())
			.Returns("id_token");

		var result = await _handler.Handle(ValidCommand(), TestCancellationToken);

		result.IsSuccess.Should().BeTrue();
		result.Value!.IdToken.Should().Be("id_token");
	}

	[Fact]
	public async Task Handle_WhenOpenIdScopeGranted_AndUserNotFound_ReturnsNotFound()
	{
		SetupActiveClient();
		SetupValidAuthCode(scopes: "openid");

		_userRepository.FindByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
			.Returns((User?)null);

		var result = await _handler.Handle(ValidCommand(), TestCancellationToken);

		result.ErrorType.Should().Be(ResultError.NotFound);
	}

	[Fact]
	public async Task Handle_WhenOfflineAccessScopeGranted_AddsRefreshTokenAndReturnsIt()
	{
		SetupActiveClient();
		SetupValidAuthCode(scopes: "openid offline_access");
		SetupUser();

		_tokenService.GenerateIdToken(
			Arg.Any<Guid>(),
			Arg.Any<string>(),
			Arg.Any<DateTimeOffset>(),
			Arg.Any<string?>(),
			Arg.Any<string?>(),
			Arg.Any<string?>(),
			Arg.Any<bool?>())
			.Returns("id_token");

		var result = await _handler.Handle(ValidCommand(), TestCancellationToken);

		result.IsSuccess.Should().BeTrue();
		result.Value!.RefreshToken.Should().Be("plain_refresh_token");
		_refreshTokenRepository.Received(1).Add(Arg.Any<RefreshToken>());
	}

	[Fact]
	public async Task Handle_WhenOfflineAccessScopeNotGranted_DoesNotAddRefreshToken()
	{
		SetupActiveClient();
		SetupValidAuthCode(scopes: "openid profile");
		SetupUser();

		_tokenService.GenerateIdToken(
			Arg.Any<Guid>(),
			Arg.Any<string>(),
			Arg.Any<DateTimeOffset>(),
			Arg.Any<string?>(),
			Arg.Any<string?>(),
			Arg.Any<string?>(),
			Arg.Any<bool?>())
			.Returns("id_token");

		var result = await _handler.Handle(ValidCommand(), TestCancellationToken);

		result.Value!.RefreshToken.Should().BeNull();
		_refreshTokenRepository.DidNotReceive().Add(Arg.Any<RefreshToken>());
	}

	[Fact]
	public async Task Handle_WithValidRequest_SavesChanges()
	{
		SetupActiveClient();
		SetupValidAuthCode();

		await _handler.Handle(ValidCommand(), TestCancellationToken);

		await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
	}

	private void SetupActiveClient()
	{
		_clientRepository.FindByClientIdAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
			.Returns(_activeClient);
	}

	private void SetupValidAuthCode(string scopes = "profile")
	{
		var authCode = CreateAuthCode(scopes: scopes);

		_authCodeRepository.FindByCodeHashAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
			.Returns(authCode);
	}

	private void SetupUser()
	{
		_userRepository.FindByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
			.Returns(CreateUser());
	}

	private static Client CreateActiveClient() =>
		new(
			"dmauth_test_client",
			"Test Client",
			ClientType.Public,
			Guid.NewGuid(),
			["https://example.com/callback"],
			["openid", "profile", "email", "offline_access"]);

	private AuthorizationCode CreateAuthCode(
		Guid? clientId = null,
		string redirectUri = "https://example.com/callback",
		string scopes = "profile",
		DateTimeOffset? expiresAt = null) =>
		new(
			HashValue(PlainCode),
			Guid.NewGuid(),
			clientId ?? _activeClient.Id,
			redirectUri,
			scopes,
			new CodeChallenge(HashValue(CodeVerifier)),
			CodeChallengeMethod.S256,
			expiresAt ?? DateTimeOffset.UtcNow.AddMinutes(5));

	private static User CreateUser() =>
		new(
			new Email("user@example.com"),
			"testuser",
			new HashedPassword("$2a$12$examplehashedpasswordvalue"),
			"Test User");

	private static ExchangeTokenCommand ValidCommand() =>
		new(
			"authorization_code",
			PlainCode,
			"dmauth_test_client",
			"https://example.com/callback",
			CodeVerifier);

	private static string HashValue(string value) =>
		Base64Url.EncodeToString(SHA256.HashData(Encoding.UTF8.GetBytes(value)));

	private const string PlainCode = "test_plain_code";
	private static readonly string CodeVerifier = new('v', 43);
}
