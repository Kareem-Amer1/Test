import { useQuery } from "@tanstack/react-query";
import { dashboardApi } from "./dashboard.api";

export const DASHBOARD_STATS_KEY = ["dashboard", "stats"] as const;

export function useDashboardStats() {
  return useQuery({
    queryKey: DASHBOARD_STATS_KEY,
    queryFn: dashboardApi.getStats,
    staleTime: 30_000,
  });
}
