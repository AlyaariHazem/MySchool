import { DatePipe, NgIf } from '@angular/common';
import { Component, EventEmitter, Input, OnInit, Output, inject } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { ButtonModule } from 'primeng/button';
import { CheckboxModule } from 'primeng/checkbox';
import { FloatLabelModule } from 'primeng/floatlabel';
import { InputNumberModule } from 'primeng/inputnumber';
import { InputTextModule } from 'primeng/inputtext';
import { Select } from 'primeng/select';
import { TableModule } from 'primeng/table';
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
  EmployeeRequestCategory,
  EmployeeRequestDetailDto,
  EmployeeRequestStatus,
  EmployeeRequestTypeListItemDto,
  EmployeeRequestWriteDto,
  RequestApprovalDecision,
  RequestExecutionStatus,
} from '../employee-requests.models';
import { EmployeeRequestsService, readEmployeeRequestHttpError } from '../employee-requests.service';

@Component({
  selector: 'app-employee-request-form',
  standalone: true,
  imports: [
    ShardModule,
    NgIf,
    DatePipe,
    FormsModule,
    TranslateModule,
    ButtonModule,
    Select,
    FloatLabelModule,
    InputTextModule,
    TextareaModule,
    InputNumberModule,
    TableModule,
    CheckboxModule,
  ],
  templateUrl: './employee-request-form.component.html',
  styleUrl: './employee-request-form.component.scss',
})
export class EmployeeRequestFormComponent implements OnInit {
  @Input() embedded = false;
  @Input() requestIdInput: number | null = null;
  @Input() presetSchoolId: number | null = null;

  @Output() closed = new EventEmitter<void>();
  @Output() saved = new EventEmitter<void>();

  private readonly svc = inject(EmployeeRequestsService);
  private readonly employeesHr = inject(EmployeesHrService);
  private readonly schoolService = inject(SchoolService);
  private readonly toastr = inject(ToastrService);
  private readonly translate = inject(TranslateService);
  private readonly perm = inject(PermissionService);

  readonly filterSelectPanelStyle: Record<string, string> = {
    maxWidth: 'min(22rem, calc(100vw - 2rem))',
  };

  loading = false;
  saving = false;
  requestId: number | null = null;

  schoolID: number | null = null;
  employeeProfileID: number | null = null;
  requestTypeID: number | null = null;
  title = '';
  details = '';
  requestedAmount: number | null = null;
  status: EmployeeRequestStatus = EmployeeRequestStatus.Draft;

  schoolOptions: { label: string; value: number }[] = [];
  employeeOptions: { label: string; value: number }[] = [];
  typeOptions: { label: string; value: number }[] = [];
  statusOptions: { label: string; value: number }[] = [];
  executionStatusOptions: { label: string; value: number }[] = [];

  typeCatalog: EmployeeRequestTypeListItemDto[] = [];
  detail: EmployeeRequestDetailDto | null = null;

  newApprovalApproverId: number | null = null;
  newApprovalStepOrder: number | null = null;
  submittingApproval = false;

  newExecStatus: RequestExecutionStatus = RequestExecutionStatus.InProgress;
  newExecNotes = '';
  newExecProgress = 0;
  newExecDue = '';
  newExecResponsibleId: number | null = null;
  submittingExec = false;

  summaryDateStr = '';
  newSummaryText = '';
  newSummaryProgress: number | null = null;
  newSummaryFinal = false;
  newSummaryAuthorId: number | null = null;
  submittingSummary = false;

  get isSchoolManager(): boolean {
    return isSchoolManagerUser();
  }

  get isEdit(): boolean {
    return this.requestId != null && this.requestId > 0;
  }

  get canSubmit(): boolean {
    return this.isEdit
      ? this.perm.hasPermission(PagePermission.Employees.Update)
      : this.perm.hasPermission(PagePermission.Employees.Create);
  }

