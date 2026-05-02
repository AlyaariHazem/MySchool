import { AsyncPipe, DatePipe, NgIf } from '@angular/common';
import { Component, OnInit, inject } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Store } from '@ngrx/store';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { ButtonModule } from 'primeng/button';
import { DialogModule } from 'primeng/dialog';
import { FloatLabelModule } from 'primeng/floatlabel';
import { InputNumberModule } from 'primeng/inputnumber';
import { InputTextModule } from 'primeng/inputtext';
import { Select } from 'primeng/select';
import { TableModule } from 'primeng/table';
import { TabsModule } from 'primeng/tabs';
import { TextareaModule } from 'primeng/textarea';
import { DatePicker } from 'primeng/datepicker';
import { TooltipModule } from 'primeng/tooltip';
import { ToastrService } from 'ngx-toastr';
import { catchError, finalize, map } from 'rxjs/operators';
import { of } from 'rxjs';

import { isSchoolManagerUser } from 'app/core/utils/school-role.util';
import { PagePermission, PermissionService } from 'app/core/services/permission.service';
import { SchoolService } from 'app/core/services/school.service';
import { YearService } from 'app/core/services/year.service';
import { School } from 'app/core/models/school.modul';
import { Year } from 'app/core/models/year.model';
import { selectLanguage } from 'app/core/store/language/language.selectors';
import { ShardModule } from 'app/shared/shard.module';

import { EmployeeProfileOptionDto, EmployeeProfilePageRequestDto } from '../employees-hr/employees-hr.models';
import { EmployeesHrService } from '../employees-hr/employees-hr.service';
import { AnnualGoalFormComponent } from './annual-goal-form/annual-goal-form.component';
import {
  AnnualGoalFilterDto,
  AnnualGoalListItemDto,
  AnnualGoalStatus,
  DepartmentGoalDetailDto,
  DepartmentGoalFilterDto,
  DepartmentGoalListItemDto,
  DepartmentGoalStatus,
  DepartmentGoalWriteDto,
  StrategicGoalDetailDto,
  StrategicGoalFilterDto,
  StrategicGoalListItemDto,
  StrategicGoalStatus,
  StrategicGoalWriteDto,
} from './organizational-plans.models';
import { OrganizationalPlansService, readOrganizationalPlanHttpError } from './organizational-plans.service';

@Component({
  selector: 'app-organizational-plans-page',
  standalone: true,
  imports: [
    ShardModule,
    NgIf,
    AsyncPipe,
    DatePipe,
    FormsModule,
    TranslateModule,
    TabsModule,
    TableModule,
    ButtonModule,
    Select,
    FloatLabelModule,
    InputTextModule,
    TextareaModule,
    InputNumberModule,
    DatePicker,
    TooltipModule,
    DialogModule,
    AnnualGoalFormComponent,
  ],
  templateUrl: './organizational-plans-page.component.html',
  styleUrl: './organizational-plans-page.component.scss',
})
export class OrganizationalPlansPageComponent implements OnInit {
  private readonly svc = inject(OrganizationalPlansService);
  private readonly schoolService = inject(SchoolService);
  private readonly yearService = inject(YearService);
  private readonly toastr = inject(ToastrService);
  private readonly translate = inject(TranslateService);
  private readonly perm = inject(PermissionService);
  private readonly store = inject(Store);
  private readonly employeesHr = inject(EmployeesHrService);

  readonly dir$ = this.store.select(selectLanguage).pipe(map((l) => (l === 'ar' ? 'rtl' : 'ltr')));

  readonly filterSelectPanelStyle: Record<string, string> = {
    maxWidth: 'min(22rem, calc(100vw - 2rem))',
  };

  readonly dtAppendTo: 'body' = 'body';

  activeTab = '0';

  get isSchoolManager(): boolean {
    return isSchoolManagerUser();
  }

  get canView(): boolean {
    return this.perm.hasPermission(PagePermission.Employees.View);
  }

