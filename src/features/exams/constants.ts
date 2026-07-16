import type { ExamStatus } from "./exams.types";

export const EXAM_STATUS_OPTIONS: { value: ExamStatus | ""; label: string }[] = [
  { value: "", label: "All statuses" },
  { value: "InProgress", label: "In progress" },
  { value: "Submitted", label: "Submitted" },
  { value: "Graded", label: "Graded" },
];

export function formatExamScore(total: number | null | undefined, max: number): string {
  if (total === null || total === undefined) return "—";
  return `${total} / ${max}`;
}

export function formatDateTime(value: string | null | undefined): string {
  if (!value) return "—";
  return new Date(value).toLocaleString();
}
