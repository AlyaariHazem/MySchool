import {
  DatePipe,
  DecimalPipe,
  NgFor,
  NgIf,
  NgSwitch,
  NgSwitchCase,
  NgSwitchDefault,
} from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';
import { Component, OnDestroy, OnInit, inject } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { TranslateModule } from '@ngx-translate/core';
import { ButtonModule } from 'primeng/button';
import { CardModule } from 'primeng/card';
import { ChartModule } from 'primeng/chart';
import { FloatLabelModule } from 'primeng/floatlabel';
import { Select } from 'primeng/select';
import { TextareaModule } from 'primeng/textarea';
import { ToastrService } from 'ngx-toastr';
import { Store } from '@ngrx/store';
import { firstValueFrom, map, Subscription } from 'rxjs';

import { YearService } from 'app/core/services/year.service';
import { Year } from 'app/core/models/year.model';
import { isSchoolHrManager } from 'app/core/utils/school-role.util';
import { selectLanguage } from 'app/core/store/language/language.selectors';

import { TimeCapsuleDetailDto, TimeCapsuleStatusDto } from './time-capsule.models';
import { TimeCapsuleService } from './time-capsule.service';

function tcStr(v: unknown): string {
  if (v == null) return '';
  return String(v);
}

function tcNum(v: unknown): number | null {
  if (v == null || v === '') return null;
  const n = Number(v);
  return Number.isFinite(n) ? n : null;
}

function asObj(v: unknown): Record<string, unknown> | null {
  return v && typeof v === 'object' ? (v as Record<string, unknown>) : null;
}

interface TcHistoryRowVM {
  academicYearId?: number;
  jobTitle: string;
  department: string;
  startDate: string | null;
  endDate: string | null;
  notes: string;
}

interface TcGeneralInfoVM {
  employeeCode: string;
  displayName: string;
  jobType: string;
  hireDate: string | null;
  employmentStatus: string;
  email: string;
  phone: string;
  history: TcHistoryRowVM[];
}

interface TcPerfYearVM {
  academicYearId: number;
  yearLabel: string;
  evaluationScore: number | null;
  performanceLevel: string | null;
  achievementPoints: number;
  violationPoints: number;
  dailyEvaluationCount: number;
  dailyEvaluationAverage: number | null;
  strengthsSummary: string | null;
  weaknessesSummary: string | null;
}

interface TcAnalyticsVM {
  academicYearId: number | null;
  periodKind: string | null;
  periodStartUtc: string | null;
  periodEndUtc: string | null;
  compositeScore: number | null;
  averageDailyEvaluationScore: number | null;
  achievementPoints: number | null;
  violationPoints: number | null;
  activityCount: number | null;
  trend: string | null;
}

interface TcEvalYearVM {
  academicYearId: number;
  count: number;
  average: number | null;
}

interface TcEvalSummaryVM {
  totalEvaluations: number;
  averageScore: number | null;
  byYear: TcEvalYearVM[];
}

interface TcAchievementVM {
  title: string;
  status: string;
  academicYearId: number | null;
  pointsHint: number | null;
  submittedAtUtc: string | null;
}

interface TcViolationVM {
  title: string;
  status: string;
  typeName: string;
  academicYearId: number | null;
  openedAtUtc: string | null;
}

interface TcActivityVM {
  title: string;
  status: string;
  academicYearId: number | null;
  submittedAtUtc: string | null;
}

interface TcReportVM {
  documentType: string;
  title: string;
  uploadedAtUtc: string | null;
}

@Component({
  selector: 'app-employees-hr-time-capsule',
  standalone: true,
  imports: [
    NgIf,
    NgFor,
    NgSwitch,
    NgSwitchCase,
    NgSwitchDefault,
    DatePipe,
    DecimalPipe,
    FormsModule,
    TranslateModule,
    RouterLink,
    ButtonModule,
    CardModule,
    ChartModule,
    Select,
    FloatLabelModule,
    TextareaModule,
  ],
  templateUrl: './employees-hr-time-capsule.component.html',
  styleUrl: './employees-hr-time-capsule.component.scss',
})
export class EmployeesHrTimeCapsuleComponent implements OnInit, OnDestroy {
  private readonly route = inject(ActivatedRoute);
  private readonly capsuleApi = inject(TimeCapsuleService);
  private readonly yearsApi = inject(YearService);
  private readonly toastr = inject(ToastrService);
  private readonly store = inject(Store);

  /** True when opened from `/teacher/time-capsule/:id` (teacher shell). */
  teacherShell = false;
  /** Back navigation: workspace for teachers, full HR profile for managers. */
  backLink: (string | number)[] = ['/school/employees-hr'];

  id = 0;
  loading = true;
  status: TimeCapsuleStatusDto | null = null;
  detail: TimeCapsuleDetailDto | null = null;
  loadError = '';

