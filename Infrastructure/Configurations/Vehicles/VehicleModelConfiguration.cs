using Domain.Entities.Vehicles;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Configurations.Vehicles;

public class VehicleModelConfiguration : IEntityTypeConfiguration<VehicleModel>
{
  public void Configure(EntityTypeBuilder<VehicleModel> builder)
  {
    // ==================== TABLE NAME ====================
    builder.ToTable("VehicleModels");

    // ==================== PRIMARY KEY ====================
    builder.HasKey(vm => vm.Id);

    // ==================== PROPERTIES ====================

    builder.Property(vm => vm.Id)
        .HasColumnName("Id")
        .HasColumnOrder(0);

    builder.Property(vm => vm.Brand)
        .IsRequired()
        .HasMaxLength(50)
        .HasColumnName("Brand")
        .HasColumnOrder(1);

    builder.Property(vm => vm.Name)
        .IsRequired()
        .HasMaxLength(100)
        .HasColumnName("Name")
        .HasColumnOrder(2);

    builder.Property(vm => vm.Description)
        .HasMaxLength(500)
        .HasColumnName("Description")
        .HasColumnOrder(3);

    // ❌ Stock ve AvailableStock DB'de YOK — computed property
    // builder.Property(vm => vm.Stock)...
    // builder.Property(vm => vm.AvailableStock)...

    // ==================== AUDIT / SOFT DELETE ====================
    // (BaseEntityConfiguration varsa SİL)
    builder.Property(vm => vm.IsActive)
        .IsRequired()
        .HasDefaultValue(true)
        .HasColumnName("IsActive")
        .HasColumnOrder(4);

    builder.Property(vm => vm.IsDeleted)
        .IsRequired()
        .HasDefaultValue(false)
        .HasColumnName("IsDeleted")
        .HasColumnOrder(5);

    builder.Property(vm => vm.CreatedAt)
        .IsRequired()
        .HasColumnName("CreatedAt")
        .HasColumnOrder(6);

    builder.Property(vm => vm.UpdatedAt)
        .HasColumnName("UpdatedAt")
        .HasColumnOrder(7);

    builder.Property(vm => vm.CreatedBy)
        .IsRequired()
        .HasColumnName("CreatedBy")
        .HasColumnOrder(8);

    builder.Property(vm => vm.UpdatedBy)
        .HasColumnName("UpdatedBy")
        .HasColumnOrder(9);

    // ==================== RELATIONSHIPS ====================

    // VehicleType ile ilişki
    builder.HasOne(vm => vm.VehicleType)
        .WithMany(vt => vt.VehicleModels)
        .HasForeignKey(vm => vm.VehicleTypeId)
        .OnDelete(DeleteBehavior.Restrict)
        .IsRequired();

    // Vehicle ile ilişki (one-to-many)
    builder.HasMany(vm => vm.Vehicles)
        .WithOne(v => v.VehicleModel)
        .HasForeignKey(v => v.VehicleModelId)
        .OnDelete(DeleteBehavior.Restrict)
        .IsRequired();          // 🆕

    // ==================== INDEXES ====================

    // Unique: Brand + Name (soft delete hariç)
    builder.HasIndex(vm => new { vm.Brand, vm.Name })
        .IsUnique()
        .HasDatabaseName("UX_VehicleModels_Brand_Name")
        .HasFilter("\"IsDeleted\" = false");

    builder.HasIndex(vm => vm.Brand)
        .HasDatabaseName("IX_VehicleModels_Brand");

    builder.HasIndex(vm => vm.Name)
        .HasDatabaseName("IX_VehicleModels_Name");

    builder.HasIndex(vm => vm.VehicleTypeId)
        .HasDatabaseName("IX_VehicleModels_VehicleTypeId");

    // ❌ Stock / AvailableStock index'leri SİL (computed, kolon yok)
    // builder.HasIndex(vm => vm.Stock)...
    // builder.HasIndex(vm => vm.AvailableStock)...

    // ==================== QUERY FILTERS ====================
    builder.HasQueryFilter(vm => !vm.IsDeleted);

    // ==================== IGNORED PROPERTIES ====================
    builder.Ignore(vm => vm.Stock);            // 🆕 computed
    builder.Ignore(vm => vm.AvailableStock);   // 🆕 computed
    builder.Ignore(vm => vm.IsInStock);        // computed
  }
}