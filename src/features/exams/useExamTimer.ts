import { useEffect, useState } from "react";

export function computeRemainingMs(startedAt: string, durationMinutes: number, now = Date.now()): number {
  const startMs = new Date(startedAt).getTime();
  if (!Number.isFinite(startMs) || durationMinutes <= 0) return durationMinutes * 60 * 1000;
  const end = startMs + durationMinutes * 60 * 1000;
  return Math.max(0, end - now);
}

export function formatRemaining(ms: number): string {
  const totalSec = Math.ceil(ms / 1000);
  const m = Math.floor(totalSec / 60);
  const s = totalSec % 60;
  return `${String(m).padStart(2, "0")}:${String(s).padStart(2, "0")}`;
}

export function useExamTimer(
  startedAt: string | undefined,
  durationMinutes: number | undefined,
  enabled: boolean,
) {
  const active = enabled && !!startedAt && (durationMinutes ?? 0) > 0;
  const duration = durationMinutes ?? 0;
  const start = startedAt ?? "";

  // Recompute on every render + tick — avoids stale remainingMs=0 after loading finishes.
  const [now, setNow] = useState(() => Date.now());

  useEffect(() => {
    if (!active) return;
    setNow(Date.now());
    const id = window.setInterval(() => setNow(Date.now()), 1000);
    return () => window.clearInterval(id);
  }, [start, duration, active]);

  const remainingMs = active
    ? computeRemainingMs(start, duration, now)
    : duration * 60 * 1000;

  return {
    remainingMs,
    isExpired: active && remainingMs <= 0,
    formatted: formatRemaining(remainingMs),
  };
}
