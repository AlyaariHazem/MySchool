import { AsyncPipe, NgIf } from '@angular/common';
import { Component, inject, OnDestroy, OnInit } from '@angular/core';
import { Subscription } from 'rxjs';
import { FormsModule } from '@angular/forms';
import { Store } from '@ngrx/store';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { ConfirmationService } from 'primeng/api';
import { ButtonModule } from 'primeng/button';
import { ConfirmDialogModule } from 'primeng/confirmdialog';
import { DialogModule } from 'primeng/dialog';
import { FloatLabelModule } from 'primeng/floatlabel';
import { ProgressSpinnerModule } from 'primeng/progressspinner';
import { Select } from 'primeng/select';
import { TableModule } from 'primeng/table';
import { TooltipModule } from 'primeng/tooltip';
import { map } from 'rxjs';
import { ToastrService } from 'ngx-toastr';
import { finalize } from 'rxjs/operators';

import { isSchoolManagerUser } from 'app/core/utils/school-role.util';
import { PagePermission, PermissionService } from 'app/core/services/permission.service';
import { SchoolService } from 'app/core/services/school.service';
import { YearService } from 'app/core/services/year.service';
import { Year } from 'app/core/models/year.model';
import { School } from 'app/core/models/school.modul';
import { selectLanguage } from 'app/core/store/language/language.selectors';
import { ShardModule } from 'app/shared/shard.module';

import {
  EmployeeProfileCreateDto,
  EmployeeJobTypeDto,
  EmployeeProfileListFilterDto,
  EmployeeProfileReadDto,
  EmploymentStatus,
} from '../employees-hr.models';
import { EmployeesHrDetailComponent } from '../employees-hr-detail/employees-hr-detail.component';
import { EmployeesHrFullProfileComponent } from '../employees-hr-full-profile/employees-hr-full-profile.component';
import { EmployeesHrProfileFormComponent } from '../employees-hr-profile-form/employees-hr-profile-form.component';
import { EmployeesHrService, readHttpError } from '../employees-hr.service';

@Component({
  selector: 'app-employees-hr-list',
  standalone: true,
  imports: [
    ShardModule,
    NgIf,
    AsyncPipe,
    FormsModule,
    TranslateModule,
    TableModule,
    ButtonModule,
    Select,
    FloatLabelModule,
    ProgressSpinnerModule,
    TooltipModule,
    ConfirmDialogModule,
    DialogModule,
    EmployeesHrProfileFormComponent,
    EmployeesHrDetailComponent,
    EmployeesHrFullProfileComponent,
  ],
  providers: [ConfirmationService],
  templateUrl: './employees-hr-list.component.html',
  styleUrl: './employees-hr-list.component.scss',
})
export class EmployeesHrListComponent implements OnInit, OnDestroy {
  private readonly employeesHr = inject(EmployeesHrService);
  private jobTypesRows: EmployeeJobTypeDto[] = [];
  private langSub?: Subscription;
  private readonly schoolService = inject(SchoolService);
  private readonly yearService = inject(YearService);
  private readonly toastr = inject(ToastrService);
  private readonly confirm = inject(ConfirmationService);
  private readonly translate = inject(TranslateService);
  private readonly perm = inject(PermissionService);
  private readonly store = inject(Store);

  readonly dir$ = this.store.select(selectLanguage).pipe(map((l) => (l === 'ar' ? 'rtl' : 'ltr')));

  /** Keeps PrimeNG select overlays from stretching full viewport width (RTL / grid). */
  readonly filterSelectPanelStyle: Record<string, string> = {
    maxWidth: 'min(22rem, calc(100vw - 2rem))',
  };

  employmentOptions: { label: string; value: EmploymentStatus }[] = [];

  schools: School[] = [];
  years: Year[] = [];
  rows: EmployeeProfileReadDto[] = [];
  loading = false;
  error: string | null = null;

  filter: EmployeeProfileListFilterDto = {};

  canCreate = this.perm.hasPermission(PagePermission.Employees.Create);
  canUpdate = this.perm.hasPermission(PagePermission.Employees.Update);
  canDelete = this.perm.hasPermission(PagePermission.Employees.Delete);
  canView = this.perm.hasPermission(PagePermission.Employees.View);

  get isSchoolManager(): boolean {
    return isSchoolManagerUser();
  }

  schoolOptions: { label: string; value: number }[] = [];
  jobTypeOptions: { label: string; value: number }[] = [];
  jobTypesLoading = false;
  activeOptions: { label: string; value: boolean | null }[] = [];

