import { useTranslation } from "react-i18next";
import { Textarea } from "@/components/ui/textarea";
import { Label } from "@/components/ui/label";
import { RadioGroup, RadioGroupItem } from "@/components/ui/radio-group";
import { Badge } from "@/components/ui/badge";
import type { ExamAnswerInput, ExamSessionQuestion } from "../exams.types";

interface Props {
  question: ExamSessionQuestion;
  answer: ExamAnswerInput | undefined;
  onChange: (patch: Partial<ExamAnswerInput>) => void;
  readOnly?: boolean;
}

export function QuestionPanel({ question, answer, onChange, readOnly }: Props) {
  const { t } = useTranslation();

  return (
    <div className="space-y-4">
      <div className="flex flex-wrap items-center gap-2">
        <Badge variant="secondary">{question.type}</Badge>
        <span className="text-xs text-muted-foreground">{question.points} pts</span>
      </div>
      <p className="text-base font-medium leading-relaxed">{question.text}</p>

      {question.type === "Essay" && (
        <div className="space-y-1.5">
          <Label>{t("exams.yourAnswer", "Your answer")}</Label>
          <Textarea
            rows={6}
            value={answer?.essayText ?? ""}
            onChange={(e) => onChange({ essayText: e.target.value })}
            disabled={readOnly}
            placeholder={t("exams.essayPlaceholder", "Type your answer here…")}
          />
        </div>
      )}

      {question.type === "TrueFalse" && (
        <RadioGroup
          value={
            answer?.trueFalseAnswer === undefined
              ? ""
              : answer.trueFalseAnswer
                ? "true"
                : "false"
          }
          onValueChange={(v) => onChange({ trueFalseAnswer: v === "true" })}
          className="flex gap-6"
          disabled={readOnly}
        >
          <div className="flex items-center gap-2">
            <RadioGroupItem value="true" id={`tf-true-${question.id}`} disabled={readOnly} />
            <Label htmlFor={`tf-true-${question.id}`}>{t("common.true", "True")}</Label>
          </div>
          <div className="flex items-center gap-2">
            <RadioGroupItem value="false" id={`tf-false-${question.id}`} disabled={readOnly} />
            <Label htmlFor={`tf-false-${question.id}`}>{t("common.false", "False")}</Label>
          </div>
        </RadioGroup>
      )}

      {question.type === "MCQ" && (
        <RadioGroup
          value={answer?.selectedChoiceId ?? ""}
          onValueChange={(v) => onChange({ selectedChoiceId: v })}
          className="space-y-2"
          disabled={readOnly}
        >
          {(question.choices ?? []).map((c) => (
            <div key={c.id} className="flex items-center gap-2 rounded-md border border-app-border-strong p-3">
              <RadioGroupItem value={c.id} id={`mcq-${question.id}-${c.id}`} disabled={readOnly} />
              <Label htmlFor={`mcq-${question.id}-${c.id}`} className="flex-1 cursor-pointer">
                {c.text}
              </Label>
            </div>
          ))}
        </RadioGroup>
      )}
    </div>
  );
}
