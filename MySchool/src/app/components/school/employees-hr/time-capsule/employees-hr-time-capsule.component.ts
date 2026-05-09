import { NgFor, NgIf, NgSwitch, NgSwitchCase, NgSwitchDefault } from '@angular/common';
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
import { Subscription } from 'rxjs';

import { YearService } from 'app/core/services/year.service';
import { Year } from 'app/core/models/year.model';
import { isSchoolHrManager } from 'app/core/utils/school-role.util';
import { firstValueFrom } from 'rxjs';

import { TimeCapsuleDetailDto, TimeCapsuleStatusDto } from './time-capsule.models';
import { TimeCapsuleService } from './time-capsule.service';

@Component({
  selector: 'app-employees-hr-time-capsule',
  standalone: true,
  imports: [
    NgIf,
    NgFor,
    NgSwitch,
    NgSwitchCase,
    NgSwitchDefault,
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
  chartOpts = {
    responsive: true,
    maintainAspectRatio: false,
    plugins: { legend: { position: 'bottom' as const } },
    scales: { y: { beginAtZero: true } },
  };

  private sub?: Subscription;

  ngOnInit(): void {
    this.hrActions = isSchoolHrManager();
    this.teacherShell = this.route.snapshot.data['timeCapsuleTeacherShell'] === true;

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

  stringify(data: unknown): string {
    try {
      return JSON.stringify(data, null, 2);
    } catch {
      return '';
    }
  }

  asAchievementRows(data: unknown): string[] {
    if (!Array.isArray(data)) return [];
    return data.map((x: { title?: string; status?: string }) => {
      const t = x?.title ?? '—';
      const st = x?.status ? ` (${x.status})` : '';
      return `${t}${st}`;
    });
  }

  asViolationRows(data: unknown): string[] {
    if (!Array.isArray(data)) return [];
    return data.map((x: { title?: string; typeName?: string }) => {
      const t = x?.title ?? '—';
      const ty = x?.typeName ? ` — ${x.typeName}` : '';
      return `${t}${ty}`;
    });
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
