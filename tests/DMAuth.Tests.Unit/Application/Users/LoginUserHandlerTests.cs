using DMAuth.Application.Common.Interfaces;
using DMAuth.Application.Common.Results;
using DMAuth.Application.Features.Users.Login;
using DMAuth.Domain.Entities.User;
using DMAuth.Domain.Interfaces;
using DMAuth.Domain.ValueObjects;
using DMAuth.Tests.Unit.Common;
using FluentAssertions;
using NSubstitute;

namespace DMAuth.Tests.Unit.Application.Users;

public class LoginUserHandlerTests
	: UnitTestBase
{
	private readonly IUserRepository _userRepository = Substitute.For<IUserRepository>();
	private readonly IPasswordHasher _passwordHasher = Substitute.For<IPasswordHasher>();
	private readonly LoginUserHandler _handler;

	public LoginUserHandlerTests()
	{
		_handler = new LoginUserHandler(_userRepository, _passwordHasher);
	}

	[Fact]
	public async Task Handle_WhenUserNotFound_ReturnsUnauthorized()
	{
		_userRepository.FindByEmailAsync(Arg.Any<Email>(), Arg.Any<CancellationToken>())
			.Returns((User?)null);

		var command = new LoginUserCommand("user@example.com", "Secure1!");

		var result = await _handler.Handle(command, TestCancellationToken);

		result.IsSuccess.Should().BeFalse();
		result.ErrorType.Should().Be(ResultError.Unauthorized);
	}

	[Fact]
	public async Task Handle_WhenPasswordInvalid_ReturnsUnauthorized()
	{
		_userRepository.FindByEmailAsync(Arg.Any<Email>(), Arg.Any<CancellationToken>())
			.Returns(CreateActiveUser());
		_passwordHasher.Verify(Arg.Any<string>(), Arg.Any<HashedPassword>())
			.Returns(false);

		var command = new LoginUserCommand("user@example.com", "wrongpassword");

		var result = await _handler.Handle(command, TestCancellationToken);

		result.IsSuccess.Should().BeFalse();
		result.ErrorType.Should().Be(ResultError.Unauthorized);
	}

	[Fact]
	public async Task Handle_WhenUserIsInactive_ReturnsUnauthorized()
	{
		var user = CreateActiveUser();
		user.Deactivate();

		_userRepository.FindByEmailAsync(Arg.Any<Email>(), Arg.Any<CancellationToken>())
			.Returns(user);
		_passwordHasher.Verify(Arg.Any<string>(), Arg.Any<HashedPassword>())
			.Returns(true);

		var command = new LoginUserCommand("user@example.com", "Secure1!");

		var result = await _handler.Handle(command, TestCancellationToken);

		result.IsSuccess.Should().BeFalse();
		result.ErrorType.Should().Be(ResultError.Unauthorized);
	}

	[Fact]
	public async Task Handle_WithValidCredentials_ReturnsSuccessWithUserDetails()
	{
		var user = CreateActiveUser();

		_userRepository.FindByEmailAsync(Arg.Any<Email>(), Arg.Any<CancellationToken>())
			.Returns(user);
		_passwordHasher.Verify(Arg.Any<string>(), Arg.Any<HashedPassword>())
			.Returns(true);

		var command = new LoginUserCommand("user@example.com", "Secure1!");

		var result = await _handler.Handle(command, TestCancellationToken);

		result.IsSuccess.Should().BeTrue();
		result.Value!.Username.Should().Be(user.Username);
		result.Value.Email.Should().Be(user.Email.Value);
		result.Value.DisplayName.Should().Be(user.DisplayName);
	}

	[Fact]
	public async Task Handle_WhenAuthenticationFails_UsesGenericErrorMessage()
	{
		_userRepository.FindByEmailAsync(Arg.Any<Email>(), Arg.Any<CancellationToken>())
			.Returns((User?)null);

		var command = new LoginUserCommand("user@example.com", "Secure1!");

		var result = await _handler.Handle(command, TestCancellationToken);

		result.Error.Should().Be("Invalid email or password.");
	}

	private static User CreateActiveUser() =>
		new(
			new Email("user@example.com"),
			"testuser",
			new HashedPassword("$2a$12$hashed"),
			"Test User");
}
