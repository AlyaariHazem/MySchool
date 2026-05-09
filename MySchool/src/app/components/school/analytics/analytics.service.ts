import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import type { Observable } from 'rxjs';
import { map } from 'rxjs/operators';

import { BackendAspService } from 'app/ASP.NET/backend-asp.service';
import { ApiResponse } from 'app/core/models/response.model';

import {
  AnalyticsDashboardDto,
  AnalyticsDashboardQuery,
  DashboardCardDto,
  DepartmentAnalyticsDto,
  KpiSnapshotDto,
  SchoolAnalyticsDto,
  TeacherAnalyticsDto,
  TrendAnalysisDto,
} from './analytics.models';

function unwrap<T>(body: ApiResponse<T> | Record<string, unknown>): T {
  const b = body as Record<string, unknown>;
  const ok = (b['isSuccess'] ?? b['IsSuccess']) !== false;
  const errs = (b['errorMasseges'] ?? b['ErrorMasseges']) as string[] | undefined;
  if (!ok && errs?.length) throw new Error(errs.join('; '));
  return (b['result'] ?? b['Result']) as T;
}

function num(raw: unknown, fallback = 0): number {
  const n = Number(raw);
  return Number.isFinite(n) ? n : fallback;
}

function str(raw: unknown): string {
  return raw == null ? '' : String(raw);
}

export function readAnalyticsHttpError(err: unknown): string {
  const e = err as HttpErrorResponse;
  const msgs = e?.error?.errorMasseges ?? e?.error?.ErrorMasseges;
  if (Array.isArray(msgs) && msgs.length) return msgs.join('; ');
  if (typeof e?.error?.message === 'string') return e.error.message;
  if (typeof e?.message === 'string') return e.message;
  return 'Request failed';
}

@Injectable({ providedIn: 'root' })
export class AnalyticsService {
  private readonly http = inject(HttpClient);
  private readonly api = inject(BackendAspService);

  private url(path: string): string {
    return `${this.api.baseUrl}/analytics${path}`;
  }

  getDashboard(query: AnalyticsDashboardQuery): Observable<AnalyticsDashboardDto> {
    return this.http.post<ApiResponse<unknown>>(this.url('/dashboard'), query).pipe(
      map((r) => {
        const raw = unwrap<Record<string, unknown>>(r) ?? {};
        return this.normDashboard(raw);
      }),
    );
  }

  private normDashboard(raw: Record<string, unknown>): AnalyticsDashboardDto {
    const cards = (raw['cards'] ?? raw['Cards'] ?? []) as unknown[];
    const snapshots = (raw['snapshots'] ?? raw['Snapshots'] ?? raw['kpiSnapshots'] ?? raw['KpiSnapshots'] ?? []) as unknown[];
    const trends = (raw['trends'] ?? raw['Trends'] ?? []) as unknown[];
    const departments = (raw['departments'] ?? raw['Departments'] ?? raw['departmentAnalytics'] ?? raw['DepartmentAnalytics'] ?? []) as unknown[];
    const teachers = (raw['teachers'] ?? raw['Teachers'] ?? raw['teacherAnalytics'] ?? raw['TeacherAnalytics'] ?? []) as unknown[];
    const school = (raw['school'] ?? raw['School'] ?? raw['schoolAnalytics'] ?? raw['SchoolAnalytics'] ?? []) as unknown[];

    return {
      cards: cards.map((x) => this.normCard(x as Record<string, unknown>)),
      snapshots: snapshots.map((x) => this.normSnapshot(x as Record<string, unknown>)),
      trends: trends.map((x) => this.normTrend(x as Record<string, unknown>)),
      departments: departments.map((x) => this.normDepartment(x as Record<string, unknown>)),
      teachers: teachers.map((x) => this.normTeacher(x as Record<string, unknown>)),
      school: school.map((x) => this.normSchool(x as Record<string, unknown>)),
    };
  }

  private normCard(raw: Record<string, unknown>): DashboardCardDto {
    return {
      code: str(raw['code'] ?? raw['Code']),
      label: str(raw['label'] ?? raw['Label']),
      value: num(raw['value'] ?? raw['Value']),
      target: raw['target'] != null ? num(raw['target'] ?? raw['Target']) : null,
      trend: raw['trend'] != null ? num(raw['trend'] ?? raw['Trend']) : null,
    };
  }

  private normSnapshot(raw: Record<string, unknown>): KpiSnapshotDto {
    return {
      kpiSnapshotID: num(raw['kpiSnapshotID'] ?? raw['KpiSnapshotID']),
      kpiDefinitionID: num(raw['kpiDefinitionID'] ?? raw['KpiDefinitionID']),
      kpiTitle: (raw['kpiTitle'] ?? raw['KpiTitle']) as string | undefined,
      schoolID: num(raw['schoolID'] ?? raw['SchoolID']),
      academicYearID: raw['academicYearID'] != null ? num(raw['academicYearID'] ?? raw['AcademicYearID']) : null,
      termID: raw['termID'] != null ? num(raw['termID'] ?? raw['TermID']) : null,
      employeeProfileID: raw['employeeProfileID'] != null ? num(raw['employeeProfileID'] ?? raw['EmployeeProfileID']) : null,
      departmentName: (raw['departmentName'] ?? raw['DepartmentName']) as string | null | undefined,
      periodKind: num(raw['periodKind'] ?? raw['PeriodKind']) as KpiSnapshotDto['periodKind'],
      periodStartUtc: str(raw['periodStartUtc'] ?? raw['PeriodStartUtc']),
      periodEndUtc: str(raw['periodEndUtc'] ?? raw['PeriodEndUtc']),
      value: num(raw['value'] ?? raw['Value']),
      targetValue: raw['targetValue'] != null ? num(raw['targetValue'] ?? raw['TargetValue']) : null,
      recordedAtUtc: str(raw['recordedAtUtc'] ?? raw['RecordedAtUtc']),
    };
  }

