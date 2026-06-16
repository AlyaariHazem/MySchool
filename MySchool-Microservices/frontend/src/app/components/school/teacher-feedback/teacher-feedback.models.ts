/** Mirrors backend <c>TeacherFeedbackCycleStatus</c>. */
export enum TeacherFeedbackCycleStatus {
  Draft = 0,
  Active = 1,
  Closed = 2,
}

export enum FeedbackQuestionType {
  Rating1To5 = 1,
  Text = 2,
  YesNo = 3,
}

export enum FeedbackQuestionAudience {
  StudentsOnly = 1,
  ParentsOnly = 2,
  Both = 3,
}

export enum FeedbackSubmissionStatus {
  Draft = 0,
  Submitted = 1,
}

export interface TeacherFeedbackCycleFilterDto {
  schoolID?: number | null;
  academicYearID?: number | null;
  teacherID?: number | null;
  status?: number | null;
}

export interface FeedbackQuestionDto {
  feedbackQuestionID: number;
  teacherFeedbackCycleID: number;
  sortOrder: number;
  questionText: string;
  questionType: number;
  audience: number;
  isRequired: boolean;
}

export interface FeedbackQuestionWriteDto {
  feedbackQuestionID?: number | null;
  sortOrder: number;
  questionText: string;
  questionType: number;
  audience: number;
  isRequired: boolean;
}

export interface TeacherFeedbackCycleListItemDto {
  teacherFeedbackCycleID: number;
  schoolID: number;
  academicYearID: number;
  teacherID: number;
  teacherName?: string | null;
  title: string;
  opensAtUtc: string;
  closesAtUtc: string;
  status: number;
  questionCount: number;
  studentSubmittedCount: number;
  parentSubmittedCount: number;
}

export interface FeedbackSummaryDto {
  feedbackSummaryID: number;
  teacherFeedbackCycleID: number;
  audience: number;
  submittedCount: number;
  averageNumericScore?: number | null;
  aggregateJson?: string | null;
  notes?: string | null;
  computedAtUtc: string;
}

export interface TeacherFeedbackCycleDetailDto extends TeacherFeedbackCycleListItemDto {
  description?: string | null;
  questions: FeedbackQuestionDto[];
  summaries: FeedbackSummaryDto[];
}

export interface TeacherFeedbackCycleWriteDto {
  schoolID: number;
  academicYearID: number;
  teacherID: number;
  title: string;
  description?: string | null;
  opensAtUtc: string;
  closesAtUtc: string;
  status: number;
  questions?: FeedbackQuestionWriteDto[];
}

export interface FeedbackResponseItemDto {
  questionId: number;
  rating?: number | null;
  text?: string | null;
  yesNo?: boolean | null;
}

export interface StudentFeedbackSubmitDto {
  teacherFeedbackCycleID: number;
  submit: boolean;
  responses: FeedbackResponseItemDto[];
}

export interface ParentFeedbackSubmitDto {
  teacherFeedbackCycleID: number;
  studentID: number;
  submit: boolean;
  responses: FeedbackResponseItemDto[];
}

export interface TeacherFeedbackOpenCycleDto {
  teacherFeedbackCycleID: number;
  title: string;
  teacherName?: string | null;
  closesAtUtc: string;
}

export interface TeacherFeedbackParticipantFormDto {
  teacherFeedbackCycleID: number;
  title: string;
  teacherName?: string | null;
  closesAtUtc: string;
  questions: FeedbackQuestionDto[];
  existingResponses?: FeedbackResponseItemDto[] | null;
  submissionStatus: number;
}
