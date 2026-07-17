import { useState } from "react";
import { useTranslation } from "react-i18next";
import { Pencil, Plus, Trash2 } from "lucide-react";
import { Button } from "@/components/ui/button";
import {
  Dialog,
  DialogContent,
  DialogFooter,
  DialogHeader,
  DialogTitle,
} from "@/components/ui/dialog";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";

interface Props {
  open: boolean;
  onOpenChange: (open: boolean) => void;
  initialName?: string;
  onSave: (name: string) => Promise<void>;
}

export function PartitionFormDialog({ open, onOpenChange, initialName = "", onSave }: Props) {
  const { t } = useTranslation();
  const [name, setName] = useState(initialName);
  const [saving, setSaving] = useState(false);
  const isEdit = !!initialName;

  const handleOpenChange = (next: boolean) => {
    if (next) setName(initialName);
    onOpenChange(next);
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    const trimmed = name.trim();
    if (trimmed.length < 2) return;
    setSaving(true);
    try {
      await onSave(trimmed);
      onOpenChange(false);
      setName("");
    } finally {
      setSaving(false);
    }
  };

  return (
    <Dialog open={open} onOpenChange={handleOpenChange}>
      <DialogContent>
        <form onSubmit={handleSubmit}>
          <DialogHeader>
            <DialogTitle>
              {isEdit
                ? t("templates.editPartition", "Rename partition")
                : t("templates.addPartition", "Add partition")}
            </DialogTitle>
          </DialogHeader>
          <div className="py-4">
            <Label htmlFor="partition-name">{t("templates.partitionName", "Partition name")}</Label>
            <Input
              id="partition-name"
              value={name}
              onChange={(e) => setName(e.target.value)}
              placeholder={t("templates.partitionNamePlaceholder", "e.g. Soft Skills")}
              className="mt-2"
              autoFocus
            />
          </div>
          <DialogFooter>
            <Button type="button" variant="outline" onClick={() => onOpenChange(false)}>
              {t("common.cancel", "Cancel")}
            </Button>
            <Button type="submit" disabled={saving || name.trim().length < 2}>
              {t("common.save", "Save")}
            </Button>
          </DialogFooter>
        </form>
      </DialogContent>
    </Dialog>
  );
}

interface AddButtonProps {
  onClick: () => void;
}

export function AddPartitionButton({ onClick }: AddButtonProps) {
  const { t } = useTranslation();
  return (
    <Button size="sm" onClick={onClick}>
      <Plus className="h-4 w-4 me-1" />
      {t("templates.addPartition", "Add partition")}
    </Button>
  );
}

interface PartitionActionsProps {
  onEdit: () => void;
  onDelete: () => void;
}

export function PartitionActions({ onEdit, onDelete }: PartitionActionsProps) {
  const { t } = useTranslation();
  return (
    <div className="flex gap-1 shrink-0">
      <Button variant="ghost" size="icon" onClick={onEdit} title={t("templates.editPartition", "Rename partition")}>
        <Pencil className="h-4 w-4" />
      </Button>
      <Button
        variant="ghost"
        size="icon"
        className="text-destructive"
        onClick={onDelete}
        title={t("templates.deletePartition", "Delete partition")}
      >
        <Trash2 className="h-4 w-4" />
      </Button>
    </div>
  );
}
