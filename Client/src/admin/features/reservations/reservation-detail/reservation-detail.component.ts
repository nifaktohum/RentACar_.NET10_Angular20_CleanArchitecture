import { ChangeDetectionStrategy, Component, inject, OnInit, signal } from '@angular/core';
import { CommonModule, DatePipe, CurrencyPipe } from '@angular/common';
import { ActivatedRoute, Router } from '@angular/router';
import { MessageService } from 'primeng/api';

import { CardModule } from 'primeng/card';
import { ButtonModule } from 'primeng/button';
import { TagModule } from 'primeng/tag';
import { DividerModule } from 'primeng/divider';
import { DialogModule } from 'primeng/dialog';
import { InputTextModule } from 'primeng/inputtext';
import { InputNumberModule } from 'primeng/inputnumber';
import { FormsModule } from '@angular/forms';
import { TooltipModule } from 'primeng/tooltip';

import { ReservationStatusEnum } from '../../../../shared/enums/reservation-status';
import { ReservationDetail } from '../../../core/models/reservations/reservation-detail.model';
import { ReservationService } from '../../../core/services/reservation.service';
import { VehicleImageService } from '../../../core/services/vehicle-image.service';
import { CustomConfirmDialogService } from '../../../shared/services/custom-confirm-dialog.service';

