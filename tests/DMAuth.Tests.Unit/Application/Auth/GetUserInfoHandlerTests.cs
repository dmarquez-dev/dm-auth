using DMAuth.Application.Common.Results;
using DMAuth.Application.Features.Auth.GetUserInfo;
using DMAuth.Domain.Entities.User;
using DMAuth.Domain.Interfaces;
using DMAuth.Domain.ValueObjects;
using DMAuth.Tests.Unit.Common;
using FluentAssertions;
using NSubstitute;

namespace DMAuth.Tests.Unit.Application.Auth;

public class GetUserInfoHandlerTests
	: UnitTestBase
{
	private readonly IUserRepository _userRepository = Substitute.For<IUserRepository>();
	private readonly GetUserInfoHandler _handler;

	private static readonly User TestUser = new(
		new Email("userinfo@example.com"),
		"userinfouser",
		new HashedPassword("$2a$12$testhash"),
		"UserInfo Test User");

	public GetUserInfoHandlerTests()
	{
		_handler = new GetUserInfoHandler(_userRepository);
	}

	[Fact]
	public async Task Handle_WhenUserNotFound_ReturnsNotFound()
	{
		_userRepository
			.FindByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
			.Returns((User?)null);

		var query = new GetUserInfoQuery(Guid.NewGuid(), "openid");

		var result = await _handler.Handle(query, TestCancellationToken);

		result.IsSuccess.Should().BeFalse();
		result.ErrorType.Should().Be(ResultError.NotFound);
	}

	[Fact]
	public async Task Handle_WithOpenIdScope_ReturnsSubOnly()
	{
		_userRepository
			.FindByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
			.Returns(TestUser);

		var query = new GetUserInfoQuery(TestUser.Id, "openid");

		var result = await _handler.Handle(query, TestCancellationToken);

		result.IsSuccess.Should().BeTrue();
		result.Value!.Sub.Should().Be(TestUser.Id.ToString());
		result.Value.Name.Should().BeNull();
		result.Value.PreferredUsername.Should().BeNull();
		result.Value.Email.Should().BeNull();
		result.Value.EmailVerified.Should().BeNull();
	}

	[Fact]
	public async Task Handle_WithProfileScope_ReturnsProfileClaims()
	{
		_userRepository
			.FindByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
			.Returns(TestUser);

		var query = new GetUserInfoQuery(TestUser.Id, "openid profile");

		var result = await _handler.Handle(query, TestCancellationToken);

		result.IsSuccess.Should().BeTrue();
		result.Value!.Sub.Should().Be(TestUser.Id.ToString());
		result.Value.Name.Should().Be(TestUser.DisplayName);
		result.Value.PreferredUsername.Should().Be(TestUser.Username);
		result.Value.Email.Should().BeNull();
		result.Value.EmailVerified.Should().BeNull();
	}

	[Fact]
	public async Task Handle_WithEmailScope_ReturnsEmailClaims()
	{
		_userRepository
			.FindByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
			.Returns(TestUser);

		var query = new GetUserInfoQuery(TestUser.Id, "openid email");

		var result = await _handler.Handle(query, TestCancellationToken);

		result.IsSuccess.Should().BeTrue();
		result.Value!.Sub.Should().Be(TestUser.Id.ToString());
		result.Value.Name.Should().BeNull();
		result.Value.PreferredUsername.Should().BeNull();
		result.Value.Email.Should().Be(TestUser.Email.Value);
		result.Value.EmailVerified.Should().Be(TestUser.EmailVerified);
	}

	[Fact]
	public async Task Handle_WithAllScopes_ReturnsAllClaims()
	{
		_userRepository
			.FindByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
			.Returns(TestUser);

		var query = new GetUserInfoQuery(TestUser.Id, "openid profile email");

		var result = await _handler.Handle(query, TestCancellationToken);

		result.IsSuccess.Should().BeTrue();
		result.Value!.Sub.Should().Be(TestUser.Id.ToString());
		result.Value.Name.Should().Be(TestUser.DisplayName);
		result.Value.PreferredUsername.Should().Be(TestUser.Username);
		result.Value.Email.Should().Be(TestUser.Email.Value);
		result.Value.EmailVerified.Should().Be(TestUser.EmailVerified);
	}
}
