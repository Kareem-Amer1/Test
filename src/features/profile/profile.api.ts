import { api } from "@/lib/apiClient";
import type { ChangePasswordDto, UpdateProfileDto, UserProfile } from "./profile.types";

export const profileApi = {
  getMe: () => api.get<UserProfile>("/auth/me"),
  updateProfile: (dto: UpdateProfileDto) => api.put<UserProfile>("/auth/me", dto),
  changePassword: (dto: ChangePasswordDto) => api.put<boolean>("/auth/me/password", dto),
};
