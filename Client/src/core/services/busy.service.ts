import { Injectable, signal } from '@angular/core';
import { environment } from '../../environments/environment';

@Injectable({
  providedIn: 'root'
})
export class BusyService {
  // 🔒 Sadece bu servisin içinden değiştirilebilen private signal
  private readonly _loading = signal<boolean>(false);

  // Aktif HTTP isteklerinin sayısını tutan sayaç
  private _busyCount = 0;


  // 🔓 Component'lerin HTML'de direkt "busyService.loading()" diye okuyabileceği salt okunur signal
  readonly loading = this._loading.asReadonly();

  // Bir işlem başladığında çağrılır
  busy() {
    this._busyCount++;
    this._loading.set(true); // Signal güncelleniyor kanka
  }

  // Bir işlem tamamlandığında çağrılır
  idle() {
    this._busyCount--;

    if (this._busyCount <= 0) {
      this._busyCount = 0;
      this._loading.set(false); // Signal sıfırlanıyor kanka
    }
  }

  // Debug için (opsiyonel)
  // 🎯 Dönüş tipini nesne şablonu VEYA null olarak imzalıyoruz
  getDebugInfo(): { isLoading: boolean; busyCount: number } | null {
    if (!environment.production) {
      return {
        isLoading: this._loading(),
        busyCount: this._busyCount
      };
    }
    return null; // Production'da gizli kalsın kanka
  }
}