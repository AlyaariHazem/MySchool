import { AsyncPipe, DatePipe, NgIf } from '@angular/common';
import { Component, OnInit, inject } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { Store } from '@ngrx/store';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { ButtonModule } from 'primeng/button';
import { DatePicker } from 'primeng/datepicker';
import { DialogModule } from 'primeng/dialog';
import { FloatLabelModule } from 'primeng/floatlabel';
import { InputTextModule } from 'primeng/inputtext';
import { ProgressSpinnerModule } from 'primeng/progressspinner';
import { Select } from 'primeng/select';
import { TableModule } from 'primeng/table';
import { TextareaModule } from 'primeng/textarea';
import { TooltipModule } from 'primeng/tooltip';
import { map } from 'rxjs';
import { ToastrService } from 'ngx-toastr';
import { finalize } from 'rxjs/operators';

import { PagePermission, PermissionService } from 'app/core/services/permission.service';
import { SchoolService } from 'app/core/services/school.service';
import { YearService } from 'app/core/services/year.service';
import { Year } from 'app/core/models/year.model';
import { School } from 'app/core/models/school.modul';
import { selectLanguage } from 'app/core/store/language/language.selectors';
import { ShardModule } from 'app/shared/shard.module';

import { EmployeeProfileReadDto } from '../../employees-hr/employees-hr.models';
import { EmployeesHrService } from '../../employees-hr/employees-hr.service';
import {
  DailyEvaluationFilterDto,
  DailyEvaluationListDto,
  DailyEvaluationStatus,
  DailyEvaluationTemplateFilterDto,
  DailyEvaluationTemplateListDto,
  EvaluationLockReadDto,
  EvaluationLockStatus,
} from '../daily-evaluations.models';
import { DailyEvaluationsNavService } from '../daily-evaluations-nav.service';
import { DailyEvaluationsService, readDailyEvalHttpError } from '../daily-evaluations.service';

@Component({
  selector: 'app-daily-evaluations-list',
  standalone: true,
  imports: [
    ShardModule,
    NgIf,
    AsyncPipe,
    DatePipe,
    FormsModule,
    RouterLink,
    TranslateModule,
    TableModule,
    ButtonModule,
    Select,
    FloatLabelModule,
    ProgressSpinnerModule,
    TooltipModule,
    DialogModule,
    DatePicker,
    InputTextModule,
    TextareaModule,
  ],
  templateUrl: './daily-evaluations-list.component.html',
  styleUrl: './daily-evaluations-list.component.scss',
})
export class DailyEvaluationsListComponent implements OnInit {
  private readonly svc = inject(DailyEvaluationsService);
  private readonly employeesHr = inject(EmployeesHrService);
  private readonly schoolService = inject(SchoolService);
  private readonly yearService = inject(YearService);
  private readonly toastr = inject(ToastrService);
  private readonly translate = inject(TranslateService);
  private readonly perm = inject(PermissionService);
  private readonly store = inject(Store);
  readonly dailyEvalNav = inject(DailyEvaluationsNavService);

  /** Keeps PrimeNG select overlays from stretching full viewport width (RTL / grid). */
  readonly filterSelectPanelStyle: Record<string, string> = {
    maxWidth: 'min(22rem, calc(100vw - 2rem))',
  };

  readonly dir$ = this.store.select(selectLanguage).pipe(map((l) => (l === 'ar' ? 'rtl' : 'ltr')));

  schools: School[] = [];
  years: Year[] = [];
  employees: EmployeeProfileReadDto[] = [];
  templates: DailyEvaluationTemplateListDto[] = [];
  rows: DailyEvaluationListDto[] = [];
  loading = false;
  error: string | null = null;

  filter: DailyEvaluationFilterDto = {};

  canView = this.perm.hasPermission(PagePermission.Evaluations.View);
  canCreate = this.perm.hasPermission(PagePermission.Evaluations.Create);
  canUpdate = this.perm.hasPermission(PagePermission.Evaluations.Update);
  canLock = this.hasManagerOrEvalUpdate();

  schoolOptions: { label: string; value: number }[] = [];
  yearOptions: { label: string; value: number }[] = [];
  employeeOptions: { label: string; value: number }[] = [];
  templateOptions: { label: string; value: number }[] = [];
  statusOptions: { label: string; value: DailyEvaluationStatus }[] = [];

  DailyEvaluationStatus = DailyEvaluationStatus;
  EvaluationLockStatus = EvaluationLockStatus;

  lockDialog = false;
  reopenDialog = false;
  lockSaving = false;
  reopenSaving = false;
  lockForm: { schoolID: number | null; academicYearID: number | null; lockDate: Date | null; templateId: number | null; notes: string } = {
    schoolID: null,
    academicYearID: null,
    lockDate: null,
    templateId: null,
    notes: '',
  };
  reopenForm: { reason: string; notes: string } = { reason: '', notes: '' };
  lockPreview: EvaluationLockReadDto | null = null;
  reopenLock: EvaluationLockReadDto | null = null;

