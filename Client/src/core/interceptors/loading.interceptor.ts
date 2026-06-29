import { HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { delay, finalize, identity, OperatorFunction } from 'rxjs';
import { environment } from '../../environments/environment';
import { BusyService } from '../services/busy.service';

export const loadingInterceptor: HttpInterceptorFn = (req, next) => {
  const busyService = inject(BusyService);
  // 🔄 İstek çıktığı an Signal 'true' moduna geçiyor
  busyService.busy();

  return next(req).pipe(
    (environment.production ? identity : delay(500)),
    // 🔄 İstek bittiği an Signal 'false' moduna geçiyor
    finalize(() => busyService.idle())
  );
};
