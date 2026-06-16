import { AsyncPipe, DatePipe, NgIf } from '@angular/common';
import { Component, OnInit, inject } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Store } from '@ngrx/store';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { ButtonModule } from 'primeng/button';
import { DialogModule } from 'primeng/dialog';
import { FloatLabelModule } from 'primeng/floatlabel';
import { Select } from 'primeng/select';
import { TableModule } from 'primeng/table';
import { TooltipModule } from 'primeng/tooltip';
import { ToastrService } from 'ngx-toastr';
import { finalize } from 'rxjs/operators';
import { map } from 'rxjs';

import { isSchoolManagerUser } from 'app/core/utils/school-role.util';
import { PagePermission, PermissionService } from 'app/core/services/permission.service';
import { SchoolService } from 'app/core/services/school.service';
import { School } from 'app/core/models/school.modul';
import { selectLanguage } from 'app/core/store/language/language.selectors';
import { ShardModule } from 'app/shared/shard.module';

import { ActivityFilterDto, ActivityListItemDto, ActivityRequestStatus } from './activities.models';
import { ActivitiesService, readActivityHttpError } from './activities.service';
import { ActivityFormComponent } from './activity-form/activity-form.component';

@Component({
  selector: 'app-activities-list',
  standalone: true,
  imports: [
    ShardModule,
    NgIf,
    AsyncPipe,
    DatePipe,
    FormsModule,
    TranslateModule,
    TableModule,
    ButtonModule,
    Select,
    FloatLabelModule,
    TooltipModule,
    DialogModule,
    ActivityFormComponent,
  ],
  templateUrl: './activities-list.component.html',
  styleUrl: './activities-list.component.scss',
})
export class ActivitiesListComponent implements OnInit {
  private readonly svc = inject(ActivitiesService);
  private readonly schoolService = inject(SchoolService);
  private readonly toastr = inject(ToastrService);
  private readonly translate = inject(TranslateService);
  private readonly perm = inject(PermissionService);
  private readonly store = inject(Store);

  readonly dir$ = this.store.select(selectLanguage).pipe(map((l) => (l === 'ar' ? 'rtl' : 'ltr')));

  readonly filterSelectPanelStyle: Record<string, string> = {
    maxWidth: 'min(22rem, calc(100vw - 2rem))',
  };

  loading = false;
  rows: ActivityListItemDto[] = [];
  filter: ActivityFilterDto = {};
  schoolOptions: { label: string; value: number }[] = [];
  statusOptions: { label: string; value: number }[] = [];

  formDialogVisible = false;
  editingRecordId: number | null = null;

  get isSchoolManager(): boolean {
    return isSchoolManagerUser();
  }

  get canView(): boolean {
    return this.perm.hasPermission(PagePermission.Employees.View);
  }

  get canCreate(): boolean {
    return this.perm.hasPermission(PagePermission.Employees.Create);
  }

  get canUpdate(): boolean {
    return this.perm.hasPermission(PagePermission.Employees.Update);
  }

  openCreateDialog(): void {
    this.editingRecordId = null;
    this.formDialogVisible = true;
  }

  openEditDialog(id: number): void {
    this.editingRecordId = id;
    this.formDialogVisible = true;
  }

  closeFormDialog(): void {
    this.formDialogVisible = false;
    this.editingRecordId = null;
  }

  onFormSaved(): void {
    this.closeFormDialog();
    this.load();
  }

  ngOnInit(): void {
    this.statusOptions = [
      { label: this.translate.instant('activities.status.draft'), value: ActivityRequestStatus.Draft },
      { label: this.translate.instant('activities.status.submitted'), value: ActivityRequestStatus.Submitted },
      { label: this.translate.instant('activities.status.inReview'), value: ActivityRequestStatus.InReview },
      { label: this.translate.instant('activities.status.approved'), value: ActivityRequestStatus.Approved },
      { label: this.translate.instant('activities.status.rejected'), value: ActivityRequestStatus.Rejected },
      { label: this.translate.instant('activities.status.inProgress'), value: ActivityRequestStatus.InProgress },
      { label: this.translate.instant('activities.status.completed'), value: ActivityRequestStatus.Completed },
      { label: this.translate.instant('activities.status.cancelled'), value: ActivityRequestStatus.Cancelled },
    ];

    if (!this.isSchoolManager) {
      this.schoolService.getAllSchools().subscribe({
        next: (schools: School[]) => {
          this.schoolOptions = (schools ?? [])
            .filter((s) => s.schoolID != null && s.schoolID > 0)
            .map((s) => ({
              label: s.schoolName ?? String(s.schoolID),
              value: s.schoolID as number,
            }));
        },
        error: () => undefined,
      });
    }

    if (this.canView) this.load();
  }

  load(): void {
    if (!this.canView) return;
    this.loading = true;
    const f: ActivityFilterDto = { ...this.filter };
    if (f.status === null || f.status === undefined) delete f.status;

    this.svc
      .listActivities(f)
      .pipe(finalize(() => (this.loading = false)))
      .subscribe({
        next: (r) => (this.rows = r ?? []),
        error: (e) => this.toastr.error(readActivityHttpError(e)),
      });
  }

  statusLabel(v: number): string {
    const m: Record<number, string> = {
      [ActivityRequestStatus.Draft]: 'activities.status.draft',
      [ActivityRequestStatus.Submitted]: 'activities.status.submitted',
      [ActivityRequestStatus.InReview]: 'activities.status.inReview',
      [ActivityRequestStatus.Approved]: 'activities.status.approved',
      [ActivityRequestStatus.Rejected]: 'activities.status.rejected',
      [ActivityRequestStatus.InProgress]: 'activities.status.inProgress',
      [ActivityRequestStatus.Completed]: 'activities.status.completed',
      [ActivityRequestStatus.Cancelled]: 'activities.status.cancelled',
    };
    const key = m[v];
    return key ? this.translate.instant(key) : String(v);
  }
}
