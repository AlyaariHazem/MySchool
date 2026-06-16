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

import { EmployeeRequestFormComponent } from '../employee-request-form/employee-request-form.component';
import { EmployeeRequestFilterDto, EmployeeRequestListItemDto, EmployeeRequestStatus } from '../employee-requests.models';
import { EmployeeRequestsService, readEmployeeRequestHttpError } from '../employee-requests.service';

@Component({
  selector: 'app-employee-requests-list',
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
    EmployeeRequestFormComponent,
  ],
  templateUrl: './employee-requests-list.component.html',
  styleUrl: './employee-requests-list.component.scss',
})
export class EmployeeRequestsListComponent implements OnInit {
  private readonly svc = inject(EmployeeRequestsService);
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
  rows: EmployeeRequestListItemDto[] = [];

  filter: EmployeeRequestFilterDto = {};

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

  get formDialogHeaderKey(): string {
    return this.editingRequestId != null && this.editingRequestId > 0
      ? 'employeeRequests.form.titleEdit'
      : 'employeeRequests.form.titleNew';
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

  ngOnInit(): void {
    this.statusOptions = [
      { label: this.translate.instant('employeeRequests.status.draft'), value: EmployeeRequestStatus.Draft },
      { label: this.translate.instant('employeeRequests.status.submitted'), value: EmployeeRequestStatus.Submitted },
      { label: this.translate.instant('employeeRequests.status.inApproval'), value: EmployeeRequestStatus.InApproval },
      { label: this.translate.instant('employeeRequests.status.approved'), value: EmployeeRequestStatus.Approved },
      { label: this.translate.instant('employeeRequests.status.rejected'), value: EmployeeRequestStatus.Rejected },
      { label: this.translate.instant('employeeRequests.status.inExecution'), value: EmployeeRequestStatus.InExecution },
      { label: this.translate.instant('employeeRequests.status.completed'), value: EmployeeRequestStatus.Completed },
      { label: this.translate.instant('employeeRequests.status.cancelled'), value: EmployeeRequestStatus.Cancelled },
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
    const f: EmployeeRequestFilterDto = { ...this.filter };
    if (f.status === null || f.status === undefined) delete f.status;
    this.svc
      .list(f)
      .pipe(finalize(() => (this.loading = false)))
      .subscribe({
        next: (r) => (this.rows = r ?? []),
        error: (e) => this.toastr.error(readEmployeeRequestHttpError(e)),
      });
  }

  statusLabel(v: number): string {
    const m: Record<number, string> = {
      [EmployeeRequestStatus.Draft]: 'employeeRequests.status.draft',
      [EmployeeRequestStatus.Submitted]: 'employeeRequests.status.submitted',
      [EmployeeRequestStatus.InApproval]: 'employeeRequests.status.inApproval',
      [EmployeeRequestStatus.Approved]: 'employeeRequests.status.approved',
      [EmployeeRequestStatus.Rejected]: 'employeeRequests.status.rejected',
      [EmployeeRequestStatus.InExecution]: 'employeeRequests.status.inExecution',
      [EmployeeRequestStatus.Completed]: 'employeeRequests.status.completed',
      [EmployeeRequestStatus.Cancelled]: 'employeeRequests.status.cancelled',
    };
    const key = m[v];
    return key ? this.translate.instant(key) : String(v);
  }

  typeLabel(row: EmployeeRequestListItemDto): string {
    const ar = row.requestTypeNameAr?.trim();
    if (ar) return `${ar} (${row.requestTypeCode})`;
    return `${row.requestTypeName} (${row.requestTypeCode})`;
  }
}
