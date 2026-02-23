using DMAuth.Application.Common.Results;
using DMAuth.Application.Features.Users.GetProfile;
using DMAuth.Domain.Entities.User;
using DMAuth.Domain.Interfaces;
using DMAuth.Domain.ValueObjects;
using DMAuth.Tests.Unit.Common;
using FluentAssertions;
using NSubstitute;

namespace DMAuth.Tests.Unit.Application.Users;

public class GetUserProfileHandlerTests
	: UnitTestBase
{
	private readonly IUserRepository _userRepository = Substitute.For<IUserRepository>();
	private readonly GetUserProfileHandler _handler;

	public GetUserProfileHandlerTests()
	{
		_handler = new GetUserProfileHandler(_userRepository);
	}

	[Fact]
	public async Task Handle_WhenUserNotFound_ReturnsNotFound()
	{
		_userRepository.FindByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
			.Returns((User?)null);

		var query = new GetUserProfileQuery(Guid.NewGuid());

		var result = await _handler.Handle(query, TestCancellationToken);

		result.IsSuccess.Should().BeFalse();
		result.ErrorType.Should().Be(ResultError.NotFound);
	}

	[Fact]
	public async Task Handle_WhenUserExists_ReturnsSuccessWithProfileData()
	{
		var user = new User(
			new Email("user@example.com"),
			"testuser",
			new HashedPassword("$2a$12$hashed"),
			"Test User");

		_userRepository.FindByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
			.Returns(user);

		var query = new GetUserProfileQuery(Guid.NewGuid());

		var result = await _handler.Handle(query, TestCancellationToken);

		result.IsSuccess.Should().BeTrue();
		result.Value!.Username.Should().Be("testuser");
		result.Value.Email.Should().Be("user@example.com");
		result.Value.DisplayName.Should().Be("Test User");
		result.Value.EmailVerified.Should().BeFalse();
	}
}
