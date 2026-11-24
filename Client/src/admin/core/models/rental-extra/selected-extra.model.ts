import { ExtraSummary } from "../extra/extraSummary";

export interface SelectedExtra {
  extra: ExtraSummary; // Seçilen ekstra hizmet/ürün özet bilgisi
  quantity: number;      // Seçilen ekstra adedi
  totalPrice: number;    // Gün sayısına, fiyat tipine ve adede göre hesaplanan toplam tutar
}