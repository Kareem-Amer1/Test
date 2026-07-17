import { useCallback, useEffect, useMemo, useRef, useState } from "react";
import { Link, useNavigate, useParams } from "react-router-dom";
import { useTranslation } from "react-i18next";
import { ArrowLeft, ChevronLeft, ChevronRight } from "lucide-react";
import { Button } from "@/components/ui/button";
import { toast } from "sonner";
import { useExamSession, useSaveExamAnswers, useSubmitExam } from "./useExams";
import { useExamTimer } from "./useExamTimer";
import { ExamTimerDisplay } from "./components/ExamTimerDisplay";
import { QuestionPanel } from "./components/QuestionPanel";
import type { ExamAnswerInput } from "./exams.types";

export default function ExamSessionPage() {
  const { t } = useTranslation();
  const navigate = useNavigate();
  const { examId = "" } = useParams<{ examId: string }>();
  const { data: session, isLoading, isError, error, refetch } = useExamSession(examId);
  const saveAnswers = useSaveExamAnswers(examId);
  const submitExam = useSubmitExam(examId);

  const [currentIndex, setCurrentIndex] = useState(0);
  const [localAnswers, setLocalAnswers] = useState<ExamAnswerInput[]>([]);
  const submitLock = useRef(false);
  const saveTimer = useRef<number | null>(null);
  const pendingAnswers = useRef<ExamAnswerInput[] | null>(null);
  const answersInitialized = useRef<string | null>(null);

  const inProgress = session?.status === "InProgress";
  const { remainingMs, isExpired, formatted } = useExamTimer(
    session?.startedAt,
    session?.durationMinutes,
    !!session && inProgress,
  );

  useEffect(() => {
    if (!session?.answers || answersInitialized.current === examId) return;
    setLocalAnswers(session.answers);
    answersInitialized.current = examId;
  }, [examId, session?.answers]);

  useEffect(() => {
    answersInitialized.current = null;
    setLocalAnswers([]);
    setCurrentIndex(0);
    submitLock.current = false;
  }, [examId]);

  const questions = useMemo(
    () => [...(session?.questions ?? [])].sort((a, b) => a.order - b.order),
    [session?.questions],
  );

  const currentQuestion = questions[currentIndex];
  const currentAnswer = localAnswers.find((a) => a.questionId === currentQuestion?.id);

  const persistAnswers = useCallback(
    async (answers: ExamAnswerInput[]) => {
      if (!inProgress) return false;
      try {
        await saveAnswers.mutateAsync(answers);
        pendingAnswers.current = null;
        return true;
      } catch (err) {
        toast.error(err instanceof Error ? err.message : t("exams.saveError", "Failed to save answers."));
        return false;
      }
    },
    [inProgress, saveAnswers, t],
  );

  const flushPendingSave = useCallback(async () => {
    if (saveTimer.current) {
      window.clearTimeout(saveTimer.current);
      saveTimer.current = null;
    }
    const pending = pendingAnswers.current;
    if (!pending) return true;
    return persistAnswers(pending);
  }, [persistAnswers]);

  const scheduleSave = useCallback(
    (answers: ExamAnswerInput[]) => {
      pendingAnswers.current = answers;
      if (saveTimer.current) window.clearTimeout(saveTimer.current);
      saveTimer.current = window.setTimeout(() => void persistAnswers(answers), 800);
    },
    [persistAnswers],
  );

  const updateAnswer = (questionId: string, patch: Partial<ExamAnswerInput>) => {
    setLocalAnswers((prev) => {
      const next = prev.map((a) =>
        a.questionId === questionId ? { ...a, ...patch } : a,
      );
      scheduleSave(next);
      return next;
    });
  };

  const handleSubmit = useCallback(async () => {
    if (submitLock.current || !inProgress) return;
    submitLock.current = true;
    try {
      await flushPendingSave();
      await submitExam.mutateAsync(localAnswers);
      toast.success(t("exams.submitted", "Exam submitted."));
    } catch (err) {
      submitLock.current = false;
      toast.error(err instanceof Error ? err.message : String(err));
    }
  }, [inProgress, localAnswers, flushPendingSave, submitExam, t]);

  useEffect(() => {
    if (isLoading || !session || !inProgress || submitLock.current) return;
    if (isExpired) void handleSubmit();
  }, [isLoading, session, isExpired, inProgress, handleSubmit]);

  useEffect(() => {
    const onLeave = () => {
      if (!pendingAnswers.current || !inProgress) return;
      void flushPendingSave();
    };
    window.addEventListener("beforeunload", onLeave);
    return () => window.removeEventListener("beforeunload", onLeave);
  }, [flushPendingSave, inProgress]);

  if (isLoading) {
    return <p className="text-sm text-muted-foreground">{t("common.loading", "Loading…")}</p>;
  }

  if (isError || !session) {
    const apiError = error as (Error & { code?: string; status?: number }) | undefined;
    const forbidden = apiError?.code === "exams.forbidden" || apiError?.status === 403;
    return (
      <div className="space-y-2">
        <p className="text-sm text-destructive">
          {forbidden
            ? t("exams.forbidden", "You do not have access to this exam.")
            : t("exams.loadError", "Failed to load exam.")}
        </p>
        <Button variant="outline" size="sm" onClick={() => refetch()}>{t("common.retry", "Retry")}</Button>
      </div>
    );
  }

  if (session.status === "Submitted" || session.status === "Graded") {
    return (
      <div className="max-w-lg mx-auto text-center space-y-4 py-12">
        <h1 className="text-2xl font-semibold">{t("exams.completeTitle", "Exam complete")}</h1>
        <p className="text-muted-foreground">
          {t("exams.completeMessage", "{{name}}'s exam for {{position}} has been submitted.", {
            name: session.candidateName,
            position: session.positionName,
          })}
        </p>
        <div className="flex flex-wrap justify-center gap-2">
          <Button asChild variant="outline">
            <Link to={`/exams/${session.id}`}>{t("exams.viewReview", "View review")}</Link>
          </Button>
          <Button asChild variant="outline">
            <Link to="/exams">{t("exams.viewAll", "View all exams")}</Link>
          </Button>
          <Button asChild>
            <Link to="/positions">{t("exams.backToPositions", "Back to positions")}</Link>
          </Button>
        </div>
      </div>
    );
  }

  return (
    <div className="space-y-6 max-w-3xl mx-auto">
      <div className="flex flex-wrap items-start justify-between gap-4">
        <div>
          <Button variant="ghost" size="sm" className="-ms-2 mb-2" onClick={() => navigate("/positions")}>
            <ArrowLeft className="h-4 w-4 me-1" />
            {t("common.back", "Back")}
          </Button>
          <h1 className="text-xl font-semibold">{session.candidateName}</h1>
          <p className="text-sm text-muted-foreground">{session.positionName}</p>
        </div>
        <ExamTimerDisplay
          formatted={formatted}
          isExpired={isExpired}
          urgent={remainingMs > 0 && remainingMs <= 5 * 60 * 1000}
        />
      </div>

      <div className="flex items-center justify-between text-sm text-muted-foreground">
        <span>
          {t("exams.questionProgress", "Question {{current}} of {{total}}", {
            current: currentIndex + 1,
            total: questions.length,
          })}
        </span>
        {(saveAnswers.isPending || pendingAnswers.current) && (
          <span>{t("exams.saving", "Saving…")}</span>
        )}
      </div>

      {currentQuestion && (
        <div className="rounded-lg border border-app-border-strong bg-card p-6">
          <QuestionPanel
            question={currentQuestion}
            answer={currentAnswer}
            onChange={(patch) => updateAnswer(currentQuestion.id, patch)}
            readOnly={!inProgress}
          />
        </div>
      )}

      <div className="flex flex-wrap items-center justify-between gap-3">
        <div className="flex gap-2">
          <Button
            variant="outline"
            size="sm"
            disabled={currentIndex === 0}
            onClick={() => setCurrentIndex((i) => i - 1)}
          >
            <ChevronLeft className="h-4 w-4 me-1" />
            {t("common.previous", "Previous")}
          </Button>
          <Button
            variant="outline"
            size="sm"
            disabled={currentIndex >= questions.length - 1}
            onClick={() => setCurrentIndex((i) => i + 1)}
          >
            {t("common.next", "Next")}
            <ChevronRight className="h-4 w-4 ms-1" />
          </Button>
        </div>
        <Button
          onClick={() => {
            if (window.confirm(t("exams.confirmSubmit", "Submit exam now?"))) void handleSubmit();
          }}
          disabled={submitExam.isPending || saveAnswers.isPending}
        >
          {t("exams.submit", "Submit exam")}
        </Button>
      </div>
    </div>
  );
}
