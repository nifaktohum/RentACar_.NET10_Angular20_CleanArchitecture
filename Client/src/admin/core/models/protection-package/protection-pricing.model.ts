// core/models/protection-pricing.model.ts

export interface ProtectionPricing {
  id: string;
  protectionPackageId: string;
  dailyPrice?: number | null;
  deductibleAmount?: number | null;
  isDefault: boolean;
  validityStart: string;
  validityEnd?: string | null;
  isCurrentlyValid: boolean;
  isActive: boolean;
  createdAt: string;
  createdBy: string;
  createdByName?: string;
  updatedAt?: string | null;
  updatedBy?: string | null;
  updatedByName?: string | null;
}