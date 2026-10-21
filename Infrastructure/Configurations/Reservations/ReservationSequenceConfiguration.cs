using Domain.Entities.Reservations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Configurations.Reservations;

public class ReservationSequenceConfiguration : IEntityTypeConfiguration<ReservationSequence>
{
  public void Configure(EntityTypeBuilder<ReservationSequence> builder)
  {
    // ==================== TABLE ====================
    builder.ToTable("ReservationSequences");

    // ==================== PRIMARY KEY ====================
    builder.HasKey(s => s.Year);

    builder.Property(s => s.Year)
        .IsRequired()
        .HasColumnName("Year")
        .HasColumnOrder(0)
        .ValueGeneratedNever(); 

    builder.Property(s => s.LastNumber)
        .IsRequired()
        .HasDefaultValue(0)
        .IsConcurrencyToken()
        .HasColumnName("LastNumber")
        .HasColumnOrder(1);
  }
}
