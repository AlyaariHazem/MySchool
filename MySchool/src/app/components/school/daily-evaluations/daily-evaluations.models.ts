/** Mirrors backend enums (stored as int). */
export enum EvaluationTemplateStatus {
  Draft = 1,
  Active = 2,
  Inactive = 3,
  Archived = 4,
}

export enum DailyEvaluationStatus {
  Draft = 1,
  Submitted = 2,
  Locked = 3,
}

export enum EvaluationLockStatus {
  Open = 1,
  Locked = 2,
  Reopened = 3,
}

export enum EvaluationOverrideActionType {
  EditAfterLock = 1,
  ReopenEvaluation = 2,
  UnlockDay = 3,
  ForceUpdate = 4,
  DeleteAfterLock = 5,
}

export interface DailyEvaluationTemplateFilterDto {
  schoolID?: number;
  academicYearID?: number;
  employeeJobTypeID?: number;
  status?: EvaluationTemplateStatus;
  isActive?: boolean;
}

/** Zero-based page index; matches POST api/daily-evaluations/templates/page. */
export interface DailyEvaluationTemplatesPageRequestDto {
  pageIndex: number;
  pageSize: number;
  filter?: DailyEvaluationTemplateFilterDto | null;
}

/** Mirrors Backend.Common.PagedResult (camelCase JSON). */
export interface PagedResultDto<T> {
  data: T[];
  pageNumber: number;
  pageSize: number;
  totalCount: number;
  totalPages: number;
}

export interface DailyEvaluationTemplateCreateDto {
  schoolID: number;
  academicYearID: number;
  employeeJobTypeID?: number | null;
  name: string;
  description?: string | null;
  effectiveFrom?: string | null;
  effectiveTo?: string | null;
  isDefault: boolean;
}

export interface DailyEvaluationTemplateUpdateDto {
  name: string;
  description?: string | null;
  effectiveFrom?: string | null;
  effectiveTo?: string | null;
  isDefault: boolean;
  isActive: boolean;
}

export interface DailyEvaluationTemplateReadDto {
  dailyEvaluationTemplateID: number;
  schoolID: number;
  academicYearID: number;
  employeeJobTypeID?: number | null;
  name: string;
  description?: string | null;
  status: EvaluationTemplateStatus;
  effectiveFrom?: string | null;
  effectiveTo?: string | null;
  isDefault: boolean;
  isActive: boolean;
  createdAtUtc: string;
  updatedAtUtc: string;
}

export interface DailyEvaluationTemplateListDto {
  dailyEvaluationTemplateID: number;
  schoolID: number;
  academicYearID: number;
  name: string;
  status: EvaluationTemplateStatus;
  isActive: boolean;
  updatedAtUtc: string;
}

export interface DailyEvaluationCriteriaCreateDto {
  name: string;
  description?: string | null;
  weight: number;
  maxScore: number;
  minScore: number;
  isMandatory: boolean;
  sortOrder: number;
  notes?: string | null;
}

export interface DailyEvaluationCriteriaUpdateDto {
  name: string;
  description?: string | null;
  weight: number;
  maxScore: number;
  minScore: number;
  isMandatory: boolean;
  sortOrder: number;
  isActive: boolean;
  notes?: string | null;
}

export interface DailyEvaluationCriteriaReadDto {
  dailyEvaluationCriteriaID: number;
  dailyEvaluationTemplateID: number;
  name: string;
  description?: string | null;
  weight: number;
  maxScore: number;
  minScore: number;
  isMandatory: boolean;
  sortOrder: number;
  isActive: boolean;
  notes?: string | null;
  updatedAtUtc: string;
}

export interface DailyEvaluationFilterDto {
  schoolID?: number;
  academicYearID?: number;
  evaluatedEmployeeProfileID?: number;
  dailyEvaluationTemplateID?: number;
  fromDate?: string | null;
  toDate?: string | null;
  status?: DailyEvaluationStatus;
  evaluatorUserId?: string | null;
}

