/** Mirrors backend HomeworkSubmissionStatus (byte). */
export const HomeworkSubmissionStatus = {
  Pending: 0,
  Submitted: 1,
  Late: 2,
  Graded: 3,
  Completed: 4,
  Missing: 5,
} as const;

export function homeworkStatusLabelAr(status: number): string {
  switch (status) {
    case HomeworkSubmissionStatus.Pending:
      return 'معلق';
    case HomeworkSubmissionStatus.Submitted:
      return 'مُسلَّم';
    case HomeworkSubmissionStatus.Late:
      return 'متأخر';
    case HomeworkSubmissionStatus.Graded:
      return 'مقيَّم';
    case HomeworkSubmissionStatus.Completed:
      return 'مُكتمل';
    case HomeworkSubmissionStatus.Missing:
      return 'مفقود';
    default:
      return String(status);
  }
}

export interface HomeworkTaskLinkDto {
  homeworkTaskLinkID: number;
  url: string;
  label?: string | null;
  sortOrder: number;
}

export interface HomeworkTaskList {
  homeworkTaskID: number;
  teacherID: number;
  teacherName?: string | null;
  yearID: number;
  termID: number;
  classID: number;
  className?: string | null;
  divisionID: number;
  divisionName?: string | null;
  subjectID: number;
  subjectName?: string | null;
  title: string;
  dueDateUtc: string;
  submissionRequired: boolean;
  submissionCount: number;
  pendingCount: number;
  createdAtUtc: string;
}

export interface HomeworkTaskDetail extends HomeworkTaskList {
  description?: string | null;
  links: HomeworkTaskLinkDto[];
}

export interface CreateHomeworkTask {
  teacherID?: number | null;
  /** Set only by the server from the active academic year; omit when creating/updating. */
  yearID?: number;
  termID: number;
  classID: number;
  divisionID: number;
  subjectID: number;
  title: string;
  description?: string | null;
  dueDateUtc: string;
  submissionRequired: boolean;
  links?: { url: string; label?: string | null; sortOrder: number }[];
}

export interface HomeworkSubmissionFileDto {
  homeworkSubmissionFileID: number;
  fileUrl: string;
  fileName?: string | null;
}

export interface HomeworkSubmissionRow {
  homeworkSubmissionID: number;
  studentID: number;
  studentName?: string | null;
  status: number;
  submittedAtUtc?: string | null;
  answerText?: string | null;
  files: HomeworkSubmissionFileDto[];
  teacherFeedback?: string | null;
  score?: number | null;
  feedbackPublished: boolean;
}

export interface ReviewHomeworkSubmission {
  status: number;
  teacherFeedback?: string | null;
  score?: number | null;
  feedbackPublished: boolean;
}

export interface StudentHomeworkItem {
  homeworkTaskID: number;
  homeworkSubmissionID: number;
  title: string;
  subjectName?: string | null;
  className?: string | null;
  divisionName?: string | null;
  dueDateUtc: string;
  submissionRequired: boolean;
  status: number;
  submittedAtUtc?: string | null;
  teacherFeedback?: string | null;
  score?: number | null;
  feedbackPublished: boolean;
}

export interface StudentHomeworkDetail extends StudentHomeworkItem {
  description?: string | null;
  taskLinks?: HomeworkTaskLinkDto[];
  answerText?: string | null;
  files?: HomeworkSubmissionFileDto[];
}

export interface StudentSubmitHomework {
  answerText?: string | null;
  files?: { fileUrl: string; fileName?: string | null }[];
}

export interface HomeworkActivitySummary {
  taskCount: number;
  missingSubmissionCount: number;
  gradedCount: number;
  teachers: HomeworkTeacherActivity[];
}

export interface HomeworkTeacherActivity {
  teacherID: number;
  teacherName?: string | null;
  tasksCreated: number;
}
