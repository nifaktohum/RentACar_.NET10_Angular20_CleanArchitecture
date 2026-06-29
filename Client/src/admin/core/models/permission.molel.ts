export interface PermissionModel {
  id: string;
  name: string;
  description: string;
  isActive: boolean;
  isSystemDisabled: boolean;
  
}

export interface PermissionResponseDto {
  isSuccess: boolean;
  message: string;
  affectedUsersCount: number;
}