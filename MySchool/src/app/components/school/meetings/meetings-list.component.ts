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

import { MeetingFormComponent } from './meeting-form/meeting-form.component';
import { MeetingFilterDto, MeetingListItemDto, MeetingStatus } from './meetings.models';
import { MeetingsService, readMeetingHttpError } from './meetings.service';

@Component({
  selector: 'app-meetings-list',
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
    MeetingFormComponent,
  ],
  templateUrl: './meetings-list.component.html',
  styleUrl: './meetings-list.component.scss',
})
export class MeetingsListComponent implements OnInit {
  private readonly svc = inject(MeetingsService);
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
  rows: MeetingListItemDto[] = [];
  filter: MeetingFilterDto = {};
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
      { label: this.translate.instant('meetings.status.draft'), value: MeetingStatus.Draft },
      { label: this.translate.instant('meetings.status.scheduled'), value: MeetingStatus.Scheduled },
      { label: this.translate.instant('meetings.status.inProgress'), value: MeetingStatus.InProgress },
      { label: this.translate.instant('meetings.status.completed'), value: MeetingStatus.Completed },
      { label: this.translate.instant('meetings.status.cancelled'), value: MeetingStatus.Cancelled },
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
    const f: MeetingFilterDto = { ...this.filter };
    if (f.status === null || f.status === undefined) delete f.status;

    this.svc
      .listMeetings(f)
      .pipe(finalize(() => (this.loading = false)))
      .subscribe({
        next: (r) => (this.rows = r ?? []),
        error: (e) => this.toastr.error(readMeetingHttpError(e)),
      });
  }

  statusLabel(v: number): string {
    const m: Record<number, string> = {
      [MeetingStatus.Draft]: 'meetings.status.draft',
      [MeetingStatus.Scheduled]: 'meetings.status.scheduled',
      [MeetingStatus.InProgress]: 'meetings.status.inProgress',
      [MeetingStatus.Completed]: 'meetings.status.completed',
      [MeetingStatus.Cancelled]: 'meetings.status.cancelled',
    };
    const key = m[v];
    return key ? this.translate.instant(key) : String(v);
  }
}
