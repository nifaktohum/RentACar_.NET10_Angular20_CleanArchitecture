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
        .HasMaxLength(20)
        .HasColumnName("Plate")
        .HasColumnOrder(4);

    builder.Property(v => v.Color)
        .IsRequired()
        .HasMaxLength(30)
        .HasColumnName("Color")
        .HasColumnOrder(5);

    builder.Property(v => v.VinNumber)
        .IsRequired()
        .HasMaxLength(17)
        .IsFixedLength()
        .HasColumnName("VinNumber")
        .HasColumnOrder(6);

    builder.Property(v => v.EngineNumber)
        .IsRequired()
        .HasMaxLength(30)
        .HasColumnName("EngineNumber")
        .HasColumnOrder(7);

    builder.Property(v => v.FuelType)
        .IsRequired()
        .HasMaxLength(20)
        .HasColumnName("FuelType")
        .HasColumnOrder(8);

    builder.Property(v => v.Transmission)
        .IsRequired()
        .HasMaxLength(20)
        .HasColumnName("Transmission")
        .HasColumnOrder(9);

    builder.Property(v => v.TractionType)
        .IsRequired()
        .HasMaxLength(20)
        .HasColumnName("TractionType")
        .HasColumnOrder(10);

    builder.Property(v => v.EngineVolume)
        .IsRequired()
        .HasPrecision(5, 2)
        .HasColumnName("EngineVolume")
        .HasColumnOrder(11);

    builder.Property(v => v.EnginePower)
        .IsRequired()
        .HasColumnName("EnginePower")
        .HasColumnOrder(12);

    builder.Property(v => v.FuelConsumption)
        .IsRequired()
        .HasPrecision(5, 2)
        .HasColumnName("FuelConsumption")
        .HasColumnOrder(13);

    builder.Property(v => v.SeatCount)
        .IsRequired()
        .HasColumnName("SeatCount")
        .HasColumnOrder(14);

    builder.Property(v => v.DoorCount)
        .IsRequired()
        .HasColumnName("DoorCount")
        .HasColumnOrder(15);

    builder.Property(v => v.MinAge)
        .HasColumnName("MinAge")
        .HasColumnOrder(16);

    builder.Property(v => v.DailyPrice)
        .IsRequired()
        .HasPrecision(18, 2)
        .HasColumnName("DailyPrice")
        .HasColumnOrder(17);

    builder.Property(v => v.DepositAmount)
        .IsRequired()
        .HasPrecision(18, 2)
        .HasColumnName("DepositAmount")
        .HasColumnOrder(18);

    builder.Property(v => v.WeeklyDiscountRate)
        .IsRequired()
        .HasPrecision(5, 2)
        .HasColumnName("WeeklyDiscountRate")
        .HasColumnOrder(19);

    builder.Property(v => v.MonthlyDiscountRate)
        .IsRequired()
        .HasPrecision(5, 2)
        .HasColumnName("MonthlyDiscountRate")
        .HasColumnOrder(20);

    builder.Property(v => v.CurrentKilometer)
        .IsRequired()
        .HasColumnName("CurrentKilometer")
        .HasColumnOrder(21);

    builder.Property(v => v.LastMaintenanceKm)
        .IsRequired()
        .HasColumnName("LastMaintenanceKm")
        .HasColumnOrder(22);

    builder.Property(v => v.NextMaintenanceKm)
        .IsRequired()
        .HasColumnName("NextMaintenanceKm")
        .HasColumnOrder(23);

    builder.Property(v => v.LastMaintenanceDate)
        .IsRequired()
        .HasColumnType("timestamp with time zone")
        .HasColumnName("LastMaintenanceDate")
        .HasColumnOrder(24);

    builder.Property(v => v.InspectionDate)
        .IsRequired()
        .HasColumnType("timestamp with time zone")
        .HasColumnName("InspectionDate")
        .HasColumnOrder(25);

    builder.Property(v => v.InsuranceEndDate)
        .IsRequired()
        .HasColumnType("timestamp with time zone")
        .HasColumnName("InsuranceEndDate")
        .HasColumnOrder(26);

    builder.Property(v => v.CascoEndDate)
        .IsRequired()
        .HasColumnType("timestamp with time zone")
        .HasColumnName("CascoEndDate")
        .HasColumnOrder(27);

    builder.Property(v => v.TireStatus)
        .IsRequired()
        .HasMaxLength(50)
        .HasColumnName("TireStatus")
        .HasColumnOrder(28);

    builder.Property(v => v.GeneralStatus)
        .IsRequired()
        .HasMaxLength(50)
        .HasColumnName("GeneralStatus")
        .HasColumnOrder(29);

    builder.Property(v => v.Description)
        .HasMaxLength(1000)
        .HasColumnName("Description")
        .HasColumnOrder(30);

    builder.Property(v => v.Status)
        .IsRequired()
        .HasConversion<int>()
        .HasDefaultValue(VehicleStatus.Available)
        .HasColumnName("Status")
        .HasColumnOrder(31);

    // ==================== AUDIT / SOFT DELETE ====================

    builder.Property(v => v.IsActive)
        .IsRequired()
        .HasDefaultValue(true)
        .HasColumnName("IsActive")
        .HasColumnOrder(32);

    builder.Property(v => v.IsDeleted)
        .IsRequired()
        .HasDefaultValue(false)
        .HasColumnName("IsDeleted")
        .HasColumnOrder(33);

    builder.Property(v => v.CreatedAt)
        .IsRequired()
        .HasColumnType("timestamp with time zone")
        .HasColumnName("CreatedAt")
        .HasColumnOrder(34);

    builder.Property(v => v.UpdatedAt)
        .HasColumnType("timestamp with time zone")
        .HasColumnName("UpdatedAt")
        .HasColumnOrder(35);

    builder.Property(v => v.CreatedBy)
        .IsRequired()
        .HasColumnName("CreatedBy")
        .HasColumnOrder(36);

    builder.Property(v => v.UpdatedBy)
        .HasColumnName("UpdatedBy")
        .HasColumnOrder(37);

    // ==================== RELATIONSHIPS ====================

    // Vehicle → VehicleModel (Many-to-One)
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

    // Vehicle → Reservation (One-to-Many)
    builder.HasMany(v => v.Reservations)
        .WithOne(r => r.Vehicle)
        .HasForeignKey(r => r.VehicleId)
        .OnDelete(DeleteBehavior.Restrict);

    // ==================== INDEXES ====================

    builder.HasIndex(v => v.Plate)
        .IsUnique()
        .HasDatabaseName("IX_Vehicles_Plate")
        .HasFilter("\"IsDeleted\" = false");

    builder.HasIndex(v => v.VinNumber)
        .IsUnique()
        .HasDatabaseName("IX_Vehicles_VinNumber")
        .HasFilter("\"IsDeleted\" = false");

    builder.HasIndex(v => v.EngineNumber)
        .IsUnique()
        .HasDatabaseName("IX_Vehicles_EngineNumber")
        .HasFilter("\"IsDeleted\" = false");

    builder.HasIndex(v => v.VehicleModelId)
        .HasDatabaseName("IX_Vehicles_VehicleModelId");

    builder.HasIndex(v => v.BranchId)
        .HasDatabaseName("IX_Vehicles_BranchId");

    builder.HasIndex(v => v.Status)
        .HasDatabaseName("IX_Vehicles_Status");

    builder.HasIndex(v => v.Brand)
        .HasDatabaseName("IX_Vehicles_Brand");

    builder.HasIndex(v => v.Model)
        .HasDatabaseName("IX_Vehicles_Model");

    builder.HasIndex(v => v.Year)
        .HasDatabaseName("IX_Vehicles_Year");

    builder.HasIndex(v => new { v.Brand, v.Model, v.Year })
        .HasDatabaseName("IX_Vehicles_Brand_Model_Year");

    builder.HasIndex(v => new { v.Status, v.IsActive, v.IsDeleted })
        .HasDatabaseName("IX_Vehicles_Status_IsActive_IsDeleted");

    builder.HasIndex(v => new { v.CurrentKilometer, v.NextMaintenanceKm })
        .HasDatabaseName("IX_Vehicles_MaintenanceKm");

    // ==================== QUERY FILTERS ====================
    builder.HasQueryFilter(v => !v.IsDeleted);

    // ==================== IGNORED PROPERTIES (computed) ====================
    // ⭐ EF Core bu property'leri kolon olarak yansıtmaz
    builder.Ignore(v => v.IsAvailable);
    builder.Ignore(v => v.IsMaintenanceDue);
    builder.Ignore(v => v.IsLegallyCompliantNow);
    builder.Ignore(v => v.CanBeReservedNow);
  }
}