using DMAuth.Domain.Entities.User;
using DMAuth.Domain.Exceptions;
using DMAuth.Domain.ValueObjects;
using FluentAssertions;

namespace DMAuth.Tests.Unit.Domain.Entities;

public class UserTests
{
	private static readonly Email TestEmail = new("user@example.com");
	private static readonly HashedPassword TestPassword = new("$2a$12$somehashvalue");

	[Fact]
	public void Constructor_WithValidArguments_SetsAllProperties()
	{
		var user = new User(TestEmail, "testuser", TestPassword, "Test User");

		user.Email.Should().Be(TestEmail);
		user.Username.Should().Be("testuser");
		user.HashedPassword.Should().Be(TestPassword);
		user.DisplayName.Should().Be("Test User");
	}

	[Fact]
	public void Constructor_SetsIsActiveTrue()
	{
		var user = new User(TestEmail, "testuser", TestPassword, "Test User");

		user.IsActive.Should().BeTrue();
	}

	[Fact]
	public void Constructor_SetsEmailVerifiedFalse()
	{
		var user = new User(TestEmail, "testuser", TestPassword, "Test User");

		user.EmailVerified.Should().BeFalse();
	}

	[Fact]
	public void UpdateProfile_ChangesDisplayName()
	{
		var user = new User(TestEmail, "testuser", TestPassword, "Original Name");

		user.UpdateProfile("Updated Name");

		user.DisplayName.Should().Be("Updated Name");
	}

	[Fact]
	public void UpdateProfile_SetsUpdatedAt()
	{
		var user = new User(TestEmail, "testuser", TestPassword, "Test User");

		user.UpdateProfile("Updated Name");

		user.UpdatedAt.Should().NotBeNull();
		user.UpdatedAt!.Value.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(1));
	}

	[Fact]
	public void ChangePassword_ChangesHashedPassword()
	{
		var user = new User(TestEmail, "testuser", TestPassword, "Test User");
		var newPassword = new HashedPassword("$2a$12$newhashvalue");

		user.ChangePassword(newPassword);

		user.HashedPassword.Should().Be(newPassword);
	}

	[Fact]
	public void ChangePassword_SetsUpdatedAt()
	{
		var user = new User(TestEmail, "testuser", TestPassword, "Test User");

		user.ChangePassword(new HashedPassword("$2a$12$newhashvalue"));

		user.UpdatedAt.Should().NotBeNull();
		user.UpdatedAt!.Value.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(1));
	}

	[Fact]
	public void Deactivate_SetsIsActiveFalse()
	{
		var user = new User(TestEmail, "testuser", TestPassword, "Test User");

		user.Deactivate();

		user.IsActive.Should().BeFalse();
	}

	[Fact]
	public void Deactivate_WhenAlreadyInactive_ThrowsDomainException()
	{
		var user = new User(TestEmail, "testuser", TestPassword, "Test User");
		user.Deactivate();

		var act = () => user.Deactivate();

		act.Should().Throw<DomainException>()
			.WithMessage("User account is already inactive.");
	}
}