  get canCreate(): boolean {
    return this.perm.hasPermission(PagePermission.Employees.Create);
  }

  get canUpdate(): boolean {
    return this.perm.hasPermission(PagePermission.Employees.Update);
  }

  // --- Shared options ---
  schoolOptions: { label: string; value: number }[] = [];
  yearOptionsAnnual: { label: string; value: number }[] = [];
  yearOptionsDept: { label: string; value: number }[] = [];
  yearOptionsDeptDialog: { label: string; value: number }[] = [];
  strategicOptionsForDept: { label: string; value: number }[] = [];
  annualOptionsForDept: { label: string; value: number }[] = [];
  employeeOptionsDept: { label: string; value: number }[] = [];

  // --- Strategic tab ---
  stratFilter: StrategicGoalFilterDto = {};
  stratLoading = false;
  stratRows: StrategicGoalListItemDto[] = [];
  stratStatusOptions: { label: string; value: number }[] = [];
  stratDialogVisible = false;
  stratEditId: number | null = null;
  stratRefCode = '';
  stratTitle = '';
  stratDetails = '';
  stratStatus: StrategicGoalStatus = StrategicGoalStatus.Draft;
  stratSort = 0;
  stratFrom: Date | null = null;
  stratTo: Date | null = null;
  stratSchoolId: number | null = null;
  stratSaving = false;

  // --- Annual tab ---
  annualFilter: AnnualGoalFilterDto = {};
  annualLoading = false;
  annualRows: AnnualGoalListItemDto[] = [];
  annualStatusOptions: { label: string; value: number }[] = [];
  annualDialogVisible = false;
  annualEditId: number | null = null;

  // --- Department tab ---
  deptFilter: DepartmentGoalFilterDto = {};
  deptLoading = false;
  deptRows: DepartmentGoalListItemDto[] = [];
  deptStatusOptions: { label: string; value: number }[] = [];
  deptDialogVisible = false;
  deptEditId: number | null = null;
  deptSchoolId: number | null = null;
  deptYearId: number | null = null;
  deptStrategicId: number | null = null;
  deptAnnualId: number | null = null;
  deptName = '';
  deptTitle = '';
  deptDetails = '';
  deptStatus: DepartmentGoalStatus = DepartmentGoalStatus.Draft;
  deptSort = 0;
  deptOwnerId: number | null = null;
  deptSaving = false;

  ngOnInit(): void {
    this.stratStatusOptions = [
      { label: this.translate.instant('orgPlans.strategicStatus.draft'), value: StrategicGoalStatus.Draft },
      { label: this.translate.instant('orgPlans.strategicStatus.active'), value: StrategicGoalStatus.Active },
      { label: this.translate.instant('orgPlans.strategicStatus.achieved'), value: StrategicGoalStatus.Achieved },
      { label: this.translate.instant('orgPlans.strategicStatus.superseded'), value: StrategicGoalStatus.Superseded },
      { label: this.translate.instant('orgPlans.strategicStatus.archived'), value: StrategicGoalStatus.Archived },
      { label: this.translate.instant('orgPlans.strategicStatus.cancelled'), value: StrategicGoalStatus.Cancelled },
    ];
    this.annualStatusOptions = [
      { label: this.translate.instant('orgPlans.annualStatus.draft'), value: AnnualGoalStatus.Draft },
      { label: this.translate.instant('orgPlans.annualStatus.active'), value: AnnualGoalStatus.Active },
      { label: this.translate.instant('orgPlans.annualStatus.completed'), value: AnnualGoalStatus.Completed },
      { label: this.translate.instant('orgPlans.annualStatus.cancelled'), value: AnnualGoalStatus.Cancelled },
    ];
    this.deptStatusOptions = [
      { label: this.translate.instant('orgPlans.deptStatus.draft'), value: DepartmentGoalStatus.Draft },
      { label: this.translate.instant('orgPlans.deptStatus.active'), value: DepartmentGoalStatus.Active },
      { label: this.translate.instant('orgPlans.deptStatus.achieved'), value: DepartmentGoalStatus.Achieved },
      { label: this.translate.instant('orgPlans.deptStatus.cancelled'), value: DepartmentGoalStatus.Cancelled },
    ];

    if (!this.isSchoolManager) {
      this.schoolService.getAllSchools().subscribe({
        next: (schools: School[]) => {
          this.schoolOptions = (schools ?? [])
            .filter((s) => s.schoolID != null && s.schoolID > 0)
            .map((s) => ({
              label: s.schoolName ?? String(s.schoolID),
              value: s.schoolID as number,
            }));
        },
        error: () => undefined,
      });
    } else if (typeof localStorage !== 'undefined') {
      const raw = localStorage.getItem('schoolId');
      const n = raw != null && raw !== '' ? Number(raw) : NaN;
      if (Number.isFinite(n) && n > 0) {
        this.stratFilter.schoolID = n;
        this.annualFilter.schoolID = n;
        this.deptFilter.schoolID = n;
        this.loadYearOptionsAnnual(n);
        this.loadYearOptionsDeptFilter(n);
      }
    }

    if (this.canView) {
      this.loadStrategic();
      this.loadAnnual();
      this.loadDept();
    }
  }

