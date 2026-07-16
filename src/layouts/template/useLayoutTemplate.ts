import { useCallback, useEffect, useState } from "react";

export type LayoutTemplate = "classic" | "topnav" | "bento" | "fancy" | "split" | "brutalist";

const STORAGE_KEY = "app.layout";
const VALID: LayoutTemplate[] = ["classic", "topnav", "bento", "fancy", "split", "brutalist"];


export const layoutTemplates: { id: LayoutTemplate; labelKey: string; descKey: string }[] = [
  { id: "classic", labelKey: "layout.classic", descKey: "layout.classicDesc" },
  { id: "topnav", labelKey: "layout.topnav", descKey: "layout.topnavDesc" },
  { id: "bento", labelKey: "layout.bento", descKey: "layout.bentoDesc" },
  { id: "fancy", labelKey: "layout.fancy", descKey: "layout.fancyDesc" },
  // { id: "split", labelKey: "layout.split", descKey: "layout.splitDesc" },
  { id: "brutalist", labelKey: "layout.brutalist", descKey: "layout.brutalistDesc" },
];

function read(): LayoutTemplate {
  if (typeof window === "undefined") return "bento";
  const raw = window.localStorage.getItem(STORAGE_KEY);
  if (raw === "modern") return "bento"; // legacy migration
  if (raw && (VALID as string[]).includes(raw)) return raw as LayoutTemplate;
  return "bento";
}

function apply(t: LayoutTemplate) {
  if (typeof document === "undefined") return;
  document.documentElement.dataset.layout = t;
}

apply(read());

const listeners = new Set<(t: LayoutTemplate) => void>();
if (typeof window !== "undefined") {
  window.addEventListener("storage", (e) => {
    if (e.key === STORAGE_KEY) {
      const next = read();
      apply(next);
      listeners.forEach((l) => l(next));
    }
  });
}

export function useLayoutTemplate() {
  const [template, setTemplateState] = useState<LayoutTemplate>(() => read());

  useEffect(() => {
    apply(template);
  }, [template]);

  useEffect(() => {
    const l = (t: LayoutTemplate) => setTemplateState(t);
    listeners.add(l);
    return () => {
      listeners.delete(l);
    };
  }, []);

  const setTemplate = useCallback((t: LayoutTemplate) => {
    window.localStorage.setItem(STORAGE_KEY, t);
    apply(t);
    setTemplateState(t);
    listeners.forEach((l) => l(t));
  }, []);

  return { template, setTemplate, templates: layoutTemplates };
}