  ngOnInit(): void {
    this.statusOptions = [
      { label: this.translate.instant('dailyEvaluations.status.draft'), value: DailyEvaluationStatus.Draft },
      { label: this.translate.instant('dailyEvaluations.status.submitted'), value: DailyEvaluationStatus.Submitted },
      { label: this.translate.instant('dailyEvaluations.status.locked'), value: DailyEvaluationStatus.Locked },
    ];
    if (this.isTeacherEvaluations) {
      this.applyTeacherSchoolContextFromSession();
      const yid = this.dailyEvalNav.teacherSessionYearId();
      if (yid != null) {
        const yLabel =
          typeof localStorage !== 'undefined'
            ? (localStorage.getItem('academicYear') ?? localStorage.getItem('studyYearName') ?? '').trim()
            : '';
        this.yearOptions = [{ label: yLabel ? `${yid} — ${yLabel}` : String(yid), value: yid }];
      }
    } else {
      this.schoolService.getAllSchools().subscribe({
        next: (list) => {
          this.schools = list ?? [];
          this.schoolOptions = this.schools
            .filter((s): s is School & { schoolID: number } => s.schoolID != null && s.schoolID > 0)
            .map((s) => ({ label: s.schoolName || String(s.schoolID), value: s.schoolID }));
        },
        error: () => this.toastr.error('employeesHr.errors.loadSchools'),
      });
      this.yearService.getAllYears().subscribe({
        next: (list) => {
          this.years = list ?? [];
          this.rebuildYearOptions();
        },
        error: () => this.toastr.error('employeesHr.errors.loadYears'),
      });
    }
    this.loadTemplatesForFilter();
    if (!this.isTeacherEvaluations) {
      this.loadEmployeesForFilter();
    }
    this.load();
  }

  get isTeacherEvaluations(): boolean {
    return this.dailyEvalNav.isTeacherDailyEvaluationsRoute();
  }

  /** Teachers cannot call GET /api/School; scope filters to the school already on the session (same as rest of teacher UI). */
  private applyTeacherSchoolContextFromSession(): void {
    const opt = this.dailyEvalNav.teacherSessionSchoolOption();
    if (opt) {
      this.filter.schoolID = opt.value;
      this.schoolOptions = [opt];
    }
    const yid = this.dailyEvalNav.teacherSessionYearId();
    if (yid != null) {
      this.filter.academicYearID = yid;
    }
  }

  private hasManagerOrEvalUpdate(): boolean {
    if (typeof window === 'undefined') return false;
    const t = localStorage.getItem('userType');
    return t === 'ADMIN' || t === 'MANAGER' || this.perm.hasPermission(PagePermission.Evaluations.Update);
  }

  private loadTemplatesForFilter(): void {
    let f: DailyEvaluationTemplateFilterDto = {};
    if (this.isTeacherEvaluations) {
      if (this.filter.schoolID != null && this.filter.schoolID > 0) {
        f.schoolID = this.filter.schoolID;
      }
      if (this.filter.academicYearID != null && this.filter.academicYearID > 0) {
        f.academicYearID = this.filter.academicYearID;
      }
    }
    this.svc.getTemplates(f).subscribe({
      next: (rows) => {
        this.templates = rows ?? [];
        if (!this.isTeacherEvaluations) {
          this.templateOptions = this.templates.map((t) => ({ label: t.name, value: t.dailyEvaluationTemplateID }));
        } else {
          this.templateOptions = [];
        }
      },
    });
  }

  private loadEmployeesForFilter(): void {
    const f = this.filter.schoolID ? { schoolID: this.filter.schoolID } : {};
    this.employeesHr.getEmployees(f).subscribe({
      next: (rows) => {
        this.employees = rows ?? [];
        this.rebuildEmployeeOptions();
      },
    });
  }

  onSchoolChange(): void {
    this.rebuildYearOptions();
    if (
      this.filter.academicYearID &&
      !this.yearOptions.some((y) => y.value === this.filter.academicYearID)
    ) {
      this.filter.academicYearID = undefined;
    }
    if (!this.isTeacherEvaluations) {
      this.loadEmployeesForFilter();
    }
  }


  private rebuildYearOptions(): void {
    const sid = this.filter.schoolID;
    const filtered =
      sid != null && sid > 0 ? this.years.filter((y) => y.schoolID === sid) : [...this.years];
    this.yearOptions = filtered.map((y) => ({
      label: `${y.yearID} — ${y.yearDateStart ? new Date(y.yearDateStart).toLocaleDateString() : ''}`,
      value: y.yearID,
    }));
  }

  private rebuildEmployeeOptions(): void {
    this.employeeOptions = this.employees.map((e) => ({
      label: this.displayName(e),
      value: e.employeeProfileID,
    }));
  }

  displayName(e: EmployeeProfileReadDto): string {
    const n = e.fullName;
    if (!n) return e.employeeCode || String(e.employeeProfileID);
    return [n.firstName, n.middleName, n.lastName].filter(Boolean).join(' ');
  }

