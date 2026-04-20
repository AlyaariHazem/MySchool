import { AsyncPipe, NgIf } from '@angular/common';
import { Component, OnInit, inject } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { Store } from '@ngrx/store';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { ButtonModule } from 'primeng/button';
import { DatePicker } from 'primeng/datepicker';
import { DialogModule } from 'primeng/dialog';
import { FloatLabelModule } from 'primeng/floatlabel';
import { ProgressSpinnerModule } from 'primeng/progressspinner';
import { Select } from 'primeng/select';
import { TableLazyLoadEvent, TableModule } from 'primeng/table';
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
import { isSchoolManagerUser } from 'app/core/utils/school-role.util';
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
import { DailyEvaluationsDetailComponent } from '../daily-evaluations-detail/daily-evaluations-detail.component';
import { DailyEvaluationsFormComponent } from '../daily-evaluations-form/daily-evaluations-form.component';

@Component({
  selector: 'app-daily-evaluations-list',
  standalone: true,
  imports: [
    ShardModule,
    NgIf,
    AsyncPipe,
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
    TextareaModule,
    DailyEvaluationsFormComponent,
    DailyEvaluationsDetailComponent,
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
  totalRecords = 0;
  first = 0;
  pageSize = 10;
  /** School HR: wait for years before first lazy load (filter uses active year). */
  yearLoading = false;
  loading = false;
  error: string | null = null;

  filter: DailyEvaluationFilterDto = {};

  /** UI date pickers (filter still uses ISO yyyy-mm-dd strings for API). */
  filterFromDate: Date | null = null;
  filterToDate: Date | null = null;

  canLock = this.hasManagerOrEvalUpdate();

  get isStudentEvaluations(): boolean {
    return this.dailyEvalNav.isStudentDailyEvaluationsRoute();
  }

  get isSessionPortalDailyEvaluations(): boolean {
    return this.isTeacherEvaluations || this.isStudentEvaluations;
  }

  /** School portal manager: single school from login — no school picker. */
  get isSchoolManager(): boolean {
    return isSchoolManagerUser();
  }

  get canViewEvaluations(): boolean {
    if (this.isSessionPortalDailyEvaluations) return true;
    return this.perm.hasPermission(PagePermission.Evaluations.View);
  }

  get canCreateEvaluations(): boolean {
    if (this.isSessionPortalDailyEvaluations) return true;
    return this.perm.hasPermission(PagePermission.Evaluations.Create);
  }

  get canUpdateEvaluations(): boolean {
    if (this.isSessionPortalDailyEvaluations) return true;
    return this.perm.hasPermission(PagePermission.Evaluations.Update);
  }

  schoolOptions: { label: string; value: number }[] = [];
  yearOptions: { label: string; value: number }[] = [];
  employeeOptions: { label: string; value: number }[] = [];
  /** Student portal: teacher names for evaluatedEmployeeProfileID column (HR list not available). */
  teacherEvalOptions: { label: string; value: number }[] = [];
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

  /** List dialogs: create/edit evaluation form */
  evalFormDialogVisible = false;
  /** null = create, number = edit */
  evalFormEvaluationId: number | null = null;
  evalViewDialogVisible = false;
  evalViewEvaluationId: number | null = null;

  ngOnInit(): void {
    this.statusOptions = [
      { label: this.translate.instant('dailyEvaluations.status.draft'), value: DailyEvaluationStatus.Draft },
      { label: this.translate.instant('dailyEvaluations.status.submitted'), value: DailyEvaluationStatus.Submitted },
      { label: this.translate.instant('dailyEvaluations.status.locked'), value: DailyEvaluationStatus.Locked },
    ];
    if (this.isSessionPortalDailyEvaluations) {
      this.yearLoading = false;
      this.applyTeacherSchoolContextFromSession();
      const yid = this.dailyEvalNav.teacherSessionYearId();
      if (yid != null) {
        const yLabel =
          typeof localStorage !== 'undefined'
            ? (localStorage.getItem('academicYear') ?? localStorage.getItem('studyYearName') ?? '').trim()
            : '';
        this.yearOptions = [{ label: yLabel ? `${yid} — ${yLabel}` : String(yid), value: yid }];
      }
      this.syncFilterDatePickersFromStrings();
    } else {
      if (this.isSchoolManager) {
        const sid = Number(typeof localStorage !== 'undefined' ? localStorage.getItem('schoolId') : '');
        if (Number.isFinite(sid) && sid > 0) {
          this.filter.schoolID = sid;
        }
      }
      this.yearLoading = true;
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
          this.loadTemplatesForFilter();
          this.loadEmployeesForFilter();
          if (this.isStudentEvaluations && this.filter.schoolID != null && this.filter.schoolID > 0) {
            this.svc.getTeachersForStudentEvaluation(this.filter.schoolID).subscribe({
              next: (rows) => {
                this.teacherEvalOptions = (rows ?? []).map((r) => ({
                  label: r.displayName,
                  value: r.employeeProfileID,
                }));
              },
              error: () => undefined,
            });
          }
          this.yearLoading = false;
          this.syncFilterDatePickersFromStrings();
        },
        error: () => {
          this.yearLoading = false;
          this.toastr.error('employeesHr.errors.loadYears');
        },
      });
    }
    this.loadTemplatesForFilter();
    if (this.isStudentEvaluations && this.filter.schoolID != null && this.filter.schoolID > 0) {
      this.svc.getTeachersForStudentEvaluation(this.filter.schoolID).subscribe({
        next: (rows) => {
          this.teacherEvalOptions = (rows ?? []).map((r) => ({
            label: r.displayName,
            value: r.employeeProfileID,
          }));
        },
        error: () => undefined,
      });
    }
  }

  private parseYmdToLocalDate(s?: string | null): Date | null {
    const v = (s ?? '').trim();
    if (!v) return null;
    const m = /^(\d{4})-(\d{2})-(\d{2})$/.exec(v);
    if (!m) return null;
    const y = +m[1];
    const mo = +m[2];
    const d = +m[3];
    if (!y || mo < 1 || mo > 12 || d < 1 || d > 31) return null;
    return new Date(y, mo - 1, d);
  }

  private toYmdString(d: Date | null | undefined): string | undefined {
    if (!d) return undefined;
    const y = d.getFullYear();
    const m = String(d.getMonth() + 1).padStart(2, '0');
    const day = String(d.getDate()).padStart(2, '0');
    return `${y}-${m}-${day}`;
  }

  private syncFilterDatePickersFromStrings(): void {
    this.filterFromDate = this.parseYmdToLocalDate(this.filter.fromDate);
    this.filterToDate = this.parseYmdToLocalDate(this.filter.toDate);
  }

  onFilterFromDateChange(d: Date | null): void {
    this.filterFromDate = d;
    if (d) {
      const ymd = this.toYmdString(d);
      if (ymd) this.filter.fromDate = ymd;
    } else {
      delete this.filter.fromDate;
    }
    this.normalizeFilterDateRange();
  }

  onFilterToDateChange(d: Date | null): void {
    this.filterToDate = d;
    if (d) {
      const ymd = this.toYmdString(d);
      if (ymd) this.filter.toDate = ymd;
    } else {
      delete this.filter.toDate;
    }
    this.normalizeFilterDateRange();
  }

  private normalizeFilterDateRange(): void {
    const a = this.parseYmdToLocalDate(this.filter.fromDate);
    const b = this.parseYmdToLocalDate(this.filter.toDate);
    if (a && b && a.getTime() > b.getTime()) {
      this.filterFromDate = b;
      this.filterToDate = a;
      const y1 = this.toYmdString(b);
      const y2 = this.toYmdString(a);
      if (y1) this.filter.fromDate = y1;
      if (y2) this.filter.toDate = y2;
    }
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
    if (this.isTeacherEvaluations || !this.isSessionPortalDailyEvaluations) {
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
        if (!this.isSessionPortalDailyEvaluations) {
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
    this.loadTemplatesForFilter();
    if (!this.isTeacherEvaluations) {
      this.loadEmployeesForFilter();
    }
    this.first = 0;
    this.load();
  }

  applyFilters(): void {
    this.first = 0;
    this.syncFilterDatePickersFromStrings();
    this.load();
  }

  onLazyLoad(event: TableLazyLoadEvent): void {
    this.first = event.first ?? 0;
    const r = event.rows;
    if (r != null && r > 0) {
      this.pageSize = r;
    }
    this.load();
  }

  private yearSchoolId(y: Year): number {
    const raw = y as unknown as { schoolID?: number; SchoolID?: number };
    return raw.schoolID ?? raw.SchoolID ?? 0;
  }

  private yearIsActive(y: Year): boolean {
    const raw = y as unknown as { active?: boolean; Active?: boolean };
    return !!(raw.active ?? raw.Active);
  }

  private yearIdNum(y: Year): number {
    const raw = y as unknown as { yearID?: number; YearID?: number };
    const n = raw.yearID ?? raw.YearID;
    return typeof n === 'number' && !Number.isNaN(n) ? n : 0;
  }

  /** Align with backend GetActiveYearIdForSchoolAsync. */
  private resolveActiveYearIdForSchool(schoolId: number | null | undefined): number | null {
    if (schoolId == null || schoolId <= 0) return null;
    const forSchool = this.years.filter((x) => this.yearSchoolId(x) === schoolId);
    const actives = forSchool.filter((x) => this.yearIsActive(x)).sort((a, b) => this.yearIdNum(a) - this.yearIdNum(b));
    if (actives.length) return this.yearIdNum(actives[0]);
    const latest = [...forSchool].sort((a, b) => this.yearIdNum(b) - this.yearIdNum(a));
    return latest.length ? this.yearIdNum(latest[0]) : null;
  }

  private rebuildYearOptions(): void {
    const sid = this.filter.schoolID;
    const filtered =
      sid != null && sid > 0 ? this.years.filter((y) => this.yearSchoolId(y) === sid) : [...this.years];
    this.yearOptions = filtered.map((y) => ({
      label: `${this.yearIdNum(y)} — ${y.yearDateStart ? new Date(y.yearDateStart).toLocaleDateString() : ''}`,
      value: this.yearIdNum(y),
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
    if (!this.canViewEvaluations) {
      this.error = 'dailyEvaluations.errors.noPermission';
      return;
    }
    if (this.yearLoading) {
      return;
    }
    this.loading = true;
    this.error = null;
    const pageIndex = this.pageSize > 0 ? Math.floor(this.first / this.pageSize) : 0;
    this.svc
      .getEvaluationsPage({
        pageIndex,
        pageSize: this.pageSize,
        filter: this.filter,
      })
      .pipe(finalize(() => (this.loading = false)))
      .subscribe({
        next: (page) => {
          this.rows = page.data ?? [];
          this.totalRecords = page.totalCount ?? 0;
        },
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
    const t = this.teacherEvalOptions.find((x) => x.value === id);
    if (t) return t.label;
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
    if (!this.isSessionPortalDailyEvaluations && this.lockForm.schoolID != null) {
      const yid = this.resolveActiveYearIdForSchool(this.lockForm.schoolID);
      if (yid != null) {
        this.lockForm.academicYearID = yid;
      }
    }
    this.rebuildYearOptionsForLockSchool();
    this.lockPreview = null;
    this.lockDialog = true;
    this.refreshLockPreview();
  }

  /** Lock/reopen dialogs: year dropdown options follow selected school. */
  onLockFormSchoolChange(): void {
    this.rebuildYearOptionsForLockSchool();
    if (!this.isSessionPortalDailyEvaluations && this.lockForm.schoolID != null) {
      const yid = this.resolveActiveYearIdForSchool(this.lockForm.schoolID);
      if (yid != null) {
        this.lockForm.academicYearID = yid;
      }
    }
    this.refreshLockPreview();
    this.refreshReopenLock();
  }

  private rebuildYearOptionsForLockSchool(): void {
    const sid = this.lockForm.schoolID;
    const filtered =
      sid != null && sid > 0 ? this.years.filter((y) => this.yearSchoolId(y) === sid) : [...this.years];
    this.yearOptions = filtered.map((y) => ({
      label: `${this.yearIdNum(y)} — ${y.yearDateStart ? new Date(y.yearDateStart).toLocaleDateString() : ''}`,
      value: this.yearIdNum(y),
    }));
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
    if (!this.isSessionPortalDailyEvaluations && this.lockForm.schoolID != null) {
      const yid = this.resolveActiveYearIdForSchool(this.lockForm.schoolID);
      if (yid != null) {
        this.lockForm.academicYearID = yid;
      }
    }
    this.rebuildYearOptionsForLockSchool();
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

  openEvalCreateDialog(): void {
    this.evalFormEvaluationId = null;
    this.evalFormDialogVisible = true;
  }

  openEvalEditDialog(row: DailyEvaluationListDto): void {
    this.evalFormEvaluationId = row.dailyEvaluationID;
    this.evalFormDialogVisible = true;
  }

  closeEvalFormDialog(): void {
    this.evalFormDialogVisible = false;
    this.evalFormEvaluationId = null;
  }

  openEvalViewDialog(row: DailyEvaluationListDto): void {
    this.evalViewEvaluationId = row.dailyEvaluationID;
    this.evalViewDialogVisible = true;
  }

  closeEvalViewDialog(): void {
    this.evalViewDialogVisible = false;
    this.evalViewEvaluationId = null;
  }

  onEvalDetailRequestEdit(id: number): void {
    this.evalViewDialogVisible = false;
    this.evalViewEvaluationId = null;
    this.evalFormEvaluationId = id;
    this.evalFormDialogVisible = true;
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
