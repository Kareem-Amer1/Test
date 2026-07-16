import { useMemo, useState } from "react";
import { Link, useParams } from "react-router-dom";
import { useTranslation } from "react-i18next";
import { ArrowLeft } from "lucide-react";
import { Button } from "@/components/ui/button";
import { Badge } from "@/components/ui/badge";
import { toast } from "sonner";
import { useExamDetail, useGradeExam } from "./useExams";
import { ExamReviewQuestion } from "./components/ExamReviewQuestion";
import { formatDateTime, formatExamScore } from "./constants";
import type { EssayScoreInput } from "./exams.types";

export default function ExamDetailPage() {
  const { t } = useTranslation();
  const { examId = "" } = useParams<{ examId: string }>();
  const { data: exam, isLoading, isError, refetch } = useExamDetail(examId);
  const gradeExam = useGradeExam(examId);

  const essayQuestions = useMemo(
    () => (exam?.questions ?? []).filter((q) => q.questionType === "Essay"),
    [exam?.questions],
  );

  const [essayScores, setEssayScores] = useState<Record<string, number>>({});

  const resolvedScores = useMemo(() => {
    const map: Record<string, number> = {};
    for (const q of essayQuestions) {
      map[q.questionId] = essayScores[q.questionId] ?? q.earnedPoints ?? 0;
    }
    return map;
  }, [essayQuestions, essayScores]);

  const canGrade = exam && !exam.isFullyGraded && essayQuestions.length > 0;

  const handleSaveScores = async (finalize: boolean) => {
    if (!exam) return;
    const payload: EssayScoreInput[] = essayQuestions.map((q) => ({
      questionId: q.questionId,
      earnedPoints: resolvedScores[q.questionId] ?? 0,
    }));

    try {
      await gradeExam.mutateAsync({ essayScores: payload, finalize });
      toast.success(
        finalize
          ? t("exams.gradingComplete", "Exam grading finalized.")
          : t("exams.scoresSaved", "Essay scores saved."),
      );
    } catch (err) {
      toast.error(err instanceof Error ? err.message : String(err));
    }
  };

  if (isLoading) {
    return <p className="text-sm text-muted-foreground">{t("common.loading", "Loading…")}</p>;
  }

  if (isError || !exam) {
    return (
      <div className="space-y-2">
        <p className="text-sm text-destructive">{t("exams.detailError", "Failed to load exam.")}</p>
        <Button variant="outline" size="sm" onClick={() => refetch()}>{t("common.retry", "Retry")}</Button>
      </div>
    );
  }

  return (
    <div className="space-y-6 max-w-3xl">
      <div>
        <Button variant="ghost" size="sm" className="-ms-2 mb-2" asChild>
          <Link to="/exams">
            <ArrowLeft className="h-4 w-4 me-1" />
            {t("exams.backToList", "Back to exams")}
          </Link>
        </Button>
        <div className="flex flex-wrap items-start justify-between gap-3">
          <div>
            <h1 className="text-2xl font-semibold">{exam.candidateName}</h1>
            <p className="text-sm text-muted-foreground">{exam.positionName}</p>
            <p className="text-xs text-muted-foreground mt-1">
              {t("exams.conductedBy", "Conducted by")}: {exam.conductedByName}
            </p>
          </div>
          <div className="text-end space-y-1">
            <Badge>{exam.status}</Badge>
            <p className="text-sm font-medium">
              {formatExamScore(exam.totalScore, exam.maxScore)}
            </p>
            {exam.autoGradedScore > 0 && (
              <p className="text-xs text-muted-foreground">
                {t("exams.autoGraded", "Auto-graded")}: {exam.autoGradedScore}
              </p>
            )}
          </div>
        </div>
        <p className="text-xs text-muted-foreground mt-2">
          {t("exams.submittedAt", "Submitted")}: {formatDateTime(exam.submittedAt)}
        </p>
      </div>

      <div className="space-y-4">
        {exam.questions.map((q) => (
          <ExamReviewQuestion
            key={q.questionId}
            question={q}
            essayScore={resolvedScores[q.questionId]}
            onEssayScoreChange={
              canGrade
                ? (pts) => setEssayScores((prev) => ({ ...prev, [q.questionId]: pts }))
                : undefined
            }
            readOnly={!canGrade}
          />
        ))}
      </div>

      {canGrade && (
        <div className="flex flex-wrap gap-2">
          <Button
            variant="outline"
            disabled={gradeExam.isPending}
            onClick={() => void handleSaveScores(false)}
          >
            {t("exams.saveScores", "Save essay scores")}
          </Button>
          <Button
            disabled={gradeExam.isPending}
            onClick={() => {
              if (window.confirm(t("exams.confirmFinalize", "Finalize grading for this exam?"))) {
                void handleSaveScores(true);
              }
            }}
          >
            {t("exams.finalizeGrading", "Finalize grading")}
          </Button>
        </div>
      )}
    </div>
  );
}
