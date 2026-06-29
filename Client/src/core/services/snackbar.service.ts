import { inject, Injectable } from '@angular/core';
import { MatSnackBar } from '@angular/material/snack-bar';

@Injectable({
  providedIn: 'root'
})
export class SnackbarService {
  private snackbar = inject(MatSnackBar);

  // Varsayılan ayarlar
  private defaultConfig = {
    duration: 10000,
    horizontalPosition: 'right' as const,
    verticalPosition: 'top' as const
  };

  show(message: string, action: string = 'Kapat',
    duration: number = 10000): void {
    this.snackbar.open(message, action, {
      duration,
      horizontalPosition: 'right',
      verticalPosition: 'top'
    }); }
 //Başarı mesajı
  success(message: string): void { 
    this.snackbar.open(message, 'Kapat', {
      ...this.defaultConfig,
      panelClass: ['snackbar-success'],
    }); }
 //Hata mesajı
  error(message: string): void {
    this.snackbar.open(message, 'Kapat', {
      ...this.defaultConfig,
      panelClass: ['snackbar-error'],
    }); }
 //Uyarı mesajı
  warning(message: string): void {
    this.snackbar.open(message, 'Kapat', {
      ...this.defaultConfig,
      panelClass: ['snackbar-warning'],
    });
  }
  
  // Info mesajı da ekleyebilirsin
  info(message: string): void {
    this.snackbar.open(message, 'Kapat', {
      ...this.defaultConfig,
      panelClass: ['snackbar-info']
    });
  }


}
