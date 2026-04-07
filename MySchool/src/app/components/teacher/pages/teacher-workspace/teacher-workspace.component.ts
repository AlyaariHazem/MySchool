import { Component, inject, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { map } from 'rxjs';
import { Store } from '@ngrx/store';
import { TranslateService } from '@ngx-translate/core';
import { ToastrService } from 'ngx-toastr';

import { TeacherWorkspaceResult } from '../../../../core/models/teacher-workspace.model';
import { TeacherWorkspaceService } from '../../../../core/services/teacher-workspace.service';
import { selectLanguage } from '../../../../core/store/language/language.selectors';

@Component({
  selector: 'app-teacher-workspace',
  templateUrl: './teacher-workspace.component.html',
  styleUrl: './teacher-workspace.component.scss',
})
export class TeacherWorkspaceComponent implements OnInit {
  private readonly workspaceService = inject(TeacherWorkspaceService);
  private readonly router = inject(Router);
  private readonly toastr = inject(ToastrService);
  private readonly translate = inject(TranslateService);
  private readonly store = inject(Store);

  readonly dir$ = this.store.select(selectLanguage).pipe(map((l) => (l === 'ar' ? 'rtl' : 'ltr')));

  isLoading = true;
  workspace: TeacherWorkspaceResult | null = null;

  ngOnInit(): void {
    this.workspaceService.getWorkspace().subscribe({
      next: (res) => {
        if (res.isSuccess && res.result) {
          this.workspace = res.result as TeacherWorkspaceResult;
        } else {
          const msg = res.errorMasseges?.[0] ?? this.translate.instant('teacherWorkspace.loadError');
          this.toastr.error(msg);
        }
        this.isLoading = false;
      },
      error: () => {
        this.isLoading = false;
        this.toastr.error(this.translate.instant('teacherWorkspace.loadError'));
      },
    });
  }

  go(path: string): void {
    void this.router.navigateByUrl(path);
  }
}
