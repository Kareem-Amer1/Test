import { describe, expect, it, vi, beforeEach } from "vitest";
import { render, screen, waitFor } from "@testing-library/react";
import { MemoryRouter, Route, Routes } from "react-router-dom";
import ExamSessionPage from "@/features/exams/ExamSessionPage";

const mockSubmit = vi.fn();
const mockSave = vi.fn();

vi.mock("@/features/exams/useExams", () => ({
  useExamSession: vi.fn(),
  useSaveExamAnswers: () => ({ mutateAsync: mockSave, isPending: false }),
  useSubmitExam: () => ({ mutateAsync: mockSubmit, isPending: false }),
}));

vi.mock("@/features/exams/useExamTimer", () => ({
  useExamTimer: vi.fn(),
  computeRemainingMs: vi.fn(),
  formatRemaining: vi.fn(() => "00:00"),
}));

import { useExamSession } from "@/features/exams/useExams";
import { useExamTimer } from "@/features/exams/useExamTimer";

const sessionFixture = {
  id: "exam-1",
  candidateName: "Ali",
  positionName: "Engineer",
  durationMinutes: 60,
  startedAt: new Date().toISOString(),
  status: "InProgress" as const,
  questions: [
    {
      id: "q1",
      type: "Essay" as const,
      text: "Describe yourself",
      points: 10,
      order: 0,
    },
  ],
  answers: [{ questionId: "q1", essayText: "Hello" }],
};

function renderPage() {
  return render(
    <MemoryRouter initialEntries={["/exams/exam-1/session"]}>
      <Routes>
        <Route path="/exams/:examId/session" element={<ExamSessionPage />} />
      </Routes>
    </MemoryRouter>,
  );
}

describe("ExamSessionPage", () => {
  beforeEach(() => {
    vi.clearAllMocks();
    vi.mocked(useExamSession).mockReturnValue({
      data: sessionFixture,
      isLoading: false,
      isError: false,
      error: null,
      refetch: vi.fn(),
    } as never);
    vi.mocked(useExamTimer).mockReturnValue({
      remainingMs: 0,
      isExpired: true,
      formatted: "00:00",
    });
    mockSubmit.mockResolvedValue({ id: "exam-1", status: "Submitted" });
  });

  it("auto-submits when timer expires", async () => {
    renderPage();

    await waitFor(() => {
      expect(mockSubmit).toHaveBeenCalledTimes(1);
    });
  });

  it("shows question navigation controls", () => {
    vi.mocked(useExamTimer).mockReturnValue({
      remainingMs: 60_000,
      isExpired: false,
      formatted: "01:00",
    });

    renderPage();

    expect(screen.getByRole("button", { name: /previous/i })).toBeInTheDocument();
    expect(screen.getByRole("button", { name: /next/i })).toBeInTheDocument();
    expect(screen.getByRole("button", { name: /submit exam/i })).toBeInTheDocument();
  });
});
