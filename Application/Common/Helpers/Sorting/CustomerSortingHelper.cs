// using Domain.Entities.Customers;
// using Domain.Specifications;

// namespace Application.Common.Helpers.Sorting;

// public static class CustomerSortingHelper
// {
//   public static void ApplyCustomerSorting(this BaseSpecification<Customer> spec, string? sort)
//   {
//     switch (sort)
//     {
//       // ==================== 📧 EMAIL ====================
//       case "emailasc":
//         spec.ApplyOrderBy(c => c.Email ?? "");
//         spec.ApplyThenBy(c => c.Id);              // ⭐ stabil
//         break;
//       case "emaildesc":
//         spec.ApplyOrderByDescending(c => c.Email ?? "");
//         spec.ApplyThenBy(c => c.Id);
//         break;

//       // ==================== 👤 AD ====================
//       case "firstnameasc":
//       case "nameasc":
//         spec.ApplyOrderBy(c => c.FirstName ?? "");
//         spec.ApplyThenBy(c => c.LastName ?? "");  // ⭐ soyad tie-breaker
//         break;
//       case "firstnamedesc":
//       case "namedesc":
//         spec.ApplyOrderByDescending(c => c.FirstName ?? "");
//         spec.ApplyThenBy(c => c.LastName ?? "");
//         break;

//       // ==================== 👤 SOYAD ====================
//       case "lastnameasc":
//         spec.ApplyOrderBy(c => c.LastName ?? "");
//         spec.ApplyThenBy(c => c.FirstName ?? "");
//         break;
//       case "lastnamedesc":
//         spec.ApplyOrderByDescending(c => c.LastName ?? "");
//         spec.ApplyThenBy(c => c.FirstName ?? "");
//         break;

//       // ==================== 👤 TAM AD (Ad + Soyad) ====================
//       case "fullnameasc":
//         // Bireysel: FirstName + LastName
//         // Kurumsal: CompanyName
//         spec.ApplyOrderBy(c =>
//           c.Type == CustomerType.Individual
//             ? ((c.FirstName ?? "") + " " + (c.LastName ?? ""))
//             : (c.CompanyName ?? "")
//         );
//         spec.ApplyThenBy(c => c.Id);
//         break;

//       case "fullnamedesc":
//         spec.ApplyOrderByDescending(c =>
//           c.Type == CustomerType.Individual
//             ? ((c.FirstName ?? "") + " " + (c.LastName ?? ""))
//             : (c.CompanyName ?? "")
//         );
//         spec.ApplyThenBy(c => c.Id);
//         break;

//       // ==================== 🏢 ŞİRKET ====================
//       case "companynameasc":
//       case "companyasc":
//         spec.ApplyOrderBy(c => c.CompanyName ?? "");
//         spec.ApplyThenBy(c => c.Id);
//         break;
//       case "companynamedesc":
//       case "companydesc":
//         spec.ApplyOrderByDescending(c => c.CompanyName ?? "");
//         spec.ApplyThenBy(c => c.Id);
//         break;

//       // Bireysel müşterileri ada göre sırala
//       case "individualnameasc":
//         spec.ApplyOrderBy(c => c.FirstName ?? "");
//         spec.ApplyThenBy(c => c.LastName ?? "");
//         break;
//       case "individualnamedesc":
//         spec.ApplyOrderByDescending(c => c.FirstName ?? "");
//         spec.ApplyThenBy(c => c.LastName ?? "");
//         break;

//       // ==================== 📊 FİNDEKS ====================
//       case "findeksasc":
//       case "findeksscoreasc":
//         spec.ApplyOrderBy(c => c.FindeksScore ?? 0);
//         spec.ApplyThenBy(c => c.Id);
//         break;
//       case "findeksdesc":
//       case "findeksscoredesc":
//         spec.ApplyOrderByDescending(c => c.FindeksScore ?? 0);
//         spec.ApplyThenBy(c => c.Id);
//         break;

//       // ==================== 🏙️ ŞEHİR ====================
//       case "cityasc":
//         spec.ApplyOrderBy(c => c.City ?? "");
//         spec.ApplyThenBy(c => c.Id);
//         break;
//       case "citydesc":
//         spec.ApplyOrderByDescending(c => c.City ?? "");
//         spec.ApplyThenBy(c => c.Id);
//         break;

//       // ==================== 🎯 MÜŞTERİ TİPİ ====================
//       case "typeasc":
//         spec.ApplyOrderBy(c => c.Type);
//         spec.ApplyThenBy(c => c.Id);
//         break;
//       case "typedesc":
//         spec.ApplyOrderByDescending(c => c.Type);
//         spec.ApplyThenBy(c => c.Id);
//         break;

