using Domain.Entities.Reservations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Configurations.Reservations;

public sealed class ReservationConfiguration : IEntityTypeConfiguration<Reservation>
{
  public void Configure(EntityTypeBuilder<Reservation> builder)
  {
    // ==================== TABLE ====================
    builder.ToTable("Reservations");

    // ==================== PRIMARY KEY ====================
    builder.HasKey(r => r.Id);

    builder.Property(r => r.Id)
        .HasColumnName("Id")
        .HasColumnOrder(0);

    // ==================== KİMLİK ====================
    builder.Property(r => r.ReservationNumber)
        .IsRequired()
        .HasMaxLength(30)
        .HasColumnName("ReservationNumber")
        .HasColumnOrder(1);

    // ==================== İLİŞKİLER (FK) ====================
    builder.Property(r => r.CustomerId)
        .IsRequired()
        .HasColumnName("CustomerId")
        .HasColumnOrder(2);

    builder.Property(r => r.VehicleId)
        .IsRequired()
        .HasColumnName("VehicleId")
        .HasColumnOrder(3);

    builder.Property(r => r.PickupBranchId)
        .IsRequired()
        .HasColumnName("PickupBranchId")
        .HasColumnOrder(4);

    builder.Property(r => r.ReturnBranchId)
        .HasColumnName("ReturnBranchId")
        .HasColumnOrder(5);

    // ==================== PLANLANAN TARİHLER ====================
    builder.Property(r => r.PlannedStartDate)
        .IsRequired()
        .HasColumnType("timestamp with time zone")
        .HasColumnName("PlannedStartDate")
        .HasColumnOrder(6);

    builder.Property(r => r.PlannedEndDate)
        .IsRequired()
        .HasColumnType("timestamp with time zone")
        .HasColumnName("PlannedEndDate")
        .HasColumnOrder(7);

    builder.Property(r => r.PlannedDays)
        .IsRequired()
        .HasColumnName("PlannedDays")
        .HasColumnOrder(8);

    // ==================== FİİLİ TARİHLER ====================
    builder.Property(r => r.ActualPickupAt)
        .HasColumnType("timestamp with time zone")
        .HasColumnName("ActualPickupAt")
        .HasColumnOrder(9);

    builder.Property(r => r.ActualReturnAt)
        .HasColumnType("timestamp with time zone")
        .HasColumnName("ActualReturnAt")
        .HasColumnOrder(10);

    // ==================== FİYATLANDIRMA ====================
    builder.Property(r => r.DailyPrice)
        .IsRequired()
        .HasPrecision(18, 2)
        .HasColumnName("DailyPrice")
        .HasColumnOrder(11);

    builder.Property(r => r.TotalPrice)
        .IsRequired()
        .HasPrecision(18, 2)
        .HasColumnName("TotalPrice")
        .HasColumnOrder(12);

    builder.Property(r => r.BillablePrice)
        .HasPrecision(18, 2)
        .HasColumnName("BillablePrice")
        .HasColumnOrder(13);

    builder.Property(r => r.DepositAmount)
        .IsRequired()
        .HasPrecision(18, 2)
        .HasColumnName("DepositAmount")
        .HasColumnOrder(14);

    builder.Property(r => r.RemainingAmount)
        .IsRequired()
        .HasPrecision(18, 2)
        .HasColumnName("RemainingAmount")
        .HasColumnOrder(15);

    // ==================== ÖDEME ====================
    builder.Property(r => r.DepositPaid)
        .IsRequired()
        .HasDefaultValue(false)
        .HasColumnName("DepositPaid")
        .HasColumnOrder(16);

    builder.Property(r => r.DepositPaidAt)
        .HasColumnType("timestamp with time zone")
        .HasColumnName("DepositPaidAt")
        .HasColumnOrder(17);

    builder.Property(r => r.PaymentId)
        .HasMaxLength(100)
        .HasColumnName("PaymentId")
        .HasColumnOrder(18);

    // ==================== DURUM ====================
    builder.Property(r => r.Status)
        .IsRequired()
        .HasConversion<string>()
        .HasComment("PendingPayment, Confirmed, Active, Completed, Expired, NoShow, Cancelled")
        .HasColumnName("Status")
        .HasColumnOrder(19);

    builder.Property(r => r.PaymentDeadline)
        .IsRequired()
        .HasColumnType("timestamp with time zone")
        .HasColumnName("PaymentDeadline")
        .HasColumnOrder(20);

    builder.Property(r => r.PickupDeadline)
        .HasColumnType("timestamp with time zone")
        .HasColumnName("PickupDeadline")
        .HasColumnOrder(21);

    builder.Property(r => r.ConfirmedAt)
        .HasColumnType("timestamp with time zone")
        .HasColumnName("ConfirmedAt")
        .HasColumnOrder(22);

    builder.Property(r => r.CompletedAt)
        .HasColumnType("timestamp with time zone")
        .HasColumnName("CompletedAt")
        .HasColumnOrder(23);

    builder.Property(r => r.CancelledAt)
        .HasColumnType("timestamp with time zone")
        .HasColumnName("CancelledAt")
        .HasColumnOrder(24);

    builder.Property(r => r.CancellationReason)
        .HasMaxLength(500)
        .HasColumnName("CancellationReason")
        .HasColumnOrder(25);

    // ==================== NOTLAR ====================
    builder.Property(r => r.Notes)
        .HasMaxLength(500)
        .HasColumnName("Notes")
        .HasColumnOrder(26);

    builder.Property(r => r.AdminNotes)
        .HasMaxLength(500)
        .HasColumnName("AdminNotes")
        .HasColumnOrder(27);

    // ==================== RELATIONSHIPS ====================

    // Reservation → Customer (Many-to-One)
    builder.HasOne(r => r.Customer)
        .WithMany(c => c.Reservations)
        .HasForeignKey(r => r.CustomerId)
        .OnDelete(DeleteBehavior.Restrict)
        .IsRequired();

    // Reservation → Vehicle (Many-to-One)
    builder.HasOne(r => r.Vehicle)
        .WithMany(v => v.Reservations)
        .HasForeignKey(r => r.VehicleId)
        .OnDelete(DeleteBehavior.Restrict)
        .IsRequired();

    // Reservation → PickupBranch (Many-to-One)
    builder.HasOne(r => r.PickupBranch)
        .WithMany(b => b.PickupReservations)
        .HasForeignKey(r => r.PickupBranchId)
        .OnDelete(DeleteBehavior.Restrict)
        .IsRequired();

    // Reservation → ReturnBranch (Many-to-One, nullable)
    builder.HasOne(r => r.ReturnBranch)
        .WithMany(b => b.ReturnReservations)
        .HasForeignKey(r => r.ReturnBranchId)
        .OnDelete(DeleteBehavior.Restrict)
        .IsRequired(false);

    // Reservation → RentalExtras (One-to-Many)
    builder.HasMany(r => r.Extras)
        .WithOne()
        .HasForeignKey(re => re.RentalId)
        .OnDelete(DeleteBehavior.Cascade);

    // ==================== INDEXES ====================

    // Rezervasyon numarası unique
    builder.HasIndex(r => r.ReservationNumber)
        .IsUnique()
        .HasDatabaseName("IX_Reservations_ReservationNumber")
        .HasFilter("\"IsDeleted\" = false");

    // Filtreleme/sorgu için
    builder.HasIndex(r => r.CustomerId)
        .HasDatabaseName("IX_Reservations_CustomerId");

    builder.HasIndex(r => r.Status)
        .HasDatabaseName("IX_Reservations_Status");

    builder.HasIndex(r => r.PaymentDeadline)
        .HasDatabaseName("IX_Reservations_PaymentDeadline");

    builder.HasIndex(r => r.PickupDeadline)
        .HasDatabaseName("IX_Reservations_PickupDeadline");

    // ⭐ Araç müsaitlik kontrolü için composite index
    builder.HasIndex(r => new { r.VehicleId, r.Status, r.PlannedStartDate, r.PlannedEndDate })
        .HasDatabaseName("IX_Reservations_Vehicle_Status_DateRange");

    // Müşteri rezervasyonları
    builder.HasIndex(r => new { r.CustomerId, r.Status })
        .HasDatabaseName("IX_Reservations_Customer_Status");

    // ==================== IGNORED PROPERTIES ====================
    builder.Ignore(r => r.ExtrasTotalPrice);

  }
}
