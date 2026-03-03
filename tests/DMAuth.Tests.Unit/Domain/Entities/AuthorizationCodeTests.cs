using DMAuth.Domain.Entities.AuthorizationCode;
using DMAuth.Domain.Enums;
using DMAuth.Domain.Exceptions;
using DMAuth.Domain.ValueObjects;
using FluentAssertions;

namespace DMAuth.Tests.Unit.Domain.Entities;

public class AuthorizationCodeTests
{
	private static readonly Guid TestUserId = Guid.NewGuid();
	private static readonly Guid TestClientId = Guid.NewGuid();

	private static AuthorizationCode CreateAuthorizationCode(string? nonce = null) =>
		new(
			"code_hash_value",
			TestUserId,
			TestClientId,
			"https://example.com/callback",
			"openid profile",
			new CodeChallenge(new string('A', 43)),
			CodeChallengeMethod.S256,
			DateTimeOffset.UtcNow.AddMinutes(5),
			nonce);

	[Fact]
	public void Constructor_WithValidArguments_SetsAllProperties()
	{
		var authCode = CreateAuthorizationCode();

		authCode.CodeHash.Should().Be("code_hash_value");
		authCode.UserId.Should().Be(TestUserId);
		authCode.ClientId.Should().Be(TestClientId);
		authCode.RedirectUri.Should().Be("https://example.com/callback");
		authCode.Scopes.Should().Be("openid profile");
		authCode.CodeChallenge.Value.Should().Be(new string('A', 43));
		authCode.CodeChallengeMethod.Should().Be(CodeChallengeMethod.S256);
	}

	[Fact]
	public void Constructor_SetsExpiresAt()
	{
		var before = DateTimeOffset.UtcNow;
		var authCode = new AuthorizationCode(
			"code_hash_value",
			TestUserId,
			TestClientId,
			"https://example.com/callback",
			"openid",
			new CodeChallenge(new string('A', 43)),
			CodeChallengeMethod.S256,
			DateTimeOffset.UtcNow.AddMinutes(5));

		authCode.ExpiresAt.Should().BeAfter(before);
	}

	[Fact]
	public void Constructor_WhenNonceProvided_SetsNonce()
	{
		var authCode = CreateAuthorizationCode(nonce: "test_nonce");

		authCode.Nonce.Should().Be("test_nonce");
	}

	[Fact]
	public void Constructor_WhenNoNonce_LeavesNonceNull()
	{
		var authCode = CreateAuthorizationCode();

		authCode.Nonce.Should().BeNull();
	}

	[Fact]
	public void Constructor_SetsUsedAtNull()
	{
		var authCode = CreateAuthorizationCode();

		authCode.UsedAt.Should().BeNull();
	}

	[Fact]
	public void MarkAsUsed_SetsUsedAt()
	{
		var authCode = CreateAuthorizationCode();

		authCode.MarkAsUsed();

		authCode.UsedAt.Should().NotBeNull();
		authCode.UsedAt!.Value.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(1));
	}

	[Fact]
	public void MarkAsUsed_WhenCalledTwice_ThrowsDomainException()
	{
		var authCode = CreateAuthorizationCode();
		authCode.MarkAsUsed();

		var act = () => authCode.MarkAsUsed();

		act.Should().Throw<DomainException>()
			.WithMessage("Authorization code has already been used.");
	}
}
