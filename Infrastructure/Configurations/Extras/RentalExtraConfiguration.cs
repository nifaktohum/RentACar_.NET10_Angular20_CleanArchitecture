using Domain.Entities.Extras;
using Domain.Entities.Extras.Enum;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Configurations.Extras;

public sealed class RentalExtraConfiguration : IEntityTypeConfiguration<RentalExtra>
{
  public void Configure(EntityTypeBuilder<RentalExtra> builder)
  {
    // ==================== TABLE ====================
    builder.ToTable("RentalExtras");

    // ==================== PRIMARY KEY ====================
    builder.HasKey(re => re.Id);

    builder.Property(re => re.Id)
        .HasColumnName("Id")
        .HasColumnOrder(0);

    // ==================== FK ====================
    builder.Property(re => re.RentalId)
        .IsRequired()
        .HasColumnName("RentalId")
        .HasColumnOrder(1);

    builder.Property(re => re.ExtraId)
        .IsRequired()
        .HasColumnName("ExtraId")
        .HasColumnOrder(2);

    // ==================== FİYATLANDIRMA ====================
    builder.Property(re => re.UnitPrice)
        .IsRequired()
        .HasPrecision(18, 2)
        .HasColumnName("UnitPrice")
        .HasColumnOrder(3);

    builder.Property(re => re.Quantity)
        .IsRequired()
        .HasDefaultValue(1)
        .HasColumnName("Quantity")
        .HasColumnOrder(4);

    builder.Property(re => re.RentalDayCount)
        .IsRequired()
        .HasDefaultValue(1)
        .HasColumnName("RentalDayCount")
        .HasColumnOrder(5);

    builder.Property(re => re.PriceType)
        .IsRequired()
        .HasDefaultValue(PriceType.Daily)
        .HasColumnName("PriceType")
        .HasColumnOrder(6);

    builder.Property(re => re.TotalPrice)
        .IsRequired()
        .HasPrecision(18, 2)
        .HasColumnName("TotalPrice")
        .HasColumnOrder(7);

    // ==================== RELATIONSHIPS ====================
    builder.HasOne(re => re.Extra)
        .WithMany(e => e.RentalExtras)
        .HasForeignKey(re => re.ExtraId)
        .OnDelete(DeleteBehavior.Restrict)
        .IsRequired();

    // ==================== INDEXES ====================
    builder.HasIndex(re => re.RentalId)
        .HasDatabaseName("IX_RentalExtras_RentalId");

    builder.HasIndex(re => re.ExtraId)
        .HasDatabaseName("IX_RentalExtras_ExtraId");

    builder.HasIndex("RentalId", "ExtraId")
        .IsUnique()
        .HasDatabaseName("IX_RentalExtras_Rental_Extra")  // ✅ bu olmalı
        .HasFilter("\"IsDeleted\" = false");
  }
}