using DMAuth.Application.Common.Interfaces;
using DMAuth.Application.Common.Results;
using DMAuth.Application.Features.Users.Register;
using DMAuth.Domain.Interfaces;
using DMAuth.Domain.ValueObjects;
using DMAuth.Tests.Unit.Common;
using FluentAssertions;
using NSubstitute;

namespace DMAuth.Tests.Unit.Application.Users;

public class RegisterUserHandlerTests
	: UnitTestBase
{
	private readonly IUserRepository _userRepository = Substitute.For<IUserRepository>();
	private readonly IPasswordHasher _passwordHasher = Substitute.For<IPasswordHasher>();
	private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
	private readonly RegisterUserHandler _handler;

	public RegisterUserHandlerTests()
	{
		_handler = new RegisterUserHandler(_userRepository, _passwordHasher, _unitOfWork);
	}

	[Fact]
	public async Task Handle_WhenPasswordFailsPolicy_ReturnsInvalid()
	{
		var command = new RegisterUserCommand("user@example.com", "testuser", "weak", "Test User");

		var result = await _handler.Handle(command, TestCancellationToken);

		result.IsSuccess.Should().BeFalse();
		result.ErrorType.Should().Be(ResultError.Invalid);
	}

	[Fact]
	public async Task Handle_WhenEmailAlreadyExists_ReturnsConflict()
	{
		_userRepository.ExistsByEmailAsync(Arg.Any<Email>(), Arg.Any<CancellationToken>())
			.Returns(true);

		var command = new RegisterUserCommand("user@example.com", "testuser", "Secure1!", "Test User");

		var result = await _handler.Handle(command, TestCancellationToken);

		result.IsSuccess.Should().BeFalse();
		result.ErrorType.Should().Be(ResultError.Conflict);
	}

	[Fact]
	public async Task Handle_WhenUsernameAlreadyExists_ReturnsConflict()
	{
		_userRepository.ExistsByEmailAsync(Arg.Any<Email>(), Arg.Any<CancellationToken>())
			.Returns(false);
		_userRepository.ExistsByUsernameAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
			.Returns(true);

		var command = new RegisterUserCommand("user@example.com", "testuser", "Secure1!", "Test User");

		var result = await _handler.Handle(command, TestCancellationToken);

		result.IsSuccess.Should().BeFalse();
		result.ErrorType.Should().Be(ResultError.Conflict);
	}

	[Fact]
	public async Task Handle_WithValidRequest_ReturnsSuccessWithUserId()
	{
		SetupSuccessfulRegistration();

		var command = new RegisterUserCommand("user@example.com", "testuser", "Secure1!", "Test User");

		var result = await _handler.Handle(command, TestCancellationToken);

		result.IsSuccess.Should().BeTrue();
		result.Value!.UserId.Should().NotBeEmpty();
	}

	[Fact]
	public async Task Handle_WithValidRequest_AddsUserToRepository()
	{
		SetupSuccessfulRegistration();

		var command = new RegisterUserCommand("user@example.com", "testuser", "Secure1!", "Test User");

		await _handler.Handle(command, TestCancellationToken);

		_userRepository.Received(1).Add(Arg.Any<DMAuth.Domain.Entities.User.User>());
	}

	[Fact]
	public async Task Handle_WithValidRequest_SavesChanges()
	{
		SetupSuccessfulRegistration();

		var command = new RegisterUserCommand("user@example.com", "testuser", "Secure1!", "Test User");

		await _handler.Handle(command, TestCancellationToken);

		await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
	}

	private void SetupSuccessfulRegistration()
	{
		_userRepository.ExistsByEmailAsync(Arg.Any<Email>(), Arg.Any<CancellationToken>())
			.Returns(false);
		_userRepository.ExistsByUsernameAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
			.Returns(false);
		_passwordHasher.Hash(Arg.Any<string>())
			.Returns(new HashedPassword("$2a$12$hashed"));
	}
}
