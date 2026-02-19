using DMAuth.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DMAuth.Infrastructure.Persistence.Configurations;

/// <summary>
///		EF Core entity configuration for the <see cref="Client"/> entity.
/// </summary>
public class ClientConfiguration
	: IEntityTypeConfiguration<Client>
{
	/// <inheritdoc />
	public void Configure(
		EntityTypeBuilder<Client> builder)
	{
		builder.ToTable("Clients");

		builder.HasKey(client =>
			client.Id);

		builder.Property(client =>
			client.Id)
			.HasDefaultValueSql("NEWSEQUENTIALID()");

		builder.Property(client =>
			client.ClientId)
			.HasMaxLength(100)
			.IsRequired();

		builder.Property(client =>
			client.ClientName)
			.HasMaxLength(200)
			.IsRequired();

		builder.Property(client =>
			client.ClientSecretHash)
			.HasMaxLength(512);

		builder.Property(client =>
			client.ClientType)
			.HasConversion<string>()
			.HasMaxLength(20)
			.IsRequired();

		builder.Property(client =>
			client.OwnerId)
			.IsRequired();

		builder.Property(client =>
			client.IsActive)
			.IsRequired();

		builder.Property<List<string>>("_redirectUris")
			.HasColumnName("RedirectUris")
			.HasColumnType("nvarchar(max)");

		builder.Property<List<string>>("_allowedScopes")
			.HasColumnName("AllowedScopes")
			.HasColumnType("nvarchar(max)");

		builder.Property(client =>
			client.CreatedAt)
			.IsRequired();

		builder.Property(client =>
			client.UpdatedAt);

		builder.HasIndex(client =>
			client.ClientId)
			.IsUnique();

		builder.HasOne<User>()
			.WithMany()
			.HasForeignKey(client =>
				client.OwnerId)
			.OnDelete(DeleteBehavior.Restrict);
	}
}
