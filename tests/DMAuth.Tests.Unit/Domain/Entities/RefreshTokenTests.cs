using DMAuth.Domain.Entities.RefreshToken;
using DMAuth.Domain.Exceptions;
using FluentAssertions;

namespace DMAuth.Tests.Unit.Domain.Entities;

public class RefreshTokenTests
{
	private static readonly Guid TestUserId = Guid.NewGuid();
	private static readonly Guid TestClientId = Guid.NewGuid();
	private static readonly Guid TestFamilyId = Guid.NewGuid();

	private static RefreshToken CreateRefreshToken() =>
		new(
			"token_hash_value",
			TestUserId,
			TestClientId,
			DateTimeOffset.UtcNow.AddDays(30),
			"openid profile",
			TestFamilyId);

	[Fact]
	public void Constructor_WithValidArguments_SetsAllProperties()
	{
		var token = CreateRefreshToken();

		token.TokenHash.Should().Be("token_hash_value");
		token.UserId.Should().Be(TestUserId);
		token.ClientId.Should().Be(TestClientId);
		token.Scopes.Should().Be("openid profile");
		token.FamilyId.Should().Be(TestFamilyId);
	}

	[Fact]
	public void Constructor_SetsExpiresAt()
	{
		var before = DateTimeOffset.UtcNow;
		var token = CreateRefreshToken();

		token.ExpiresAt.Should().BeAfter(before);
	}

	[Fact]
	public void Constructor_SetsRevokedAtNull()
	{
		var token = CreateRefreshToken();

		token.RevokedAt.Should().BeNull();
	}

	[Fact]
	public void Constructor_SetsReplacedByTokenNull()
	{
		var token = CreateRefreshToken();

		token.ReplacedByToken.Should().BeNull();
	}

	[Fact]
	public void Revoke_SetsRevokedAt()
	{
		var token = CreateRefreshToken();

		token.Revoke();

		token.RevokedAt.Should().NotBeNull();
		token.RevokedAt!.Value.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(1));
	}

	[Fact]
	public void Revoke_WhenAlreadyRevoked_ThrowsDomainException()
	{
		var token = CreateRefreshToken();
		token.Revoke();

		var act = () => token.Revoke();

		act.Should().Throw<DomainException>()
			.WithMessage("Refresh token is already revoked.");
	}

	[Fact]
	public void Rotate_SetsRevokedAt()
	{
		var token = CreateRefreshToken();

		token.Rotate("new_token_hash");

		token.RevokedAt.Should().NotBeNull();
		token.RevokedAt!.Value.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(1));
	}

	[Fact]
	public void Rotate_SetsReplacedByToken()
	{
		var token = CreateRefreshToken();

		token.Rotate("new_token_hash");

		token.ReplacedByToken.Should().Be("new_token_hash");
	}

	[Fact]
	public void Rotate_WhenAlreadyRevoked_ThrowsDomainException()
	{
		var token = CreateRefreshToken();
		token.Revoke();

		var act = () => token.Rotate("new_token_hash");

		act.Should().Throw<DomainException>()
			.WithMessage("Cannot rotate a revoked refresh token.");
	}
}
