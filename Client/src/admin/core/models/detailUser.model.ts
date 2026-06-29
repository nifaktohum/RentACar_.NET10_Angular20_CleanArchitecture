// Core modellerin içerisine koyabilirsin

export interface UserDetail {
  id: string;
  fullName: string;
  email: string;
  phoneNumber: string;
  branchName: string;
  branchId: string;
  isActive: boolean;
  isDeleted: boolean;
  createdAt: string;   // Backend'den gelen tarih formatı için Date kullanıyoruz
  createdBy: string;
  createdByName: string;
  updatedAt: string | null;
  updatedBy: string;
  updatedByName: string;
  roles: UserRoleDto[]; 
  permissions: UserPermissionDto[];
}


export interface UserRoleDto {
  roleId: string;
  roleName: string;
  description: string | null;
}

export interface UserPermissionDto {
  permissionId: string;     // ✅ Backend'deki "PermissionId" ile aynı!
  permissionName: string;   // ✅ Backend'deki "PermissionName" ile aynı!
  module: string | null;    // ✅ Backend'deki "Module" ile aynı!
}
