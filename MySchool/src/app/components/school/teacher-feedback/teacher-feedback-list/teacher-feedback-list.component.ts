import { AsyncPipe, NgIf } from '@angular/common';
import { Component, OnInit, inject } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Store } from '@ngrx/store';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { ButtonModule } from 'primeng/button';
import { ConfirmDialogModule } from 'primeng/confirmdialog';
import { DialogModule } from 'primeng/dialog';
import { FloatLabelModule } from 'primeng/floatlabel';
import { Select } from 'primeng/select';
import { TableModule } from 'primeng/table';
import { TooltipModule } from 'primeng/tooltip';
import { ConfirmationService } from 'primeng/api';
import { ToastrService } from 'ngx-toastr';
import { map } from 'rxjs';
import { finalize } from 'rxjs/operators';

import { isSchoolManagerUser } from 'app/core/utils/school-role.util';
import { PagePermission, PermissionService } from 'app/core/services/permission.service';
import { SchoolService } from 'app/core/services/school.service';
import { School } from 'app/core/models/school.modul';
import { selectLanguage } from 'app/core/store/language/language.selectors';
import { ShardModule } from 'app/shared/shard.module';

import { TeacherFeedbackCycleFilterDto, TeacherFeedbackCycleListItemDto, TeacherFeedbackCycleStatus } from '../teacher-feedback.models';
import { readTeacherFeedbackHttpError, TeacherFeedbackService } from '../teacher-feedback.service';
import { TeacherFeedbackCycleFormComponent } from '../teacher-feedback-cycle-form/teacher-feedback-cycle-form.component';

@Component({
  selector: 'app-teacher-feedback-list',
  standalone: true,
  imports: [
    ShardModule,
    NgIf,
    AsyncPipe,
    FormsModule,
    TranslateModule,
    TableModule,
    ButtonModule,
    Select,
    FloatLabelModule,
    TooltipModule,
    ConfirmDialogModule,
    DialogModule,
    TeacherFeedbackCycleFormComponent,
  ],
  providers: [ConfirmationService],
  templateUrl: './teacher-feedback-list.component.html',
  styleUrl: './teacher-feedback-list.component.scss',
})
export class TeacherFeedbackListComponent implements OnInit {
  private readonly svc = inject(TeacherFeedbackService);
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
  rows: TeacherFeedbackCycleListItemDto[] = [];
  filter: TeacherFeedbackCycleFilterDto = {};
  schoolOptions: { label: string; value: number }[] = [];
  statusOptions: { label: string; value: number }[] = [];

  formDialogVisible = false;
  editingCycleId: number | null = null;

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
    return this.editingCycleId != null && this.editingCycleId > 0
      ? 'teacherFeedback.form.titleEdit'
      : 'teacherFeedback.form.titleNew';
  }

  ngOnInit(): void {
    this.statusOptions = [
      { label: this.translate.instant('teacherFeedback.status.draft'), value: TeacherFeedbackCycleStatus.Draft },
      { label: this.translate.instant('teacherFeedback.status.active'), value: TeacherFeedbackCycleStatus.Active },
      { label: this.translate.instant('teacherFeedback.status.closed'), value: TeacherFeedbackCycleStatus.Closed },
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
    this.svc
      .listCycles(this.filter)
      .pipe(finalize(() => (this.loading = false)))
      .subscribe({
        next: (r) => (this.rows = r ?? []),
        error: (e) => this.toastr.error(readTeacherFeedbackHttpError(e)),
      });
  }

  statusLabel(v: number): string {
    const m: Record<number, string> = {
      [TeacherFeedbackCycleStatus.Draft]: 'teacherFeedback.status.draft',
      [TeacherFeedbackCycleStatus.Active]: 'teacherFeedback.status.active',
      [TeacherFeedbackCycleStatus.Closed]: 'teacherFeedback.status.closed',
    };
    return m[v] ?? String(v);
  }

  openCreateDialog(): void {
    this.editingCycleId = null;
    this.formDialogVisible = true;
  }

  openEditDialog(id: number): void {
    this.editingCycleId = id;
    this.formDialogVisible = true;
  }

  closeFormDialog(): void {
    this.formDialogVisible = false;
    this.editingCycleId = null;
  }

  onFormSaved(): void {
    this.closeFormDialog();
    this.load();
  }

  confirmDelete(row: TeacherFeedbackCycleListItemDto): void {
    this.confirm.confirm({
      message: this.translate.instant('teacherFeedback.list.confirmDelete', { title: row.title }),
      header: this.translate.instant('teacherFeedback.list.confirmHeader'),
      icon: 'pi pi-exclamation-triangle',
      acceptLabel: this.translate.instant('teacherFeedback.actions.delete'),
      rejectLabel: this.translate.instant('teacherFeedback.actions.cancel'),
      accept: () => {
        this.svc.deleteCycle(row.teacherFeedbackCycleID).subscribe({
          next: () => {
            this.toastr.success(this.translate.instant('teacherFeedback.toast.deleted'));
            this.load();
          },
          error: (e) => this.toastr.error(readTeacherFeedbackHttpError(e)),
        });
      },
    });
  }

  recompute(row: TeacherFeedbackCycleListItemDto): void {
    this.svc.recomputeSummaries(row.teacherFeedbackCycleID).subscribe({
      next: () => {
        this.toastr.success(this.translate.instant('teacherFeedback.toast.summariesUpdated'));
        this.load();
      },
      error: (e) => this.toastr.error(readTeacherFeedbackHttpError(e)),
    });
  }
}
