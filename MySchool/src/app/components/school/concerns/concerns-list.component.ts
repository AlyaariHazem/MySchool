import { AsyncPipe, DatePipe, NgIf } from '@angular/common';
import { Component, OnInit, inject } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
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

import { ConcernFormComponent } from './concern-form/concern-form.component';
import {
  ComplaintListItemDto,
  ConcernFilterDto,
  ConcernKind,
  ConcernStatus,
  SuggestionListItemDto,
} from './concerns.models';
import { ConcernsService, readConcernHttpError } from './concerns.service';

@Component({
  selector: 'app-concerns-list',
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
    ConcernFormComponent,
  ],
  templateUrl: './concerns-list.component.html',
  styleUrl: './concerns-list.component.scss',
})
export class ConcernsListComponent implements OnInit {
  private readonly route = inject(ActivatedRoute);
  private readonly svc = inject(ConcernsService);
  private readonly schoolService = inject(SchoolService);
  private readonly toastr = inject(ToastrService);
  private readonly translate = inject(TranslateService);
  private readonly perm = inject(PermissionService);
  private readonly store = inject(Store);

  readonly dir$ = this.store.select(selectLanguage).pipe(map((l) => (l === 'ar' ? 'rtl' : 'ltr')));

  readonly filterSelectPanelStyle: Record<string, string> = {
    maxWidth: 'min(22rem, calc(100vw - 2rem))',
  };

  mode: ConcernKind = 'complaint';
  loading = false;
  complaintRows: ComplaintListItemDto[] = [];
  suggestionRows: SuggestionListItemDto[] = [];

  filter: ConcernFilterDto = {};

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

  get listTitleKey(): string {
    return this.mode === 'complaint' ? 'concerns.complaints.listTitle' : 'concerns.suggestions.listTitle';
  }

  get formDialogHeaderKey(): string {
    if (this.mode === 'complaint') {
      return this.editingRecordId != null && this.editingRecordId > 0
        ? 'concerns.complaints.formTitleEdit'
        : 'concerns.complaints.formTitleNew';
    }
    return this.editingRecordId != null && this.editingRecordId > 0
      ? 'concerns.suggestions.formTitleEdit'
      : 'concerns.suggestions.formTitleNew';
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

  rowIdComplaint(row: ComplaintListItemDto): number {
    return row.complaintID;
  }

  rowIdSuggestion(row: SuggestionListItemDto): number {
    return row.suggestionID;
  }

  categoryLabelComplaint(row: ComplaintListItemDto): string {
    const ar = row.categoryNameAr?.trim();
    if (ar) return `${ar} (${row.categoryCode})`;
    return `${row.categoryName} (${row.categoryCode})`;
  }

  categoryLabelSuggestion(row: SuggestionListItemDto): string {
    const ar = row.categoryNameAr?.trim();
    if (ar) return `${ar} (${row.categoryCode})`;
    return `${row.categoryName} (${row.categoryCode})`;
  }

  ngOnInit(): void {
    const kind = this.route.snapshot.data['concernKind'];
    this.mode = kind === 'suggestion' ? 'suggestion' : 'complaint';

    this.statusOptions = [
      { label: this.translate.instant('concerns.status.draft'), value: ConcernStatus.Draft },
      { label: this.translate.instant('concerns.status.submitted'), value: ConcernStatus.Submitted },
      { label: this.translate.instant('concerns.status.underReview'), value: ConcernStatus.UnderReview },
      { label: this.translate.instant('concerns.status.inProgress'), value: ConcernStatus.InProgress },
      { label: this.translate.instant('concerns.status.resolved'), value: ConcernStatus.Resolved },
      { label: this.translate.instant('concerns.status.rejected'), value: ConcernStatus.Rejected },
      { label: this.translate.instant('concerns.status.closed'), value: ConcernStatus.Closed },
      { label: this.translate.instant('concerns.status.cancelled'), value: ConcernStatus.Cancelled },
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
    const f: ConcernFilterDto = { ...this.filter };
    if (f.status === null || f.status === undefined) delete f.status;

    if (this.mode === 'complaint') {
      this.svc
        .listComplaints(f)
        .pipe(finalize(() => (this.loading = false)))
        .subscribe({
          next: (r) => (this.complaintRows = r ?? []),
          error: (e) => this.toastr.error(readConcernHttpError(e)),
        });
    } else {
      this.svc
        .listSuggestions(f)
        .pipe(finalize(() => (this.loading = false)))
        .subscribe({
          next: (r) => (this.suggestionRows = r ?? []),
          error: (e) => this.toastr.error(readConcernHttpError(e)),
        });
    }
  }

  statusLabel(v: number): string {
    const m: Record<number, string> = {
      [ConcernStatus.Draft]: 'concerns.status.draft',
      [ConcernStatus.Submitted]: 'concerns.status.submitted',
      [ConcernStatus.UnderReview]: 'concerns.status.underReview',
      [ConcernStatus.InProgress]: 'concerns.status.inProgress',
      [ConcernStatus.Resolved]: 'concerns.status.resolved',
      [ConcernStatus.Rejected]: 'concerns.status.rejected',
      [ConcernStatus.Closed]: 'concerns.status.closed',
      [ConcernStatus.Cancelled]: 'concerns.status.cancelled',
    };
    const key = m[v];
    return key ? this.translate.instant(key) : String(v);
  }
}
