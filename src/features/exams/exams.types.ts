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
}

export interface ExamListItem {
  id: string;
  candidateName: string;
  positionName: string;
  status: ExamStatus;
  startedAt: string;
  submittedAt?: string | null;
  totalScore?: number | null;
  maxScore: number;
  conductedByName: string;
}
