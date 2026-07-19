import { api } from "@/lib/apiClient";
import { publicApi } from "@/lib/publicApiClient";
import type { ExamAnswerInput, ExamSession, SubmitExamResult } from "@/features/exams/exams.types";
import type {
  CreateInvitationDto,
  CreateInvitationResult,
  InvitationListItem,
  InviteInfo,
  StartCandidateExamDto,
} from "./invitations.types";

export const invitationsApi = {
  create: (dto: CreateInvitationDto) =>
    api.post<CreateInvitationResult>("/invitations", dto),
  list: () => api.get<InvitationListItem[]>("/invitations"),
};

export const candidateInviteApi = {
  getInfo: (token: string) => publicApi.get<InviteInfo>(`/invite/${token}`),
  start: (token: string, dto: StartCandidateExamDto) =>
    publicApi.post<ExamSession>(`/invite/${token}/start`, dto),
  getSession: (token: string) => publicApi.get<ExamSession>(`/invite/${token}/session`),
  saveAnswers: (token: string, answers: ExamAnswerInput[], elapsedSeconds: number) =>
    publicApi.put<ExamSession>(`/invite/${token}/answers`, { answers, elapsedSeconds }),
  submit: (token: string, answers: ExamAnswerInput[], elapsedSeconds: number) =>
    publicApi.post<SubmitExamResult>(`/invite/${token}/submit`, { answers, elapsedSeconds }),
};
