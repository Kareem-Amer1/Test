import { useState } from "react";
import { useTranslation } from "react-i18next";
import { ChevronDown, Check } from "lucide-react";
import { Popover, PopoverTrigger, PopoverContent } from "@/components/ui/popover";
import { Switch } from "@/components/ui/switch";
import { useTheme, type Theme } from "@/layouts/template/useTheme";
import { useLayoutTemplate, type LayoutTemplate } from "@/layouts/template/useLayoutTemplate";
import { useStyle, type Style } from "@/layouts/template/useStyle";
import { usePalette } from "@/layouts/template/usePalette";
import { useSidebarTint } from "@/layouts/template/useSidebarTint";
import { cn } from "@/lib/utils";


type Tile = { id: string; label: string; preview: React.ReactNode };

function TileButton({
  active,
  onClick,
  label,
  children,
}: {
  active: boolean;
  onClick: () => void;
  label: string;
  children: React.ReactNode;
}) {
  return (
    <button
      type="button"
      onClick={onClick}
      className={cn(
        "group flex flex-col items-center gap-1.5 outline-none",
      )}
      aria-pressed={active}
    >
      <div
        className={cn(
          "h-12 w-12 rounded-lg border bg-card overflow-hidden flex items-center justify-center transition-all",
          active
            ? "border-primary ring-2 ring-primary/40"
            : "border-app-border-strong hover:border-foreground/40",
        )}
      >
        {children}
      </div>
      <span
        className={cn(
          "text-[10px] font-bold tracking-wider uppercase",
          active ? "text-primary" : "text-muted-foreground",
        )}
      >
        {label}
      </span>
    </button>
  );
}

function Section({ title, cols = 5, gap = "gap-2", layout = "grid", children }: { title: string; cols?: number; gap?: string; layout?: "grid" | "flex"; children: React.ReactNode }) {
  return (
    <div className="space-y-2.5">
      <div className="text-[10px] font-semibold tracking-[0.14em] text-muted-foreground uppercase">
        {title}
      </div>
      {layout === "flex" ? (
        <div className={cn("flex flex-wrap", gap)}>{children}</div>
      ) : (
        <div className={cn("grid", gap)} style={{ gridTemplateColumns: `repeat(${cols}, minmax(0, 1fr))` }}>{children}</div>
      )}
    </div>
  );
}

// ---- Theme tile previews ----
const themePreview: Record<Theme, React.ReactNode> = {
  lite: <div className="h-full w-full bg-white text-[9px] font-black text-neutral-800 flex items-center justify-center">Aa</div>,
  dark: <div className="h-full w-full bg-neutral-900 text-[9px] font-black text-neutral-100 flex items-center justify-center">Aa</div>,
  dim: <div className="h-full w-full bg-slate-700 text-[9px] font-black text-slate-100 flex items-center justify-center">Aa</div>,
  neon: <div className="h-full w-full text-[9px] font-black text-white flex items-center justify-center" style={{ background: "hsl(var(--primary))" }}>Aa</div>,
  brut: (
    <div className="h-full w-full relative bg-white text-[9px] font-black text-neutral-900 flex items-center justify-center">
      <div className="absolute inset-0" style={{ clipPath: "polygon(100% 0, 100% 100%, 0 100%)", background: "#0a0a0a" }} />
      <span className="relative z-10">Aa</span>
    </div>
  ),
};

// ---- Layout wireframe glyphs ----
const layoutPreview: Record<LayoutTemplate, React.ReactNode> = {
  classic: (
    <svg viewBox="0 0 32 32" className="h-7 w-7">
      <rect x="2" y="2" width="9" height="28" rx="1.5" fill="currentColor" opacity="0.85" />
      <rect x="13" y="2" width="17" height="6" rx="1" fill="currentColor" opacity="0.35" />
      <rect x="13" y="10" width="17" height="20" rx="1.5" fill="currentColor" opacity="0.2" />
    </svg>
  ),
  topnav: (
    <svg viewBox="0 0 32 32" className="h-7 w-7">
      <rect x="2" y="2" width="28" height="6" rx="1" fill="currentColor" opacity="0.85" />
      <rect x="2" y="10" width="28" height="20" rx="1.5" fill="currentColor" opacity="0.25" />
    </svg>
  ),
  bento: (
    <svg viewBox="0 0 32 32" className="h-7 w-7">
      <rect x="2" y="2" width="9" height="13" rx="1.5" fill="currentColor" opacity="0.7" />
      <rect x="2" y="17" width="9" height="13" rx="1.5" fill="currentColor" opacity="0.45" />
      <rect x="13" y="2" width="17" height="9" rx="1.5" fill="currentColor" opacity="0.55" />
      <rect x="13" y="13" width="17" height="17" rx="1.5" fill="currentColor" opacity="0.3" />
    </svg>
  ),
  split: (
    <svg viewBox="0 0 32 32" className="h-7 w-7">
      <rect x="2" y="2" width="5" height="28" rx="1" fill="currentColor" opacity="0.85" />
      <rect x="9" y="2" width="13" height="28" rx="1.5" fill="currentColor" opacity="0.4" />
      <rect x="24" y="2" width="6" height="28" rx="1" fill="currentColor" opacity="0.6" />
    </svg>
  ),
  brutalist: (
    <svg viewBox="0 0 32 32" className="h-7 w-7">
      <rect x="3" y="3" width="9" height="26" fill="none" stroke="currentColor" strokeWidth="2" />
      <rect x="14" y="3" width="16" height="26" fill="none" stroke="currentColor" strokeWidth="2" />
    </svg>
  ),
  fancy: (
    <svg viewBox="0 0 32 32" className="h-7 w-7">
      <rect x="2" y="2" width="9" height="28" rx="2" fill="currentColor" opacity="0.85" />
      <circle cx="6.5" cy="9" r="1.6" fill="#00E58A" />
      <text x="20" y="20" fontFamily="serif" fontStyle="italic" fontSize="14" fill="currentColor" opacity="0.85" textAnchor="middle">Aa</text>
      <circle cx="27" cy="6" r="2" fill="#00E58A" opacity="0.6" />
    </svg>
  ),
};

