import { useState } from "react";
import { Link } from "react-router-dom";
import { useTranslation } from "react-i18next";
import { Briefcase, Link2, Pencil, Trash2 } from "lucide-react";
import { Button } from "@/components/ui/button";
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from "@/components/ui/table";
import { useAuth } from "@/hooks/useAuth";
import { useDeletePosition, usePositions } from "../usePositions";
import type { Position } from "../positions.types";
import { CreatePositionDialog } from "./CreatePositionDialog";
import { CreateInvitationDialog } from "@/features/invitations/components/CreateInvitationDialog";
import { toast } from "sonner";

export default function PositionsTable() {
  const { t } = useTranslation();
  const { isSuperAdmin } = useAuth();
  const { data, isLoading, isError, refetch } = usePositions();
  const deletePosition = useDeletePosition();
  const [createOpen, setCreateOpen] = useState(false);
  const [invitePosition, setInvitePosition] = useState<Position | null>(null);

  const onDelete = async (position: Position) => {
    if (!window.confirm(t("positions.confirmDelete", "Delete position \"{{name}}\"?", { name: position.name })))
      return;
    try {
      await deletePosition.mutateAsync(position.id);
      toast.success(t("positions.deleted", "Position deleted."));
    } catch (err) {
      toast.error(err instanceof Error ? err.message : String(err));
    }
  };

  if (isLoading) {
    return <p className="text-sm text-muted-foreground">{t("common.loading", "Loading…")}</p>;
  }
  if (isError) {
    return (
      <div className="space-y-2">
        <p className="text-sm text-destructive">{t("positions.loadError", "Failed to load positions.")}</p>
        <Button variant="outline" size="sm" onClick={() => refetch()}>{t("common.retry", "Retry")}</Button>
      </div>
    );
  }

  const positions = data ?? [];

  return (
    <div className="space-y-4">
      {isSuperAdmin && (
        <div className="flex justify-end">
          <Button onClick={() => setCreateOpen(true)}>
            <Briefcase className="h-4 w-4 me-2" />
            {t("positions.create", "Add Position")}
          </Button>
        </div>
      )}

      {positions.length === 0 ? (
        <div className="rounded-lg border border-dashed p-8 text-center text-sm text-muted-foreground">
          {t("positions.empty", "No positions yet. Run seed or create one as Super Admin.")}
        </div>
      ) : (
        <div className="rounded-lg border border-app-border-strong overflow-hidden">
          <Table>
            <TableHeader>
              <TableRow>
                <TableHead>{t("positions.name", "Name")}</TableHead>
                <TableHead>{t("positions.description", "Description")}</TableHead>
                <TableHead className="w-64 text-end">{t("common.actions", "Actions")}</TableHead>
              </TableRow>
            </TableHeader>
            <TableBody>
              {positions.map((p) => (
                <TableRow key={p.id}>
                  <TableCell className="font-medium">{p.name}</TableCell>
                  <TableCell className="text-muted-foreground">{p.description ?? "—"}</TableCell>
                  <TableCell className="text-end space-x-2">
                    <Button variant="default" size="sm" onClick={() => setInvitePosition(p)}>
                      <Link2 className="h-4 w-4 me-1" />
                      {t("invitations.createLink", "Create link")}
                    </Button>
                    <Button variant="outline" size="sm" asChild>
                      <Link to={`/positions/${p.id}/template`}>
                        <Pencil className="h-4 w-4 me-1" />
                        {t("positions.editTemplate", "Template")}
                      </Link>
                    </Button>
                    {isSuperAdmin && (
                      <Button
                        variant="ghost"
                        size="sm"
                        className="text-destructive hover:text-destructive"
                        disabled={deletePosition.isPending}
                        onClick={() => onDelete(p)}
                      >
                        <Trash2 className="h-4 w-4" />
                      </Button>
                    )}
                  </TableCell>
                </TableRow>
              ))}
            </TableBody>
          </Table>
        </div>
      )}

      <CreatePositionDialog open={createOpen} onOpenChange={setCreateOpen} />
      <CreateInvitationDialog
        position={invitePosition}
        open={!!invitePosition}
        onOpenChange={(open) => { if (!open) setInvitePosition(null); }}
      />
    </div>
  );
}
