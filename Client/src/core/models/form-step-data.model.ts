// ==================== INTERFACES (TİP TANIMLARI) ====================
// Step 1 Formunun tip güvenliğini sağlayan model
export interface Step1Data {
  firstName: string;
  lastName: string;
  identityNumber: string;
  birthDate: Date | null;
  gender: string | null;
}

// Step 2 Formunun tip güvenliğini sağlayan model
export interface Step2Data {
  email: string;
  phoneNumber: string;
  password: string;
  confirmPassword: string;
  address: string;
  city: string;
  country: string;
  postalCode: string;
}