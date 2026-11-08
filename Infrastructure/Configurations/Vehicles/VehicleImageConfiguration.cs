using Domain.Entities.Vehicles;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Configurations.Vehicles;

public sealed class VehicleImageConfiguration : IEntityTypeConfiguration<VehicleImage>
{
  public void Configure(EntityTypeBuilder<VehicleImage> builder)
  {
    builder.ToTable("VehicleImages");

    // ==================== PRIMARY KEY ====================
    builder.HasKey(vi => vi.Id);

    // ==================== PROPERTIES ====================
    builder.Property(vi => vi.Id)
        .HasColumnName("Id")
        .HasColumnOrder(0);

    builder.Property(vi => vi.VehicleId)
        .IsRequired()
        .HasColumnName("VehicleId")
        .HasColumnOrder(1);

    builder.Property(vi => vi.ImageUrl)
        .IsRequired()
        .HasMaxLength(1000)              // ⚠️ 500 → 1000
        .HasColumnName("ImageUrl")
        .HasColumnOrder(2);

    builder.Property(vi => vi.DisplayOrder)
        .IsRequired()
        .HasDefaultValue(0)
        .HasColumnName("DisplayOrder")
        .HasColumnOrder(3);

    builder.Property(vi => vi.IsMain)
        .IsRequired()
        .HasDefaultValue(false)
        .HasColumnName("IsMain")
        .HasColumnOrder(4);

    builder.Property(vi => vi.Description)
        .HasMaxLength(200)
        .HasColumnName("Description")
        .HasColumnOrder(5);

    // ==================== AUDIT / SOFT DELETE ====================
    // (BaseEntityConfiguration varsa bu blok SİLİNEBİLİR)
    builder.Property(vi => vi.IsActive)
        .IsRequired()
        .HasDefaultValue(true)
        .HasColumnName("IsActive")
        .HasColumnOrder(6);

    builder.Property(vi => vi.IsDeleted)
        .IsRequired()
        .HasDefaultValue(false)
        .HasColumnName("IsDeleted")
        .HasColumnOrder(7);

    builder.Property(vi => vi.CreatedAt)
        .IsRequired()
        .HasColumnName("CreatedAt")
        .HasColumnOrder(8);

    builder.Property(vi => vi.UpdatedAt)
        .HasColumnName("UpdatedAt")
        .HasColumnOrder(9);

    builder.Property(vi => vi.CreatedBy)
        .IsRequired()
        .HasColumnName("CreatedBy")
        .HasColumnOrder(10);

    builder.Property(vi => vi.UpdatedBy)
        .HasColumnName("UpdatedBy")
        .HasColumnOrder(11);

    // ==================== RELATIONSHIPS ====================
    // VehicleImage → Vehicle (Many-to-One)
    builder.HasOne(vi => vi.Vehicle)
        .WithMany(v => v.Images)
        .HasForeignKey(vi => vi.VehicleId)
        .OnDelete(DeleteBehavior.Cascade)
        .IsRequired();

    // ==================== INDEXES ====================
    builder.HasIndex(vi => vi.VehicleId)
        .HasDatabaseName("IX_VehicleImages_VehicleId");

    builder.HasIndex(vi => new { vi.VehicleId, vi.DisplayOrder })
        .HasDatabaseName("IX_VehicleImages_VehicleId_DisplayOrder");

    // ⚠️ Eski index: filtered non-unique
    // builder.HasIndex(vi => new { vi.VehicleId, vi.IsMain })
    //     .HasDatabaseName("IX_VehicleImages_VehicleId_IsMain")
    //     .HasFilter("\"IsMain\" = true");

    // ✅ Yeni: "Her araçta en fazla 1 ana görsel" garantisi (DB seviyesinde)
    builder.HasIndex(vi => vi.VehicleId)
        .IsUnique()
        .HasDatabaseName("UX_VehicleImages_VehicleId_IsMain")
        .HasFilter("\"IsMain\" = true AND \"IsDeleted\" = false");

    // ==================== QUERY FILTERS ====================
    builder.HasQueryFilter(vi => !vi.IsDeleted);
  }
}