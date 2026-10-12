// ==================== LOGIN ====================
export interface CustomerLoginResponse {
  token: string;
  refreshToken: string;
  customerId: string;              // Guid → string
  fullName: string;
  email: string;
  customerType: 'Individual' | 'Corporate';
  isEmailVerified: boolean;
  isPhoneVerified: boolean;
  isBlacklisted: boolean;
  blacklistReason: string | null;
}

export interface CustomerLoginRequest {
  email: string;
  password: string;
  rememberMe: boolean;
}