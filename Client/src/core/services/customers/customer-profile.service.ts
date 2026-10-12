import { HttpClient, HttpContext } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { environment } from '../../../environments/environment';
import { Result } from '../../models/result.model';
import { BYPASS_INTERCEPTOR } from '../../interceptors/error.interceptor';
import { TokenType, withAuthOptions } from '../../interceptors/token-context';

@Injectable({
  providedIn: 'root',
})
export class CustomerProfileService {
  private http = inject(HttpClient);
  private readonly apiUrl = environment.apiUrl + '/customersprofile';

  getProfile() {
    return this.http.get<Result<any>>(this.apiUrl + '/get-profile', 
      withAuthOptions(TokenType.Customer) 
    );
  }

  updateProfile(data: any) {
    return this.http.put<Result<string>>(this.apiUrl + '/update-profile',
      data,
      withAuthOptions(TokenType.Customer)
    );
  }
}