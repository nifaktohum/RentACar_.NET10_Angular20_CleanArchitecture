using System.Linq.Expressions;
using Application.Specifications;
using Domain.Entities.Customers;
using LinqKit;
using Microsoft.EntityFrameworkCore;

namespace Application.Common.Helpers.Filters;

public static class CustomerFilterExtensions
{
  public static Expression<Func<Customer, bool>> BuildFilter(CustomerSpecParams? specParams, bool includeAll = false)
  {
    // ⭐ Başlangıç koşulu
    // includeAll = true  → Admin: sadece silinmemişler (aktif/pasif fark etmez)
    // includeAll = false → Personel: aktif ve silinmemişler
    Expression<Func<Customer, bool>> filter = includeAll
        ? c => !c.IsDeleted
        : c => c.IsActive && !c.IsDeleted;

    if (specParams == null) return filter;

    // ==================== 🔍 GENEL ARAMA ====================

    if (!string.IsNullOrWhiteSpace(specParams.SearchTerm))
    {
      var rawSearch = specParams.SearchTerm.Trim();
      var lowerSearch = rawSearch.ToLowerInvariant();

      // Wildcard escape
      var escaped = lowerSearch
          .Replace("\\", "\\\\")
          .Replace("%", "\\%")
          .Replace("_", "\\_");

      var searchPattern = $"%{escaped}%";

      filter = filter.And(c =>
          (c.Email != null && EF.Functions.Like(c.Email.ToLower(), searchPattern))
          || (c.FirstName != null && EF.Functions.Like(c.FirstName.ToLower(), searchPattern))
          || (c.LastName != null && EF.Functions.Like(c.LastName.ToLower(), searchPattern))
          || (c.CompanyName != null && EF.Functions.Like(c.CompanyName.ToLower(), searchPattern))
          || (c.PhoneNumber != null && c.PhoneNumber.Contains(rawSearch))
          || (c.IdentityNumber != null && c.IdentityNumber.Contains(rawSearch))
          || (c.TaxNumber != null && c.TaxNumber.Contains(rawSearch))
      );
    }

    // ==================== 🎯 MÜŞTERİ TİPİ ====================
    if (specParams.Type.HasValue)
      filter = filter.And(c => c.Type == specParams.Type.Value);

    // ==================== 🚫 KARA LİSTE ====================
    if (specParams.IsBlacklisted.HasValue)
      filter = filter.And(c => c.IsBlacklisted == specParams.IsBlacklisted.Value);

    // ==================== 📧 EMAIL DOĞRULAMA ====================
    if (specParams.IsEmailVerified.HasValue)
      filter = filter.And(c => c.IsEmailVerified == specParams.IsEmailVerified.Value);

    // ==================== 📱 TELEFON DOĞRULAMA ====================
    if (specParams.IsPhoneVerified.HasValue)
      filter = filter.And(c => c.IsPhoneVerified == specParams.IsPhoneVerified.Value);

    // ==================== ✅ AKTİFLİK ====================
    if (specParams.IsActive.HasValue)
      filter = filter.And(c => c.IsActive == specParams.IsActive.Value);

    // ==================== 🏙️ ŞEHİR ====================
    if (!string.IsNullOrWhiteSpace(specParams.City))
    {
      var city = specParams.City.ToLowerInvariant();
      filter = filter.And(c =>
          c.City != null && c.City.ToLower().Contains(city));
    }

    // ==================== 🌍 ÜLKE ====================
    if (!string.IsNullOrWhiteSpace(specParams.Country))
    {
      var country = specParams.Country.ToLowerInvariant();
      filter = filter.And(c =>
          c.Country != null && c.Country.ToLower().Contains(country));
    }

    // ==================== 📊 FİNDEKS SKORU ARALIĞI ====================
    if (specParams.MinFindeks.HasValue)
      filter = filter.And(c =>
          c.FindeksScore.HasValue && c.FindeksScore.Value >= specParams.MinFindeks.Value);

    if (specParams.MaxFindeks.HasValue)
      filter = filter.And(c =>
          c.FindeksScore.HasValue && c.FindeksScore.Value <= specParams.MaxFindeks.Value);

    // ==================== 📅 KAYIT TARİHİ ARALIĞI ====================
    if (specParams.CreatedAfter.HasValue)
      filter = filter.And(c => c.CreatedAt >= specParams.CreatedAfter.Value);

    if (specParams.CreatedBefore.HasValue)
      filter = filter.And(c => c.CreatedAt <= specParams.CreatedBefore.Value);

    return filter;
  }
}