import { Link } from "react-router-dom";
import { useTranslation } from "react-i18next";
import { ClipboardList, Clock, GraduationCap, ListChecks } from "lucide-react";
import { Button } from "@/components/ui/button";
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from "@/components/ui/table";
import { useDashboardStats } from "../useDashboard";

export default function DashboardStatsPanel() {
  const { t } = useTranslation();
  const { data, isLoading, isError, refetch } = useDashboardStats();

  if (isLoading) {
    return <p className="text-sm text-muted-foreground">{t("common.loading", "Loading…")}</p>;
  }

  if (isError || !data) {
    return (
      <div className="space-y-2">
        <p className="text-sm text-destructive">{t("dashboard.statsError", "Failed to load statistics.")}</p>
        <Button variant="outline" size="sm" onClick={() => refetch()}>{t("common.retry", "Retry")}</Button>
      </div>
    );
  }

  const cards = [
    {
      label: t("dashboard.totalExams", "Total exams"),
      value: data.totalExams,
      icon: ClipboardList,
    },
    {
      label: t("dashboard.pendingGrading", "Pending grading"),
      value: data.pendingGrading,
      icon: ListChecks,
    },
    {
      label: t("dashboard.inProgress", "In progress"),
      value: data.inProgress,
      icon: Clock,
    },
    {
      label: t("dashboard.graded", "Fully graded"),
      value: data.graded,
      icon: GraduationCap,
    },
  ];

  return (
    <div className="space-y-6">
      <div className="grid gap-4 sm:grid-cols-2 xl:grid-cols-4">
        {cards.map(({ label, value, icon: Icon }) => (
          <div key={label} className="rounded-lg border border-app-border-strong bg-card p-5">
            <div className="flex items-start justify-between gap-3">
              <div>
                <p className="text-xs text-muted-foreground uppercase tracking-wide">{label}</p>
                <p className="text-2xl font-semibold mt-1">{value}</p>
              </div>
              <Icon className="h-5 w-5 text-muted-foreground shrink-0" />
            </div>
          </div>
        ))}
      </div>

      <div className="rounded-lg border border-app-border-strong overflow-hidden">
        <div className="flex items-center justify-between gap-3 border-b border-app-border-strong px-4 py-3">
          <h2 className="text-sm font-medium">{t("dashboard.examsByPosition", "Exams by position")}</h2>
          <Button variant="outline" size="sm" asChild>
            <Link to="/exams">{t("dashboard.viewExams", "View exams")}</Link>
          </Button>
        </div>
        {(data.examsByPosition ?? []).length === 0 ? (
          <p className="p-6 text-sm text-muted-foreground text-center">
            {t("dashboard.noExamData", "No exams recorded yet.")}
          </p>
        ) : (
          <Table>
            <TableHeader>
              <TableRow>
                <TableHead>{t("exams.position", "Position")}</TableHead>
                <TableHead className="text-end">{t("dashboard.examCount", "Exams")}</TableHead>
              </TableRow>
            </TableHeader>
            <TableBody>
              {data.examsByPosition.map((row) => (
                <TableRow key={row.positionId}>
                  <TableCell>{row.positionName}</TableCell>
                  <TableCell className="text-end font-medium">{row.count}</TableCell>
                </TableRow>
              ))}
            </TableBody>
          </Table>
        )}
      </div>
    </div>
  );
}
