using Domain.Roles;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Configurations;

public sealed class PermissionRoleConfiguration : IEntityTypeConfiguration<PermissionRole>
{
  public void Configure(EntityTypeBuilder<PermissionRole> builder)
  {
    builder.ToTable("PermissionRoles");

    // Composite Primary Key
    builder.HasKey(pr => new { pr.RoleId, pr.PermissionId });

    // ⭐ QUERY FILTER EKLE (Permission ile aynı mantık)
    builder.HasQueryFilter(pr => !pr.Permission.IsDeleted && pr.Permission.IsActive);

    // 🛡️ Role ile İlişki
    builder.HasOne(pr => pr.Role)
           .WithMany(r => r.PermissionRoles)
           .HasForeignKey(pr => pr.RoleId)
           .OnDelete(DeleteBehavior.Cascade)
           .Metadata.DependentToPrincipal!.SetPropertyAccessMode(PropertyAccessMode.Field);

    // 🛡️ Permission ile İlişki
    builder.HasOne(pr => pr.Permission)
           .WithMany(p => p.PermissionRoles)
           .HasForeignKey(pr => pr.PermissionId)
           .OnDelete(DeleteBehavior.Cascade)
           .Metadata.DependentToPrincipal!.SetPropertyAccessMode(PropertyAccessMode.Field);
  }
}
