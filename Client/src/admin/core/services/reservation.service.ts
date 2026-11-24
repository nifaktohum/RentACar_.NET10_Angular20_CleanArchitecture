// reservation.service.ts
import { HttpClient, HttpParams } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { environment } from '../../../environments/environment';
import { ReservationFilter } from '../models/reservations/reservation-filter.model';
import { Observable } from 'rxjs';
import { PagedResult, Result } from '../../../core/models/result.model';
import { Reservation } from '../models/reservations/reservation.model';
import { ReservationDetail } from '../models/reservations/reservation-detail.model';
import { ReservationUpdateStatus } from '../models/reservations/reservation-update-status.model';
import { ReservationStats } from '../models/reservations/reservation-stats.model';
import { CreateReservationRequest } from '../models/reservations/reservation-create.model';

@Injectable({
  providedIn: 'root',
})
export class ReservationService {
  private http = inject(HttpClient);
  private apiUrl = `${environment.apiUrl}/Reservations`;

  getAll(filters: ReservationFilter): Observable<Result<PagedResult<Reservation>>> {
    let params = new HttpParams()
      .set('pageNumber', filters.pageNumber.toString())
      .set('pageSize', filters.pageSize.toString());

    if (filters.searchTerm) params = params.set('searchTerm', filters.searchTerm);
    if (filters.status) params = params.set('status', filters.status);
    if (filters.customerId) params = params.set('customerId', filters.customerId);
    if (filters.sort) params = params.set('sort', filters.sort);

    // 🆕 AKTİF REZERVASYONLAR FİLTRESİ
    if (filters.activeOnly) params = params.set('activeOnly', 'true');
    if (filters.paymentFilter) params = params.set('paymentFilter', filters.paymentFilter);

    return this.http.get<Result<PagedResult<Reservation>>>(`${this.apiUrl}/get-all`, { params });
  }

  getById(id: string): Observable<Result<ReservationDetail>> {
    return this.http.get<Result<ReservationDetail>>(`${this.apiUrl}/get-by-id/${id}`);
  }

  create(request: CreateReservationRequest): Observable<Result<string>> {
    return this.http.post<Result<string>>(`${this.apiUrl}/create`, request);
  }
  
  payDeposit(id: string, paymentId: string): Observable<Result<any>> {
    return this.http.post<Result<any>>(`${this.apiUrl}/pay-deposit/${id}`, {
      reservationId: id,
      paymentId: paymentId
    });
  }

  payFullPayment(id: string, fullPaymentId: string): Observable<Result<any>> {
    return this.http.post<Result<any>>(`${this.apiUrl}/pay-full-payment/${id}`, {
      reservationId: id,
      fullPaymentId: fullPaymentId
    });
  }

  updateStatus(command: ReservationUpdateStatus): Observable<Result<ReservationDetail>> {
    return this.http.put<Result<ReservationDetail>>(`${this.apiUrl}/update-status`, command);
  }

  activate(id: string, actualPickupAt: string) {
    return this.http.post<Result<any>>(`${this.apiUrl}/activate/${id}`, {
      reservationId: id,
      actualPickupAt
    });
  }

  complete(id: string, actualReturnAt: string) {
    return this.http.post<Result<any>>(`${this.apiUrl}/complete/${id}`, {
      reservationId: id,
      actualReturnAt
    });
  }

  getStats(): Observable<Result<ReservationStats>> {
    return this.http.get<Result<ReservationStats>>(`${this.apiUrl}/Get-Stats`);
  }

  cancel(id: string, reason: string): Observable<Result<boolean>> {
    return this.http.post<Result<boolean>>(`${this.apiUrl}/cancel/${id}`, { reservationId: id, reason });
  }
}