import { useTranslation } from "react-i18next";
import { useAuth } from "@/hooks/useAuth";

export default function Dashboard() {
  const { t } = useTranslation();
  const { user, isSuperAdmin } = useAuth();

  return (
    <div className="space-y-6">
      <div>
        <h1 className="text-2xl font-semibold">{t("dashboard.title", "Dashboard")}</h1>
        <p className="text-sm text-muted-foreground mt-1">
          {t("dashboard.welcome", "Welcome to HireExam, {{name}}.", { name: user?.fullName || user?.email })}
        </p>
      </div>

      <div className="grid gap-4 sm:grid-cols-2 lg:grid-cols-3">
        <div className="rounded-lg border border-app-border-strong bg-card p-5">
          <p className="text-xs text-muted-foreground uppercase tracking-wide">{t("dashboard.role", "Role")}</p>
          <p className="text-lg font-semibold mt-1">{user?.role}</p>
        </div>
        <div className="rounded-lg border border-app-border-strong bg-card p-5">
          <p className="text-xs text-muted-foreground uppercase tracking-wide">{t("dashboard.email", "Email")}</p>
          <p className="text-lg font-semibold mt-1 truncate">{user?.email}</p>
        </div>
        <div className="rounded-lg border border-app-border-strong bg-card p-5">
          <p className="text-xs text-muted-foreground uppercase tracking-wide">{t("dashboard.access", "Access")}</p>
          <p className="text-lg font-semibold mt-1">
            {isSuperAdmin
              ? t("dashboard.superAdminAccess", "All exams & HR management")
              : t("dashboard.hrAccess", "Conduct & review your exams")}
          </p>
        </div>
      </div>

      <div className="rounded-lg border border-dashed border-app-border-strong bg-muted/30 p-6 text-sm text-muted-foreground">
        {t(
          "dashboard.phase1Note",
          "Phase 1 is complete — authentication and role-based access are active. Position, template, and exam features arrive in the next phases.",
        )}
      </div>
    </div>
  );
}
