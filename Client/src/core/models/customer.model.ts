export interface CustomerProfileDto {
  id: string;
  type: 'Individual' | 'Corporate';
  email: string;
  phoneNumber: string;
  address: string;
  city: string;
  country: string;
  postalCode: string | null;

  firstName: string | null;
  lastName: string | null;
  identityNumber: string | null;
  birthDate: string | null;
  gender: string | null;
  age: number | null;

  companyName: string | null;
  taxNumber: string | null;
  taxOffice: string | null;

  driverLicenseNumber: string | null;
  driverLicenseIssueDate: string | null;
  driverLicenseClass: string | null;
  hasValidDriverLicense: boolean;

  findeksScore: number | null;

  isEmailVerified: boolean;
  isPhoneVerified: boolean;
  isBlacklisted: boolean;
  blacklistReason: string | null;

  canRent: boolean;
  canRentLuxury: boolean;

  createdAt: string;
}

export interface CustomerListDto {
  id: string;
  type: 'Individual' | 'Corporate';
  email: string;
  phoneNumber: string;
  firstName: string | null;
  lastName: string | null;
  companyName: string | null;
  city: string;
  country: string;
  isEmailVerified: boolean;
  isPhoneVerified: boolean;
  isBlacklisted: boolean;
  findeksScore: number | null;
  createdAt: string;
}

export interface CustomerDetailDto extends CustomerProfileDto {
  isActive: boolean;
  updatedAt: string | null;
}