import { ChangeDetectionStrategy, Component, computed, EventEmitter, inject, Input, Output, signal, ViewEncapsulation } from '@angular/core';
import { MessageService } from 'primeng/api';
import { CustomerManagementService } from '../../../../core/services/customers/customer-management.service';
import { finalize } from 'rxjs';
import { Result } from '../../../../../core/models/result.model';
import { DialogModule } from 'primeng/dialog';
import { InputNumberModule } from 'primeng/inputnumber';
import { FormsModule } from '@angular/forms';
import { TagModule } from 'primeng/tag';
import { ButtonModule } from 'primeng/button';

@Component({
  selector: 'app-findeks-dialog',
  imports: [
    DialogModule,
    InputNumberModule,
    FormsModule,
    TagModule,
    ButtonModule
  ],
  templateUrl: './findeks-dialog.component.html',
  styleUrl: './findeks-dialog.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
  encapsulation: ViewEncapsulation.Emulated
})
export class FindeksDialogComponent {
  private customerService = inject(CustomerManagementService);
  private messageService = inject(MessageService);

  // ==================== INPUTS ====================
  @Input() visible = false;
  @Input() customerId: string | null = null;
  @Input() customerName: string = '';
  @Input() currentScore: number | null = null;

  // ==================== OUTPUTS ====================
  @Output() visibleChange = new EventEmitter<boolean>();
  @Output() onSuccess = new EventEmitter<void>();

  // ==================== STATE ====================
  newScore = signal<number | null>(null);
  isLoading = signal<boolean>(false);

  // ==================== COMPUTED ====================
  canSubmit = computed(() => {
    const score = this.newScore();
    return score != null && score >= 0 && score <= 1900 && !this.isLoading();
  });

  previewPercent = computed(() => {
    const score = this.newScore();
    if (score == null) return 0;
    return Math.round((score / 1900) * 100);
  });

  previewStatus = computed(() => {
    const score = this.newScore();
    if (score == null) return { label: 'Belirsiz', severity: 'secondary' as const };

    if (score >= 1500) return { label: 'Çok İyi', severity: 'success' as const };
    if (score >= 1200) return { label: 'İyi', severity: 'info' as const };
    if (score >= 900) return { label: 'Orta', severity: 'warn' as const };
    return { label: 'Düşük', severity: 'danger' as const };
  });

  // ==================== ACTIONS ====================
  onSubmit(): void {
    const id = this.customerId;
    const score = this.newScore();

    if (!id) {
      this.messageService.add({
        severity: 'error',
        summary: 'Hata',
        detail: 'Müşteri ID bulunamadı.'
      });
      return;
    }

    if (score == null || score < 0 || score > 1900) {
      this.messageService.add({
        severity: 'warn',
        summary: 'Uyarı',
        detail: 'Skor 0-1900 arasında olmalıdır.'
      });
      return;
    }

    this.isLoading.set(true);

    this.customerService.updateFindeksScore({ customerId: id, score })
      .pipe(finalize(() => this.isLoading.set(false)))
      .subscribe({
        next: (res: Result<string>) => {
          if (res.isSuccessful) {
            this.messageService.add({
              severity: 'success',
              summary: 'Başarılı',
              detail: `"${this.customerName}" Findeks skoru ${score} olarak güncellendi.`,
              life: 4000
            });
            this.close();
            this.onSuccess.emit();
          } else {
            this.messageService.add({
              severity: 'warn',
              summary: 'Uyarı',
              detail: res.errorMessages?.[0] || 'İşlem başarısız.'
            });
          }
        },
        error: (err) => {
          this.messageService.add({
            severity: 'error',
            summary: 'Hata',
            detail: err?.error?.errorMessages?.[0]
              || err?.error?.message
              || 'Sunucu hatası.'
          });
        }
      });
  }

  close(): void {
    this.visible = false;
    this.visibleChange.emit(false);
    this.newScore.set(null);
  }

  onDialogHide(): void {
    this.visibleChange.emit(false);
    this.newScore.set(null);
  }

  onDialogShow(): void {
    // Dialog açıldığında mevcut skoru input'a doldur
    if (this.currentScore != null) {
      this.newScore.set(this.currentScore);
    } else {
      this.newScore.set(null);
    }
  }











}
