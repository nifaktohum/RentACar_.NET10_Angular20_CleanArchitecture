// src/app/modules/admin/categories/models/category.model.ts

export interface Category {
  // BaseEntity alanları
  id: string;
  createdAt: string;           // ISO format: "2026-06-25T01:57:02.508Z"
  createdBy: string;
  updatedAt: string | null;
  updatedBy: string | null;
  isActive: boolean;
  isDeleted: boolean;
  deletedAt: string | null;
  deletedBy: string | null;

  // Category özel alanları
  name: string;
  slug: string;
  description?: string | null;
  displayOrder?: number | null;
  parentCategoryId?: string | null;
  parentCategoryName: string | null; 

  // Navigation property'ler
  parentCategory?: Category;
  subCategories?: Category[];
}

// Request/Response tipleri
export interface CreateCategoryRequest {
  name: string;
  slug: string;
  description?: string | null;
  displayOrder?: number | null;
  parentCategoryId?: string | null;
}

export interface UpdateCategoryRequest {
  id: string;
  name: string;
  slug: string;
  description?: string | null;
  displayOrder?: number | null;
  parentCategoryId?: string | null;
}