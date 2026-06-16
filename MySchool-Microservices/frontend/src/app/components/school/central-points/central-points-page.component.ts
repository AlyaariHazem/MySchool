import { AsyncPipe, DatePipe, NgIf } from '@angular/common';
import { Component, OnInit, inject } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Store } from '@ngrx/store';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { ButtonModule } from 'primeng/button';
import { CheckboxModule } from 'primeng/checkbox';
import { DialogModule } from 'primeng/dialog';
import { FloatLabelModule } from 'primeng/floatlabel';
import { InputNumberModule } from 'primeng/inputnumber';
import { InputTextModule } from 'primeng/inputtext';
import { Select } from 'primeng/select';
import { TableLazyLoadEvent, TableModule } from 'primeng/table';
import { TabsModule } from 'primeng/tabs';
import { TextareaModule } from 'primeng/textarea';
import { TooltipModule } from 'primeng/tooltip';
import { ToastrService } from 'ngx-toastr';
import { catchError, finalize, map } from 'rxjs/operators';
import { of } from 'rxjs';

import { isSchoolManagerUser } from 'app/core/utils/school-role.util';
import { PagePermission, PermissionService } from 'app/core/services/permission.service';
import { SchoolService } from 'app/core/services/school.service';
import { School } from 'app/core/models/school.modul';
import { selectLanguage } from 'app/core/store/language/language.selectors';
import { ShardModule } from 'app/shared/shard.module';

import { EmployeeProfileOptionDto, EmployeeProfilePageRequestDto } from '../employees-hr/employees-hr.models';
import { EmployeesHrService } from '../employees-hr/employees-hr.service';
import {
  PointsLedgerListItemDto,
  PointsRuleDto,
  PointsRuleWriteDto,
  PostCentralPointsDto,
} from './central-points.models';
import { CentralPointsService, readCentralPointsHttpError } from './central-points.service';

@Component({
  selector: 'app-central-points-page',
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
    InputNumberModule,
    TextareaModule,
    CheckboxModule,
    TooltipModule,
    DialogModule,
  ],
  templateUrl: './central-points-page.component.html',
  styleUrl: './central-points-page.component.scss',
})
export class CentralPointsPageComponent implements OnInit {
  private readonly svc = inject(CentralPointsService);
  private readonly schoolService = inject(SchoolService);
  private readonly employeesHr = inject(EmployeesHrService);
  private readonly toastr = inject(ToastrService);
  private readonly translate = inject(TranslateService);
  private readonly perm = inject(PermissionService);
  private readonly store = inject(Store);

  readonly dir$ = this.store.select(selectLanguage).pipe(map((l) => (l === 'ar' ? 'rtl' : 'ltr')));

  readonly filterSelectPanelStyle: Record<string, string> = {
    maxWidth: 'min(22rem, calc(100vw - 2rem))',
  };

  activeTab = '0';

  schoolOptions: { label: string; value: number }[] = [];
  sourceOptions: { label: string; value: number }[] = [];
  postSourceCodeOptions: { label: string; value: string }[] = [];
  employeeOptionsLedger: { label: string; value: number }[] = [];
  employeeOptionsPost: { label: string; value: number }[] = [];

  get isSchoolManager(): boolean {
    return isSchoolManagerUser();
  }

  get isTenantAdmin(): boolean {
    return typeof localStorage !== 'undefined' && localStorage.getItem('userType') === 'ADMIN';
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

  // --- Ledger ---
  ledgerSchoolId: number | null = null;
  ledgerEmployeeId: number | null = null;
  ledgerSourceId: number | null = null;
  ledgerRows: PointsLedgerListItemDto[] = [];
  ledgerLoading = false;
  ledgerTotal = 0;
  ledgerFirst = 0;
  ledgerPageSize = 25;

  // --- Rules ---
  rulesSchoolId: number | null = null;
  rulesSourceId: number | null = null;
  rulesActiveOnly = true;
  rulesRows: PointsRuleDto[] = [];
  rulesLoading = false;
  ruleDialogVisible = false;
  ruleEditId: number | null = null;
  ruleForm: PointsRuleWriteDto = this.emptyRuleForm();
  ruleSaving = false;

  // --- Post / balance ---
  postSchoolId: number | null = null;
  postEmployeeId: number | null = null;
  postSourceCode: string | null = null;
  postRuleKey = '*';
  postOverrideEnabled = false;
  postOverrideValue: number | null = null;
  postCorrelationType = '';
  postCorrelationId: number | null = null;
  postIdempotency = '';
  postNotes = '';
  postMemo = '';
  postSubmitting = false;
  balanceLoading = false;
  balanceSnapshot: { totalPoints: number; updatedAtUtc: string; academicYearID?: number } | null = null;
  rebuildSubmitting = false;

  ngOnInit(): void {
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
    } else {
      const raw = typeof localStorage !== 'undefined' ? localStorage.getItem('schoolId') : null;
      const n = raw != null && raw !== '' ? Number(raw) : NaN;
      if (Number.isFinite(n) && n > 0) {
        this.ledgerSchoolId = n;
        this.rulesSchoolId = n;
        this.postSchoolId = n;
      }
    }

    if (this.canView) {
      this.svc.listSources().subscribe({
        next: (rows) => {
          const list = rows ?? [];
          this.sourceOptions = list.map((s) => ({
            label: `${s.displayName} (${s.code})`,
            value: s.pointsSourceID,
          }));
          this.postSourceCodeOptions = list.map((s) => ({
            label: `${s.displayName} (${s.code})`,
            value: s.code,
          }));
        },
        error: (e) => this.toastr.error(readCentralPointsHttpError(e)),
      });
      if (this.ledgerSchoolId != null && this.ledgerSchoolId > 0) {
        this.loadLedgerEmployees();
      }
      if (this.postSchoolId != null && this.postSchoolId > 0) {
        this.loadPostEmployees();
      }
    }
  }

