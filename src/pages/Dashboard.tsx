import { useTranslation } from "react-i18next";
import { useAuth } from "@/hooks/useAuth";
import DashboardStatsPanel from "@/features/dashboard/components/DashboardStatsPanel";

export default function Dashboard() {
  const { t } = useTranslation();
  const { user } = useAuth();

  return (
    <div className="space-y-6">
      <div>
        <h1 className="text-2xl font-semibold">{t("dashboard.title", "Dashboard")}</h1>
        <p className="text-sm text-muted-foreground mt-1">
          {t("dashboard.welcome", "Welcome to HireExam, {{name}}.", { name: user?.fullName || user?.email })}
        </p>
      </div>

      <DashboardStatsPanel />
    </div>
  );
}
