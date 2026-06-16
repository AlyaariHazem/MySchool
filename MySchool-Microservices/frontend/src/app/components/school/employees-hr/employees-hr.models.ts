import { isSchoolManagerUser } from 'app/core/utils/school-role.util';

/** Mirrors backend enums (int). */
export enum EmploymentStatus {
  Active = 1,
  OnLeave = 2,
  Suspended = 3,
  Terminated = 4,
}

export enum LeaveType {
  Annual = 1,
  Sick = 2,
  Unpaid = 3,
  Emergency = 4,
  Other = 99,
}

export enum ApprovalStatus {
  Pending = 1,
  Approved = 2,
  Rejected = 3,
  Cancelled = 4,
}

/** Matches GET /api/employees/job-types (camelCase JSON). */
export interface EmployeeJobTypeDto {
  employeeJobTypeID: number;
  code: string;
  name: string;
  nameAr?: string | null;
  sortOrder: number;
  isActive: boolean;
}

export interface EmployeeNameDto {
  firstName: string;
  middleName?: string | null;
  lastName: string;
}

export interface EmployeeNameAlisDto {
  firstNameEng?: string | null;
  middleNameEng?: string | null;
  lastNameEng?: string | null;
}

export interface EmployeeProfileReadDto {
  employeeProfileID: number;
  userId?: string | null;
  schoolID: number;
  currentAcademicYearID: number;
  employeeJobTypeID: number;
  jobTypeCode: string;
  jobTypeName: string;
  employeeCode: string;
  fullName: EmployeeNameDto;
  fullNameAlis?: EmployeeNameAlisDto | null;
  nationalId?: string | null;
  dateOfBirth?: string | null;
  gender?: string | null;
  phone?: string | null;
  email?: string | null;
  address?: string | null;
  hireDate?: string | null;
  employmentStatus: EmploymentStatus;
  notes?: string | null;
  isActive: boolean;
  createdAtUtc: string;
  updatedAtUtc: string;
  teacherID?: number | null;
  managerID?: number | null;
  schoolStaffID?: number | null;
}

export interface EmployeeProfileCreateDto {
  userId?: string | null;
  /** Omitted for school managers — API resolves from the logged-in manager's school and active year. */
  schoolID?: number;
  currentAcademicYearID?: number;
  employeeJobTypeID: number;
  /** Omitted on create — API assigns next numeric code per school. Required on update when sending full body. */
  employeeCode?: string;
  fullName: EmployeeNameDto;
  fullNameAlis?: EmployeeNameAlisDto | null;
  nationalId?: string | null;
  dateOfBirth?: string | null;
  gender?: string | null;
  phone?: string | null;
  email?: string | null;
  address?: string | null;
  hireDate?: string | null;
  employmentStatus: EmploymentStatus;
  notes?: string | null;
  isActive: boolean;
  teacherID?: number | null;
  managerID?: number | null;
  schoolStaffID?: number | null;
}

export type EmployeeProfileUpdateDto = EmployeeProfileCreateDto;

export interface EmployeeProfileListFilterDto {
  schoolID?: number | null;
  /** Ignored by the API; the server uses the school's active academic year. */
  academicYearID?: number | null;
  employeeJobTypeID?: number | null;
  isActive?: boolean | null;
  employmentStatus?: EmploymentStatus | null;
}

/** POST /employees/list (full array): active year is server-side. MANAGER: school is server-side — omit from payload. */
export function employeeProfileListFilterForPostApi(f: EmployeeProfileListFilterDto): EmployeeProfileListFilterDto {
  const out = { ...f };
  delete out.academicYearID;
  if (isSchoolManagerUser()) {
    delete out.schoolID;
  }
  return out;
}

/** POST /employees/page and POST /employees/list/page — same filter rules as list. */
export interface EmployeeProfilePageRequestDto {
  pageIndex: number;
  pageSize: number;
  filter?: EmployeeProfileListFilterDto | null;
}

/** Minimal employee row for dropdowns (matches API). */
export interface EmployeeProfileOptionDto {
  id: number;
  fullName: EmployeeNameDto;
}

export function employeeProfilePageRequestForPostApi(req: EmployeeProfilePageRequestDto): EmployeeProfilePageRequestDto {
  return {
    ...req,
    filter: employeeProfileListFilterForPostApi(req.filter ?? {}),
  };
}

export interface EmployeeQualificationDto {
  employeeQualificationID?: number | null;
  degreeName: string;
  major?: string | null;
  institution?: string | null;
  graduationYear?: number | null;
  gradeOrScore?: string | null;
  notes?: string | null;
}

export interface EmployeeSpecializationDto {
  employeeSpecializationID?: number | null;
  name: string;
  category?: string | null;
  level?: string | null;
  notes?: string | null;
}

export interface EmployeeHistoryDto {
  employeeHistoryID?: number | null;
  academicYearID: number;
  schoolID: number;
  employeeJobTypeID?: number | null;
  jobTitle?: string | null;
  department?: string | null;
  startDate?: string | null;
  endDate?: string | null;
  status?: string | null;
  notes?: string | null;
}

export interface EmployeeDocumentDto {
  employeeDocumentID?: number | null;
  documentType: string;
  title: string;
  fileName?: string | null;
  fileUrl?: string | null;
  uploadedAtUtc?: string | null;
  expiryDate?: string | null;
  notes?: string | null;
  isActive: boolean;
}

export interface EmployeeLeaveDto {
  employeeLeaveID?: number | null;
  academicYearID: number;
  leaveType: LeaveType;
  startDate: string;
  endDate: string;
  totalDays: number;
  reason?: string | null;
  approvalStatus: ApprovalStatus;
  approvedByUserId?: string | null;
  approvedAtUtc?: string | null;
  notes?: string | null;
}

export interface EmployeePerformanceSummaryDto {
  employeePerformanceSummaryID?: number | null;
  academicYearID: number;
  schoolID: number;
  employeeJobTypeID?: number | null;
  jobTitle?: string | null;
  evaluationScore?: number | null;
  achievementPoints: number;
  violationPoints: number;
  requestCount: number;
  activityCount: number;
  performanceLevel?: string | null;
  strengthsSummary?: string | null;
  weaknessesSummary?: string | null;
  recommendations?: string | null;
  finalNotes?: string | null;
  generatedAtUtc?: string | null;
}

export interface EmployeeProfileFullDto {
  profile: EmployeeProfileReadDto;
  qualifications: EmployeeQualificationDto[];
  specializations: EmployeeSpecializationDto[];
  historyRecords: EmployeeHistoryDto[];
  documents: EmployeeDocumentDto[];
  leaves: EmployeeLeaveDto[];
  performanceSummaries: EmployeePerformanceSummaryDto[];
}
