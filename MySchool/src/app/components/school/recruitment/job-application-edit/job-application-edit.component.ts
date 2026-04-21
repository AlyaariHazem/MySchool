import { NgIf } from '@angular/common';
import { Component, inject, OnInit } from '@angular/core';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { ButtonModule } from 'primeng/button';
import { ToastrService } from 'ngx-toastr';
import { finalize } from 'rxjs/operators';

import { PagePermission, PermissionService } from 'app/core/services/permission.service';
import { YearService } from 'app/core/services/year.service';
import { Year } from 'app/core/models/year.model';
import { ShardModule } from 'app/shared/shard.module';

import { JobApplicationReadDto, JobApplicationUpdateDto } from '../recruitment.models';
import { JobApplicationFormComponent } from '../job-application-form/job-application-form.component';
import { RecruitmentService, readRecruitmentHttpError } from '../recruitment.service';

@Component({
  selector: 'app-job-application-edit',
  standalone: true,
  imports: [
    ShardModule,
    NgIf,
    TranslateModule,
    ButtonModule,
    RouterLink,
    JobApplicationFormComponent,
  ],
  templateUrl: './job-application-edit.component.html',
  styleUrl: './job-application-edit.component.scss',
})
export class JobApplicationEditComponent implements OnInit {
  private readonly route = inject(ActivatedRoute);
  private readonly recruitment = inject(RecruitmentService);
  private readonly yearService = inject(YearService);
  private readonly router = inject(Router);
  private readonly toastr = inject(ToastrService);
  private readonly translate = inject(TranslateService);
  private readonly perm = inject(PermissionService);

  id = 0;
  years: Year[] = [];
  initial: JobApplicationReadDto | null = null;
  loadingRow = true;
  submitting = false;

  canUpdate = this.perm.hasAny([PagePermission.Recruitment.Update, PagePermission.Employees.Update]);

  cancel(): void {
    this.router.navigate(['/school/recruitment/job-applications', this.id]).catch(() => undefined);
  }

  ngOnInit(): void {
    const p = this.route.snapshot.paramMap.get('id');
    this.id = p ? +p : 0;
    if (!this.id || !this.canUpdate) {
      this.loadingRow = false;
      return;
    }
    this.yearService.getAllYears().subscribe({
      next: (y) => (this.years = y ?? []),
      error: () => this.toastr.error(this.translate.instant('employeesHr.errors.loadYears')),
    });
    this.recruitment
      .getJobApplicationById(this.id)
      .pipe(finalize(() => (this.loadingRow = false)))
      .subscribe({
        next: (row) => (this.initial = row),
        error: (err) => {
          this.toastr.error(readRecruitmentHttpError(err));
          this.initial = null;
        },
      });
  }

  onSubmit(payload: JobApplicationUpdateDto | Record<string, unknown>): void {
    this.submitting = true;
    this.recruitment
      .updateJobApplication(this.id, payload as JobApplicationUpdateDto)
      .pipe(finalize(() => (this.submitting = false)))
      .subscribe({
        next: () => {
          this.toastr.success(this.translate.instant('recruitment.applications.updated'));
          this.router.navigate(['/school/recruitment/job-applications', this.id]).catch(() => undefined);
        },
        error: (err) => this.toastr.error(readRecruitmentHttpError(err)),
      });
  }
}
