import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { usersApi } from "./users.api";
import type { CreateHrUserDto } from "./users.types";

export const USERS_KEY = ["users", "hr"] as const;

export function useHrUsers() {
  return useQuery({
    queryKey: USERS_KEY,
    queryFn: usersApi.list,
    staleTime: 30_000,
  });
}

export function useCreateHrUser() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (dto: CreateHrUserDto) => usersApi.create(dto),
    onSuccess: () => qc.invalidateQueries({ queryKey: USERS_KEY }),
  });
}

export function useDeactivateHrUser() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (id: string) => usersApi.deactivate(id),
    onSuccess: () => qc.invalidateQueries({ queryKey: USERS_KEY }),
  });
}
