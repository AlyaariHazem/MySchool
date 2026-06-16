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
  AchievementApprovalDecision,
  AchievementRequestDetailDto,
  AchievementRequestStatus,
  AchievementRequestWriteDto,
  displayAchievementTitle,
} from '../achievements.models';
import { AchievementsService, readAchievementHttpError } from '../achievements.service';

@Component({
  selector: 'app-achievements-form',
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
  templateUrl: './achievements-form.component.html',
  styleUrl: './achievements-form.component.scss',
})
export class AchievementsFormComponent implements OnInit {
  @Input() embedded = false;
  @Input() requestIdInput: number | null = null;
  @Input() presetSchoolId: number | null = null;

  @Output() closed = new EventEmitter<void>();
  @Output() saved = new EventEmitter<void>();

  private readonly svc = inject(AchievementsService);
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
  requestId: number | null = null;

  schoolID: number | null = null;
  employeeProfileID: number | null = null;
  /** 0 = custom title; &gt; 0 = catalog achievement id. */
  catalogOrCustom: number = 0;
  customTitle = '';
  notes = '';
  status: AchievementRequestStatus = AchievementRequestStatus.Draft;

  schoolOptions: { label: string; value: number }[] = [];
  employeeOptions: { label: string; value: number }[] = [];
  catalogOptions: { label: string; value: number }[] = [];
  statusOptions: { label: string; value: number }[] = [];

  detail: AchievementRequestDetailDto | null = null;

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

  get displayTitle(): string {
    if (!this.detail) return '';
    return displayAchievementTitle(this.detail) || '—';
  }

  ngOnInit(): void {
    this.requestId = this.embedded
      ? this.requestIdInput != null && this.requestIdInput > 0
        ? this.requestIdInput
        : null
      : Number(this.route.snapshot.paramMap.get('id')) || null;

    if (!this.embedded) {
      if (this.requestId && !this.perm.hasPermission(PagePermission.Employees.Update)) {
        this.router.navigate(['/school/achievements']).catch(() => undefined);
        return;
      }
      if (!this.requestId && !this.perm.hasPermission(PagePermission.Employees.Create)) {
        this.router.navigate(['/school/achievements']).catch(() => undefined);
        return;
      }
    }

    this.statusOptions = [
      { label: this.translate.instant('achievementRequests.status.draft'), value: AchievementRequestStatus.Draft },
      { label: this.translate.instant('achievementRequests.status.submitted'), value: AchievementRequestStatus.Submitted },
      { label: this.translate.instant('achievementRequests.status.inReview'), value: AchievementRequestStatus.InReview },
      { label: this.translate.instant('achievementRequests.status.approved'), value: AchievementRequestStatus.Approved },
      { label: this.translate.instant('achievementRequests.status.rejected'), value: AchievementRequestStatus.Rejected },
      { label: this.translate.instant('achievementRequests.status.cancelled'), value: AchievementRequestStatus.Cancelled },
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

    if (this.requestId) {
      this.loadRequest();
    } else {
      this.loadEmployees();
      this.loadCatalog();
    }
  }

  onSchoolChange(): void {
    this.loadEmployees();
    this.loadCatalog();
  }

  onCatalogKindChange(): void {
    if (this.catalogOrCustom > 0) this.customTitle = '';
  }

  approvalDecisionLabel(v: number): string {
    const m: Record<number, string> = {
      [AchievementApprovalDecision.Pending]: 'achievementRequests.approval.pending',
      [AchievementApprovalDecision.Approved]: 'achievementRequests.approval.approved',
      [AchievementApprovalDecision.Rejected]: 'achievementRequests.approval.rejected',
      [AchievementApprovalDecision.Skipped]: 'achievementRequests.approval.skipped',
    };
    const key = m[v];
    return key ? this.translate.instant(key) : String(v);
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

  private loadCatalog(): void {
    const sid = this.schoolID;
    if (sid == null || sid <= 0) {
      this.catalogOptions = [{ label: this.translate.instant('achievementRequests.form.customTitleOption'), value: 0 }];
      return;
    }
    this.svc
      .catalog({ schoolID: sid })
      .pipe(
        map((items) => {
          const custom = { label: this.translate.instant('achievementRequests.form.customTitleOption'), value: 0 };
          const rest = (items ?? []).map((c) => ({
            label: c.code ? `${c.code} — ${c.title}` : c.title,
            value: c.achievementID,
          }));
          return [custom, ...rest];
        }),
        catchError(() =>
          of([{ label: this.translate.instant('achievementRequests.form.customTitleOption'), value: 0 }] as {
            label: string;
            value: number;
          }[]),
        ),
      )
      .subscribe((opts) => {
        this.catalogOptions = opts;
        this.ensureCatalogChoiceInOptions();
      });
  }

  private ensureCatalogChoiceInOptions(): void {
    if (this.catalogOrCustom <= 0) return;
    if (this.catalogOptions.some((o) => o.value === this.catalogOrCustom)) return;
    const t = this.detail ? displayAchievementTitle(this.detail) : `#${this.catalogOrCustom}`;
    this.catalogOptions = [{ label: t || `#${this.catalogOrCustom}`, value: this.catalogOrCustom }, ...this.catalogOptions];
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
          this.notes = d.notes ?? '';
          this.status = d.status as AchievementRequestStatus;
          if (d.achievementID != null && d.achievementID > 0) {
            this.catalogOrCustom = d.achievementID;
            this.customTitle = '';
          } else {
            this.catalogOrCustom = 0;
            this.customTitle = d.customTitle ?? '';
          }
          this.loadEmployees();
          this.loadCatalog();
        },
        error: (e) => {
          this.toastr.error(readAchievementHttpError(e));
          if (this.embedded) this.closed.emit();
          else this.router.navigate(['/school/achievements']).catch(() => undefined);
        },
      });
  }

  save(): void {
    const sid = this.schoolID;
    const eid = this.employeeProfileID;
    if (sid == null || sid <= 0 || eid == null || eid <= 0) {
      this.toastr.warning(this.translate.instant('achievementRequests.form.validationCore'));
      return;
    }
    const useCatalog = this.catalogOrCustom > 0;
    const ct = this.customTitle.trim();
    if (!useCatalog && !ct) {
      this.toastr.warning(this.translate.instant('achievementRequests.form.validationCustom'));
      return;
    }

    const dto: AchievementRequestWriteDto = {
      schoolID: sid,
      employeeProfileID: eid,
      achievementID: useCatalog ? this.catalogOrCustom : null,
      customTitle: useCatalog ? null : ct,
      notes: this.notes.trim() || null,
      status: this.status,
    };

    this.saving = true;
    const req$ = this.isEdit && this.requestId
      ? this.svc.update(this.requestId, dto)
      : this.svc.create(dto);
    req$.pipe(finalize(() => (this.saving = false))).subscribe({
      next: () => {
        this.toastr.success(
          this.translate.instant(this.isEdit ? 'achievementRequests.toast.updated' : 'achievementRequests.toast.created'),
        );
        if (this.embedded) this.saved.emit();
        else this.router.navigate(['/school/achievements']).catch(() => undefined);
      },
      error: (e) => this.toastr.error(readAchievementHttpError(e)),
    });
  }

  cancel(): void {
    if (this.embedded) this.closed.emit();
    else this.router.navigate(['/school/achievements']).catch(() => undefined);
  }
}
