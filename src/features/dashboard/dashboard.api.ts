import { api } from "@/lib/apiClient";
import type { DashboardStats } from "./dashboard.types";

export const dashboardApi = {
  getStats: () => api.get<DashboardStats>("/dashboard/stats"),
};