  private mapYearsForSchool(years: Year[] | null | undefined, schoolId: number): { label: string; value: number }[] {
    return (years ?? [])
      .filter((y) => y.schoolID === schoolId && y.yearID > 0)
      .map((y) => ({
        label: `${y.yearID}${y.active ? ' *' : ''}`,
        value: y.yearID,
      }));
  }

  private loadYearOptionsAnnual(schoolId: number | null): void {
    if (schoolId == null || schoolId <= 0) {
      this.yearOptionsAnnual = [];
      return;
    }
    const sid = schoolId;
    this.yearService.getAllYears().subscribe({
      next: (years) => (this.yearOptionsAnnual = this.mapYearsForSchool(years, sid)),
      error: () => (this.yearOptionsAnnual = []),
    });
  }

  private loadYearOptionsDeptFilter(schoolId: number | null): void {
    if (schoolId == null || schoolId <= 0) {
      this.yearOptionsDept = [];
      return;
    }
    const sid = schoolId;
    this.yearService.getAllYears().subscribe({
      next: (years) => (this.yearOptionsDept = this.mapYearsForSchool(years, sid)),
      error: () => (this.yearOptionsDept = []),
    });
  }

  private loadYearOptionsDeptDialog(schoolId: number | null): void {
    if (schoolId == null || schoolId <= 0) {
      this.yearOptionsDeptDialog = [];
      return;
    }
    const sid = schoolId;
    this.yearService.getAllYears().subscribe({
      next: (years) => (this.yearOptionsDeptDialog = this.mapYearsForSchool(years, sid)),
      error: () => (this.yearOptionsDeptDialog = []),
    });
  }

  onAnnualFilterSchoolChange(): void {
    this.loadYearOptionsAnnual(this.annualFilter.schoolID ?? null);
  }

  onDeptFilterSchoolChange(): void {
    this.loadYearOptionsDeptFilter(this.deptFilter.schoolID ?? null);
    this.refreshDeptLinkOptions(this.deptFilter.schoolID ?? null);
  }

  private refreshDeptLinkOptions(schoolId: number | null): void {
    if (schoolId == null || schoolId <= 0) {
      this.strategicOptionsForDept = [];
      this.annualOptionsForDept = [];
      return;
    }
    this.svc.listStrategicGoals({ schoolID: schoolId }).subscribe({
      next: (rows) =>
        (this.strategicOptionsForDept = (rows ?? []).map((r) => ({
          label: r.title,
          value: r.strategicGoalID,
        }))),
      error: () => (this.strategicOptionsForDept = []),
    });
    this.svc.listAnnualGoals({ schoolID: schoolId }).subscribe({
      next: (rows) =>
        (this.annualOptionsForDept = (rows ?? []).map((r) => ({
          label: r.title,
          value: r.annualGoalID,
        }))),
      error: () => (this.annualOptionsForDept = []),
    });
  }

