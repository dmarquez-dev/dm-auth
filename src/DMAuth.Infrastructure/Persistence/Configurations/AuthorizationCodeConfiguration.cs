using DMAuth.Domain.Entities;
using DMAuth.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DMAuth.Infrastructure.Persistence.Configurations;

/// <summary>
///		EF Core entity configuration for the <see cref="AuthorizationCode"/> entity.
/// </summary>
public class AuthorizationCodeConfiguration
	: IEntityTypeConfiguration<AuthorizationCode>
{
	/// <inheritdoc />
	public void Configure(
		EntityTypeBuilder<AuthorizationCode> builder)
	{
		builder.ToTable("AuthorizationCodes");

		builder.HasKey(authCode =>
			authCode.Id);

		builder.Property(authCode =>
			authCode.Id)
			.HasDefaultValueSql("NEWSEQUENTIALID()");

		builder.Property(authCode =>
			authCode.CodeHash)
			.HasMaxLength(512)
			.IsRequired();

		builder.Property(authCode =>
			authCode.UserId)
			.IsRequired();

		builder.Property(authCode =>
			authCode.ClientId)
			.IsRequired();

		builder.Property(authCode =>
			authCode.RedirectUri)
			.HasMaxLength(2000)
			.IsRequired();

		builder.Property(authCode =>
			authCode.Scopes)
			.HasMaxLength(1000)
			.IsRequired();

		builder.Property(authCode =>
			authCode.CodeChallenge)
			.HasConversion(
				challenge => challenge.Value,
				value => new CodeChallenge(value))
			.HasMaxLength(256)
			.IsRequired();

		builder.Property(authCode =>
			authCode.CodeChallengeMethod)
			.HasConversion<string>()
			.HasMaxLength(10)
			.IsRequired();

		builder.Property(authCode =>
			authCode.ExpiresAt)
			.IsRequired();

		builder.Property(authCode =>
			authCode.CreatedAt)
			.IsRequired();

		builder.HasIndex(authCode =>
			authCode.CodeHash)
			.IsUnique();

		builder.HasOne<User>()
			.WithMany()
			.HasForeignKey(authCode =>
				authCode.UserId)
			.OnDelete(DeleteBehavior.Cascade);

		builder.HasOne<Client>()
			.WithMany()
			.HasForeignKey(authCode =>
				authCode.ClientId)
			.OnDelete(DeleteBehavior.Cascade);
	}
}
