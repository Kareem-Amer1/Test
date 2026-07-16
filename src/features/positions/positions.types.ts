export type QuestionType = "Essay" | "TrueFalse" | "MCQ";

export interface Position {
  id: string;
  name: string;
  description?: string | null;
  isActive: boolean;
  createdAt: string;
}

export interface McqChoice {
  id: string;
  text: string;
}

export interface TemplateQuestion {
  id: string;
  type: QuestionType;
  text: string;
  points: number;
  correctAnswer?: boolean | null;
  choices?: McqChoice[] | null;
  correctChoiceId?: string | null;
  order: number;
}

export interface ExamTemplate {
  id: string;
  positionId: string;
  durationMinutes: number;
  questions: TemplateQuestion[];
  lastModifiedAt: string;
}

export interface CreatePositionDto {
  name: string;
  description?: string;
}

export interface UpsertQuestionDto {
  type: QuestionType;
  text: string;
  points: number;
  correctAnswer?: boolean | null;
  choices?: { id?: string; text: string }[];
  correctChoiceId?: string | null;
}