  createDialogVisible = false;
  editDialogVisible = false;
  viewDialogVisible = false;
  fullProfileDialogVisible = false;
  dialogSubmitting = false;
  editLoading = false;
  editProfile: EmployeeProfileReadDto | null = null;
  selectedEmployeeId: number | null = null;

  ngOnInit(): void {
    this.activeOptions = [
      { label: this.translate.instant('employeesHr.filter.allActive'), value: null },
      { label: this.translate.instant('employeesHr.filter.activeOnly'), value: true },
      { label: this.translate.instant('employeesHr.filter.inactiveOnly'), value: false },
    ];
    this.employmentOptions = [
      { label: this.translate.instant('employeesHr.status.active'), value: EmploymentStatus.Active },
      { label: this.translate.instant('employeesHr.status.onLeave'), value: EmploymentStatus.OnLeave },
      { label: this.translate.instant('employeesHr.status.suspended'), value: EmploymentStatus.Suspended },
      { label: this.translate.instant('employeesHr.status.terminated'), value: EmploymentStatus.Terminated },
    ];
    this.loadJobTypes();
    this.langSub = this.translate.onLangChange.subscribe(() => this.rebuildJobTypeFilterLabels());
    if (this.isSchoolManager) {
      const sid = Number(typeof localStorage !== 'undefined' ? localStorage.getItem('schoolId') : '');
      if (Number.isFinite(sid) && sid > 0) {
        this.filter.schoolID = sid;
      }
    }
    this.schoolService.getAllSchools().subscribe({
      next: (list) => {
        this.schools = list ?? [];
        this.schoolOptions = this.schools
          .filter((s): s is School & { schoolID: number } => s.schoolID != null && s.schoolID > 0)
          .map((s) => ({
            label: s.schoolName || String(s.schoolID),
            value: s.schoolID,
          }));
      },
      error: () => this.toastr.error('employeesHr.errors.loadSchools'),
    });
    this.yearService.getAllYears().subscribe({
      next: (list) => {
        this.years = list ?? [];
      },
      error: () => this.toastr.error('employeesHr.errors.loadYears'),
    });
    this.load();
  }

  ngOnDestroy(): void {
    this.langSub?.unsubscribe();
  }

  private loadJobTypes(): void {
    this.jobTypesLoading = true;
    this.employeesHr
      .getEmployeeJobTypes()
      .pipe(finalize(() => (this.jobTypesLoading = false)))
      .subscribe({
        next: (rows) => {
          this.jobTypesRows = rows ?? [];
          this.rebuildJobTypeFilterLabels();
        },
      });
  }

  private rebuildJobTypeFilterLabels(): void {
    this.jobTypeOptions = this.jobTypesRows.map((j) => ({
      label: this.jobTypeOptionLabel(j),
      value: j.employeeJobTypeID,
    }));
  }

  private jobTypeOptionLabel(j: EmployeeJobTypeDto): string {
    const lang = this.translate.currentLang || '';
    const primary = lang.startsWith('ar') && j.nameAr?.trim() ? j.nameAr.trim() : j.name;
    const inactive = !j.isActive
      ? ` (${this.translate.instant('employeesHr.form.jobTypeInactiveSuffix')})`
      : '';
    return `${primary} (${j.code})${inactive}`;
  }

  load(): void {
    if (!this.canView) {
      this.error = 'employeesHr.errors.noPermission';
      return;
    }
    this.loading = true;
    this.error = null;
    this.employeesHr
      .getEmployees(this.filter)
      .pipe(finalize(() => (this.loading = false)))
      .subscribe({
        next: (data) => (this.rows = data ?? []),
        error: (err) => {
          this.error = readHttpError(err);
          this.toastr.error(this.error);
        },
      });
  }

  openCreateDialog(): void {
    if (!this.canCreate) return;
    this.createDialogVisible = true;
  }

  closeCreateDialog(): void {
    this.createDialogVisible = false;
    this.dialogSubmitting = false;
  }

  submitCreate(dto: EmployeeProfileCreateDto): void {
    this.dialogSubmitting = true;
    this.employeesHr
      .createEmployee(dto)
      .pipe(finalize(() => (this.dialogSubmitting = false)))
      .subscribe({
        next: () => {
          this.toastr.success('employeesHr.toast.created');
          this.createDialogVisible = false;
          this.load();
        },
        error: (err) => this.toastr.error(readHttpError(err)),
      });
  }

  openEditDialog(row: EmployeeProfileReadDto): void {
    if (!this.canUpdate || !row.employeeProfileID) return;
    this.editProfile = null;
    this.editDialogVisible = true;
    this.editLoading = true;
    this.employeesHr
      .getEmployeeById(row.employeeProfileID)
      .pipe(finalize(() => (this.editLoading = false)))
      .subscribe({
        next: (profile) => (this.editProfile = profile),
        error: (err) => {
          this.toastr.error(readHttpError(err));
          this.editDialogVisible = false;
        },
      });
  }

