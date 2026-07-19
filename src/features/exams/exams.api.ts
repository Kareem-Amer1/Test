import { api } from "@/lib/apiClient";
import type {
  CreateExamDto,
  CreateExamResult,
  ExamAnswerInput,
  ExamDetail,
  ExamListFilters,
  ExamListItem,
  ExamSession,
  GradeExamDto,
  GradeExamResult,
  SubmitExamResult,
} from "./exams.types";

function buildListQuery(filters?: ExamListFilters): string {
  if (!filters) return "";
  const params = new URLSearchParams();
  if (filters.positionId) params.set("positionId", filters.positionId);
  if (filters.status) params.set("status", filters.status);
  if (filters.from) params.set("from", filters.from);
  if (filters.to) params.set("to", filters.to);
  if (filters.search?.trim()) params.set("search", filters.search.trim());
  const qs = params.toString();
  return qs ? `?${qs}` : "";
}

export const examsApi = {
  list: (filters?: ExamListFilters) =>
    api.get<ExamListItem[]>(`/exams${buildListQuery(filters)}`),
  create: (dto: CreateExamDto) => api.post<CreateExamResult>("/exams", dto),
  getDetail: (id: string) => api.get<ExamDetail>(`/exams/${id}`),
  getSession: (id: string) => api.get<ExamSession>(`/exams/${id}/session`),
  saveAnswers: (id: string, answers: ExamAnswerInput[], elapsedSeconds?: number) =>
    api.put<ExamSession>(`/exams/${id}/answers`, { answers, elapsedSeconds }),
  submit: (id: string, answers: ExamAnswerInput[], elapsedSeconds?: number) =>
    api.post<SubmitExamResult>(`/exams/${id}/submit`, { answers, elapsedSeconds }),
  grade: (id: string, dto: GradeExamDto) =>
    api.put<GradeExamResult>(`/exams/${id}/grade`, dto),
};
