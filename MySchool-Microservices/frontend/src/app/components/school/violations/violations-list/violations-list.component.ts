import { AsyncPipe, DatePipe, NgIf } from '@angular/common';
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

import { ViolationsFormComponent } from '../violations-form/violations-form.component';
import { ViolationFilterDto, ViolationListItemDto, ViolationStatus } from '../violations.models';
import { ViolationsService, readViolationHttpError } from '../violations.service';

@Component({
  selector: 'app-violations-list',
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
    ConfirmDialogModule,
    DialogModule,
    ViolationsFormComponent,
  ],
  providers: [ConfirmationService],
  templateUrl: './violations-list.component.html',
  styleUrl: './violations-list.component.scss',
})
export class ViolationsListComponent implements OnInit {
  private readonly svc = inject(ViolationsService);
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
  rows: ViolationListItemDto[] = [];

  filter: ViolationFilterDto = {};

  schoolOptions: { label: string; value: number }[] = [];
  statusOptions: { label: string; value: number }[] = [];

  formDialogVisible = false;
  editingViolationId: number | null = null;

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
    return this.editingViolationId != null && this.editingViolationId > 0
      ? 'violations.form.titleEdit'
      : 'violations.form.titleNew';
  }

  openCreateDialog(): void {
    this.editingViolationId = null;
    this.formDialogVisible = true;
  }

  openEditDialog(id: number): void {
    this.editingViolationId = id;
    this.formDialogVisible = true;
  }

  closeFormDialog(): void {
    this.formDialogVisible = false;
    this.editingViolationId = null;
  }

  onFormSaved(): void {
    this.closeFormDialog();
    this.load();
  }

  ngOnInit(): void {
    this.statusOptions = [
      { label: this.translate.instant('violations.status.draft'), value: ViolationStatus.Draft },
      { label: this.translate.instant('violations.status.open'), value: ViolationStatus.Open },
      { label: this.translate.instant('violations.status.inProgress'), value: ViolationStatus.InProgress },
      { label: this.translate.instant('violations.status.resolved'), value: ViolationStatus.Resolved },
      { label: this.translate.instant('violations.status.closed'), value: ViolationStatus.Closed },
      { label: this.translate.instant('violations.status.cancelled'), value: ViolationStatus.Cancelled },
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
    const f: ViolationFilterDto = { ...this.filter };
    if (f.status === null || f.status === undefined) delete f.status;
    this.svc
      .list(f)
      .pipe(finalize(() => (this.loading = false)))
      .subscribe({
        next: (r) => (this.rows = r ?? []),
        error: (e) => this.toastr.error(readViolationHttpError(e)),
      });
  }

  statusLabel(v: number): string {
    const m: Record<number, string> = {
      [ViolationStatus.Draft]: 'violations.status.draft',
      [ViolationStatus.Open]: 'violations.status.open',
      [ViolationStatus.InProgress]: 'violations.status.inProgress',
      [ViolationStatus.Resolved]: 'violations.status.resolved',
      [ViolationStatus.Closed]: 'violations.status.closed',
      [ViolationStatus.Cancelled]: 'violations.status.cancelled',
    };
    const key = m[v];
    return key ? this.translate.instant(key) : String(v);
  }

  confirmDelete(row: ViolationListItemDto): void {
    this.confirm.confirm({
      message: this.translate.instant('violations.list.confirmDelete', {
        title: row.title,
        name: row.subjectEmployeeName,
      }),
      header: this.translate.instant('violations.list.confirmHeader'),
      icon: 'pi pi-exclamation-triangle',
      acceptLabel: this.translate.instant('violations.actions.delete'),
      rejectLabel: this.translate.instant('violations.actions.cancel'),
      accept: () => {
        this.svc.delete(row.violationID).subscribe({
          next: () => {
            this.toastr.success(this.translate.instant('violations.toast.deleted'));
            this.load();
          },
          error: (e) => this.toastr.error(readViolationHttpError(e)),
        });
      },
    });
  }
}
