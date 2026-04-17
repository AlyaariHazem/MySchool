import { DatePipe, NgFor, NgIf } from '@angular/common';
import { Component, OnInit, inject } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { ButtonModule } from 'primeng/button';
import { DialogModule } from 'primeng/dialog';
import { FloatLabelModule } from 'primeng/floatlabel';
import { InputNumberModule } from 'primeng/inputnumber';
import { InputTextModule } from 'primeng/inputtext';
import { ProgressSpinnerModule } from 'primeng/progressspinner';
import { TableModule } from 'primeng/table';
import { TextareaModule } from 'primeng/textarea';
import { TooltipModule } from 'primeng/tooltip';
import { ToastrService } from 'ngx-toastr';
import { forkJoin } from 'rxjs';
import { finalize } from 'rxjs/operators';

import { PagePermission, PermissionService } from 'app/core/services/permission.service';
import { SchoolService } from 'app/core/services/school.service';
import { YearService } from 'app/core/services/year.service';
import { School } from 'app/core/models/school.modul';
import { Year } from 'app/core/models/year.model';
import { ShardModule } from 'app/shared/shard.module';

import { EmployeeProfileReadDto } from '../../employees-hr/employees-hr.models';
import { EmployeesHrService } from '../../employees-hr/employees-hr.service';
import {
  DailyEvaluationFullDto,
  DailyEvaluationItemReadDto,
  DailyEvaluationStatus,
  EvaluationLockReadDto,
  EvaluationLockStatus,
  EvaluationOverrideActionType,
  EvaluationOverrideLogReadDto,
  EvaluationOverrideRequestDto,
} from '../daily-evaluations.models';
import { DailyEvaluationsService, readDailyEvalHttpError } from '../daily-evaluations.service';

@Component({
  selector: 'app-daily-evaluations-detail',
  standalone: true,
  imports: [
    ShardModule,
    NgIf,
    NgFor,
    DatePipe,
    FormsModule,
    RouterLink,
    TranslateModule,
    ButtonModule,
    TableModule,
    ProgressSpinnerModule,
    DialogModule,
    FloatLabelModule,
    InputNumberModule,
    InputTextModule,
    TextareaModule,
    TooltipModule,
  ],
  templateUrl: './daily-evaluations-detail.component.html',
  styleUrl: './daily-evaluations-detail.component.scss',
})
export class DailyEvaluationsDetailComponent implements OnInit {
  private readonly svc = inject(DailyEvaluationsService);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly toastr = inject(ToastrService);
  private readonly translate = inject(TranslateService);
  private readonly perm = inject(PermissionService);
  private readonly schoolService = inject(SchoolService);
  private readonly yearService = inject(YearService);
  private readonly employeesHr = inject(EmployeesHrService);

  loading = true;
  full: DailyEvaluationFullDto | null = null;
  templateName = '';
  employeeLabel = '';
  schools: School[] = [];
  years: Year[] = [];
  lockForDay: EvaluationLockReadDto | null = null;

  canView = this.perm.hasPermission(PagePermission.Evaluations.View);
  canUpdate = this.perm.hasPermission(PagePermission.Evaluations.Update);
  canOverride = this.hasOverrideAccess();

  overrideDialog = false;
  logsDialog = false;
  overrideSaving = false;
  overrideReason = '';
  overrideNotes = '';
  overrideDrafts: Record<number, { score: number; comment: string }> = {};

  logs: EvaluationOverrideLogReadDto[] = [];

  DailyEvaluationStatus = DailyEvaluationStatus;
  EvaluationOverrideActionType = EvaluationOverrideActionType;

  ngOnInit(): void {
    const id = Number(this.route.snapshot.paramMap.get('evaluationId')) || 0;
    if (!id || !this.canView) {
      this.loading = false;
      return;
    }
    this.schoolService.getAllSchools().subscribe({ next: (s) => (this.schools = s ?? []) });
    this.yearService.getAllYears().subscribe({ next: (y) => (this.years = y ?? []) });
    this.load(id);
  }

  private hasOverrideAccess(): boolean {
    if (typeof window === 'undefined') return false;
    const t = localStorage.getItem('userType');
    return t === 'ADMIN' || t === 'MANAGER' || this.perm.hasPermission(PagePermission.Evaluations.Update);
  }

  load(id: number): void {
    this.loading = true;
    this.svc
      .getEvaluationFull(id)
      .pipe(finalize(() => (this.loading = false)))
      .subscribe({
        next: (f) => {
          this.full = f;
          this.resolveLabels(f);
          this.refreshLockPreview(f);
          f.items?.forEach((it) => {
            this.overrideDrafts[it.dailyEvaluationItemID] = { score: it.score, comment: it.comment ?? '' };
          });
        },
        error: (err) => {
          this.toastr.error(readDailyEvalHttpError(err));
          this.router.navigate(['/school/daily-evaluations']).catch(() => undefined);
        },
      });
  }

