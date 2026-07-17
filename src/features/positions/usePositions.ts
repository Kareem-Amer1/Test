import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { positionsApi } from "./positions.api";
import type { CreatePositionDto, UpsertPartitionDto, UpsertQuestionDto } from "./positions.types";

export const POSITIONS_KEY = ["positions"] as const;
export const templateKey = (positionId: string) => ["positions", positionId, "template"] as const;

export function usePositions() {
  return useQuery({
    queryKey: POSITIONS_KEY,
    queryFn: positionsApi.list,
    staleTime: 60_000,
  });
}

export function useCreatePosition() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (dto: CreatePositionDto) => positionsApi.create(dto),
    onSuccess: () => qc.invalidateQueries({ queryKey: POSITIONS_KEY }),
  });
}

export function useDeletePosition() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (id: string) => positionsApi.delete(id),
    onSuccess: () => qc.invalidateQueries({ queryKey: POSITIONS_KEY }),
  });
}

export function useTemplate(positionId: string) {
  return useQuery({
    queryKey: templateKey(positionId),
    queryFn: () => positionsApi.getTemplate(positionId),
    enabled: !!positionId,
  });
}

export function useUpdateDuration(positionId: string) {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (durationMinutes: number) => positionsApi.updateDuration(positionId, durationMinutes),
    onSuccess: (data) => qc.setQueryData(templateKey(positionId), data),
  });
}

export function useAddPartition(positionId: string) {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (dto: UpsertPartitionDto) => positionsApi.addPartition(positionId, dto),
    onSuccess: (data) => qc.setQueryData(templateKey(positionId), data),
  });
}

export function useUpdatePartition(positionId: string) {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: ({ partitionId, dto }: { partitionId: string; dto: UpsertPartitionDto }) =>
      positionsApi.updatePartition(positionId, partitionId, dto),
    onSuccess: (data) => qc.setQueryData(templateKey(positionId), data),
  });
}

export function useDeletePartition(positionId: string) {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (partitionId: string) => positionsApi.deletePartition(positionId, partitionId),
    onSuccess: () => qc.invalidateQueries({ queryKey: templateKey(positionId) }),
  });
}

export function useAddQuestion(positionId: string, partitionId: string) {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (dto: UpsertQuestionDto) => positionsApi.addQuestion(positionId, partitionId, dto),
    onSuccess: (data) => qc.setQueryData(templateKey(positionId), data),
  });
}

export function useUpdateQuestion(positionId: string, partitionId: string) {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: ({ questionId, dto }: { questionId: string; dto: UpsertQuestionDto }) =>
      positionsApi.updateQuestion(positionId, partitionId, questionId, dto),
    onSuccess: (data) => qc.setQueryData(templateKey(positionId), data),
  });
}

export function useDeleteQuestion(positionId: string, partitionId: string) {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (questionId: string) =>
      positionsApi.deleteQuestion(positionId, partitionId, questionId),
    onSuccess: () => qc.invalidateQueries({ queryKey: templateKey(positionId) }),
  });
}

export function useReorderQuestions(positionId: string, partitionId: string) {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (questionIds: string[]) =>
      positionsApi.reorderQuestions(positionId, partitionId, questionIds),
    onSuccess: (data) => qc.setQueryData(templateKey(positionId), data),
  });
}
