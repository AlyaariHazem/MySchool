export type TimeCapsulePhase =
  | 'LockedNoResignation'
  | 'ResignationPending'
  | 'ResignationRejected'
  | 'UnlockPending'
  | 'UnlockRejected'
  | 'Unlocked';

export interface TimeCapsuleStatusDto {
  employeeProfileId: number;
  timeCapsuleId?: number | null;
  phase: TimeCapsulePhase | string;
  messageAr: string;
  resignationRequestId?: number | null;
  resignationStatus?: number | null;
  pendingUnlockApprovalId?: number | null;
  isUnlocked: boolean;
}

export interface TimeCapsuleSectionReadDto {
  timeCapsuleSectionId: number;
  sectionType: number;
  title: string;
  dataJson: string;
  sortOrder: number;
}

export interface TimeCapsuleDetailDto {
  timeCapsuleId: number;
  employeeProfileId: number;
  schoolId: number;
  isLocked: boolean;
  unlockedAtUtc?: string | null;
  unlockedByUserId?: string | null;
  unlockReason?: string | null;
  sections: TimeCapsuleSectionReadDto[];
  narrativeText?: string | null;
  narrativeGeneratedAtUtc?: string | null;
  narrativeGeneratedBy?: number | null;
}

export interface ResignationRequestCreateDto {
  employeeProfileId: number;
  academicYearId: number;
  reason?: string | null;
}
