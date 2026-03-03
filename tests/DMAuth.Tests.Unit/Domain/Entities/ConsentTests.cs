using DMAuth.Domain.Entities.Consent;
using FluentAssertions;

namespace DMAuth.Tests.Unit.Domain.Entities;

public class ConsentTests
{
	private static readonly Guid TestUserId = Guid.NewGuid();
	private static readonly Guid TestClientId = Guid.NewGuid();

	private static Consent CreateConsent() =>
		new(TestUserId, TestClientId, "openid profile");

	[Fact]
	public void Constructor_WithValidArguments_SetsAllProperties()
	{
		var consent = CreateConsent();

		consent.UserId.Should().Be(TestUserId);
		consent.ClientId.Should().Be(TestClientId);
		consent.GrantedScopes.Should().Be("openid profile");
	}

	[Fact]
	public void Constructor_SetsGrantedAt()
	{
		var before = DateTimeOffset.UtcNow;

		var consent = CreateConsent();

		consent.GrantedAt.Should().BeOnOrAfter(before);
		consent.GrantedAt.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(1));
	}

	[Fact]
	public void UpdateGrantedScopes_ChangesGrantedScopes()
	{
		var consent = CreateConsent();

		consent.UpdateGrantedScopes("openid profile email");

		consent.GrantedScopes.Should().Be("openid profile email");
	}

	[Fact]
	public void UpdateGrantedScopes_UpdatesGrantedAt()
	{
		var consent = CreateConsent();
		var grantedAtBefore = consent.GrantedAt;

		consent.UpdateGrantedScopes("openid profile email");

		consent.GrantedAt.Should().BeOnOrAfter(grantedAtBefore);
	}
}
