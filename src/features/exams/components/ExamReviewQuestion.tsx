import { useTranslation } from "react-i18next";
import { Badge } from "@/components/ui/badge";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import type { ExamAnswerReview } from "../exams.types";

interface Props {
  question: ExamAnswerReview;
  essayScore?: number;
  onEssayScoreChange?: (points: number) => void;
  readOnly?: boolean;
}

export function ExamReviewQuestion({ question, essayScore, onEssayScoreChange, readOnly }: Props) {
  const { t } = useTranslation();
  const isEssay = question.questionType === "Essay";
  const showCorrectness = !isEssay && question.isCorrect !== null && question.isCorrect !== undefined;

  return (
    <div className="rounded-lg border border-app-border-strong bg-card p-5 space-y-4">
      {question.partitionName && (
        <p className="text-sm font-semibold text-primary">{question.partitionName}</p>
      )}
      <div className="flex flex-wrap items-center gap-2">
        <Badge variant="secondary">{question.questionType}</Badge>
        <span className="text-xs text-muted-foreground">{question.points} pts</span>
        {showCorrectness && (
          <Badge variant={question.isCorrect ? "default" : "destructive"}>
            {question.isCorrect
              ? t("exams.correct", "Correct")
              : t("exams.incorrect", "Incorrect")}
          </Badge>
        )}
        {question.earnedPoints !== null && question.earnedPoints !== undefined && (
          <span className="text-xs font-medium ms-auto">
            {t("exams.earned", "Earned")}: {question.earnedPoints} / {question.points}
          </span>
        )}
      </div>

      <p className="font-medium">{question.questionText}</p>

      <div className="grid gap-3 sm:grid-cols-2">
        <div className="space-y-1">
          <p className="text-xs font-medium text-muted-foreground uppercase tracking-wide">
            {t("exams.candidateAnswer", "Candidate answer")}
          </p>
          <p className="text-sm whitespace-pre-wrap">{question.candidateAnswer}</p>
        </div>
        {!isEssay && (
          <div className="space-y-1">
            <p className="text-xs font-medium text-muted-foreground uppercase tracking-wide">
              {t("exams.correctAnswer", "Correct answer")}
            </p>
            <p className="text-sm">{question.correctAnswer}</p>
          </div>
        )}
      </div>

      {isEssay && !readOnly && onEssayScoreChange && (
        <div className="space-y-1.5 max-w-[200px]">
          <Label htmlFor={`score-${question.questionId}`}>
            {t("exams.essayScore", "Essay score (0–{{max}})", { max: question.points })}
          </Label>
          <Input
            id={`score-${question.questionId}`}
            type="number"
            min={0}
            max={question.points}
            value={essayScore ?? question.earnedPoints ?? ""}
            onChange={(e) => onEssayScoreChange(Number(e.target.value))}
          />
        </div>
      )}
    </div>
  );
}
