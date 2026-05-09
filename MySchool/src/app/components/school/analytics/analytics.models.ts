export enum AnalyticsPeriodKind {
  Daily = 1,
  Weekly = 2,
  Monthly = 3,
  Termly = 4,
  Yearly = 5,
}

export enum DashboardAudience {
  TopManagement = 1,
  EducationalSupervisor = 2,
  AdministrativeSupervisor = 3,
  EmployeeSelf = 4,
  School = 5,
  YearComparison = 6,
}

export interface KpiDefinitionDto {
  kpiDefinitionID: number;
  schoolID: number;
  code: string;
  title: string;
  description?: string | null;
  unit?: string | null;
  higherIsBetter: boolean;
  targetValue?: number | null;
  isActive: boolean;
}

export interface KpiSnapshotDto {
  kpiSnapshotID: number;
  kpiDefinitionID: number;
  kpiTitle?: string;
  schoolID: number;
  academicYearID?: number | null;
  termID?: number | null;
  employeeProfileID?: number | null;
  departmentName?: string | null;
  periodKind: AnalyticsPeriodKind;
  periodStartUtc: string;
  periodEndUtc: string;
  value: number;
  targetValue?: number | null;
  recordedAtUtc: string;
}

export interface DepartmentAnalyticsDto {
  departmentAnalyticsID: number;
  schoolID: number;
  employeeJobTypeID?: number | null;
  departmentName: string;
  periodKind: AnalyticsPeriodKind;
  periodStartUtc: string;
  periodEndUtc: string;
  kpiCount: number;
  averageScore?: number | null;
  targetAchievementPercent?: number | null;
  violationCount?: number;
  achievementCount?: number;
  activityCount?: number;
  complaintCount?: number;
  employeeCount?: number;
  performanceLevel?: string | null;
  computedAtUtc: string;
}

export interface TeacherAnalyticsDto {
  teacherAnalyticsID: number;
  schoolID: number;
  employeeProfileID: number;
  employeeName?: string;
  periodKind: AnalyticsPeriodKind;
  periodStartUtc: string;
  periodEndUtc: string;
  kpiCount: number;
  compositeScore?: number | null;
  averageDailyEvaluationScore?: number | null;
  supervisorVisitAverage?: number | null;
  achievementPoints?: number;
  violationPoints?: number;
  activityCount?: number;
  complaintCount?: number;
  trendDirection?: number;
  performanceLevel?: string | null;
  targetAchievementPercent?: number | null;
  computedAtUtc: string;
}

export interface SchoolAnalyticsDto {
  schoolAnalyticsID: number;
  schoolID: number;
  periodKind: AnalyticsPeriodKind;
  periodStartUtc: string;
  periodEndUtc: string;
  kpiCount: number;
  overallScore?: number | null;
  averageTeacherScore?: number | null;
  totalViolations?: number;
  totalAchievements?: number;
  totalActivities?: number;
  totalComplaints?: number;
  employeeCount?: number;
  activeTeacherCount?: number;
  riskLevel?: number;
  targetAchievementPercent?: number | null;
  computedAtUtc: string;
}

export interface TrendAnalysisDto {
  trendAnalysisID: number;
  schoolID: number;
  kpiDefinitionID?: number | null;
  kpiTitle?: string;
  metricCode?: string | null;
  entityType?: number;
  entityID?: number | null;
  dashboardAudience: DashboardAudience;
  periodKind: AnalyticsPeriodKind;
  fromUtc: string;
  toUtc: string;
  baselineValue?: number | null;
  currentValue?: number | null;
  deltaValue?: number | null;
  deltaPercent?: number | null;
  isPositiveTrend: boolean;
  trendDirection?: number;
  trendLabel?: string | null;
  interpretation?: string | null;
}

export interface DashboardCardDto {
  code: string;
  label: string;
  value: number;
  target?: number | null;
  trend?: number | null;
}

export interface AnalyticsDashboardDto {
  cards: DashboardCardDto[];
  snapshots: KpiSnapshotDto[];
  trends: TrendAnalysisDto[];
  departments: DepartmentAnalyticsDto[];
  teachers: TeacherAnalyticsDto[];
  school: SchoolAnalyticsDto[];
}

export interface AnalyticsDashboardQuery {
  schoolID?: number | null;
  periodKind?: AnalyticsPeriodKind | null;
  audience: DashboardAudience;
}

export interface AnalyticsGenerateRequest {
  schoolID: number;
  academicYearID?: number | null;
  periodKind: AnalyticsPeriodKind;
  replaceExistingForPeriod?: boolean;
}
