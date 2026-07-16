import { MessageSquare } from "lucide-react";
import { useTranslation } from "react-i18next";
import { cn } from "@/lib/utils";

interface Props {
  open: boolean;
  available: boolean;
  onToggle: () => void;
}

export function CopilotNavbarChatButton({ open, available, onToggle }: Props) {
  const { i18n } = useTranslation();
  const isRtl = i18n.language?.toLowerCase().startsWith("ar");
  if (!available) return null;
  const label = open
    ? isRtl ? "إغلاق المساعد" : "Close assistant"
    : isRtl ? "فتح المساعد" : "Open assistant";

  return (
    <button
      type="button"
      onMouseDown={(e) => e.stopPropagation()}
      onClick={onToggle}
      aria-label={label}
      title={label}
      aria-pressed={open}
      className={cn(
        "inline-flex items-center justify-center h-9 w-9 rounded-full transition",
        "text-muted-foreground hover:text-foreground hover:bg-muted",
        open && "bg-primary/10 text-primary",
      )}
    >
      <MessageSquare className="h-5 w-5" />
    </button>
  );
}
