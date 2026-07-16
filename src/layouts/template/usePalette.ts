import { useCallback, useEffect, useState } from "react";

export type Palette =
  | "emerald"
  | "indigo"
  | "violet"
  | "rose"
  | "amber"
  | "sky"
  | "teal"
  | "slate";

const STORAGE_KEY = "app.palette";
const VALID: Palette[] = ["emerald", "indigo", "violet", "rose", "amber", "sky", "teal", "slate"];

export const palettes: { id: Palette; swatch: [string, string, string, string] }[] = [
  { id: "emerald", swatch: ["#33691E", "#558B2F", "#7CB342", "#C5E1A5"] },
  { id: "indigo",  swatch: ["#1E1B4B", "#3730A3", "#6366F1", "#C7D2FE"] },
  { id: "violet",  swatch: ["#2E1065", "#6D28D9", "#8B5CF6", "#DDD6FE"] },
  { id: "rose",    swatch: ["#4C0519", "#9F1239", "#F43F5E", "#FECDD3"] },
  { id: "amber",   swatch: ["#451A03", "#B45309", "#F59E0B", "#FDE68A"] },
  { id: "sky",     swatch: ["#0C2340", "#0369A1", "#0EA5E9", "#BAE6FD"] },
  { id: "teal",    swatch: ["#042F2E", "#0F766E", "#14B8A6", "#99F6E4"] },
  { id: "slate",   swatch: ["#0F172A", "#334155", "#64748B", "#CBD5E1"] },
];

function read(): Palette {
  if (typeof window === "undefined") return "emerald";
  const raw = window.localStorage.getItem(STORAGE_KEY);
  return raw && (VALID as string[]).includes(raw) ? (raw as Palette) : "emerald";
}

function apply(p: Palette) {
  if (typeof document === "undefined") return;
  document.documentElement.dataset.palette = p;
}

apply(read());

const listeners = new Set<(p: Palette) => void>();
if (typeof window !== "undefined") {
  window.addEventListener("storage", (e) => {
    if (e.key === STORAGE_KEY) {
      const next = read();
      apply(next);
      listeners.forEach((l) => l(next));
    }
  });
}

export function usePalette() {
  const [palette, setPaletteState] = useState<Palette>(() => read());

  useEffect(() => { apply(palette); }, [palette]);

  useEffect(() => {
    const l = (p: Palette) => setPaletteState(p);
    listeners.add(l);
    return () => { listeners.delete(l); };
  }, []);

  const setPalette = useCallback((p: Palette) => {
    window.localStorage.setItem(STORAGE_KEY, p);
    apply(p);
    setPaletteState(p);
    listeners.forEach((l) => l(p));
  }, []);

  return { palette, setPalette, palettes };
}