@Component({
  selector: 'app-reservation-detail',
  standalone: true,
  imports: [
    CommonModule, DatePipe, CurrencyPipe, FormsModule,
    CardModule, ButtonModule, TagModule, DividerModule,
    DialogModule, InputTextModule, InputNumberModule, TooltipModule
  ],
  templateUrl: './reservation-detail.component.html',
  styleUrl: './reservation-detail.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class ReservationDetailComponent implements OnInit {
  private route = inject(ActivatedRoute);
  private router = inject(Router);
  private reservationService = inject(ReservationService);
  public vehicleImageService = inject(VehicleImageService);
  private messageService = inject(MessageService);
  private confirmDialog = inject(CustomConfirmDialogService);

  // Signals
  readonly reservation = signal<ReservationDetail | null>(null);
  readonly isLoading = signal<boolean>(false);
  readonly vehicleImageUrl = signal<string>('assets/no-image.jpg');

  // Dialog State
  readonly displayActionDialog = signal<boolean>(false);
  readonly pendingAction = signal<'deposit' | 'fullPayment' | 'activate' | 'complete' | null>(null);
  readonly isSubmitting = signal<boolean>(false);

  // Form Inputs
  paymentIdInput = '';
  returnKilometerInput: number | null = null;
  actualDateInput = new Date(); // Activate veya Complete için tarih/saat

  readonly StatusEnum = ReservationStatusEnum;

  ngOnInit(): void {
    const id = this.route.snapshot.paramMap.get('id');
    if (id) {
      this.loadReservation(id);
    } else {
      this.router.navigate(['/admin/reservations']);
    }
  }

  loadReservation(id: string): void {
    this.isLoading.set(true);
    this.reservationService.getById(id).subscribe({
      next: (res) => {
        if (res.isSuccessful && res.data) {
          this.reservation.set(res.data);
          this.loadVehicleImage(res.data.vehicleId);
        }
      },
      error: () => {
        this.messageService.add({ severity: 'error', summary: 'Hata', detail: 'Rezervasyon yüklenemedi.' });
        this.router.navigate(['/admin/reservations']);
      },
      complete: () => this.isLoading.set(false)
    });
  }

  private loadVehicleImage(vehicleId: string): void {
    this.vehicleImageService.getImagesByVehicle(vehicleId).subscribe({
      next: (imgRes) => {
        if (imgRes.isSuccessful && imgRes.data && imgRes.data.length > 0) {
          const mainImage = imgRes.data.find(i => i.isMain) || imgRes.data[0];
          const url = this.vehicleImageService.getImageUrl(mainImage.imageUrl);
          if (url) {
            this.vehicleImageUrl.set(url);
          }
        }
      },
      error: (err) => {
        console.error('Araç resmi yüklenemedi:', err);
      }
    });
  }

  openActionDialog(action: 'deposit' | 'fullPayment' | 'activate' | 'complete'): void {
    this.pendingAction.set(action);

    // Form değişkenlerini sıfırla
    this.paymentIdInput = 'MOCK-PAYMENT-' + Math.floor(Math.random() * 10000);
    this.returnKilometerInput = null;
    this.actualDateInput = new Date(); // Şu anki zamanı default yap

    this.displayActionDialog.set(true);
  }

  confirmAction(): void {
    const res = this.reservation();
    const action = this.pendingAction();
    if (!res || !action) return;

    this.isSubmitting.set(true);

    // 🎯 İşleme göre doğru endpoint'i çağırıyoruz
    switch (action) {
      // 1. DEPOZİTO AL (Kapora)
      case 'deposit':
        this.reservationService.payDeposit(res.id, this.paymentIdInput).subscribe({
          next: (result) => this.handleSuccess(result),
          error: (err) => this.handleError(err),
          complete: () => this.isSubmitting.set(false)
        });
        break;

      // 2. TAM ÖDEME AL
      case 'fullPayment':
        this.reservationService.payFullPayment(res.id, this.paymentIdInput).subscribe({
          next: (result) => this.handleSuccess(result),
          error: (err) => this.handleError(err),
          complete: () => this.isSubmitting.set(false)
        });
        break;

      // 3. ARACI TESLİM ET (Active)
      case 'activate':
        this.reservationService.activate(res.id, this.actualDateInput.toISOString()).subscribe({
          next: (result) => this.handleSuccess(result),
          error: (err) => this.handleError(err),
          complete: () => this.isSubmitting.set(false)
        });
        break;

      // 4. ARACI İADE AL (Completed)
      case 'complete':
        // Not: İade alırken km bilgisi de gerekebilir. Backend CompleteReservationCommand'a bakarak
        // eğer km alanı varsa buraya ekleyebilirsin. Şimdilik sadece tarih gönderiyoruz.
        this.reservationService.complete(res.id, this.actualDateInput.toISOString()).subscribe({
          next: (result) => this.handleSuccess(result),
          error: (err) => this.handleError(err),
          complete: () => this.isSubmitting.set(false)
        });
        break;
    }
  }

  private handleSuccess(result: any): void {
    if (result.isSuccessful) {
      this.messageService.add({ severity: 'success', summary: 'Başarılı', detail: 'İşlem başarıyla tamamlandı.' });
      this.displayActionDialog.set(false);

      const res = this.reservation();
      if (res) {
        this.loadReservation(res.id); // Sayfayı yenile
      }
    }
  }

  private handleError(err: any): void {
    this.messageService.add({
      severity: 'error',
      summary: 'Hata',
      detail: err?.error?.errorMessages?.[0] || 'İşlem başarısız.'
    });
  }

  cancelReservation(): void {
    const res = this.reservation();
    if (!res) return;

    this.confirmDialog.showConfirm(
      'Rezervasyon İptali',
      `"${res.reservationNumber}" numaralı rezervasyonu iptal etmek istediğinize emin misiniz?`,
      () => {
        this.reservationService.cancel(res.id, 'Admin tarafından detay sayfasından iptal edildi.').subscribe({
          next: (result) => {
            if (result.isSuccessful) {
              this.messageService.add({ severity: 'success', summary: 'Başarılı', detail: 'Rezervasyon iptal edildi.' });
              this.loadReservation(res.id);
            }
          },
          error: (err) => {
            this.messageService.add({
              severity: 'error',
              summary: 'Hata',
              detail: err?.error?.errorMessages?.[0] || 'İptal edilemedi.'
            });
          }
        });
      },
      undefined, 'Evet, İptal Et', 'Vazgeç', 'p-button-danger'
    );
  }

  goBack(): void {
    this.router.navigate(['/admin/reservations']);
  }

  getStatusSeverity(status: string): 'success' | 'info' | 'warn' | 'danger' | 'secondary' {
    switch (status) {
      case 'PendingPayment': return 'warn';
      case 'Confirmed': return 'info';
      case 'Active': return 'success';
      case 'Completed': return 'success';
      case 'Expired': return 'secondary';
      case 'NoShow': return 'danger';
      case 'Cancelled': return 'danger';
      default: return 'secondary';
    }
  }

  getStatusLabel(status: string): string {
    const labels: Record<string, string> = {
      'PendingPayment': 'Ödeme Bekliyor',
      'Confirmed': 'Onaylandı',
      'Active': 'Kirada',
      'Completed': 'Tamamlandı',
      'Expired': 'Süresi Doldu',
      'NoShow': 'Gelmedi',
      'Cancelled': 'İptal Edildi'
    };
    return labels[status] || status;
  }

  // Dialog başlığı için
  getActionLabel(action: string | null): string {
    switch (action) {
      case 'deposit': return 'Depozito Ödemesi Al';
      case 'fullPayment': return 'Tam Ödeme Al';
      case 'activate': return 'Aracı Teslim Et';
      case 'complete': return 'Aracı İade Al';
      default: return 'İşlem Yap';
    }
  }
}