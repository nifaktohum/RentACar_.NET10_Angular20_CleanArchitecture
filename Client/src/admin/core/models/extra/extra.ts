export interface Extra {
  id: string;
  name: string;
  description: string | null;
  icon: string | null;
  price: number;
  priceType: string; // "Daily" | "Rental"
  category: string; // "Guarantee" | "Driver" | "Seat" | "Other"
  displayOrder: number;
  isRecommended: boolean;
  minAge: number | null;
  ageRange: string | null;
  stockLimit: number | null;
  isActive: boolean;
  createdAt: string;
  createdBy: string;
  createdByName: string | null;
  updatedAt: string | null;
  updatedBy: string | null;
  updatedByName: string | null;
}