export interface CreateExtraRequest {
  name: string;
  description: string | null;
  icon: string | null;
  price: number;
  priceType: number; // 1=Daily, 2=Rental
  category: number; // 1=Guarantee, 2=Driver, 3=Seat, 4=Other
  displayOrder: number;
  isRecommended: boolean;
  minAge: number | null;
  ageRange: string | null;
  stockLimit: number | null;
  isActive: boolean;
}