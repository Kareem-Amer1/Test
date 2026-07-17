export interface UserProfile {
  id: string;
  email: string;
  fullName: string;
  role: string;
  createdAt: string;
}

export interface UpdateProfileDto {
  fullName: string;
}

export interface ChangePasswordDto {
  currentPassword: string;
  newPassword: string;
}
