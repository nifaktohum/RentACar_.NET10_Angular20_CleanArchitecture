import { ApplicationConfig, provideBrowserGlobalErrorListeners, provideZonelessChangeDetection } from '@angular/core';
import { provideRouter } from '@angular/router'

import { routes } from './app.routes'
import { provideHttpClient, withInterceptors } from '@angular/common/http'
import { httpInterceptor } from './core/interceptors/http.interceptor'
import { errorInterceptor } from './core/interceptors/error.interceptor';
import { loadingInterceptor } from './core/interceptors/loading.interceptor';
import { provideAnimations } from '@angular/platform-browser/animations';
import { providePrimeNG } from 'primeng/config';
import lara from '@primeng/themes/lara';
import { ConfirmationService, MessageService } from 'primeng/api';


export const appConfig: ApplicationConfig = {
  providers: [
    provideBrowserGlobalErrorListeners(),
    provideZonelessChangeDetection(),
    provideAnimations(),
    provideRouter(routes),
    provideHttpClient(withInterceptors([
      httpInterceptor,    // 1. Auth / Token Ekleme
      loadingInterceptor, // 2. Loader / Spinner Yönetimi
      errorInterceptor    // 3. Global Hata Yakalama
    ])),
    MessageService, ConfirmationService,  
    providePrimeNG({
      theme: {
        preset: lara,        
        options: {
          darkModeSelector: '.dark'
        }
      },
      ripple: true,
      inputStyle: 'outlined'
    })
  ]
};

