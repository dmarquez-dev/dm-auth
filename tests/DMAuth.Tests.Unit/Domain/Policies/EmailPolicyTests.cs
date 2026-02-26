using DMAuth.Domain.Policies;
using FluentAssertions;

namespace DMAuth.Tests.Unit.Domain.Policies;

public class EmailPolicyTests
{
	[Fact]
	public void Validate_WithValidEmail_ReturnsCompliant()
	{
		var result = EmailPolicy.Validate("user@example.com");

		result.IsCompliant.Should().BeTrue();
		result.Violations.Should().BeEmpty();
	}

	[Theory]
	[InlineData("")]
	[InlineData("   ")]
	public void Validate_WithEmptyOrWhitespace_ReturnsNonCompliant(string value)
	{
		var result = EmailPolicy.Validate(value);

		result.IsCompliant.Should().BeFalse();
		result.Violations.Should().ContainSingle(violation =>
			violation.Contains("cannot be empty"));
	}

	[Fact]
	public void Validate_WithNoAtSign_ReturnsNonCompliant()
	{
		var result = EmailPolicy.Validate("invalidemail.com");

		result.IsCompliant.Should().BeFalse();
		result.Violations.Should().ContainSingle(violation =>
			violation.Contains("format is invalid"));
	}

	[Fact]
	public void Validate_WithEmailExceeding256Chars_ReturnsNonCompliant()
	{
		var longEmail = new string('a', 251) + "@b.com"; // 257 chars

		var result = EmailPolicy.Validate(longEmail);

		result.IsCompliant.Should().BeFalse();
		result.Violations.Should().ContainSingle(violation =>
			violation.Contains("must not exceed 256 characters"));
	}

	[Fact]
	public void Validate_WithEmailExceeding256CharsAndNoAtSign_ReturnsBothViolations()
	{
		var longInvalidEmail = new string('a', 257); // >256 chars, no @

		var result = EmailPolicy.Validate(longInvalidEmail);

		result.IsCompliant.Should().BeFalse();
		result.Violations.Should().HaveCount(2);
		result.Violations.Should().Contain(violation => violation.Contains("must not exceed 256 characters"));
		result.Violations.Should().Contain(violation => violation.Contains("format is invalid"));
	}

	[Fact]
	public void Validate_WhenCompliant_ViolationSummaryIsEmpty()
	{
		var result = EmailPolicy.Validate("user@example.com");

		result.ViolationSummary.Should().BeEmpty();
	}

	[Fact]
	public void Validate_WithMultipleViolations_ViolationSummaryJoinsWithSpace()
	{
		var result = EmailPolicy.Validate(new string('a', 257));

		result.ViolationSummary.Should().Be(string.Join(" ", result.Violations));
	}
}
