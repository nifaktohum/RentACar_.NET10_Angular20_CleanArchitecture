import { ChangeDetectionStrategy, Component, inject, OnInit, signal, ViewEncapsulation } from '@angular/core';
import { CustomerAuthService } from '../../../core/services/customers/customer-auth.service';
import { Router, ActivatedRoute, RouterLink } from '@angular/router';
import { MessageService } from 'primeng/api';
import { CustomerLoginResponse } from '../../../core/models/customer-auth.model';
import { Result } from '../../../core/models/result.model';
import { finalize } from 'rxjs';
import { FormsModule, NgForm } from '@angular/forms';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { TogglePasswordDirective } from '../../../shared/directives/toggle-password.directive';
import { EmailValidationDirective } from '../../../shared/directives/email-validatio.directive';

@Component({
  selector: 'app-login',
  imports: [
    MatProgressSpinnerModule,
    FormsModule,
    RouterLink,
    TogglePasswordDirective,
    EmailValidationDirective
  ],
  templateUrl: './login.component.html',
  styleUrl: './login.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
  encapsulation: ViewEncapsulation.Emulated
})
export class LoginComponent implements OnInit{
  private customerAuthService = inject(CustomerAuthService);
  private router = inject(Router);
  private route = inject(ActivatedRoute);
  private messageService = inject(MessageService);

  // ==================== STATE ====================
  loginEmail: string = '';
  isRememberMeChecked: boolean = false;
  loading = signal<boolean>(false);

  returnUrl: string = '/';



  ngOnInit(): void {
    // Remember me — kayıtlı email
    const savedEmail = localStorage.getItem('customer_rememberedEmail');
    if (savedEmail) {
      this.loginEmail = savedEmail;
      this.isRememberMeChecked = true;
    }

    // returnUrl
    const url = this.route.snapshot.queryParams['returnUrl'];
    if (url) this.returnUrl = url;
  }

  // ==================== LOGIN ====================
  login(form: NgForm): void {
    form.control.markAllAsTouched();
    form.control.updateValueAndValidity();

    if (form.invalid) {
      const firstInvalidInput = document.querySelector(
        '.form-control.ng-invalid, input.ng-invalid'
      ) as HTMLElement;

      if (firstInvalidInput) {
        firstInvalidInput.scrollIntoView({ behavior: 'smooth', block: 'center' });
        firstInvalidInput.focus();
      }
      return;
    }

    this.loading.set(true);

    const credentials = {
      email: form.value.email,
      password: form.value.password,
      rememberMe: this.isRememberMeChecked
    };

    this.customerAuthService.login(credentials)
      .pipe(finalize(() => this.loading.set(false)))
      .subscribe({
        next: (res: Result<CustomerLoginResponse>) => {
          if (!res.isSuccessful || !res.data) {
            this.messageService.add({
              severity: 'warn',
              summary: 'Uyarı',
              detail: res.errorMessages?.[0] || 'Giriş yapılamadı.'
            });
            return;
          }

          // Remember me kaydet
          if (this.isRememberMeChecked) {
            localStorage.setItem('customer_rememberedEmail', res.data.email);
          } else {
            localStorage.removeItem('customer_rememberedEmail');
          }

          // ⭐ KARA LİSTE UYARISI
          if (res.data.isBlacklisted) {
            this.messageService.add({
              severity: 'warn',
              summary: 'Hesap Uyarısı',
              detail: `Hesabınız kara listede: ${res.data.blacklistReason || 'Sebep belirtilmemiş'}`,
              life: 8000
            });
          }

          // Başarılı giriş
          this.messageService.add({
            severity: 'success',
            summary: 'Giriş Başarılı',
            detail: `Hoş geldiniz, ${res.data.fullName}!`
          });

          this.router.navigateByUrl(this.returnUrl);
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
}

