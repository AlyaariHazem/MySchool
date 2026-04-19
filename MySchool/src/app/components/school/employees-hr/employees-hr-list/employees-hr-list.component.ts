import { AsyncPipe, DatePipe, NgIf } from '@angular/common';
import { Component, inject, OnDestroy, OnInit } from '@angular/core';
import { Subscription } from 'rxjs';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { Store } from '@ngrx/store';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { ConfirmationService } from 'primeng/api';
import { ButtonModule } from 'primeng/button';
import { ConfirmDialogModule } from 'primeng/confirmdialog';
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
  EmployeeJobTypeDto,
  EmployeeProfileListFilterDto,
  EmployeeProfileReadDto,
  EmploymentStatus,
} from '../employees-hr.models';
import { EmployeesHrService, readHttpError } from '../employees-hr.service';

@Component({
  selector: 'app-employees-hr-list',
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
    ConfirmDialogModule,
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
