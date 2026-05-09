import { Component, OnInit, inject } from '@angular/core';
import { Router } from '@angular/router';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { ToastrService } from 'ngx-toastr';

import { TeacherWorkspaceResult } from 'app/core/models/teacher-workspace.model';
import { TeacherWorkspaceService } from 'app/core/services/teacher-workspace.service';

/** Resolves `/teacher/time-capsule` to `/teacher/time-capsule/:employeeProfileId` from workspace API. */
@Component({
  standalone: true,
  selector: 'app-teacher-time-capsule-redirect',
  imports: [TranslateModule],
  template: `<div class="p-4 text-muted">{{ 'teacherWorkspace.loading' | translate }}</div>`,
})
export class TeacherTimeCapsuleRedirectComponent implements OnInit {
  private readonly workspace = inject(TeacherWorkspaceService);
  private readonly router = inject(Router);
  private readonly toastr = inject(ToastrService);
  private readonly translate = inject(TranslateService);

  ngOnInit(): void {
    this.workspace.getWorkspace().subscribe({
      next: (res) => {
        if (!res.isSuccess || !res.result) {
          this.toastr.error(this.translate.instant('teacherWorkspace.loadError'));
          void this.router.navigateByUrl('/teacher/workspace');
          return;
        }
        const r = res.result as TeacherWorkspaceResult;
        const id = r.employeeProfileId;
        if (id != null && id > 0) {
          void this.router.navigate(['/teacher', 'time-capsule', id], { replaceUrl: true });
        } else {
          this.toastr.warning(this.translate.instant('teacherWorkspace.timeCapsuleNoProfile'));
          void this.router.navigateByUrl('/teacher/workspace');
        }
      },
      error: () => {
        this.toastr.error(this.translate.instant('teacherWorkspace.loadError'));
        void this.router.navigateByUrl('/teacher/workspace');
      },
    });
  }
}
