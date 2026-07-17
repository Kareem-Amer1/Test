export type ExamStatus = "InProgress" | "Submitted" | "Graded";
export type QuestionType = "Essay" | "TrueFalse" | "MCQ";

export interface CreateExamDto {
  positionId: string;
  candidateName: string;
}

export interface CreateExamResult {
  id: string;
  candidateName: string;
  positionName: string;
  status: ExamStatus;
}

export interface ExamSessionChoice {
  id: string;
  text: string;
}

export interface ExamSessionQuestion {
  id: string;
  type: QuestionType;
  text: string;
  points: number;
  choices?: ExamSessionChoice[] | null;
  order: number;
}

export interface ExamAnswerInput {
  questionId: string;
  essayText?: string | null;
  trueFalseAnswer?: boolean | null;
  selectedChoiceId?: string | null;
}

export interface ExamSession {
  id: string;
  candidateName: string;
  positionName: string;
  durationMinutes: number;
  startedAt: string;
  status: ExamStatus;
  questions: ExamSessionQuestion[];
  answers: ExamAnswerInput[];
}

export interface SubmitExamResult {
  id: string;
  status: ExamStatus;
  submittedAt: string;
  autoGradedScore: number;
  isFullyGraded: boolean;
  totalScore?: number | null;
}

export interface ExamListItem {
  id: string;
  candidateName: string;
  positionName: string;
  positionId: string;
  status: ExamStatus;
  startedAt: string;
  submittedAt?: string | null;
  totalScore?: number | null;
  maxScore: number;
  autoGradedScore: number;
  isFullyGraded: boolean;
  conductedByName: string;
}

export interface ExamListFilters {
  positionId?: string;
  status?: ExamStatus | "";
  from?: string;
  to?: string;
  search?: string;
}

export interface ExamAnswerReview {
  questionId: string;
  questionText: string;
  questionType: QuestionType;
  points: number;
  order: number;
  choices?: ExamSessionChoice[] | null;
  candidateAnswer: string;
  correctAnswer: string;
  isCorrect?: boolean | null;
  earnedPoints?: number | null;
  isAutoGraded: boolean;
}

export interface ExamDetail {
  id: string;
  candidateName: string;
  positionName: string;
  status: ExamStatus;
  startedAt: string;
  submittedAt?: string | null;
  totalScore?: number | null;
  maxScore: number;
  autoGradedScore: number;
  isFullyGraded: boolean;
  conductedByName: string;
  questions: ExamAnswerReview[];
}

export interface EssayScoreInput {
  questionId: string;
  earnedPoints: number;
}

export interface GradeExamDto {
  essayScores: EssayScoreInput[];
  finalize: boolean;
}

export interface GradeExamResult {
  id: string;
  status: ExamStatus;
  totalScore?: number | null;
  isFullyGraded: boolean;
}
