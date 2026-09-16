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

    // ==================== RELATIONSHIPS ====================
    builder.HasMany(vt => vt.VehicleModels)  
         .WithOne(vm => vm.VehicleType)       
         .HasForeignKey(vm => vm.VehicleTypeId)  
         .OnDelete(DeleteBehavior.Restrict);

    builder.HasIndex(x => x.Name)
        .IsUnique()
        .HasDatabaseName("IX_VehicleTypes_Name")
        .HasFilter("\"IsDeleted\" = false");

    builder.HasIndex(vt => vt.DisplayOrder)
     .HasDatabaseName("IX_VehicleTypes_DisplayOrder");

    // ==================== QUERY FILTERS ====================
    builder.HasQueryFilter(vt => !vt.IsDeleted);
  }
}