  private emptyRuleForm(): PointsRuleWriteDto {
    return {
      schoolID: null,
      pointsSourceID: 0,
      ruleKey: '*',
      deltaPoints: 0,
      priority: 0,
      isActive: true,
    };
  }

  onLedgerSchoolChange(): void {
    this.loadLedgerEmployees();
    this.ledgerFirst = 0;
  }

  onPostSchoolChange(): void {
    this.loadPostEmployees();
  }

  private loadLedgerEmployees(): void {
    const sid = this.ledgerSchoolId;
    if (sid == null || sid <= 0) {
      this.employeeOptionsLedger = [];
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
      .subscribe((opts) => (this.employeeOptionsLedger = opts.filter((x) => x.value > 0)));
  }

  private loadPostEmployees(): void {
    const sid = this.postSchoolId;
    if (sid == null || sid <= 0) {
      this.employeeOptionsPost = [];
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
      .subscribe((opts) => (this.employeeOptionsPost = opts.filter((x) => x.value > 0)));
  }

  applyLedgerFilters(): void {
    this.ledgerFirst = 0;
    this.loadLedger();
  }

  onLedgerLazy(ev: TableLazyLoadEvent): void {
    this.ledgerFirst = ev.first ?? 0;
    const r = ev.rows;
    if (r != null && r > 0) this.ledgerPageSize = r;
    this.loadLedger();
  }

  loadLedger(): void {
    if (!this.canView) return;
    const sid = this.ledgerSchoolId;
    if (!this.isSchoolManager && (sid == null || sid <= 0)) {
      this.toastr.warning(this.translate.instant('centralPoints.validation.school'));
      return;
    }
    this.ledgerLoading = true;
    this.svc
      .listLedger({
        schoolID: sid ?? undefined,
        employeeProfileID: this.ledgerEmployeeId ?? undefined,
        pointsSourceID: this.ledgerSourceId ?? undefined,
        skip: this.ledgerFirst,
        take: this.ledgerPageSize,
      })
      .pipe(finalize(() => (this.ledgerLoading = false)))
      .subscribe({
        next: ({ items, totalCount }) => {
          this.ledgerRows = items ?? [];
          this.ledgerTotal = totalCount ?? 0;
        },
        error: (e) => this.toastr.error(readCentralPointsHttpError(e)),
      });
  }

  loadRules(): void {
    if (!this.canView) return;
    this.rulesLoading = true;
    this.svc
      .listRules({
        schoolID: this.rulesSchoolId ?? undefined,
        pointsSourceID: this.rulesSourceId ?? undefined,
        activeOnly: this.rulesActiveOnly,
      })
      .pipe(finalize(() => (this.rulesLoading = false)))
      .subscribe({
        next: (r) => (this.rulesRows = r ?? []),
        error: (e) => this.toastr.error(readCentralPointsHttpError(e)),
      });
  }

  openRuleCreate(): void {
    this.ruleEditId = null;
    this.ruleForm = this.emptyRuleForm();
    if (this.isSchoolManager && this.rulesSchoolId != null) {
      this.ruleForm.schoolID = this.rulesSchoolId;
    }
    if (this.rulesSourceId != null && this.rulesSourceId > 0) {
      this.ruleForm.pointsSourceID = this.rulesSourceId;
    }
    this.ruleDialogVisible = true;
  }

  openRuleEdit(row: PointsRuleDto): void {
    this.ruleEditId = row.pointsRuleID;
    this.ruleForm = {
      schoolID: row.schoolID ?? null,
      pointsSourceID: row.pointsSourceID,
      ruleKey: row.ruleKey || '*',
      deltaPoints: row.deltaPoints,
      priority: row.priority,
      isActive: row.isActive,
    };
    this.ruleDialogVisible = true;
  }

  closeRuleDialog(): void {
    this.ruleDialogVisible = false;
    this.ruleEditId = null;
  }

  saveRule(): void {
    if (!this.canCreate && this.ruleEditId == null) return;
    if (!this.canUpdate && this.ruleEditId != null) return;
    if (!this.ruleForm.pointsSourceID) {
      this.toastr.warning(this.translate.instant('centralPoints.validation.source'));
      return;
    }
    if (this.isSchoolManager && this.rulesSchoolId != null) {
      this.ruleForm.schoolID = this.rulesSchoolId;
    }
    this.ruleSaving = true;
    const onDone = (): void => {
      this.toastr.success(
        this.translate.instant(this.ruleEditId ? 'centralPoints.toast.ruleUpdated' : 'centralPoints.toast.ruleCreated'),
      );
      this.closeRuleDialog();
      this.loadRules();
    };
    const onErr = (e: unknown): void => {
      this.toastr.error(readCentralPointsHttpError(e));
    };
    if (this.ruleEditId != null && this.ruleEditId > 0) {
      this.svc
        .updateRule(this.ruleEditId, this.ruleForm)
        .pipe(finalize(() => (this.ruleSaving = false)))
        .subscribe({ next: onDone, error: onErr });
    } else {
      this.svc
        .createRule(this.ruleForm)
        .pipe(finalize(() => (this.ruleSaving = false)))
        .subscribe({ next: onDone, error: onErr });
    }
  }

  loadBalance(): void {
    const sid = this.postSchoolId;
    const eid = this.postEmployeeId;
    if (sid == null || sid <= 0 || eid == null || eid <= 0) {
      this.toastr.warning(this.translate.instant('centralPoints.validation.balance'));
      return;
    }
    this.balanceLoading = true;
    this.balanceSnapshot = null;
    this.svc
      .getBalance(eid, sid)
      .pipe(finalize(() => (this.balanceLoading = false)))
      .subscribe({
        next: (b) => {
          if (b == null) {
            this.balanceSnapshot = null;
            this.toastr.info(this.translate.instant('centralPoints.balance.none'));
            return;
          }
          this.balanceSnapshot = {
            totalPoints: b.totalPoints,
            updatedAtUtc: b.updatedAtUtc,
            academicYearID: b.academicYearID,
          };
        },
        error: (e) => this.toastr.error(readCentralPointsHttpError(e)),
      });
  }

  submitPost(): void {
    const sid = this.postSchoolId;
    const eid = this.postEmployeeId;
    const code = (this.postSourceCode ?? '').trim();
    if (sid == null || sid <= 0 || eid == null || eid <= 0) {
      this.toastr.warning(this.translate.instant('centralPoints.validation.post'));
      return;
    }
    if (!code) {
      this.toastr.warning(this.translate.instant('centralPoints.validation.sourceCode'));
      return;
    }
    if (this.postOverrideEnabled && this.postOverrideValue == null) {
      this.toastr.warning(this.translate.instant('centralPoints.validation.override'));
      return;
    }
    const dto: PostCentralPointsDto = {
      employeeProfileID: eid,
      schoolID: sid,
      pointsSourceCode: code,
      ruleKey: (this.postRuleKey ?? '*').trim() || '*',
      overrideDeltaPoints: this.postOverrideEnabled ? this.postOverrideValue : null,
      correlationEntityType: this.postCorrelationType.trim() || null,
      correlationEntityID: this.postCorrelationId != null && this.postCorrelationId > 0 ? this.postCorrelationId : null,
      idempotencyKey: this.postIdempotency.trim() || null,
      notes: this.postNotes.trim() || null,
      memo: this.postMemo.trim() || null,
    };
    this.postSubmitting = true;
    this.svc
      .post(dto)
      .pipe(finalize(() => (this.postSubmitting = false)))
      .subscribe({
        next: (res) => {
          const msg = res.wasIdempotentReplay
            ? this.translate.instant('centralPoints.toast.postIdempotent')
            : this.translate.instant('centralPoints.toast.postOk', { delta: res.appliedDeltaPoints, bal: res.newBalanceTotal });
          this.toastr.success(msg);
          if (this.activeTab === '0') this.loadLedger();
          this.balanceSnapshot = { totalPoints: res.newBalanceTotal, updatedAtUtc: new Date().toISOString() };
        },
        error: (e) => this.toastr.error(readCentralPointsHttpError(e)),
      });
  }

  rebuildBalance(): void {
    if (!this.isTenantAdmin) return;
    const sid = this.postSchoolId;
    const eid = this.postEmployeeId;
    if (sid == null || sid <= 0 || eid == null || eid <= 0) {
      this.toastr.warning(this.translate.instant('centralPoints.validation.balance'));
      return;
    }
    this.rebuildSubmitting = true;
    this.svc
      .rebuildBalance(eid, sid)
      .pipe(finalize(() => (this.rebuildSubmitting = false)))
      .subscribe({
        next: (total) => {
          this.balanceSnapshot = { totalPoints: total, updatedAtUtc: new Date().toISOString() };
          this.toastr.success(this.translate.instant('centralPoints.toast.rebuildOk', { total }));
        },
        error: (e) => this.toastr.error(readCentralPointsHttpError(e)),
      });
  }

  onRulesTabActivate(): void {
    this.loadRules();
  }

  onPostTabActivate(): void {
    this.loadPostEmployees();
  }
}
