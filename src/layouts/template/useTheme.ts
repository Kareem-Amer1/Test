import { useCallback, useEffect, useState } from "react";

export type Theme = "lite" | "dark" | "dim" | "neon" | "brut";

const STORAGE_KEY = "app.theme";
const VALID: Theme[] = ["lite", "dark", "dim", "neon", "brut"];
const DARKISH: Theme[] = ["dark", "dim", "brut"];

function read(): Theme {
  if (typeof window === "undefined") return "lite";
  const raw = window.localStorage.getItem(STORAGE_KEY);
  // Migrate legacy "light"/"dark" values.
  if (raw === "light") return "lite";
  if (raw && (VALID as string[]).includes(raw)) return raw as Theme;
  return "lite";
}

function apply(t: Theme) {
  if (typeof document === "undefined") return;
  document.documentElement.dataset.theme = t;
  document.documentElement.classList.toggle("dark", DARKISH.includes(t));
}

apply(read());

const listeners = new Set<(t: Theme) => void>();
if (typeof window !== "undefined") {
  window.addEventListener("storage", (e) => {
    if (e.key === STORAGE_KEY) {
      const next = read();
      apply(next);
      listeners.forEach((l) => l(next));
    }
  });
}

export function useTheme() {
  const [theme, setThemeState] = useState<Theme>(() => read());

  useEffect(() => { apply(theme); }, [theme]);

  useEffect(() => {
    const l = (t: Theme) => setThemeState(t);
    listeners.add(l);
    return () => { listeners.delete(l); };
  }, []);

  const setTheme = useCallback((t: Theme) => {
    window.localStorage.setItem(STORAGE_KEY, t);
    apply(t);
    setThemeState(t);
    listeners.forEach((l) => l(t));
  }, []);

  const toggle = useCallback(() => {
    setTheme(read() === "lite" ? "dark" : "lite");
  }, [setTheme]);

  return { theme, setTheme, toggle };
}
