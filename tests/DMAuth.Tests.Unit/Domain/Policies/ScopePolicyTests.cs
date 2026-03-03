using DMAuth.Domain.Policies;
using FluentAssertions;

namespace DMAuth.Tests.Unit.Domain.Policies;

public class ScopePolicyTests
{
	[Theory]
	[InlineData("openid")]
	[InlineData("profile")]
	[InlineData("email")]
	[InlineData("offline_access")]
	public void Validate_WithRecognizedScope_ReturnsCompliant(string scope)
	{
		var result = ScopePolicy.Validate(scope);

		result.IsCompliant.Should().BeTrue();
		result.Violations.Should().BeEmpty();
	}

	[Theory]
	[InlineData("OpenId")]
	[InlineData("PROFILE")]
	[InlineData("Email")]
	public void Validate_WithRecognizedScopeInMixedCase_ReturnsCompliant(string scope)
	{
		var result = ScopePolicy.Validate(scope);

		result.IsCompliant.Should().BeTrue();
	}

	[Theory]
	[InlineData("")]
	[InlineData("   ")]
	public void Validate_WithEmptyOrWhitespace_ReturnsNonCompliant(string value)
	{
		var result = ScopePolicy.Validate(value);

		result.IsCompliant.Should().BeFalse();
		result.Violations.Should().ContainSingle(violation =>
			violation.Contains("cannot be empty"));
	}

	[Theory]
	[InlineData("unknown")]
	[InlineData("admin")]
	[InlineData("read:users")]
	public void Validate_WithUnrecognizedScope_ReturnsNonCompliant(string scope)
	{
		var result = ScopePolicy.Validate(scope);

		result.IsCompliant.Should().BeFalse();
		result.Violations.Should().ContainSingle(violation =>
			violation.Contains("not a recognized scope"));
	}

	[Fact]
	public void Validate_WithUnrecognizedScope_ViolationIncludesAllowedScopes()
	{
		var result = ScopePolicy.Validate("unknown");

		result.Violations.Should().ContainSingle(violation =>
			violation.Contains("openid")
			&& violation.Contains("profile")
			&& violation.Contains("email"));
	}
}
