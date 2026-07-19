import { useState } from "react";
import { useTranslation } from "react-i18next";
import { Copy, Check } from "lucide-react";
import {
  Dialog,
  DialogContent,
  DialogFooter,
  DialogHeader,
  DialogTitle,
} from "@/components/ui/dialog";
import { Button } from "@/components/ui/button";
import { Label } from "@/components/ui/label";
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/components/ui/select";
import { Input } from "@/components/ui/input";
import { toast } from "sonner";
import { useCreateInvitation } from "@/features/invitations/useInvitations";
import { buildInviteUrl, LINK_DURATION_OPTIONS } from "../invitations.types";
import type { Position } from "@/features/positions/positions.types";

interface Props {
  position: Position | null;
  open: boolean;
  onOpenChange: (open: boolean) => void;
}

export function CreateInvitationDialog({ position, open, onOpenChange }: Props) {
  const { t } = useTranslation();
  const createInvitation = useCreateInvitation();
  const [durationHours, setDurationHours] = useState(String(LINK_DURATION_OPTIONS[1].value));
  const [createdLink, setCreatedLink] = useState<string | null>(null);
  const [copied, setCopied] = useState(false);

  const reset = () => {
    setCreatedLink(null);
    setCopied(false);
    setDurationHours(String(LINK_DURATION_OPTIONS[1].value));
  };

  const handleClose = (next: boolean) => {
    if (!next) reset();
    onOpenChange(next);
  };

  const onCreate = async () => {
    if (!position) return;
    try {
      const result = await createInvitation.mutateAsync({
        positionId: position.id,
        linkDurationHours: Number(durationHours),
      });
      const url = buildInviteUrl(result.token);
      setCreatedLink(url);
      toast.success(t("invitations.created", "Invitation link created."));
    } catch (err) {
      toast.error(err instanceof Error ? err.message : String(err));
    }
  };

  const onCopy = async () => {
    if (!createdLink) return;
    await navigator.clipboard.writeText(createdLink);
    setCopied(true);
    toast.success(t("invitations.copied", "Link copied to clipboard."));
    window.setTimeout(() => setCopied(false), 2000);
  };

  return (
    <Dialog open={open} onOpenChange={handleClose}>
      <DialogContent>
        <DialogHeader>
          <DialogTitle>{t("invitations.createTitle", "Create invitation link")}</DialogTitle>
        </DialogHeader>
        {position && (
          <p className="text-sm text-muted-foreground">
            {t("invitations.forPosition", "Position")}: <strong>{position.name}</strong>
          </p>
        )}

        {createdLink ? (
          <div className="space-y-3">
            <p className="text-sm">{t("invitations.shareHint", "Send this link to the candidate before the interview:")}</p>
            <div className="flex gap-2">
              <Input readOnly value={createdLink} className="font-mono text-xs" />
              <Button type="button" variant="outline" size="icon" onClick={() => void onCopy()}>
                {copied ? <Check className="h-4 w-4" /> : <Copy className="h-4 w-4" />}
              </Button>
            </div>
            <DialogFooter>
              <Button type="button" onClick={() => handleClose(false)}>
                {t("common.close", "Close")}
              </Button>
            </DialogFooter>
          </div>
        ) : (
          <>
            <div className="space-y-1.5">
              <Label>{t("invitations.linkDuration", "Link validity")}</Label>
              <Select value={durationHours} onValueChange={setDurationHours}>
                <SelectTrigger>
                  <SelectValue />
                </SelectTrigger>
                <SelectContent>
                  {LINK_DURATION_OPTIONS.map((opt) => (
                    <SelectItem key={opt.value} value={String(opt.value)}>
                      {t(opt.labelKey, opt.fallback)}
                    </SelectItem>
                  ))}
                </SelectContent>
              </Select>
              <p className="text-xs text-muted-foreground">
                {t("invitations.linkDurationHint", "The link expires after this period if the exam has not been completed.")}
              </p>
            </div>
            <DialogFooter>
              <Button type="button" variant="outline" onClick={() => handleClose(false)}>
                {t("common.cancel", "Cancel")}
              </Button>
              <Button type="button" disabled={createInvitation.isPending || !position} onClick={() => void onCreate()}>
                {t("invitations.create", "Create link")}
              </Button>
            </DialogFooter>
          </>
        )}
      </DialogContent>
    </Dialog>
  );
}
