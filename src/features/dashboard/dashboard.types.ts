export interface ExamsByPosition {
  positionId: string;
  positionName: string;
  count: number;
}

export interface DashboardStats {
  totalExams: number;
  pendingGrading: number;
  inProgress: number;
  graded: number;
  examsByPosition: ExamsByPosition[];
}
