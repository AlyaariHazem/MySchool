import { NgIf } from '@angular/common';
import { Component, inject, OnInit } from '@angular/core';
import { Router, RouterLink } from '@angular/router';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { ButtonModule } from 'primeng/button';
import { ProgressSpinnerModule } from 'primeng/progressspinner';
import { ToastrService } from 'ngx-toastr';
import { finalize } from 'rxjs/operators';

import { PagePermission, PermissionService } from 'app/core/services/permission.service';
import { YearService } from 'app/core/services/year.service';
import { Year } from 'app/core/models/year.model';
import { ShardModule } from 'app/shared/shard.module';

import { JobApplicationCreateDto, JobPostingListDto, JobPostingStatus } from '../recruitment.models';
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
  private readonly router = inject(Router);
  private readonly toastr = inject(ToastrService);
  private readonly translate = inject(TranslateService);
  private readonly perm = inject(PermissionService);

  years: Year[] = [];
  openPostings: JobPostingListDto[] = [];
  loadingMeta = true;
  submitting = false;

  canCreate = this.perm.hasAny([PagePermission.Recruitment.Create, PagePermission.Employees.Create]);

  cancel(): void {
    this.router.navigate(['/school/recruitment/job-applications']).catch(() => undefined);
  }

  ngOnInit(): void {
    if (!this.canCreate) {
      this.loadingMeta = false;
      return;
    }
    this.yearService
      .getAllYears()
      .pipe(finalize(() => (this.loadingMeta = false)))
      .subscribe({
        next: (y) => (this.years = y ?? []),
        error: () => this.toastr.error(this.translate.instant('employeesHr.errors.loadYears')),
      });
    this.recruitment.getJobPostings({ status: JobPostingStatus.Open, isActive: true }).subscribe({
      next: (rows) => (this.openPostings = rows ?? []),
      error: () => (this.openPostings = []),
    });
  }

  onSubmit(dto: JobApplicationCreateDto | Record<string, unknown>): void {
    this.submitting = true;
    this.recruitment
      .createJobApplication(dto as JobApplicationCreateDto)
      .pipe(finalize(() => (this.submitting = false)))
      .subscribe({
        next: (row) => {
          this.toastr.success(this.translate.instant('recruitment.applications.created'));
          this.router.navigate(['/school/recruitment/job-applications', row.jobApplicationID]).catch(() => undefined);
        },
        error: (err) => this.toastr.error(readRecruitmentHttpError(err)),
      });
  }
}
