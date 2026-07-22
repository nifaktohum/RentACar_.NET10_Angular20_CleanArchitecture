using Domain.Entities.Roles;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Configurations;

public sealed class UserRoleConfiguration : IEntityTypeConfiguration<UserRole>
{
  public void Configure(EntityTypeBuilder<UserRole> builder)
  {
    builder.ToTable("UserRoles");

    // Composite Primary Key
    builder.HasKey(ur => new { ur.UserId, ur.RoleId });

    // User ile İlişki
    builder.HasOne(ur => ur.User)
           .WithMany(u => u.UserRoles)
           .HasForeignKey(ur => ur.UserId)
           .OnDelete(DeleteBehavior.Cascade);

    // Role ile İlişki
    builder.HasOne(ur => ur.Role)
           .WithMany(r => r.UserRoles)
           .HasForeignKey(ur => ur.RoleId)
           .OnDelete(DeleteBehavior.Cascade);

    // UserRole üzerinden User'a git ve silinmemiş olanları filtrele kanks!
    builder.HasQueryFilter(ur => !ur.User.IsDeleted);
  }
}