import { AsyncPipe, NgIf } from '@angular/common';
import { Component, OnInit, inject } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Store } from '@ngrx/store';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { ButtonModule } from 'primeng/button';
import { ConfirmDialogModule } from 'primeng/confirmdialog';
import { FloatLabelModule } from 'primeng/floatlabel';
import { Select } from 'primeng/select';
import { TableModule } from 'primeng/table';
import { TooltipModule } from 'primeng/tooltip';
import { DialogModule } from 'primeng/dialog';

import { SupervisorVisitsFormComponent } from '../supervisor-visits-form/supervisor-visits-form.component';
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

import { SupervisorVisitFilterDto, SupervisorVisitListItemDto, SupervisorVisitStatus } from '../supervisor-visits.models';
import { SupervisorVisitsService, readSupervisorVisitHttpError } from '../supervisor-visits.service';

@Component({
  selector: 'app-supervisor-visits-list',
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
    SupervisorVisitsFormComponent,
  ],
  providers: [ConfirmationService],
  templateUrl: './supervisor-visits-list.component.html',
  styleUrl: './supervisor-visits-list.component.scss',
})
export class SupervisorVisitsListComponent implements OnInit {
  private readonly svc = inject(SupervisorVisitsService);
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
  rows: SupervisorVisitListItemDto[] = [];

  filter: SupervisorVisitFilterDto = {};

  schoolOptions: { label: string; value: number }[] = [];

  /** Dialog for create / edit visit (same page). */
  formDialogVisible = false;
  /** null = new visit, number = edit. */
  editingVisitId: number | null = null;

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
    return this.editingVisitId != null && this.editingVisitId > 0
      ? 'supervisorVisits.form.titleEdit'
      : 'supervisorVisits.form.titleNew';
  }

  openCreateDialog(): void {
    this.editingVisitId = null;
    this.formDialogVisible = true;
  }

  openEditDialog(id: number): void {
    this.editingVisitId = id;
    this.formDialogVisible = true;
  }

  closeFormDialog(): void {
    this.formDialogVisible = false;
    this.editingVisitId = null;
  }

  onFormSaved(): void {
    this.closeFormDialog();
    this.load();
  }

  ngOnInit(): void {
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
      .list(this.filter)
      .pipe(finalize(() => (this.loading = false)))
      .subscribe({
        next: (r) => (this.rows = r ?? []),
        error: (e) => this.toastr.error(readSupervisorVisitHttpError(e)),
      });
  }

  statusLabel(v: number): string {
    const m: Record<number, string> = {
      [SupervisorVisitStatus.Draft]: 'supervisorVisits.status.draft',
      [SupervisorVisitStatus.Submitted]: 'supervisorVisits.status.submitted',
      [SupervisorVisitStatus.Archived]: 'supervisorVisits.status.archived',
    };
    return m[v] ?? String(v);
  }

  confirmDelete(row: SupervisorVisitListItemDto): void {
    this.confirm.confirm({
      message: this.translate.instant('supervisorVisits.list.confirmDelete', {
        name: row.visitedTeacherName,
        date: row.visitDate,
      }),
      header: this.translate.instant('supervisorVisits.list.confirmHeader'),
      icon: 'pi pi-exclamation-triangle',
      acceptLabel: this.translate.instant('supervisorVisits.actions.delete'),
      rejectLabel: this.translate.instant('supervisorVisits.actions.cancel'),
      accept: () => {
        this.svc.delete(row.supervisorVisitID).subscribe({
          next: () => {
            this.toastr.success('supervisorVisits.toast.deleted');
            this.load();
          },
          error: (e) => this.toastr.error(readSupervisorVisitHttpError(e)),
        });
      },
    });
  }
}