  years: Year[] = [];
  resignYearId: number | null = null;
  resignReason = '';
  submittingResign = false;

  hrActions = false;

  perfChart: { labels: string[]; datasets: { label: string; data: number[]; borderColor?: string; backgroundColor?: string }[] } | null =
    null;

  /** Text direction for Arabic (`rtl`) vs English (`ltr`). */
  uiDir: 'ltr' | 'rtl' = 'rtl';

  /** Back button: arrow points toward the logical “start” side. */
  get backNavIcon(): string {
    return this.uiDir === 'rtl' ? 'pi pi-arrow-right' : 'pi pi-arrow-left';
  }

  /** Chart.js options respect RTL (mirroring + Y-axis on the right in Arabic). */
  get chartOptions(): Record<string, unknown> {
    const rtl = this.uiDir === 'rtl';
    return {
      responsive: true,
      maintainAspectRatio: false,
      rtl,
      plugins: {
        legend: {
          position: 'bottom',
          rtl,
          labels: { color: '#475569' },
        },
      },
      scales: {
        x: {
          ticks: { color: '#64748b' },
          grid: { color: 'rgba(148, 163, 184, 0.25)' },
        },
        y: {
          position: rtl ? 'right' : 'left',
          beginAtZero: true,
          ticks: { color: '#64748b' },
          grid: { color: 'rgba(148, 163, 184, 0.25)' },
        },
      },
    };
  }

  private sub?: Subscription;
  private langSub?: Subscription;

  ngOnInit(): void {
    this.hrActions = isSchoolHrManager();
    this.teacherShell = this.route.snapshot.data['timeCapsuleTeacherShell'] === true;

    this.langSub = this.store
      .select(selectLanguage)
      .pipe(map((lang) => (lang === 'en' ? 'ltr' : 'rtl')))
      .subscribe((dir) => {
        this.uiDir = dir;
      });

    this.sub = this.route.paramMap.subscribe((pm) => {
      const raw = pm.get('id');
      this.id = raw ? +raw : 0;
      this.backLink = this.teacherShell
        ? ['/teacher/workspace']
        : ['/school/employees-hr'];
      if (this.id > 0) void this.bootstrap();
    });
  }

  ngOnDestroy(): void {
    this.sub?.unsubscribe();
    this.langSub?.unsubscribe();
  }

  private async bootstrap(): Promise<void> {
    this.loading = true;
    this.loadError = '';
    this.status = null;
    this.detail = null;
    this.perfChart = null;
    try {
      this.status = await firstValueFrom(this.capsuleApi.getStatus(this.id));
      if (this.status?.isUnlocked) {
        this.detail = await firstValueFrom(this.capsuleApi.getCapsule(this.id));
        this.buildChartFromDetail();
      }
      if (this.status?.phase === 'LockedNoResignation' || this.status?.phase === 'ResignationRejected') {
        this.years = (await firstValueFrom(this.yearsApi.getAllYears())) ?? [];
        const cur = this.years[0];
        if (cur) this.resignYearId = cur.yearID;
      }
    } catch (e) {
      this.loadError = this.readErr(e);
    } finally {
      this.loading = false;
    }
  }

  private readErr(e: unknown): string {
    const err = e as HttpErrorResponse;
    const msgs = err?.error?.errorMasseges ?? err?.error?.ErrorMasseges;
    if (Array.isArray(msgs) && msgs.length) return msgs.join('; ');
    if (e instanceof Error) return e.message;
    return 'Failed to load';
  }

  private buildChartFromDetail(): void {
    const sec = this.detail?.sections?.find((s) => s.sectionType === 2);
    if (!sec?.dataJson) return;
    try {
      const data = JSON.parse(sec.dataJson) as { years?: { yearLabel?: string; dailyEvaluationAverage?: number | null }[] };
      const years = data.years ?? [];
      this.perfChart = {
        labels: years.map((y) => y.yearLabel ?? '—'),
        datasets: [
          {
            label: 'Avg. daily evaluation',
            data: years.map((y) => Number(y.dailyEvaluationAverage ?? 0)),
            borderColor: 'rgba(199, 162, 74, 1)',
            backgroundColor: 'rgba(199, 162, 74, 0.25)',
          },
        ],
      };
    } catch {
      this.perfChart = null;
    }
  }

  parseJson(raw: string | undefined | null): unknown {
    if (!raw) return null;
    try {
      return JSON.parse(raw);
    } catch {
      return null;
    }
  }