  // ----- Strategic -----
  loadStrategic(): void {
    if (!this.canView) return;
    this.stratLoading = true;
    const f = { ...this.stratFilter };
    if (f.status === null || f.status === undefined) delete f.status;
    this.svc
      .listStrategicGoals(f)
      .pipe(finalize(() => (this.stratLoading = false)))
      .subscribe({
        next: (r) => (this.stratRows = r ?? []),
        error: (e) => this.toastr.error(readOrganizationalPlanHttpError(e)),
      });
  }

  openStrategicCreate(): void {
    this.stratEditId = null;
    this.stratRefCode = '';
    this.stratTitle = '';
    this.stratDetails = '';
    this.stratStatus = StrategicGoalStatus.Draft;
    this.stratSort = 0;
    this.stratFrom = null;
    this.stratTo = null;
    this.stratSchoolId = this.stratFilter.schoolID ?? (this.isSchoolManager ? this.getManagerSchoolFromStorage() : null);
    this.stratDialogVisible = true;
  }

  private getManagerSchoolFromStorage(): number | null {
    if (typeof localStorage === 'undefined') return null;
    const raw = localStorage.getItem('schoolId');
    const n = raw != null && raw !== '' ? Number(raw) : NaN;
    return Number.isFinite(n) && n > 0 ? n : null;
  }

  openStrategicEdit(id: number): void {
    this.stratEditId = id;
    this.svc.getStrategicGoal(id).subscribe({
      next: (d: StrategicGoalDetailDto) => {
        this.stratSchoolId = d.schoolID;
        this.stratRefCode = d.referenceCode ?? '';
        this.stratTitle = d.title;
        this.stratDetails = d.details ?? '';
        this.stratStatus = d.status as StrategicGoalStatus;
        this.stratSort = d.sortOrder;
        this.stratFrom = d.effectiveFromUtc ? new Date(d.effectiveFromUtc) : null;
        this.stratTo = d.effectiveToUtc ? new Date(d.effectiveToUtc) : null;
        this.stratDialogVisible = true;
      },
      error: (e) => this.toastr.error(readOrganizationalPlanHttpError(e)),
    });
  }

  closeStrategicDialog(): void {
    this.stratDialogVisible = false;
    this.stratEditId = null;
  }

  saveStrategic(): void {
    const sid = this.stratSchoolId;
    if (sid == null || sid <= 0) {
      this.toastr.warning(this.translate.instant('orgPlans.form.validationSchool'));
      return;
    }
    if (!this.stratTitle.trim()) {
      this.toastr.warning(this.translate.instant('orgPlans.form.validationTitle'));
      return;
    }
    const dto: StrategicGoalWriteDto = {
      schoolID: sid,
      referenceCode: this.stratRefCode.trim() || null,
      title: this.stratTitle.trim(),
      details: this.stratDetails.trim() || null,
      status: this.stratStatus,
      sortOrder: this.stratSort,
      effectiveFromUtc: this.stratFrom ? this.stratFrom.toISOString() : null,
      effectiveToUtc: this.stratTo ? this.stratTo.toISOString() : null,
    };
    this.stratSaving = true;
    const req$ =
      this.stratEditId != null && this.stratEditId > 0
        ? this.svc.updateStrategicGoal(this.stratEditId, dto)
        : this.svc.createStrategicGoal(dto);
    req$.pipe(finalize(() => (this.stratSaving = false))).subscribe({
      next: () => {
        this.toastr.success(
          this.translate.instant(this.stratEditId ? 'orgPlans.toast.strategicUpdated' : 'orgPlans.toast.strategicCreated'),
        );
        this.closeStrategicDialog();
        this.loadStrategic();
      },
      error: (e) => this.toastr.error(readOrganizationalPlanHttpError(e)),
    });
  }

