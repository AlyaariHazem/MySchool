export enum AwardCycleKind {
  Week = 1,
  Month = 2,
  Term = 3,
  Year = 4,
}

export enum AwardCycleStatus {
  Draft = 0,
  Open = 1,
  NominationsClosed = 2,
  Completed = 3,
}

export enum AwardNominationStatus {
  Pending = 0,
  Shortlisted = 1,
  Rejected = 2,
  Withdrawn = 3,
}

export interface AwardDto {
  awardID: number;
  schoolID: number;
  code: string;
  title: string;
  description?: string | null;
  cycleKind: AwardCycleKind;
  isActive: boolean;
  sortOrder: number;
}

export interface AwardWriteDto {
  schoolID: number;
  code: string;
  title: string;
  description?: string | null;
  cycleKind: AwardCycleKind;
  isActive: boolean;
  sortOrder: number;
}

export interface AwardCycleDto {
  awardCycleID: number;
  awardID: number;
  awardTitle?: string;
  academicYearID: number;
  termID?: number | null;
  periodStartUtc: string;
  periodEndUtc: string;
  status: AwardCycleStatus;
}

export interface AwardCycleWriteDto {
  awardID: number;
  academicYearID: number;
  termID?: number | null;
  periodStartUtc: string;
  periodEndUtc: string;
  status: AwardCycleStatus;
}

export interface AwardNominationDto {
  awardNominationID: number;
  awardCycleID: number;
  studentID: number;
  studentName?: string;
  nominatedByEmployeeProfileID?: number | null;
  nominatedByEmployeeName?: string;
  notes?: string | null;
  status: AwardNominationStatus;
  createdAtUtc: string;
}

export interface AwardWinnerDto {
  awardWinnerID: number;
  awardCycleID: number;
  studentID: number;
  studentName?: string;
  rank: number;
  selectedByEmployeeProfileID?: number | null;
  selectedByEmployeeName?: string;
  notes?: string | null;
  selectedAtUtc: string;
}
