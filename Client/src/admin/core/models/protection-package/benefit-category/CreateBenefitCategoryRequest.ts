export interface CreateBenefitCategoryRequest {
  name: string;
  slug: string;
  description?: string | null;
  icon?: string | null;
  displayOrder: number;
}