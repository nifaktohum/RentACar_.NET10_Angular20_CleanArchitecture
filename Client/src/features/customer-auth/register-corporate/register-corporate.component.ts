import { ChangeDetectionStrategy, Component, inject, signal, ViewEncapsulation } from '@angular/core';
import { Router, RouterLink } from '@angular/router';
import { MessageService } from 'primeng/api';
import { CustomerAuthService } from '../../../core/services/customers/customer-auth.service';
import { finalize } from 'rxjs';
import { Result } from '../../../core/models/result.model';
import { FormsModule, NgForm } from '@angular/forms';
import { getCities, getDistrictsByCityCode } from 'turkey-neighbourhoods';
import { CheckboxModule } from 'primeng/checkbox';
import { SelectModule } from 'primeng/select';
import { PasswordValidationDirective } from '../../../shared/directives/password-validation.directive';
import { TogglePasswordDirective } from '../../../shared/directives/toggle-password.directive';
import { EmailValidationDirective } from '../../../shared/directives/email-validatio.directive';
import { PhoneFormatPipe } from '../../../shared/pipes/phone-format.pipe';
import { ConfirmPasswordValidationDirective } from '../../../shared/directives/confirm-password-validation.directive';

@Component({
  selector: 'app-register-corporate',
  imports: [
    CheckboxModule,
    FormsModule,
    RouterLink,
    SelectModule,
    PasswordValidationDirective,
    TogglePasswordDirective,
    EmailValidationDirective,
    PhoneFormatPipe,
    ConfirmPasswordValidationDirective,


  ],
  templateUrl: './register-corporate.component.html',
  styleUrl: './register-corporate.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
  encapsulation: ViewEncapsulation.Emulated
})
export class RegisterCorporateComponent {
  private customerAuthService = inject(CustomerAuthService);
  private router = inject(Router);
  private messageService = inject(MessageService);

  // ==================== İL / İLÇE STATE ====================
  cities: { code: string; name: string }[] = [];
  districts: { name: string }[] = [];
  selectedCityCode: string | null = null;
  selectedDistrict: string | null = null;

  // ==================== FORM STATE ====================
  formData = signal({
    // Şirket
    companyName: '',
    taxNumber: '',
    taxOffice: '',

    // İletişim
    email: '',
    phoneNumber: '',

    // Şifre
    password: '',
    confirmPassword: '',

    // Adres
    address: '',
    city: '',
    country: 'Türkiye',
    postalCode: ''
  });

  // Signal field güncelleme
  updateField<K extends keyof ReturnType<typeof this.formData>>(
    field: K,
    value: ReturnType<typeof this.formData>[K]
  ): void {
    this.formData.update(prev => ({ ...prev, [field]: value }));
  }

  // ==================== UI STATE ====================
  loading = signal<boolean>(false);
  termsAccepted = signal<boolean>(false);

  // ==================== LIFECYCLE ====================
  ngOnInit(): void {
    this.cities = getCities();
  }

  // ==================== SUBMIT ====================
  onSubmit(form: NgForm): void {
    form.control.markAllAsTouched();

    // 1. Form validasyonu
    if (form.invalid) {
      this.messageService.add({
        severity: 'warn',
        summary: 'Uyarı',
        detail: 'Lütfen tüm zorunlu alanları eksiksiz ve doğru doldurun.'
      });

      const firstInvalid = document.querySelector('.ng-invalid') as HTMLElement;
      firstInvalid?.scrollIntoView({ behavior: 'smooth', block: 'center' });
      firstInvalid?.focus();
      return;
    }

    // 2. Terms kontrolü
    if (!this.termsAccepted()) {
      this.messageService.add({
        severity: 'warn',
        summary: 'Uyarı',
        detail: 'Devam etmek için kullanım koşullarını kabul etmelisiniz.'
      });
      return;
    }

    // 3. Şifre eşleşme kontrolü
    if (this.formData().password !== this.formData().confirmPassword) {
      this.messageService.add({
        severity: 'error',
        summary: 'Şifre Hatası',
        detail: 'Şifreler birbiriyle eşleşmiyor.'
      });
      return;
    }

    this.loading.set(true);

    // 4. Payload
    const payload = {
      companyName: this.formData().companyName.trim(),
      taxNumber: this.formData().taxNumber.trim(),
      taxOffice: this.formData().taxOffice.trim(),
      email: this.formData().email.trim(),
      phoneNumber: this.formData().phoneNumber?.trim() ?? '',
      password: this.formData().password,
      address: this.formData().address.trim(),
      city: this.formData().city.trim(),
      country: this.formData().country.trim(),
      postalCode: this.formData().postalCode.trim() || null
    };

    // 5. API çağrısı
    this.customerAuthService.registerCorporate(payload)
      .pipe(finalize(() => this.loading.set(false)))
      .subscribe({
        next: (res: Result<string>) => {
          if (res.isSuccessful) {
            this.messageService.add({
              severity: 'success',
              summary: 'Kayıt Başarılı',
              detail: 'Kurumsal hesabınız oluşturuldu. Şimdi giriş yapabilirsiniz.',
              life: 5000
            });
            this.router.navigate(['/customer-auth/customer-login']);
          } else {
            this.messageService.add({
              severity: 'warn',
              summary: 'Uyarı',
              detail: res.errorMessages?.[0] || 'Kayıt yapılamadı.'
            });
          }
        },
        error: (err) => {
          this.messageService.add({
            severity: 'error',
            summary: 'Hata',
            detail: err?.error?.errorMessages?.[0]
              || err?.error?.message
              || err?.error?.detail
              || 'Sunucu hatası, lütfen tekrar deneyin.'
          });
          console.log(err);
        }
      });
  }


  // ==================== TELEFON FORMATLAMA ====================
  onPhoneChange(value: string): void {
    if (!value) {
      this.updateField('phoneNumber', '');
      return;
    }

    let cleaned = value.replace(/\D/g, '');

    if (cleaned.startsWith('90')) {
      cleaned = cleaned.substring(2);
    } else if (cleaned.startsWith('0')) {
      cleaned = cleaned.substring(1);
    }

    cleaned = cleaned.substring(0, 10);

    const finalFormattedNumber = cleaned.length > 0 ? `+90${cleaned}` : '';
    this.updateField('phoneNumber', finalFormattedNumber);
  }


  onlyNumbers(event: KeyboardEvent): boolean {
    const charCode = event.which ? event.which : event.keyCode;
    if (charCode > 31 && (charCode < 48 || charCode > 57)) {
      return false;
    }
    return true;
  }

  // ==================== İL / İLÇE ====================
  onCityChange(cityCode: string | null): void {
    if (cityCode) {
      this.districts = getDistrictsByCityCode(cityCode).map(d => ({ name: d }));
    } else {
      this.districts = [];
    }
    this.selectedDistrict = null;
    this.updateCityField();
  }

  onDistrictChange(_district: string | null): void {
    this.updateCityField();
  }

  private updateCityField(): void {
    const city = this.cities.find(c => c.code === this.selectedCityCode);

    let cityValue = '';
    if (city && this.selectedDistrict) {
      cityValue = `${city.name}/${this.selectedDistrict}`;
    } else if (city) {
      cityValue = city.name;
    }

    this.updateField('city', cityValue);
  }

  // ==================== HELPERS ====================
  goToLogin(): void {
    this.router.navigate(['/customer-auth/customer-login']);
  }











}
