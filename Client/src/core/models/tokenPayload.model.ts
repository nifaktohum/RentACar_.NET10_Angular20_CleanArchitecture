// export interface TokenPayload {
//   Id: string;
//   Email: string;
//   FullName: string;
//   SecurityStamp: string;
//   BranchId: string;
//   UserType?: 'Customer' | 'Staff';
//   CustomerType?: string;           // (Individual/Corporate)
//   Role?: string | string[];
//   Permission?: string[];
//   exp: number;
//   iss: string;
//   aud: string;
// }

export interface TokenPayload {
  // .NET ClaimTypes.NameIdentifier
  'http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier'?: string;

  // ⭐ Backend'de özel claim'ler
  Id: string;
  Email: string;
  PhoneNumber?: string;
  FullName: string;
  SecurityStamp: string;

  // ⭐ YENİ: User mı Customer mı?
  UserType?: 'Customer' | 'Staff';

  // ⭐ YENİ: Sadece Customer token'ında olur
  CustomerType?: 'Individual' | 'Corporate';
  IsEmailVerified?: string;   // "True" / "False" (string olarak gelir!)
  IsPhoneVerified?: string;
  IsBlacklisted?: string;

  // ⭐ Sadece User (personel) token'ında olur
  BranchId?: string;
  BranchName?: string;
  Role?: string | string[];
  Permission?: string[];

  // JWT standart
  exp: number;
  iss: string;
  aud: string;
}