  strategicStatusLabel(v: number): string {
    const m: Record<number, string> = {
      [StrategicGoalStatus.Draft]: 'orgPlans.strategicStatus.draft',
      [StrategicGoalStatus.Active]: 'orgPlans.strategicStatus.active',
      [StrategicGoalStatus.Achieved]: 'orgPlans.strategicStatus.achieved',
      [StrategicGoalStatus.Superseded]: 'orgPlans.strategicStatus.superseded',
      [StrategicGoalStatus.Archived]: 'orgPlans.strategicStatus.archived',
      [StrategicGoalStatus.Cancelled]: 'orgPlans.strategicStatus.cancelled',
    };
    const k = m[v];
    return k ? this.translate.instant(k) : String(v);
  }

  // ----- Annual -----
  loadAnnual(): void {
    if (!this.canView) return;
    this.annualLoading = true;
    const f = { ...this.annualFilter };
    if (f.status === null || f.status === undefined) delete f.status;
    this.svc
      .listAnnualGoals(f)
      .pipe(finalize(() => (this.annualLoading = false)))
      .subscribe({
        next: (r) => (this.annualRows = r ?? []),
        error: (e) => this.toastr.error(readOrganizationalPlanHttpError(e)),
      });
  }

  openAnnualCreate(): void {
    this.annualEditId = null;
    this.annualDialogVisible = true;
  }

  openAnnualEdit(id: number): void {
    this.annualEditId = id;
    this.annualDialogVisible = true;
  }

  closeAnnualDialog(): void {
    this.annualDialogVisible = false;
    this.annualEditId = null;
  }

  onAnnualSaved(): void {
    this.closeAnnualDialog();
    this.loadAnnual();
  }

  annualStatusLabel(v: number): string {
    const m: Record<number, string> = {
      [AnnualGoalStatus.Draft]: 'orgPlans.annualStatus.draft',
      [AnnualGoalStatus.Active]: 'orgPlans.annualStatus.active',
      [AnnualGoalStatus.Completed]: 'orgPlans.annualStatus.completed',
      [AnnualGoalStatus.Cancelled]: 'orgPlans.annualStatus.cancelled',
    };
    const k = m[v];
    return k ? this.translate.instant(k) : String(v);
  }

  // ----- Department -----
  loadDept(): void {
    if (!this.canView) return;
    this.deptLoading = true;
    const f = { ...this.deptFilter };
    if (f.status === null || f.status === undefined) delete f.status;
    this.svc
      .listDepartmentGoals(f)
      .pipe(finalize(() => (this.deptLoading = false)))
      .subscribe({
        next: (r) => (this.deptRows = r ?? []),
        error: (e) => this.toastr.error(readOrganizationalPlanHttpError(e)),
      });
  }

  openDeptCreate(): void {
    this.deptEditId = null;
    this.deptSchoolId = this.deptFilter.schoolID ?? this.getManagerSchoolFromStorage();
    this.deptYearId = this.deptFilter.academicYearID ?? null;
    this.deptStrategicId = null;
    this.deptAnnualId = null;
    this.deptName = '';
    this.deptTitle = '';
    this.deptDetails = '';
    this.deptStatus = DepartmentGoalStatus.Draft;
    this.deptSort = 0;
    this.deptOwnerId = null;
    this.refreshDeptLinkOptions(this.deptSchoolId);
    this.loadYearOptionsDeptDialog(this.deptSchoolId);
    this.loadDeptEmployees();
    this.deptDialogVisible = true;
  }

