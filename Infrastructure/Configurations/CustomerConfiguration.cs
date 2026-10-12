using Domain.Entities.Customers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Configurations;

public sealed class CustomerConfiguration : BaseEntityConfiguration<Customer>
{
  public override void Configure(EntityTypeBuilder<Customer> builder)
  {
    // ==================== TABLO ====================
    base.Configure(builder);   // ⭐ Audit + query filter

    builder.ToTable("Customers");

    // ==================== KİMLİK ====================
    builder.Property(c => c.Type)
        .IsRequired()
        .HasConversion<int>()
        .HasComment("1 = Individual, 2 = Corporate");

    // ==================== BİREYSEL ====================
    builder.Property(c => c.FirstName).HasMaxLength(100);
    builder.Property(c => c.LastName).HasMaxLength(100);
    builder.Property(c => c.IdentityNumber).HasMaxLength(11);

    builder.Property(c => c.BirthDate)
        .HasColumnType("date");   // ⭐

    builder.Property(c => c.Gender).HasMaxLength(10);

    // ==================== KURUMSAL ====================
    builder.Property(c => c.CompanyName).HasMaxLength(200);
    builder.Property(c => c.TaxNumber).HasMaxLength(20);
    builder.Property(c => c.TaxOffice).HasMaxLength(100);

    // ==================== İLETİŞİM ====================
    builder.Property(c => c.Email).IsRequired().HasMaxLength(200);
    builder.Property(c => c.PhoneNumber).IsRequired().HasMaxLength(20);
    builder.Property(c => c.Address).IsRequired().HasMaxLength(500);
    builder.Property(c => c.City).IsRequired().HasMaxLength(100);
    builder.Property(c => c.Country).IsRequired().HasMaxLength(100);
    builder.Property(c => c.PostalCode).HasMaxLength(20);

    // ==================== SÜRÜCÜ BELGESİ ====================
    builder.Property(c => c.DriverLicenseNumber).HasMaxLength(50);

    builder.Property(c => c.DriverLicenseIssueDate)
        .HasColumnType("date");   // ⭐

    builder.Property(c => c.DriverLicenseClass).HasMaxLength(10);

    // ==================== FİNANSAL ====================
    builder.Property(c => c.FindeksScore)
        .HasComment("Findeks skoru (0-1900)");

    // ==================== AUTH ====================
    builder.Property(c => c.PasswordHash).IsRequired().HasMaxLength(500);
    builder.Property(c => c.RefreshToken).HasMaxLength(500);

    builder.Property(c => c.RefreshTokenExpiresAt)
        .HasColumnType("timestamp with time zone");   // ⭐

    builder.Property(c => c.PasswordResetCode).HasMaxLength(10);

    builder.Property(c => c.PasswordResetCodeExpiresAt)
        .HasColumnType("timestamp with time zone");   // ⭐

    builder.Property(c => c.EmailVerificationCode).HasMaxLength(10);

    builder.Property(c => c.EmailVerificationCodeExpiresAt)
        .HasColumnType("timestamp with time zone");   // ⭐

    builder.Property(c => c.PhoneVerificationCode).HasMaxLength(10);

    builder.Property(c => c.PhoneVerificationCodeExpiresAt)
        .HasColumnType("timestamp with time zone");   // ⭐

    builder.Property(c => c.SmsResetCode).HasMaxLength(10);

    builder.Property(c => c.SmsResetCodeExpiresAt)
        .HasColumnType("timestamp with time zone"); // ⭐

    // ==================== DURUM ====================
    builder.Property(c => c.IsBlacklisted).IsRequired().HasDefaultValue(false);
    builder.Property(c => c.BlacklistReason).HasMaxLength(500);
    builder.Property(c => c.IsEmailVerified).IsRequired().HasDefaultValue(false);
    builder.Property(c => c.IsPhoneVerified).IsRequired().HasDefaultValue(false);
    builder.Property(c => c.SecurityStamp).HasMaxLength(100);

    // ==================== INDEX'LER (PostgreSQL) ====================
    builder.HasIndex(c => c.Email)
        .IsUnique()
        .HasFilter("\"IsDeleted\" = false")
        .HasDatabaseName("IX_Customers_Email");

    builder.HasIndex(c => c.PhoneNumber)
        .IsUnique()
        .HasFilter("\"IsDeleted\" = false")
        .HasDatabaseName("IX_Customers_PhoneNumber");

    builder.HasIndex(c => c.IdentityNumber)
        .IsUnique()
        .HasFilter("\"IdentityNumber\" IS NOT NULL AND \"IsDeleted\" = false")
        .HasDatabaseName("IX_Customers_IdentityNumber");

    builder.HasIndex(c => c.TaxNumber)
        .IsUnique()
        .HasFilter("\"TaxNumber\" IS NOT NULL AND \"IsDeleted\" = false")
        .HasDatabaseName("IX_Customers_TaxNumber");

    builder.HasIndex(c => c.Type).HasDatabaseName("IX_Customers_Type");
    builder.HasIndex(c => c.IsBlacklisted).HasDatabaseName("IX_Customers_IsBlacklisted");
    builder.HasIndex(c => c.IsEmailVerified).HasDatabaseName("IX_Customers_IsEmailVerified");
    
  }

}
