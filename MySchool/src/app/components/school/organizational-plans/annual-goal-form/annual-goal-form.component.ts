import { NgFor, NgIf } from '@angular/common';
import { Component, EventEmitter, Input, OnInit, Output, inject } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { ButtonModule } from 'primeng/button';
import { DatePicker } from 'primeng/datepicker';
import { FloatLabelModule } from 'primeng/floatlabel';
import { InputNumberModule } from 'primeng/inputnumber';
import { InputTextModule } from 'primeng/inputtext';
import { Select } from 'primeng/select';
import { TextareaModule } from 'primeng/textarea';
import { ToastrService } from 'ngx-toastr';
import { catchError, finalize, map } from 'rxjs/operators';
import { of } from 'rxjs';

import { isSchoolManagerUser } from 'app/core/utils/school-role.util';
import { PagePermission, PermissionService } from 'app/core/services/permission.service';
import { SchoolService } from 'app/core/services/school.service';
import { YearService } from 'app/core/services/year.service';
import { School } from 'app/core/models/school.modul';
import { Year } from 'app/core/models/year.model';
import { ShardModule } from 'app/shared/shard.module';

import { EmployeeProfileOptionDto, EmployeeProfilePageRequestDto } from '../../employees-hr/employees-hr.models';
import { EmployeesHrService } from '../../employees-hr/employees-hr.service';
import {
  AnnualGoalStatus,
  AnnualGoalWriteDto,
  OperationalPlanStatus,
  PlanTaskStatus,
} from '../organizational-plans.models';
import { OrganizationalPlansService, readOrganizationalPlanHttpError } from '../organizational-plans.service';

export interface ProgressUpdateFormRow {
  note: string;
  progressPercent: number | null;
  authorEmployeeProfileID: number | null;
}

export interface PlanTaskFormRow {
  title: string;
  details: string;
  status: PlanTaskStatus;
  sortOrder: number;
  progressPercent: number;
  dueAt: Date | null;
  assignedToEmployeeProfileID: number | null;
  updates: ProgressUpdateFormRow[];
}

export interface OperationalPlanFormRow {
  title: string;
  details: string;
  status: OperationalPlanStatus;
  sortOrder: number;
  startAt: Date | null;
  endAt: Date | null;
  ownerEmployeeProfileID: number | null;
  tasks: PlanTaskFormRow[];
}

@Component({
  selector: 'app-annual-goal-form',
  standalone: true,
  imports: [
    ShardModule,
    NgIf,
    NgFor,
    FormsModule,
    TranslateModule,
    ButtonModule,
    Select,
    FloatLabelModule,
    InputTextModule,
    TextareaModule,
    DatePicker,
    InputNumberModule,
  ],
  templateUrl: './annual-goal-form.component.html',
  styleUrl: './annual-goal-form.component.scss',
})
export class AnnualGoalFormComponent implements OnInit {
  @Input() embedded = false;
  @Input() recordIdInput: number | null = null;
  @Input() presetSchoolId: number | null = null;

  @Output() closed = new EventEmitter<void>();
  @Output() saved = new EventEmitter<void>();

  private readonly svc = inject(OrganizationalPlansService);
  private readonly employeesHr = inject(EmployeesHrService);
  private readonly schoolService = inject(SchoolService);
  private readonly yearService = inject(YearService);
  private readonly toastr = inject(ToastrService);
  private readonly translate = inject(TranslateService);
  private readonly perm = inject(PermissionService);

  readonly filterSelectPanelStyle: Record<string, string> = {
    maxWidth: 'min(22rem, calc(100vw - 2rem))',
  };

  readonly dtAppendTo: 'body' = 'body';

  loading = false;
  saving = false;
  recordId: number | null = null;

  schoolID: number | null = null;
  academicYearID: number | null = null;
  strategicGoalID: number | null = null;
  title = '';
  details = '';
  status: AnnualGoalStatus = AnnualGoalStatus.Draft;
  sortOrder = 0;

