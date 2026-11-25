using System.Buffers.Text;
using System.Security.Cryptography;
using System.Text;
using DMAuth.Application.Common.Interfaces;
using DMAuth.Application.Common.Results;
using DMAuth.Application.Features.Auth.RotateToken;
using DMAuth.Domain.Entities.Client;
using DMAuth.Domain.Entities.RefreshToken;
using DMAuth.Domain.Enums;
using DMAuth.Domain.Interfaces;
using DMAuth.Domain.ValueObjects;
using DMAuth.Tests.Unit.Common;
using FluentAssertions;
using NSubstitute;

namespace DMAuth.Tests.Unit.Application.Auth;

public class RotateTokenHandlerTests
	: UnitTestBase
{
	private readonly IClientRepository _clientRepository = Substitute.For<IClientRepository>();
	private readonly IRefreshTokenRepository _refreshTokenRepository = Substitute.For<IRefreshTokenRepository>();
	private readonly ITokenService _tokenService = Substitute.For<ITokenService>();
	private readonly IPasswordHasher _passwordHasher = Substitute.For<IPasswordHasher>();
	private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
	private readonly RotateTokenHandler _handler;
	private readonly Client _activeClient = CreateActiveClient();

	public RotateTokenHandlerTests()
	{
		_tokenService.GenerateAccessToken(Arg.Any<Guid>(), Arg.Any<string>(), Arg.Any<string>())
			.Returns("access_token");

		_tokenService.GenerateRefreshToken()
			.Returns(("new_plain_refresh_token", "new_hashed_refresh_token"));

		_handler = new RotateTokenHandler(
			_clientRepository,
			_refreshTokenRepository,
			_tokenService,
			_passwordHasher,
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

		var result = await _handler.Handle(ValidCommand(), TestCancellationToken);

		result.ErrorType.Should().Be(ResultError.Forbidden);
	}

	[Fact]
	public async Task Handle_WhenTokenNotFound_ReturnsUnauthorized()
	{
		SetupActiveClient();

		_refreshTokenRepository.FindByTokenHashAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
			.Returns((RefreshToken?)null);

		var result = await _handler.Handle(ValidCommand(), TestCancellationToken);

		result.ErrorType.Should().Be(ResultError.Unauthorized);
	}

	[Fact]
	public async Task Handle_WhenTokenAlreadyRevoked_RevokesTokenFamilyAndReturnsUnauthorized()
	{
		SetupActiveClient();

		var token = CreateActiveRefreshToken();
		token.Revoke();

		_refreshTokenRepository.FindByTokenHashAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
			.Returns(token);

		var result = await _handler.Handle(ValidCommand(), TestCancellationToken);

		await _refreshTokenRepository.Received(1).RevokeByTokenFamilyAsync(
			token.FamilyId,
			Arg.Any<CancellationToken>());

		result.ErrorType.Should().Be(ResultError.Unauthorized);
	}

	[Fact]
	public async Task Handle_WhenTokenClientIdMismatch_ReturnsUnauthorized()
	{
		SetupActiveClient();

		var token = CreateActiveRefreshToken(clientId: Guid.NewGuid());

		_refreshTokenRepository.FindByTokenHashAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
			.Returns(token);

		var result = await _handler.Handle(ValidCommand(), TestCancellationToken);

		result.ErrorType.Should().Be(ResultError.Unauthorized);
	}

	[Fact]
	public async Task Handle_WhenTokenExpired_ReturnsUnauthorized()
	{
		SetupActiveClient();

		var token = CreateActiveRefreshToken(expiresAt: DateTimeOffset.UtcNow.AddDays(-1));

		_refreshTokenRepository.FindByTokenHashAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
			.Returns(token);

		var result = await _handler.Handle(ValidCommand(), TestCancellationToken);

		result.ErrorType.Should().Be(ResultError.Unauthorized);
	}

	[Fact]
	public async Task Handle_WithValidRequest_ReturnsSuccessWithNewTokens()
	{
		SetupActiveClient();
		SetupActiveRefreshToken();

		var result = await _handler.Handle(ValidCommand(), TestCancellationToken);

		result.IsSuccess.Should().BeTrue();
		result.Value!.AccessToken.Should().Be("access_token");
		result.Value.RefreshToken.Should().Be("new_plain_refresh_token");
	}

	[Fact]
	public async Task Handle_WithValidRequest_DoesNotReturnIdToken()
	{
		SetupActiveClient();
		SetupActiveRefreshToken();

		var result = await _handler.Handle(ValidCommand(), TestCancellationToken);

		result.Value!.IdToken.Should().BeNull();
	}

	[Fact]
	public async Task Handle_WithValidRequest_RotatesOldToken()
	{
		SetupActiveClient();

		var token = CreateActiveRefreshToken();

		_refreshTokenRepository.FindByTokenHashAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
			.Returns(token);

		await _handler.Handle(ValidCommand(), TestCancellationToken);

		token.RevokedAt.Should().NotBeNull();
		token.ReplacedByToken.Should().Be("new_hashed_refresh_token");
	}

	[Fact]
	public async Task Handle_WithValidRequest_AddsNewRefreshToken()
	{
		SetupActiveClient();
		SetupActiveRefreshToken();

		await _handler.Handle(ValidCommand(), TestCancellationToken);

		_refreshTokenRepository.Received(1).Add(Arg.Any<RefreshToken>());
	}

	[Fact]
	public async Task Handle_WithValidRequest_SavesChanges()
	{
		SetupActiveClient();
		SetupActiveRefreshToken();

		await _handler.Handle(ValidCommand(), TestCancellationToken);

		await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
	}

	[Fact]
	public async Task Handle_WhenConfidentialClientSecretIsWrong_ReturnsInvalidClientWithoutLookingUpToken()
	{
		// Arrange
		var client = SetupConfidentialClient();
		SetupActiveRefreshToken(client.Id);

		_passwordHasher.Verify(Arg.Any<string>(), Arg.Any<HashedPassword>())
			.Returns(false);

		var command = ValidCommand() with { ClientSecret = "wrong_secret" };

		// Act
		var result = await _handler.Handle(command, TestCancellationToken);

		// Assert
		result.IsSuccess.Should().BeFalse();
		result.ErrorType.Should().Be(ResultError.Unauthorized);
		result.Error.Should().Be("invalid_client");
		await _refreshTokenRepository.DidNotReceive().FindByTokenHashAsync(
			Arg.Any<string>(),
			Arg.Any<CancellationToken>());
	}

	[Theory]
	[InlineData(null)]
	[InlineData("")]
	public async Task Handle_WhenConfidentialClientSecretIsMissing_ReturnsInvalidClientWithoutLookingUpToken(
		string? clientSecret)
	{
		// Arrange
		var client = SetupConfidentialClient();
		SetupActiveRefreshToken(client.Id);

		_passwordHasher.Verify(Arg.Any<string>(), Arg.Any<HashedPassword>())
			.Returns(true);

		var command = ValidCommand() with { ClientSecret = clientSecret };

		// Act
		var result = await _handler.Handle(command, TestCancellationToken);

		// Assert
		result.IsSuccess.Should().BeFalse();
		result.ErrorType.Should().Be(ResultError.Unauthorized);
		result.Error.Should().Be("invalid_client");
		await _refreshTokenRepository.DidNotReceive().FindByTokenHashAsync(
			Arg.Any<string>(),
			Arg.Any<CancellationToken>());
	}

	[Fact]
	public async Task Handle_WhenConfidentialClientSecretIsCorrect_ReturnsSuccess()
	{
		// Arrange
		var client = SetupConfidentialClient();
		SetupActiveRefreshToken(client.Id);

		_passwordHasher.Verify(
				ConfidentialSecret,
				Arg.Is<HashedPassword>(hashedPassword =>
					hashedPassword.Value == ConfidentialSecretHash))
			.Returns(true);

		var command = ValidCommand() with { ClientSecret = ConfidentialSecret };

		// Act
		var result = await _handler.Handle(command, TestCancellationToken);

		// Assert
		result.IsSuccess.Should().BeTrue();
	}

	[Fact]
	public async Task Handle_WhenPublicClientSendsNoSecret_ReturnsSuccessWithoutVerifyingSecret()
	{
		// Arrange
		SetupActiveClient();
		SetupActiveRefreshToken();

		var command = ValidCommand() with { ClientSecret = null };

		// Act
		var result = await _handler.Handle(command, TestCancellationToken);

		// Assert
		result.IsSuccess.Should().BeTrue();
		_passwordHasher.DidNotReceive().Verify(
			Arg.Any<string>(),
			Arg.Any<HashedPassword>());
	}

	private Client SetupConfidentialClient()
	{
		var client = new Client(
			"dmauth_test_client",
			"Test Client",
			ClientType.Confidential,
			Guid.NewGuid(),
			["https://example.com/callback"],
			["openid", "profile", "offline_access"],
			ConfidentialSecretHash);

		_clientRepository.FindByClientIdAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
			.Returns(client);

		return client;
	}

	private void SetupActiveClient()
	{
		_clientRepository.FindByClientIdAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
			.Returns(_activeClient);
	}

	private void SetupActiveRefreshToken(Guid? clientId = null)
	{
		_refreshTokenRepository.FindByTokenHashAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
			.Returns(CreateActiveRefreshToken(clientId));
	}

	private static Client CreateActiveClient() =>
		new(
			"dmauth_test_client",
			"Test Client",
			ClientType.Public,
			Guid.NewGuid(),
			["https://example.com/callback"],
			["openid", "profile", "offline_access"]);

	private RefreshToken CreateActiveRefreshToken(
		Guid? clientId = null,
		DateTimeOffset? expiresAt = null) =>
		new(
			HashValue(PlainToken),
			Guid.NewGuid(),
			clientId ?? _activeClient.Id,
			expiresAt ?? DateTimeOffset.UtcNow.AddDays(30),
			"openid profile",
			Guid.NewGuid());

	private static RotateTokenCommand ValidCommand() =>
		new(
			"dmauth_test_client",
			PlainToken,
			null);

	private static string HashValue(string value) =>
		Base64Url.EncodeToString(SHA256.HashData(Encoding.UTF8.GetBytes(value)));

	private const string PlainToken = "test_plain_refresh_token";
	private const string ConfidentialSecret = "correct_client_secret";
	private const string ConfidentialSecretHash = "$2a$12$exampleclientsecrethashvalue";
}
