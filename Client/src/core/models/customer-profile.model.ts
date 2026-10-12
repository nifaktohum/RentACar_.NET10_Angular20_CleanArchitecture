export interface CustomerProfileModel {
  id: string;
  type: 'Individual' | 'Corporate';
  email: string;
  phoneNumber: string;
  address: string;
  city: string;
  country: string;
  postalCode: string | null;

  // Bireysel
  firstName: string | null;
  lastName: string | null;
  identityNumber: string | null;
  birthDate: string | null;   // ISO format: "1990-01-01"
  gender: string | null;
  age: number | null;

  // Kurumsal
  companyName: string | null;
  taxNumber: string | null;
  taxOffice: string | null;

  // Ehliyet
  driverLicenseNumber: string | null;
  driverLicenseIssueDate: string | null;
  driverLicenseClass: string | null;
  hasValidDriverLicense: boolean;

  // Finansal
  findeksScore: number | null;

  // Durum
  isEmailVerified: boolean;
  isPhoneVerified: boolean;
  isBlacklisted: boolean;
  blacklistReason: string | null;

  // İş kuralları
  canRent: boolean;
  canRentLuxury: boolean;

  // Audit
  createdAt: string;
}