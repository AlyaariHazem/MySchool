/** Mirrors backend enums (int). */
export enum SupervisorVisitStatus {
  Draft = 0,
  Submitted = 1,
  Archived = 2,
}

export enum RecommendationImplementationStatus {
  Pending = 0,
  InProgress = 1,
  Completed = 2,
  Deferred = 3,
  NotApplicable = 4,
}

export interface SupervisorVisitFilterDto {
  schoolID?: number | null;
  academicYearID?: number | null;
  visitedTeacherID?: number | null;
  fromDate?: string | null;
  toDate?: string | null;
}

export interface SupervisorVisitListItemDto {
  supervisorVisitID: number;
  schoolID: number;
  academicYearID: number;
  visitedTeacherID: number;
  visitedTeacherName: string;
  classID?: number | null;
  className?: string | null;
  subjectID?: number | null;
  subjectName?: string | null;
  supervisorEmployeeProfileID: number;
  supervisorName: string;
  visitDate: string;
  status: number;
  overallScoreOutOf100: number;
}

export interface RecommendationFollowUpReadDto {
  recommendationFollowUpID: number;
  visitRecommendationID: number;
  followUpNote: string;
  followUpDate: string;
  followUpByEmployeeProfileID?: number | null;
  createdAtUtc: string;
}

export interface VisitObservationReadDto {
  visitObservationID: number;
  supervisorVisitID: number;
  category?: string | null;
  observationText: string;
  sortOrder: number;
  createdAtUtc: string;
}

export interface VisitRecommendationReadDto {
  visitRecommendationID: number;
  supervisorVisitID: number;
  recommendationText: string;
  implementationStatus: number;
  dueDate?: string | null;
  completedAtUtc?: string | null;
  sortOrder: number;
  createdAtUtc: string;
  updatedAtUtc: string;
  followUps: RecommendationFollowUpReadDto[];
}

export interface SupervisorVisitDetailDto extends SupervisorVisitListItemDto {
  summaryNotes?: string | null;
  createdAtUtc: string;
  updatedAtUtc: string;
  observations: VisitObservationReadDto[];
  recommendations: VisitRecommendationReadDto[];
}

export interface RecommendationFollowUpWriteDto {
  followUpNote: string;
  followUpDate: string;
  followUpByEmployeeProfileID?: number | null;
}

export interface VisitObservationWriteDto {
  category?: string | null;
  observationText: string;
  sortOrder: number;
}

export interface VisitRecommendationWriteDto {
  recommendationText: string;
  implementationStatus: number;
  dueDate?: string | null;
  /** ISO date-time when marked completed (optional). */
  completedAtUtc?: string | null;
  sortOrder: number;
  followUps: RecommendationFollowUpWriteDto[];
}

export interface SupervisorVisitWriteDto {
  schoolID: number;
  academicYearID: number;
  visitedTeacherID: number;
  classID?: number | null;
  subjectID?: number | null;
  supervisorEmployeeProfileID: number;
  visitDate: string;
  status: number;
  overallScoreOutOf100: number;
  summaryNotes?: string | null;
  observations: VisitObservationWriteDto[];
  recommendations: VisitRecommendationWriteDto[];
}

export function supervisorVisitFilterForApi(f: SupervisorVisitFilterDto): Record<string, unknown> {
  return {
    schoolID: f.schoolID ?? null,
    academicYearID: f.academicYearID ?? null,
    visitedTeacherID: f.visitedTeacherID ?? null,
    fromDate: f.fromDate ?? null,
    toDate: f.toDate ?? null,
  };
}

export function supervisorVisitWriteForApi(d: SupervisorVisitWriteDto): Record<string, unknown> {
  return {
    schoolID: d.schoolID,
    academicYearID: d.academicYearID,
    visitedTeacherID: d.visitedTeacherID,
    classID: d.classID ?? null,
    subjectID: d.subjectID ?? null,
    supervisorEmployeeProfileID: d.supervisorEmployeeProfileID,
    visitDate: d.visitDate,
    status: d.status,
    overallScoreOutOf100: d.overallScoreOutOf100,
    summaryNotes: d.summaryNotes ?? null,
    observations: (d.observations ?? []).map((o) => ({
      category: o.category ?? null,
      observationText: o.observationText,
      sortOrder: o.sortOrder,
    })),
    recommendations: (d.recommendations ?? []).map((r) => ({
      recommendationText: r.recommendationText,
      implementationStatus: r.implementationStatus,
      dueDate: r.dueDate != null && String(r.dueDate).trim() !== '' ? r.dueDate : null,
      completedAtUtc: r.completedAtUtc != null && String(r.completedAtUtc).trim() !== '' ? r.completedAtUtc : null,
      sortOrder: r.sortOrder,
      followUps: (r.followUps ?? []).map((f) => ({
        followUpNote: f.followUpNote,
        followUpDate: f.followUpDate,
        followUpByEmployeeProfileID: f.followUpByEmployeeProfileID ?? null,
      })),
    })),
  };
}