  closeEditDialog(): void {
    this.editDialogVisible = false;
    this.dialogSubmitting = false;
    this.editLoading = false;
    this.editProfile = null;
  }

  submitEdit(dto: EmployeeProfileCreateDto): void {
    const id = this.editProfile?.employeeProfileID;
    if (!id) return;
    this.dialogSubmitting = true;
    this.employeesHr
      .updateEmployee(id, dto)
      .pipe(finalize(() => (this.dialogSubmitting = false)))
      .subscribe({
        next: () => {
          this.toastr.success('employeesHr.toast.updated');
          this.editDialogVisible = false;
          this.editProfile = null;
          this.load();
        },
        error: (err) => this.toastr.error(readHttpError(err)),
      });
  }

  openViewDialog(row: EmployeeProfileReadDto): void {
    if (!row.employeeProfileID) return;
    this.selectedEmployeeId = row.employeeProfileID;
    this.viewDialogVisible = true;
  }

  closeViewDialog(): void {
    this.viewDialogVisible = false;
    this.selectedEmployeeId = null;
  }

  openFullProfileDialog(row: EmployeeProfileReadDto): void {
    if (!row.employeeProfileID) return;
    this.selectedEmployeeId = row.employeeProfileID;
    this.fullProfileDialogVisible = true;
  }

  closeFullProfileDialog(): void {
    this.fullProfileDialogVisible = false;
    this.selectedEmployeeId = null;
  }

  displayName(row: EmployeeProfileReadDto): string {
    const n = row.fullName;
    if (!n) return '';
    return [n.firstName, n.middleName, n.lastName].filter(Boolean).join(' ');
  }

  schoolName(id: number): string {
    return this.schools.find((s) => s.schoolID === id)?.schoolName ?? String(id);
  }

  yearLabel(id: number): string {
    const y = this.years.find((x) => x.yearID === id);
    if (!y) return String(id);
    return `${y.yearID}`;
  }

  jobTypeLabelForRow(row: EmployeeProfileReadDto): string {
    if (row.jobTypeName || row.jobTypeCode) {
      return [row.jobTypeName, row.jobTypeCode ? `(${row.jobTypeCode})` : ''].filter(Boolean).join(' ').trim();
    }
    const j = this.jobTypesRows.find((x) => x.employeeJobTypeID === row.employeeJobTypeID);
    return j ? this.jobTypeOptionLabel(j) : String(row.employeeJobTypeID);
  }

  employmentLabel(v: EmploymentStatus): string {
    const m: Record<EmploymentStatus, string> = {
      [EmploymentStatus.Active]: this.translate.instant('employeesHr.status.active'),
      [EmploymentStatus.OnLeave]: this.translate.instant('employeesHr.status.onLeave'),
      [EmploymentStatus.Suspended]: this.translate.instant('employeesHr.status.suspended'),
      [EmploymentStatus.Terminated]: this.translate.instant('employeesHr.status.terminated'),
    };
    return m[v] ?? String(v);
  }

  /** yyyy-MM-dd for list; prefers calendar date from API `yyyy-MM-dd` prefix to avoid TZ shifts. */
  hireDateLabel(v?: string | null): string {
    const t = (v ?? '').trim();
    if (!t) return '—';
    const m = /^(\d{4}-\d{2}-\d{2})/.exec(t);
    if (m) return m[1];
    const d = new Date(t);
    if (Number.isNaN(d.getTime())) return '—';
    const y = d.getFullYear();
    const mo = String(d.getMonth() + 1).padStart(2, '0');
    const day = String(d.getDate()).padStart(2, '0');
    return `${y}-${mo}-${day}`;
  }

  confirmDeactivate(row: EmployeeProfileReadDto): void {
    this.confirm.confirm({
      message: this.translate.instant('employeesHr.list.confirmDeactivate', {
        name: this.displayName(row),
        code: row.employeeCode,
      }),
      header: this.translate.instant('employeesHr.list.confirmHeader'),
      icon: 'pi pi-exclamation-triangle',
      acceptLabel: this.translate.instant('employeesHr.actions.deactivate'),
      rejectLabel: this.translate.instant('employeesHr.actions.cancel'),
      accept: () => this.runDeactivate(row.employeeProfileID),
    });
  }

  private runDeactivate(id: number): void {
    this.employeesHr.deactivateEmployee(id).subscribe({
      next: () => {
        this.toastr.success('employeesHr.toast.deactivated');
        this.load();
      },
      error: (err) => this.toastr.error(readHttpError(err)),
    });
  }
}
