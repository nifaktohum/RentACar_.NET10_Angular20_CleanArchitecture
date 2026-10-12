export interface CustomerSpecParams {
  // Sayfalama
  pageNumber?: number;
  pageSize?: number;

  // Sıralama ("createdAtDesc" gibi birleşik)
  sort?: string;

  // Genel arama
  searchTerm?: string;

  // Müşteri tipi
  typeString?: string;        // Backend TypeString setter'ı hem "Individual" hem "1" kabul ediyor

  // Boolean filtreler
  isBlacklisted?: boolean;
  isEmailVerified?: boolean;
  isPhoneVerified?: boolean;
  isActive?: boolean;

  // Findeks aralığı
  minFindeks?: number;
  maxFindeks?: number;

  // Konum
  city?: string;
  country?: string;

  // Tarih aralığı (ISO string)
  createdAfter?: string;
  createdBefore?: string;
}