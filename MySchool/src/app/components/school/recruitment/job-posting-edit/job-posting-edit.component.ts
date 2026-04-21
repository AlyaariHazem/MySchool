import { NgIf } from '@angular/common';
import { Component, inject, OnInit } from '@angular/core';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { ButtonModule } from 'primeng/button';
import { ToastrService } from 'ngx-toastr';
import { finalize } from 'rxjs/operators';

import { PagePermission, PermissionService } from 'app/core/services/permission.service';
import { SchoolService } from 'app/core/services/school.service';
import { YearService } from 'app/core/services/year.service';
import { Year } from 'app/core/models/year.model';
import { School } from 'app/core/models/school.modul';
import { ShardModule } from 'app/shared/shard.module';

import { JobPostingCreateDto, JobPostingReadDto } from '../recruitment.models';
import { JobPostingFormComponent } from '../job-posting-form/job-posting-form.component';
import { RecruitmentService, readRecruitmentHttpError } from '../recruitment.service';

@Component({
  selector: 'app-job-posting-edit',
  standalone: true,
  imports: [
    ShardModule,
    NgIf,
    TranslateModule,
    ButtonModule,
    RouterLink,
    JobPostingFormComponent,
  ],
  templateUrl: './job-posting-edit.component.html',
  styleUrl: './job-posting-edit.component.scss',
})
export class JobPostingEditComponent implements OnInit {
  private readonly route = inject(ActivatedRoute);
  private readonly recruitment = inject(RecruitmentService);
  private readonly schoolService = inject(SchoolService);
  private readonly yearService = inject(YearService);
  private readonly router = inject(Router);
  private readonly toastr = inject(ToastrService);
  private readonly translate = inject(TranslateService);
  private readonly perm = inject(PermissionService);

  id = 0;
  schools: School[] = [];
  years: Year[] = [];
  initial: JobPostingReadDto | null = null;
  loadingMeta = true;
  loadingRow = true;
  submitting = false;

  canUpdate = this.perm.hasAny([PagePermission.Recruitment.Update, PagePermission.Employees.Update]);

  cancel(): void {
    this.router.navigate(['/school/recruitment/job-postings', this.id]).catch(() => undefined);
  }

  ngOnInit(): void {
    const p = this.route.snapshot.paramMap.get('id');
    this.id = p ? +p : 0;
    if (!this.id || !this.canUpdate) {
      this.loadingMeta = false;
      this.loadingRow = false;
      return;
    }
    this.schoolService.getAllSchools().subscribe({
      next: (s) => (this.schools = s ?? []),
      error: () => this.toastr.error('employeesHr.errors.loadSchools'),
    });
    this.yearService.getAllYears().subscribe({
      next: (y) => (this.years = y ?? []),
      error: () => this.toastr.error('employeesHr.errors.loadYears'),
    });
    this.recruitment
      .getJobPostingById(this.id)
      .pipe(finalize(() => (this.loadingRow = false)))
      .subscribe({
        next: (row) => (this.initial = row),
        error: (err) => {
          this.toastr.error(readRecruitmentHttpError(err));
          this.initial = null;
        },
      });
    this.loadingMeta = false;
  }

  onSubmit(dto: JobPostingCreateDto): void {
    this.submitting = true;
    this.recruitment
      .updateJobPosting(this.id, dto)
      .pipe(finalize(() => (this.submitting = false)))
      .subscribe({
        next: () => {
          this.toastr.success(this.translate.instant('recruitment.postings.updated'));
          this.router.navigate(['/school/recruitment/job-postings', this.id]).catch(() => undefined);
        },
        error: (err) => this.toastr.error(readRecruitmentHttpError(err)),
      });
  }
}
