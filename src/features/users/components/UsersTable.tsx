import { useState } from "react";
import { useTranslation } from "react-i18next";
import { UserPlus, UserX } from "lucide-react";
import { Button } from "@/components/ui/button";
import { Badge } from "@/components/ui/badge";
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from "@/components/ui/table";
import { toast } from "sonner";
import { useDeactivateHrUser, useHrUsers } from "../useUsers";
import type { HrUser } from "../users.types";
import { CreateUserDialog } from "./CreateUserDialog";

function formatDate(iso: string): string {
  try {
    return new Intl.DateTimeFormat(undefined, { dateStyle: "medium" }).format(new Date(iso));
  } catch {
    return iso;
  }
}

export default function UsersTable() {
  const { t } = useTranslation();
  const { data, isLoading, isError, refetch } = useHrUsers();
  const deactivateUser = useDeactivateHrUser();
  const [createOpen, setCreateOpen] = useState(false);

  const onDeactivate = async (user: HrUser) => {
    if (!window.confirm(t("users.confirmDeactivate", "Deactivate account for \"{{name}}\"?", { name: user.fullName })))
      return;
    try {
      await deactivateUser.mutateAsync(user.id);
      toast.success(t("users.deactivated", "Account deactivated."));
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
        <p className="text-sm text-destructive">{t("users.loadError", "Failed to load HR accounts.")}</p>
        <Button variant="outline" size="sm" onClick={() => refetch()}>{t("common.retry", "Retry")}</Button>
      </div>
    );
  }

  const users = data ?? [];

  return (
    <div className="space-y-4">
      <div className="flex justify-end">
        <Button onClick={() => setCreateOpen(true)}>
          <UserPlus className="h-4 w-4 me-2" />
          {t("users.create", "Add HR Account")}
        </Button>
      </div>

      {users.length === 0 ? (
        <div className="rounded-lg border border-dashed p-8 text-center text-sm text-muted-foreground">
          {t("users.empty", "No HR accounts yet.")}
        </div>
      ) : (
        <div className="rounded-lg border border-app-border-strong overflow-hidden">
          <Table>
            <TableHeader>
              <TableRow>
                <TableHead>{t("users.fullName", "Full name")}</TableHead>
                <TableHead>{t("users.email", "Email")}</TableHead>
                <TableHead>{t("common.status", "Status")}</TableHead>
                <TableHead>{t("users.createdAt", "Created")}</TableHead>
                <TableHead className="w-24 text-end">{t("common.actions", "Actions")}</TableHead>
              </TableRow>
            </TableHeader>
            <TableBody>
              {users.map((user) => (
                <TableRow key={user.id}>
                  <TableCell className="font-medium">{user.fullName}</TableCell>
                  <TableCell>{user.email}</TableCell>
                  <TableCell>
                    <Badge variant={user.isActive ? "default" : "secondary"}>
                      {user.isActive
                        ? t("users.active", "Active")
                        : t("users.inactive", "Inactive")}
                    </Badge>
                  </TableCell>
                  <TableCell className="text-muted-foreground">{formatDate(user.createdAt)}</TableCell>
                  <TableCell className="text-end">
                    {user.isActive && (
                      <Button
                        variant="ghost"
                        size="sm"
                        className="text-destructive hover:text-destructive"
                        disabled={deactivateUser.isPending}
                        onClick={() => onDeactivate(user)}
                      >
                        <UserX className="h-4 w-4" />
                      </Button>
                    )}
                  </TableCell>
                </TableRow>
              ))}
            </TableBody>
          </Table>
        </div>
      )}

      <CreateUserDialog open={createOpen} onOpenChange={setCreateOpen} />
    </div>
  );
}
