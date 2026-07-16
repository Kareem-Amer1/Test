import { useForm } from "react-hook-form";
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
import { toast } from "sonner";
import { useCreateExam } from "../useExams";
import type { Position } from "@/features/positions/positions.types";

const schema = z.object({
  candidateName: z.string().min(2, "At least 2 characters"),
});

type FormData = z.infer<typeof schema>;

interface Props {
  position: Position | null;
  open: boolean;
  onOpenChange: (open: boolean) => void;
  onCreated: (examId: string) => void;
}

export function StartExamDialog({ position, open, onOpenChange, onCreated }: Props) {
  const { t } = useTranslation();
  const createExam = useCreateExam();
  const { register, handleSubmit, reset, formState: { errors } } = useForm<FormData>({
    resolver: zodResolver(schema),
    defaultValues: { candidateName: "" },
  });

  const onSubmit = handleSubmit(async (values) => {
    if (!position) return;
    try {
      const result = await createExam.mutateAsync({
        positionId: position.id,
        candidateName: values.candidateName.trim(),
      });
      reset();
      onOpenChange(false);
      onCreated(result.id);
    } catch (err) {
      toast.error(err instanceof Error ? err.message : String(err));
    }
  });

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent>
        <DialogHeader>
          <DialogTitle>{t("exams.startTitle", "Start exam")}</DialogTitle>
        </DialogHeader>
        {position && (
          <p className="text-sm text-muted-foreground">
            {t("exams.startFor", "Position")}: <strong>{position.name}</strong>
          </p>
        )}
        <form onSubmit={onSubmit} className="space-y-4">
          <div className="space-y-1.5">
            <Label htmlFor="candidate">{t("exams.candidateName", "Candidate name")}</Label>
            <Input id="candidate" {...register("candidateName")} autoFocus />
            {errors.candidateName && (
              <p className="text-xs text-destructive">{errors.candidateName.message}</p>
            )}
          </div>
          <DialogFooter>
            <Button type="button" variant="outline" onClick={() => onOpenChange(false)}>
              {t("common.cancel", "Cancel")}
            </Button>
            <Button type="submit" disabled={createExam.isPending || !position}>
              {t("exams.start", "Start exam")}
            </Button>
          </DialogFooter>
        </form>
      </DialogContent>
    </Dialog>
  );
}
