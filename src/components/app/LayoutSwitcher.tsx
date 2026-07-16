import { Layers, Check } from "lucide-react";
import { useTranslation } from "react-i18next";
import {
  DropdownMenu, DropdownMenuTrigger, DropdownMenuContent,
  DropdownMenuItem, DropdownMenuLabel, DropdownMenuSeparator,
} from "@/components/ui/dropdown-menu";
import { useLayoutTemplate } from "@/layouts/template/useLayoutTemplate";

export function LayoutSwitcher() {
  const { t } = useTranslation();
  const { template, setTemplate, templates } = useLayoutTemplate();

  return (
    <DropdownMenu>
      <DropdownMenuTrigger
        className="inline-flex items-center gap-1.5 px-2 h-9 text-sm font-medium text-foreground hover:text-primary transition-colors"
        aria-label={t("layout.title", "Layout")}
      >
        <Layers className="h-4 w-4" />
      </DropdownMenuTrigger>
      <DropdownMenuContent align="end" className="w-64">
        <DropdownMenuLabel>{t("layout.title", "Layout")}</DropdownMenuLabel>
        <DropdownMenuSeparator />
        {templates.map((tpl) => {
          const active = tpl.id === template;
          return (
            <DropdownMenuItem
              key={tpl.id}
              onClick={() => setTemplate(tpl.id)}
              className="flex items-start gap-2 py-2"
            >
              <Check className={`h-4 w-4 mt-0.5 shrink-0 ${active ? "opacity-100" : "opacity-0"}`} />
              <div className="flex-1 min-w-0">
                <div className="text-sm font-medium">{t(tpl.labelKey)}</div>
                <div className="text-xs text-muted-foreground">{t(tpl.descKey)}</div>
              </div>
            </DropdownMenuItem>
          );
        })}
      </DropdownMenuContent>
    </DropdownMenu>
  );
}
