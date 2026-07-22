// core/models/protection-benefit-request.model.ts

import { BenefitCategory } from "./benefit-category/benefit-category.model";



export interface CreateProtectionBenefitRequest {
  name: string;
  description?: string | null;
  icon?: string | null;
  displayOrder: number;
  categoryId: string;
}

export interface UpdateProtectionBenefitRequest {
  id: string;
  name: string;
  description?: string | null;
  icon?: string | null;
  displayOrder: number;
  categoryId?: string;
}