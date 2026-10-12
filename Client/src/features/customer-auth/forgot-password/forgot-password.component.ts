import { ChangeDetectionStrategy, Component, inject, signal, ViewEncapsulation } from '@angular/core';
import { Router } from '@angular/router';
import { MessageService } from 'primeng/api';
import { CustomerAuthService } from '../../../core/services/customers/customer-auth.service';
import { finalize } from 'rxjs';
import { FormsModule } from '@angular/forms';
import { PhoneFormatPipe } from '../../../shared/pipes/phone-format.pipe';
import { CodeFormatPipe } from '../../../shared/pipes/code-format.pipe';
import { PasswordValidationDirective } from '../../../shared/directives/password-validation.directive';
import { TogglePasswordDirective } from '../../../shared/directives/toggle-password.directive';
import { ConfirmPasswordValidationDirective } from '../../../shared/directives/confirm-password-validation.directive';
import { AuthService } from '../../../core/services/auth.service';

@Component({
  selector: 'app-forgot-password',
  imports: [
    FormsModule,
    PhoneFormatPipe,
    CodeFormatPipe,
    //Directive
    PasswordValidationDirective,
    TogglePasswordDirective,
    ConfirmPasswordValidationDirective
],
  templateUrl: './forgot-password.component.html',
  styleUrl: './forgot-password.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
  encapsulation: ViewEncapsulation.Emulated
})
export class ForgotPasswordComponent {
  private customerAuthService = inject(CustomerAuthService);
  private authService = inject(AuthService);
  private router = inject(Router);
  private messageService = inject(MessageService);

  // ==================== STATE ====================
  activeMethod = signal<'email' | 'sms'>('email');
  currentStep = signal<number>(1);   // 1: bilgi gir, 2: kod + yeni şifre

  sendLoading = signal<boolean>(false);
  resetLoading = signal<boolean>(false);

  resetModel = signal({
    email: '',
    phoneNumber: '',
    code: '',
    newPassword: '',
    confirmPassword: ''
  });

  // ==================== TAB DEĞİŞİMİ ====================
  switchMethod(method: 'email' | 'sms'): void {
    if (this.activeMethod() === method) return;

    this.activeMethod.set(method);
    // Modeli temizle
    // 2. Model Signal'ini immutable şekilde sıfırla
    this.resetModel.update(current => ({
      ...current,
      email: '',
      phoneNumber: ''
    }));
  }

  // ==================== ADIM 1: KOD GÖNDER ====================
  sendResetCode(): void {
    const method = this.activeMethod();
    this.sendLoading.set(true);

    if (method === 'email') {
      if (!this.resetModel().email) {
        this.sendLoading.set(false);
        return;
      }

      this.customerAuthService.forgotPasswordByEmail(this.resetModel().email)
        .pipe(finalize(() => this.sendLoading.set(false)))
        .subscribe({
          next: (res: any) => {
            const message = res?.data || 'Sıfırlama kodu e-posta adresinize gönderildi.';
            this.messageService.add({
              severity: 'success',
              summary: 'Başarılı',
              detail: message
            });
            this.currentStep.set(2);
          },
          error: (err: any) => {
            console.log(err);
            this.messageService.add({
              
              severity: 'error',
              summary: 'Hata',
              detail: err?.error?.error[0]
                || err?.error?.message
                || err?.error?.detail
                || 'Kod gönderilirken bir hata oluştu.'
            });
          }
        });
    }

    else if (method === 'sms') {
      if (!this.resetModel().phoneNumber) {
        this.sendLoading.set(false);
        return;
      }

      const cleanPhone = '+' + this.resetModel().phoneNumber.replace(/\D/g, '');

      this.customerAuthService.forgotPasswordBySms(cleanPhone)
        .pipe(finalize(() => this.sendLoading.set(false)))
        .subscribe({
          next: (res: any) => {
            const message = res?.data || 'Şifre sıfırlama kodu telefonunuza SMS olarak gönderildi.';
            this.messageService.add({
              severity: 'success',
              summary: 'Başarılı',
              detail: message
            });
            this.currentStep.set(2);
          },
          error: (err: any) => {
            this.messageService.add({
              severity: 'error',
              summary: 'Hata',
              detail: err?.error?.errorMessages?.[0]
                || err?.error?.message
                || err?.error?.detail
                || 'SMS gönderilemedi, numarayı kontrol edin.'
            });
          }
        });
    }
  }


