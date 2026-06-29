import { NgClass } from '@angular/common';
import { ChangeDetectionStrategy, Component, inject, input, output, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { PhoneFormatPipe } from '../../../../shared/pipes/phone-format.pipe';
import { CodeFormatPipe } from '../../../../shared/pipes/code-format.pipe';
import { AuthService } from '../../../../core/services/auth.service';
import { SnackbarService } from '../../../../core/services/snackbar.service';
import { PasswordValidationDirective } from '../../../../shared/directives/password-validation.directive';
import { TogglePasswordDirective } from "../../../../shared/directives/toggle-password.directive";
import { ConfirmPasswordValidationDirective } from '../../../../shared/directives/confirm-password-validation.directive';
import { AutofocusDirective } from '../../../../shared/directives/autofocus.directive';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { finalize } from 'rxjs';

@Component({
  selector: 'app-auth-password-reset',
  imports: [
    FormsModule,
    NgClass,
    //Pipe
    PhoneFormatPipe,
    CodeFormatPipe,
    //Directive
    PasswordValidationDirective,
    TogglePasswordDirective,
    ConfirmPasswordValidationDirective,
    AutofocusDirective,
    MatProgressSpinnerModule
],
  templateUrl: './auth-password-reset.component.html',
  styleUrl: './auth-password-reset.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class AuthPasswordResetComponent {
  readonly authService = inject(AuthService);
  readonly snackbarService = inject(SnackbarService);

  // 🔥 Üst komponentten (Login) gelen sıfırlama türünü zorunlu bir Signal Input olarak alıyoruz
  activeMethod = input.required<'email' | 'sms' | null>();
  // 🔥 İşlem bittiğinde veya iptal edildiğinde üst komponente haber uçuran modern Output
  onClose = output<void>();
  currentResetStep = signal<number>(1);
  // // Şifrenin görünürlük durumunu tutan boolean bayrağımız (Varsayılan olarak gizli)
  isPasswordVisible: boolean = false;
  sendLoading = signal<boolean>(false);
  resetLoading = signal<boolean>(false);

  // // Form verilerini temiz bir nesne olarak saklıyoruz
  resetModel = {
    email: '',
    phoneNumber: '',
    code: '',
    newPassword: '',
    confirmPassword: ''
  };


  // // 1. AŞAMA: E-posta adresine kod fırlatma lojiği
  sendResetCode(): void {
    const method = this.activeMethod();

    // 🚀 Her şey yolunda, spinner'ı ateşliyoruz!
    this.sendLoading.set(true);

    if (method === 'email') {
      if (!this.resetModel.email) return;

      // // Backend'deki 'api/auth/forgot-password' endpoint'ini tetikliyoruz
      this.authService.forgotPassword(this.resetModel.email).pipe(finalize(() => this.sendLoading.set(false))).subscribe({
        next: (res: any) => {
          // Backend'den dönen kurumsal başarı mesajını basıyoruz
          this.snackbarService.success(res.message || 'Sıfırlama kodu başarıyla gönderildi!');
          this.currentResetStep.set(2); // // Kullanıcıyı şak diye kod ve şifre girme alanına geçiyoruz!      
        },
        error: (err: any) => {
          this.snackbarService.error(err.error?.error?.[0] || 'Kod gönderilirken bir hata oluştu.');
        }
      });
    }

    else if (method === 'sms') {
      if (!this.resetModel.phoneNumber) return;

      const cleanPhone = '+' + this.resetModel.phoneNumber.replace(/\D/g, '');

      // // Backend'deki o az önce Twilio bağladığımız endpoint'i tetikliyoruz!
      // // AuthService'de smsSendResetCode metodunun tanımlı olduğunu varsayıyoruz
      this.authService.smsSendResetCode(cleanPhone).pipe(finalize(() => this.sendLoading.set(false))).subscribe({
        next: (res: any) => {
          this.snackbarService.success(res.message || 'Şifre sıfırlama kodu telefonunuza SMS olarak gönderildi!');
          this.currentResetStep.set(2); // Şak diye 2. aşamaya geçiyoruz
        },
        error: (err: any) => {
          this.snackbarService.error(err.error?.errorMessage?.[0] || 'SMS gönderilemedi, numarayı kontrol et.');
        }
      });
    }

  }

  // // 2. AŞAMA: Gelen kod ve yeni şifreyle sıfırlamayı bitirme lojiği
  completePasswordReset(): void {
    const method = this.activeMethod();
    this.resetLoading.set(true);

    if (method === 'email') {
      // // 🔥 INPUT'TAN GELEN TİRELİ KODU BURADA KILÇIKSIZ HALE GETİRİYORUZ!
      const cleanCode = this.resetModel.code.replace(/-/g, '').trim();

      const payload = {
        email: this.resetModel.email,
        code: cleanCode,
        newPassword: this.resetModel.newPassword
      };

      if (!this.resetModel.code || !this.resetModel.newPassword) return;

      // // Backend'deki 'api/auth/reset-password' endpoint'ini tetikliyoruz
      this.authService.resetPassword(payload).pipe(finalize(() => this.resetLoading.set(false))).subscribe({
        next: (res: any) => {
          this.snackbarService.success(res.message || 'Şifreniz başarıyla sıfırlandı! Giriş yapabilirsiniz.');
          this.closeModal(); // // İşlem başarılıysa modal'ı kapatıyoruz
        },
        error: (err: any) => {
          this.snackbarService.error(err.error?.error?.[0] || 'Şifre güncellenemedi, kodu kontrol edin.');
        }
      });

    } else if (method === 'sms') {
      // // Hatırlarsan backend'de SmsResetPasswordCommand nesnemiz tam olarak bu 3 veriyi bekliyordu!
      const cleanPhone = '+' + this.resetModel.phoneNumber.replace(/\D/g, '');
      // // 🔥 INPUT'TAN GELEN TİRELİ KODU BURADA KILÇIKSIZ HALE GETİRİYORUZ!
      const cleanCode = this.resetModel.code.replace(/-/g, '').trim();

      const payload = {
        phoneNumber: cleanPhone,
        code: cleanCode,
        newPassword: this.resetModel.newPassword
      };

      // // AuthService'de smsResetPassword metodunun tanımlı olduğunu varsayıyoruz
      this.authService.smsResetPassword(payload).pipe(finalize(() => this.resetLoading.set(false))).subscribe({
        next: (res: any) => {
          this.snackbarService.success(res.message || 'Şifreniz SMS doğrulamasıyla başarıyla güncellendi!');
          this.closeModal();
        },
        error: (err: any) => {
          this.snackbarService.error(err.error?.errorMessage?.[0] || 'Kod hatalı veya süresi dolmuş.');
        }
      });
    }
  }

  togglePasswordVisibility(): void {
    this.isPasswordVisible = !this.isPasswordVisible;
  }

  closeModal(): void {
    // Kendi iç modelini sıfırla
    this.resetModel = { email: '', phoneNumber: '', code: '', newPassword: '', confirmPassword: '' };
    this.currentResetStep.set(1);
    // Üst komponente modal kapandı bilgisini fırlat
    this.onClose.emit();
  }

  get isResetButtonDisabled(): boolean {
    const code = this.resetModel.code?.trim();
    const pass = this.resetModel.newPassword?.trim();
    const confirm = this.resetModel.confirmPassword?.trim();

    // Alanlardan biri bile boşsa veya şifreler uyuşmuyorsa kilitle (true dön)
    return !code || !pass || !confirm || pass !== confirm;
  }
}