  private normTrend(raw: Record<string, unknown>): TrendAnalysisDto {
    return {
      trendAnalysisID: num(raw['trendAnalysisID'] ?? raw['TrendAnalysisID']),
      schoolID: num(raw['schoolID'] ?? raw['SchoolID']),
      kpiDefinitionID: num(raw['kpiDefinitionID'] ?? raw['KpiDefinitionID']),
      kpiTitle: (raw['kpiTitle'] ?? raw['KpiTitle']) as string | undefined,
      dashboardAudience: num(raw['dashboardAudience'] ?? raw['DashboardAudience']) as TrendAnalysisDto['dashboardAudience'],
      periodKind: num(raw['periodKind'] ?? raw['PeriodKind']) as TrendAnalysisDto['periodKind'],
      fromUtc: str(raw['fromUtc'] ?? raw['FromUtc']),
      toUtc: str(raw['toUtc'] ?? raw['ToUtc']),
      baselineValue: raw['baselineValue'] != null ? num(raw['baselineValue'] ?? raw['BaselineValue']) : null,
      currentValue: raw['currentValue'] != null ? num(raw['currentValue'] ?? raw['CurrentValue']) : null,
      deltaValue: raw['deltaValue'] != null ? num(raw['deltaValue'] ?? raw['DeltaValue']) : null,
      deltaPercent: raw['deltaPercent'] != null ? num(raw['deltaPercent'] ?? raw['DeltaPercent']) : null,
      isPositiveTrend: Boolean(raw['isPositiveTrend'] ?? raw['IsPositiveTrend']),
      trendLabel: (raw['trendLabel'] ?? raw['TrendLabel']) as string | null | undefined,
    };
  }

  private normDepartment(raw: Record<string, unknown>): DepartmentAnalyticsDto {
    return {
      departmentAnalyticsID: num(raw['departmentAnalyticsID'] ?? raw['DepartmentAnalyticsID']),
      schoolID: num(raw['schoolID'] ?? raw['SchoolID']),
      departmentName: str(raw['departmentName'] ?? raw['DepartmentName']),
      periodKind: num(raw['periodKind'] ?? raw['PeriodKind']) as DepartmentAnalyticsDto['periodKind'],
      periodStartUtc: str(raw['periodStartUtc'] ?? raw['PeriodStartUtc']),
      periodEndUtc: str(raw['periodEndUtc'] ?? raw['PeriodEndUtc']),
      kpiCount: num(raw['kpiCount'] ?? raw['KpiCount']),
      averageScore: raw['averageScore'] != null ? num(raw['averageScore'] ?? raw['AverageScore']) : null,
      targetAchievementPercent:
        raw['targetAchievementPercent'] != null
          ? num(raw['targetAchievementPercent'] ?? raw['TargetAchievementPercent'])
          : null,
      computedAtUtc: str(raw['computedAtUtc'] ?? raw['ComputedAtUtc']),
    };
  }

  private normTeacher(raw: Record<string, unknown>): TeacherAnalyticsDto {
    return {
      teacherAnalyticsID: num(raw['teacherAnalyticsID'] ?? raw['TeacherAnalyticsID']),
      schoolID: num(raw['schoolID'] ?? raw['SchoolID']),
      employeeProfileID: num(raw['employeeProfileID'] ?? raw['EmployeeProfileID']),
      employeeName: (raw['employeeName'] ?? raw['EmployeeName']) as string | undefined,
      periodKind: num(raw['periodKind'] ?? raw['PeriodKind']) as TeacherAnalyticsDto['periodKind'],
      periodStartUtc: str(raw['periodStartUtc'] ?? raw['PeriodStartUtc']),
      periodEndUtc: str(raw['periodEndUtc'] ?? raw['PeriodEndUtc']),
      kpiCount: num(raw['kpiCount'] ?? raw['KpiCount']),
      compositeScore: raw['compositeScore'] != null ? num(raw['compositeScore'] ?? raw['CompositeScore']) : null,
      targetAchievementPercent:
        raw['targetAchievementPercent'] != null
          ? num(raw['targetAchievementPercent'] ?? raw['TargetAchievementPercent'])
          : null,
      computedAtUtc: str(raw['computedAtUtc'] ?? raw['ComputedAtUtc']),
    };
  }

  private normSchool(raw: Record<string, unknown>): SchoolAnalyticsDto {
    return {
      schoolAnalyticsID: num(raw['schoolAnalyticsID'] ?? raw['SchoolAnalyticsID']),
      schoolID: num(raw['schoolID'] ?? raw['SchoolID']),
      periodKind: num(raw['periodKind'] ?? raw['PeriodKind']) as SchoolAnalyticsDto['periodKind'],
      periodStartUtc: str(raw['periodStartUtc'] ?? raw['PeriodStartUtc']),
      periodEndUtc: str(raw['periodEndUtc'] ?? raw['PeriodEndUtc']),
      kpiCount: num(raw['kpiCount'] ?? raw['KpiCount']),
      overallScore: raw['overallScore'] != null ? num(raw['overallScore'] ?? raw['OverallScore']) : null,
      targetAchievementPercent:
        raw['targetAchievementPercent'] != null
          ? num(raw['targetAchievementPercent'] ?? raw['TargetAchievementPercent'])
          : null,
      computedAtUtc: str(raw['computedAtUtc'] ?? raw['ComputedAtUtc']),
    };
  }
}
