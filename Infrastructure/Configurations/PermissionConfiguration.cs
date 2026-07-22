using Domain.Entities.Roles;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Configurations;

public sealed class PermissionConfiguration : IEntityTypeConfiguration<Permission>
{
  public void Configure(EntityTypeBuilder<Permission> builder)
  {
    builder.ToTable("Permissions");

    builder.HasKey(p => p.Id);

    builder.Property(p => p.Name)
           .HasMaxLength(100)
           .IsRequired();

    builder.HasIndex(p => p.Name).IsUnique();

    builder.Property(p => p.Description)
           .HasMaxLength(250)
           .IsRequired();

    // =====================================================================================
    // 🛡️ SOFT DELETE VE AKTİFLİK VERİTABANI POLİTİKALARI
    // =====================================================================================
    builder.Property(p => p.IsActive)
           .HasDefaultValue(true) // SQL tarafında default: true (1)
           .IsRequired();

    builder.Property(p => p.IsDeleted)
           .HasDefaultValue(false) // SQL tarafında default: false (0)
           .IsRequired();

    builder.Property(p => p.DeletedDate)
           .IsRequired(false); // Silinene kadar null kalabilir 

    // Artık bu tabloya atılan tüm sorgularda silinenler otomatik elenecek!
    builder.HasQueryFilter(p => !p.IsDeleted && p.IsActive);

    // =====================================================================================
    // 🚀 RELATIONSHIP & DDD BACKGROUND FIELD CONFIGURATION (SIHIRLI DOKUNUŞ)
    // =====================================================================================
    // EF Core'a diyoruz ki: "Dışarıdaki IReadOnlyCollection'a kafana göre veri eklemeye çalışma, 
    // arka planda duran private '_permissionRoles' listesine doğrudan eriş!"
    builder.HasMany(p => p.PermissionRoles)
           .WithOne(pr => pr.Permission)
           .HasForeignKey(pr => pr.PermissionId)
           .Metadata.PrincipalToDependent!.SetPropertyAccessMode(PropertyAccessMode.Field);
  }
}