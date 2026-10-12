import { ChangeDetectionStrategy, Component, computed, inject, OnDestroy, OnInit, signal, ViewEncapsulation } from '@angular/core';
import { FormsModule, NgForm } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { ButtonModule } from 'primeng/button';
import { TooltipModule } from 'primeng/tooltip';
import { CodeFormatPipe } from '../../../shared/pipes/code-format.pipe';
import { MessageService } from 'primeng/api';
import { CustomerAuthService } from '../../../core/services/customers/customer-auth.service';
import { VerifyState } from '../../../shared/enums/verify-state';
import { Result } from '../../../core/models/result.model';
import { finalize } from 'rxjs';

@Component({
  selector: 'app-verify-phone',
  imports: [
    FormsModule,
    CodeFormatPipe,
    ButtonModule,
    TooltipModule,
    RouterLink
  ],
  templateUrl: './verify-phone.component.html',
  styleUrl: './verify-phone.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
  encapsulation: ViewEncapsulation.Emulated
})
export class VerifyPhoneComponent implements OnInit, OnDestroy {
  private customerAuth = inject(CustomerAuthService);
  private router = inject(Router);
  private messageService = inject(MessageService);

  // ==================== STATE ====================
  state = signal<VerifyState>(VerifyState.Pending);
  code = signal<string>('');
  loading = signal<boolean>(false);
  resending = signal<boolean>(false);

  cooldownSeconds = signal<number>(0);
  private cooldownInterval?: ReturnType<typeof setInterval>;

  // ==================== COMPUTED ====================
  customerPhone = computed(() => {
    return this.customerAuth.currentCustomer()?.['PhoneNumber'] || '';
  });

  isPhoneVerified = computed(() => this.customerAuth.isPhoneVerified());

  formattedCooldown = computed(() => {
    const total = this.cooldownSeconds();
    const minutes = Math.floor(total / 60);
    const seconds = total % 60;
    return `${String(minutes).padStart(2, '0')}:${String(seconds).padStart(2, '0')}`;
  });

  canResend = computed(() => this.cooldownSeconds() === 0 && !this.resending());

  canVerify = computed(() => {
    const cleaned = this.code().replace(/-/g, '').trim();
    return cleaned.length === 6 && !this.loading();
  });

  ngOnInit(): void {
    if (this.isPhoneVerified()) {
      this.state.set(VerifyState.Verified);
    }

  }

  ngOnDestroy(): void {
    this.stopCooldown();
  }

  // ==================== KOD GÖNDER ====================
  sendCode(): void {
    if (this.loading()) return;

    this.loading.set(true);

    this.customerAuth.sendSmsVerificationCode()
      .pipe(finalize(() => this.loading.set(false)))
      .subscribe({
        next: (res: Result<string>) => {
          if (res.isSuccessful) {
            this.messageService.add({
              severity: 'success',
              summary: 'Kod Gönderildi',
              detail: '6 haneli doğrulama kodu telefon numaranıza SMS olarak gönderildi.',
              life: 5000
            });
            this.state.set(VerifyState.CodeSent);
            this.startCooldown(60);
          } else {
            this.messageService.add({
              severity: 'warn',
              summary: 'Uyarı',
              detail: res.errorMessages?.[0] || 'Kod gönderilemedi.'
            });
          }
        },
        error: (err) => {
          this.messageService.add({
            severity: 'error',
            summary: 'Hata',
            detail: err?.error?.errorMessages?.[0]
              || err?.error?.message
              || 'Sunucu hatası, lütfen tekrar deneyin.'
          });
        }
      });
  }

  // ==================== YENİDEN GÖNDER ====================
  resendCode(): void {
    if (!this.canResend()) return;

    this.resending.set(true);
    this.code.set('');

    this.customerAuth.sendSmsVerificationCode()
      .pipe(finalize(() => this.resending.set(false)))
      .subscribe({
        next: (res: Result<string>) => {
          if (res.isSuccessful) {
            this.messageService.add({
              severity: 'success',
              summary: 'Kod Yeniden Gönderildi',
              detail: 'Yeni kod telefon numaranıza SMS olarak gönderildi.',
              life: 4000
            });
            this.startCooldown(60);
          } else {
            this.messageService.add({
              severity: 'warn',
              summary: 'Uyarı',
              detail: res.errorMessages?.[0] || 'Kod gönderilemedi.'
            });
          }
        },
        error: (err) => {
          this.messageService.add({
            severity: 'error',
            summary: 'Hata',
            detail: err?.error?.errorMessages?.[0]
              || err?.error?.message
              || 'Sunucu hatası, lütfen tekrar deneyin.'
          });
        }
      });
  }

  // ==================== KOD DOĞRULA ====================
  verifyCode(form: NgForm): void {
    form.control.markAllAsTouched();
    const cleanCode = this.code().replace(/-/g, '').trim();

    if (cleanCode.length !== 6) {
      this.messageService.add({
        severity: 'warn',
        summary: 'Uyarı',
        detail: 'Lütfen 6 haneli doğrulama kodunu girin.'
      });
      return;
    }

    this.loading.set(true);

    this.customerAuth.verifyPhone(cleanCode)
      .pipe(finalize(() => this.loading.set(false)))
      .subscribe({
        next: () => {
          this.state.set(VerifyState.Verified);
          this.stopCooldown();

          this.messageService.add({
            severity: 'success',
            summary: 'Başarılı',
            detail: 'Telefon numaranız başarıyla doğrulandı!',
            life: 5000
          });

          setTimeout(() => {
            this.router.navigate(['/customer/profile']);
          }, 2000);
        },
        error: (err) => {
          this.messageService.add({
            severity: 'error',
            summary: 'Hata',
            detail: err?.error?.errorMessages?.[0]
              || err?.error?.message
              || 'Kod hatalı veya süresi dolmuş.'
          });
        }
      });
  }

  // ==================== GERİ SAYIM ====================
  private startCooldown(seconds: number): void {
    this.stopCooldown();
    this.cooldownSeconds.set(seconds);

    this.cooldownInterval = setInterval(() => {
      const current = this.cooldownSeconds();
      if (current <= 1) {
        this.cooldownSeconds.set(0);
        this.stopCooldown();
      } else {
        this.cooldownSeconds.set(current - 1);
      }
    }, 1000);
  }

  private stopCooldown(): void {
    if (this.cooldownInterval) {
      clearInterval(this.cooldownInterval);
      this.cooldownInterval = undefined;
    }
  }

  // ==================== NAVIGATION ====================
  goToProfile(): void {
    this.router.navigate(['/customer/profile']);
  }

  // ==================== HELPERS ====================
  updateCode(value: string): void {
    this.code.set(value);
  }




}
