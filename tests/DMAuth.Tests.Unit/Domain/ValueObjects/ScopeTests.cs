using DMAuth.Domain.Exceptions;
using DMAuth.Domain.ValueObjects;
using FluentAssertions;

namespace DMAuth.Tests.Unit.Domain.ValueObjects;

public class ScopeTests
{
	[Fact]
	public void Constructor_WithValidScope_StoresLowercaseValue()
	{
		var scope = new Scope("openid");

		scope.Value.Should().Be("openid");
	}

	[Fact]
	public void Constructor_WithMixedCaseScope_StoresLowercaseValue()
	{
		var scope = new Scope("OpenId");

		scope.Value.Should().Be("openid");
	}

	[Theory]
	[InlineData("")]
	[InlineData("   ")]
	public void Constructor_WithEmptyOrWhitespace_ThrowsDomainException(string value)
	{
		var act = () => new Scope(value);

		act.Should().Throw<DomainException>()
			.WithMessage("*cannot be empty*");
	}

	[Fact]
	public void Constructor_WithUnrecognizedScope_ThrowsDomainException()
	{
		var act = () => new Scope("unknown");

		act.Should().Throw<DomainException>()
			.WithMessage("*not a recognized scope*");
	}

	[Fact]
	public void TwoScopes_WithSameValue_AreEqual()
	{
		var first = new Scope("openid");
		var second = new Scope("openid");

		first.Should().Be(second);
	}

	[Fact]
	public void TwoScopes_WithSameValueDifferentCase_AreEqual()
	{
		var first = new Scope("OpenId");
		var second = new Scope("openid");

		first.Should().Be(second);
	}

	[Fact]
	public void TwoScopes_WithDifferentValues_AreNotEqual()
	{
		var first = new Scope("openid");
		var second = new Scope("profile");

		first.Should().NotBe(second);
	}
}
