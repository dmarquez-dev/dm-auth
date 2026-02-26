using DMAuth.Domain.Policies;
using FluentAssertions;

namespace DMAuth.Tests.Unit.Domain.Policies;

public class CodeChallengePolicyTests
{
	private const string ValidChallenge = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQ"; // 43 chars

	[Fact]
	public void Validate_WithValidChallenge_ReturnsCompliant()
	{
		var result = CodeChallengePolicy.Validate(ValidChallenge);

		result.IsCompliant.Should().BeTrue();
		result.Violations.Should().BeEmpty();
	}

	[Fact]
	public void Validate_WithMaximumLengthChallenge_ReturnsCompliant()
	{
		var challenge = new string('a', 128);

		var result = CodeChallengePolicy.Validate(challenge);

		result.IsCompliant.Should().BeTrue();
	}

	[Theory]
	[InlineData("")]
	[InlineData("   ")]
	public void Validate_WithEmptyOrWhitespace_ReturnsNonCompliant(string value)
	{
		var result = CodeChallengePolicy.Validate(value);

		result.IsCompliant.Should().BeFalse();
		result.Violations.Should().ContainSingle(violation =>
			violation.Contains("cannot be empty"));
	}

	[Fact]
	public void Validate_WithChallengeTooShort_ReturnsNonCompliant()
	{
		var challenge = new string('a', 42); // one below minimum

		var result = CodeChallengePolicy.Validate(challenge);

		result.IsCompliant.Should().BeFalse();
		result.Violations.Should().ContainSingle(violation =>
			violation.Contains("between 43 and 128 characters"));
	}

	[Fact]
	public void Validate_WithChallengeTooLong_ReturnsNonCompliant()
	{
		var challenge = new string('a', 129); // one above maximum

		var result = CodeChallengePolicy.Validate(challenge);

		result.IsCompliant.Should().BeFalse();
		result.Violations.Should().ContainSingle(violation =>
			violation.Contains("between 43 and 128 characters"));
	}

	[Fact]
	public void Validate_WithInvalidBase64UrlChars_ReturnsNonCompliant()
	{
		var challenge = new string('a', 42) + "+"; // 43 chars, invalid '+'

		var result = CodeChallengePolicy.Validate(challenge);

		result.IsCompliant.Should().BeFalse();
		result.Violations.Should().ContainSingle(violation =>
			violation.Contains("Base64url characters"));
	}

	[Fact]
	public void Validate_WithTooShortAndInvalidChars_ReturnsBothViolations()
	{
		var challenge = "a+"; // too short and contains invalid char

		var result = CodeChallengePolicy.Validate(challenge);

		result.IsCompliant.Should().BeFalse();
		result.Violations.Should().HaveCount(2);
		result.Violations.Should().Contain(violation => violation.Contains("between 43 and 128 characters"));
		result.Violations.Should().Contain(violation => violation.Contains("Base64url characters"));
	}
}
