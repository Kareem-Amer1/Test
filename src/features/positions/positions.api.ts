import { api } from "@/lib/apiClient";
import type { CreatePositionDto, ExamTemplate, Position, UpsertQuestionDto } from "./positions.types";

export const positionsApi = {
  list: () => api.get<Position[]>("/positions"),
  create: (dto: CreatePositionDto) => api.post<Position>("/positions", dto),
  delete: (id: string) => api.deleteRaw(`/positions/${id}`),

  getTemplate: (positionId: string) =>
    api.get<ExamTemplate>(`/positions/${positionId}/template`),
  updateDuration: (positionId: string, durationMinutes: number) =>
    api.put<ExamTemplate>(`/positions/${positionId}/template/duration`, { durationMinutes }),
  addQuestion: (positionId: string, dto: UpsertQuestionDto) =>
    api.post<ExamTemplate>(`/positions/${positionId}/template/questions`, dto),
  updateQuestion: (positionId: string, questionId: string, dto: UpsertQuestionDto) =>
    api.put<ExamTemplate>(`/positions/${positionId}/template/questions/${questionId}`, dto),
  deleteQuestion: (positionId: string, questionId: string) =>
    api.deleteRaw(`/positions/${positionId}/template/questions/${questionId}`).then(() => undefined),
  reorderQuestions: (positionId: string, questionIds: string[]) =>
    api.put<ExamTemplate>(`/positions/${positionId}/template/questions/reorder`, { questionIds }),
};
