using DMAuth.Application.Common.Interfaces;
using DMAuth.Application.Common.Results;
using DMAuth.Application.Features.Users.UpdateProfile;
using DMAuth.Domain.Entities.User;
using DMAuth.Domain.Interfaces;
using DMAuth.Domain.ValueObjects;
using DMAuth.Tests.Unit.Common;
using FluentAssertions;
using NSubstitute;

namespace DMAuth.Tests.Unit.Application.Users;

public class UpdateUserProfileHandlerTests
	: UnitTestBase
{
	private readonly IUserRepository _userRepository = Substitute.For<IUserRepository>();
	private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
	private readonly UpdateUserProfileHandler _handler;

	public UpdateUserProfileHandlerTests()
	{
		_handler = new UpdateUserProfileHandler(_userRepository, _unitOfWork);
	}

	[Fact]
	public async Task Handle_WhenUserNotFound_ReturnsNotFound()
	{
		_userRepository.FindByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
			.Returns((User?)null);

		var command = new UpdateUserProfileCommand(Guid.NewGuid(), "New Name");

		var result = await _handler.Handle(command, TestCancellationToken);

		result.IsSuccess.Should().BeFalse();
		result.ErrorType.Should().Be(ResultError.NotFound);
	}

	[Fact]
	public async Task Handle_WhenUserExists_UpdatesDisplayName()
	{
		var user = CreateUser();
		_userRepository.FindByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
			.Returns(user);

		var command = new UpdateUserProfileCommand(Guid.NewGuid(), "Updated Name");

		await _handler.Handle(command, TestCancellationToken);

		user.DisplayName.Should().Be("Updated Name");
	}

	[Fact]
	public async Task Handle_WhenUserExists_SavesChanges()
	{
		_userRepository.FindByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
			.Returns(CreateUser());

		var command = new UpdateUserProfileCommand(Guid.NewGuid(), "Updated Name");

		await _handler.Handle(command, TestCancellationToken);

		await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
	}

	[Fact]
	public async Task Handle_WhenUserExists_ReturnsSuccess()
	{
		_userRepository.FindByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
			.Returns(CreateUser());

		var command = new UpdateUserProfileCommand(Guid.NewGuid(), "Updated Name");

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
