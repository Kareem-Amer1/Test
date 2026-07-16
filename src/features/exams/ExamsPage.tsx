import { useState } from "react";
import { Link } from "react-router-dom";
import { useTranslation } from "react-i18next";
import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from "@/components/ui/table";
import { usePositions } from "@/features/positions/usePositions";
import { useExamsList } from "./useExams";
import { ExamFilters } from "./components/ExamFilters";
import { formatDateTime, formatExamScore } from "./constants";
import type { ExamListFilters } from "./exams.types";

export default function ExamsPage() {
  const { t } = useTranslation();
  const [filters, setFilters] = useState<ExamListFilters>({});
  const { data: positions } = usePositions();
  const { data: exams, isLoading, isError, refetch } = useExamsList(filters);

  if (isLoading) {
    return <p className="text-sm text-muted-foreground">{t("common.loading", "Loading…")}</p>;
  }

  if (isError) {
    return (
      <div className="space-y-2">
        <p className="text-sm text-destructive">{t("exams.listError", "Failed to load exams.")}</p>
        <Button variant="outline" size="sm" onClick={() => refetch()}>{t("common.retry", "Retry")}</Button>
      </div>
    );
  }

  return (
    <div className="space-y-6">
      <div>
        <h1 className="text-2xl font-semibold">{t("exams.listTitle", "Exams")}</h1>
        <p className="text-sm text-muted-foreground mt-1">
          {t("exams.listSubtitle", "All conducted exams are stored permanently.")}
        </p>
      </div>

      <ExamFilters
        filters={filters}
        positions={positions ?? []}
        onChange={setFilters}
      />

      <div className="rounded-lg border border-app-border-strong overflow-hidden">
        <Table>
          <TableHeader>
            <TableRow>
              <TableHead>{t("exams.candidateName", "Candidate name")}</TableHead>
              <TableHead>{t("exams.position", "Position")}</TableHead>
              <TableHead>{t("common.status", "Status")}</TableHead>
              <TableHead>{t("exams.score", "Score")}</TableHead>
              <TableHead>{t("exams.submittedAt", "Submitted")}</TableHead>
              <TableHead>{t("exams.conductedBy", "Conducted by")}</TableHead>
            </TableRow>
          </TableHeader>
          <TableBody>
            {(exams ?? []).length === 0 ? (
              <TableRow>
                <TableCell colSpan={6} className="text-center text-muted-foreground py-8">
                  {t("exams.empty", "No exams yet.")}
                </TableCell>
              </TableRow>
            ) : (
              (exams ?? []).map((exam) => (
                <TableRow key={exam.id}>
                  <TableCell className="font-medium">
                    {exam.status === "InProgress" ? (
                      <Link to={`/exams/${exam.id}/session`} className="hover:underline">
                        {exam.candidateName}
                      </Link>
                    ) : (
                      <Link to={`/exams/${exam.id}`} className="hover:underline">
                        {exam.candidateName}
                      </Link>
                    )}
                  </TableCell>
                  <TableCell>{exam.positionName}</TableCell>
                  <TableCell>
                    <Badge variant={exam.status === "Graded" ? "default" : "secondary"}>
                      {exam.status}
                    </Badge>
                  </TableCell>
                  <TableCell>{formatExamScore(exam.totalScore, exam.maxScore)}</TableCell>
                  <TableCell>{formatDateTime(exam.submittedAt)}</TableCell>
                  <TableCell>{exam.conductedByName}</TableCell>
                </TableRow>
              ))
            )}
          </TableBody>
        </Table>
      </div>
    </div>
  );
}
