import { NgIf } from '@angular/common';
import { Component, inject, Input, OnChanges, OnDestroy, OnInit, SimpleChanges } from '@angular/core';
import { Subscription } from 'rxjs';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { TranslateModule } from '@ngx-translate/core';
import { ButtonModule } from 'primeng/button';
import { DialogModule } from 'primeng/dialog';
import { FloatLabelModule } from 'primeng/floatlabel';
import { InputNumberModule } from 'primeng/inputnumber';
import { InputTextModule } from 'primeng/inputtext';
import { Select } from 'primeng/select';
import { TableModule } from 'primeng/table';
import { TextareaModule } from 'primeng/textarea';
import { MatTabsModule } from '@angular/material/tabs';
import { ToastrService } from 'ngx-toastr';
import { TranslateService } from '@ngx-translate/core';
import { finalize } from 'rxjs/operators';

import { PagePermission, PermissionService } from 'app/core/services/permission.service';
import { SchoolService } from 'app/core/services/school.service';
import { YearService } from 'app/core/services/year.service';
import { Year } from 'app/core/models/year.model';
import { School } from 'app/core/models/school.modul';
import { ShardModule } from 'app/shared/shard.module';

import {
  ApprovalStatus,
  EmployeeJobTypeDto,
  EmployeeDocumentDto,
  EmployeeHistoryDto,
  EmployeeLeaveDto,
  EmployeePerformanceSummaryDto,
  EmployeeProfileFullDto,
  EmployeeQualificationDto,
  EmployeeSpecializationDto,
  EmploymentStatus,
  LeaveType,
} from '../employees-hr.models';
import { EmployeesHrService, readHttpError } from '../employees-hr.service';

@Component({
  selector: 'app-employees-hr-full-profile',
  standalone: true,
  imports: [
    ShardModule,
    NgIf,
    FormsModule,
    TranslateModule,
    RouterLink,
    ButtonModule,
    TableModule,
    DialogModule,
    InputTextModule,
    TextareaModule,
    Select,
    FloatLabelModule,
    InputNumberModule,
    MatTabsModule,
  ],
  templateUrl: './employees-hr-full-profile.component.html',
  styleUrl: './employees-hr-full-profile.component.scss',
})
export class EmployeesHrFullProfileComponent implements OnInit, OnDestroy, OnChanges {
  private readonly route = inject(ActivatedRoute);
  private readonly employeesHr = inject(EmployeesHrService);
  private readonly schoolService = inject(SchoolService);
  private readonly yearService = inject(YearService);
  private readonly toastr = inject(ToastrService);
  private readonly translate = inject(TranslateService);
  private readonly perm = inject(PermissionService);
  @Input() employeeId: number | null = null;
  @Input() embedded = false;

  id = 0;
  data: EmployeeProfileFullDto | null = null;
  loading = true;
  schools: School[] = [];
  years: Year[] = [];

  canView = this.perm.hasPermission(PagePermission.Employees.View);
  canCreate = this.perm.hasPermission(PagePermission.Employees.Create);

  private jobTypesRows: EmployeeJobTypeDto[] = [];
  jobTypeSelectOptions: { label: string; employeeJobTypeID: number }[] = [];
  jobTypesLoading = false;
  private langSub?: Subscription;
  leaveTypeOptions = [
    { label: 'Annual', value: LeaveType.Annual },
    { label: 'Sick', value: LeaveType.Sick },
    { label: 'Unpaid', value: LeaveType.Unpaid },
    { label: 'Emergency', value: LeaveType.Emergency },
    { label: 'Other', value: LeaveType.Other },
  ];
  approvalOptions = [
    { label: 'Pending', value: ApprovalStatus.Pending },
    { label: 'Approved', value: ApprovalStatus.Approved },
    { label: 'Rejected', value: ApprovalStatus.Rejected },
    { label: 'Cancelled', value: ApprovalStatus.Cancelled },
  ];

  // dialogs
  showQual = false;
  showSpec = false;
  showHist = false;
  showDoc = false;
  showLeave = false;
  showPerf = false;
  savingChild = false;

  qualDraft: EmployeeQualificationDto = { degreeName: '' };
  specDraft: EmployeeSpecializationDto = { name: '' } as EmployeeSpecializationDto;
  histDraft: EmployeeHistoryDto = { academicYearID: 0, schoolID: 0 } as EmployeeHistoryDto;
  docDraft: EmployeeDocumentDto = { documentType: '', title: '', isActive: true };
  leaveDraft: EmployeeLeaveDto = {
    academicYearID: 0,
    leaveType: LeaveType.Other,
    startDate: new Date().toISOString(),
    endDate: new Date().toISOString(),
    totalDays: 1,
    approvalStatus: ApprovalStatus.Pending,
  };
  perfDraft: EmployeePerformanceSummaryDto = {
    academicYearID: 0,
    schoolID: 0,
    achievementPoints: 0,
    violationPoints: 0,
    requestCount: 0,
    activityCount: 0,
  };

