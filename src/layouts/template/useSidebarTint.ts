import { useCallback, useEffect, useState } from "react";

export type SidebarTint = "on" | "off";

const STORAGE_KEY = "app.sidebarTint";
const VALID: SidebarTint[] = ["on", "off"];

function read(): SidebarTint {
  if (typeof window === "undefined") return "off";
  const raw = window.localStorage.getItem(STORAGE_KEY);
  return raw && (VALID as string[]).includes(raw) ? (raw as SidebarTint) : "off";
}

function apply(v: SidebarTint) {
  if (typeof document === "undefined") return;
  document.documentElement.dataset.sidebarTint = v;
}

apply(read());

const listeners = new Set<(v: SidebarTint) => void>();
if (typeof window !== "undefined") {
  window.addEventListener("storage", (e) => {
    if (e.key === STORAGE_KEY) {
      const next = read();
      apply(next);
      listeners.forEach((l) => l(next));
    }
  });
}

export function useSidebarTint() {
  const [sidebarTint, setState] = useState<SidebarTint>(() => read());

  useEffect(() => { apply(sidebarTint); }, [sidebarTint]);

  useEffect(() => {
    const l = (v: SidebarTint) => setState(v);
    listeners.add(l);
    return () => { listeners.delete(l); };
  }, []);

  const setSidebarTint = useCallback((v: SidebarTint) => {
    window.localStorage.setItem(STORAGE_KEY, v);
    apply(v);
    setState(v);
    listeners.forEach((l) => l(v));
  }, []);

  return { sidebarTint, setSidebarTint };
}