  load(): void {
    if (!this.canView) {
      this.error = 'dailyEvaluations.errors.noPermission';
      return;
    }
    this.loading = true;
    this.error = null;
    this.svc
      .getEvaluations(this.filter)
      .pipe(finalize(() => (this.loading = false)))
      .subscribe({
        next: (data) => (this.rows = data ?? []),
        error: (err) => {
          this.error = readDailyEvalHttpError(err);
          this.toastr.error(this.error);
        },
      });
  }

  templateName(id: number): string {
    return this.templates.find((t) => t.dailyEvaluationTemplateID === id)?.name ?? String(id);
  }

  employeeName(id: number): string {
    const e = this.employees.find((x) => x.employeeProfileID === id);
    return e ? this.displayName(e) : String(id);
  }

  statusLabelKey(s: DailyEvaluationStatus): string {
    const m: Record<DailyEvaluationStatus, string> = {
      [DailyEvaluationStatus.Draft]: 'draft',
      [DailyEvaluationStatus.Submitted]: 'submitted',
      [DailyEvaluationStatus.Locked]: 'locked',
    };
    return m[s] ?? String(s);
  }

  openLockDialog(): void {
    this.lockForm = {
      schoolID: this.filter.schoolID ?? null,
      academicYearID: this.filter.academicYearID ?? null,
      lockDate: new Date(),
      templateId: null,
      notes: '',
    };
    this.lockPreview = null;
    this.lockDialog = true;
    this.refreshLockPreview();
  }

  refreshLockPreview(): void {
    const sid = this.lockForm.schoolID;
    const yid = this.lockForm.academicYearID;
    const d = this.lockForm.lockDate;
    if (!sid || !yid || !d) {
      this.lockPreview = null;
      return;
    }
    const ds = d.toISOString().slice(0, 10);
    this.svc
      .getLockByDate({ schoolId: sid, academicYearId: yid, date: ds, templateId: this.lockForm.templateId })
      .subscribe({
        next: (l) => (this.lockPreview = l),
        error: () => (this.lockPreview = null),
      });
  }

  submitLock(): void {
    const sid = this.lockForm.schoolID;
    const yid = this.lockForm.academicYearID;
    const d = this.lockForm.lockDate;
    if (!sid || !yid || !d) {
      this.toastr.error(this.translate.instant('dailyEvaluations.lock.validation'));
      return;
    }
    this.lockSaving = true;
    this.svc
      .createLock({
        schoolID: sid,
        academicYearID: yid,
        lockDate: d.toISOString().slice(0, 10),
        dailyEvaluationTemplateID: this.lockForm.templateId && this.lockForm.templateId > 0 ? this.lockForm.templateId : null,
        notes: this.lockForm.notes?.trim() || null,
      })
      .pipe(finalize(() => (this.lockSaving = false)))
      .subscribe({
        next: () => {
          this.toastr.success('dailyEvaluations.toast.dayLocked');
          this.lockDialog = false;
          this.load();
        },
        error: (err) => this.toastr.error(readDailyEvalHttpError(err)),
      });
  }

  openReopenDialog(): void {
    this.reopenForm = { reason: '', notes: '' };
    this.lockForm = {
      schoolID: this.filter.schoolID ?? null,
      academicYearID: this.filter.academicYearID ?? null,
      lockDate: new Date(),
      templateId: null,
      notes: '',
    };
    this.reopenLock = null;
    this.reopenDialog = true;
    this.refreshReopenLock();
  }

  refreshReopenLock(): void {
    const sid = this.lockForm.schoolID;
    const yid = this.lockForm.academicYearID;
    const d = this.lockForm.lockDate;
    if (!sid || !yid || !d) {
      this.reopenLock = null;
      return;
    }
    const ds = d.toISOString().slice(0, 10);
    this.svc
      .getLockByDate({ schoolId: sid, academicYearId: yid, date: ds, templateId: this.lockForm.templateId })
      .subscribe({
        next: (l) => (this.reopenLock = l),
        error: () => (this.reopenLock = null),
      });
  }

  submitReopen(): void {
    if (!this.reopenForm.reason.trim()) {
      this.toastr.error(this.translate.instant('dailyEvaluations.override.reasonRequired'));
      return;
    }
    if (!this.reopenLock || this.reopenLock.status !== EvaluationLockStatus.Locked) {
      this.toastr.error(this.translate.instant('dailyEvaluations.reopen.noLock'));
      return;
    }
    this.reopenSaving = true;
    this.svc
      .reopenLock(this.reopenLock.evaluationLockID, {
        reason: this.reopenForm.reason.trim(),
        notes: this.reopenForm.notes?.trim() || null,
      })
      .pipe(finalize(() => (this.reopenSaving = false)))
      .subscribe({
        next: () => {
          this.toastr.success('dailyEvaluations.toast.lockReopened');
          this.reopenDialog = false;
          this.load();
        },
        error: (err) => this.toastr.error(readDailyEvalHttpError(err)),
      });
  }
}