  categoryLabel(cat: number): string {
    const m: Record<number, string> = {
      [EmployeeRequestCategory.Tools]: 'employeeRequests.category.tools',
      [EmployeeRequestCategory.Advance]: 'employeeRequests.category.advance',
      [EmployeeRequestCategory.Support]: 'employeeRequests.category.support',
    };
    const key = m[cat];
    return key ? this.translate.instant(key) : String(cat);
  }

  execStatusLabel(v: number): string {
    const m: Record<number, string> = {
      [RequestExecutionStatus.Pending]: 'employeeRequests.execStatus.pending',
      [RequestExecutionStatus.InProgress]: 'employeeRequests.execStatus.inProgress',
      [RequestExecutionStatus.WaitingExternal]: 'employeeRequests.execStatus.waitingExternal',
      [RequestExecutionStatus.Completed]: 'employeeRequests.execStatus.completed',
      [RequestExecutionStatus.Blocked]: 'employeeRequests.execStatus.blocked',
      [RequestExecutionStatus.Cancelled]: 'employeeRequests.execStatus.cancelled',
    };
    const key = m[v];
    return key ? this.translate.instant(key) : String(v);
  }

  approvalDecisionLabel(v: number): string {
    const m: Record<number, string> = {
      [RequestApprovalDecision.Pending]: 'employeeRequests.approval.pending',
      [RequestApprovalDecision.Approved]: 'employeeRequests.approval.approved',
      [RequestApprovalDecision.Rejected]: 'employeeRequests.approval.rejected',
      [RequestApprovalDecision.Skipped]: 'employeeRequests.approval.skipped',
    };
    const key = m[v];
    return key ? this.translate.instant(key) : String(v);
  }

