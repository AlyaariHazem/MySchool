import { DatePipe, NgIf } from '@angular/common';
import { Component, EventEmitter, Input, OnInit, Output, inject } from '@angular/core';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { ButtonModule } from 'primeng/button';
import { FloatLabelModule } from 'primeng/floatlabel';
import { Select } from 'primeng/select';
import { TableModule } from 'primeng/table';
import { TextareaModule } from 'primeng/textarea';
import { InputTextModule } from 'primeng/inputtext';
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
  ViolationActionCategory,
  ViolationActionWriteDto,
  ViolationDetailDto,
  ViolationEscalateDto,
  ViolationKind,
  ViolationResponseWriteDto,
  ViolationStatus,
  ViolationTypeListItemDto,
  ViolationWriteDto,
} from '../violations.models';
import { ViolationsService, readViolationHttpError } from '../violations.service';

@Component({
  selector: 'app-violations-form',
  standalone: true,
  imports: [
    ShardModule,
    NgIf,
    DatePipe,
    FormsModule,
    TranslateModule,
    RouterLink,
    ButtonModule,
    Select,
    FloatLabelModule,
    InputTextModule,
    TextareaModule,
    TableModule,
  ],
  templateUrl: './violations-form.component.html',
  styleUrl: './violations-form.component.scss',
})
export class ViolationsFormComponent implements OnInit {
  @Input() embedded = false;
  @Input() violationIdInput: number | null = null;
  @Input() presetSchoolId: number | null = null;

  @Output() closed = new EventEmitter<void>();
  @Output() saved = new EventEmitter<void>();

  private readonly svc = inject(ViolationsService);
  private readonly employeesHr = inject(EmployeesHrService);
  private readonly schoolService = inject(SchoolService);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly toastr = inject(ToastrService);
  private readonly translate = inject(TranslateService);
  private readonly perm = inject(PermissionService);

  readonly filterSelectPanelStyle: Record<string, string> = {
    maxWidth: 'min(22rem, calc(100vw - 2rem))',
  };

  loading = false;
  saving = false;
  violationId: number | null = null;

  schoolID: number | null = null;
  subjectEmployeeProfileID: number | null = null;
  openedByEmployeeProfileID: number | null = null;
  /** 0 = server default (Clarification). */
  initialViolationTypeID = 0;
  title = '';
  details = '';
  status: ViolationStatus = ViolationStatus.Draft;

  schoolOptions: { label: string; value: number }[] = [];
  employeeOptions: { label: string; value: number }[] = [];
  statusOptions: { label: string; value: number }[] = [];
  initialTypeOptions: { label: string; value: number }[] = [];
  actionCategoryOptions: { label: string; value: number }[] = [];

  typeCatalog: ViolationTypeListItemDto[] = [];

  detail: ViolationDetailDto | null = null;

  newResponseBody = '';
  newResponseAuthorId: number | null = null;

  newActionCategory = ViolationActionCategory.GeneralNote;
  newActionTitle = '';
  newActionNotes = '';
  newActionPerformedById: number | null = null;

  escalateNewTypeId: number | null = null;
  escalateChangedById: number | null = null;
  escalateReason = '';

  submittingResponse = false;
  submittingAction = false;
  submittingEscalate = false;

  get isSchoolManager(): boolean {
    return isSchoolManagerUser();
  }

  get isEdit(): boolean {
    return this.violationId != null && this.violationId > 0;
  }

  get canSubmit(): boolean {
    return this.isEdit
      ? this.perm.hasPermission(PagePermission.Employees.Update)
      : this.perm.hasPermission(PagePermission.Employees.Create);
  }

  get escalateTypeOptions(): { label: string; value: number }[] {
    const d = this.detail;
    if (!d) return [];
    const k = d.violationTypeKind;
    return this.typeCatalog
      .filter((t) => t.isActive && t.kind > k)
      .map((t) => ({ label: `${t.name} (${this.kindLabel(t.kind)})`, value: t.violationTypeID }));
  }

  get canEscalateFurther(): boolean {
    return this.escalateTypeOptions.length > 0;
  }

  kindLabel(kind: number): string {
    const m: Record<number, string> = {
      [ViolationKind.Clarification]: 'violations.kind.clarification',
      [ViolationKind.WrittenWarning]: 'violations.kind.writtenWarning',
      [ViolationKind.AttentionNotice]: 'violations.kind.attentionNotice',
      [ViolationKind.FinalWarning]: 'violations.kind.finalWarning',
    };
    const key = m[kind];
    return key ? this.translate.instant(key) : String(kind);
  }

