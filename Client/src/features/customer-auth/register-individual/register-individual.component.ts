import { ChangeDetectionStrategy, Component, inject, OnInit, signal, viewChild, ViewChild, ViewEncapsulation } from '@angular/core';
import { Router, RouterLink } from '@angular/router';
import { MessageService } from 'primeng/api';
import { CustomerAuthService } from '../../../core/services/customers/customer-auth.service';
import { FormsModule, NgForm } from '@angular/forms';
import { finalize } from 'rxjs';
import { Result } from '../../../core/models/result.model';

// PrimeNG Modülleri
import { DatePickerModule } from 'primeng/datepicker';
import { SelectModule } from 'primeng/select';
import { CheckboxModule } from 'primeng/checkbox';
import { InputMaskModule } from 'primeng/inputmask';

// Özel Pipe ve Direktifler
import { PhoneFormatPipe } from '../../../shared/pipes/phone-format.pipe';
import { TogglePasswordDirective } from '../../../shared/directives/toggle-password.directive';
import { ConfirmPasswordValidationDirective } from '../../../shared/directives/confirm-password-validation.directive';
import { PasswordValidationDirective } from '../../../shared/directives/password-validation.directive';
import { EmailValidationDirective } from '../../../shared/directives/email-validatio.directive';
import { PrimeDateMaskDirective } from '../../../shared/directives/prime-date-mask.directive';

// Türkiye İl/İlçe Veri Paketi
import { getCities, getDistrictsByCityCode } from 'turkey-neighbourhoods';
import { Step1Data, Step2Data } from '../../../core/models/form-step-data.model';




@Component({
  selector: 'app-register-individual',
  imports: [
    FormsModule,
    RouterLink,
    DatePickerModule,
    SelectModule,
    CheckboxModule,
    InputMaskModule,
    PhoneFormatPipe,
    ConfirmPasswordValidationDirective,
    TogglePasswordDirective,
    PasswordValidationDirective,
    EmailValidationDirective,
    PrimeDateMaskDirective,
    
  ],
  templateUrl: './register-individual.component.html',
  styleUrl: './register-individual.component.scss',
  // OnPush: Sadece Signal veya Input değiştiğinde change detection çalıştırır (Performans optimizasyonu)
  changeDetection: ChangeDetectionStrategy.OnPush,
  // Emulated: CSS stillerini sadece bu bileşene kapsüller
  encapsulation: ViewEncapsulation.Emulated
})
export class RegisterIndividualComponent implements OnInit {

  // ==================== SERVIS ENJEKSIYONLARI ====================
  // CustomerAuthService: Müşteri kayıt/giriş API isteklerini yönetir
  private customerAuthService = inject(CustomerAuthService);
  // Router: Sayfalar arası yönlendirme sağlar
  private router = inject(Router);
  // MessageService: PrimeNG Toast bildirimlerini göstermek için kullanılır
  private messageService = inject(MessageService);

  // Form valid takibi
  readonly step2Form = viewChild<NgForm>('step2Form');
  get isStep2Valid(): boolean {
    return !!this.step2Form()?.valid;
  }

  // ==================== İL / İLÇE STATE ====================
  // İl listesini nesne dizisi olarak tutar (Örn: [{code: "16", name: "Bursa"}])
  cities: { code: string; name: string }[] = [];
  // Seçilen ile ait ilçeleri tutar
  districts: { name: string }[] = [];
  // Seçili ilin plaka kodu ("16" gibi)
  selectedCityCode: string | null = null;
  // Seçili ilçenin ismi ("Nilüfer" gibi)
  selectedDistrict: string | null = null;

  // ==================== WIZARD / ADIM STATE ====================
  // Çok adımlı formun mevcut adım numarası (Signal)
  currentStep = signal<number>(1);
  // Toplam form adım sayısı
  totalSteps = 2;

  // ==================== FORM SIGNALS STATE ====================
  // 1. Adımdaki kişisel bilgileri tutan Signal
  step1Data = signal<Step1Data>({
    firstName: '',
    lastName: '',
    identityNumber: '',
    birthDate: null,
    gender: null
  });

