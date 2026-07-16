import { useState } from "react";
import { useTranslation } from "react-i18next";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { toast } from "sonner";
import { useUpdateDuration } from "../usePositions";

interface Props {
  positionId: string;
  durationMinutes: number;
}

export function TemplateDurationForm({ positionId, durationMinutes }: Props) {
  const { t } = useTranslation();
  const [value, setValue] = useState(String(durationMinutes));
  const updateDuration = useUpdateDuration(positionId);

  const onSave = async () => {
    const n = Number(value);
    if (!Number.isFinite(n) || n < 1 || n > 480) {
      toast.error(t("templates.durationInvalid", "Duration must be between 1 and 480 minutes."));
      return;
    }
    try {
      await updateDuration.mutateAsync(n);
      toast.success(t("templates.durationSaved", "Duration updated."));
    } catch (err) {
      toast.error(err instanceof Error ? err.message : String(err));
    }
  };

  return (
    <div className="flex flex-wrap items-end gap-3">
      <div className="space-y-1.5">
        <Label htmlFor="duration">{t("templates.duration", "Exam duration (minutes)")}</Label>
        <Input
          id="duration"
          type="number"
          min={1}
          max={480}
          value={value}
          onChange={(e) => setValue(e.target.value)}
          className="w-32"
        />
      </div>
      <Button onClick={onSave} disabled={updateDuration.isPending}>
        {t("common.save", "Save")}
      </Button>
    </div>
  );
}
