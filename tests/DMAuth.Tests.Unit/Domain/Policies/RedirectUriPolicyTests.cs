using DMAuth.Domain.Policies;
using FluentAssertions;

namespace DMAuth.Tests.Unit.Domain.Policies;

public class RedirectUriPolicyTests
{
	[Theory]
	[InlineData("https://example.com/callback")]
	[InlineData("https://app.example.com/auth/callback")]
	public void Validate_WithValidHttpsUri_ReturnsCompliant(string uri)
	{
		var result = RedirectUriPolicy.Validate(uri);

		result.IsCompliant.Should().BeTrue();
		result.Violations.Should().BeEmpty();
	}

	[Theory]
	[InlineData("http://localhost/callback")]
	[InlineData("http://localhost:3000/callback")]
	public void Validate_WithLocalhostHttpUri_ReturnsCompliant(string uri)
	{
		var result = RedirectUriPolicy.Validate(uri);

		result.IsCompliant.Should().BeTrue();
	}

	[Theory]
	[InlineData("http://127.0.0.1/callback")]
	[InlineData("http://127.0.0.1:5000/callback")]
	public void Validate_With127001HttpUri_ReturnsCompliant(string uri)
	{
		var result = RedirectUriPolicy.Validate(uri);

		result.IsCompliant.Should().BeTrue();
	}

	[Theory]
	[InlineData("")]
	[InlineData("   ")]
	public void Validate_WithEmptyOrWhitespace_ReturnsNonCompliant(string value)
	{
		var result = RedirectUriPolicy.Validate(value);

		result.IsCompliant.Should().BeFalse();
		result.Violations.Should().ContainSingle(violation =>
			violation.Contains("cannot be empty"));
	}

	[Theory]
	[InlineData("not-a-uri")]
	[InlineData("/relative/path")]
	public void Validate_WithInvalidUri_ReturnsNonCompliant(string value)
	{
		var result = RedirectUriPolicy.Validate(value);

		result.IsCompliant.Should().BeFalse();
		result.Violations.Should().ContainSingle(violation =>
			violation.Contains("valid absolute URI"));
	}

	[Fact]
	public void Validate_WithFragment_ReturnsNonCompliant()
	{
		var result = RedirectUriPolicy.Validate("https://example.com/callback#section");

		result.IsCompliant.Should().BeFalse();
		result.Violations.Should().ContainSingle(violation =>
			violation.Contains("fragment"));
	}

	[Fact]
	public void Validate_WithHttpAndNonLocalhostHost_ReturnsNonCompliant()
	{
		var result = RedirectUriPolicy.Validate("http://example.com/callback");

		result.IsCompliant.Should().BeFalse();
		result.Violations.Should().ContainSingle(violation =>
			violation.Contains("HTTPS"));
	}

	[Fact]
	public void Validate_WithFragmentAndHttpNonLocalhost_ReturnsBothViolations()
	{
		var result = RedirectUriPolicy.Validate("http://example.com/callback#section");

		result.IsCompliant.Should().BeFalse();
		result.Violations.Should().HaveCount(2);
		result.Violations.Should().Contain(violation => violation.Contains("fragment"));
		result.Violations.Should().Contain(violation => violation.Contains("HTTPS"));
	}
}
