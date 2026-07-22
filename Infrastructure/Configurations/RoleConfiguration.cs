using Domain.Entities.Roles;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Configurations;

public sealed class RoleConfiguration : IEntityTypeConfiguration<Role>
{
  public void Configure(EntityTypeBuilder<Role> builder)
  {
    builder.ToTable("Roles");

    // Primary Key
    builder.HasKey(r => r.Id);

    // Rol Adı (Sınırlandırılmış ve Benzersiz)
    builder.Property(r => r.Name)
           .HasMaxLength(50)
           .IsRequired();

    builder.HasIndex(r => r.Name).IsUnique();

    builder.Property(r => r.Description)
               .HasMaxLength(250) // FluentValidation ile senkronize ettik kanka
               .IsRequired();

  }
}
