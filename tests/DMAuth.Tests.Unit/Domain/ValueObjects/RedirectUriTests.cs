using DMAuth.Domain.Exceptions;
using DMAuth.Domain.ValueObjects;
using FluentAssertions;

namespace DMAuth.Tests.Unit.Domain.ValueObjects;

public class RedirectUriTests
{
	[Fact]
	public void Constructor_WithValidHttpsUri_StoresValue()
	{
		var uri = new RedirectUri("https://example.com/callback");

		uri.Value.Should().Be("https://example.com/callback");
	}

	[Fact]
	public void Constructor_WithLocalhostHttpUri_StoresValue()
	{
		var uri = new RedirectUri("http://localhost:3000/callback");

		uri.Value.Should().Be("http://localhost:3000/callback");
	}

	[Theory]
	[InlineData("")]
	[InlineData("not-a-uri")]
	public void Constructor_WithInvalidUri_ThrowsDomainException(string value)
	{
		var act = () => new RedirectUri(value);

		act.Should().Throw<DomainException>();
	}

	[Fact]
	public void Constructor_WithFragment_ThrowsDomainException()
	{
		var act = () => new RedirectUri("https://example.com/callback#section");

		act.Should().Throw<DomainException>()
			.WithMessage("*fragment*");
	}

	[Fact]
	public void Constructor_WithHttpNonLocalhostHost_ThrowsDomainException()
	{
		var act = () => new RedirectUri("http://example.com/callback");

		act.Should().Throw<DomainException>()
			.WithMessage("*HTTPS*");
	}

	[Fact]
	public void TwoRedirectUris_WithSameValue_AreEqual()
	{
		var first = new RedirectUri("https://example.com/callback");
		var second = new RedirectUri("https://example.com/callback");

		first.Should().Be(second);
	}

	[Fact]
	public void TwoRedirectUris_WithDifferentValues_AreNotEqual()
	{
		var first = new RedirectUri("https://example.com/callback");
		var second = new RedirectUri("https://other.com/callback");

		first.Should().NotBe(second);
	}
}
