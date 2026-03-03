using System.Buffers.Text;
using System.Security.Cryptography;
using System.Text;
using DMAuth.Application.Common.Interfaces;
using DMAuth.Application.Features.Auth.RevokeToken;
using DMAuth.Domain.Entities.RefreshToken;
using DMAuth.Domain.Interfaces;
using DMAuth.Tests.Unit.Common;
using FluentAssertions;
using NSubstitute;

namespace DMAuth.Tests.Unit.Application.Auth;

public class RevokeTokenHandlerTests
	: UnitTestBase
{
	private readonly IRefreshTokenRepository _refreshTokenRepository = Substitute.For<IRefreshTokenRepository>();
	private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
	private readonly RevokeTokenHandler _handler;

	public RevokeTokenHandlerTests()
	{
		_handler = new RevokeTokenHandler(
			_refreshTokenRepository,
			_unitOfWork);
	}

	[Fact]
	public async Task Handle_WhenTokenNotFound_ReturnsSuccess()
	{
		_refreshTokenRepository.FindByTokenHashAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
			.Returns((RefreshToken?)null);

		var result = await _handler.Handle(ValidCommand(), TestCancellationToken);

		result.IsSuccess.Should().BeTrue();
	}

	[Fact]
	public async Task Handle_WhenTokenNotFound_DoesNotSaveChanges()
	{
		_refreshTokenRepository.FindByTokenHashAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
			.Returns((RefreshToken?)null);

		await _handler.Handle(ValidCommand(), TestCancellationToken);

		await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
	}

	[Fact]
	public async Task Handle_WhenTokenAlreadyRevoked_ReturnsSuccess()
	{
		var token = CreateActiveRefreshToken();
		token.Revoke();

		_refreshTokenRepository.FindByTokenHashAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
			.Returns(token);

		var result = await _handler.Handle(ValidCommand(), TestCancellationToken);

		result.IsSuccess.Should().BeTrue();
	}

	[Fact]
	public async Task Handle_WhenTokenAlreadyRevoked_DoesNotSaveChanges()
	{
		var token = CreateActiveRefreshToken();
		token.Revoke();

		_refreshTokenRepository.FindByTokenHashAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
			.Returns(token);

		await _handler.Handle(ValidCommand(), TestCancellationToken);

		await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
	}

	[Fact]
	public async Task Handle_WhenTokenIsActive_RevokesTokenAndSavesChanges()
	{
		var token = CreateActiveRefreshToken();

		_refreshTokenRepository.FindByTokenHashAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
			.Returns(token);

		await _handler.Handle(ValidCommand(), TestCancellationToken);

		token.RevokedAt.Should().NotBeNull();
		await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
	}

	[Fact]
	public async Task Handle_WhenTokenIsActive_ReturnsSuccess()
	{
		_refreshTokenRepository.FindByTokenHashAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
			.Returns(CreateActiveRefreshToken());

		var result = await _handler.Handle(ValidCommand(), TestCancellationToken);

		result.IsSuccess.Should().BeTrue();
	}

	private static RefreshToken CreateActiveRefreshToken() =>
		new(
			HashValue(PlainToken),
			Guid.NewGuid(),
			Guid.NewGuid(),
			DateTimeOffset.UtcNow.AddDays(30),
			"openid profile",
			Guid.NewGuid());

	private static RevokeTokenCommand ValidCommand() =>
		new(PlainToken);

	private static string HashValue(string value) =>
		Base64Url.EncodeToString(SHA256.HashData(Encoding.UTF8.GetBytes(value)));

	private const string PlainToken = "test_plain_refresh_token";
}
