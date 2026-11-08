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

    // ==================== ÖDEME: DEPOZİTO (Katman 2) ====================
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

    // Depozito sonrası 24 saatlik kilit
    builder.Property(r => r.DepositLockDeadline)
        .HasColumnType("timestamp with time zone")
        .HasColumnName("DepositLockDeadline")
        .HasColumnOrder(19);

    // ==================== ÖDEME: TAM ÖDEME (Katman 3) ====================
    builder.Property(r => r.FullPaymentPaid)
        .IsRequired()
        .HasDefaultValue(false)
        .HasColumnName("FullPaymentPaid")
        .HasColumnOrder(20);

    builder.Property(r => r.FullPaymentPaidAt)
        .HasColumnType("timestamp with time zone")
        .HasColumnName("FullPaymentPaidAt")
        .HasColumnOrder(21);

    builder.Property(r => r.FullPaymentId)
        .HasMaxLength(100)
        .HasColumnName("FullPaymentId")
        .HasColumnOrder(22);

    // ==================== İADE YÖNETİMİ ====================
    builder.Property(r => r.IsDepositRefunded)
        .HasColumnName("IsDepositRefunded")
        .HasColumnOrder(32);

    builder.Property(r => r.RefundedAmount)
        .HasPrecision(18, 2)
        .HasColumnName("RefundedAmount")
        .HasColumnOrder(33);

    builder.Property(r => r.ForfeitedAmount)
        .HasPrecision(18, 2)
        .HasColumnName("ForfeitedAmount")
        .HasColumnOrder(34);

    // ==================== DURUM ====================
    builder.Property(r => r.Status)
        .IsRequired()
        .HasConversion<string>()
        .HasComment("PendingPayment, Confirmed, Active, Completed, Expired, Cancelled")
        .HasColumnName("Status")
        .HasColumnOrder(23);

    builder.Property(r => r.PaymentDeadline)
        .IsRequired()
        .HasColumnType("timestamp with time zone")
        .HasColumnName("PaymentDeadline")
        .HasColumnOrder(24);

    // ❌ PickupDeadline kaldırıldı (entity'den silindi)

    builder.Property(r => r.ConfirmedAt)
        .HasColumnType("timestamp with time zone")
        .HasColumnName("ConfirmedAt")
        .HasColumnOrder(25);

    builder.Property(r => r.CompletedAt)
        .HasColumnType("timestamp with time zone")
        .HasColumnName("CompletedAt")
        .HasColumnOrder(26);

    builder.Property(r => r.CancelledAt)
        .HasColumnType("timestamp with time zone")
        .HasColumnName("CancelledAt")
        .HasColumnOrder(27);

    builder.Property(r => r.CancellationReason)
        .HasMaxLength(500)
        .HasColumnName("CancellationReason")
        .HasColumnOrder(28);

    // ==================== NOTLAR ====================
    builder.Property(r => r.Notes)
        .HasMaxLength(1000)
        .HasColumnName("Notes")
        .HasColumnOrder(29);

    builder.Property(r => r.AdminNotes)
        .HasMaxLength(1000)
        .HasColumnName("AdminNotes")
        .HasColumnOrder(30);

    // ==================== RELATIONSHIPS ====================

    builder.HasOne(r => r.Customer)
        .WithMany(c => c.Reservations)
        .HasForeignKey(r => r.CustomerId)
        .OnDelete(DeleteBehavior.Restrict)
        .IsRequired();

    builder.HasOne(r => r.Vehicle)
        .WithMany(v => v.Reservations)
        .HasForeignKey(r => r.VehicleId)
        .OnDelete(DeleteBehavior.Restrict)
        .IsRequired();

    builder.HasOne(r => r.PickupBranch)
        .WithMany(b => b.PickupReservations)
        .HasForeignKey(r => r.PickupBranchId)
        .OnDelete(DeleteBehavior.Restrict)
        .IsRequired();

    builder.HasOne(r => r.ReturnBranch)
        .WithMany(b => b.ReturnReservations)
        .HasForeignKey(r => r.ReturnBranchId)
        .OnDelete(DeleteBehavior.Restrict)
        .IsRequired(false);

    builder.HasMany(r => r.Extras)
        .WithOne()
        .HasForeignKey(re => re.RentalId)
        .OnDelete(DeleteBehavior.Cascade);

    // ==================== INDEXES ====================

    builder.HasIndex(r => r.ReservationNumber)
        .IsUnique()
        .HasDatabaseName("IX_Reservations_ReservationNumber")
        .HasFilter("\"IsDeleted\" = false");

    builder.HasIndex(r => r.CustomerId)
        .HasDatabaseName("IX_Reservations_CustomerId");

    builder.HasIndex(r => r.Status)
        .HasDatabaseName("IX_Reservations_Status");

    // Katman 1 job: PendingPayment süresi dolanlar
    builder.HasIndex(r => r.PaymentDeadline)
        .HasDatabaseName("IX_Reservations_PaymentDeadline")
        .HasFilter("\"IsDeleted\" = false AND \"Status\" = 'PendingPayment'");

    // Katman 2 job: Depozito sonrası 24 saat dolanlar
    builder.HasIndex(r => r.DepositLockDeadline)
        .HasDatabaseName("IX_Reservations_DepositLockDeadline")
        .HasFilter("\"IsDeleted\" = false AND \"DepositLockDeadline\" IS NOT NULL");

    // ⭐ PickupDeadline index KALDIRILDI

    // Araç müsaitlik kontrolü (ConflictingReservationsSpecification)
    builder.HasIndex(r => new { r.VehicleId, r.Status, r.PlannedStartDate, r.PlannedEndDate })
        .HasDatabaseName("IX_Reservations_Vehicle_Status_DateRange")
        .HasFilter("\"IsDeleted\" = false");

    // Müşteri rezervasyonları
    builder.HasIndex(r => new { r.CustomerId, r.Status })
        .HasDatabaseName("IX_Reservations_Customer_Status")
        .HasFilter("\"IsDeleted\" = false");

    // ==================== IGNORED PROPERTIES ====================
    builder.Ignore(r => r.ExtrasTotalPrice);
  }
}