  ngOnChanges(changes: SimpleChanges): void {
    if (changes['employeeId'] && !changes['employeeId'].firstChange) {
      const resolved = this.resolveId();
      if (resolved !== this.id) {
        this.id = resolved;
        this.reload();
      }
    }
  }

  ngOnInit(): void {
    this.id = this.resolveId();
    this.schoolService.getAllSchools().subscribe({ next: (s) => (this.schools = s ?? []) });
    this.yearService.getAllYears().subscribe({ next: (y) => (this.years = y ?? []) });
    this.loadJobTypes();
    this.langSub = this.translate.onLangChange.subscribe(() => this.rebuildJobTypeSelectOptions());
    this.reload();
  }

  private resolveId(): number {
    if (this.employeeId != null && this.employeeId > 0) return this.employeeId;
    const p = this.route.snapshot.paramMap.get('id');
    return p ? +p : 0;
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
          this.rebuildJobTypeSelectOptions();
        },
      });
  }

  private rebuildJobTypeSelectOptions(): void {
    this.jobTypeSelectOptions = this.jobTypesRows.map((j) => ({
      employeeJobTypeID: j.employeeJobTypeID,
      label: this.jobTypeOptionLabel(j),
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

  jobTypeLabelForId(id?: number | null): string {
    if (id == null || id <= 0) {
      return this.translate.instant('employeesHr.form.jobTypeDash');
    }
    const j = this.jobTypesRows.find((x) => x.employeeJobTypeID === id);
    return j ? this.jobTypeOptionLabel(j) : String(id);
  }

  reload(): void {
    if (!this.id || !this.canView) {
      this.loading = false;
      return;
    }
    this.loading = true;
    this.employeesHr
      .getEmployeeFullProfile(this.id)
      .pipe(finalize(() => (this.loading = false)))
      .subscribe({
        next: (d) => {
          this.data = d;
          this.resetDraftsFromProfile();
        },
        error: (err) => {
          this.toastr.error(readHttpError(err));
          this.data = null;
        },
      });
  }

  private resetDraftsFromProfile(): void {
    const pr = this.data?.profile;
    if (!pr) return;
    this.histDraft = {
      academicYearID: pr.currentAcademicYearID,
      schoolID: pr.schoolID,
      employeeJobTypeID: pr.employeeJobTypeID,
    };
    this.leaveDraft = {
      ...this.leaveDraft,
      academicYearID: pr.currentAcademicYearID,
    };
    this.perfDraft = {
      ...this.perfDraft,
      academicYearID: pr.currentAcademicYearID,
      schoolID: pr.schoolID,
      employeeJobTypeID: pr.employeeJobTypeID,
    };
  }

  displayName(): string {
    const n = this.data?.profile?.fullName;
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

  employmentLabel(v: EmploymentStatus): string {
    const m: Record<EmploymentStatus, string> = {
      [EmploymentStatus.Active]: this.translate.instant('employeesHr.status.active'),
      [EmploymentStatus.OnLeave]: this.translate.instant('employeesHr.status.onLeave'),
      [EmploymentStatus.Suspended]: this.translate.instant('employeesHr.status.suspended'),
      [EmploymentStatus.Terminated]: this.translate.instant('employeesHr.status.terminated'),
    };
    return m[v] ?? String(v);
  }

  /** yyyy-MM-dd; prefers API date prefix to avoid TZ shifts. */
  dateYmd(v?: string | null): string {
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

  openQual(): void {
    this.qualDraft = { degreeName: '', major: '', institution: '', graduationYear: undefined, gradeOrScore: '', notes: '' };
    this.showQual = true;
  }
  saveQual(): void {
    if (!this.qualDraft.degreeName?.trim()) {
      this.toastr.warning(this.translate.instant('employeesHr.validation.degreeRequired'));
      return;
    }
    this.savingChild = true;
    this.employeesHr
      .addQualification(this.id, this.qualDraft)
      .pipe(finalize(() => (this.savingChild = false)))
      .subscribe({
        next: () => {
          this.toastr.success('employeesHr.toast.childSaved');
          this.showQual = false;
          this.reload();
        },
        error: (e) => this.toastr.error(readHttpError(e)),
      });
  }

  openSpec(): void {
    this.specDraft = { name: '', category: '', level: '', notes: '' };
    this.showSpec = true;
  }
  saveSpec(): void {
    if (!this.specDraft.name?.trim()) {
      this.toastr.warning(this.translate.instant('employeesHr.validation.nameRequired'));
      return;
    }
    this.savingChild = true;
    this.employeesHr
      .addSpecialization(this.id, this.specDraft)
      .pipe(finalize(() => (this.savingChild = false)))
      .subscribe({
        next: () => {
          this.toastr.success('employeesHr.toast.childSaved');
          this.showSpec = false;
          this.reload();
        },
        error: (e) => this.toastr.error(readHttpError(e)),
      });
  }

  openHist(): void {
    this.resetDraftsFromProfile();
    this.histDraft.startDate = '';
    this.histDraft.endDate = '';
    this.showHist = true;
  }
  saveHist(): void {
    if (!this.histDraft.academicYearID || !this.histDraft.schoolID) {
      this.toastr.warning(this.translate.instant('employeesHr.validation.yearSchool'));
      return;
    }
    const sd = this.histDraft.startDate ? new Date(String(this.histDraft.startDate)) : null;
    const ed = this.histDraft.endDate ? new Date(String(this.histDraft.endDate)) : null;
    if (sd && isNaN(sd.getTime())) {
      this.toastr.warning(this.translate.instant('employeesHr.validation.badDate'));
      return;
    }
    if (ed && isNaN(ed.getTime())) {
      this.toastr.warning(this.translate.instant('employeesHr.validation.badDate'));
      return;
    }
    if (sd && ed && ed < sd) {
      this.toastr.warning(this.translate.instant('employeesHr.validation.dateOrder'));
      return;
    }
    this.savingChild = true;
    const jid = this.histDraft.employeeJobTypeID;
    const jobTypeId =
      jid != null && typeof jid === 'number' && jid > 0 ? jid : undefined;
    this.employeesHr
      .addHistory(this.id, {
        ...this.histDraft,
        employeeJobTypeID: jobTypeId,
        startDate: sd && !isNaN(sd.getTime()) ? sd.toISOString() : undefined,
        endDate: ed && !isNaN(ed.getTime()) ? ed.toISOString() : undefined,
      })
      .pipe(finalize(() => (this.savingChild = false)))
      .subscribe({
        next: () => {
          this.toastr.success('employeesHr.toast.childSaved');
          this.showHist = false;
          this.reload();
        },
        error: (e) => this.toastr.error(readHttpError(e)),
      });
  }

  openDoc(): void {
    this.docDraft = { documentType: '', title: '', fileName: '', fileUrl: '', notes: '', isActive: true };
    this.showDoc = true;
  }
  saveDoc(): void {
    if (!this.docDraft.documentType?.trim() || !this.docDraft.title?.trim()) {
      this.toastr.warning(this.translate.instant('employeesHr.validation.docFields'));
      return;
    }
    this.savingChild = true;
    this.employeesHr
      .addDocument(this.id, this.docDraft)
      .pipe(finalize(() => (this.savingChild = false)))
      .subscribe({
        next: () => {
          this.toastr.success('employeesHr.toast.childSaved');
          this.showDoc = false;
          this.reload();
        },
        error: (e) => this.toastr.error(readHttpError(e)),
      });
  }

  openLeave(): void {
    this.resetDraftsFromProfile();
    const today = new Date();
    this.leaveDraft = {
      academicYearID: this.data!.profile.currentAcademicYearID,
      leaveType: LeaveType.Other,
      startDate: today.toISOString(),
      endDate: today.toISOString(),
      totalDays: 1,
      approvalStatus: ApprovalStatus.Pending,
    };
    this.showLeave = true;
  }
  saveLeave(): void {
    const s = new Date(this.leaveDraft.startDate);
    const e = new Date(this.leaveDraft.endDate);
    if (isNaN(s.getTime()) || isNaN(e.getTime())) {
      this.toastr.warning(this.translate.instant('employeesHr.validation.badDate'));
      return;
    }
    if (e < s) {
      this.toastr.warning(this.translate.instant('employeesHr.validation.dateOrder'));
      return;
    }
    this.savingChild = true;
    this.employeesHr
      .addLeave(this.id, {
        ...this.leaveDraft,
        startDate: s.toISOString(),
        endDate: e.toISOString(),
      })
      .pipe(finalize(() => (this.savingChild = false)))
      .subscribe({
        next: () => {
          this.toastr.success('employeesHr.toast.childSaved');
          this.showLeave = false;
          this.reload();
        },
        error: (err) => this.toastr.error(readHttpError(err)),
      });
  }

  openPerf(): void {
    this.resetDraftsFromProfile();
    this.showPerf = true;
  }
  savePerf(): void {
    if (!this.perfDraft.academicYearID || !this.perfDraft.schoolID) {
      this.toastr.warning(this.translate.instant('employeesHr.validation.yearSchool'));
      return;
    }
    this.savingChild = true;
    const pjid = this.perfDraft.employeeJobTypeID;
    const perfJobTypeId =
      pjid != null && typeof pjid === 'number' && pjid > 0 ? pjid : undefined;
    this.employeesHr
      .addPerformanceSummary(this.id, {
        ...this.perfDraft,
        employeeJobTypeID: perfJobTypeId,
        generatedAtUtc: new Date().toISOString(),
      })
      .pipe(finalize(() => (this.savingChild = false)))
      .subscribe({
        next: () => {
          this.toastr.success('employeesHr.toast.childSaved');
          this.showPerf = false;
          this.reload();
        },
        error: (e) => this.toastr.error(readHttpError(e)),
      });
  }

}
