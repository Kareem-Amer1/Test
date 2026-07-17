import { api } from "@/lib/apiClient";
import type { CreateHrUserDto, HrUser } from "./users.types";

export const usersApi = {
  list: () => api.get<HrUser[]>("/users"),
  create: (dto: CreateHrUserDto) => api.post<HrUser>("/users", dto),
  deactivate: (id: string) => api.deleteRaw(`/users/${id}`),
};
