import { AsyncPipe, DatePipe, NgIf } from '@angular/common';
import { Component, inject, OnDestroy, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { Store } from '@ngrx/store';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { ButtonModule } from 'primeng/button';
import { DialogModule } from 'primeng/dialog';
import { FloatLabelModule } from 'primeng/floatlabel';
import { InputTextModule } from 'primeng/inputtext';
import { ProgressSpinnerModule } from 'primeng/progressspinner';
import { Select } from 'primeng/select';
import { TableModule } from 'primeng/table';
import { TagModule } from 'primeng/tag';
import { TooltipModule } from 'primeng/tooltip';
import { map } from 'rxjs';
import { ToastrService } from 'ngx-toastr';
import { finalize } from 'rxjs/operators';
import { Subscription } from 'rxjs';

import { PagePermission, PermissionService } from 'app/core/services/permission.service';
import { SchoolService } from 'app/core/services/school.service';
import { YearService } from 'app/core/services/year.service';
import { Year } from 'app/core/models/year.model';
import { School } from 'app/core/models/school.modul';
import { selectLanguage } from 'app/core/store/language/language.selectors';
import { ShardModule } from 'app/shared/shard.module';

import {
  JobApplicationCreateDto,
  JobApplicationFilterDto,
  JobApplicationListDto,
  JobApplicationReadDto,
  JobApplicationStatus,
  JobApplicationUpdateDto,
  JobPostingListDto,
  JobPostingStatus,
} from '../recruitment.models';
import { JobApplicationDetailComponent } from '../job-application-detail/job-application-detail.component';
import { JobApplicationFormComponent } from '../job-application-form/job-application-form.component';
import { RecruitmentService, readRecruitmentHttpError } from '../recruitment.service';

@Component({
  selector: 'app-job-applications-list',
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
    DialogModule,
    Select,
    FloatLabelModule,
    InputTextModule,
    ProgressSpinnerModule,
    TooltipModule,
    TagModule,
    JobApplicationFormComponent,
    JobApplicationDetailComponent,
  ],
  templateUrl: './job-applications-list.component.html',
  styleUrl: './job-applications-list.component.scss',
})
export class JobApplicationsListComponent implements OnInit, OnDestroy {
  private readonly recruitment = inject(RecruitmentService);
  private readonly schoolService = inject(SchoolService);
  private readonly yearService = inject(YearService);
  private readonly route = inject(ActivatedRoute);
  private readonly toastr = inject(ToastrService);
  private readonly translate = inject(TranslateService);
  private readonly perm = inject(PermissionService);
  private readonly store = inject(Store);
  private qpSub?: Subscription;

  readonly dir$ = this.store.select(selectLanguage).pipe(map((l) => (l === 'ar' ? 'rtl' : 'ltr')));

  readonly canView = this.perm.hasAny([PagePermission.Recruitment.View, PagePermission.Employees.View]);
  readonly canCreate = this.perm.hasAny([PagePermission.Recruitment.Create, PagePermission.Employees.Create]);
  readonly canUpdate = this.perm.hasAny([PagePermission.Recruitment.Update, PagePermission.Employees.Update]);

  schools: School[] = [];
  years: Year[] = [];
  rows: JobApplicationListDto[] = [];
  loading = false;
  error: string | null = null;

  filter: JobApplicationFilterDto = {};
  schoolOptions: { label: string; value: number }[] = [];
  yearOptions: { label: string; value: number }[] = [];
  statusOptions: { label: string; value: JobApplicationStatus }[] = [];
  openPostings: JobPostingListDto[] = [];
  showCreateDialog = false;
  showEditDialog = false;
  showWorkflowDialog = false;
  dialogLoading = false;
  formSubmitting = false;
  dialogError: string | null = null;
  selectedApplicationId: number | null = null;
  selectedApplication: JobApplicationReadDto | null = null;

  JobApplicationStatus = JobApplicationStatus;

  ngOnInit(): void {
    this.statusOptions = [
      { label: this.translate.instant('recruitment.appStatus.submitted'), value: JobApplicationStatus.Submitted },
      { label: this.translate.instant('recruitment.appStatus.underReview'), value: JobApplicationStatus.UnderReview },
      {
        label: this.translate.instant('recruitment.appStatus.interviewScheduled'),
        value: JobApplicationStatus.InterviewScheduled,
      },
      { label: this.translate.instant('recruitment.appStatus.evaluated'), value: JobApplicationStatus.Evaluated },
      { label: this.translate.instant('recruitment.appStatus.accepted'), value: JobApplicationStatus.Accepted },
      { label: this.translate.instant('recruitment.appStatus.rejected'), value: JobApplicationStatus.Rejected },
      {
        label: this.translate.instant('recruitment.appStatus.convertedToEmployee'),
        value: JobApplicationStatus.ConvertedToEmployee,
      },
      { label: this.translate.instant('recruitment.appStatus.withdrawn'), value: JobApplicationStatus.Withdrawn },
    ];
    this.schoolService.getAllSchools().subscribe({
      next: (list) => {
        this.schools = list ?? [];
        this.schoolOptions = this.schools
          .filter((s): s is School & { schoolID: number } => s.schoolID != null && s.schoolID > 0)
          .map((s) => ({ label: s.schoolName || String(s.schoolID), value: s.schoolID }));
      },
      error: () => this.toastr.error(this.translate.instant('employeesHr.errors.loadSchools')),
    });
    this.yearService.getAllYears().subscribe({
      next: (list) => {
        this.years = list ?? [];
        this.rebuildYearOptions();
      },
      error: () => this.toastr.error(this.translate.instant('employeesHr.errors.loadYears')),
    });
    this.recruitment.getJobPostings({ status: JobPostingStatus.Open, isActive: true }).subscribe({
      next: (rows) => (this.openPostings = rows ?? []),
      error: () => (this.openPostings = []),
    });
    this.qpSub = this.route.queryParamMap.subscribe((q) => {
      const jpid = q.get('jobPostingID');
      if (jpid) {
        const n = +jpid;
        if (n > 0) this.filter.jobPostingID = n;
      }
      this.load();
    });
  }

