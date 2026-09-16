import { HttpClient, HttpContext } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { environment } from '../../../environments/environment';
import { Observable } from 'rxjs';
import { Result } from '../../../core/models/result.model';
import { SetMainImageRequest, VehicleImage } from '../models/vehicle/vehicle-image.model';
import { BYPASS_INTERCEPTOR } from '../../../core/interceptors/error.interceptor';

@Injectable({
  providedIn: 'root',
})
export class VehicleImageService {
  private http = inject(HttpClient);
  private readonly baseUrl = `${environment.apiUrl}/vehicleimages`;

  // ==================== GET IMAGES ====================
  getImagesByVehicle(vehicleId: string): Observable<Result<VehicleImage[]>> {
    return this.http.get<Result<VehicleImage[]>>(
      `${this.baseUrl}/get-vehicle-images/${vehicleId}`
    );
  }

  // ==================== UPLOAD (ÇOKLU) ====================
  // ⭐ POST /api/VehicleImages/upload
  uploadImages(vehicleId: string, files: File[], isMain: boolean = false): Observable<Result<VehicleImage[]>> {
    const formData = new FormData();

    // ⭐ Postman'deki isimler (küçük harf)
    formData.append('vehicleId', vehicleId);

    files.forEach((file) => {
      formData.append('files', file, file.name);
    });

    formData.append('isMain', isMain.toString());

    return this.http.post<Result<VehicleImage[]>>(`${this.baseUrl}/upload`, formData);
  }

  // ==================== SET MAIN IMAGE ====================
  setMainImage(vehicleId: string, imageId: string): Observable<Result<void>> {
    const request: SetMainImageRequest = { vehicleId, imageId };
    return this.http.patch<Result<void>>(`${this.baseUrl}/set-main`, request, {
      context: new HttpContext().set(BYPASS_INTERCEPTOR, true)
    });
  }

  // ==================== DELETE IMAGE ====================
  deleteImage(imageId: string): Observable<Result<void>> {
    return this.http.delete<Result<void>>(`${this.baseUrl}/delete/${imageId}`);
  }

  // ==================== GET IMAGE URL ====================
  getImageUrl(imageUrl: string | null | undefined): string | null {
    if (!imageUrl || imageUrl.trim() === '') return null;
    if (imageUrl.startsWith('http://') || imageUrl.startsWith('https://')) return imageUrl;

    const baseUrl = environment.apiUrl.replace(/\/api\/?$/, '');
    const path = imageUrl.startsWith('/') ? imageUrl : `/${imageUrl}`;

    return `${baseUrl}${path}`;
  }
}
