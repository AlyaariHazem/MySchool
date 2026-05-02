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
import { School } from 'app/core/models/school.modul';
import { ShardModule } from 'app/shared/shard.module';

import { EmployeeProfileOptionDto, EmployeeProfilePageRequestDto } from '../../employees-hr/employees-hr.models';
import { EmployeesHrService } from '../../employees-hr/employees-hr.service';
import {
  ActivityApprovalDecision,
  ActivityEvaluationWriteDto,
  ActivityExecutionStatus,
  ActivityRequestStatus,
  ActivityRequestWriteDto,
} from '../activities.models';
import { ActivitiesService, readActivityHttpError } from '../activities.service';

export interface ApprovalFormRow {
  approverEmployeeProfileID: number | null;
  sortOrder: number;
  decision: ActivityApprovalDecision;
  comment: string;
  decidedAt: Date | null;
}

export interface ExecutionFormRow {
  status: ActivityExecutionStatus;
  notes: string;
  progressPercent: number;
  dueAt: Date | null;
  executedAt: Date | null;
  responsibleEmployeeProfileID: number | null;
}

export interface EvaluationFormRow {
  evaluatorEmployeeProfileID: number | null;
  score: number;
  feedback: string;
}

export interface PointsFormRow {
  points: number;
  reason: string;
  awardedByEmployeeProfileID: number | null;
}

@Component({
  selector: 'app-activity-form',
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
  templateUrl: './activity-form.component.html',
  styleUrl: './activity-form.component.scss',
})
export class ActivityFormComponent implements OnInit {
  @Input() embedded = false;
  @Input() recordIdInput: number | null = null;
  @Input() presetSchoolId: number | null = null;

  @Output() closed = new EventEmitter<void>();
  @Output() saved = new EventEmitter<void>();

  private readonly svc = inject(ActivitiesService);
  private readonly employeesHr = inject(EmployeesHrService);
  private readonly schoolService = inject(SchoolService);
  private readonly toastr = inject(ToastrService);
  private readonly translate = inject(TranslateService);
  private readonly perm = inject(PermissionService);

  readonly filterSelectPanelStyle: Record<string, string> = {
    maxWidth: 'min(22rem, calc(100vw - 2rem))',
  };

  readonly activityDatePickerAppendTo: 'body' = 'body';

  loading = false;
  saving = false;
  recordId: number | null = null;

  schoolID: number | null = null;
  employeeProfileID: number | null = null;
  title = '';
  details = '';
  status: ActivityRequestStatus = ActivityRequestStatus.Draft;

  schoolOptions: { label: string; value: number }[] = [];
  employeeOptions: { label: string; value: number }[] = [];
  requestStatusOptions: { label: string; value: number }[] = [];
  approvalDecisionOptions: { label: string; value: number }[] = [];
  executionStatusOptions: { label: string; value: number }[] = [];
  scoreOptions: { label: string; value: number }[] = [];

  approvalRows: ApprovalFormRow[] = [];
  executionRows: ExecutionFormRow[] = [];
  evaluationRows: EvaluationFormRow[] = [];
  pointsRows: PointsFormRow[] = [];

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

