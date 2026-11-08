using Domain.Entities.Vehicles;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Configurations.Vehicles;

public sealed class VehicleTypeConfiguration : IEntityTypeConfiguration<VehicleType>
{
  public void Configure(EntityTypeBuilder<VehicleType> builder)
  {
    builder.ToTable("VehicleTypes");

    // ==================== PRIMARY KEY ====================
    builder.HasKey(vt => vt.Id);

    // ==================== PROPERTIES ====================
    builder.Property(vt => vt.Id)
        .HasColumnName("Id")
        .HasColumnOrder(0);

    builder.Property(vt => vt.Name)
        .IsRequired()
        .HasMaxLength(50)
        .HasColumnName("Name")
        .HasColumnOrder(1);

    builder.Property(vt => vt.Description)
        .HasMaxLength(200)
        .HasColumnName("Description")
        .HasColumnOrder(2);

    builder.Property(vt => vt.Icon)
        .HasMaxLength(50)
        .HasColumnName("Icon")
        .HasColumnOrder(3);

    builder.Property(vt => vt.DisplayOrder)
        .IsRequired()
        .HasDefaultValue(0)
        .HasColumnName("DisplayOrder")
        .HasColumnOrder(4);

    // ==================== AUDIT / SOFT DELETE ====================
    // (BaseEntityConfiguration varsa SİL)
    builder.Property(vt => vt.IsActive)
        .IsRequired()
        .HasDefaultValue(true)
        .HasColumnName("IsActive")
        .HasColumnOrder(5);

    builder.Property(vt => vt.IsDeleted)
        .IsRequired()
        .HasDefaultValue(false)
        .HasColumnName("IsDeleted")
        .HasColumnOrder(6);

    builder.Property(vt => vt.CreatedAt)
        .IsRequired()
        .HasColumnName("CreatedAt")
        .HasColumnOrder(7);

    builder.Property(vt => vt.UpdatedAt)
        .HasColumnName("UpdatedAt")
        .HasColumnOrder(8);

    builder.Property(vt => vt.CreatedBy)
        .IsRequired()
        .HasColumnName("CreatedBy")
        .HasColumnOrder(9);

    builder.Property(vt => vt.UpdatedBy)
        .HasColumnName("UpdatedBy")
        .HasColumnOrder(10);

    // ==================== RELATIONSHIPS ====================
    builder.HasMany(vt => vt.VehicleModels)
        .WithOne(vm => vm.VehicleType)
        .HasForeignKey(vm => vm.VehicleTypeId)
        .OnDelete(DeleteBehavior.Restrict)
        .IsRequired();          // 🆕

    // ==================== INDEXES ====================

    // Unique: Name (soft delete hariç)
    builder.HasIndex(vt => vt.Name)
        .IsUnique()
        .HasDatabaseName("UX_VehicleTypes_Name")   // 🆕 IX → UX
        .HasFilter("\"IsDeleted\" = false");

    builder.HasIndex(vt => vt.DisplayOrder)
        .HasDatabaseName("IX_VehicleTypes_DisplayOrder");

    // ==================== QUERY FILTERS ====================
    builder.HasQueryFilter(vt => !vt.IsDeleted);
  }
}