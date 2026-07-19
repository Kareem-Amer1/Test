export interface CreateInvitationDto {
  positionId: string;
  linkDurationHours: number;
}

export interface CreateInvitationResult {
  id: string;
  token: string;
  positionName: string;
  linkExpiresAt: string;
  inviteUrl: string;
}

export interface InvitationListItem {
  id: string;
  token: string;
  positionName: string;
  positionId: string;
  linkExpiresAt: string;
  linkDurationHours: number;
  status: InvitationStatus;
  examId?: string | null;
  createdAt: string;
}

export type InvitationStatus = "Pending" | "InProgress" | "Completed" | "Expired";

export interface InviteInfo {
  positionName: string;
  status: InvitationStatus;
  canStart: boolean;
  canResume: boolean;
  isCompleted: boolean;
  isExpired: boolean;
  linkExpiresAt: string;
}

export interface StartCandidateExamDto {
  fullName: string;
  email: string;
  mobile: string;
}

export const LINK_DURATION_OPTIONS = [
  { value: 24, labelKey: "invitations.duration24h", fallback: "24 hours" },
  { value: 48, labelKey: "invitations.duration48h", fallback: "48 hours" },
  { value: 72, labelKey: "invitations.duration72h", fallback: "72 hours" },
  { value: 168, labelKey: "invitations.duration1w", fallback: "1 week" },
] as const;

export function buildInviteUrl(token: string): string {
  return `${window.location.origin}/invite/${token}`;
}
