import { computed, Injectable, signal } from "@angular/core";
import { CustomerType } from "../../../../shared/enums/customer-type";
import { CustomerSpecParams } from "../../models/customers/customer-spec-params";

@Injectable({
  providedIn: 'root',
})

export class CustomerStateService {

  // ============ 🔢 SAYFALAMA ============
  readonly pageIndex = signal<number>(1);
  readonly pageSize = signal<number>(5);

  // ============ 🔀 SIRALAMA ============
  readonly sortField = signal<string>('createdAt');
  readonly sortOrder = signal<number>(-1);   // -1 → Desc, 1 → Asc

  // ============ 🔍 GENEL ARAMA ============
  readonly filterSearch = signal<string>('');

  // ============ 🎯 MÜŞTERİ TİPİ ============
  readonly selectedType = signal<CustomerType | null>(null);

  // ============ 🚫 KARA LİSTE ============
  readonly filterIsBlacklisted = signal<boolean | null>(null);

  // ============ 📧 EMAIL DOĞRULAMA ============
  readonly filterIsEmailVerified = signal<boolean | null>(null);

  // ============ 📱 TELEFON DOĞRULAMA ============
  readonly filterIsPhoneVerified = signal<boolean | null>(null);

  // ============ ✅ AKTİFLİK ============
  readonly filterIsActive = signal<boolean | null>(null);

  // ============ 📊 FİNDEKS ARALIĞI ============
  readonly filterMinFindeks = signal<number | null>(null);
  readonly filterMaxFindeks = signal<number | null>(null);

  // ============ 🏙️ ŞEHİR / 🌍 ÜLKE ============
  readonly filterCity = signal<string>('');
  readonly filterCountry = signal<string>('');

  // ============ 📅 TARİH ARALIĞI ============
  readonly filterCreatedAfter = signal<Date | null>(null);
  readonly filterCreatedBefore = signal<Date | null>(null);

  // ============================================================
  // 🚀 TAM REAKTİF PARAMETRE NESNESİ
  // Filtre değiştiğinde otomatik tetiklenir (Vehicle'daki gibi)
  // ============================================================
  readonly filterParams = computed<CustomerSpecParams>(() => {
    const params: CustomerSpecParams = {
      pageNumber: this.pageIndex(),
      pageSize: this.pageSize(),
    };

    // Sıralama — "createdAtDesc" formatı
    if (this.sortField()) {
      const direction = this.sortOrder() === 1 ? 'asc' : 'desc';
      params.sort = `${this.sortField().toLowerCase()}${direction}`;
      
      // ⭐ Debug
      console.log('SORT DEBUG:', {
        sortField: this.sortField(),
        sortOrder: this.sortOrder(),
        sortParam: params.sort
      });
    }

    // Genel arama
    if (this.filterSearch()) params.searchTerm = this.filterSearch();

    // Müşteri tipi
    const type = this.selectedType();
    if (type) params.typeString = type;

    // Boolean filtreler (⚠️ false da değerli!)
    const isBlacklisted = this.filterIsBlacklisted();
    if (isBlacklisted !== null) params.isBlacklisted = isBlacklisted;

    const isEmailVerified = this.filterIsEmailVerified();
    if (isEmailVerified !== null) params.isEmailVerified = isEmailVerified;

    const isPhoneVerified = this.filterIsPhoneVerified();
    if (isPhoneVerified !== null) params.isPhoneVerified = isPhoneVerified;

    const isActive = this.filterIsActive();
    if (isActive !== null) params.isActive = isActive;

    // Findeks aralığı
    const minFindeks = this.filterMinFindeks();
    if (minFindeks !== null) params.minFindeks = minFindeks;

    const maxFindeks = this.filterMaxFindeks();
    if (maxFindeks !== null) params.maxFindeks = maxFindeks;

    // Konum
    if (this.filterCity()) params.city = this.filterCity();
    if (this.filterCountry()) params.country = this.filterCountry();

    // Tarih aralığı (ISO string)
    const createdAfter = this.filterCreatedAfter();
    if (createdAfter) params.createdAfter = createdAfter.toISOString();

    const createdBefore = this.filterCreatedBefore();
    if (createdBefore) params.createdBefore = createdBefore.toISOString();

    return params;
  });

  // ============================================================
  // ⭐ FİLTRE VAR MI?
  // ============================================================
  readonly hasActiveFilters = computed(() => {
    return !!(
      this.filterSearch() ||
      this.selectedType() ||
      this.filterIsBlacklisted() !== null ||
      this.filterIsEmailVerified() !== null ||
      this.filterIsPhoneVerified() !== null ||
      this.filterIsActive() !== null ||
      this.filterMinFindeks() !== null ||
      this.filterMaxFindeks() !== null ||
      this.filterCity() ||
      this.filterCountry() ||
      this.filterCreatedAfter() ||
      this.filterCreatedBefore()
    );
  });

  // ============================================================
  // 🔄 RESET — Tüm state
  // ============================================================
  reset(): void {
    this.pageIndex.set(1);
    this.pageSize.set(5);
    this.sortField.set('createdAt');
    this.sortOrder.set(-1);
    this.filterSearch.set('');
    this.selectedType.set(null);
    this.filterIsBlacklisted.set(null);
    this.filterIsEmailVerified.set(null);
    this.filterIsPhoneVerified.set(null);
    this.filterIsActive.set(null);
    this.filterMinFindeks.set(null);
    this.filterMaxFindeks.set(null);
    this.filterCity.set('');
    this.filterCountry.set('');
    this.filterCreatedAfter.set(null);
    this.filterCreatedBefore.set(null);
  }

  // ============================================================
  // 🔄 RESET — Sadece filtreler (sayfa korunur mu? Hayır, 1'e döner)
  // ============================================================
  resetFilters(): void {
    this.filterSearch.set('');
    this.selectedType.set(null);
    this.filterIsBlacklisted.set(null);
    this.filterIsEmailVerified.set(null);
    this.filterIsPhoneVerified.set(null);
    this.filterIsActive.set(null);
    this.filterMinFindeks.set(null);
    this.filterMaxFindeks.set(null);
    this.filterCity.set('');
    this.filterCountry.set('');
    this.filterCreatedAfter.set(null);
    this.filterCreatedBefore.set(null);
    this.pageIndex.set(1);
    this.sortField.set('createdAt');
    this.sortOrder.set(-1);
  }

  // ============================================================
  // 🔄 RESET — Sadece sayfa (filtre değişince kullan)
  // ============================================================
  resetPage(): void {
    this.pageIndex.set(1);
  }

}