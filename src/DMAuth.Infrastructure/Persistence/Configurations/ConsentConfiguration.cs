using DMAuth.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DMAuth.Infrastructure.Persistence.Configurations;

/// <summary>
///		EF Core entity configuration for the <see cref="Consent"/> entity.
/// </summary>
public class ConsentConfiguration
	: IEntityTypeConfiguration<Consent>
{
	/// <inheritdoc />
	public void Configure(EntityTypeBuilder<Consent> builder)
	{
		builder.ToTable("Consents");

		builder.HasKey(consent =>
			consent.Id);

		builder.Property(consent =>
			consent.Id)
			.HasDefaultValueSql("NEWSEQUENTIALID()");

		builder.Property(consent =>
			consent.UserId)
			.IsRequired();

		builder.Property(consent =>
			consent.ClientId)
			.IsRequired();

		builder.Property(consent =>
			consent.GrantedScopes)
			.HasMaxLength(1000)
			.IsRequired();

		builder.Property(consent =>
			consent.GrantedAt)
			.IsRequired();

		builder.Property(consent =>
			consent.CreatedAt)
			.IsRequired();

		builder.HasIndex(consent =>
			new { consent.UserId, consent.ClientId })
			.IsUnique();

		builder.HasOne<User>()
			.WithMany()
			.HasForeignKey(consent =>
				consent.UserId)
			.OnDelete(DeleteBehavior.Cascade);

		builder.HasOne<Client>()
			.WithMany()
			.HasForeignKey(consent =>
				consent.ClientId)
			.OnDelete(DeleteBehavior.Cascade);
	}
}
