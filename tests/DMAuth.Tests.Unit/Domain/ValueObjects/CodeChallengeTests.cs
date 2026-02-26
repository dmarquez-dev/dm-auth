using DMAuth.Domain.Exceptions;
using DMAuth.Domain.ValueObjects;
using FluentAssertions;

namespace DMAuth.Tests.Unit.Domain.ValueObjects;

public class CodeChallengeTests
{
	private const string ValidChallenge = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQ"; // 43 chars

	[Fact]
	public void Constructor_WithValidChallenge_StoresValue()
	{
		var challenge = new CodeChallenge(ValidChallenge);

		challenge.Value.Should().Be(ValidChallenge);
	}

	[Theory]
	[InlineData("")]
	[InlineData("   ")]
	public void Constructor_WithEmptyOrWhitespace_ThrowsDomainException(string value)
	{
		var act = () => new CodeChallenge(value);

		act.Should().Throw<DomainException>()
			.WithMessage("*cannot be empty*");
	}

	[Fact]
	public void Constructor_WithChallengeTooShort_ThrowsDomainException()
	{
		var act = () => new CodeChallenge(new string('a', 42));

		act.Should().Throw<DomainException>()
			.WithMessage("*between 43 and 128 characters*");
	}

	[Fact]
	public void Constructor_WithChallengeTooLong_ThrowsDomainException()
	{
		var act = () => new CodeChallenge(new string('a', 129));

		act.Should().Throw<DomainException>()
			.WithMessage("*between 43 and 128 characters*");
	}

	[Fact]
	public void Constructor_WithInvalidBase64UrlChars_ThrowsDomainException()
	{
		var act = () => new CodeChallenge(new string('a', 42) + "+");

		act.Should().Throw<DomainException>()
			.WithMessage("*Base64url characters*");
	}

	[Fact]
	public void TwoChallenges_WithSameValue_AreEqual()
	{
		var first = new CodeChallenge(ValidChallenge);
		var second = new CodeChallenge(ValidChallenge);

		first.Should().Be(second);
	}

	[Fact]
	public void TwoChallenges_WithDifferentValues_AreNotEqual()
	{
		var first = new CodeChallenge(ValidChallenge);
		var second = new CodeChallenge(new string('b', 43));

		first.Should().NotBe(second);
	}
}
