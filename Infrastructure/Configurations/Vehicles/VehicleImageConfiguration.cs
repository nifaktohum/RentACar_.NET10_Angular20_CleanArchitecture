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
    
    builder.Property(vi => vi.ImageUrl)
        .IsRequired()
        .HasMaxLength(500)
        .HasColumnName("ImageUrl")
        .HasColumnOrder(1);

    builder.Property(vi => vi.DisplayOrder)
        .IsRequired()
        .HasDefaultValue(0)
        .HasColumnName("DisplayOrder")
        .HasColumnOrder(2);

    builder.Property(vi => vi.IsMain)
        .IsRequired()
        .HasDefaultValue(false)
        .HasColumnName("IsMain")
        .HasColumnOrder(3);

    builder.Property(vi => vi.Description)
        .HasMaxLength(200)
        .HasColumnName("Description")
        .HasColumnOrder(4);

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

    builder.HasIndex(vi => new { vi.VehicleId, vi.IsMain })
        .HasDatabaseName("IX_VehicleImages_VehicleId_IsMain")
        .HasFilter("\"IsMain\" = true");


    // ==================== QUERY FILTERS ====================
    builder.HasQueryFilter(vi => !vi.IsDeleted);
  }
}
