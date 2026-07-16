import { useCallback, useEffect, useState } from "react";

export type Style = "flat" | "gradient" | "fancy" | "glass" | "soft" | "sharp";

const STORAGE_KEY = "app.style";
const VALID: Style[] = ["flat", "gradient", "fancy", "glass", "soft", "sharp"];

export const styles: { id: Style; labelKey: string; descKey: string }[] = [
  { id: "flat", labelKey: "style.flat", descKey: "style.flatDesc" },
  { id: "gradient", labelKey: "style.gradient", descKey: "style.gradientDesc" },
  { id: "fancy", labelKey: "style.fancy", descKey: "style.fancyDesc" },
  { id: "glass", labelKey: "style.glass", descKey: "style.glassDesc" },
  { id: "soft", labelKey: "style.soft", descKey: "style.softDesc" },
  { id: "sharp", labelKey: "style.sharp", descKey: "style.sharpDesc" },
];

function read(): Style {
  if (typeof window === "undefined") return "flat";
  const raw = window.localStorage.getItem(STORAGE_KEY);
  return raw && (VALID as string[]).includes(raw) ? (raw as Style) : "flat";
}

function apply(s: Style) {
  if (typeof document === "undefined") return;
  document.documentElement.dataset.style = s;
}

apply(read());

const listeners = new Set<(s: Style) => void>();
if (typeof window !== "undefined") {
  window.addEventListener("storage", (e) => {
    if (e.key === STORAGE_KEY) {
      const next = read();
      apply(next);
      listeners.forEach((l) => l(next));
    }
  });
}

export function useStyle() {
  const [style, setStyleState] = useState<Style>(() => read());

  useEffect(() => {
    apply(style);
  }, [style]);

  useEffect(() => {
    const l = (s: Style) => setStyleState(s);
    listeners.add(l);
    return () => {
      listeners.delete(l);
    };
  }, []);

  const setStyle = useCallback((s: Style) => {
    window.localStorage.setItem(STORAGE_KEY, s);
    apply(s);
    setStyleState(s);
    listeners.forEach((l) => l(s));
  }, []);

  return { style, setStyle, styles };
}
