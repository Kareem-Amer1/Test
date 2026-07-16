import { useEffect, useState } from "react";
import { useForm, useFieldArray, Controller } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { z } from "zod";
import { useTranslation } from "react-i18next";
import {
  Dialog,
  DialogContent,
  DialogFooter,
  DialogHeader,
  DialogTitle,
} from "@/components/ui/dialog";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { Textarea } from "@/components/ui/textarea";
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/components/ui/select";
import { RadioGroup, RadioGroupItem } from "@/components/ui/radio-group";
import { toast } from "sonner";
import { QUESTION_TYPES, DEFAULT_QUESTION_POINTS } from "../constants";
import type { QuestionType, TemplateQuestion, UpsertQuestionDto } from "../positions.types";

const choiceSchema = z.object({ id: z.string().optional(), text: z.string() });

const schema = z.object({
  type: z.enum(["Essay", "TrueFalse", "MCQ"]),
  text: z.string().min(3),
  points: z.coerce.number().min(1),
  correctAnswer: z.boolean().optional(),
  choices: z.array(choiceSchema).optional(),
});

type FormData = z.infer<typeof schema>;

interface Props {
  open: boolean;
  onOpenChange: (open: boolean) => void;
  initial?: TemplateQuestion | null;
  onSave: (dto: UpsertQuestionDto) => Promise<void>;
}

export function QuestionFormDialog({ open, onOpenChange, initial, onSave }: Props) {
  const { t } = useTranslation();
  const isEdit = !!initial;
  const [correctChoiceIndex, setCorrectChoiceIndex] = useState(0);

  const { register, control, handleSubmit, watch, reset, formState: { errors, isSubmitting } } = useForm<FormData>({
    resolver: zodResolver(schema),
    defaultValues: {
      type: "Essay",
      text: "",
      points: DEFAULT_QUESTION_POINTS,
      choices: [{ text: "" }, { text: "" }],
    },
  });

  const type = watch("type") as QuestionType;
  const { fields, append, remove } = useFieldArray({ control, name: "choices" });

  useEffect(() => {
    if (!open) return;
    if (initial) {
      const choices = initial.choices?.map((c) => ({ id: c.id, text: c.text })) ?? [{ text: "" }, { text: "" }];
      const correctIdx = initial.correctChoiceId
        ? Math.max(0, choices.findIndex((c) => c.id === initial.correctChoiceId))
        : 0;
      setCorrectChoiceIndex(correctIdx);
      reset({
        type: initial.type,
        text: initial.text,
        points: initial.points,
        correctAnswer: initial.correctAnswer ?? undefined,
        choices,
      });
    } else {
      setCorrectChoiceIndex(0);
      reset({
        type: "Essay",
        text: "",
        points: DEFAULT_QUESTION_POINTS,
        choices: [{ text: "" }, { text: "" }],
      });
    }
  }, [open, initial, reset]);

  const onSubmit = handleSubmit(async (values) => {
    const dto: UpsertQuestionDto = {
      type: values.type,
      text: values.text.trim(),
      points: values.points,
    };

    if (values.type === "TrueFalse") {
      if (values.correctAnswer === undefined) {
        toast.error(t("templates.correctAnswerRequired", "Select the correct answer."));
        return;
      }
      dto.correctAnswer = values.correctAnswer;
    }

    if (values.type === "MCQ") {
      const raw = (values.choices ?? []).map((c) => c.text.trim()).filter(Boolean);
      if (raw.length < 2) {
        toast.error(t("templates.choicesRequired", "Add at least 2 choices."));
        return;
      }
      const choices = raw.map((text, i) => ({
        id: values.choices?.[i]?.id,
        text,
      }));
      const correctIdx = Math.min(correctChoiceIndex, choices.length - 1);
      dto.choices = choices.map((c, i) => ({
        id: c.id ?? `new-${i}`,
        text: c.text,
      }));
      dto.correctChoiceId = dto.choices[correctIdx].id;
    }

    try {
      await onSave(dto);
      onOpenChange(false);
    } catch (err) {
      toast.error(err instanceof Error ? err.message : String(err));
    }
  });

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent className="max-w-lg max-h-[90vh] overflow-y-auto">
        <DialogHeader>
          <DialogTitle>
            {isEdit ? t("templates.editQuestion", "Edit question") : t("templates.addQuestion", "Add question")}
          </DialogTitle>
        </DialogHeader>
        <form onSubmit={onSubmit} className="space-y-4">
          <div className="space-y-1.5">
            <Label>{t("templates.questionType", "Type")}</Label>
            <Controller
              name="type"
              control={control}
              render={({ field }) => (
                <Select value={field.value} onValueChange={field.onChange}>
                  <SelectTrigger><SelectValue /></SelectTrigger>
                  <SelectContent>
                    {QUESTION_TYPES.map((qt) => (
                      <SelectItem key={qt.value} value={qt.value}>{qt.label}</SelectItem>
                    ))}
                  </SelectContent>
                </Select>
              )}
            />
          </div>
          <div className="space-y-1.5">
            <Label>{t("templates.questionText", "Question")}</Label>
            <Textarea rows={3} {...register("text")} />
            {errors.text && <p className="text-xs text-destructive">{errors.text.message}</p>}
          </div>
          <div className="space-y-1.5">
            <Label>{t("templates.points", "Points")}</Label>
            <Input type="number" min={1} {...register("points")} className="w-24" />
          </div>

          {type === "TrueFalse" && (
            <div className="space-y-1.5">
              <Label>{t("templates.correctAnswer", "Correct answer")}</Label>
              <Controller
                name="correctAnswer"
                control={control}
                render={({ field }) => (
                  <RadioGroup
                    value={field.value === undefined ? "" : field.value ? "true" : "false"}
                    onValueChange={(v) => field.onChange(v === "true")}
                    className="flex gap-4"
                  >
                    <div className="flex items-center gap-2">
                      <RadioGroupItem value="true" id="tf-true" />
                      <Label htmlFor="tf-true">{t("common.true", "True")}</Label>
                    </div>
                    <div className="flex items-center gap-2">
                      <RadioGroupItem value="false" id="tf-false" />
                      <Label htmlFor="tf-false">{t("common.false", "False")}</Label>
                    </div>
                  </RadioGroup>
                )}
              />
            </div>
          )}

          {type === "MCQ" && (
            <div className="space-y-3">
              <Label>{t("templates.choices", "Choices")}</Label>
              {fields.map((field, index) => (
                <div key={field.id} className="flex gap-2 items-center">
                  <input
                    type="radio"
                    name="correctChoice"
                    checked={correctChoiceIndex === index}
                    onChange={() => setCorrectChoiceIndex(index)}
                    title={t("templates.markCorrect", "Mark as correct")}
                  />
                  <Input {...register(`choices.${index}.text` as const)} placeholder={`Choice ${index + 1}`} />
                  {fields.length > 2 && (
                    <Button type="button" variant="ghost" size="sm" onClick={() => remove(index)}>×</Button>
                  )}
                </div>
              ))}
              <Button type="button" variant="outline" size="sm" onClick={() => append({ text: "" })}>
                {t("templates.addChoice", "Add choice")}
              </Button>
            </div>
          )}

          <DialogFooter>
            <Button type="button" variant="outline" onClick={() => onOpenChange(false)}>
              {t("common.cancel", "Cancel")}
            </Button>
            <Button type="submit" disabled={isSubmitting}>{t("common.save", "Save")}</Button>
          </DialogFooter>
        </form>
      </DialogContent>
    </Dialog>
  );
}
