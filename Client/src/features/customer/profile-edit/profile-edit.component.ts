import { ChangeDetectionStrategy, Component, computed, inject, signal, ViewEncapsulation } from '@angular/core';
import { Router, RouterLink } from '@angular/router';
import { MessageService } from 'primeng/api';
import { CustomerProfileService } from '../../../core/services/customers/customer-profile.service';
import { CustomerProfileModel } from '../../../core/models/customer-profile.model';
import { finalize } from 'rxjs';
import { Result } from '../../../core/models/result.model';
import { FormsModule, NgForm } from '@angular/forms';
import { DatePickerModule } from 'primeng/datepicker';
import { TextareaModule } from 'primeng/textarea';
import { PhoneFormatPipe } from '../../../shared/pipes/phone-format.pipe';
import { SelectModule } from 'primeng/select';
import { SkeletonModule } from 'primeng/skeleton';
import { InputTextModule } from 'primeng/inputtext';
import { PrimeDateMaskDirective } from '../../../shared/directives/prime-date-mask.directive';
import { getCities, getDistrictsByCityCode } from 'turkey-neighbourhoods';
import { DRIVER_LICENSE_CLASSES } from '../../../shared/constants/driver-license.constants';


@Component({
  selector: 'app-profile-edit',
  imports: [
    FormsModule,
    DatePickerModule,
    TextareaModule,
    SelectModule,
    SkeletonModule,
    PhoneFormatPipe,
    InputTextModule,
    RouterLink,
    PrimeDateMaskDirective,
],
  templateUrl: './profile-edit.component.html',
  styleUrl: './profile-edit.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
  encapsulation: ViewEncapsulation.Emulated
})
export class ProfileEditComponent {
  private profileService = inject(CustomerProfileService);
  private router = inject(Router);
  private messageService = inject(MessageService);
  readonly driverLicenseClassOptions = DRIVER_LICENSE_CLASSES;
  // ==================== STATE ====================
  isLoading = signal<boolean>(true);
  isSaving = signal<boolean>(false);
  profile = signal<CustomerProfileModel | null>(null);

  maxBirthDate = new Date();
  maxLicenseDate = new Date();

  // Form model
  formModel = signal({
    // Bireysel
    firstName: '',
    lastName: '',
    identityNumber: '',
    birthDate: '',
    gender: '' ,

    // Kurumsal
    companyName: '',
    taxNumber: '',
    taxOffice: '',

    // İletişim
    phoneNumber: '',
    address: '',
    city: '',
    country: 'Türkiye',
    postalCode: '',

    // Ehliyet
    driverLicenseNumber: '',
    driverLicenseIssueDate: '',
    driverLicenseClass: ''
  });


  // ==================== COMPUTED ====================
  customerType = computed(() => this.profile()?.type || 'Individual');
  isIndividual = computed(() => this.customerType() === 'Individual');
  isCorporate = computed(() => this.customerType() === 'Corporate');

  pageTitle = computed(() => {
    return this.isCorporate() ? 'Şirket Bilgilerini Düzenle' : 'Profil Bilgilerini Düzenle';
  });

  // ==================== OPTIONS ====================
  genderOptions = [
    { label: 'Erkek', value: 'Male' },
    { label: 'Kadın', value: 'Female' },
  ];

  // ==================== İL / İLÇE STATE ====================
  // İl listesini nesne dizisi olarak tutar (Örn: [{code: "16", name: "Bursa"}])
  cities: { code: string; name: string }[] = [];
  // Seçilen ile ait ilçeleri tutar
  districts: { name: string }[] = [];
  // Seçili ilin plaka kodu ("16" gibi)
  selectedCityCode: string | null = null;
  // Seçili ilçenin ismi ("Nilüfer" gibi)
  selectedDistrict: string | null = null;

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
  
