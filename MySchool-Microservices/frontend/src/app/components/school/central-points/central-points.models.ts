export interface PointsSourceDto {
  pointsSourceID: number;
  code: string;
  displayName: string;
  description?: string | null;
  sortOrder: number;
  isActive: boolean;
}

export interface PointsRuleDto {
  pointsRuleID: number;
  schoolID?: number | null;
  academicYearID?: number | null;
  pointsSourceID: number;
  pointsSourceCode: string;
  ruleKey: string;
  deltaPoints: number;
  priority: number;
  isActive: boolean;
  effectiveFromUtc?: string | null;
  effectiveToUtc?: string | null;
}

export interface PointsRuleWriteDto {
  schoolID?: number | null;
  pointsSourceID: number;
  ruleKey: string;
  deltaPoints: number;
  priority: number;
  isActive: boolean;
  effectiveFromUtc?: string | null;
  effectiveToUtc?: string | null;
}

export interface PointsRuleFilterDto {
  schoolID?: number | null;
  pointsSourceID?: number | null;
  activeOnly?: boolean | null;
}

export interface PointsLedgerFilterDto {
  schoolID?: number | null;
  employeeProfileID?: number | null;
  pointsSourceID?: number | null;
  fromUtc?: string | null;
  toUtc?: string | null;
  skip?: number;
  take?: number;
}

export interface PointsLedgerListItemDto {
  pointsLedgerID: number;
  pointsTransactionID: number;
  employeeProfileID: number;
  employeeDisplayName?: string | null;
  schoolID: number;
  academicYearID: number;
  pointsSourceCode: string;
  pointsRuleID?: number | null;
  deltaPoints: number;
  memo?: string | null;
  createdAtUtc: string;
  correlationEntityType?: string | null;
  correlationEntityID?: number | null;
  idempotencyKey?: string | null;
}

export interface PointsBalanceDto {
  employeeProfileID: number;
  schoolID: number;
  academicYearID: number;
  totalPoints: number;
  updatedAtUtc: string;
}

export interface PostCentralPointsDto {
  employeeProfileID: number;
  schoolID: number;
  pointsSourceCode: string;
  ruleKey: string;
  overrideDeltaPoints?: number | null;
  correlationEntityType?: string | null;
  correlationEntityID?: number | null;
  idempotencyKey?: string | null;
  notes?: string | null;
  memo?: string | null;
}

export interface PostCentralPointsResultDto {
  pointsTransactionID: number;
  pointsLedgerID: number;
  appliedDeltaPoints: number;
  matchedPointsRuleID?: number | null;
  newBalanceTotal: number;
  wasIdempotentReplay: boolean;
}

export function postCentralPointsForApi(d: PostCentralPointsDto): Record<string, unknown> {
  const o: Record<string, unknown> = {
    employeeProfileID: d.employeeProfileID,
    schoolID: d.schoolID,
    pointsSourceCode: d.pointsSourceCode,
    ruleKey: d.ruleKey ?? '*',
  };
  if (d.overrideDeltaPoints != null) o['overrideDeltaPoints'] = d.overrideDeltaPoints;
  if (d.correlationEntityType?.trim()) o['correlationEntityType'] = d.correlationEntityType.trim();
  if (d.correlationEntityID != null) o['correlationEntityID'] = d.correlationEntityID;
  if (d.idempotencyKey?.trim()) o['idempotencyKey'] = d.idempotencyKey.trim();
  if (d.notes?.trim()) o['notes'] = d.notes.trim();
  if (d.memo?.trim()) o['memo'] = d.memo.trim();
  return o;
}
