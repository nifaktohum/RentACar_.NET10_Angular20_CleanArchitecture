import { ChangeDetectionStrategy, Component, HostListener, inject, signal } from '@angular/core';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { Router, RouterOutlet } from '@angular/router';
import { BusyService } from './core/services/busy.service';
import { environment } from './environments/environment';
import { ToastModule } from 'primeng/toast';
import { ConfirmDialogModule } from 'primeng/confirmdialog';

@Component({
  selector: 'app-root',
  imports: [RouterOutlet,
    MatProgressBarModule,
    ToastModule, ConfirmDialogModule
  ],
  templateUrl: './app.html',
  styleUrl: './app.scss',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class App {
  // protected readonly title = signal('Client');
  readonly router = inject(Router);
  readonly busyService = inject(BusyService);
  readonly environment = environment;

  // 🔥 SENİN DÜZENİNE UYGUN DOKUNUŞ: Tarayıcının hafıza olaylarını dinliyoruz
  @HostListener('window:storage', ['$event'])
    
  onStorageChange(event: StorageEvent) {
    // 🔥 TEST İÇİN: Konsola bir log at, tetikleniyor mu görelim
    console.log('Storage değişti!', event);

    // Eğer localStorage tamamen temizlendiyse (clear) event.key null gelir.
    // Ya da senin token key adın neyse (örn: 'token' veya 'accessToken') onu kontrol etmeliyiz.
    if (event.key === null || event.key === 'token') {
      // Token'ın gerçekten silindiğinden emin oluyoruz
      if (!localStorage.getItem('token')) {
        console.log('Token silinmiş, logine uçuruyorum!');

      this.router.navigate(['/auth/login']); // Kendi login path'ine göre güncelle
        
      }
    }
  }

  get debugInfo() {
    return this.busyService.getDebugInfo();
  }
}
