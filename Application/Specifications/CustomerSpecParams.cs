using System.Text.Json.Serialization;
using Application.Common.Paging;
using Domain.Entities.Customers;

namespace Application.Specifications;

public record class CustomerSpecParams : BasePagedRequest
{
  // ============ 🔀 SIRALAMA ============
  private string? _sort;
  public string? Sort
  {
    get => _sort;
    set => _sort = string.IsNullOrWhiteSpace(value) ? null : value.Trim().ToLowerInvariant();
  }

  public bool IsDescending { get; set; } = true;

  // ============ 🔍 GENEL ARAMA ============
  private string? _searchTerm;
  public string? SearchTerm
  {
    get => _searchTerm;
    set => _searchTerm = string.IsNullOrWhiteSpace(value) ? null : value.Trim();
  }

  // ============ 🎯 MÜŞTERİ TİPİ ============
  // Kullanım: ?type=Individual veya ?type=1
  [JsonIgnore]
  public CustomerType? Type { get; private set; }

  public string? TypeString
  {
    get => Type?.ToString();
    set => Type = ParseCustomerType(value);
  }

  private static CustomerType? ParseCustomerType(string? value)
  {
    if (string.IsNullOrWhiteSpace(value)) return null;
    var trimmed = value.Trim();

    if (Enum.TryParse<CustomerType>(trimmed, ignoreCase: true, out var parsed))
      return parsed;

    if (int.TryParse(trimmed, out var intVal) && Enum.IsDefined(typeof(CustomerType), intVal))
      return (CustomerType)intVal;

    return null;
  }

  // ============ 🚫 KARA LİSTE ============
  public bool? IsBlacklisted { get; set; }

  // ============ 📧 EMAIL DOĞRULAMA ============
  public bool? IsEmailVerified { get; set; }

  // ============ 📱 TELEFON DOĞRULAMA ============
  public bool? IsPhoneVerified { get; set; }

  // ============ ✅ AKTİFLİK ============
  public bool? IsActive { get; set; }

  // ============ 📊 FİNDEKS SKORU ARALIĞI ============
  private int? _minFindeks;
  public int? MinFindeks
  {
    get => _minFindeks;
    set => _minFindeks = value switch
    {
      null => null,
      < 0 => 0,
      > 1900 => 1900,
      _ => value
    };
  }

  private int? _maxFindeks;
  public int? MaxFindeks
  {
    get => _maxFindeks;
    set => _maxFindeks = value switch
    {
      null => null,
      < 0 => 0,
      > 1900 => 1900,
      _ => value
    };
  }

  // ============ 🏙️ ŞEHİR ============
  private string? _city;
  public string? City
  {
    get => _city;
    set => _city = string.IsNullOrWhiteSpace(value) ? null : value.Trim();
  }

  // ============ 🌍 ÜLKE ============
  private string? _country;
  public string? Country
  {
    get => _country;
    set => _country = string.IsNullOrWhiteSpace(value) ? null : value.Trim();
  }

  // ============ 📅 KAYIT TARİHİ ARALIĞI ============
  private DateTime? _createdAfter;
  public DateTime? CreatedAfter
  {
    get => _createdAfter;
    set => _createdAfter = value.HasValue
        ? DateTime.SpecifyKind(value.Value, DateTimeKind.Utc)
        : null;
  }

  private DateTime? _createdBefore;
  public DateTime? CreatedBefore
  {
    get => _createdBefore;
    set => _createdBefore = value.HasValue
        ? DateTime.SpecifyKind(value.Value, DateTimeKind.Utc)
        : null;
  }
}