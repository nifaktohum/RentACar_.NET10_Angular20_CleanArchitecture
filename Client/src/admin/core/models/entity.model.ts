export interface EntityModel {
  id: string;
  createdAt: string;       // DateTimeOffset -> string olarak serileşir
  createdBy: string;       // Guid -> string
  createdFullname: string; // 👈 Artık ID değil, direkt personelin ismi geliyor!
  updatedAt?: string;      // C#'taki UpdatedAt? alanı
  updatedBy?: string;      // C#'taki UpdatedBy? alanı
  updateFullname?: string; // 👈 Güncelleyen personelin ismi!
  isActive: boolean;       // C#'taki IsActive şalteri
  isDeleted: boolean;      // Soft-delete durum takibi için
}
