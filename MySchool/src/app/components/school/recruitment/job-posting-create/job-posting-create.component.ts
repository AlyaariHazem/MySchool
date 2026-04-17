import { NgIf } from '@angular/common';
import { Component, inject, OnInit } from '@angular/core';
import { Router, RouterLink } from '@angular/router';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { ButtonModule } from 'primeng/button';
import { ProgressSpinnerModule } from 'primeng/progressspinner';
import { ToastrService } from 'ngx-toastr';
import { finalize } from 'rxjs/operators';

import { PagePermission, PermissionService } from 'app/core/services/permission.service';
import { SchoolService } from 'app/core/services/school.service';
import { YearService } from 'app/core/services/year.service';
import { Year } from 'app/core/models/year.model';
import { School } from 'app/core/models/school.modul';
import { ShardModule } from 'app/shared/shard.module';

import { JobPostingCreateDto } from '../recruitment.models';
import { JobPostingFormComponent } from '../job-posting-form/job-posting-form.component';
import { RecruitmentService, readRecruitmentHttpError } from '../recruitment.service';

@Component({
  selector: 'app-job-posting-create',
  standalone: true,
  imports: [
    ShardModule,
    NgIf,
    TranslateModule,
    ButtonModule,
    RouterLink,
    ProgressSpinnerModule,
    JobPostingFormComponent,
  ],
  templateUrl: './job-posting-create.component.html',
  styleUrl: './job-posting-create.component.scss',
})
export class JobPostingCreateComponent implements OnInit {
  private readonly recruitment = inject(RecruitmentService);
  private readonly schoolService = inject(SchoolService);
  private readonly yearService = inject(YearService);
  private readonly router = inject(Router);
  private readonly toastr = inject(ToastrService);
  private readonly translate = inject(TranslateService);
  private readonly perm = inject(PermissionService);

  schools: School[] = [];
  years: Year[] = [];
  loadingMeta = true;
  submitting = false;

  canCreate = this.perm.hasAny([PagePermission.Recruitment.Create, PagePermission.Employees.Create]);

  cancel(): void {
    this.router.navigate(['/school/recruitment/job-postings']).catch(() => undefined);
  }

  ngOnInit(): void {
    if (!this.canCreate) {
      this.loadingMeta = false;
      return;
    }
    this.schoolService.getAllSchools().subscribe({
      next: (s) => (this.schools = s ?? []),
      error: () => this.toastr.error('employeesHr.errors.loadSchools'),
    });
    this.yearService
      .getAllYears()
      .pipe(finalize(() => (this.loadingMeta = false)))
      .subscribe({
        next: (y) => (this.years = y ?? []),
        error: () => this.toastr.error('employeesHr.errors.loadYears'),
      });
  }

  onSubmit(dto: JobPostingCreateDto): void {
    this.submitting = true;
    this.recruitment
      .createJobPosting(dto)
      .pipe(finalize(() => (this.submitting = false)))
      .subscribe({
        next: (row) => {
          this.toastr.success(this.translate.instant('recruitment.postings.created'));
          this.router.navigate(['/school/recruitment/job-postings', row.jobPostingID]).catch(() => undefined);
        },
        error: (err) => this.toastr.error(readRecruitmentHttpError(err)),
      });
  }
}
