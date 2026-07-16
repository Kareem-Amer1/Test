import { useTranslation } from "react-i18next";
import { useAuth } from "@/hooks/useAuth";

export default function UsersAdmin() {
  const { t } = useTranslation();
  const { user } = useAuth();

  return (
    <div className="space-y-4">
      <div>
        <h1 className="text-2xl font-semibold">{t("users.title", "HR Accounts")}</h1>
        <p className="text-sm text-muted-foreground mt-1">
          {t("users.superAdminOnly", "Super Admin only — full HR management arrives in a later phase.")}
        </p>
      </div>
      <div className="rounded-lg border border-app-border-strong bg-card p-6 text-sm text-muted-foreground">
        {t("users.signedInAs", "Signed in as")}: {user?.fullName || user?.email} ({user?.role})
      </div>
    </div>
  );
}