  actionCategoryLabel(v: number): string {
    const m: Record<number, string> = {
      [ViolationActionCategory.GeneralNote]: 'violations.actionCategory.generalNote',
      [ViolationActionCategory.MeetingHeld]: 'violations.actionCategory.meetingHeld',
      [ViolationActionCategory.FormalDocumentation]: 'violations.actionCategory.formalDocumentation',
      [ViolationActionCategory.Other]: 'violations.actionCategory.other',
    };
    const key = m[v];
    return key ? this.translate.instant(key) : String(v);
  }

  ngOnInit(): void {
    this.violationId = this.embedded
      ? this.violationIdInput != null && this.violationIdInput > 0
        ? this.violationIdInput
        : null
      : Number(this.route.snapshot.paramMap.get('id')) || null;

    if (!this.embedded) {
      if (this.violationId && !this.perm.hasPermission(PagePermission.Employees.Update)) {
        this.router.navigate(['/school/violations']).catch(() => undefined);
        return;
      }
      if (!this.violationId && !this.perm.hasPermission(PagePermission.Employees.Create)) {
        this.router.navigate(['/school/violations']).catch(() => undefined);
        return;
      }
    }

    this.statusOptions = [
      { label: this.translate.instant('violations.status.draft'), value: ViolationStatus.Draft },
      { label: this.translate.instant('violations.status.open'), value: ViolationStatus.Open },
      { label: this.translate.instant('violations.status.inProgress'), value: ViolationStatus.InProgress },
      { label: this.translate.instant('violations.status.resolved'), value: ViolationStatus.Resolved },
      { label: this.translate.instant('violations.status.closed'), value: ViolationStatus.Closed },
      { label: this.translate.instant('violations.status.cancelled'), value: ViolationStatus.Cancelled },
    ];

    this.actionCategoryOptions = [
      { label: this.translate.instant('violations.actionCategory.generalNote'), value: ViolationActionCategory.GeneralNote },
      { label: this.translate.instant('violations.actionCategory.meetingHeld'), value: ViolationActionCategory.MeetingHeld },
      {
        label: this.translate.instant('violations.actionCategory.formalDocumentation'),
        value: ViolationActionCategory.FormalDocumentation,
      },
      { label: this.translate.instant('violations.actionCategory.other'), value: ViolationActionCategory.Other },
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

    if (this.violationId) {
      this.loadRequest();
    } else {
      this.rebuildInitialTypeOptions();
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
      this.rebuildInitialTypeOptions();
      return;
    }
    this.svc.listTypes(sid).subscribe({
      next: (rows) => {
        this.typeCatalog = rows ?? [];
        this.rebuildInitialTypeOptions();
      },
      error: () => {
        this.typeCatalog = [];
        this.rebuildInitialTypeOptions();
      },
    });
  }

  private rebuildInitialTypeOptions(): void {
    const def = { label: this.translate.instant('violations.form.initialTypeDefault'), value: 0 };
    const rest = (this.typeCatalog ?? [])
      .filter((t) => t.isActive)
      .map((t) => ({ label: `${t.name} (${this.kindLabel(t.kind)})`, value: t.violationTypeID }));
    this.initialTypeOptions = [def, ...rest];
  }

  private loadRequest(): void {
    if (!this.violationId) return;
    this.loading = true;
    this.svc
      .getById(this.violationId)
      .pipe(finalize(() => (this.loading = false)))
      .subscribe({
        next: (d) => {
          this.detail = d;
          this.schoolID = d.schoolID;
          this.subjectEmployeeProfileID = d.subjectEmployeeProfileID;
          this.openedByEmployeeProfileID = d.openedByEmployeeProfileID ?? null;
          this.title = d.title;
          this.details = d.details ?? '';
          this.status = d.status as ViolationStatus;
          this.initialViolationTypeID = d.violationTypeID;
          this.loadEmployees();
          this.loadTypes();
          this.resetSubForms();
        },
        error: (e) => {
          this.toastr.error(readViolationHttpError(e));
          if (this.embedded) this.closed.emit();
          else this.router.navigate(['/school/violations']).catch(() => undefined);
        },
      });
  }

  private resetSubForms(): void {
    this.newResponseBody = '';
    this.newResponseAuthorId = null;
    this.newActionCategory = ViolationActionCategory.GeneralNote;
    this.newActionTitle = '';
    this.newActionNotes = '';
    this.newActionPerformedById = null;
    this.escalateNewTypeId = null;
    this.escalateChangedById = null;
    this.escalateReason = '';
  }

  save(): void {
    const sid = this.schoolID;
    const sub = this.subjectEmployeeProfileID;
    const t = this.title.trim();
    if (sid == null || sid <= 0 || sub == null || sub <= 0 || !t) {
      this.toastr.warning(this.translate.instant('violations.form.validationCore'));
      return;
    }

    const dto: ViolationWriteDto = {
      schoolID: sid,
      subjectEmployeeProfileID: sub,
      openedByEmployeeProfileID: this.openedByEmployeeProfileIdOrNull(),
      violationTypeID: this.isEdit ? 0 : this.initialViolationTypeID,
      title: t,
      details: this.details.trim() || null,
      status: this.status,
    };

    this.saving = true;
    const req$ = this.isEdit && this.violationId ? this.svc.update(this.violationId, dto) : this.svc.create(dto);
    req$.pipe(finalize(() => (this.saving = false))).subscribe({
      next: () => {
        this.toastr.success(
          this.translate.instant(this.isEdit ? 'violations.toast.updated' : 'violations.toast.created'),
        );
        if (this.embedded) this.saved.emit();
        else this.router.navigate(['/school/violations']).catch(() => undefined);
      },
      error: (e) => this.toastr.error(readViolationHttpError(e)),
    });
  }

  private openedByEmployeeProfileIdOrNull(): number | null | undefined {
    const v = this.openedByEmployeeProfileID;
    if (v == null || v <= 0) return null;
    return v;
  }

  submitResponse(): void {
    if (!this.isEdit || !this.violationId) return;
    const body = this.newResponseBody.trim();
    if (!body) {
      this.toastr.warning(this.translate.instant('violations.form.validationResponseBody'));
      return;
    }
    const dto: ViolationResponseWriteDto = {
      body,
      authorEmployeeProfileID: this.newResponseAuthorId != null && this.newResponseAuthorId > 0 ? this.newResponseAuthorId : null,
    };
    this.submittingResponse = true;
    this.svc
      .addResponse(this.violationId, dto)
      .pipe(finalize(() => (this.submittingResponse = false)))
      .subscribe({
        next: () => {
          this.toastr.success(this.translate.instant('violations.toast.responseAdded'));
          this.newResponseBody = '';
          this.newResponseAuthorId = null;
          this.loadRequest();
        },
        error: (e) => this.toastr.error(readViolationHttpError(e)),
      });
  }

  submitAction(): void {
    if (!this.isEdit || !this.violationId) return;
    const title = this.newActionTitle.trim();
    const perf = this.newActionPerformedById;
    if (!title || perf == null || perf <= 0) {
      this.toastr.warning(this.translate.instant('violations.form.validationAction'));
      return;
    }
    const dto: ViolationActionWriteDto = {
      category: this.newActionCategory,
      title,
      notes: this.newActionNotes.trim() || null,
      performedByEmployeeProfileID: perf,
    };
    this.submittingAction = true;
    this.svc
      .addAction(this.violationId, dto)
      .pipe(finalize(() => (this.submittingAction = false)))
      .subscribe({
        next: () => {
          this.toastr.success(this.translate.instant('violations.toast.actionAdded'));
          this.newActionTitle = '';
          this.newActionNotes = '';
          this.newActionPerformedById = null;
          this.loadRequest();
        },
        error: (e) => this.toastr.error(readViolationHttpError(e)),
      });
  }

  submitEscalate(): void {
    if (!this.isEdit || !this.violationId) return;
    const tid = this.escalateNewTypeId;
    const by = this.escalateChangedById;
    if (tid == null || tid <= 0 || by == null || by <= 0) {
      this.toastr.warning(this.translate.instant('violations.form.validationEscalate'));
      return;
    }
    const dto: ViolationEscalateDto = {
      newViolationTypeID: tid,
      changedByEmployeeProfileID: by,
      reason: this.escalateReason.trim() || null,
    };
    this.submittingEscalate = true;
    this.svc
      .escalate(this.violationId, dto)
      .pipe(finalize(() => (this.submittingEscalate = false)))
      .subscribe({
        next: () => {
          this.toastr.success(this.translate.instant('violations.toast.escalated'));
          this.resetSubForms();
          this.loadRequest();
        },
        error: (e) => this.toastr.error(readViolationHttpError(e)),
      });
  }

  cancel(): void {
    if (this.embedded) this.closed.emit();
    else this.router.navigate(['/school/violations']).catch(() => undefined);
  }
}