  openDeptEdit(id: number): void {
    this.deptEditId = id;
    this.svc.getDepartmentGoal(id).subscribe({
      next: (d: DepartmentGoalDetailDto) => {
        this.deptSchoolId = d.schoolID;
        this.deptYearId = d.academicYearID ?? null;
        this.deptStrategicId = d.strategicGoalID ?? null;
        this.deptAnnualId = d.annualGoalID ?? null;
        this.deptName = d.departmentName;
        this.deptTitle = d.title;
        this.deptDetails = d.details ?? '';
        this.deptStatus = d.status as DepartmentGoalStatus;
        this.deptSort = d.sortOrder;
        this.deptOwnerId = d.ownerEmployeeProfileID ?? null;
        this.refreshDeptLinkOptions(d.schoolID);
        this.loadYearOptionsDeptDialog(d.schoolID);
        this.loadDeptEmployees();
        this.deptDialogVisible = true;
      },
      error: (e) => this.toastr.error(readOrganizationalPlanHttpError(e)),
    });
  }

  onDeptDialogSchoolChange(): void {
    this.loadYearOptionsDeptDialog(this.deptSchoolId);
    this.refreshDeptLinkOptions(this.deptSchoolId);
    this.loadDeptEmployees();
  }

  private loadDeptEmployees(): void {
    const sid = this.deptSchoolId;
    if (sid == null || sid <= 0) {
      this.employeeOptionsDept = [];
      return;
    }
    const body: EmployeeProfilePageRequestDto = {
      pageIndex: 0,
      pageSize: 500,
      filter: { schoolID: sid },
    };
    this.employeesHr
      .getEmployeesPage(body)
      .pipe(
        map((p) => {
          const rows = p?.data ?? [];
          return rows.map((o: EmployeeProfileOptionDto) => {
            const n = o.fullName;
            const parts = [n?.firstName, n?.middleName, n?.lastName].filter((x) => !!x?.trim());
            const label = parts.length ? parts.join(' ') : String(o.id);
            return { label, value: o.id };
          });
        }),
        catchError(() => of([] as { label: string; value: number }[])),
      )
      .subscribe((opts) => (this.employeeOptionsDept = opts.filter((x) => x.value > 0)));
  }

  closeDeptDialog(): void {
    this.deptDialogVisible = false;
    this.deptEditId = null;
  }

  saveDept(): void {
    const sid = this.deptSchoolId;
    if (sid == null || sid <= 0) {
      this.toastr.warning(this.translate.instant('orgPlans.form.validationSchool'));
      return;
    }
    if (!this.deptName.trim() || !this.deptTitle.trim()) {
      this.toastr.warning(this.translate.instant('orgPlans.form.validationDept'));
      return;
    }
    const dto: DepartmentGoalWriteDto = {
      schoolID: sid,
      academicYearID: this.deptYearId,
      strategicGoalID: this.deptStrategicId,
      annualGoalID: this.deptAnnualId,
      departmentName: this.deptName.trim(),
      title: this.deptTitle.trim(),
      details: this.deptDetails.trim() || null,
      status: this.deptStatus,
      sortOrder: this.deptSort,
      ownerEmployeeProfileID: this.deptOwnerId,
    };
    this.deptSaving = true;
    const req$ =
      this.deptEditId != null && this.deptEditId > 0
        ? this.svc.updateDepartmentGoal(this.deptEditId, dto)
        : this.svc.createDepartmentGoal(dto);
    req$.pipe(finalize(() => (this.deptSaving = false))).subscribe({
      next: () => {
        this.toastr.success(this.translate.instant(this.deptEditId ? 'orgPlans.toast.deptUpdated' : 'orgPlans.toast.deptCreated'));
        this.closeDeptDialog();
        this.loadDept();
      },
      error: (e) => this.toastr.error(readOrganizationalPlanHttpError(e)),
    });
  }

  deptStatusLabel(v: number): string {
    const m: Record<number, string> = {
      [DepartmentGoalStatus.Draft]: 'orgPlans.deptStatus.draft',
      [DepartmentGoalStatus.Active]: 'orgPlans.deptStatus.active',
      [DepartmentGoalStatus.Achieved]: 'orgPlans.deptStatus.achieved',
      [DepartmentGoalStatus.Cancelled]: 'orgPlans.deptStatus.cancelled',
    };
    const k = m[v];
    return k ? this.translate.instant(k) : String(v);
  }
}
