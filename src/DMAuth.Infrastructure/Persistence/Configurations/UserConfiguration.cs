using DMAuth.Domain.Entities;
using DMAuth.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DMAuth.Infrastructure.Persistence.Configurations;

/// <summary>
///		EF Core entity configuration for the <see cref="User"/> entity.
/// </summary>
public class UserConfiguration
	: IEntityTypeConfiguration<User>
{
	/// <inheritdoc />
	public void Configure(EntityTypeBuilder<User> builder)
	{
		builder.ToTable("Users");

		builder.HasKey(user =>
			user.Id);

		builder.Property(user =>
			user.Id)
			.HasDefaultValueSql("NEWSEQUENTIALID()");

		builder.Property(user =>
			user.Email)
			.HasConversion(
				email => email.Value,
				value => new Email(value))
			.HasMaxLength(256)
			.IsRequired();

		builder.Property(user =>
			user.Username)
			.HasMaxLength(100)
			.IsRequired();

		builder.Property(user =>
			user.HashedPassword)
			.HasConversion(
				hash => hash.Value,
				value => new HashedPassword(value))
			.HasMaxLength(512)
			.IsRequired();

		builder.Property(user =>
			user.DisplayName)
			.HasMaxLength(200)
			.IsRequired();

		builder.Property(user =>
			user.IsActive)
			.IsRequired();

		builder.Property(user =>
			user.EmailVerified)
			.IsRequired();

		builder.Property(user =>
			user.CreatedAt)
			.IsRequired();

		builder.Property(user
			=> user.UpdatedAt);

		builder.HasIndex(user =>
			user.Email)
			.IsUnique();

		builder.HasIndex(user =>
			user.Username)
			.IsUnique();
	}
}
