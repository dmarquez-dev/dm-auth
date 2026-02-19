using DMAuth.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DMAuth.Infrastructure.Persistence.Configurations;

/// <summary>
///		EF Core entity configuration for the <see cref="RefreshToken"/> entity.
/// </summary>
public class RefreshTokenConfiguration
	: IEntityTypeConfiguration<RefreshToken>
{
	/// <inheritdoc />
	public void Configure(
		EntityTypeBuilder<RefreshToken> builder)
	{
		builder.ToTable("RefreshTokens");

		builder.HasKey(token =>
			token.Id);

		builder.Property(token =>
			token.Id)
			.HasDefaultValueSql("NEWSEQUENTIALID()");

		builder.Property(token =>
			token.TokenHash)
			.HasMaxLength(512)
			.IsRequired();

		builder.Property(token =>
			token.UserId)
			.IsRequired();

		builder.Property(token =>
			token.ClientId)
			.IsRequired();

		builder.Property(token =>
			token.ExpiresAt)
			.IsRequired();

		builder.Property(token =>
			token.RevokedAt);

		builder.Property(token =>
			token.ReplacedByToken)
			.HasMaxLength(512);

		builder.Property(token =>
			token.CreatedAt)
			.IsRequired();

		builder.HasIndex(token =>
			token.TokenHash)
			.IsUnique();

		builder.HasIndex(token =>
			new { token.UserId, token.ClientId });

		builder.HasOne<User>()
			.WithMany()
			.HasForeignKey(token =>
				token.UserId)
			.OnDelete(DeleteBehavior.Cascade);

		builder.HasOne<Client>()
			.WithMany()
			.HasForeignKey(token =>
				token.ClientId)
			.OnDelete(DeleteBehavior.Cascade);
	}
}