  // ==================== ADIM 2: ŞİFREYİ SIFIRLA ====================
  completePasswordReset(): void {
    const method = this.activeMethod();

    // Şifre eşleşme kontrolü
    if (this.resetModel().newPassword !== this.resetModel().confirmPassword) {
      this.messageService.add({
        severity: 'error',
        summary: 'Şifre Hatası',
        detail: 'Şifreler birbiriyle eşleşmiyor.'
      });
      return;
    }

    this.resetLoading.set(true);
    const cleanCode = this.resetModel().code.replace(/-/g, '').trim();

    if (method === 'email') {
      if (!this.resetModel().code || !this.resetModel().newPassword) {
        this.resetLoading.set(false);
        return;
      }

      const payload = {
        email: this.resetModel().email,
        code: cleanCode,
        newPassword: this.resetModel().newPassword
      };

      this.customerAuthService.resetPasswordByEmail(payload)
        .pipe(finalize(() => this.resetLoading.set(false)))
        .subscribe({
          next: (res: any) => {
            const message = res?.data || 'Şifreniz başarıyla sıfırlandı! Giriş yapabilirsiniz.';
            this.messageService.add({
              severity: 'success',
              summary: 'Başarılı',
              detail: message,
              life: 5000
            });
            this.router.navigate(['/customer-auth/customer-login']);
          },
          error: (err: any) => {
            this.messageService.add({
              severity: 'error',
              summary: 'Hata',
              detail: err?.error?.errorMessages?.[0]
                || err?.error?.message
                || err?.error?.detail
                || 'Şifre güncellenemedi, kodu kontrol edin.'
            });
          }
        });
    }

    else if (method === 'sms') {
      const cleanPhone = '+' + this.resetModel().phoneNumber.replace(/\D/g, '');

      const payload = {
        phoneNumber: cleanPhone,
        code: cleanCode,
        newPassword: this.resetModel().newPassword
      };

      this.customerAuthService.resetPasswordBySms(payload)
        .pipe(finalize(() => this.resetLoading.set(false)))
        .subscribe({
          next: (res: any) => {
            const message = res?.data || 'Şifreniz SMS doğrulamasıyla başarıyla güncellendi!';
            this.messageService.add({
              severity: 'success',
              summary: 'Başarılı',
              detail: message,
              life: 5000
            });
            this.router.navigate(['/customer-auth/customer-login']);
          },
          error: (err: any) => {
            this.messageService.add({
              severity: 'error',
              summary: 'Hata',
              detail: err?.error?.errorMessages?.[0]
                || err?.error?.message
                || err?.error?.detail
                || 'Kod hatalı veya süresi dolmuş.'
            });
          }
        });
    }
  }

  // ==================== ADIM GERİ ====================
  goBackToStep1(): void {
    this.currentStep.set(1);
    this.resetModel().code = '';
    this.resetModel().newPassword = '';
    this.resetModel().confirmPassword = '';
  }


  // ==================== RESET BUTTON DURUMU ====================
  get isResetButtonDisabled(): boolean {
    const code = this.resetModel().code?.trim();
    const pass = this.resetModel().newPassword?.trim();
    const confirm = this.resetModel().confirmPassword?.trim();

    return !code || !pass || !confirm || pass !== confirm;
  }

  // ==================== GİRİŞE DÖN ====================
  goToLogin(): void {
    this.router.navigate(['/customer-auth/customer-login']);
  }





}