  schoolOptions: { label: string; value: number }[] = [];
  yearOptions: { label: string; value: number }[] = [];
  strategicOptions: { label: string; value: number }[] = [];
  employeeOptions: { label: string; value: number }[] = [];
  annualStatusOptions: { label: string; value: number }[] = [];
  opStatusOptions: { label: string; value: number }[] = [];
  taskStatusOptions: { label: string; value: number }[] = [];

  planRows: OperationalPlanFormRow[] = [];

  get isSchoolManager(): boolean {
    return isSchoolManagerUser();
  }

  get isEdit(): boolean {
    return this.recordId != null && this.recordId > 0;
  }

  get canSubmit(): boolean {
    return this.isEdit ? this.perm.hasPermission(PagePermission.Employees.Update) : this.perm.hasPermission(PagePermission.Employees.Create);
  }

  ngOnInit(): void {
    this.recordId =
      this.embedded && this.recordIdInput != null && this.recordIdInput > 0 ? this.recordIdInput : null;

    this.annualStatusOptions = [
      { label: this.translate.instant('orgPlans.annualStatus.draft'), value: AnnualGoalStatus.Draft },
      { label: this.translate.instant('orgPlans.annualStatus.active'), value: AnnualGoalStatus.Active },
      { label: this.translate.instant('orgPlans.annualStatus.completed'), value: AnnualGoalStatus.Completed },
      { label: this.translate.instant('orgPlans.annualStatus.cancelled'), value: AnnualGoalStatus.Cancelled },
    ];
    this.opStatusOptions = [
      { label: this.translate.instant('orgPlans.opStatus.draft'), value: OperationalPlanStatus.Draft },
      { label: this.translate.instant('orgPlans.opStatus.active'), value: OperationalPlanStatus.Active },
      { label: this.translate.instant('orgPlans.opStatus.onHold'), value: OperationalPlanStatus.OnHold },
      { label: this.translate.instant('orgPlans.opStatus.completed'), value: OperationalPlanStatus.Completed },
      { label: this.translate.instant('orgPlans.opStatus.cancelled'), value: OperationalPlanStatus.Cancelled },
    ];
    this.taskStatusOptions = [
      { label: this.translate.instant('orgPlans.taskStatus.notStarted'), value: PlanTaskStatus.NotStarted },
      { label: this.translate.instant('orgPlans.taskStatus.inProgress'), value: PlanTaskStatus.InProgress },
      { label: this.translate.instant('orgPlans.taskStatus.blocked'), value: PlanTaskStatus.Blocked },
      { label: this.translate.instant('orgPlans.taskStatus.completed'), value: PlanTaskStatus.Completed },
      { label: this.translate.instant('orgPlans.taskStatus.cancelled'), value: PlanTaskStatus.Cancelled },
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
    }

    this.applyDefaultSchoolId();
    if (this.embedded && this.presetSchoolId != null && this.presetSchoolId > 0) {
      this.schoolID = this.presetSchoolId;
    }

    if (this.recordId) {
      this.loadRecord();
    } else {
      this.onSchoolOrYearChange();
      this.addPlanRow();
    }
  }

  private applyDefaultSchoolId(): void {
    if (this.schoolID != null && this.schoolID > 0) return;
    if (typeof localStorage === 'undefined') return;
    const raw = localStorage.getItem('schoolId');
    const n = raw != null && raw !== '' ? Number(raw) : NaN;
    if (Number.isFinite(n) && n > 0) this.schoolID = n;
  }

  onSchoolChange(): void {
    this.loadYears();
    this.loadStrategicOptions();
    this.loadEmployees();
  }

  onSchoolOrYearChange(): void {
    this.loadYears();
    this.loadStrategicOptions();
    this.loadEmployees();
  }

