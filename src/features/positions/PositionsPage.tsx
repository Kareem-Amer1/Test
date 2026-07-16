import { useTranslation } from "react-i18next";
import PositionsTable from "./components/PositionsTable";

export default function PositionsPage() {
  const { t } = useTranslation();

  return (
    <div className="space-y-6">
      <div>
        <h1 className="text-2xl font-semibold">{t("positions.title", "Positions")}</h1>
        <p className="text-sm text-muted-foreground mt-1">
          {t("positions.subtitle", "Manage job positions and their shared exam templates.")}
        </p>
      </div>
      <PositionsTable />
    </div>
  );
}
