import { Maximize2, Minimize2 } from "lucide-react";
import { useTranslation } from "react-i18next";
import { useLocation } from "react-router-dom";
import { Tooltip, TooltipTrigger, TooltipContent } from "@/components/ui/tooltip";
import { useFocusMode } from "@/hooks/useFocusMode";

export function FocusModeToggle() {
  const { t } = useTranslation();
  const { focus, toggle } = useFocusMode();
  const location = useLocation();
  const disabled = location.pathname.startsWith("/ivr/editor");

  const label = disabled
    ? t("focusMode.unavailable", "Focus mode is unavailable here")
    : focus
      ? t("focusMode.exit", "Exit focus mode")
      : t("focusMode.enter", "Enter focus mode");

  return (
    <Tooltip>
      <TooltipTrigger asChild>
        <button
          type="button"
          onClick={disabled ? undefined : toggle}
          disabled={disabled}
          aria-label={label}
          aria-pressed={!disabled && focus}
          className="inline-flex items-center justify-center h-9 w-9 rounded-md text-foreground hover:text-primary transition-colors disabled:opacity-40 disabled:hover:text-foreground disabled:cursor-not-allowed"
        >
          {focus && !disabled ? <Minimize2 className="h-4 w-4" /> : <Maximize2 className="h-4 w-4" />}
        </button>
      </TooltipTrigger>
      <TooltipContent>{label}</TooltipContent>
    </Tooltip>
  );
}
