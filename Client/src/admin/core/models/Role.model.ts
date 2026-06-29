// Backend'deki Role entity'sinin Angular'daki karşılığı
// 1. Tabloda listeleme yaparken dönen ana model
export interface RoleModel {
  id: string;
  name: string;
  description: string;
}

// 2. CreateRoleCommand için ID barındırmayan model
export interface CreateRoleModel {
  name: string;
  description: string;
}

// 3. UpdateRoleCommand için ID barındıran model
export interface UpdateRoleModel {
  id: string;
  name: string;
  description: string;
}