  ngOnDestroy(): void {
    this.qpSub?.unsubscribe();
  }

  onSchoolChange(): void {
    this.filter.academicYearID = undefined;
    this.rebuildYearOptions();
  }

  private rebuildYearOptions(): void {
    const sid = this.filter.schoolID;
    const list = sid ? this.years.filter((y) => y.schoolID === sid) : this.years;
    this.yearOptions = list
      .filter((y) => y.yearID != null && y.yearID > 0)
      .map((y) => ({
        label: `${y.yearDateStart ? new Date(y.yearDateStart).getFullYear() : y.yearID}`,
        value: y.yearID!,
      }));
  }

  load(): void {
    if (!this.canView) return;
    this.loading = true;
    this.error = null;
    this.recruitment
      .getJobApplications(this.filter)
      .pipe(finalize(() => (this.loading = false)))
      .subscribe({
        next: (r) => (this.rows = r ?? []),
        error: (err) => {
          this.error = readRecruitmentHttpError(err);
          this.rows = [];
        },
      });
  }

  schoolName(id: number): string {
    return this.schools.find((s) => s.schoolID === id)?.schoolName ?? String(id);
  }

  applicantName(row: JobApplicationListDto): string {
    return [row.applicantFirstName, row.applicantLastName].filter(Boolean).join(' ');
  }

  appStatusSeverity(
    s: JobApplicationStatus,
  ): 'success' | 'info' | 'warn' | 'danger' | 'secondary' | 'contrast' {
    switch (s) {
      case JobApplicationStatus.Accepted:
      case JobApplicationStatus.ConvertedToEmployee:
        return 'success';
      case JobApplicationStatus.Rejected:
      case JobApplicationStatus.Withdrawn:
        return 'danger';
      case JobApplicationStatus.Evaluated:
      case JobApplicationStatus.InterviewScheduled:
        return 'info';
      default:
        return 'secondary';
    }
  }

  appStatusLabelKey(s: JobApplicationStatus): string {
    const m: Record<number, string> = {
      [JobApplicationStatus.Submitted]: 'submitted',
      [JobApplicationStatus.UnderReview]: 'underReview',
      [JobApplicationStatus.InterviewScheduled]: 'interviewScheduled',
      [JobApplicationStatus.Evaluated]: 'evaluated',
      [JobApplicationStatus.Accepted]: 'accepted',
      [JobApplicationStatus.Rejected]: 'rejected',
      [JobApplicationStatus.ConvertedToEmployee]: 'convertedToEmployee',
      [JobApplicationStatus.Withdrawn]: 'withdrawn',
    };
    return m[s] ?? 'submitted';
  }

  openCreateDialog(): void {
    this.dialogError = null;
    this.showCreateDialog = true;
  }

  closeCreateDialog(): void {
    if (this.formSubmitting) return;
    this.showCreateDialog = false;
  }

  openEditDialog(id: number): void {
    this.showWorkflowDialog = false;
    this.selectedApplicationId = id;
    this.selectedApplication = null;
    this.dialogError = null;
    this.dialogLoading = true;
    this.showEditDialog = true;
    this.recruitment
      .getJobApplicationById(id)
      .pipe(finalize(() => (this.dialogLoading = false)))
      .subscribe({
        next: (row) => (this.selectedApplication = row),
        error: (err) => {
          this.dialogError = readRecruitmentHttpError(err);
          this.selectedApplication = null;
        },
      });
  }

  closeEditDialog(): void {
    if (this.formSubmitting) return;
    this.showEditDialog = false;
  }

  openWorkflowDialog(id: number): void {
    this.selectedApplicationId = id;
    this.showWorkflowDialog = true;
  }

  closeWorkflowDialog(): void {
    this.showWorkflowDialog = false;
    this.selectedApplicationId = null;
  }

  createApplication(dto: JobApplicationCreateDto | Record<string, unknown>): void {
    this.formSubmitting = true;
    this.recruitment
      .createJobApplication(dto as JobApplicationCreateDto)
      .pipe(finalize(() => (this.formSubmitting = false)))
      .subscribe({
        next: (row) => {
          this.toastr.success(this.translate.instant('recruitment.applications.created'));
          this.showCreateDialog = false;
          this.load();
          this.openWorkflowDialog(row.jobApplicationID);
        },
        error: (err) => this.toastr.error(readRecruitmentHttpError(err)),
      });
  }

  updateApplication(payload: JobApplicationUpdateDto | Record<string, unknown>): void {
    if (!this.selectedApplicationId) return;
    this.formSubmitting = true;
    this.recruitment
      .updateJobApplication(this.selectedApplicationId, payload as JobApplicationUpdateDto)
      .pipe(finalize(() => (this.formSubmitting = false)))
      .subscribe({
        next: () => {
          this.toastr.success(this.translate.instant('recruitment.applications.updated'));
          this.showEditDialog = false;
          this.load();
          this.openWorkflowDialog(this.selectedApplicationId!);
        },
        error: (err) => this.toastr.error(readRecruitmentHttpError(err)),
      });
  }
}
