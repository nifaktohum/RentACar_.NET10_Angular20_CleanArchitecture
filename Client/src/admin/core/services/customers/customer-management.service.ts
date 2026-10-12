import { HttpClient, HttpContext, HttpParams } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { environment } from '../../../../environments/environment';
import { Result } from '../../../../core/models/result.model';
import { BYPASS_INTERCEPTOR } from '../../../../core/interceptors/error.interceptor';
import { CustomerStateService } from './customer-state.service';
import { CustomerSpecParams } from '../../models/customers/customer-spec-params';

@Injectable({
  providedIn: 'root',
})
export class CustomerManagementService {
  private http = inject(HttpClient);
  private state = inject(CustomerStateService);
  private readonly apiUrl = environment.apiUrl + '/CustomersManagement';

  getCustomers(params: CustomerSpecParams) {
    const specParams = this.toHttpParams(this.state.filterParams());
    return this.http.get<Result<any>>(this.apiUrl + '/get-all',
      { params: specParams });
  }

  getCustomerById(id: string) {
    return this.http.get<Result<any>>(`${this.apiUrl}/get-by-id/${id}`);
  }
  // Kara Liste
  blacklistCustomer(data: { customerId: string; reason: string }) {
    return this.http.post<Result<string>>(this.apiUrl + '/blacklist-added', data );
  }

  removeFromBlacklist(id: string) {
    return this.http.delete<Result<string>>(`${this.apiUrl}/blacklist-removed/${id}` );
  }
  // Findeks
  updateFindeksScore(data: { customerId: string; score: number }) {
    return this.http.put<Result<string>>(this.apiUrl + '/findeks-added', data );
  }


  private toHttpParams(obj: CustomerSpecParams): HttpParams {
    let params = new HttpParams();
    Object.entries(obj).forEach(([key, value]) => {
      if (value !== null && value !== undefined && value !== '') {
        params = params.set(key, String(value));
      }
    });
    return params;
  }

}
