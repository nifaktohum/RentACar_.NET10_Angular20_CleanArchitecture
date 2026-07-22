export interface BenefitCategory {
  id: string;
  name: string;
  slug: string;
  description?: string | null;
  icon?: string | null;
  displayOrder: number;
  isActive: boolean;
  createdAt: string;
  createdBy: string;
  createdByName?: string;
  updatedAt?: string | null;
  updatedBy?: string | null;
  updatedByName?: string | null;
  benefitCount?: number;
  benefits?: BenefitBrief[];
}

export interface BenefitBrief {
  id: string;
  name: string;
  icon?: string | null;
  displayOrder: number;
  isActive: boolean;
}