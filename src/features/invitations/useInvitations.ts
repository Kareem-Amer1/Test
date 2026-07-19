import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { candidateInviteApi, invitationsApi } from "./invitations.api";
import type { CreateInvitationDto, StartCandidateExamDto } from "./invitations.types";
import type { ExamAnswerInput } from "@/features/exams/exams.types";

export const INVITATIONS_LIST_KEY = ["invitations", "list"] as const;
export const inviteInfoKey = (token: string) => ["invite", token, "info"] as const;
export const inviteSessionKey = (token: string) => ["invite", token, "session"] as const;

export function useInvitationsList() {
  return useQuery({
    queryKey: INVITATIONS_LIST_KEY,
    queryFn: () => invitationsApi.list(),
    staleTime: 30_000,
  });
}

export function useCreateInvitation() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (dto: CreateInvitationDto) => invitationsApi.create(dto),
    onSuccess: () => void qc.invalidateQueries({ queryKey: INVITATIONS_LIST_KEY }),
  });
}

export function useInviteInfo(token: string) {
  return useQuery({
    queryKey: inviteInfoKey(token),
    queryFn: () => candidateInviteApi.getInfo(token),
    enabled: !!token,
  });
}

export function useCandidateExamSession(token: string, enabled: boolean) {
  return useQuery({
    queryKey: inviteSessionKey(token),
    queryFn: () => candidateInviteApi.getSession(token),
    enabled: !!token && enabled,
    retry: false,
  });
}

export function useStartCandidateExam(token: string) {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (dto: StartCandidateExamDto) => candidateInviteApi.start(token, dto),
    onSuccess: (data) => {
      qc.setQueryData(inviteSessionKey(token), data);
      void qc.invalidateQueries({ queryKey: inviteInfoKey(token) });
    },
  });
}

export function useSaveCandidateAnswers(token: string) {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: ({ answers, elapsedSeconds }: { answers: ExamAnswerInput[]; elapsedSeconds: number }) =>
      candidateInviteApi.saveAnswers(token, answers, elapsedSeconds),
    onSuccess: (data) => qc.setQueryData(inviteSessionKey(token), data),
  });
}

export function useSubmitCandidateExam(token: string) {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: ({ answers, elapsedSeconds }: { answers: ExamAnswerInput[]; elapsedSeconds: number }) =>
      candidateInviteApi.submit(token, answers, elapsedSeconds),
    onSuccess: () => {
      void qc.invalidateQueries({ queryKey: inviteInfoKey(token) });
      void qc.invalidateQueries({ queryKey: inviteSessionKey(token) });
    },
  });
}
