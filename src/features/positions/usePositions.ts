import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { positionsApi } from "./positions.api";
import type { CreatePositionDto, UpsertQuestionDto } from "./positions.types";

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

export function useAddQuestion(positionId: string) {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (dto: UpsertQuestionDto) => positionsApi.addQuestion(positionId, dto),
    onSuccess: (data) => qc.setQueryData(templateKey(positionId), data),
  });
}

export function useUpdateQuestion(positionId: string) {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: ({ questionId, dto }: { questionId: string; dto: UpsertQuestionDto }) =>
      positionsApi.updateQuestion(positionId, questionId, dto),
    onSuccess: (data) => qc.setQueryData(templateKey(positionId), data),
  });
}

export function useDeleteQuestion(positionId: string) {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (questionId: string) => positionsApi.deleteQuestion(positionId, questionId),
    onSuccess: () => qc.invalidateQueries({ queryKey: templateKey(positionId) }),
  });
}

export function useReorderQuestions(positionId: string) {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (questionIds: string[]) => positionsApi.reorderQuestions(positionId, questionIds),
    onSuccess: (data) => qc.setQueryData(templateKey(positionId), data),
  });
}