  // 2. Adımdaki iletişim/hesap bilgilerini tutan Signal
  step2Data = signal<Step2Data>({
    email: '',
    phoneNumber: '',
    password: '',
    confirmPassword: '',
    address: '',
    city: '',
    country: 'Türkiye',
    postalCode: ''
  });

  // Signal içindeki spesifik tek bir alanı (property) güvenli şekilde güncelleme metodu
  updateStep1Field<K extends keyof Step1Data>(field: K, value: Step1Data[K]): void {
    this.step1Data.update(prev => ({ ...prev, [field]: value }));
  }

  // Signal içindeki spesifik tek bir alanı (property) güvenli şekilde güncelleme metodu
  updateStep2Field<K extends keyof Step2Data>(field: K, value: Step2Data[K]): void {
    this.step2Data.update(prev => ({ ...prev, [field]: value }));
  }

  // ==================== UI STATE SIGNALS ====================
  // API isteği sırasında loading/spinner gösterim durumu
  loading = signal<boolean>(false);
  // Şifre alanının görünürlük durumu
  showPassword = signal<boolean>(false);
  // Şifre tekrar alanının görünürlük durumu
  showConfirmPassword = signal<boolean>(false);
  // Kullanım koşulları onay tiki kontrolü
  termsAccepted = signal<boolean>(false);

  // ==================== FORM KONTROL DEĞERLERI ====================
  // Doğum günü için seçilebilecek maksimum tarih (Bugün)
  maxBirthDate = new Date();

  // Cinsiyet seçimi için Dropdown opsiyonları
  genderOptions = [
    { label: 'Erkek', value: 'Male' },
    { label: 'Kadın', value: 'Female' }
  ];

  // Component yüklendiğinde il listesini doldurur
  ngOnInit(): void {
    this.cities = getCities();
  }

  // ==================== TELEFON FORMATLAMA ====================
  // Telefon input'una yazıldıkça tetiklenir, ham girdiyi temizleyip '+903129876543' formatına dönüştürür
  onPhoneChange(value: string): void {
    if (!value) {
      this.updateStep2Field('phoneNumber', '');
      return;
    }

    // 1. Sadece rakamları ayıkla
    let cleaned = value.replace(/\D/g, '');

    // 2. Başındaki '90' veya '0'ı temizle
    if (cleaned.startsWith('90')) {
      cleaned = cleaned.substring(2);
    } else if (cleaned.startsWith('0')) {
      cleaned = cleaned.substring(1);
    }

    // 3. Maksimum 10 haneli ulusal numarayı al
    cleaned = cleaned.substring(0, 10);

    // 4. Başına +90 ekleyerek uluslararası formata getir
    const finalFormattedNumber = cleaned.length > 0 ? `+90${cleaned}` : '';

    // Signal'i güncelle
    this.updateStep2Field('phoneNumber', finalFormattedNumber);
  }

  // Sadece rakam girilmesini zorunlu kılan klavye dinleyici metodu
  onlyNumbers(event: KeyboardEvent): boolean {
    const charCode = event.which ? event.which : event.keyCode;
    if (charCode > 31 && (charCode < 48 || charCode > 57)) {
      return false;
    }
    return true;
  }

  // ==================== İL / İLÇE YÖNETİMİ ====================
  // İl seçimi değiştiğinde ilgili ilçeleri getirir
  onCityChange(cityCode: string | null) {
    if (cityCode) {
      this.districts = getDistrictsByCityCode(cityCode).map(d => ({ name: d }));
    } else {
      this.districts = [];
    }
    this.selectedDistrict = null;
    this.updateCityField();
  }

  // İlçe seçimi değiştiğinde metni günceller
  onDistrictChange(_district: string | null) {
    this.updateCityField();
  }

  // Şehir ve ilçeyi birleştirip step2Data().city alanına yazar (Örn: "Bursa/Nilüfer")
  private updateCityField() {
    const city = this.cities.find(c => c.code === this.selectedCityCode);

    let cityValue = '';
    if (city && this.selectedDistrict) {
      cityValue = `${city.name}/${this.selectedDistrict}`;
    } else if (city) {
      cityValue = city.name;
    }

    this.updateStep2Field('city', cityValue);
  }

