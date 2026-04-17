import { DatePipe, NgIf } from '@angular/common';
import { Component, inject, OnInit } from '@angular/core';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { ConfirmationService } from 'primeng/api';
import { ButtonModule } from 'primeng/button';
import { ConfirmDialogModule } from 'primeng/confirmdialog';
import { ProgressSpinnerModule } from 'primeng/progressspinner';
import { TagModule } from 'primeng/tag';
import { ToastrService } from 'ngx-toastr';
import { finalize } from 'rxjs/operators';

import { PagePermission, PermissionService } from 'app/core/services/permission.service';
import { SchoolService } from 'app/core/services/school.service';
import { ShardModule } from 'app/shared/shard.module';

import { JobPostingReadDto, JobPostingStatus } from '../recruitment.models';
import { RecruitmentService, readRecruitmentHttpError } from '../recruitment.service';

@Component({
  selector: 'app-job-posting-detail',
  standalone: true,
  imports: [
    ShardModule,
    NgIf,
    TranslateModule,
    ButtonModule,
    RouterLink,
    ProgressSpinnerModule,
    DatePipe,
    TagModule,
    ConfirmDialogModule,
  ],
  providers: [ConfirmationService],
  templateUrl: './job-posting-detail.component.html',
  styleUrl: './job-posting-detail.component.scss',
})
export class JobPostingDetailComponent implements OnInit {
  private readonly route = inject(ActivatedRoute);
  private readonly recruitment = inject(RecruitmentService);
  private readonly schoolService = inject(SchoolService);
  private readonly toastr = inject(ToastrService);
  private readonly translate = inject(TranslateService);
  private readonly confirm = inject(ConfirmationService);
  private readonly perm = inject(PermissionService);

  id = 0;
  row: JobPostingReadDto | null = null;
  loading = true;
  schoolName = '';

  readonly canView = this.perm.hasAny([PagePermission.Recruitment.View, PagePermission.Employees.View]);
  readonly canUpdate = this.perm.hasAny([PagePermission.Recruitment.Update, PagePermission.Employees.Update]);

  JobPostingStatus = JobPostingStatus;

  ngOnInit(): void {
    const p = this.route.snapshot.paramMap.get('id');
    this.id = p ? +p : 0;
    if (!this.id || !this.canView) {
      this.loading = false;
      return;
    }
    this.load();
  }

  load(): void {
    this.loading = true;
    this.recruitment
      .getJobPostingById(this.id)
      .pipe(finalize(() => (this.loading = false)))
      .subscribe({
        next: (r) => {
          this.row = r;
          if (r?.schoolID) {
            this.schoolService.getAllSchools().subscribe({
              next: (list) => {
                const s = list?.find((x) => x.schoolID === r.schoolID);
                this.schoolName = s?.schoolName ?? String(r.schoolID);
              },
            });
          }
        },
        error: (err) => {
          this.toastr.error(readRecruitmentHttpError(err));
          this.row = null;
        },
      });
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

  confirmOpen(): void {
    if (!this.row) return;
    this.confirm.confirm({
      message: this.translate.instant('recruitment.postings.confirmOpen'),
      accept: () =>
        this.recruitment.openJobPosting(this.row!.jobPostingID).subscribe({
          next: (r) => {
            this.row = r;
            this.toastr.success(this.translate.instant('recruitment.postings.opened'));
          },
          error: (e) => this.toastr.error(readRecruitmentHttpError(e)),
        }),
    });
  }

  confirmClose(): void {
    if (!this.row) return;
    this.confirm.confirm({
      message: this.translate.instant('recruitment.postings.confirmClose'),
      accept: () =>
        this.recruitment.closeJobPosting(this.row!.jobPostingID).subscribe({
          next: (r) => {
            this.row = r;
            this.toastr.success(this.translate.instant('recruitment.postings.closed'));
          },
          error: (e) => this.toastr.error(readRecruitmentHttpError(e)),
        }),
    });
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

  confirmArchive(): void {
    if (!this.row) return;
    this.confirm.confirm({
      message: this.translate.instant('recruitment.postings.confirmArchive'),
      accept: () =>
        this.recruitment.archiveJobPosting(this.row!.jobPostingID).subscribe({
          next: (r) => {
            this.row = r;
            this.toastr.success(this.translate.instant('recruitment.postings.archived'));
          },
          error: (e) => this.toastr.error(readRecruitmentHttpError(e)),
        }),
    });
  }
}
