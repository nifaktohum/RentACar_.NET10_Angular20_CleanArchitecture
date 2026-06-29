using Domain.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Configurations;

public abstract class BaseEntityConfiguration<T> : IEntityTypeConfiguration<T> where T : BaseEntity
{
  public virtual void Configure(EntityTypeBuilder<T> builder)
  {
    builder.HasKey(x => x.Id);
    builder.Property(x => x.CreatedAt).IsRequired();
    builder.Property(x => x.CreatedBy).IsRequired();
    builder.Property(x => x.IsActive).IsRequired().HasDefaultValue(true);
    builder.Property(x => x.IsDeleted).IsRequired().HasDefaultValue(false);
    builder.HasQueryFilter(x => !x.IsDeleted);
  }
}
