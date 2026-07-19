export type UserRole = 'User' | 'Operator' | 'Admin';

export interface UserAccount {
  id: string;
  email: string;
  role: UserRole;
  isActive: boolean;
}

export interface CreateUserAccountRequest {
  email: string;
  password: string;
  role: UserRole;
}

export interface UpdateUserRoleRequest {
  role: UserRole;
}

export interface UpdateUserStatusRequest {
  isActive: boolean;
}

export interface UserAccountQuery {
  page?: number;
  pageSize?: number;
  search?: string;
  role?: UserRole | '';
  isActive?: boolean | null;
}

export interface PagedUserAccounts {
  items: UserAccount[];
  total: number;
  page: number;
  pageSize: number;
}
