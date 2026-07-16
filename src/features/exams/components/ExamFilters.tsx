import { useTranslation } from "react-i18next";
import { Label } from "@/components/ui/label";
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/components/ui/select";
import { Input } from "@/components/ui/input";
import { Button } from "@/components/ui/button";
import { EXAM_STATUS_OPTIONS } from "../constants";
import type { ExamListFilters } from "../exams.types";
import type { Position } from "@/features/positions/positions.types";

interface Props {
  filters: ExamListFilters;
  positions: Position[];
  onChange: (filters: ExamListFilters) => void;
}

export function ExamFilters({ filters, positions, onChange }: Props) {
  const { t } = useTranslation();

  return (
    <div className="flex flex-wrap items-end gap-3 rounded-lg border border-app-border-strong p-4">
      <div className="space-y-1.5 min-w-[160px]">
        <Label>{t("exams.position", "Position")}</Label>
        <Select
          value={filters.positionId ?? "all"}
          onValueChange={(v) => onChange({ ...filters, positionId: v === "all" ? undefined : v })}
        >
          <SelectTrigger>
            <SelectValue placeholder={t("exams.allPositions", "All positions")} />
          </SelectTrigger>
          <SelectContent>
            <SelectItem value="all">{t("exams.allPositions", "All positions")}</SelectItem>
            {positions.map((p) => (
              <SelectItem key={p.id} value={p.id}>{p.name}</SelectItem>
            ))}
          </SelectContent>
        </Select>
      </div>

      <div className="space-y-1.5 min-w-[140px]">
        <Label>{t("common.status", "Status")}</Label>
        <Select
          value={filters.status ?? "all"}
          onValueChange={(v) =>
            onChange({ ...filters, status: v === "all" ? undefined : (v as ExamListFilters["status"]) })
          }
        >
          <SelectTrigger>
            <SelectValue />
          </SelectTrigger>
          <SelectContent>
            {EXAM_STATUS_OPTIONS.map((opt) => (
              <SelectItem key={opt.value || "all"} value={opt.value || "all"}>
                {t(`exams.status.${opt.value || "all"}`, opt.label)}
              </SelectItem>
            ))}
          </SelectContent>
        </Select>
      </div>

      <div className="space-y-1.5">
        <Label>{t("exams.fromDate", "From")}</Label>
        <Input
          type="date"
          value={filters.from ?? ""}
          onChange={(e) => onChange({ ...filters, from: e.target.value || undefined })}
          className="w-[160px]"
        />
      </div>

      <div className="space-y-1.5">
        <Label>{t("exams.toDate", "To")}</Label>
        <Input
          type="date"
          value={filters.to ?? ""}
          onChange={(e) => onChange({ ...filters, to: e.target.value || undefined })}
          className="w-[160px]"
        />
      </div>

      <Button
        variant="outline"
        onClick={() => onChange({})}
      >
        {t("common.clear", "Clear")}
      </Button>
    </div>
  );
}
