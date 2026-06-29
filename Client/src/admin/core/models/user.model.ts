
export interface UserModel {
  id: string;
  firstName: string;
  lastName: string;
  fullName: string;
  email: string;
  phoneNumber: string;
  branchName: string;
  branchId: string;
  roleNames: string[];
  isActive: boolean;
  createdAt: string;
}

export interface UpdateUserRequest {
  id: string;
  firstName: string;
  lastName: string;
  email: string;
  phoneNumber: string;
  branchId: string;
  roleId: string;
  isActive: boolean;
}