// ---- Style swatch previews ----
const stylePreview: Record<Style, React.ReactNode> = {
  flat: <div className="h-7 w-7 rounded-md bg-foreground/80" />,
  gradient: <div className="h-7 w-7 rounded-md" style={{ background: "linear-gradient(135deg, hsl(var(--primary)), hsl(var(--brand-accent)))" }} />,
  glass: <div className="h-7 w-7 rounded-md border border-foreground/30 backdrop-blur" style={{ background: "linear-gradient(135deg, hsl(var(--foreground)/0.15), hsl(var(--foreground)/0.04))" }} />,
  soft: <div className="h-7 w-7 rounded-xl bg-muted shadow-inner" />,
  sharp: <div className="h-7 w-7 bg-foreground" style={{ borderRadius: 0 }} />,
  fancy: (
    <div
      className="h-7 w-7 rounded-lg relative overflow-hidden"
      style={{
        background:
          "radial-gradient(circle at 30% 30%, #4DFFB0 0%, #00A86A 45%, #07090A 100%)",
      }}
    >
      <div className="absolute inset-0 opacity-30" style={{ background: "radial-gradient(circle at 70% 80%, #F5B547 0%, transparent 60%)" }} />
    </div>
  ),
};

export function ThemeStudio() {
  const { t } = useTranslation();
  const [open, setOpen] = useState(false);
  const { theme, setTheme } = useTheme();
  const { template, setTemplate, templates } = useLayoutTemplate();
  const { style, setStyle, styles: styleList } = useStyle();
  const { palette, setPalette, palettes: paletteList } = usePalette();
  const { sidebarTint, setSidebarTint } = useSidebarTint();


  const themeIds: Theme[] = ["lite", "dark", "dim", "neon", "brut"];

  return (
    <Popover open={open} onOpenChange={setOpen}>
      <PopoverTrigger
        aria-label={t("studio.title", "Theme Studio")}
        className="inline-flex items-center gap-2 px-2.5 h-9 rounded-md text-xs font-bold tracking-wider uppercase text-foreground hover:text-primary transition-colors"
      >
        <span className="inline-block h-2 w-2 rounded-full bg-primary" />
        <span className="hidden sm:inline">{t("studio.title", "Theme Studio")}</span>
        <ChevronDown className={cn("h-3.5 w-3.5 transition-transform", open && "rotate-180")} />
      </PopoverTrigger>
      <PopoverContent align="end" className="w-[340px] p-4 space-y-4">
        <div className="flex items-center gap-2">
          <Check className="h-3.5 w-3.5 text-[#00E58A]" strokeWidth={3} />
          <span className="text-xs font-bold tracking-wider uppercase text-foreground">
            {t("studio.title", "Theme Studio")}
          </span>
        </div>
        <div className="border-t border-app-border-strong -mx-4 !mt-3" />

        <Section title={t("studio.sections.theme", "Theme")} cols={5}>
          {themeIds.map((id) => (
            <TileButton
              key={id}
              active={theme === id}
              onClick={() => setTheme(id)}
              label={t(`studio.theme.${id}`, id.toUpperCase())}
            >
              {themePreview[id]}
            </TileButton>
          ))}
        </Section>

        <Section title={t("studio.sections.layout", "Layout")} layout="flex" gap="gap-x-4 gap-y-2">
          {templates.map((tpl) => (
            <TileButton
              key={tpl.id}
              active={template === tpl.id}
              onClick={() => setTemplate(tpl.id)}
              label={t(`studio.layout.${tpl.id}`, tpl.id.toUpperCase())}
            >
              <div className="text-foreground/80">{layoutPreview[tpl.id]}</div>
            </TileButton>
          ))}
        </Section>

        <Section title={t("studio.sections.style", "Style")} cols={6}>
          {styleList.map((s) => (
            <TileButton
              key={s.id}
              active={style === s.id}
              onClick={() => setStyle(s.id)}
              label={t(`studio.style.${s.id}`, s.id.toUpperCase())}
            >
              {stylePreview[s.id]}
            </TileButton>
          ))}
        </Section>

        <Section title={t("studio.sections.palette", "Color Palette")} cols={4}>
          {paletteList.map((p) => (
            <TileButton
              key={p.id}
              active={palette === p.id}
              onClick={() => setPalette(p.id)}
              label={t(`studio.palette.${p.id}`, p.id.toUpperCase())}
            >
              <div className="grid grid-cols-2 grid-rows-2 h-full w-full">
                {p.swatch.map((hex, i) => (
                  <div key={i} style={{ background: hex }} />
                ))}
              </div>
            </TileButton>
          ))}
        </Section>

        <div className="space-y-2.5">
          <div className="text-[10px] font-semibold tracking-[0.14em] text-muted-foreground uppercase">
            {t("studio.sections.sidebar", "Sidebar")}
          </div>
          <div className="flex items-center justify-between rounded-md border border-app-border-strong px-3 py-2">
            <span className="text-xs font-medium text-foreground">
              {t("studio.sidebar.bgColor", "Background color")}
            </span>
            <Switch
              checked={sidebarTint === "on"}
              onCheckedChange={(v) => setSidebarTint(v ? "on" : "off")}
              aria-label={t("studio.sidebar.bgColor", "Background color")}
            />
          </div>
        </div>
      </PopoverContent>
    </Popover>
  );
}
