import { useState } from "react";
import { useTranslation } from "react-i18next";
import { toast } from "sonner";
import {
  useAddPartition,
  useDeletePartition,
  useUpdatePartition,
} from "../usePositions";
import type { TemplatePartition } from "../positions.types";
import { QuestionsList } from "./QuestionsList";
import {
  AddPartitionButton,
  PartitionActions,
  PartitionFormDialog,
} from "./PartitionFormDialog";

interface Props {
  positionId: string;
  partitions: TemplatePartition[];
}

export function PartitionsList({ positionId, partitions }: Props) {
  const { t } = useTranslation();
  const addPartition = useAddPartition(positionId);
  const updatePartition = useUpdatePartition(positionId);
  const deletePartition = useDeletePartition(positionId);

  const [dialogOpen, setDialogOpen] = useState(false);
  const [editing, setEditing] = useState<TemplatePartition | null>(null);

  const sorted = [...partitions].sort((a, b) => a.order - b.order);

  const handleCreate = async (name: string) => {
    await addPartition.mutateAsync({ name });
    toast.success(t("templates.partitionAdded", "Partition added."));
  };

  const handleRename = async (name: string) => {
    if (!editing) return;
    await updatePartition.mutateAsync({ partitionId: editing.id, dto: { name } });
    toast.success(t("templates.partitionUpdated", "Partition updated."));
  };

  const onDelete = async (partition: TemplatePartition) => {
    const message =
      partition.questions.length > 0
        ? t(
            "templates.confirmDeletePartitionWithQuestions",
            "Delete partition \"{{name}}\" and all {{count}} questions inside it?",
            { name: partition.name, count: partition.questions.length },
          )
        : t("templates.confirmDeletePartition", "Delete partition \"{{name}}\"?", {
            name: partition.name,
          });

    if (!window.confirm(message)) return;

    try {
      await deletePartition.mutateAsync(partition.id);
      toast.success(t("templates.partitionDeleted", "Partition deleted."));
    } catch (err) {
      toast.error(err instanceof Error ? err.message : String(err));
    }
  };

  return (
    <div className="space-y-4">
      <div className="flex justify-between items-center">
        <h2 className="text-lg font-semibold">{t("templates.partitions", "Partitions")}</h2>
        <AddPartitionButton onClick={() => { setEditing(null); setDialogOpen(true); }} />
      </div>

      {sorted.length === 0 ? (
        <p className="text-sm text-muted-foreground">
          {t("templates.noPartitions", "No partitions yet. Add a partition, then add questions to it.")}
        </p>
      ) : (
        <div className="space-y-6">
          {sorted.map((partition) => (
            <section
              key={partition.id}
              className="rounded-lg border border-app-border-strong bg-card/50 p-5 space-y-4"
            >
              <div className="flex items-start justify-between gap-3">
                <div>
                  <h3 className="text-base font-semibold">{partition.name}</h3>
                  <p className="text-xs text-muted-foreground">
                    {t("templates.questionCount", "{{count}} questions", {
                      count: partition.questions.length,
                    })}
                  </p>
                </div>
                <PartitionActions
                  onEdit={() => { setEditing(partition); setDialogOpen(true); }}
                  onDelete={() => void onDelete(partition)}
                />
              </div>
              <QuestionsList
                positionId={positionId}
                partitionId={partition.id}
                questions={partition.questions}
              />
            </section>
          ))}
        </div>
      )}

      <PartitionFormDialog
        key={editing?.id ?? "new"}
        open={dialogOpen}
        onOpenChange={setDialogOpen}
        initialName={editing?.name}
        onSave={editing ? handleRename : handleCreate}
      />
    </div>
  );
}
