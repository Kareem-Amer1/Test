import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { notifyAuthChanged } from "@/hooks/useAuth";
import { profileApi } from "./profile.api";
import type { ChangePasswordDto, UpdateProfileDto } from "./profile.types";

export const PROFILE_KEY = ["profile", "me"] as const;

export function useProfile() {
  return useQuery({
    queryKey: PROFILE_KEY,
    queryFn: profileApi.getMe,
    staleTime: 60_000,
  });
}

export function useUpdateProfile() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (dto: UpdateProfileDto) => profileApi.updateProfile(dto),
    onSuccess: (data) => {
      qc.setQueryData(PROFILE_KEY, data);
      notifyAuthChanged();
    },
  });
}

export function useChangePassword() {
  return useMutation({
    mutationFn: (dto: ChangePasswordDto) => profileApi.changePassword(dto),
  });
}