      this.updateField('city', cityValue);
    }
  

  // ==================== LIFECYCLE ====================
  ngOnInit(): void {
    this.cities = getCities();
    this.loadProfile();
  }

  // ==================== LOAD ====================
  loadProfile(): void {
    this.isLoading.set(true);

    this.profileService.getProfile()
      .pipe(finalize(() => this.isLoading.set(false)))
      .subscribe({
        next: (res: Result<CustomerProfileModel>) => {
          if (res.isSuccessful && res.data) {
            this.profile.set(res.data);
            this.patchForm(res.data);
          } else {
            this.messageService.add({
              severity: 'error',
              summary: 'Hata',
              detail: res.errorMessages?.[0] || 'Profil yüklenemedi.'
            });
            this.router.navigate(['/customer/profile']);
          }
        },
        error: (err) => {
          this.messageService.add({
            severity: 'error',
            summary: 'Hata',
            detail: err?.error?.errorMessages?.[0]
              || err?.error?.message
              || 'Sunucu hatası, profil yüklenemedi.'
          });
          this.router.navigate(['/customer/profile']);
        }
      });
  }

  // ==================== PATCH FORM ====================
  // ==================== PATCH FORM ====================
  private patchForm(p: CustomerProfileModel): void {
    this.formModel.set({
      firstName: p.firstName || '',
      lastName: p.lastName || '',
      identityNumber: p.identityNumber || '',
      birthDate: p.birthDate ? this.toDisplayDate(p.birthDate) : '',
      gender: p.gender || '',

      companyName: p.companyName || '',
      taxNumber: p.taxNumber || '',
      taxOffice: p.taxOffice || '',

      phoneNumber: p.phoneNumber || '',
      address: p.address || '',
      city: p.city || '',
      country: p.country || 'Türkiye',
      postalCode: p.postalCode || '',

      driverLicenseNumber: p.driverLicenseNumber || '',
      driverLicenseIssueDate: p.driverLicenseIssueDate
        ? this.toDisplayDate(p.driverLicenseIssueDate)
        : '',
      driverLicenseClass: p.driverLicenseClass || ''
    });

    // ⭐ Mevcut city'i parse et → İl/İlçe dropdown'larını doldur
    this.parseCityToDropdowns(p.city);
  }

  // ⭐ "Bursa/Nilüfer" → selectedCityCode + selectedDistrict
  private parseCityToDropdowns(cityValue: string): void {
    if (!cityValue) return;

    const parts = cityValue.split('/');
    const cityName = parts[0]?.trim();
    const districtName = parts[1]?.trim();

    if (!cityName) return;

    // İl'i bul
    const foundCity = this.cities.find(c =>
      c.name.toLowerCase() === cityName.toLowerCase()
    );

    if (foundCity) {
      this.selectedCityCode = foundCity.code;

      // İlçeleri yükle
      this.districts = getDistrictsByCityCode(foundCity.code).map(d => ({ name: d }));

      // İlçeyi bul
      if (districtName) {
        const foundDistrict = this.districts.find(d =>
          d.name.toLowerCase() === districtName.toLowerCase()
        );
        if (foundDistrict) {
          this.selectedDistrict = foundDistrict.name;
        }
      }
    }
  }


  // ⭐ Backend'den gelen "1990-01-01" → "01/01/1990"
  private toDisplayDate(isoDate: string): string {
    if (!isoDate) return '';
    const [year, month, day] = isoDate.split('-');
    if (!year || !month || !day) return '';
    return `${day}/${month}/${year}`;
  }

  // ⭐ Input'tan gelen "01/01/1990" → "1990-01-01"
  private toIsoDate(displayDate: string): string | null {
    if (!displayDate) return null;
    const cleaned = displayDate.replace(/\D/g, '');
    if (cleaned.length !== 8) return null;

    const day = cleaned.substring(0, 2);
    const month = cleaned.substring(2, 4);
    const year = cleaned.substring(4, 8);

    // Validasyon
    const d = parseInt(day, 10);
    const m = parseInt(month, 10);
    const y = parseInt(year, 10);

    if (d < 1 || d > 31 || m < 1 || m > 12 || y < 1900 || y > 2100) return null;

    return `${year}-${month}-${day}`;
  }

  // ==================== FIELD UPDATE ====================
  updateField<K extends keyof ReturnType<typeof this.formModel>>(
    field: K,
    value: ReturnType<typeof this.formModel>[K]
  ): void {
    this.formModel.update(prev => ({ ...prev, [field]: value }));
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

  // ==================== SUBMIT ====================
  onSubmit(form: NgForm): void {
    form.control.markAllAsTouched();

    if (form.invalid) {
      this.messageService.add({
        severity: 'warn',
        summary: 'Uyarı',
        detail: 'Lütfen tüm zorunlu alanları doldurun.'
      });

      const firstInvalid = document.querySelector('.ng-invalid') as HTMLElement;
      firstInvalid?.scrollIntoView({ behavior: 'smooth', block: 'center' });
      firstInvalid?.focus();
      return;
    }

    const f = this.formModel();

    // Tarih validasyonu
    if (this.isIndividual() && f.birthDate) {
      const iso = this.toIsoDate(f.birthDate);
      if (!iso) {
        this.messageService.add({
          severity: 'error',
          summary: 'Hata',
          detail: 'Geçerli bir doğum tarihi giriniz (GG/AA/YYYY).'
        });
        return;
      }
    }

    if (this.isIndividual() && f.driverLicenseIssueDate) {
      const iso = this.toIsoDate(f.driverLicenseIssueDate);
      if (!iso) {
        this.messageService.add({
          severity: 'error',
          summary: 'Hata',
          detail: 'Geçerli bir ehliyet veriliş tarihi giriniz (GG/AA/YYYY).'
        });
        return;
      }
    }

    // İl/İlçe validasyonu
    if (!this.selectedCityCode) {
      this.messageService.add({
        severity: 'warn',
        summary: 'Uyarı',
        detail: 'Lütfen il seçiniz.'
      });
      return;
    }

    if (!this.selectedDistrict) {
      this.messageService.add({
        severity: 'warn',
        summary: 'Uyarı',
        detail: 'Lütfen ilçe seçiniz.'
      });
      return;
    }

    this.isSaving.set(true);

    const payload = this.buildPayload();

    this.profileService.updateProfile(payload)
      .pipe(finalize(() => this.isSaving.set(false)))
      .subscribe({
        next: (res: Result<string>) => {
          if (res.isSuccessful) {
            this.messageService.add({
              severity: 'success',
              summary: 'Başarılı',
              detail: 'Profiliniz başarıyla güncellendi.',
              life: 4000
            });
            this.router.navigate(['/customer/profile']);
          } else {
            this.messageService.add({
              severity: 'warn',
              summary: 'Uyarı',
              detail: res.errorMessages?.[0] || 'Güncelleme yapılamadı.'
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
          console.error(err);
        }
      });
  }

  // ==================== BUILD PAYLOAD ====================
  private buildPayload(): any {
    const f = this.formModel();
    const isIndividual = this.isIndividual();

    return {
      // Ortak
      phoneNumber: f.phoneNumber?.trim() ?? '',
      address: f.address?.trim() ?? '',
      city: f.city?.trim() ?? '',
      country: f.country?.trim() ?? '',
      postalCode: f.postalCode?.trim() || null,

      // Bireysel
      firstName: isIndividual ? (f.firstName?.trim() || null) : null,
      lastName: isIndividual ? (f.lastName?.trim() || null) : null,
      identityNumber: isIndividual ? (f.identityNumber?.trim() || null) : null,
      birthDate: isIndividual ? this.toIsoDate(f.birthDate) : null,
      gender: isIndividual ? (f.gender || null) : null,

      // Kurumsal
      companyName: !isIndividual ? (f.companyName?.trim() || null) : null,
      taxNumber: !isIndividual ? (f.taxNumber?.trim() || null) : null,
      taxOffice: !isIndividual ? (f.taxOffice?.trim() || null) : null,

      // Ehliyet (sadece individual)
      driverLicenseNumber: isIndividual ? (f.driverLicenseNumber?.trim() || null) : null,
      driverLicenseIssueDate: isIndividual
        ? this.toIsoDate(f.driverLicenseIssueDate)
        : null,
      driverLicenseClass: isIndividual ? (f.driverLicenseClass?.trim() || null) : null
    };
  }

  // ==================== CANCEL ====================
  cancel(): void {
    this.router.navigate(['/customer/profile']);
  }












}
