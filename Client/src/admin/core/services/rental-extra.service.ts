import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { environment } from '../../../environments/environment';
import { Observable } from 'rxjs';
import { Result } from '../../../core/models/result.model';
import { RentalExtra } from '../models/rental-extra/rental-extra.model';
import { CreateRentalExtraRequest } from '../models/rental-extra/create-rental-extra.model';

@Injectable({
  providedIn: 'root',
})
export class RentalExtraService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.apiUrl}/rentals`;

  
  // * Kiralama ID'sine göre tüm ekstraları getirir
  getByRentalId(rentalId: string): Observable<Result<RentalExtra[]>> {
    return this.http.get<Result<RentalExtra[]>>(`${this.baseUrl}/extras/${rentalId}`);
  }

  
  // * Kiralama'ya extra ekler
  addExtra(rentalId: string, data: CreateRentalExtraRequest): Observable<Result<RentalExtra>> {
    return this.http.post<Result<RentalExtra>>(`${this.baseUrl}/extras/${rentalId}`, data);
  }

  
  // * Kiralama'dan extra kaldırır
  removeExtra(rentalId: string, extraId: string): Observable<Result<boolean>> {
    return this.http.delete<Result<boolean>>(`${this.baseUrl}/${rentalId}/extras/${extraId}`);
  }

  
  // * Kiralama'daki extra miktarını günceller
  updateExtraQuantity(rentalId: string, extraId: string, quantity: number): Observable<Result<RentalExtra>> {
    return this.http.put<Result<RentalExtra>>(
      `${this.baseUrl}/${rentalId}/extras/${extraId}`,
      { quantity }
    );
  }

  
  // * Kiralama'daki extra'ların toplam fiyatını getirir
  getTotalPrice(rentalId: string): Observable<Result<number>> {
    return this.http.get<Result<number>>(`${this.baseUrl}/${rentalId}/extras/total`);
  }
}
