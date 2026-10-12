import { ChangeDetectionStrategy, Component, EventEmitter, inject, Input, Output, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { MessageService } from 'primeng/api';
import { ButtonModule } from 'primeng/button';
import { DialogModule } from 'primeng/dialog';
import { TextareaModule } from 'primeng/textarea';
import { finalize } from 'rxjs';
import { Result } from '../../../../../core/models/result.model';
import { CustomerManagementService } from '../../../../core/services/customers/customer-management.service';

@Component({
  selector: 'app-blacklist-dialog',
  imports: [
    FormsModule,
    DialogModule,
    ButtonModule,
    TextareaModule
  ],
  templateUrl: './blacklist-dialog.component.html',
  styleUrl: './blacklist-dialog.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class BlacklistDialogComponent {
  private customerService = inject(CustomerManagementService);
  private messageService = inject(MessageService);

  // ==================== INPUTS ====================
  @Input() visible = false;
  @Input() customerId: string | null = null;
  @Input() customerName: string = '';

  // ==================== OUTPUTS ====================
  @Output() visibleChange = new EventEmitter<boolean>();
  @Output() onSuccess = new EventEmitter<void>();


  // ==================== STATE ====================
  reason = signal<string>('');
  isLoading = signal<boolean>(false);

  // ==================== COMPUTED ====================
  get canSubmit(): boolean {
    return this.reason().trim().length >= 10 && !this.isLoading();
  }

  // ==================== ACTIONS ====================
  onSubmit(): void {
    const id = this.customerId;
    const reason = this.reason().trim();

    if (!id) {
      this.messageService.add({
        severity: 'error',
        summary: 'Hata',
        detail: 'Müşteri ID bulunamadı.'
      });
      return;
    }

    if (reason.length < 10) {
      this.messageService.add({
        severity: 'warn',
        summary: 'Uyarı',
        detail: 'Kara liste sebebi en az 10 karakter olmalıdır.'
      });
      return;
    }

    this.isLoading.set(true);

    this.customerService.blacklistCustomer({ customerId: id, reason })
      .pipe(finalize(() => this.isLoading.set(false)))
      .subscribe({
        next: (res: Result<string>) => {
          if (res.isSuccessful) {
            this.messageService.add({
              severity: 'success',
              summary: 'Başarılı',
              detail: `"${this.customerName}" kara listeye alındı.`,
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
    this.reason.set('');
  }

  onDialogHide(): void {
    this.visibleChange.emit(false);
    this.reason.set('');
  }

}
