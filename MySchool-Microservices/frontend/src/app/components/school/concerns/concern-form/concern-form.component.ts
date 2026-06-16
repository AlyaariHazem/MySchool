import { DatePipe, NgIf } from '@angular/common';
import { Component, EventEmitter, Input, OnInit, Output, inject } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { ButtonModule } from 'primeng/button';
import { FloatLabelModule } from 'primeng/floatlabel';
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
  ComplaintDetailDto,
  ComplaintWriteDto,
  ConcernCategoryKind,
  ConcernCategoryListItemDto,
  ConcernKind,
  ConcernStatus,
  SuggestionDetailDto,
  SuggestionWriteDto,
} from '../concerns.models';
import { ConcernsService, readConcernHttpError } from '../concerns.service';

@Component({
  selector: 'app-concern-form',
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
    TableModule,
  ],
  templateUrl: './concern-form.component.html',
  styleUrl: './concern-form.component.scss',
})
export class ConcernFormComponent implements OnInit {
  @Input({ required: true }) mode!: ConcernKind;
  @Input() embedded = false;
  @Input() recordIdInput: number | null = null;
  @Input() presetSchoolId: number | null = null;

  @Output() closed = new EventEmitter<void>();
  @Output() saved = new EventEmitter<void>();

  private readonly svc = inject(ConcernsService);
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
  recordId: number | null = null;

  schoolID: number | null = null;
  concernCategoryID: number | null = null;
  submitterEmployeeProfileID: number | null = null;
  assignedToEmployeeProfileID: number | null = null;
  title = '';
  details = '';
  status: ConcernStatus = ConcernStatus.Draft;

  schoolOptions: { label: string; value: number }[] = [];
  employeeOptions: { label: string; value: number }[] = [];
  categoryOptions: { label: string; value: number }[] = [];
  categoryCatalog: ConcernCategoryListItemDto[] = [];
  statusOptions: { label: string; value: number }[] = [];

  complaintDetail: ComplaintDetailDto | null = null;
  suggestionDetail: SuggestionDetailDto | null = null;

  get isSchoolManager(): boolean {
    return isSchoolManagerUser();
  }

  get isEdit(): boolean {
    return this.recordId != null && this.recordId > 0;
  }

  get canSubmit(): boolean {
    return this.isEdit
      ? this.perm.hasPermission(PagePermission.Employees.Update)
      : this.perm.hasPermission(PagePermission.Employees.Create);
  }

  get detailActionLogs() {
    return this.mode === 'complaint' ? this.complaintDetail?.actionLogs ?? [] : this.suggestionDetail?.actionLogs ?? [];
  }

  ngOnInit(): void {
    this.recordId =
      this.embedded && this.recordIdInput != null && this.recordIdInput > 0 ? this.recordIdInput : null;

    this.statusOptions = [
      { label: this.translate.instant('concerns.status.draft'), value: ConcernStatus.Draft },
      { label: this.translate.instant('concerns.status.submitted'), value: ConcernStatus.Submitted },
      { label: this.translate.instant('concerns.status.underReview'), value: ConcernStatus.UnderReview },
      { label: this.translate.instant('concerns.status.inProgress'), value: ConcernStatus.InProgress },
      { label: this.translate.instant('concerns.status.resolved'), value: ConcernStatus.Resolved },
      { label: this.translate.instant('concerns.status.rejected'), value: ConcernStatus.Rejected },
      { label: this.translate.instant('concerns.status.closed'), value: ConcernStatus.Closed },
      { label: this.translate.instant('concerns.status.cancelled'), value: ConcernStatus.Cancelled },
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
      this.loadEmployees();
      this.loadCategories();
    }
  }

  onSchoolChange(): void {
    this.loadEmployees();
    this.loadCategories();
  }

  private applyDefaultSchoolId(): void {
    if (this.schoolID != null && this.schoolID > 0) return;
    if (typeof localStorage === 'undefined') return;
    const raw = localStorage.getItem('schoolId');
    const n = raw != null && raw !== '' ? Number(raw) : NaN;
    if (Number.isFinite(n) && n > 0) this.schoolID = n;
  }

