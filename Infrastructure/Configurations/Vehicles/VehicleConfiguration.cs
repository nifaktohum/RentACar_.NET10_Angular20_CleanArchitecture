using Domain.Entities.Vehicles;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Configurations.Vehicles;

public sealed class VehicleConfiguration : IEntityTypeConfiguration<Vehicle>
{
  public void Configure(EntityTypeBuilder<Vehicle> builder)
  {

    builder.ToTable("Vehicles");

    builder.HasKey(x => x.Id);

    // ==================== PROPERTIES ====================
    builder.Property(v => v.Id)
    .HasColumnName("Id")
    .HasColumnOrder(0);
    
    builder.Property(v => v.Brand)
        .IsRequired()
        .HasMaxLength(50)
        .HasColumnName("Brand")
        .HasColumnOrder(1);

    builder.Property(v => v.Model)
        .IsRequired()
        .HasMaxLength(50)
        .HasColumnName("Model")
        .HasColumnOrder(2);

    builder.Property(v => v.Year)
        .IsRequired()
        .HasMaxLength(4)
        .HasColumnName("Year")
        .HasColumnOrder(3);

    builder.Property(v => v.Plate)
        .IsRequired()
        .HasMaxLength(10)
        .HasColumnName("Plate")
        .HasColumnOrder(4);

    builder.Property(v => v.Color)
        .IsRequired()
        .HasMaxLength(30)
        .HasColumnName("Color")
        .HasColumnOrder(5);

    builder.Property(v => v.FuelType)
        .IsRequired()
        .HasMaxLength(20)
        .HasColumnName("FuelType")
        .HasColumnOrder(6);

    builder.Property(v => v.Transmission)
        .IsRequired()
        .HasMaxLength(20)
        .HasColumnName("Transmission")
        .HasColumnOrder(7);

    builder.Property(v => v.SeatCount)
        .IsRequired()
        .HasColumnName("SeatCount")
        .HasColumnOrder(8);

    builder.Property(v => v.DoorCount)
        .IsRequired()
        .HasColumnName("DoorCount")
        .HasColumnOrder(9);

    builder.Property(v => v.MinAge)
        .HasColumnName("MinAge")
        .HasColumnOrder(10);

    builder.Property(v => v.DailyPrice)
        .IsRequired()
        .HasPrecision(18, 2)
        .HasColumnName("DailyPrice")
        .HasColumnOrder(11);

    builder.Property(v => v.IsAvailable)
        .IsRequired()
        .HasDefaultValue(true)
        .HasColumnName("IsAvailable")
        .HasColumnOrder(12);

    builder.Property(v => v.Description)
        .HasMaxLength(500)
        .HasColumnName("Description")
        .HasColumnOrder(13);

    // Index'ler
    builder.HasIndex(v => v.Plate)
        .IsUnique()
        .HasDatabaseName("IX_Vehicles_Plate")
        .HasFilter("\"IsDeleted\" = false");

    // ==================== RELATIONSHIPS ====================
    // VehicleModel ile ilişki
    builder.HasOne(v => v.VehicleModel)
            .WithMany(vm => vm.Vehicles)
            .HasForeignKey(v => v.VehicleModelId)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired();

    // Vehicle → VehicleImage (One-to-Many)
    builder.HasMany(v => v.Images)
        .WithOne(i => i.Vehicle)
        .HasForeignKey(i => i.VehicleId)
        .OnDelete(DeleteBehavior.Cascade);

    // ==================== INDEXES ====================
    builder.HasIndex(v => v.VehicleModelId)
        .HasDatabaseName("IX_Vehicles_VehicleModelId");

    builder.HasIndex(v => v.Brand)
        .HasDatabaseName("IX_Vehicles_Brand");

    builder.HasIndex(v => v.Model)
        .HasDatabaseName("IX_Vehicles_Model");

    builder.HasIndex(v => v.Year)
        .HasDatabaseName("IX_Vehicles_Year");

    builder.HasIndex(v => v.IsAvailable)
        .HasDatabaseName("IX_Vehicles_IsAvailable");

    builder.HasIndex(v => new { v.Brand, v.Model, v.Year })
        .HasDatabaseName("IX_Vehicles_Brand_Model_Year");

    // ==================== QUERY FILTERS ====================
    builder.HasQueryFilter(v => !v.IsDeleted);
  }
}

