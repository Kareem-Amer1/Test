import { api } from "@/lib/apiClient";
import type {
  CreatePositionDto,
  ExamTemplate,
  Position,
  UpsertPartitionDto,
  UpsertQuestionDto,
} from "./positions.types";

export const positionsApi = {
  list: () => api.get<Position[]>("/positions"),
  create: (dto: CreatePositionDto) => api.post<Position>("/positions", dto),
  delete: (id: string) => api.deleteRaw(`/positions/${id}`),

  getTemplate: (positionId: string) =>
    api.get<ExamTemplate>(`/positions/${positionId}/template`),
  updateDuration: (positionId: string, durationMinutes: number) =>
    api.put<ExamTemplate>(`/positions/${positionId}/template/duration`, { durationMinutes }),

  addPartition: (positionId: string, dto: UpsertPartitionDto) =>
    api.post<ExamTemplate>(`/positions/${positionId}/template/partitions`, dto),
  updatePartition: (positionId: string, partitionId: string, dto: UpsertPartitionDto) =>
    api.put<ExamTemplate>(`/positions/${positionId}/template/partitions/${partitionId}`, dto),
  deletePartition: (positionId: string, partitionId: string) =>
    api.deleteRaw(`/positions/${positionId}/template/partitions/${partitionId}`).then(() => undefined),

  addQuestion: (positionId: string, partitionId: string, dto: UpsertQuestionDto) =>
    api.post<ExamTemplate>(
      `/positions/${positionId}/template/partitions/${partitionId}/questions`,
      dto,
    ),
  updateQuestion: (
    positionId: string,
    partitionId: string,
    questionId: string,
    dto: UpsertQuestionDto,
  ) =>
    api.put<ExamTemplate>(
      `/positions/${positionId}/template/partitions/${partitionId}/questions/${questionId}`,
      dto,
    ),
  deleteQuestion: (positionId: string, partitionId: string, questionId: string) =>
    api
      .deleteRaw(
        `/positions/${positionId}/template/partitions/${partitionId}/questions/${questionId}`,
      )
      .then(() => undefined),
  reorderQuestions: (positionId: string, partitionId: string, questionIds: string[]) =>
    api.put<ExamTemplate>(
      `/positions/${positionId}/template/partitions/${partitionId}/questions/reorder`,
      { questionIds },
    ),
};
