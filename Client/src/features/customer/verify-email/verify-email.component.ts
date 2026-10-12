import { ChangeDetectionStrategy, Component, computed, inject, signal, ViewEncapsulation } from '@angular/core';
import { Router, RouterLink } from '@angular/router';
import { MessageService } from 'primeng/api';
import { CustomerAuthService } from '../../../core/services/customers/customer-auth.service';
import { finalize } from 'rxjs';
import { Result } from '../../../core/models/result.model';
import { FormsModule, NgForm } from '@angular/forms';
import { CodeFormatPipe } from '../../../shared/pipes/code-format.pipe';
import { VerifyState } from '../../../shared/enums/verify-state';
import { ButtonModule } from 'primeng/button';
import { TooltipModule } from 'primeng/tooltip';


@Component({
  selector: 'app-verify-email',
  imports: [
    FormsModule,
    CodeFormatPipe,
    ButtonModule,
    TooltipModule,
    RouterLink
  ],
  templateUrl: './verify-email.component.html',
  styleUrl: './verify-email.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
  encapsulation: ViewEncapsulation.Emulated
})
export class VerifyEmailComponent {
  private customerAuth = inject(CustomerAuthService);
  private router = inject(Router);
  private messageService = inject(MessageService);

  // ==================== STATE ====================
  state = signal<VerifyState>(VerifyState.Pending);
  code = signal<string>('');
  loading = signal<boolean>(false);
  resending = signal<boolean>(false);

  // Geri sayım
  // Bekleme süresinin (cooldown) ekranda anlık gösterilmesi için reaktif sayaç (saniye cinsinden)
  cooldownSeconds = signal<number>(0);
  // Arka planda saniyede bir çalışan zamanlayıcının (timer) referansı (durdurmak/temizlemek için kullanılır)
  private cooldownInterval?: ReturnType<typeof setInterval>;

  // ==================== COMPUTED ====================

  // Müşteri oturum açmamışsa veya e-posta bilgisi yoksa varsayılan olarak boş string ('') döner.
  customerEmail = computed(() => {
    return this.customerAuth.currentCustomer()?.['Email'] || '';
  });

  // Müşterinin e-posta doğrulama durumunu reaktif olarak takip eden salt okunur (read-only) Signal.
  // customerAuth servisindeki 'isEmailVerified' Signal'i her değiştiğinde bu değer de otomatik güncellenir.
  isEmailVerified = computed(() => this.customerAuth.isEmailVerified());

  // Bekleme süresini (saniye) reaktif olarak 'dakika:saniye' (ör. 02:05) formatına dönüştüren Computed Signal
  formattedCooldown = computed(() => {
    const total = this.cooldownSeconds();                     // Toplam saniye değerini okur
    const minutes = Math.floor(total / 60);                   // Tam dakika kısmını hesaplar
    const seconds = total % 60;                               // Dakikadan artan kalan saniyeyi hesaplar

    // 'padStart(2, '0')' ile sayı tek haneliyse başına '0' ekler (ör: 5 -> '05')
    return `${String(minutes).padStart(2, '0')}:${String(seconds).padStart(2, '0')}`;
  });

  // 'Tekrar Kod Gönder' butonunun aktif (tıklanabilir) olup olmadığını belirler.
  // Yalnızca bekleme süresi bittiğinde (0 saniye) VE aktif bir istek atılmıyorken 'true' döner.
  canResend = computed(() => this.cooldownSeconds() === 0 && !this.resending());

  canVerify = computed(() => {
    const cleaned = this.code().replace(/-/g, '').trim();
    return cleaned.length === 6 && !this.loading();
  });

  // ==================== LIFECYCLE ====================
  ngOnInit(): void {
    // Zaten doğrulanmışsa direkt verified state'e geç
    if (this.isEmailVerified()) {
      this.state.set(VerifyState.Verified);
    }
  }

  // * Kullanıcı sayfadan/bileşenden ayrıldığında arka planda çalışan zamanlayıcının açık kalıp belleği şişirmesini engellemek için:
  ngOnDestroy(): void {
    this.stopCooldown();
  }

