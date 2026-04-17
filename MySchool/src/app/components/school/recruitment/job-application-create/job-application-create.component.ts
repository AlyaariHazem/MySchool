import { NgIf } from '@angular/common';
import { Component, inject, OnInit } from '@angular/core';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { ButtonModule } from 'primeng/button';
import { ProgressSpinnerModule } from 'primeng/progressspinner';
import { ToastrService } from 'ngx-toastr';
import { catchError, finalize, forkJoin, of } from 'rxjs';

import { YearService } from 'app/core/services/year.service';
import { SchoolService } from 'app/core/services/school.service';
import { hasAuthToken } from 'app/core/utils/auth-token.util';
import { isSchoolHrManager } from 'app/core/utils/school-role.util';
import { Year } from 'app/core/models/year.model';
import { ShardModule } from 'app/shared/shard.module';

import {
  JobApplicationCreateDto,
  JobPostingListDto,
  JobPostingReadDto,
  JobPostingStatus,
} from '../recruitment.models';
import { JobApplicationFormComponent } from '../job-application-form/job-application-form.component';
import { RecruitmentService, readRecruitmentHttpError } from '../recruitment.service';

@Component({
  selector: 'app-job-application-create',
  standalone: true,
  imports: [
    ShardModule,
    NgIf,
    TranslateModule,
    ButtonModule,
    RouterLink,
    ProgressSpinnerModule,
    JobApplicationFormComponent,
  ],
  templateUrl: './job-application-create.component.html',
  styleUrl: './job-application-create.component.scss',
})
export class JobApplicationCreateComponent implements OnInit {
  private readonly recruitment = inject(RecruitmentService);
  private readonly yearService = inject(YearService);
  private readonly schoolService = inject(SchoolService);
  private readonly router = inject(Router);
  private readonly route = inject(ActivatedRoute);
  private readonly toastr = inject(ToastrService);
  private readonly translate = inject(TranslateService);

  years: Year[] = [];
  openPostings: JobPostingListDto[] = [];
  loadingMeta = true;
  submitting = false;

  /** Any authenticated school user may submit an application. */
  readonly canCreate = true;
  /** Pre-selected posting from ?jobPostingID= (only when open + active). */
  presetJobPostingId: number | null = null;
  lockPostingFromQuery = false;
  /** Shown above the form when applying from a valid vacancy link. */
  postingSummary: JobPostingReadDto | null = null;
  postingSchoolName: string | null = null;

  /** GET failed for query posting (e.g. not found). */
  queryPostingInvalid = false;
  /** Loaded but not open or not active — applications not accepted. */
  queryPostingClosed = false;
  closedQueryPosting: JobPostingReadDto | null = null;

  cancel(): void {
    this.router.navigate(['/school/recruitment/job-postings']).catch(() => undefined);
  }

  ngOnInit(): void {
    const q = this.route.snapshot.queryParamMap.get('jobPostingID');
    let preset: number | null = null;
    if (q) {
      const n = +q;
      if (n > 0) preset = n;
    }

    let queryPostingFailed = false;
    const years$ = this.yearService.getAllYears().pipe(
      catchError(() => {
        if (hasAuthToken()) {
          this.toastr.error(this.translate.instant('employeesHr.errors.loadYears'));
        }
        return of([] as Year[]);
      }),
    );
    const postings$ = this.recruitment
      .getJobPostings({ status: JobPostingStatus.Open, isActive: true })
      .pipe(catchError(() => of([] as JobPostingListDto[])));

    const posting$ =
      preset && preset > 0
        ? this.recruitment.getJobPostingById(preset).pipe(
            catchError(() => {
              queryPostingFailed = true;
              return of(null as JobPostingReadDto | null);
            }),
          )
        : of(null as JobPostingReadDto | null);

    forkJoin({ years: years$, postings: postings$, posting: posting$ })
      .pipe(finalize(() => (this.loadingMeta = false)))
      .subscribe(({ years, postings, posting }) => {
        this.years = years ?? [];
        this.openPostings = postings ?? [];

        if (!preset || preset <= 0) return;

        if (queryPostingFailed || !posting) {
          this.queryPostingInvalid = true;
          this.presetJobPostingId = null;
          this.lockPostingFromQuery = false;
          return;
        }

        const canApply = posting.status === JobPostingStatus.Open && posting.isActive;
        if (!canApply) {
          this.queryPostingClosed = true;
          this.closedQueryPosting = posting;
          this.presetJobPostingId = null;
          this.lockPostingFromQuery = false;
          return;
        }

        this.postingSummary = posting;
        this.presetJobPostingId = preset;
        this.lockPostingFromQuery = true;

        if (posting.schoolID) {
          this.schoolService.getSchoolByID(posting.schoolID).subscribe({
            next: (s) => {
              this.postingSchoolName = s?.schoolName ?? String(posting.schoolID);
            },
            error: () => {
              this.postingSchoolName = String(posting.schoolID);
            },
          });
        }
      });
  }

  onSubmit(dto: JobApplicationCreateDto | Record<string, unknown>): void {
    this.submitting = true;
    this.recruitment
      .createJobApplication(dto as JobApplicationCreateDto)
      .pipe(finalize(() => (this.submitting = false)))
      .subscribe({
        next: (row) => {
          const hr = isSchoolHrManager();
          if (hr) {
            this.toastr.success(this.translate.instant('recruitment.applications.created'));
            this.router.navigate(['/school/recruitment/job-applications', row.jobApplicationID]).catch(() => undefined);
          } else {
            this.toastr.success(this.translate.instant('recruitment.applications.submittedSuccessApplicant'));
            this.router.navigate(['/school/recruitment/job-postings']).catch(() => undefined);
          }
        },
        error: (err) => this.toastr.error(readRecruitmentHttpError(err)),
      });
  }
}
