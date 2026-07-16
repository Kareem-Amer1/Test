import { useCallback, useEffect, useState } from "react";

const STORAGE_KEY = "app.focus";

function read(): boolean {
  if (typeof window === "undefined") return false;
  return window.localStorage.getItem(STORAGE_KEY) === "1";
}

const listeners = new Set<(v: boolean) => void>();
if (typeof window !== "undefined") {
  window.addEventListener("storage", (e) => {
    if (e.key === STORAGE_KEY) {
      const next = read();
      listeners.forEach((l) => l(next));
    }
  });
}

export function useFocusMode() {
  const [focus, setFocusState] = useState<boolean>(() => read());

  useEffect(() => {
    const l = (v: boolean) => setFocusState(v);
    listeners.add(l);
    return () => {
      listeners.delete(l);
    };
  }, []);

  const setFocus = useCallback((v: boolean) => {
    window.localStorage.setItem(STORAGE_KEY, v ? "1" : "0");
    setFocusState(v);
    listeners.forEach((l) => l(v));
  }, []);

  const toggle = useCallback(() => {
    setFocus(!read());
  }, [setFocus]);

  return { focus, setFocus, toggle };
}
