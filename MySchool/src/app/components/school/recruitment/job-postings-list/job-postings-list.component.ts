import { AsyncPipe, DatePipe, NgIf } from '@angular/common';
import { Component, inject, OnDestroy, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { Store } from '@ngrx/store';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { ConfirmationService } from 'primeng/api';
import { ButtonModule } from 'primeng/button';
import { ConfirmDialogModule } from 'primeng/confirmdialog';
import { FloatLabelModule } from 'primeng/floatlabel';
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

import { EmployeeJobTypeDto } from '../../employees-hr/employees-hr.models';
import { EmployeesHrService } from '../../employees-hr/employees-hr.service';
import { JobPostingFilterDto, JobPostingListDto, JobPostingStatus } from '../recruitment.models';
import { RecruitmentService, readRecruitmentHttpError } from '../recruitment.service';

@Component({
  selector: 'app-job-postings-list',
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
    Select,
    FloatLabelModule,
    ProgressSpinnerModule,
    TooltipModule,
    ConfirmDialogModule,
    TagModule,
  ],
  providers: [ConfirmationService],
  templateUrl: './job-postings-list.component.html',
  styleUrl: './job-postings-list.component.scss',
})
export class JobPostingsListComponent implements OnInit, OnDestroy {
  private readonly recruitment = inject(RecruitmentService);
  private readonly employeesHr = inject(EmployeesHrService);
  private readonly schoolService = inject(SchoolService);
  private readonly yearService = inject(YearService);
  private readonly toastr = inject(ToastrService);
  private readonly confirm = inject(ConfirmationService);
  private readonly translate = inject(TranslateService);
  private readonly perm = inject(PermissionService);
  private readonly store = inject(Store);
  private langSub?: Subscription;

  readonly dir$ = this.store.select(selectLanguage).pipe(map((l) => (l === 'ar' ? 'rtl' : 'ltr')));

  readonly canView = this.perm.hasAny([PagePermission.Recruitment.View, PagePermission.Employees.View]);
  readonly canCreate = this.perm.hasAny([PagePermission.Recruitment.Create, PagePermission.Employees.Create]);
  readonly canUpdate = this.perm.hasAny([PagePermission.Recruitment.Update, PagePermission.Employees.Update]);

  schools: School[] = [];
  years: Year[] = [];
  rows: JobPostingListDto[] = [];
  loading = false;
  error: string | null = null;

  filter: JobPostingFilterDto = {};
  schoolOptions: { label: string; value: number }[] = [];
  yearOptions: { label: string; value: number }[] = [];
  jobTypeOptions: { label: string; value: number }[] = [];
  jobTypesLoading = false;

  statusOptions: { label: string; value: JobPostingStatus }[] = [];
  activeOptions: { label: string; value: boolean | null }[] = [];

  JobPostingStatus = JobPostingStatus;

  ngOnInit(): void {
    this.statusOptions = [
      { label: this.translate.instant('recruitment.postingStatus.draft'), value: JobPostingStatus.Draft },
      { label: this.translate.instant('recruitment.postingStatus.open'), value: JobPostingStatus.Open },
      { label: this.translate.instant('recruitment.postingStatus.closed'), value: JobPostingStatus.Closed },
      { label: this.translate.instant('recruitment.postingStatus.archived'), value: JobPostingStatus.Archived },
    ];
    this.activeOptions = [
      { label: this.translate.instant('employeesHr.filter.allActive'), value: null },
      { label: this.translate.instant('employeesHr.filter.activeOnly'), value: true },
      { label: this.translate.instant('employeesHr.filter.inactiveOnly'), value: false },
    ];
    this.langSub = this.translate.onLangChange.subscribe(() => this.rebuildStaticLabels());
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
    this.loadJobTypes();
    this.load();
  }

  ngOnDestroy(): void {
    this.langSub?.unsubscribe();
  }

  private rebuildStaticLabels(): void {
    this.statusOptions = [
      { label: this.translate.instant('recruitment.postingStatus.draft'), value: JobPostingStatus.Draft },
      { label: this.translate.instant('recruitment.postingStatus.open'), value: JobPostingStatus.Open },
      { label: this.translate.instant('recruitment.postingStatus.closed'), value: JobPostingStatus.Closed },
      { label: this.translate.instant('recruitment.postingStatus.archived'), value: JobPostingStatus.Archived },
    ];
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

  onSchoolChange(): void {
    this.filter.academicYearID = undefined;
    this.rebuildYearOptions();
  }

  private loadJobTypes(): void {
    this.jobTypesLoading = true;
    this.employeesHr
      .getEmployeeJobTypes()
      .pipe(finalize(() => (this.jobTypesLoading = false)))
      .subscribe({
        next: (rows: EmployeeJobTypeDto[]) => {
          const lang = this.translate.currentLang;
          this.jobTypeOptions = rows.map((j) => ({
            value: j.employeeJobTypeID,
            label: lang === 'ar' && j.nameAr ? j.nameAr : j.name,
          }));
        },
        error: () => (this.jobTypeOptions = []),
      });
  }

  load(): void {
    if (!this.canView) return;
    this.loading = true;
    this.error = null;
    this.recruitment
      .getJobPostings(this.filter)
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

  yearLabel(id: number | null | undefined): string {
    if (id == null) return '—';
    const y = this.years.find((x) => x.yearID === id);
    return y?.yearDateStart ? String(new Date(y.yearDateStart).getFullYear()) : String(id);
  }

  postingStatusLabelKey(s: JobPostingStatus): string {
    switch (s) {
      case JobPostingStatus.Draft:
        return 'draft';
      case JobPostingStatus.Open:
        return 'open';
      case JobPostingStatus.Closed:
        return 'closed';
      case JobPostingStatus.Archived:
        return 'archived';
      default:
        return 'draft';
    }
  }

  severityStatus(s: JobPostingStatus): 'success' | 'info' | 'warn' | 'danger' | 'secondary' | 'contrast' {
    switch (s) {
      case JobPostingStatus.Open:
        return 'success';
      case JobPostingStatus.Closed:
        return 'warn';
      case JobPostingStatus.Archived:
        return 'secondary';
      default:
        return 'info';
    }
  }

  confirmOpen(row: JobPostingListDto): void {
    this.confirm.confirm({
      message: this.translate.instant('recruitment.postings.confirmOpen'),
      accept: () =>
        this.recruitment.openJobPosting(row.jobPostingID).subscribe({
          next: () => {
            this.toastr.success(this.translate.instant('recruitment.postings.opened'));
            this.load();
          },
          error: (e) => this.toastr.error(readRecruitmentHttpError(e)),
        }),
    });
  }

  confirmClose(row: JobPostingListDto): void {
    this.confirm.confirm({
      message: this.translate.instant('recruitment.postings.confirmClose'),
      accept: () =>
        this.recruitment.closeJobPosting(row.jobPostingID).subscribe({
          next: () => {
            this.toastr.success(this.translate.instant('recruitment.postings.closed'));
            this.load();
          },
          error: (e) => this.toastr.error(readRecruitmentHttpError(e)),
        }),
    });
  }

  confirmArchive(row: JobPostingListDto): void {
    this.confirm.confirm({
      message: this.translate.instant('recruitment.postings.confirmArchive'),
      accept: () =>
        this.recruitment.archiveJobPosting(row.jobPostingID).subscribe({
          next: () => {
            this.toastr.success(this.translate.instant('recruitment.postings.archived'));
            this.load();
          },
          error: (e) => this.toastr.error(readRecruitmentHttpError(e)),
        }),
    });
  }
}
