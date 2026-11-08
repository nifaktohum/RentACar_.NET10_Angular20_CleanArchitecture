import { HttpClient, HttpParams } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { environment } from '../../../environments/environment';
import { ReservationFilter } from '../models/reservations/reservation-filter.model';
import { Observable } from 'rxjs';
import { PagedResult, Result } from '../../../core/models/result.model';
import { Reservation } from '../models/reservations/reservation.model';

@Injectable({
  providedIn: 'root',
})
export class ReservationService {
  private http = inject(HttpClient);
  private apiUrl = `${environment.apiUrl}/reservations`;

  getAll(filters: ReservationFilter): Observable<Result<PagedResult<Reservation>>> {
    let params = new HttpParams()
      .set('pageNumber', filters.pageNumber.toString())
      .set('pageSize', filters.pageSize.toString());

    if (filters.searchTerm) params = params.set('searchTerm', filters.searchTerm);
    if (filters.status) params = params.set('status', filters.status);
    if (filters.customerId) params = params.set('customerId', filters.customerId);
    if (filters.sort) params = params.set('sort', filters.sort);

    return this.http.get<Result<PagedResult<Reservation>>>(`${this.apiUrl}/get-all`, { params });
  }

  // updateStatus(command: UpdateReservationStatusCommand): Observable<Result<ReservationDetail>> {
  //   return this.http.put<Result<ReservationDetail>>(
  //     `${this.apiUrl}/update-status/${command.reservationId}`,
  //     command
  //   );
  // }

  cancel(id: string, reason: string): Observable<Result<boolean>> {
    return this.http.post<Result<boolean>>(`${this.apiUrl}/${id}/cancel`, { reservationId: id, reason });
  }
  
}
