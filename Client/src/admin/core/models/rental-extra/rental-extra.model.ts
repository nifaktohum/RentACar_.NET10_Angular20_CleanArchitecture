export interface RentalExtra {
  id: string;
  rentalId: string;
  extraId: string;
  extraName: string;
  extraIcon: string | null;
  unitPrice: number;
  quantity: number;
  totalPrice: number;
  priceType: 'Daily' | 'Rental';
  createdAt: string;
  createdBy: string;
}