using DMAuth.Application.Common.Interfaces;
using DMAuth.Application.Common.Results;
using DMAuth.Application.Features.Users.ChangePassword;
using DMAuth.Domain.Entities.User;
using DMAuth.Domain.Interfaces;
using DMAuth.Domain.ValueObjects;
using DMAuth.Tests.Unit.Common;
using FluentAssertions;
using NSubstitute;

namespace DMAuth.Tests.Unit.Application.Users;

public class ChangeUserPasswordHandlerTests
	: UnitTestBase
{
	private readonly IUserRepository _userRepository = Substitute.For<IUserRepository>();
	private readonly IPasswordHasher _passwordHasher = Substitute.For<IPasswordHasher>();
	private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
	private readonly ChangeUserPasswordHandler _handler;

	public ChangeUserPasswordHandlerTests()
	{
		_handler = new ChangeUserPasswordHandler(_userRepository, _passwordHasher, _unitOfWork);
	}

	[Fact]
	public async Task Handle_WhenUserNotFound_ReturnsNotFound()
	{
		_userRepository.FindByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
			.Returns((User?)null);

		var command = new ChangeUserPasswordCommand(Guid.NewGuid(), "OldPass1!", "NewSecure1!");

		var result = await _handler.Handle(command, TestCancellationToken);

		result.IsSuccess.Should().BeFalse();
		result.ErrorType.Should().Be(ResultError.NotFound);
	}

	[Fact]
	public async Task Handle_WhenCurrentPasswordIncorrect_ReturnsUnauthorized()
	{
		_userRepository.FindByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
			.Returns(CreateUser());
		_passwordHasher.Verify(Arg.Any<string>(), Arg.Any<HashedPassword>())
			.Returns(false);

		var command = new ChangeUserPasswordCommand(Guid.NewGuid(), "WrongPass1!", "NewSecure1!");

		var result = await _handler.Handle(command, TestCancellationToken);

		result.IsSuccess.Should().BeFalse();
		result.ErrorType.Should().Be(ResultError.Unauthorized);
	}

	[Fact]
	public async Task Handle_WhenNewPasswordFailsPolicy_ReturnsInvalid()
	{
		_userRepository.FindByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
			.Returns(CreateUser());
		_passwordHasher.Verify(Arg.Any<string>(), Arg.Any<HashedPassword>())
			.Returns(true);

		var command = new ChangeUserPasswordCommand(Guid.NewGuid(), "OldPass1!", "weak");

		var result = await _handler.Handle(command, TestCancellationToken);

		result.IsSuccess.Should().BeFalse();
		result.ErrorType.Should().Be(ResultError.Invalid);
	}

	[Fact]
	public async Task Handle_WithValidRequest_ChangesPasswordAndSaves()
	{
		var user = CreateUser();
		var newHash = new HashedPassword("$2a$12$newhash");

		_userRepository.FindByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
			.Returns(user);
		_passwordHasher.Verify(Arg.Any<string>(), Arg.Any<HashedPassword>())
			.Returns(true);
		_passwordHasher.Hash(Arg.Any<string>())
			.Returns(newHash);

		var command = new ChangeUserPasswordCommand(Guid.NewGuid(), "OldPass1!", "NewSecure1!");

		await _handler.Handle(command, TestCancellationToken);

		user.HashedPassword.Should().Be(newHash);
		await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
	}

	[Fact]
	public async Task Handle_WithValidRequest_ReturnsSuccess()
	{
		_userRepository.FindByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
			.Returns(CreateUser());
		_passwordHasher.Verify(Arg.Any<string>(), Arg.Any<HashedPassword>())
			.Returns(true);
		_passwordHasher.Hash(Arg.Any<string>())
			.Returns(new HashedPassword("$2a$12$newhash"));

		var command = new ChangeUserPasswordCommand(Guid.NewGuid(), "OldPass1!", "NewSecure1!");

		var result = await _handler.Handle(command, TestCancellationToken);

		result.IsSuccess.Should().BeTrue();
	}

	private static User CreateUser() =>
		new(
			new Email("user@example.com"),
			"testuser",
			new HashedPassword("$2a$12$hashed"),
			"Test User");
}
