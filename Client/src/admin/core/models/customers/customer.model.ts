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

export interface CustomerDetailDto extends CustomerListDto {
  address: string;
  postalCode: string | null;
  identityNumber: string | null;
  birthDate: string | null;
  gender: string | null;
  age: number | null;
  taxNumber: string | null;
  taxOffice: string | null;
  driverLicenseNumber: string | null;
  driverLicenseIssueDate: string | null;
  driverLicenseClass: string | null;
  hasValidDriverLicense: boolean;
  blacklistReason: string | null;
  isActive: boolean;
  canRent: boolean;
  canRentLuxury: boolean;
  updatedAt: string | null;
}