/** Zero-based page index; matches POST api/daily-evaluations/page. */
export interface DailyEvaluationsPageRequestDto {
  pageIndex: number;
  pageSize: number;
  filter?: DailyEvaluationFilterDto | null;
}

/** GET /daily-evaluations/for-student/teachers */
export interface TeacherEvaluationOptionDto {
  employeeProfileID: number;
  displayName: string;
}

export interface DailyEvaluationCreateDto {
  schoolID: number;
  academicYearID: number;
  evaluatedEmployeeProfileID: number;
  dailyEvaluationTemplateID: number;
  evaluationDate: string;
  evaluatorUserId?: string | null;
  evaluatorEmployeeProfileID?: number | null;
  notes?: string | null;
}

export interface DailyEvaluationUpdateDto {
  notes?: string | null;
}

export interface DailyEvaluationReadDto {
  dailyEvaluationID: number;
  schoolID: number;
  academicYearID: number;
  evaluatedEmployeeProfileID: number;
  evaluatorUserId?: string | null;
  evaluatorEmployeeProfileID?: number | null;
  dailyEvaluationTemplateID: number;
  evaluationDate: string;
  status: DailyEvaluationStatus;
  totalScore: number;
  notes?: string | null;
  submittedAtUtc?: string | null;
  lockedAtUtc?: string | null;
  isLocked: boolean;
  updatedAtUtc: string;
}

export interface DailyEvaluationListDto {
  dailyEvaluationID: number;
  evaluatedEmployeeProfileID: number;
  dailyEvaluationTemplateID: number;
  evaluationDate: string;
  status: DailyEvaluationStatus;
  totalScore: number;
  isLocked: boolean;
  updatedAtUtc: string;
}

export interface DailyEvaluationItemReadDto {
  dailyEvaluationItemID: number;
  dailyEvaluationID: number;
  dailyEvaluationCriteriaID: number;
  criteriaName: string;
  score: number;
  comment?: string | null;
  isMandatorySatisfied: boolean;
}

export interface DailyEvaluationFullDto extends DailyEvaluationReadDto {
  items: DailyEvaluationItemReadDto[];
}

export interface DailyEvaluationItemCreateDto {
  dailyEvaluationCriteriaID: number;
  score: number;
  comment?: string | null;
}

export interface DailyEvaluationItemUpdateDto {
  score: number;
  comment?: string | null;
}

export interface DailyEvaluationItemPatchDto {
  dailyEvaluationItemID: number;
  score: number;
  comment?: string | null;
}

export interface EvaluationOverrideRequestDto {
  reason: string;
  evaluation?: DailyEvaluationUpdateDto | null;
  items?: DailyEvaluationItemPatchDto[] | null;
  notes?: string | null;
}

export interface EvaluationOverrideLogReadDto {
  evaluationOverrideLogID: number;
  dailyEvaluationID?: number | null;
  evaluationLockID?: number | null;
  overrideActionType: EvaluationOverrideActionType;
  reason: string;
  previousValuesJson?: string | null;
  newValuesJson?: string | null;
  performedByUserId: string;
  performedAtUtc: string;
}

export interface EvaluationLockCreateDto {
  schoolID: number;
  academicYearID: number;
  lockDate: string;
  dailyEvaluationTemplateID?: number | null;
  notes?: string | null;
}

export interface EvaluationLockReadDto {
  evaluationLockID: number;
  schoolID: number;
  academicYearID: number;
  lockDate: string;
  dailyEvaluationTemplateID?: number | null;
  status: EvaluationLockStatus;
  lockedAtUtc?: string | null;
  lockedByUserId?: string | null;
  reopenedAtUtc?: string | null;
  reopenedByUserId?: string | null;
  notes?: string | null;
}

export interface EvaluationReopenDto {
  reason: string;
  notes?: string | null;
}
