import { useTranslation } from "react-i18next";
import UsersTable from "@/features/users/components/UsersTable";

export default function UsersAdmin() {
  const { t } = useTranslation();

  return (
    <div className="space-y-6">
      <div>
        <h1 className="text-2xl font-semibold">{t("users.title", "HR Accounts")}</h1>
        <p className="text-sm text-muted-foreground mt-1">
          {t("users.subtitle", "Create and deactivate HR accounts. Super Admin only.")}
        </p>
      </div>
      <UsersTable />
    </div>
  );
}