  generalInfoVM(data: unknown): TcGeneralInfoVM {
    const o = asObj(data);
    const history: TcHistoryRowVM[] = [];
    const histRaw = o?.['history'];
    if (Array.isArray(histRaw)) {
      for (const h of histRaw) {
        const r = asObj(h);
        if (!r) continue;
        history.push({
          academicYearId: tcNum(r['academicYearID'] ?? r['academicYearId']) ?? undefined,
          jobTitle: tcStr(r['jobTitle']),
          department: tcStr(r['department']),
          startDate: r['startDate'] != null ? tcStr(r['startDate']) : null,
          endDate: r['endDate'] != null ? tcStr(r['endDate']) : null,
          notes: tcStr(r['notes']),
        });
      }
    }
    return {
      employeeCode: tcStr(o?.['employeeCode']),
      displayName: tcStr(o?.['displayName']),
      jobType: tcStr(o?.['jobType']),
      hireDate: o?.['hireDate'] != null ? tcStr(o['hireDate']) : null,
      employmentStatus: tcStr(o?.['employmentStatus']),
      email: tcStr(o?.['email']),
      phone: tcStr(o?.['phone']),
      history,
    };
  }

  fullNameLine(data: unknown): string {
    const o = asObj(data);
    const fn = asObj(o?.['fullName']);
    if (!fn) return '';
    const parts = [tcStr(fn['firstName']), tcStr(fn['middleName']), tcStr(fn['lastName'])].filter(Boolean);
    return parts.join(' ');
  }

  performanceYearsVM(data: unknown): TcPerfYearVM[] {
    const o = asObj(data);
    const years = o?.['years'];
    if (!Array.isArray(years)) return [];
    const out: TcPerfYearVM[] = [];
    for (const y of years) {
      const r = asObj(y);
      if (!r) continue;
      out.push({
        academicYearId: tcNum(r['academicYearId'] ?? r['academicYearID']) ?? 0,
        yearLabel: tcStr(r['yearLabel']) || '—',
        evaluationScore: tcNum(r['evaluationScore']),
        performanceLevel: r['performanceLevel'] != null ? tcStr(r['performanceLevel']) : null,
        achievementPoints: tcNum(r['achievementPoints']) ?? 0,
        violationPoints: tcNum(r['violationPoints']) ?? 0,
        dailyEvaluationCount: tcNum(r['dailyEvaluationCount']) ?? 0,
        dailyEvaluationAverage: tcNum(r['dailyEvaluationAverage']),
        strengthsSummary: r['strengthsSummary'] != null ? tcStr(r['strengthsSummary']) : null,
        weaknessesSummary: r['weaknessesSummary'] != null ? tcStr(r['weaknessesSummary']) : null,
      });
    }
    return out;
  }

  performanceAnalyticsVM(data: unknown): TcAnalyticsVM[] {
    const o = asObj(data);
    const a = o?.['analytics'];
    return this.mapAnalyticsRows(a);
  }

  private mapAnalyticsRows(raw: unknown): TcAnalyticsVM[] {
    if (!Array.isArray(raw)) return [];
    const out: TcAnalyticsVM[] = [];
    for (const row of raw) {
      const r = asObj(row);
      if (!r) continue;
      out.push({
        academicYearId: tcNum(r['academicYearID'] ?? r['academicYearId']),
        periodKind: r['periodKind'] != null ? tcStr(r['periodKind']) : null,
        periodStartUtc: r['periodStartUtc'] != null ? tcStr(r['periodStartUtc']) : null,
        periodEndUtc: r['periodEndUtc'] != null ? tcStr(r['periodEndUtc']) : null,
        compositeScore: tcNum(r['compositeScore']),
        averageDailyEvaluationScore: tcNum(r['averageDailyEvaluationScore']),
        achievementPoints: tcNum(r['achievementPoints']),
        violationPoints: tcNum(r['violationPoints']),
        activityCount: tcNum(r['activityCount']),
        trend: r['trend'] != null ? tcStr(r['trend']) : null,
      });
    }
    return out;
  }

  evalSummaryVM(data: unknown): TcEvalSummaryVM {
    const o = asObj(data);
    const byYear: TcEvalYearVM[] = [];
    const by = o?.['byYear'];
    if (Array.isArray(by)) {
      for (const row of by) {
        const r = asObj(row);
        if (!r) continue;
        byYear.push({
          academicYearId: tcNum(r['academicYearId'] ?? r['academicYearID']) ?? 0,
          count: tcNum(r['count']) ?? 0,
          average: tcNum(r['average']),
        });
      }
    }
    return {
      totalEvaluations: tcNum(o?.['totalEvaluations']) ?? 0,
      averageScore: tcNum(o?.['averageScore']),
      byYear,
    };
  }

  achievementsList(data: unknown): TcAchievementVM[] {
    if (!Array.isArray(data)) return [];
    return data.map((x) => {
      const r = asObj(x) ?? {};
      return {
        title: tcStr(r['title']) || '—',
        status: tcStr(r['status']),
        academicYearId: tcNum(r['academicYearID'] ?? r['academicYearId']),
        pointsHint: tcNum(r['pointsHint']),
        submittedAtUtc: r['submittedAtUtc'] != null ? tcStr(r['submittedAtUtc']) : null,
      };
    });
  }

