// core/models/protection-package-request.model.ts

import { ProtectionLevel, DeductibleType } from './protection-package.model';

export interface CreateProtectionPackageRequest {
  name: string;
  description?: string | null;
  icon?: string | null;
  displayOrder: number;
  isRecommended: boolean;
  starRating: number;
  protectionLevel: ProtectionLevel;
  deductibleType: DeductibleType;
  benefitIds: string[];
  pricing: CreateProtectionPricingRequest;
  isActive: boolean;
}

export interface UpdateProtectionPackageRequest {
  id: string;
  name: string;
  description?: string | null;
  icon?: string | null;
  displayOrder: number;
  isRecommended: boolean;
  starRating: number;
  protectionLevel: ProtectionLevel;
  deductibleType: DeductibleType;
  benefitIds: string[];
  pricing: UpdateProtectionPricingRequest;
  isActive: boolean;
}

export interface CreateProtectionPricingRequest {
  dailyPrice?: number | null;
  deductibleAmount?: number | null;
  isDefault: boolean;
  validityStart?: string | null;
  validityEnd?: string | null;
}

export interface UpdateProtectionPricingRequest {
  pricingId?: string | null;
  dailyPrice?: number | null;
  deductibleAmount?: number | null;
  isDefault: boolean;
  validityStart?: string | null;
  validityEnd?: string | null;
  isActive: boolean;
}