using DMAuth.Domain.Policies;
using FluentAssertions;

namespace DMAuth.Tests.Unit.Domain.Policies;

public class PasswordPolicyTests
{
	[Fact]
	public void Validate_WithCompliantPassword_ReturnsCompliant()
	{
		var result = PasswordPolicy.Validate("Secure1!");

		result.IsCompliant.Should().BeTrue();
		result.Violations.Should().BeEmpty();
	}

	[Theory]
	[InlineData("S1!")]      // 3 chars
	[InlineData("Secur1!")]  // 7 chars
	public void Validate_WithPasswordUnderMinimumLength_ReturnsNonCompliant(string password)
	{
		var result = PasswordPolicy.Validate(password);

		result.IsCompliant.Should().BeFalse();
		result.Violations.Should().ContainSingle(violation =>
			violation.Contains("at least 8 characters"));
	}

	[Fact]
	public void Validate_AtExactMinimumLength_WithAllRules_ReturnsCompliant()
	{
		var result = PasswordPolicy.Validate("Secure1!"); // exactly 8 chars

		result.IsCompliant.Should().BeTrue();
	}

	[Fact]
	public void Validate_WithNoDigit_ReturnsNonCompliantWithDigitViolation()
	{
		var result = PasswordPolicy.Validate("SecurePass!");

		result.IsCompliant.Should().BeFalse();
		result.Violations.Should().ContainSingle(violation =>
			violation.Contains("at least one digit"));
	}

	[Fact]
	public void Validate_WithNoSpecialChar_ReturnsNonCompliantWithSpecialCharViolation()
	{
		var result = PasswordPolicy.Validate("Secure123");

		result.IsCompliant.Should().BeFalse();
		result.Violations.Should().ContainSingle(violation =>
			violation.Contains("at least one special character"));
	}

	[Fact]
	public void Validate_WithAllViolations_ReturnsThreeViolations()
	{
		var result = PasswordPolicy.Validate("abc"); // short, no digit, no special char

		result.IsCompliant.Should().BeFalse();
		result.Violations.Should().HaveCount(3);
	}

	[Theory]
	[InlineData("Secure1!")]  // punctuation
	[InlineData("Secure1$")]  // symbol
	[InlineData("Secure1.")]  // period (punctuation)
	public void Validate_WithSpecialCharVariants_ReturnsCompliant(string password)
	{
		var result = PasswordPolicy.Validate(password);

		result.IsCompliant.Should().BeTrue();
	}

	[Fact]
	public void Validate_WithMultipleViolations_ViolationSummaryJoinsWithSpace()
	{
		var result = PasswordPolicy.Validate("abc"); // three violations

		result.ViolationSummary.Should().Be(string.Join(" ", result.Violations));
	}

	[Fact]
	public void Validate_WhenCompliant_ViolationSummaryIsEmpty()
	{
		var result = PasswordPolicy.Validate("Secure1!");

		result.ViolationSummary.Should().BeEmpty();
	}
}
