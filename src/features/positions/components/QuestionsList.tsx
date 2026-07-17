import { useState } from "react";
import { useTranslation } from "react-i18next";
import { ChevronDown, ChevronUp, Pencil, Plus, Trash2 } from "lucide-react";
import { Button } from "@/components/ui/button";
import { Badge } from "@/components/ui/badge";
import { toast } from "sonner";
import {
  useAddQuestion,
  useDeleteQuestion,
  useReorderQuestions,
  useUpdateQuestion,
} from "../usePositions";
import type { TemplateQuestion, UpsertQuestionDto } from "../positions.types";
import { QuestionFormDialog } from "./QuestionFormDialog";

interface Props {
  positionId: string;
  partitionId: string;
  questions: TemplateQuestion[];
}

export function QuestionsList({ positionId, partitionId, questions }: Props) {
  const { t } = useTranslation();
  const addQuestion = useAddQuestion(positionId, partitionId);
  const updateQuestion = useUpdateQuestion(positionId, partitionId);
  const deleteQuestion = useDeleteQuestion(positionId, partitionId);
  const reorder = useReorderQuestions(positionId, partitionId);

  const [dialogOpen, setDialogOpen] = useState(false);
  const [editing, setEditing] = useState<TemplateQuestion | null>(null);

  const sorted = [...questions].sort((a, b) => a.order - b.order);

  const openAdd = () => {
    setEditing(null);
    setDialogOpen(true);
  };

  const openEdit = (q: TemplateQuestion) => {
    setEditing(q);
    setDialogOpen(true);
  };

  const handleSave = async (dto: UpsertQuestionDto) => {
    if (editing) {
      await updateQuestion.mutateAsync({ questionId: editing.id, dto });
      toast.success(t("templates.questionUpdated", "Question updated."));
    } else {
      await addQuestion.mutateAsync(dto);
      toast.success(t("templates.questionAdded", "Question added."));
    }
  };

  const onDelete = async (q: TemplateQuestion) => {
    if (!window.confirm(t("templates.confirmDelete", "Delete this question?"))) return;
    try {
      await deleteQuestion.mutateAsync(q.id);
      toast.success(t("templates.questionDeleted", "Question deleted."));
    } catch (err) {
      toast.error(err instanceof Error ? err.message : String(err));
    }
  };

  const move = async (index: number, direction: -1 | 1) => {
    const next = index + direction;
    if (next < 0 || next >= sorted.length) return;
    const ids = sorted.map((q) => q.id);
    [ids[index], ids[next]] = [ids[next], ids[index]];
    try {
      await reorder.mutateAsync(ids);
    } catch (err) {
      toast.error(err instanceof Error ? err.message : String(err));
    }
  };

  return (
    <div className="space-y-3">
      <div className="flex justify-end">
        <Button size="sm" variant="outline" onClick={openAdd}>
          <Plus className="h-4 w-4 me-1" />
          {t("templates.addQuestion", "Add question")}
        </Button>
      </div>

      {sorted.length === 0 ? (
        <p className="text-sm text-muted-foreground">{t("templates.noQuestions", "No questions yet.")}</p>
      ) : (
        <ul className="space-y-3">
          {sorted.map((q, index) => (
            <li
              key={q.id}
              className="rounded-lg border border-app-border-strong bg-card p-4 flex gap-3 items-start"
            >
              <div className="flex flex-col gap-1">
                <Button variant="ghost" size="icon" className="h-7 w-7" disabled={index === 0 || reorder.isPending} onClick={() => move(index, -1)}>
                  <ChevronUp className="h-4 w-4" />
                </Button>
                <Button variant="ghost" size="icon" className="h-7 w-7" disabled={index === sorted.length - 1 || reorder.isPending} onClick={() => move(index, 1)}>
                  <ChevronDown className="h-4 w-4" />
                </Button>
              </div>
              <div className="flex-1 min-w-0 space-y-1">
                <div className="flex flex-wrap items-center gap-2">
                  <Badge variant="secondary">{q.type}</Badge>
                  <span className="text-xs text-muted-foreground">{q.points} pts</span>
                </div>
                <p className="text-sm">{q.text}</p>
              </div>
              <div className="flex gap-1 shrink-0">
                <Button variant="ghost" size="icon" onClick={() => openEdit(q)}>
                  <Pencil className="h-4 w-4" />
                </Button>
                <Button variant="ghost" size="icon" className="text-destructive" onClick={() => onDelete(q)}>
                  <Trash2 className="h-4 w-4" />
                </Button>
              </div>
            </li>
          ))}
        </ul>
      )}

      <QuestionFormDialog
        open={dialogOpen}
        onOpenChange={setDialogOpen}
        initial={editing}
        onSave={handleSave}
      />
    </div>
  );
}
