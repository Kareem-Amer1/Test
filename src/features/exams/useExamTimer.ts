import { useEffect, useState } from "react";

export function formatRemaining(ms: number): string {
  const totalSec = Math.ceil(ms / 1000);
  const m = Math.floor(totalSec / 60);
  const s = totalSec % 60;
  return `${String(m).padStart(2, "0")}:${String(s).padStart(2, "0")}`;
}

/** Elapsed-time timer — pauses when the page is closed (server stores elapsedSeconds). */
export function useElapsedExamTimer(
  durationMinutes: number | undefined,
  serverElapsedSeconds: number | undefined,
  enabled: boolean,
) {
  const durationSec = (durationMinutes ?? 0) * 60;
  const active = enabled && durationSec > 0;

  const [anchor, setAnchor] = useState(() => ({
    serverElapsed: serverElapsedSeconds ?? 0,
    at: Date.now(),
  }));
  const [now, setNow] = useState(() => Date.now());

  useEffect(() => {
    if (serverElapsedSeconds !== undefined) {
      setAnchor({ serverElapsed: serverElapsedSeconds, at: Date.now() });
    }
  }, [serverElapsedSeconds]);

  useEffect(() => {
    if (!active) return;
    setNow(Date.now());
    const id = window.setInterval(() => setNow(Date.now()), 1000);
    return () => window.clearInterval(id);
  }, [active, durationSec, anchor.serverElapsed]);

  const localDeltaSec = active ? Math.floor((now - anchor.at) / 1000) : 0;
  const totalElapsed = active
    ? Math.min(durationSec, anchor.serverElapsed + localDeltaSec)
    : 0;
  const remainingMs = active ? Math.max(0, (durationSec - totalElapsed) * 1000) : durationSec * 1000;

  return {
    remainingMs,
    isExpired: active && remainingMs <= 0,
    formatted: formatRemaining(remainingMs),
    currentElapsedSeconds: totalElapsed,
  };
}

export function computeRemainingMs(startedAt: string, durationMinutes: number, now = Date.now()): number {
  const startMs = new Date(startedAt).getTime();
  if (!Number.isFinite(startMs) || durationMinutes <= 0) return durationMinutes * 60 * 1000;
  const end = startMs + durationMinutes * 60 * 1000;
  return Math.max(0, end - now);
}

export function useExamTimer(
  startedAt: string | undefined,
  durationMinutes: number | undefined,
  enabled: boolean,
) {
  const active = enabled && !!startedAt && (durationMinutes ?? 0) > 0;
  const duration = durationMinutes ?? 0;
  const start = startedAt ?? "";
  const [now, setNow] = useState(() => Date.now());

  useEffect(() => {
    if (!active) return;
    setNow(Date.now());
    const id = window.setInterval(() => setNow(Date.now()), 1000);
    return () => window.clearInterval(id);
  }, [start, duration, active]);

  const remainingMs = active ? computeRemainingMs(start, duration, now) : duration * 60 * 1000;

  return {
    remainingMs,
    isExpired: active && remainingMs <= 0,
    formatted: formatRemaining(remainingMs),
    currentElapsedSeconds: duration * 60 - Math.ceil(remainingMs / 1000),
  };
}
