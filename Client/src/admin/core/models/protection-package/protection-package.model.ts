import { ProtectionBenefit } from "./protection-benefit.model";
import { ProtectionPricing } from "./protection-pricing.model";

export interface ProtectionPackage {
  id: string;
  name: string;
  description?: string | null;
  icon?: string | null;
  displayOrder: number;
  isRecommended: boolean;
  starRating: number; // 1-5
  protectionLevel: ProtectionLevel;
  deductibleType: DeductibleType;
  benefits: ProtectionBenefit[];
  pricing: ProtectionPricing[];
  isActive: boolean;
  createdAt: string;
  createdBy: string;
  createdByName?: string;
  updatedAt?: string | null;
  updatedBy?: string | null;
  updatedByName?: string | null;
}

export enum ProtectionLevel {
  Basic = 0,
  Standard = 1,
  Plus = 2,
  Premium = 3,
  Platinum = 4
}

export enum DeductibleType {
  WithDeductible = 0,
  ZeroDeductible = 1,
  ReducedDeductible = 2
}