  private categoryKindFilter(): number {
    return this.mode === 'complaint' ? ConcernCategoryKind.Complaint : ConcernCategoryKind.Suggestion;
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

  private loadCategories(): void {
    const sid = this.schoolID;
    if (sid == null || sid <= 0) {
      this.categoryCatalog = [];
      this.categoryOptions = [];
      return;
    }
    this.svc.listCategories(sid, this.categoryKindFilter()).subscribe({
      next: (rows) => {
        this.categoryCatalog = rows ?? [];
        this.categoryOptions = (this.categoryCatalog ?? [])
          .filter((t) => t.isActive)
          .map((t) => ({
            label: `${t.nameAr?.trim() || t.name} — ${t.code}`,
            value: t.concernCategoryID,
          }));
      },
      error: () => {
        this.categoryCatalog = [];
        this.categoryOptions = [];
      },
    });
  }

  private loadRecord(): void {
    if (!this.recordId) return;
    this.loading = true;
    const done = () => (this.loading = false);

    if (this.mode === 'complaint') {
      this.svc
        .getComplaint(this.recordId)
        .pipe(finalize(done))
        .subscribe({
          next: (c: ComplaintDetailDto) => {
            this.complaintDetail = c;
            this.suggestionDetail = null;
            this.schoolID = c.schoolID;
            this.concernCategoryID = c.concernCategoryID;
            this.submitterEmployeeProfileID = c.submitterEmployeeProfileID;
            this.assignedToEmployeeProfileID = c.assignedToEmployeeProfileID ?? null;
            this.title = c.title;
            this.details = c.details ?? '';
            this.status = c.status as ConcernStatus;
            this.loadEmployees();
            this.loadCategories();
          },
          error: (e: unknown) => {
            this.toastr.error(readConcernHttpError(e));
            if (this.embedded) this.closed.emit();
          },
        });
    } else {
      this.svc
        .getSuggestion(this.recordId)
        .pipe(finalize(done))
        .subscribe({
          next: (s: SuggestionDetailDto) => {
            this.suggestionDetail = s;
            this.complaintDetail = null;
            this.schoolID = s.schoolID;
            this.concernCategoryID = s.concernCategoryID;
            this.submitterEmployeeProfileID = s.submitterEmployeeProfileID;
            this.assignedToEmployeeProfileID = s.assignedToEmployeeProfileID ?? null;
            this.title = s.title;
            this.details = s.details ?? '';
            this.status = s.status as ConcernStatus;
            this.loadEmployees();
            this.loadCategories();
          },
          error: (e: unknown) => {
            this.toastr.error(readConcernHttpError(e));
            if (this.embedded) this.closed.emit();
          },
        });
    }
  }

  save(): void {
    const sid = this.schoolID;
    const cat = this.concernCategoryID;
    const sub = this.submitterEmployeeProfileID;
    const t = this.title.trim();
    if (sid == null || sid <= 0 || cat == null || cat <= 0 || sub == null || sub <= 0 || !t) {
      this.toastr.warning(this.translate.instant('concerns.form.validationCore'));
      return;
    }

    this.saving = true;
    if (this.mode === 'complaint') {
      const dto: ComplaintWriteDto = {
        schoolID: sid,
        concernCategoryID: cat,
        submitterEmployeeProfileID: sub,
        title: t,
        details: this.details.trim() || null,
        status: this.status,
        assignedToEmployeeProfileID:
          this.assignedToEmployeeProfileID != null && this.assignedToEmployeeProfileID > 0
            ? this.assignedToEmployeeProfileID
            : null,
      };
      const req$ = this.isEdit && this.recordId ? this.svc.updateComplaint(this.recordId, dto) : this.svc.createComplaint(dto);
      req$.pipe(finalize(() => (this.saving = false))).subscribe({
        next: () => {
          this.toastr.success(
            this.translate.instant(this.isEdit ? 'concerns.toast.updated' : 'concerns.toast.created'),
          );
          if (this.embedded) this.saved.emit();
        },
        error: (e) => this.toastr.error(readConcernHttpError(e)),
      });
    } else {
      const dto: SuggestionWriteDto = {
        schoolID: sid,
        concernCategoryID: cat,
        submitterEmployeeProfileID: sub,
        title: t,
        details: this.details.trim() || null,
        status: this.status,
        assignedToEmployeeProfileID:
          this.assignedToEmployeeProfileID != null && this.assignedToEmployeeProfileID > 0
            ? this.assignedToEmployeeProfileID
            : null,
      };
      const req$ =
        this.isEdit && this.recordId ? this.svc.updateSuggestion(this.recordId, dto) : this.svc.createSuggestion(dto);
      req$.pipe(finalize(() => (this.saving = false))).subscribe({
        next: () => {
          this.toastr.success(
            this.translate.instant(this.isEdit ? 'concerns.toast.updated' : 'concerns.toast.created'),
          );
          if (this.embedded) this.saved.emit();
        },
        error: (e) => this.toastr.error(readConcernHttpError(e)),
      });
    }
  }

  cancel(): void {
    this.closed.emit();
  }

  actionKindLabel(v: number): string {
    const m: Record<number, string> = {
      0: 'concerns.actionKind.created',
      1: 'concerns.actionKind.statusChanged',
      2: 'concerns.actionKind.noteAdded',
      3: 'concerns.actionKind.assigned',
      4: 'concerns.actionKind.resolved',
      5: 'concerns.actionKind.closed',
      6: 'concerns.actionKind.rejected',
    };
    const key = m[v];
    return key ? this.translate.instant(key) : String(v);
  }

  statusLabel(v: number | null | undefined): string {
    if (v == null) return '—';
    const m: Record<number, string> = {
      [ConcernStatus.Draft]: 'concerns.status.draft',
      [ConcernStatus.Submitted]: 'concerns.status.submitted',
      [ConcernStatus.UnderReview]: 'concerns.status.underReview',
      [ConcernStatus.InProgress]: 'concerns.status.inProgress',
      [ConcernStatus.Resolved]: 'concerns.status.resolved',
      [ConcernStatus.Rejected]: 'concerns.status.rejected',
      [ConcernStatus.Closed]: 'concerns.status.closed',
      [ConcernStatus.Cancelled]: 'concerns.status.cancelled',
    };
    const key = m[v];
    return key ? this.translate.instant(key) : String(v);
  }
}