  // ==================== ADIM YÖNETİMİ (WIZARD) ====================
  // Bir sonraki adıma geçiş mantığı
  nextStep(form: NgForm): void {
    form.control.markAllAsTouched();

    if (form.invalid) {
      this.messageService.add({
        severity: 'warn',
        summary: 'Uyarı',
        detail: 'Lütfen tüm zorunlu alanları eksiksiz ve doğru doldurun.'
      });
      return;
    }

    // 18 yaş kontrolü
    if (this.step1Data().birthDate) {
      const age = this.calculateAge(this.step1Data().birthDate);
      if (age < 18) {
        this.messageService.add({
          severity: 'error',
          summary: 'Yaş Sınırı',
          detail: 'Kayıt olabilmek için en az 18 yaşında olmalısınız.'
        });
        return;
      }
    }

    this.currentStep.set(2);
    this.scrollToTop();
  }

  // Bir önceki adıma geri dönme metodu
  previousStep(): void {
    this.currentStep.set(1);
    this.scrollToTop();
  }

  // ==================== FORM SUBMIT (KAYIT İŞLEMİ) ====================
  onSubmit(form: NgForm): void {
    form.control.markAllAsTouched();

    if (!this.termsAccepted()) {
      this.messageService.add({
        severity: 'warn',
        summary: 'Uyarı',
        detail: 'Devam etmek için kullanım koşullarını kabul etmelisiniz.'
      });
      return;
    }

    // Şifre eşleşme kontrolü
    if (this.step2Data().password !== this.step2Data().confirmPassword) {
      this.messageService.add({
        severity: 'error',
        summary: 'Şifre Hatası',
        detail: 'Şifreler birbiriyle eşleşmiyor.'
      });
      return;
    }

    if (form.invalid) {
      this.messageService.add({
        severity: 'warn',
        summary: 'Uyarı',
        detail: 'Lütfen formdaki hataları düzeltin.'
      });
      return;
    }
    this.loading.set(true);

    // Backend DTO formatına uygun payload oluşturma
    const payload = {
      firstName: this.step1Data().firstName.trim(),
      lastName: this.step1Data().lastName.trim(),
      identityNumber: this.step1Data().identityNumber.trim(),
      birthDate: this.formatDate(this.step1Data().birthDate!),
      gender: this.step1Data().gender || null,
      email: this.step2Data().email.trim(),
      phoneNumber: this.step2Data().phoneNumber?.trim() ?? '',
      password: this.step2Data().password,
      address: this.step2Data().address.trim(),
      city: this.step2Data().city.trim(),
      country: this.step2Data().country.trim(),
      postalCode: this.step2Data().postalCode.trim() || null
    };

    // Servise kayıt isteği atma
    this.customerAuthService.registerIndividual(payload)
      .pipe(finalize(() => this.loading.set(false)))
      .subscribe({
        next: (res: Result<string>) => {
          if (res.isSuccessful) {
            this.messageService.add({
              severity: 'success',
              summary: 'Kayıt Başarılı',
              detail: 'Hesabınız oluşturuldu. Şimdi giriş yapabilirsiniz.',
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

  // ==================== YARDIMCI METOTLAR (HELPERS) ====================
  // Yaş hesaplama metodu
  private calculateAge(birthDate: Date | string | null | undefined): number {
    if (!birthDate) return 0;

    const today = new Date();
    const birth = new Date(birthDate);

    if (isNaN(birth.getTime())) return 0;

    let age = today.getFullYear() - birth.getFullYear();
    const m = today.getMonth() - birth.getMonth();

    if (m < 0 || (m === 0 && today.getDate() < birth.getDate())) {
      age--;
    }

    return age;
  }

  // Tarihi 'YYYY-MM-DD' API formatına dönüştüren metot
  private formatDate(date: Date): string {
    const y = date.getFullYear();
    const m = String(date.getMonth() + 1).padStart(2, '0');
    const d = String(date.getDate()).padStart(2, '0');
    return `${y}-${m}-${d}`;
  }

  // Adım değişiminde ekranı en üste kaydıran yumuşak kaydırma metodu
  private scrollToTop(): void {
    window.scrollTo({ top: 0, behavior: 'smooth' });
  }

  // Giriş sayfasına yönlendirme metodu
  goToLogin(): void {
    this.router.navigate(['/customer-auth/customer-login']);
  }
}