    this.requestStatusOptions = [
      { label: this.translate.instant('activities.status.draft'), value: ActivityRequestStatus.Draft },
      { label: this.translate.instant('activities.status.submitted'), value: ActivityRequestStatus.Submitted },
      { label: this.translate.instant('activities.status.inReview'), value: ActivityRequestStatus.InReview },
      { label: this.translate.instant('activities.status.approved'), value: ActivityRequestStatus.Approved },
      { label: this.translate.instant('activities.status.rejected'), value: ActivityRequestStatus.Rejected },
      { label: this.translate.instant('activities.status.inProgress'), value: ActivityRequestStatus.InProgress },
      { label: this.translate.instant('activities.status.completed'), value: ActivityRequestStatus.Completed },
      { label: this.translate.instant('activities.status.cancelled'), value: ActivityRequestStatus.Cancelled },
    ];
    this.approvalDecisionOptions = [
      { label: this.translate.instant('activities.approvalDecision.pending'), value: ActivityApprovalDecision.Pending },
      { label: this.translate.instant('activities.approvalDecision.approved'), value: ActivityApprovalDecision.Approved },
      { label: this.translate.instant('activities.approvalDecision.rejected'), value: ActivityApprovalDecision.Rejected },
      { label: this.translate.instant('activities.approvalDecision.skipped'), value: ActivityApprovalDecision.Skipped },
    ];
    this.executionStatusOptions = [
      { label: this.translate.instant('activities.executionStatus.pending'), value: ActivityExecutionStatus.Pending },
      { label: this.translate.instant('activities.executionStatus.inProgress'), value: ActivityExecutionStatus.InProgress },
      { label: this.translate.instant('activities.executionStatus.waitingExternal'), value: ActivityExecutionStatus.WaitingExternal },
      { label: this.translate.instant('activities.executionStatus.completed'), value: ActivityExecutionStatus.Completed },
      { label: this.translate.instant('activities.executionStatus.blocked'), value: ActivityExecutionStatus.Blocked },
      { label: this.translate.instant('activities.executionStatus.cancelled'), value: ActivityExecutionStatus.Cancelled },
    ];
    this.scoreOptions = [1, 2, 3, 4, 5].map((n) => ({
      label: String(n),
      value: n,
    }));

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
      this.loadEmployees();
    }
  }

  onSchoolChange(): void {
    this.loadEmployees();
  }

  private applyDefaultSchoolId(): void {
    if (this.schoolID != null && this.schoolID > 0) return;
    if (typeof localStorage === 'undefined') return;
    const raw = localStorage.getItem('schoolId');
    const n = raw != null && raw !== '' ? Number(raw) : NaN;
    if (Number.isFinite(n) && n > 0) this.schoolID = n;
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
      .getActivity(id)
      .pipe(
        finalize(() => (this.loading = false)),
        catchError((e) => {
          this.toastr.error(readActivityHttpError(e));
          return of(null);
        }),
      )
      .subscribe((d) => {
        if (!d) return;
        this.schoolID = d.schoolID;
        this.employeeProfileID = d.employeeProfileID;
        this.title = d.title;
        this.details = d.details ?? '';
        this.status = d.status as ActivityRequestStatus;
        this.approvalRows = (d.approvals ?? []).map((a) => ({
          approverEmployeeProfileID: a.approverEmployeeProfileID,
          sortOrder: a.sortOrder,
          decision: a.decision as ActivityApprovalDecision,
          comment: a.comment ?? '',
          decidedAt: a.decidedAtUtc ? new Date(a.decidedAtUtc) : null,
        }));
        this.executionRows = (d.executions ?? []).map((e) => ({
          status: e.status as ActivityExecutionStatus,
          notes: e.notes ?? '',
          progressPercent: e.progressPercent ?? 0,
          dueAt: e.dueAtUtc ? new Date(e.dueAtUtc) : null,
          executedAt: e.executedAtUtc ? new Date(e.executedAtUtc) : null,
          responsibleEmployeeProfileID: e.responsibleEmployeeProfileID ?? null,
        }));
        this.evaluationRows = (d.evaluations ?? []).map((ev) => ({
          evaluatorEmployeeProfileID: ev.evaluatorEmployeeProfileID,
          score: ev.score,
          feedback: ev.feedback ?? '',
        }));
        this.pointsRows = (d.points ?? []).map((p) => ({
          points: p.points,
          reason: p.reason ?? '',
          awardedByEmployeeProfileID: p.awardedByEmployeeProfileID,
        }));
        this.loadEmployees();
      });
  }

  addApprovalRow(): void {
    this.approvalRows.push({
      approverEmployeeProfileID: null,
      sortOrder: this.approvalRows.length,
      decision: ActivityApprovalDecision.Pending,
      comment: '',
      decidedAt: null,
    });
  }

  removeApprovalRow(i: number): void {
    this.approvalRows.splice(i, 1);
    this.reindexApprovalSort();
  }

  private reindexApprovalSort(): void {
    this.approvalRows.forEach((r, idx) => (r.sortOrder = idx));
  }

  addExecutionRow(): void {
    this.executionRows.push({
      status: ActivityExecutionStatus.Pending,
      notes: '',
      progressPercent: 0,
      dueAt: null,
      executedAt: null,
      responsibleEmployeeProfileID: null,
    });
  }

  removeExecutionRow(i: number): void {
    this.executionRows.splice(i, 1);
  }

  addEvaluationRow(): void {
    this.evaluationRows.push({
      evaluatorEmployeeProfileID: null,
      score: 5,
      feedback: '',
    });
  }

  removeEvaluationRow(i: number): void {
    this.evaluationRows.splice(i, 1);
  }

  addPointsRow(): void {
    this.pointsRows.push({
      points: 0,
      reason: '',
      awardedByEmployeeProfileID: null,
    });
  }

  removePointsRow(i: number): void {
    this.pointsRows.splice(i, 1);
  }

  cancel(): void {
    this.closed.emit();
  }

  private executionRowHasContent(r: ExecutionFormRow): boolean {
    return (
      (r.responsibleEmployeeProfileID != null && r.responsibleEmployeeProfileID > 0) ||
      !!(r.notes && r.notes.trim()) ||
      r.dueAt != null ||
      r.executedAt != null ||
      (typeof r.progressPercent === 'number' && r.progressPercent !== 0) ||
      r.status !== ActivityExecutionStatus.Pending
    );
  }

  private buildApprovals() {
    return this.approvalRows
      .filter((r) => r.approverEmployeeProfileID != null && r.approverEmployeeProfileID > 0)
      .map((r, idx) => ({
        approverEmployeeProfileID: r.approverEmployeeProfileID as number,
        sortOrder: idx,
        decision: r.decision,
        comment: r.comment?.trim() ? r.comment.trim() : null,
        decidedAtUtc: r.decidedAt ? r.decidedAt.toISOString() : null,
      }));
  }

  private buildExecutions() {
    return this.executionRows
      .filter((r) => this.executionRowHasContent(r))
      .map((r) => ({
        status: r.status,
        notes: r.notes?.trim() ? r.notes.trim() : null,
        progressPercent: Math.min(100, Math.max(0, Number(r.progressPercent) || 0)),
        dueAtUtc: r.dueAt ? r.dueAt.toISOString() : null,
        executedAtUtc: r.executedAt ? r.executedAt.toISOString() : null,
        responsibleEmployeeProfileID: r.responsibleEmployeeProfileID,
      }));
  }

  private buildEvaluations(): ActivityEvaluationWriteDto[] {
    return this.evaluationRows
      .filter((r) => r.evaluatorEmployeeProfileID != null && r.evaluatorEmployeeProfileID > 0)
      .map((r) => ({
        evaluatorEmployeeProfileID: r.evaluatorEmployeeProfileID as number,
        score: Math.min(5, Math.max(1, Number(r.score) || 1)),
        feedback: r.feedback?.trim() ? r.feedback.trim() : null,
      }));
  }

  private buildPoints() {
    return this.pointsRows
      .filter((r) => r.awardedByEmployeeProfileID != null && r.awardedByEmployeeProfileID > 0)
      .map((r) => ({
        points: Number.isFinite(Number(r.points)) ? Number(r.points) : 0,
        reason: r.reason?.trim() ? r.reason.trim() : null,
        awardedByEmployeeProfileID: r.awardedByEmployeeProfileID as number,
      }));
  }

  private buildWriteDto(): ActivityRequestWriteDto | null {
    const sid = this.schoolID;
    if (sid == null || sid <= 0) {
      this.toastr.warning(this.translate.instant('activities.form.validationSchool'));
      return null;
    }
    const emp = this.employeeProfileID;
    if (emp == null || emp <= 0) {
      this.toastr.warning(this.translate.instant('activities.form.validationEmployee'));
      return null;
    }
    if (!this.title.trim()) {
      this.toastr.warning(this.translate.instant('activities.form.validationTitle'));
      return null;
    }
    return {
      schoolID: sid,
      academicYearID: null,
      employeeProfileID: emp,
      title: this.title.trim(),
      details: this.details.trim() || null,
      status: this.status,
      approvals: this.buildApprovals(),
      executions: this.buildExecutions(),
      evaluations: this.buildEvaluations(),
      points: this.buildPoints(),
    };
  }

  save(): void {
    const dto = this.buildWriteDto();
    if (!dto) return;
    this.saving = true;
    const req$ = this.isEdit ? this.svc.updateActivity(this.recordId!, dto) : this.svc.createActivity(dto);
    req$
      .pipe(
        finalize(() => (this.saving = false)),
        catchError((e) => {
          this.toastr.error(readActivityHttpError(e));
          return of(null);
        }),
      )
      .subscribe((id) => {
        if (id == null) return;
        this.toastr.success(this.translate.instant(this.isEdit ? 'activities.toast.updated' : 'activities.toast.created'));
        this.saved.emit();
      });
  }
}
