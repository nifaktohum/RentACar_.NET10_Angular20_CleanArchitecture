import { ChangeDetectionStrategy, Component, inject, OnInit, signal } from '@angular/core';
import { FormsModule, NgForm } from '@angular/forms';
import { AuthService } from '../../../core/services/auth.service';
import { ActivatedRoute, Router } from '@angular/router';
import { AuthPasswordResetComponent } from '../components/auth-password-reset/auth-password-reset.component';
import { TogglePasswordDirective } from '../../../shared/directives/toggle-password.directive';
import { EmailValidationDirective } from '../../../shared/directives/email-validatio.directive';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { finalize } from 'rxjs';
import { MessageService } from 'primeng/api';
import { LoginResponse } from '../../../core/models/login.response';
import { Result } from '../../../core/models/result.model';


@Component({
  selector: 'app-login',
  imports: [
    AuthPasswordResetComponent,
    FormsModule,
    //Directive
    TogglePasswordDirective,
    EmailValidationDirective,
    MatProgressSpinnerModule,
  ],
  templateUrl: './login.component.html',
  styleUrl: './login.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class LoginComponent implements OnInit {
  private authService = inject(AuthService)
  private router = inject(Router)
  private route = inject(ActivatedRoute)
  private messageServiceToast = inject(MessageService);
  //=======================================>

  loginEmail: string = '';
  isRememberMeChecked: boolean = false;
  activeResetMethod = signal<'email' | 'sms' | null>(null);
  loading = signal<boolean>(false);
  // // Şifrenin görünürlük durumunu tutan boolean bayrağımız (Varsayılan olarak gizli)

  returnUrl: string = '/';


  ngOnInit(): void {
    // // 1. MANTIKSAL BAŞLANGIÇ: Sayfa yüklenirken localStorage'a bakıyoruz
    const savedEmail = localStorage.getItem('rememberedEmail');
    if (savedEmail) {
      // // Eğer önceden kaydedilmiş e-posta varsa input'a doldur ve checkbox'ı işaretle
      this.loginEmail = savedEmail;
      this.isRememberMeChecked = true;
    }

    // // Eğer url'de bir yönlendirme adresi varsa onu 'returnUrl' değişkenine atıyoruz, yoksa varsayılan '/' (ana sayfa) kalıyor.
    const url = this.route.snapshot.queryParams['returnUrl'] || '/';
    if (url) this.returnUrl = url;
  }

  login(form: NgForm) {
    // // 1. 🔥 Kullanıcı direkt butona basarsa, tüm alanları 'touched' yapıyoruz ki 
    // // yukarıda yazdığımız `validate()` metotları anında çalışıp hataları ekrana bassın!
    form.control.markAllAsTouched();
    form.control.updateValueAndValidity(); // Formu güncel durumunu kontrol etmeye zorla

    // // 🔥 İŞTE SAVUNMA DUVARI: Form geçersizse backend yolculuğunu burada bitir!
    if (form.invalid) {
      // Sayfadaki ilk hatalı (.ng-invalid sınıfına sahip) input elementini cımbızla çekiyoruz
      const firstInvalidInput = document.querySelector('.form-control.ng-invalid, input.ng-invalid') as HTMLElement;

      if (firstInvalidInput) {
        // Sayfayı o hatalı inputun olduğu yere yumuşacık kaydırıyoruz
        firstInvalidInput.scrollIntoView({ behavior: 'smooth', block: 'center' });

        // İmleci doğrudan o inputun içine odaklıyoruz ki kullanıcı hatasını anında görebilsin
        firstInvalidInput.focus();
      }

      return; // 👈 Burası kritik! Alttaki kodların çalışmasını ve backend'e istek gitmesini engeller.
    }
    // 🚀 Form geçerli! Spinner'ı ateşliyoruz
    this.loading.set(true);
    // // 2. Form içindeki değerlerden backend'in beklediği LoginCommand veri paketini (body) hazırlıyoruz
    const credentials = {
      email: form.value.email, // 🔥 buraya dikkat: HTML input ismin neyse form.value altındaki isim de odur (email ise email yap)
      password: form.value.password,
      rememberMe: this.isRememberMeChecked // // Backend bu bilgiye göre JWT süresini uzatabilir 
    };


    this.authService.login(credentials).pipe(finalize(() => this.loading.set(false))).subscribe({
      next: (res: Result<LoginResponse>) => {
        if (res.isSuccessful && res.data) {
          // Servis metodu çağrıldı, komponent rahatladı
          this.authService.saveUserSession(res.data, this.isRememberMeChecked);

          this.messageServiceToast.add({
            severity: 'success',
            summary: 'Giriş Başarılı :)',
            detail: `Hoş geldiniz - ${res.data.fullName}!`
          });
          this.router.navigateByUrl(this.returnUrl || '/');
          return;
        }
        this.messageServiceToast.add({
          severity: 'warn',
          summary: 'Uyarı',
          detail: `${res.errorMessages}` || 'Sunucu hatası, lütfen tekrar deneyin.'
        })
        // // Sunucudan nesne geldi ama içinden token çıkmadıysa (Güvenlik önlemi)
        // this.snackbarService.warning('Sunucudan geçersiz veri döndü, lütfen tekrar deneyin.');

      }, error: (err) => {

        this.messageServiceToast.add({
          severity: 'error',
          summary: 'Hata',
          detail: `${err.error.error[0]}` || 'Sunucu hatası, lütfen tekrar deneyin.'
        })
        console.log(err.error.error[0]);
      }
    });
  }

  closeResetModal(): void {
    this.activeResetMethod.set(null); // Alt komponent onClose sinyali verince modalı kapatır
  }



  // // Modalları tetikleyen yeni temiz metotlarımız
  openEmailResetModal(): void {
    this.closeResetModal();
    this.activeResetMethod.set('email');
  }

  openSmsResetModal() {
    this.closeResetModal();
    this.activeResetMethod.set('sms');
  }
}
