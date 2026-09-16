export interface VehicleImage {
  id: string;
  imageUrl: string;
  displayOrder: number;
  isMain: boolean;
  description: string | null;
  createdAt: string;
}

export interface SetMainImageRequest {
  vehicleId: string;
  imageId: string;
}