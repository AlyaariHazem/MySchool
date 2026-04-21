import { AsyncPipe, DatePipe, NgIf } from '@angular/common';
import { Component, OnInit, inject } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Store } from '@ngrx/store';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { ButtonModule } from 'primeng/button';
import { ConfirmDialogModule } from 'primeng/confirmdialog';
import { FloatLabelModule } from 'primeng/floatlabel';
import { ProgressSpinnerModule } from 'primeng/progressspinner';
import { Select } from 'primeng/select';
import { TableModule } from 'primeng/table';
import { TooltipModule } from 'primeng/tooltip';
import { DialogModule } from 'primeng/dialog';
import { ConfirmationService } from 'primeng/api';
import { ToastrService } from 'ngx-toastr';
import { finalize } from 'rxjs/operators';
import { map } from 'rxjs';

import { isSchoolManagerUser } from 'app/core/utils/school-role.util';
import { PagePermission, PermissionService } from 'app/core/services/permission.service';
import { SchoolService } from 'app/core/services/school.service';
import { School } from 'app/core/models/school.modul';
import { selectLanguage } from 'app/core/store/language/language.selectors';
import { ShardModule } from 'app/shared/shard.module';

import { AchievementsFormComponent } from '../achievements-form/achievements-form.component';
import {
  AchievementRequestFilterDto,
  AchievementRequestListItemDto,
  AchievementRequestStatus,
  displayAchievementTitle,
} from '../achievements.models';
import { AchievementsService, readAchievementHttpError } from '../achievements.service';

@Component({
  selector: 'app-achievements-list',
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
    ProgressSpinnerModule,
    TooltipModule,
    ConfirmDialogModule,
    DialogModule,
    AchievementsFormComponent,
  ],
  providers: [ConfirmationService],
  templateUrl: './achievements-list.component.html',
  styleUrl: './achievements-list.component.scss',
})
export class AchievementsListComponent implements OnInit {
  private readonly svc = inject(AchievementsService);
  private readonly schoolService = inject(SchoolService);
  private readonly toastr = inject(ToastrService);
  private readonly confirm = inject(ConfirmationService);
  private readonly translate = inject(TranslateService);
  private readonly perm = inject(PermissionService);
  private readonly store = inject(Store);

  readonly dir$ = this.store.select(selectLanguage).pipe(map((l) => (l === 'ar' ? 'rtl' : 'ltr')));

  readonly filterSelectPanelStyle: Record<string, string> = {
    maxWidth: 'min(22rem, calc(100vw - 2rem))',
  };

  loading = false;
  rows: AchievementRequestListItemDto[] = [];

  filter: AchievementRequestFilterDto = {};

  schoolOptions: { label: string; value: number }[] = [];
  statusOptions: { label: string; value: number }[] = [];

  formDialogVisible = false;
  editingRequestId: number | null = null;

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

  get canDelete(): boolean {
    return this.perm.hasPermission(PagePermission.Employees.Delete);
  }

  get formDialogHeaderKey(): string {
    return this.editingRequestId != null && this.editingRequestId > 0
      ? 'achievementRequests.form.titleEdit'
      : 'achievementRequests.form.titleNew';
  }

  openCreateDialog(): void {
    this.editingRequestId = null;
    this.formDialogVisible = true;
  }

  openEditDialog(id: number): void {
    this.editingRequestId = id;
    this.formDialogVisible = true;
  }

  closeFormDialog(): void {
    this.formDialogVisible = false;
    this.editingRequestId = null;
  }

  onFormSaved(): void {
    this.closeFormDialog();
    this.load();
  }

  rowTitle(row: AchievementRequestListItemDto): string {
    return displayAchievementTitle(row) || '—';
  }

  ngOnInit(): void {
    this.statusOptions = [
      { label: this.translate.instant('achievementRequests.status.draft'), value: AchievementRequestStatus.Draft },
      { label: this.translate.instant('achievementRequests.status.submitted'), value: AchievementRequestStatus.Submitted },
      { label: this.translate.instant('achievementRequests.status.inReview'), value: AchievementRequestStatus.InReview },
      { label: this.translate.instant('achievementRequests.status.approved'), value: AchievementRequestStatus.Approved },
      { label: this.translate.instant('achievementRequests.status.rejected'), value: AchievementRequestStatus.Rejected },
      { label: this.translate.instant('achievementRequests.status.cancelled'), value: AchievementRequestStatus.Cancelled },
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
    const f: AchievementRequestFilterDto = { ...this.filter };
    if (f.status === null || f.status === undefined) delete f.status;
    this.svc
      .list(f)
      .pipe(finalize(() => (this.loading = false)))
      .subscribe({
        next: (r) => (this.rows = r ?? []),
        error: (e) => this.toastr.error(readAchievementHttpError(e)),
      });
  }

  statusLabel(v: number): string {
    const m: Record<number, string> = {
      [AchievementRequestStatus.Draft]: 'achievementRequests.status.draft',
      [AchievementRequestStatus.Submitted]: 'achievementRequests.status.submitted',
      [AchievementRequestStatus.InReview]: 'achievementRequests.status.inReview',
      [AchievementRequestStatus.Approved]: 'achievementRequests.status.approved',
      [AchievementRequestStatus.Rejected]: 'achievementRequests.status.rejected',
      [AchievementRequestStatus.Cancelled]: 'achievementRequests.status.cancelled',
    };
    return m[v] ?? String(v);
  }

  confirmDelete(row: AchievementRequestListItemDto): void {
    this.confirm.confirm({
      message: this.translate.instant('achievementRequests.list.confirmDelete', {
        title: this.rowTitle(row),
        name: row.employeeName,
      }),
      header: this.translate.instant('achievementRequests.list.confirmHeader'),
      icon: 'pi pi-exclamation-triangle',
      acceptLabel: this.translate.instant('achievementRequests.actions.delete'),
      rejectLabel: this.translate.instant('achievementRequests.actions.cancel'),
      accept: () => {
        this.svc.delete(row.achievementRequestID).subscribe({
          next: () => {
            this.toastr.success(this.translate.instant('achievementRequests.toast.deleted'));
            this.load();
          },
          error: (e) => this.toastr.error(readAchievementHttpError(e)),
        });
      },
    });
  }
}