  violationsList(data: unknown): TcViolationVM[] {
    if (!Array.isArray(data)) return [];
    return data.map((x) => {
      const r = asObj(x) ?? {};
      return {
        title: tcStr(r['title']) || '—',
        status: tcStr(r['status']),
        typeName: tcStr(r['typeName']),
        academicYearId: tcNum(r['academicYearID'] ?? r['academicYearId']),
        openedAtUtc: r['openedAtUtc'] != null ? tcStr(r['openedAtUtc']) : null,
      };
    });
  }

  activitiesList(data: unknown): TcActivityVM[] {
    if (!Array.isArray(data)) return [];
    return data.map((x) => {
      const r = asObj(x) ?? {};
      return {
        title: tcStr(r['Title'] ?? r['title']) || '—',
        status: tcStr(r['status']),
        academicYearId: tcNum(r['academicYearID'] ?? r['academicYearId']),
        submittedAtUtc: r['submittedAtUtc'] != null ? tcStr(r['submittedAtUtc']) : null,
      };
    });
  }

  reportsList(data: unknown): TcReportVM[] {
    if (!Array.isArray(data)) return [];
    return data.map((x) => {
      const r = asObj(x) ?? {};
      return {
        documentType: tcStr(r['documentType']),
        title: tcStr(r['title']) || '—',
        uploadedAtUtc: r['uploadedAtUtc'] != null ? tcStr(r['uploadedAtUtc']) : null,
      };
    });
  }

  finalSummaryVM(data: unknown): { totals: Record<string, number>; rollup: TcAnalyticsVM[] } {
    const o = asObj(data);
    const totals: Record<string, number> = {};
    const tr = asObj(o?.['totals']);
    if (tr) {
      for (const key of [
        'yearsCovered',
        'violationCount',
        'achievementRequests',
        'activityRequests',
        'approvedAchievements',
      ]) {
        const n = tcNum(tr[key]);
        if (n != null) totals[key] = n;
      }
    }
    const rollupRaw = o?.['analyticsRollup'];
    return { totals, rollup: this.mapAnalyticsRows(rollupRaw) };
  }

  async submitResignation(): Promise<void> {
    if (!this.resignYearId) {
      this.toastr.warning('اختر العام الدراسي');
      return;
    }
    this.submittingResign = true;
    try {
      await firstValueFrom(
        this.capsuleApi.requestResignation({
          employeeProfileId: this.id,
          academicYearId: this.resignYearId,
          reason: this.resignReason.trim() || null,
        }),
      );
      this.toastr.success('تم إرسال طلب الاستقالة');
      await this.bootstrap();
    } catch (e) {
      this.toastr.error(this.readErr(e));
    } finally {
      this.submittingResign = false;
    }
  }

  async hrApproveResignation(): Promise<void> {
    const rid = this.status?.resignationRequestId;
    if (!rid) return;
    try {
      await firstValueFrom(this.capsuleApi.approveResignation(rid));
      this.toastr.success('تمت الموافقة على الاستقالة');
      await this.bootstrap();
    } catch (e) {
      this.toastr.error(this.readErr(e));
    }
  }

  async hrRejectResignation(): Promise<void> {
    const rid = this.status?.resignationRequestId;
    if (!rid) return;
    try {
      await firstValueFrom(this.capsuleApi.rejectResignation(rid));
      this.toastr.success('تم رفض طلب الاستقالة');
      await this.bootstrap();
    } catch (e) {
      this.toastr.error(this.readErr(e));
    }
  }

  async hrApproveUnlock(): Promise<void> {
    const cid = this.status?.timeCapsuleId;
    if (!cid) return;
    try {
      await firstValueFrom(this.capsuleApi.approveUnlock(cid, 'موافقة إدارية على فتح الكبسولة'));
      this.toastr.success('تم فتح الكبسولة وبناء الأرشيف');
      await this.bootstrap();
    } catch (e) {
      this.toastr.error(this.readErr(e));
    }
  }

  async hrRejectUnlock(): Promise<void> {
    const cid = this.status?.timeCapsuleId;
    if (!cid) return;
    try {
      await firstValueFrom(this.capsuleApi.rejectUnlock(cid));
      this.toastr.success('تم رفض فتح الكبسولة');
      await this.bootstrap();
    } catch (e) {
      this.toastr.error(this.readErr(e));
    }
  }

  yearOptions(): { label: string; value: number }[] {
    return this.years.map((y) => ({
      label: `${y.yearDateStart ? new Date(y.yearDateStart).getFullYear() : y.yearID}`,
      value: y.yearID,
    }));
  }
}
