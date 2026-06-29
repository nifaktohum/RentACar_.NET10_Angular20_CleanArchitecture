export interface LoginResponse {
  token: string;
  email: string;
  fullName: string;
  permissions?: string[];
}