//       // ==================== 🚫 KARA LİSTE ====================
//       case "blacklistasc":
//         spec.ApplyOrderBy(c => c.IsBlacklisted);
//         spec.ApplyThenByDescending(c => c.CreatedAt);   // ⭐ kara listedekiler önce, sonra en yeni
//         break;
//       case "blacklistdesc":
//         spec.ApplyOrderByDescending(c => c.IsBlacklisted);
//         spec.ApplyThenByDescending(c => c.CreatedAt);
//         break;

//       // ==================== 📧 EMAIL DOĞRULAMA ====================
//       case "emailverifiedasc":
//         spec.ApplyOrderBy(c => c.IsEmailVerified);
//         spec.ApplyThenByDescending(c => c.CreatedAt);
//         break;
//       case "emailverifieddesc":
//         spec.ApplyOrderByDescending(c => c.IsEmailVerified);
//         spec.ApplyThenByDescending(c => c.CreatedAt);
//         break;

//       // ==================== ✅ AKTİFLİK ====================
//       case "activeasc":
//         spec.ApplyOrderBy(c => c.IsActive);
//         spec.ApplyThenByDescending(c => c.CreatedAt);
//         break;
//       case "activedesc":
//         spec.ApplyOrderByDescending(c => c.IsActive);
//         spec.ApplyThenByDescending(c => c.CreatedAt);
//         break;

//       // ==================== 📅 KAYIT TARİHİ ====================
//       case "createdasc":
//       case "oldest":                       // ⭐ alias
//         spec.ApplyOrderBy(c => c.CreatedAt);
//         spec.ApplyThenBy(c => c.Id);
//         break;
//       case "createddesc":
//       case "latest":                       // ⭐ alias
//         spec.ApplyOrderByDescending(c => c.CreatedAt);
//         spec.ApplyThenBy(c => c.Id);
//         break;

//       // ==================== 🔄 GÜNCELLENME TARİHİ ====================
//       case "updatedasc":
//         spec.ApplyOrderBy(c => c.UpdatedAt ?? c.CreatedAt);
//         spec.ApplyThenBy(c => c.Id);
//         break;
//       case "updateddesc":
//         spec.ApplyOrderByDescending(c => c.UpdatedAt ?? c.CreatedAt);
//         spec.ApplyThenBy(c => c.Id);
//         break;

//       // ==================== 🎯 DEFAULT ====================
//       default:
//         // Varsayılan: En yeni kayıtlar önce, aynı tarihte olanlar email'e göre
//         spec.ApplyOrderByDescending(c => c.CreatedAt);
//         spec.ApplyThenBy(c => c.Email ?? "");     // ⭐ null-safe
//         spec.ApplyThenBy(c => c.Id);              // ⭐ final tie-breaker
//         break;
//     }
//   }
// }

using Domain.Entities.Customers;
using Domain.Specifications;

namespace Application.Common.Helpers.Sorting;

