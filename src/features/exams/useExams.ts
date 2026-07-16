import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { examsApi } from "./exams.api";
import type { CreateExamDto, ExamAnswerInput } from "./exams.types";

export const examSessionKey = (id: string) => ["exams", id, "session"] as const;
export const EXAMS_LIST_KEY = ["exams", "list"] as const;

export function useExamsList() {
  return useQuery({
    queryKey: EXAMS_LIST_KEY,
    queryFn: examsApi.list,
    staleTime: 30_000,
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
    refetchInterval: (query) =>
      query.state.data?.status === "InProgress" ? false : false,
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
