import { api } from "@/lib/apiClient";
import type {
  CreateExamDto,
  CreateExamResult,
  ExamAnswerInput,
  ExamListItem,
  ExamSession,
  SubmitExamResult,
} from "./exams.types";

export const examsApi = {
  list: () => api.get<ExamListItem[]>("/exams"),
  create: (dto: CreateExamDto) => api.post<CreateExamResult>("/exams", dto),
  getSession: (id: string) => api.get<ExamSession>(`/exams/${id}/session`),
  saveAnswers: (id: string, answers: ExamAnswerInput[]) =>
    api.put<ExamSession>(`/exams/${id}/answers`, { answers }),
  submit: (id: string, answers: ExamAnswerInput[]) =>
    api.post<SubmitExamResult>(`/exams/${id}/submit`, { answers }),
};