  private resolveLabels(f: DailyEvaluationFullDto): void {
    this.svc.getTemplateById(f.dailyEvaluationTemplateID).subscribe({
      next: (t) => (this.templateName = t.name),
      error: () => (this.templateName = String(f.dailyEvaluationTemplateID)),
    });
    this.employeesHr.getEmployeeById(f.evaluatedEmployeeProfileID).subscribe({
      next: (e) => (this.employeeLabel = this.displayName(e)),
      error: () => (this.employeeLabel = String(f.evaluatedEmployeeProfileID)),
    });
  }

  private displayName(e: EmployeeProfileReadDto): string {
    const n = e.fullName;
    if (!n) return e.employeeCode || String(e.employeeProfileID);
    return [n.firstName, n.middleName, n.lastName].filter(Boolean).join(' ');
  }

  private refreshLockPreview(f: DailyEvaluationFullDto): void {
    forkJoin({
      scoped: this.svc.getLockByDate({
        schoolId: f.schoolID,
        academicYearId: f.academicYearID,
        date: f.evaluationDate,
        templateId: f.dailyEvaluationTemplateID,
      }),
      whole: this.svc.getLockByDate({
        schoolId: f.schoolID,
        academicYearId: f.academicYearID,
        date: f.evaluationDate,
        templateId: null,
      }),
    }).subscribe({
      next: ({ scoped, whole }) => {
        const pick = (x: EvaluationLockReadDto | null) =>
          x && x.status === EvaluationLockStatus.Locked ? x : null;
        this.lockForDay = pick(scoped) ?? pick(whole);
      },
    });
  }

  schoolName(id: number): string {
    return this.schools.find((s) => s.schoolID === id)?.schoolName ?? String(id);
  }

  statusLabelKey(s: DailyEvaluationStatus): string {
    const m: Record<DailyEvaluationStatus, string> = {
      [DailyEvaluationStatus.Draft]: 'draft',
      [DailyEvaluationStatus.Submitted]: 'submitted',
      [DailyEvaluationStatus.Locked]: 'locked',
    };
    return m[s] ?? String(s);
  }

  openOverride(): void {
    if (!this.full) return;
    this.overrideReason = '';
    this.overrideNotes = '';
    this.overrideDialog = true;
  }

  submitOverride(): void {
    if (!this.full || !this.overrideReason.trim()) {
      this.toastr.error(this.translate.instant('dailyEvaluations.override.reasonRequired'));
      return;
    }
    const f = this.full;
    const items: EvaluationOverrideRequestDto['items'] = (f.items ?? []).map((it) => ({
      dailyEvaluationItemID: it.dailyEvaluationItemID,
      score: this.overrideDrafts[it.dailyEvaluationItemID]?.score ?? it.score,
      comment: this.overrideDrafts[it.dailyEvaluationItemID]?.comment ?? it.comment ?? null,
    }));
    const payload: EvaluationOverrideRequestDto = {
      reason: this.overrideReason.trim(),
      evaluation: { notes: f.notes?.trim() || null },
      items,
      notes: this.overrideNotes?.trim() || null,
    };
    this.overrideSaving = true;
    this.svc
      .overrideUpdateAfterLock(f.dailyEvaluationID, payload)
      .pipe(finalize(() => (this.overrideSaving = false)))
      .subscribe({
        next: () => {
          this.toastr.success('dailyEvaluations.toast.overrideSaved');
          this.overrideDialog = false;
          this.load(f.dailyEvaluationID);
        },
        error: (err) => this.toastr.error(readDailyEvalHttpError(err)),
      });
  }

  openLogs(): void {
    if (!this.full) return;
    this.svc.getOverrideLogs(this.full.dailyEvaluationID).subscribe({
      next: (rows) => {
        this.logs = rows ?? [];
        this.logsDialog = true;
      },
      error: (err) => this.toastr.error(readDailyEvalHttpError(err)),
    });
  }

  actionLabel(a: EvaluationOverrideActionType): string {
    const m: Record<EvaluationOverrideActionType, string> = {
      [EvaluationOverrideActionType.EditAfterLock]: 'editAfterLock',
      [EvaluationOverrideActionType.ReopenEvaluation]: 'reopenEvaluation',
      [EvaluationOverrideActionType.UnlockDay]: 'unlockDay',
      [EvaluationOverrideActionType.ForceUpdate]: 'forceUpdate',
      [EvaluationOverrideActionType.DeleteAfterLock]: 'deleteAfterLock',
    };
    return m[a] ?? String(a);
  }

  oRow(it: DailyEvaluationItemReadDto): { score: number; comment: string } {
    if (!this.overrideDrafts[it.dailyEvaluationItemID]) {
      this.overrideDrafts[it.dailyEvaluationItemID] = { score: it.score, comment: it.comment ?? '' };
    }
    return this.overrideDrafts[it.dailyEvaluationItemID];
  }
}
