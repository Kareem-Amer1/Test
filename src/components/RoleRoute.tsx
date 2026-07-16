import { Navigate, Outlet } from "react-router-dom";
import { useTranslation } from "react-i18next";
import { useAuth } from "@/hooks/useAuth";
import type { UserRole } from "@/lib/apiClient";

interface RoleRouteProps {
  allowedRoles: UserRole[];
  redirectTo?: string;
}

export function RoleRoute({ allowedRoles, redirectTo = "/dashboard" }: RoleRouteProps) {
  const { user, loading, hasRole } = useAuth();
  const { t } = useTranslation();

  if (loading) {
    return (
      <div className="h-screen flex items-center justify-center text-sm text-muted-foreground">
        {t("common.loading", "Loading…")}
      </div>
    );
  }

  if (!user) return <Navigate to="/auth" replace />;
  if (!hasRole(allowedRoles)) return <Navigate to={redirectTo} replace />;

  return <Outlet />;
}