  ngOnInit(): void {
    this.requestId =
      this.embedded && this.requestIdInput != null && this.requestIdInput > 0 ? this.requestIdInput : null;

    this.statusOptions = [
      { label: this.translate.instant('employeeRequests.status.draft'), value: EmployeeRequestStatus.Draft },
      { label: this.translate.instant('employeeRequests.status.submitted'), value: EmployeeRequestStatus.Submitted },
      { label: this.translate.instant('employeeRequests.status.inApproval'), value: EmployeeRequestStatus.InApproval },
      { label: this.translate.instant('employeeRequests.status.approved'), value: EmployeeRequestStatus.Approved },
      { label: this.translate.instant('employeeRequests.status.rejected'), value: EmployeeRequestStatus.Rejected },
      { label: this.translate.instant('employeeRequests.status.inExecution'), value: EmployeeRequestStatus.InExecution },
      { label: this.translate.instant('employeeRequests.status.completed'), value: EmployeeRequestStatus.Completed },
      { label: this.translate.instant('employeeRequests.status.cancelled'), value: EmployeeRequestStatus.Cancelled },
    ];

    this.executionStatusOptions = [
      { label: this.translate.instant('employeeRequests.execStatus.pending'), value: RequestExecutionStatus.Pending },
      { label: this.translate.instant('employeeRequests.execStatus.inProgress'), value: RequestExecutionStatus.InProgress },
      {
        label: this.translate.instant('employeeRequests.execStatus.waitingExternal'),
        value: RequestExecutionStatus.WaitingExternal,
      },
      { label: this.translate.instant('employeeRequests.execStatus.completed'), value: RequestExecutionStatus.Completed },
      { label: this.translate.instant('employeeRequests.execStatus.blocked'), value: RequestExecutionStatus.Blocked },
      { label: this.translate.instant('employeeRequests.execStatus.cancelled'), value: RequestExecutionStatus.Cancelled },
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

    const today = new Date();
    this.summaryDateStr = `${today.getFullYear()}-${String(today.getMonth() + 1).padStart(2, '0')}-${String(today.getDate()).padStart(2, '0')}`;

    if (this.requestId) {
      this.loadRequest();
    } else {
      this.loadEmployees();
      this.loadTypes();
    }
  }

  onSchoolChange(): void {
    this.loadEmployees();
    this.loadTypes();
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
    return [n.firstName, n.middleName, n.lastName].filter(Boolean).join(' ');
  }

  private loadTypes(): void {
    const sid = this.schoolID;
    if (sid == null || sid <= 0) {
      this.typeCatalog = [];
      this.typeOptions = [];
      return;
    }
    this.svc.listTypes(sid).subscribe({
      next: (rows) => {
        this.typeCatalog = rows ?? [];
        this.typeOptions = (this.typeCatalog ?? [])
          .filter((t) => t.isActive)
          .map((t) => ({
            label: `${t.nameAr?.trim() || t.name} — ${t.code} (${this.categoryLabel(t.category)})`,
            value: t.requestTypeID,
          }));
      },
      error: () => {
        this.typeCatalog = [];
        this.typeOptions = [];
      },
    });
  }

  private loadRequest(): void {
    if (!this.requestId) return;
    this.loading = true;
    this.svc
      .getById(this.requestId)
      .pipe(finalize(() => (this.loading = false)))
      .subscribe({
        next: (d) => {
          this.detail = d;
          this.schoolID = d.schoolID;
          this.employeeProfileID = d.employeeProfileID;
          this.requestTypeID = d.requestTypeID;
          this.title = d.title;
          this.details = d.details ?? '';
          this.requestedAmount = d.requestedAmount ?? null;
          this.status = d.status as EmployeeRequestStatus;
          this.loadEmployees();
          this.loadTypes();
          this.resetSubForms();
        },
        error: (e) => {
          this.toastr.error(readEmployeeRequestHttpError(e));
          if (this.embedded) this.closed.emit();
        },
      });
  }

  private resetSubForms(): void {
    this.newApprovalApproverId = null;
    this.newApprovalStepOrder = null;
    this.newExecStatus = RequestExecutionStatus.InProgress;
    this.newExecNotes = '';
    this.newExecProgress = 0;
    this.newExecDue = '';
    this.newExecResponsibleId = null;
    this.newSummaryText = '';
    this.newSummaryProgress = null;
    this.newSummaryFinal = false;
    this.newSummaryAuthorId = null;
    const today = new Date();
    this.summaryDateStr = `${today.getFullYear()}-${String(today.getMonth() + 1).padStart(2, '0')}-${String(today.getDate()).padStart(2, '0')}`;
  }

  save(): void {
    const sid = this.schoolID;
    const emp = this.employeeProfileID;
    const rt = this.requestTypeID;
    const t = this.title.trim();
    if (sid == null || sid <= 0 || emp == null || emp <= 0 || rt == null || rt <= 0 || !t) {
      this.toastr.warning(this.translate.instant('employeeRequests.form.validationCore'));
      return;
    }

    const dto: EmployeeRequestWriteDto = {
      schoolID: sid,
      employeeProfileID: emp,
      requestTypeID: rt,
      title: t,
      details: this.details.trim() || null,
      requestedAmount: this.requestedAmount != null && Number.isFinite(this.requestedAmount) ? this.requestedAmount : null,
      status: this.status,
    };

    this.saving = true;
    const req$ = this.isEdit && this.requestId ? this.svc.update(this.requestId, dto) : this.svc.create(dto);
    req$.pipe(finalize(() => (this.saving = false))).subscribe({
      next: () => {
        this.toastr.success(
          this.translate.instant(this.isEdit ? 'employeeRequests.toast.updated' : 'employeeRequests.toast.created'),
        );
        if (this.embedded) this.saved.emit();
      },
      error: (e) => this.toastr.error(readEmployeeRequestHttpError(e)),
    });
  }

  submitApprovalStep(): void {
    if (!this.isEdit || !this.requestId) return;
    const aid = this.newApprovalApproverId;
    if (aid == null || aid <= 0) {
      this.toastr.warning(this.translate.instant('employeeRequests.form.validationApprover'));
      return;
    }
    const order = this.newApprovalStepOrder != null && this.newApprovalStepOrder > 0 ? this.newApprovalStepOrder : 0;
    this.submittingApproval = true;
    this.svc
      .addApprovalStep(this.requestId, { approverEmployeeProfileID: aid, stepOrder: order })
      .pipe(finalize(() => (this.submittingApproval = false)))
      .subscribe({
        next: () => {
          this.toastr.success(this.translate.instant('employeeRequests.toast.approvalStepAdded'));
          this.newApprovalApproverId = null;
          this.newApprovalStepOrder = null;
          this.loadRequest();
        },
        error: (e) => this.toastr.error(readEmployeeRequestHttpError(e)),
      });
  }

  decideStep(stepId: number, decision: RequestApprovalDecision): void {
    if (!this.isEdit || !this.requestId) return;
    this.svc
      .decideApprovalStep(this.requestId, stepId, { decision, comment: null })
      .subscribe({
        next: () => {
          this.toastr.success(this.translate.instant('employeeRequests.toast.approvalDecided'));
          this.loadRequest();
        },
        error: (e) => this.toastr.error(readEmployeeRequestHttpError(e)),
      });
  }

  submitExecution(): void {
    if (!this.isEdit || !this.requestId) return;
    const progress = Math.max(0, Math.min(100, Math.round(Number(this.newExecProgress) || 0)));
    this.submittingExec = true;
    this.svc
      .addExecution(this.requestId, {
        status: this.newExecStatus,
        notes: this.newExecNotes.trim() || null,
        progressPercent: progress,
        dueAtUtc: this.newExecDue.trim() ? new Date(this.newExecDue).toISOString() : null,
        responsibleEmployeeProfileID:
          this.newExecResponsibleId != null && this.newExecResponsibleId > 0 ? this.newExecResponsibleId : null,
      })
      .pipe(finalize(() => (this.submittingExec = false)))
      .subscribe({
        next: () => {
          this.toastr.success(this.translate.instant('employeeRequests.toast.executionAdded'));
          this.newExecNotes = '';
          this.newExecProgress = 0;
          this.newExecDue = '';
          this.loadRequest();
        },
        error: (e) => this.toastr.error(readEmployeeRequestHttpError(e)),
      });
  }

  submitSummary(): void {
    if (!this.isEdit || !this.requestId) return;
    const text = this.newSummaryText.trim();
    if (!text) {
      this.toastr.warning(this.translate.instant('employeeRequests.form.validationSummary'));
      return;
    }
    const d = this.summaryDateStr.trim();
    if (!d) {
      this.toastr.warning(this.translate.instant('employeeRequests.form.validationSummaryDate'));
      return;
    }
    this.submittingSummary = true;
    this.svc
      .addDailySummary(this.requestId, {
        summaryDate: new Date(d + 'T12:00:00').toISOString(),
        summary: text,
        progressPercent:
          this.newSummaryProgress != null && this.newSummaryProgress >= 0 ? this.newSummaryProgress : null,
        isFinalForDay: this.newSummaryFinal,
        createdByEmployeeProfileID:
          this.newSummaryAuthorId != null && this.newSummaryAuthorId > 0 ? this.newSummaryAuthorId : null,
      })
      .pipe(finalize(() => (this.submittingSummary = false)))
      .subscribe({
        next: () => {
          this.toastr.success(this.translate.instant('employeeRequests.toast.summaryAdded'));
          this.newSummaryText = '';
          this.newSummaryProgress = null;
          this.newSummaryFinal = false;
          this.loadRequest();
        },
        error: (e) => this.toastr.error(readEmployeeRequestHttpError(e)),
      });
  }

  cancel(): void {
    if (this.embedded) this.closed.emit();
  }

  readonly EmployeeRequestStatus = EmployeeRequestStatus;
  readonly RequestApprovalDecision = RequestApprovalDecision;
}
