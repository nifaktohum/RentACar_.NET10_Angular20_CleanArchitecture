// core/models/protection-benefit.model.ts

import { BenefitCategory } from "./benefit-category/benefit-category.model";

export interface ProtectionBenefit {
  id: string;
  name: string;
  description?: string | null;
  icon?: string | null;
  displayOrder: number;
  categoryId: string;
  isActive: boolean;
  createdAt: string;
  createdBy: string;
  createdByName?: string;
  updatedAt?: string | null;
  updatedBy?: string | null;
  updatedByName?: string | null;
}

// export enum BenefitCategory {
//   Tire = 0,        // Lastik
//   Glass = 1,       // Cam
//   Light = 2,       // Far
//   Engine = 3,      // Motor
//   Body = 4,        // Karoser
//   ThirdParty = 5,  // 3. Şahıs
//   MiniDamage = 6,  // Mini Hasar
//   KeyPlate = 7,    // Anahtar/Plaka/Ruhsat
//   Roadside = 8,    // Yol Yardımı
//   Theft = 9        // Hırsızlık
// }