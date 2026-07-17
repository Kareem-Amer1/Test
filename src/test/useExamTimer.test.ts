import { describe, expect, it } from "vitest";
import { computeRemainingMs, formatRemaining } from "@/features/exams/useExamTimer";

describe("formatRemaining", () => {
  it("formats minutes and seconds with zero padding", () => {
    expect(formatRemaining(125_000)).toBe("02:05");
  });

  it("shows 00:00 when time is up", () => {
    expect(formatRemaining(0)).toBe("00:00");
  });
});

describe("computeRemainingMs", () => {
  it("returns full duration when exam just started", () => {
    const startedAt = "2026-07-16T12:00:00.000Z";
    const now = new Date("2026-07-16T12:00:00.000Z").getTime();
    expect(computeRemainingMs(startedAt, 60, now)).toBe(60 * 60 * 1000);
  });

  it("returns zero when duration elapsed", () => {
    const startedAt = "2026-07-16T12:00:00.000Z";
    const now = new Date("2026-07-16T13:01:00.000Z").getTime();
    expect(computeRemainingMs(startedAt, 60, now)).toBe(0);
  });

  it("returns full duration for invalid start while session not ready", () => {
    expect(computeRemainingMs("", 45)).toBe(45 * 60 * 1000);
  });
});

describe("useExamTimer", () => {
  it("marks expired when remaining hits zero", async () => {
    const { renderHook, act } = await import("@testing-library/react");
    const { useExamTimer } = await import("@/features/exams/useExamTimer");
    const startedAt = new Date(Date.now() - 61 * 60 * 1000).toISOString();

    const { result } = renderHook(() => useExamTimer(startedAt, 60, true));

    await act(async () => {
      await new Promise((r) => setTimeout(r, 1100));
    });

    expect(result.current.isExpired).toBe(true);
    expect(result.current.remainingMs).toBe(0);
  });
});
