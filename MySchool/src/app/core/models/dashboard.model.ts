export interface DashboardSummary {
  totalMoney: number;
  parentsCount: number;
  teachersCount: number;
  studentsCount: number;
}

export interface StudentEnrollmentTrend {
  year: number;
  studentCount: number;
}

export interface DashboardResult {
  summary: DashboardSummary;
  recentExams: any[];
  studentEnrollmentTrend: StudentEnrollmentTrend[];
}

export interface DashboardResponse {
  statusCode: number;
  isSuccess: boolean;
  errorMasseges: string[];
  result: DashboardResult;
}

