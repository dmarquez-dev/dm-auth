using DMAuth.Domain.Exceptions;
using DMAuth.Domain.ValueObjects;
using FluentAssertions;

namespace DMAuth.Tests.Unit.Domain.ValueObjects;

public class EmailTests
{
	[Fact]
	public void Constructor_WithValidEmail_StoresLowercaseValue()
	{
		var email = new Email("User@Example.com");

		email.Value.Should().Be("user@example.com");
	}

	[Fact]
	public void Constructor_WithAlreadyLowercaseEmail_StoresValue()
	{
		var email = new Email("user@example.com");

		email.Value.Should().Be("user@example.com");
	}

	[Theory]
	[InlineData("")]
	[InlineData("   ")]
	public void Constructor_WithEmptyOrWhitespace_ThrowsDomainException(string value)
	{
		var act = () => new Email(value);

		act.Should().Throw<DomainException>()
			.WithMessage("Email cannot be empty.");
	}

	[Fact]
	public void Constructor_WithNoAtSign_ThrowsDomainException()
	{
		var act = () => new Email("invalidemail.com");

		act.Should().Throw<DomainException>()
			.WithMessage("Email format is invalid.");
	}

	[Fact]
	public void Constructor_WithEmailExceeding256Chars_ThrowsDomainException()
	{
		var longEmail = new string('a', 251) + "@b.com"; // 257 chars

		var act = () => new Email(longEmail);

		act.Should().Throw<DomainException>()
			.WithMessage("*must not exceed 256 characters*");
	}

	[Fact]
	public void TwoEmails_WithSameValue_AreEqual()
	{
		var first = new Email("user@example.com");
		var second = new Email("user@example.com");

		first.Should().Be(second);
	}

	[Fact]
	public void TwoEmails_WithSameValueDifferentCase_AreEqual()
	{
		var first = new Email("User@Example.com");
		var second = new Email("user@example.com");

		first.Should().Be(second);
	}

	[Fact]
	public void TwoEmails_WithDifferentValues_AreNotEqual()
	{
		var first = new Email("user@example.com");
		var second = new Email("other@example.com");

		first.Should().NotBe(second);
	}
}
