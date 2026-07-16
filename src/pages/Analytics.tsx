import { useTranslation } from "react-i18next";

export default function Analytics() {
  const { t } = useTranslation();

  return (
    <div className="flex-1 p-6 space-y-6">
      <div>
        <h2 className="text-xl font-bold">{t("nav.analytics", "Analytics")}</h2>
        <p className="text-sm text-muted-foreground mt-1">Deep-dive reports and insights.</p>
      </div>

      <div className="bg-card rounded-xl border border-app-border-strong p-12 flex flex-col items-center justify-center text-center gap-3">
        <div className="h-16 w-16 rounded-2xl bg-muted flex items-center justify-center text-muted-foreground text-3xl">
          📊
        </div>
        <p className="font-medium">Analytics coming soon</p>
        <p className="text-sm text-muted-foreground max-w-xs">
          Build out your analytics views here. Connect your data sources and render charts using the recharts components already installed.
        </p>
      </div>
    </div>
  );
}
