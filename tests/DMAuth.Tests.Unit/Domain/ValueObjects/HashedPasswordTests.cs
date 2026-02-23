using DMAuth.Domain.Exceptions;
using DMAuth.Domain.ValueObjects;
using FluentAssertions;

namespace DMAuth.Tests.Unit.Domain.ValueObjects;

public class HashedPasswordTests
{
	[Fact]
	public void Constructor_WithValidHash_StoresValue()
	{
		var hashedPassword = new HashedPassword("$2a$12$somebcrypthashvalue");

		hashedPassword.Value.Should().Be("$2a$12$somebcrypthashvalue");
	}

	[Theory]
	[InlineData("")]
	[InlineData("   ")]
	public void Constructor_WithEmptyOrWhitespace_ThrowsDomainException(string value)
	{
		var act = () => new HashedPassword(value);

		act.Should().Throw<DomainException>()
			.WithMessage("Hashed password cannot be empty.");
	}

	[Fact]
	public void TwoHashedPasswords_WithSameValue_AreEqual()
	{
		var first = new HashedPassword("$2a$12$somebcrypthashvalue");
		var second = new HashedPassword("$2a$12$somebcrypthashvalue");

		first.Should().Be(second);
	}

	[Fact]
	public void TwoHashedPasswords_WithDifferentValues_AreNotEqual()
	{
		var first = new HashedPassword("$2a$12$hashone");
		var second = new HashedPassword("$2a$12$hashtwo");

		first.Should().NotBe(second);
	}
}
