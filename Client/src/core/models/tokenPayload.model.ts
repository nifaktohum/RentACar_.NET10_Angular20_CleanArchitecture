export interface TokenPayload {
  Id: string;
  Email: string;
  FullName: string;
  SecurityStamp: string;
  BranchId: string;
  Role: string | string[]; // Tek rol string gelir, çoklu rol diziye döner kanks
  Permission: string[];
  exp: number;
  iss: string;
  aud: string;
}