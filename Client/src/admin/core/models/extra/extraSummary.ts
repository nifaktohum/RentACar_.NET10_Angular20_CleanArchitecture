export interface ExtraSummary {
  id: string;
  name: string;
  price: number;
  icon: string;
  priceType: string; // "Daily" | "Rental"
  category: string; // "Guarantee" | "Driver" | "Seat" | "Other"
  displayOrder: number;
  isRecommended: boolean;
  isActive: boolean;
  createdAt: string;
  createdByName: string | null;
}