  // ==================== KOD GÖNDER ====================
  sendCode(): void {
    if (this.loading()) return;

    this.loading.set(true);

    this.customerAuth.sendEmailVerificationCode()
      .pipe(finalize(() => this.loading.set(false)))
      .subscribe({
        next: (res: Result<string>) => {
          if (res.isSuccessful) {
            this.messageService.add({
              severity: 'success',
              summary: 'Kod Gönderildi',
              detail: `${this.customerEmail()} adresine 6 haneli kod gönderildi.`,
              life: 5000
            });
            this.state.set(VerifyState.CodeSent);
            this.startCooldown(60);    // 60 saniye
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
    this.code.set('');    // Eski kodu temizle

    this.customerAuth.sendEmailVerificationCode()
      .pipe(finalize(() => this.resending.set(false)))
      .subscribe({
        next: (res: Result<string>) => {
          if (res.isSuccessful) {
            this.messageService.add({
              severity: 'success',
              summary: 'Kod Yeniden Gönderildi',
              detail: 'Yeni kod e-posta adresinize gönderildi.',
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
  // * Kullanıcı tarafından girilen e-posta doğrulama kodunu (OTP) kontrol eder ve doğrulama isteğini başlatır.
  // verifyCode(form: NgForm): void {
  //   // 1. Form üzerindeki tüm input'ları "dokunuldu" (touched) işaretleyerek doğrulama hatalarını UI'da görünür kılar
  //   form.control.markAllAsTouched();
  //   // 2. Kod içerisindeki olası tire (-) karakterlerini temizler ve sağ/sol boşlukları kırpar (ör: "123-456" -> "123456")
  //   const cleanCode = this.code().replace(/-/g, '').trim();

  //   if (cleanCode.length !== 6) {
  //     this.messageService.add({
  //       severity: 'warn',
  //       summary: 'Uyarı',
  //       detail: 'Lütfen 6 haneli doğrulama kodunu girin.'
  //     });
  //     return;
  //   }

  //   this.loading.set(true);

  //   this.customerAuth.verifyEmail(cleanCode).subscribe({
  //     next: (res) => {
  //       if (res.isSuccessful) {
  //         // Doğrulama sonrası token'ı yenileyip signal'i güncelliyoruz
  //         this.customerAuth.refreshToken()?.subscribe(() => {
  //           this.state.set(VerifyState.Verified);
  //           this.stopCooldown();

  //                   this.messageService.add({
  //                     severity: 'success',
  //                     summary: 'Başarılı',
  //                     detail: 'E-posta adresiniz başarıyla doğrulandı!',
  //                     life: 5000
  //                   });
            
  //                 // 2 saniye sonra profile yönlendir
  //           setTimeout(() => {
  //             this.router.navigate(['/customer/profile']);
              
  //           }, 2000);

  //         });
  //       }
  //     }
  //   });

  //   // this.customerAuth.verifyEmail(cleanCode)
  //   //   .pipe(
  //   //   // İstek başarılı veya hatalı tamamlansa da yükleniyor durumunu kapatır
  //   //     finalize(() => this.loading.set(false)))
  //   //   .subscribe({
  //   //     next: (res: Result<string>) => {
  //   //       if (res.isSuccessful) {
  //   //         this.state.set(VerifyState.Verified);
  //   //         this.stopCooldown();
            
  //   //         this.messageService.add({
  //   //           severity: 'success',
  //   //           summary: 'Başarılı',
  //   //           detail: 'E-posta adresiniz başarıyla doğrulandı!',
  //   //           life: 5000
  //   //         });

  //   //         // 2 saniye sonra profile yönlendir
  //   //         setTimeout(() => {
  //   //           this.router.navigate(['/customer/profile']);
              
  //   //         }, 2000);

  //   //       } else {
  //   //         this.messageService.add({
  //   //           severity: 'warn',
  //   //           summary: 'Uyarı',
  //   //           detail: res.errorMessages?.[0] || 'Kod doğrulanamadı.'
  //   //         });
  //   //       }
  //   //     },
  //   //     error: (err) => {
  //   //       this.messageService.add({
  //   //         severity: 'error',
  //   //         summary: 'Hata',
  //   //         detail: err?.error?.errorMessages?.[0]
  //   //           || err?.error?.message
  //   //           || 'Kod hatalı veya süresi dolmuş.'
  //   //       });
  //   //     }
  //   //   });
  // }

  verifyCode(form: NgForm): void {
    // * Formdaki tüm alanları "dokunulmuş" olarak işaretler
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

    this.customerAuth.verifyEmail(cleanCode)
      .pipe(finalize(() => this.loading.set(false)))    // ⭐ finalize
      .subscribe({
        next: () => {
          this.state.set(VerifyState.Verified);
          this.stopCooldown();

          this.messageService.add({
            severity: 'success',
            summary: 'Başarılı',
            detail: 'E-posta adresiniz başarıyla doğrulandı!',
            life: 5000
          });

          setTimeout(() => {
            this.router.navigate(['/customer/profile']);
          }, 2000);
        },
        error: (err) => {
          // verify VEYA refresh hatası buraya düşer
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

  // * zamanlayıcıyı durdurmaya ve temizlemeye yarar.
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
