export interface HrUser {
  id: string;
  email: string;
  fullName: string;
  isActive: boolean;
  createdAt: string;
}

export interface CreateHrUserDto {
  email: string;
  password: string;
  fullName: string;
}
