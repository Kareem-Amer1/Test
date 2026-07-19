import { useCallback, useEffect, useMemo, useRef, useState, type ReactNode } from "react";
import { useTranslation } from "react-i18next";
import { ChevronLeft, ChevronRight } from "lucide-react";
import { Button } from "@/components/ui/button";
import { toast } from "sonner";
import { useSaveCandidateAnswers, useSubmitCandidateExam } from "@/features/invitations/useInvitations";
import { useElapsedExamTimer } from "@/features/exams/useExamTimer";
import { useExamLockdown } from "@/features/exams/useExamLockdown";
import { ExamTimerDisplay } from "@/features/exams/components/ExamTimerDisplay";
import { QuestionPanel } from "@/features/exams/components/QuestionPanel";
import type { ExamAnswerInput, ExamSession } from "@/features/exams/exams.types";

interface Props {
  token: string;
  session: ExamSession;
  onSubmitted: () => void;
}

export function CandidateExamSession({ token, session, onSubmitted }: Props) {
  const { t, i18n } = useTranslation();
  const saveAnswers = useSaveCandidateAnswers(token);
  const submitExam = useSubmitCandidateExam(token);

  const [currentIndex, setCurrentIndex] = useState(0);
  const [localAnswers, setLocalAnswers] = useState<ExamAnswerInput[]>([]);
  const submitLock = useRef(false);
  const saveTimer = useRef<number | null>(null);
  const pendingAnswers = useRef<ExamAnswerInput[] | null>(null);
  const answersInitialized = useRef(false);

  const inProgress = session.status === "InProgress";
  const dir = i18n.language === "ar" ? "rtl" : "ltr";

  useExamLockdown(inProgress);

  const { remainingMs, isExpired, formatted, currentElapsedSeconds } = useElapsedExamTimer(
    session.durationMinutes,
    session.elapsedSeconds,
    inProgress,
  );

  useEffect(() => {
    if (!session.answers || answersInitialized.current) return;
    setLocalAnswers(session.answers);
    answersInitialized.current = true;
  }, [session.answers]);

  const questions = useMemo(
    () => [...session.questions].sort((a, b) => a.order - b.order),
    [session.questions],
  );

  const currentQuestion = questions[currentIndex];
  const currentAnswer = localAnswers.find((a) => a.questionId === currentQuestion?.id);

  const persistAnswers = useCallback(
    async (answers: ExamAnswerInput[]) => {
      if (!inProgress) return false;
      try {
        await saveAnswers.mutateAsync({ answers, elapsedSeconds: currentElapsedSeconds });
        pendingAnswers.current = null;
        return true;
      } catch (err) {
        toast.error(err instanceof Error ? err.message : t("exams.saveError", "Failed to save answers."));
        return false;
      }
    },
    [inProgress, saveAnswers, currentElapsedSeconds, t],
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
      const next = prev.map((a) => (a.questionId === questionId ? { ...a, ...patch } : a));
      scheduleSave(next);
      return next;
    });
  };

  const handleSubmit = useCallback(async () => {
    if (submitLock.current || !inProgress) return;
    submitLock.current = true;
    try {
      await flushPendingSave();
      await submitExam.mutateAsync({ answers: localAnswers, elapsedSeconds: currentElapsedSeconds });
      toast.success(t("exams.submitted", "Exam submitted."));
      onSubmitted();
    } catch (err) {
      submitLock.current = false;
      toast.error(err instanceof Error ? err.message : String(err));
    }
  }, [inProgress, localAnswers, flushPendingSave, submitExam, currentElapsedSeconds, onSubmitted, t]);

  useEffect(() => {
    if (!inProgress || submitLock.current) return;
    if (isExpired) void handleSubmit();
  }, [isExpired, inProgress, handleSubmit]);

  useEffect(() => {
    const onLeave = () => {
      if (!pendingAnswers.current || !inProgress) return;
      void flushPendingSave();
    };
    window.addEventListener("pagehide", onLeave);
    return () => window.removeEventListener("pagehide", onLeave);
  }, [flushPendingSave, inProgress]);

  return (
    <div data-theme="dashboard" dir={dir} className="min-h-screen bg-muted/30 px-4 py-8">
      <div className="mx-auto max-w-3xl space-y-6">
        <div className="flex flex-wrap items-start justify-between gap-4">
          <div>
            <h1 className="text-xl font-semibold">{session.candidateName}</h1>
            <p className="text-sm text-muted-foreground">{session.positionName}</p>
            <p className="text-xs text-muted-foreground mt-2">
              {t("exams.candidateHint", "Your answers are saved automatically. You can reopen this link to resume.")}
            </p>
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
    </div>
  );
}

export function CandidateExamCompleteShell({ session }: { session: ExamSession }) {
  const { t } = useTranslation();
  return (
    <div className="min-h-screen bg-muted/30 px-4 py-8">
      <div className="max-w-lg mx-auto text-center space-y-4 py-12">
        <h1 className="text-2xl font-semibold">{t("exams.completeTitle", "Exam complete")}</h1>
        <p className="text-muted-foreground">
          {t("exams.candidateCompleteMessage", "Thank you, {{name}}. Your exam for {{position}} has been submitted.", {
            name: session.candidateName,
            position: session.positionName,
          })}
        </p>
        <p className="text-sm text-muted-foreground">
          {t("exams.candidateCompleteHint", "You may close this page. This link can no longer be used.")}
        </p>
      </div>
    </div>
  );
}

export function InvitePageShell({ children }: { children: ReactNode }) {
  return (
    <div className="min-h-screen bg-muted/30 px-4 py-8">
      <div className="mx-auto max-w-lg">{children}</div>
    </div>
  );
}
