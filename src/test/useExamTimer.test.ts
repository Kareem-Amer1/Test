import { describe, expect, it } from "vitest";
import { computeRemainingMs } from "@/features/exams/useExamTimer";

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
