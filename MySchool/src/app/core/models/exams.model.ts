export interface ExamType {
  examTypeID: number;
  name: string;
  sortOrder: number;
  isActive: boolean;
}

export interface ExamSession {
  examSessionID: number;
  yearID: number;
  termID: number;
  name: string;
  isActive: boolean;
}

export interface ScheduledExamList {
  scheduledExamID: number;
  examSessionID?: number | null;
  examTypeID: number;
  examTypeName?: string | null;
  yearID: number;
  termID: number;
  classID: number;
  className?: string | null;
  divisionID: number;
  divisionName?: string | null;
  subjectID: number;
  subjectName?: string | null;
  teacherID: number;
  teacherName?: string | null;
  examDate: string;
  startTime: string;
  endTime: string;
  room?: string | null;
  totalMarks: number;
  passingMarks: number;
  schedulePublished: boolean;
  resultsPublished: boolean;
  notes?: string | null;
}

export interface CreateScheduledExam {
  examSessionID?: number | null;
  examTypeID: number;
  yearID: number;
  termID: number;
  classID: number;
  divisionID: number;
  subjectID: number;
  teacherID: number;
  examDate: string;
  startTime: string;
  endTime: string;
  room?: string | null;
  totalMarks: number;
  passingMarks: number;
  schedulePublished: boolean;
  resultsPublished: boolean;
  notes?: string | null;
}

export interface ExamResultRow {
  examResultID: number;
  studentID: number;
  studentName?: string | null;
  score?: number | null;
  isAbsent: boolean;
  remarks?: string | null;
}

export interface StudentExamCard {
  scheduledExamID: number;
  examTypeName: string;
  subjectName: string;
  className: string;
  divisionName: string;
  examDate: string;
  startTime: string;
  endTime: string;
  room?: string | null;
  schedulePublished: boolean;
  resultsPublished: boolean;
  totalMarks: number;
  passingMarks: number;
  score?: number | null;
  isAbsent: boolean;
  remarks?: string | null;
  passed: boolean;
}