  private loadYears(): void {
    const sid = this.schoolID;
    if (sid == null || sid <= 0) {
      this.yearOptions = [];
      return;
    }
    this.yearService.getAllYears().subscribe({
      next: (years: Year[]) => {
        this.yearOptions = (years ?? [])
          .filter((y) => y.schoolID === sid && y.yearID > 0)
          .map((y) => ({
            label: `${y.yearID}${y.active ? ' *' : ''}`,
            value: y.yearID,
          }));
      },
      error: () => (this.yearOptions = []),
    });
  }

  private loadStrategicOptions(): void {
    const sid = this.schoolID;
    if (sid == null || sid <= 0) {
      this.strategicOptions = [];
      return;
    }
    this.svc.listStrategicGoals({ schoolID: sid }).subscribe({
      next: (rows) => {
        this.strategicOptions = (rows ?? []).map((r) => ({
          label: r.title || String(r.strategicGoalID),
          value: r.strategicGoalID,
        }));
      },
      error: () => (this.strategicOptions = []),
    });
  }

  private loadEmployees(): void {
    const sid = this.schoolID;
    if (sid == null || sid <= 0) {
      this.employeeOptions = [];
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
          return rows.map((o: EmployeeProfileOptionDto) => ({
            label: this.displayNameFromOption(o),
            value: o.id,
          }));
        }),
        catchError(() => of([] as { label: string; value: number }[])),
      )
      .subscribe((opts) => (this.employeeOptions = opts.filter((x) => x.value > 0)));
  }

  private displayNameFromOption(o: EmployeeProfileOptionDto): string {
    const n = o.fullName;
    if (!n) return String(o.id);
    const parts = [n.firstName, n.middleName, n.lastName].filter((x) => !!x?.trim());
    return parts.length ? parts.join(' ') : String(o.id);
  }

  private loadRecord(): void {
    const id = this.recordId;
    if (id == null || id <= 0) return;
    this.loading = true;
    this.svc
      .getAnnualGoal(id)
      .pipe(
        finalize(() => (this.loading = false)),
        catchError((e) => {
          this.toastr.error(readOrganizationalPlanHttpError(e));
          return of(null);
        }),
      )
      .subscribe((d) => {
        if (!d) return;
        this.schoolID = d.schoolID;
        this.academicYearID = d.academicYearID;
        this.strategicGoalID = d.strategicGoalID ?? null;
        this.title = d.title;
        this.details = d.details ?? '';
        this.status = d.status as AnnualGoalStatus;
        this.sortOrder = d.sortOrder;
        this.planRows = (d.operationalPlans ?? []).map((p) => ({
          title: p.title,
          details: p.details ?? '',
          status: p.status as OperationalPlanStatus,
          sortOrder: p.sortOrder,
          startAt: p.startDateUtc ? new Date(p.startDateUtc) : null,
          endAt: p.endDateUtc ? new Date(p.endDateUtc) : null,
          ownerEmployeeProfileID: p.ownerEmployeeProfileID ?? null,
          tasks: (p.tasks ?? []).map((t) => ({
            title: t.title,
            details: t.details ?? '',
            status: t.status as PlanTaskStatus,
            sortOrder: t.sortOrder,
            progressPercent: t.progressPercent,
            dueAt: t.dueAtUtc ? new Date(t.dueAtUtc) : null,
            assignedToEmployeeProfileID: t.assignedToEmployeeProfileID ?? null,
            updates: (t.progressUpdates ?? []).map((u) => ({
              note: u.note ?? '',
              progressPercent: u.progressPercent ?? null,
              authorEmployeeProfileID: u.authorEmployeeProfileID ?? null,
            })),
          })),
        }));
        if (this.planRows.length === 0) this.addPlanRow();
        this.onSchoolOrYearChange();
      });
  }

  addPlanRow(): void {
    this.planRows.push({
      title: '',
      details: '',
      status: OperationalPlanStatus.Draft,
      sortOrder: this.planRows.length,
      startAt: null,
      endAt: null,
      ownerEmployeeProfileID: null,
      tasks: [],
    });
    this.addTaskRow(this.planRows[this.planRows.length - 1]);
  }

  removePlanRow(i: number): void {
    this.planRows.splice(i, 1);
    if (this.planRows.length === 0) this.addPlanRow();
  }

  addTaskRow(plan: OperationalPlanFormRow): void {
    plan.tasks.push({
      title: '',
      details: '',
      status: PlanTaskStatus.NotStarted,
      sortOrder: plan.tasks.length,
      progressPercent: 0,
      dueAt: null,
      assignedToEmployeeProfileID: null,
      updates: [],
    });
  }

  removeTaskRow(plan: OperationalPlanFormRow, i: number): void {
    plan.tasks.splice(i, 1);
    if (plan.tasks.length === 0) this.addTaskRow(plan);
  }

  addUpdateRow(task: PlanTaskFormRow): void {
    task.updates.push({ note: '', progressPercent: null, authorEmployeeProfileID: null });
  }

  removeUpdateRow(task: PlanTaskFormRow, i: number): void {
    task.updates.splice(i, 1);
  }

  cancel(): void {
    this.closed.emit();
  }

  private buildWriteDto(): AnnualGoalWriteDto | null {
    const sid = this.schoolID;
    if (sid == null || sid <= 0) {
      this.toastr.warning(this.translate.instant('orgPlans.form.validationSchool'));
      return null;
    }
    const yid = this.academicYearID;
    if (yid == null || yid <= 0) {
      this.toastr.warning(this.translate.instant('orgPlans.form.validationYear'));
      return null;
    }
    if (!this.title.trim()) {
      this.toastr.warning(this.translate.instant('orgPlans.form.validationTitle'));
      return null;
    }

    const operationalPlans = this.planRows
      .filter((p) => p.title.trim())
      .map((p, pi) => ({
        title: p.title.trim(),
        details: p.details.trim() || null,
        status: p.status,
        sortOrder: pi,
        startDateUtc: p.startAt ? p.startAt.toISOString() : null,
        endDateUtc: p.endAt ? p.endAt.toISOString() : null,
        ownerEmployeeProfileID: p.ownerEmployeeProfileID,
        tasks: p.tasks
          .filter((t) => t.title.trim())
          .map((t, ti) => ({
            title: t.title.trim(),
            details: t.details.trim() || null,
            status: t.status,
            sortOrder: ti,
            progressPercent: Math.min(100, Math.max(0, Number(t.progressPercent) || 0)),
            dueAtUtc: t.dueAt ? t.dueAt.toISOString() : null,
            assignedToEmployeeProfileID: t.assignedToEmployeeProfileID,
            progressUpdates: t.updates
              .filter((u) => u.note.trim() || u.progressPercent != null || (u.authorEmployeeProfileID != null && u.authorEmployeeProfileID > 0))
              .map((u) => ({
                note: u.note.trim() || null,
                progressPercent: u.progressPercent != null ? Math.min(100, Math.max(0, u.progressPercent)) : null,
                authorEmployeeProfileID: u.authorEmployeeProfileID,
              })),
          })),
      }));

    return {
      schoolID: sid,
      academicYearID: yid,
      strategicGoalID: this.strategicGoalID,
      title: this.title.trim(),
      details: this.details.trim() || null,
      status: this.status,
      sortOrder: this.sortOrder,
      operationalPlans,
    };
  }

  save(): void {
    const dto = this.buildWriteDto();
    if (!dto) return;
    this.saving = true;
    const req$ = this.isEdit ? this.svc.updateAnnualGoal(this.recordId!, dto) : this.svc.createAnnualGoal(dto);
    req$
      .pipe(
        finalize(() => (this.saving = false)),
        catchError((e) => {
          this.toastr.error(readOrganizationalPlanHttpError(e));
          return of(null);
        }),
      )
      .subscribe((id) => {
        if (id == null) return;
        this.toastr.success(this.translate.instant(this.isEdit ? 'orgPlans.toast.annualUpdated' : 'orgPlans.toast.annualCreated'));
        this.saved.emit();
      });
  }
}
