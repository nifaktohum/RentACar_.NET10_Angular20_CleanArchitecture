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

    builder.Property(vm => vm.Stock)
        .IsRequired()
        .HasDefaultValue(0)
        .HasColumnName("Stock")
        .HasColumnOrder(4);

    builder.Property(vm => vm.AvailableStock)
        .IsRequired()
        .HasDefaultValue(0)
        .HasColumnName("AvailableStock")
        .HasColumnOrder(5);

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
        .OnDelete(DeleteBehavior.Restrict);

    // ==================== INDEXES ====================
    // Arama performansı için
    builder.HasIndex(vm => new { vm.Brand, vm.Name })
        .IsUnique()
        .HasDatabaseName("IX_VehicleModels_Brand_Name")
        .HasFilter("\"IsDeleted\" = false"); ;

    builder.HasIndex(vm => vm.Brand)
        .HasDatabaseName("IX_VehicleModels_Brand");

    builder.HasIndex(vm => vm.Name)
        .HasDatabaseName("IX_VehicleModels_Name");

    builder.HasIndex(vm => vm.VehicleTypeId)
        .HasDatabaseName("IX_VehicleModels_VehicleTypeId");

    builder.HasIndex(vm => vm.Stock)
        .HasDatabaseName("IX_VehicleModels_Stock");

    builder.HasIndex(vm => vm.AvailableStock)
        .HasDatabaseName("IX_VehicleModels_AvailableStock");

    // ==================== QUERY FILTERS ====================
    // Soft delete filter
    builder.HasQueryFilter(vm => !vm.IsDeleted);

    // ==================== IGNORED PROPERTIES ====================
    builder.Ignore(vm => vm.IsInStock);
  }
}
