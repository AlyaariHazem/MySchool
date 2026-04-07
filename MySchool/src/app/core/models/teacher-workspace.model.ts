export interface TeacherWorkspaceSummary {
  classCount: number;
  studentCount: number;
  subjectCount: number;
}

export interface RecentCoursePlanRow {
  examId: number;
  date: string;
  time: string;
  divisionName: string;
  className: string;
  subjectName: string;
  examType: string;
}

export interface TeacherWorkspaceResult {
  summary: TeacherWorkspaceSummary;
  recentCoursePlans: RecentCoursePlanRow[];
}
