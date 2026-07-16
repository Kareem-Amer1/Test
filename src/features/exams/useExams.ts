import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { examsApi } from "./exams.api";
import type { CreateExamDto, ExamAnswerInput, ExamListFilters, GradeExamDto } from "./exams.types";

export const examSessionKey = (id: string) => ["exams", id, "session"] as const;
export const examDetailKey = (id: string) => ["exams", id, "detail"] as const;
export const EXAMS_LIST_KEY = ["exams", "list"] as const;

export function examsListKey(filters?: ExamListFilters) {
  return [...EXAMS_LIST_KEY, filters ?? {}] as const;
}

export function useExamsList(filters?: ExamListFilters) {
  return useQuery({
    queryKey: examsListKey(filters),
    queryFn: () => examsApi.list(filters),
    staleTime: 30_000,
  });
}

export function useExamDetail(examId: string) {
  return useQuery({
    queryKey: examDetailKey(examId),
    queryFn: () => examsApi.getDetail(examId),
    enabled: !!examId,
  });
}

export function useCreateExam() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (dto: CreateExamDto) => examsApi.create(dto),
    onSuccess: () => void qc.invalidateQueries({ queryKey: EXAMS_LIST_KEY }),
  });
}

export function useExamSession(examId: string) {
  return useQuery({
    queryKey: examSessionKey(examId),
    queryFn: () => examsApi.getSession(examId),
    enabled: !!examId,
  });
}

export function useSaveExamAnswers(examId: string) {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (answers: ExamAnswerInput[]) => examsApi.saveAnswers(examId, answers),
    onSuccess: (data) => qc.setQueryData(examSessionKey(examId), data),
  });
}

export function useSubmitExam(examId: string) {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (answers: ExamAnswerInput[]) => examsApi.submit(examId, answers),
    onSuccess: () => {
      void qc.invalidateQueries({ queryKey: examSessionKey(examId) });
      void qc.invalidateQueries({ queryKey: EXAMS_LIST_KEY });
    },
  });
}

export function useGradeExam(examId: string) {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (dto: GradeExamDto) => examsApi.grade(examId, dto),
    onSuccess: () => {
      void qc.invalidateQueries({ queryKey: examDetailKey(examId) });
      void qc.invalidateQueries({ queryKey: EXAMS_LIST_KEY });
    },
  });
}