public static class CustomerSortingHelper
{
  public static void ApplyCustomerSorting(this BaseSpecification<Customer> spec, string? sort)
  {
    var normalizedSort = string.IsNullOrWhiteSpace(sort)
        ? null
        : sort.Trim().ToLowerInvariant();

    switch (normalizedSort)
    {
      // ==================== 📧 EMAIL ====================
      case "emailasc":
        spec.ApplyOrderBy(c => c.Email ?? "");
        spec.ApplyThenBy(c => c.Id);
        break;
      case "emaildesc":
        spec.ApplyOrderByDescending(c => c.Email ?? "");
        spec.ApplyThenBy(c => c.Id);
        break;

      // ==================== 👤 AD ====================
      case "firstnameasc":
      case "nameasc":
        spec.ApplyOrderBy(c => c.FirstName ?? "");
        spec.ApplyThenBy(c => c.LastName ?? "");
        spec.ApplyThenBy(c => c.Id);
        break;
      case "firstnamedesc":
      case "namedesc":
        spec.ApplyOrderByDescending(c => c.FirstName ?? "");
        spec.ApplyThenBy(c => c.LastName ?? "");
        spec.ApplyThenBy(c => c.Id);
        break;

      // ==================== 👤 SOYAD ====================
      case "lastnameasc":
        spec.ApplyOrderBy(c => c.LastName ?? "");
        spec.ApplyThenBy(c => c.FirstName ?? "");
        spec.ApplyThenBy(c => c.Id);
        break;
      case "lastnamedesc":
        spec.ApplyOrderByDescending(c => c.LastName ?? "");
        spec.ApplyThenBy(c => c.FirstName ?? "");
        spec.ApplyThenBy(c => c.Id);
        break;

      // ==================== 👤 TAM AD ====================
      // NOT: EF Core'da ternary + string concat çeviri riski taşır.
      // Mümkünse entity'de DisplayName computed column kullan.
      case "fullnameasc":
        spec.ApplyOrderBy(c => c.FirstName ?? c.CompanyName ?? "");
        spec.ApplyThenBy(c => c.LastName ?? "");
        spec.ApplyThenBy(c => c.Id);
        break;
      case "fullnamedesc":
        spec.ApplyOrderByDescending(c => c.FirstName ?? c.CompanyName ?? "");
        spec.ApplyThenBy(c => c.LastName ?? "");
        spec.ApplyThenBy(c => c.Id);
        break;

      // ==================== 🏢 ŞİRKET ====================
      case "companynameasc":
      case "companyasc":
        spec.ApplyOrderBy(c => c.CompanyName ?? "");
        spec.ApplyThenBy(c => c.Id);
        break;
      case "companynamedesc":
      case "companydesc":
        spec.ApplyOrderByDescending(c => c.CompanyName ?? "");
        spec.ApplyThenBy(c => c.Id);
        break;

      // ==================== 📊 FİNDEKS ====================
      case "findeksasc":
      case "findeksscoreasc":
        spec.ApplyOrderBy(c => c.FindeksScore ?? 0);
        spec.ApplyThenBy(c => c.Id);
        break;
      case "findeksdesc":
      case "findeksscoredesc":
        spec.ApplyOrderByDescending(c => c.FindeksScore ?? 0);
        spec.ApplyThenBy(c => c.Id);
        break;

      // ==================== 🏙️ ŞEHİR ====================
      case "cityasc":
        spec.ApplyOrderBy(c => c.City ?? "");
        spec.ApplyThenBy(c => c.Id);
        break;
      case "citydesc":
        spec.ApplyOrderByDescending(c => c.City ?? "");
        spec.ApplyThenBy(c => c.Id);
        break;

      // ==================== 🎯 MÜŞTERİ TİPİ ====================
      // NOT: Enum değerine göre sıralar (alfabetik değil)
      case "typeasc":
        spec.ApplyOrderBy(c => c.Type);
        spec.ApplyThenBy(c => c.Id);
        break;
      case "typedesc":
        spec.ApplyOrderByDescending(c => c.Type);
        spec.ApplyThenBy(c => c.Id);
        break;

      // ==================== 🚫 KARA LİSTE ====================
      case "blacklistasc":
        spec.ApplyOrderBy(c => c.IsBlacklisted);
        spec.ApplyThenByDescending(c => c.CreatedAt);
        spec.ApplyThenBy(c => c.Id);
        break;
      case "blacklistdesc":
        spec.ApplyOrderByDescending(c => c.IsBlacklisted);
        spec.ApplyThenByDescending(c => c.CreatedAt);
        spec.ApplyThenBy(c => c.Id);
        break;

      // ==================== 📧 EMAIL DOĞRULAMA ====================
      case "emailverifiedasc":
        spec.ApplyOrderBy(c => c.IsEmailVerified);
        spec.ApplyThenByDescending(c => c.CreatedAt);
        spec.ApplyThenBy(c => c.Id);
        break;
      case "emailverifieddesc":
        spec.ApplyOrderByDescending(c => c.IsEmailVerified);
        spec.ApplyThenByDescending(c => c.CreatedAt);
        spec.ApplyThenBy(c => c.Id);
        break;

      // ==================== ✅ AKTİFLİK ====================
      case "activeasc":
        spec.ApplyOrderBy(c => c.IsActive);
        spec.ApplyThenByDescending(c => c.CreatedAt);
        spec.ApplyThenBy(c => c.Id);
        break;
      case "activedesc":
        spec.ApplyOrderByDescending(c => c.IsActive);
        spec.ApplyThenByDescending(c => c.CreatedAt);
        spec.ApplyThenBy(c => c.Id);
        break;

      // ==================== 📅 KAYIT TARİHİ ====================
      case "createdasc":
      case "oldest":
        spec.ApplyOrderBy(c => c.CreatedAt);
        spec.ApplyThenBy(c => c.Id);
        break;
      case "createddesc":
      case "latest":
        spec.ApplyOrderByDescending(c => c.CreatedAt);
        spec.ApplyThenBy(c => c.Id);
        break;

      // ==================== 🔄 GÜNCELLENME TARİHİ ====================
      case "updatedasc":
        spec.ApplyOrderBy(c => c.UpdatedAt ?? c.CreatedAt);
        spec.ApplyThenBy(c => c.Id);
        break;
      case "updateddesc":
        spec.ApplyOrderByDescending(c => c.UpdatedAt ?? c.CreatedAt);
        spec.ApplyThenBy(c => c.Id);
        break;

      // ==================== 🎯 DEFAULT ====================
      default:
        spec.ApplyOrderByDescending(c => c.CreatedAt);
        spec.ApplyThenBy(c => c.Id);
        break;
    }
  }
}

