import { NgFor, NgIf } from '@angular/common';
import { Component, EventEmitter, Input, OnInit, Output, inject } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { ButtonModule } from 'primeng/button';
import { FloatLabelModule } from 'primeng/floatlabel';
import { InputTextModule } from 'primeng/inputtext';
import { Select } from 'primeng/select';
import { TextareaModule } from 'primeng/textarea';
import { ToastrService } from 'ngx-toastr';
import { catchError, finalize, map, switchMap } from 'rxjs/operators';
import { forkJoin, Observable, of } from 'rxjs';

import { isSchoolManagerUser } from 'app/core/utils/school-role.util';
import { PagePermission, PermissionService } from 'app/core/services/permission.service';
import { SchoolService } from 'app/core/services/school.service';
import { School } from 'app/core/models/school.modul';
import { ShardModule } from 'app/shared/shard.module';

import { EmployeeProfileOptionDto, EmployeeProfilePageRequestDto } from '../../employees-hr/employees-hr.models';
import { EmployeesHrService } from '../../employees-hr/employees-hr.service';
import {
  MeetingAttendeeRole,
  MeetingAttendeeResponse,
  MeetingAttendeeWriteDto,
  MeetingDetailDto,
  MeetingMinutesWriteDto,
  MeetingStatus,
  MeetingTaskFollowUpWriteDto,
  MeetingTaskStatus,
  MeetingTaskWriteDto,
  MeetingWriteDto,
  datetimeLocalToIsoUtc,
  isoUtcToDatetimeLocal,
} from '../meetings.models';
import { MeetingsService, readMeetingHttpError } from '../meetings.service';

export interface AttendeeFormRow {
  employeeProfileID: number | null;
  role: MeetingAttendeeRole;
  response: MeetingAttendeeResponse;
  notes: string;
}

export interface FollowUpFormRow {
  note: string;
  progressPercent: number | null;
  authorEmployeeProfileID: number | null;
}

export interface TaskFormRow {
  title: string;
  details: string;
  assignedToEmployeeProfileID: number | null;
  dueAtLocal: string;
  status: MeetingTaskStatus;
  sortOrder: number;
  followUps: FollowUpFormRow[];
}

@Component({
  selector: 'app-meeting-form',
  standalone: true,
  imports: [
    ShardModule,
    NgIf,
    NgFor,
    FormsModule,
    TranslateModule,
    ButtonModule,
    Select,
    FloatLabelModule,
    InputTextModule,
    TextareaModule,
  ],
  templateUrl: './meeting-form.component.html',
  styleUrl: './meeting-form.component.scss',
})
export class MeetingFormComponent implements OnInit {
  @Input() embedded = false;
  @Input() recordIdInput: number | null = null;
  @Input() presetSchoolId: number | null = null;

  @Output() closed = new EventEmitter<void>();
  @Output() saved = new EventEmitter<void>();

  private readonly svc = inject(MeetingsService);
  private readonly employeesHr = inject(EmployeesHrService);
  private readonly schoolService = inject(SchoolService);
  private readonly toastr = inject(ToastrService);
  private readonly translate = inject(TranslateService);
  private readonly perm = inject(PermissionService);

  readonly filterSelectPanelStyle: Record<string, string> = {
    maxWidth: 'min(22rem, calc(100vw - 2rem))',
  };

  loading = false;
  saving = false;
  recordId: number | null = null;

  schoolID: number | null = null;
  organizerEmployeeProfileID: number | null = null;
  title = '';
  description = '';
  location = '';
  startAtLocal = '';
  endAtLocal = '';
  status: MeetingStatus = MeetingStatus.Draft;

  schoolOptions: { label: string; value: number }[] = [];
  employeeOptions: { label: string; value: number }[] = [];
  statusOptions: { label: string; value: number }[] = [];
  attendeeRoleOptions: { label: string; value: number }[] = [];
  attendeeResponseOptions: { label: string; value: number }[] = [];
  taskStatusOptions: { label: string; value: number }[] = [];

  attendeeRows: AttendeeFormRow[] = [];
  minutesBody = '';
  minutesRecordedByEmployeeProfileID: number | null = null;
  minutesApprovedByEmployeeProfileID: number | null = null;
  minutesApprovedAtLocal = '';

  taskRows: TaskFormRow[] = [];

  detail: MeetingDetailDto | null = null;

  get isSchoolManager(): boolean {
    return isSchoolManagerUser();
  }

  get isEdit(): boolean {
    return this.recordId != null && this.recordId > 0;
  }

  get canSubmit(): boolean {
    return this.isEdit ? this.perm.hasPermission(PagePermission.Employees.Update) : this.perm.hasPermission(PagePermission.Employees.Create);
  }

  ngOnInit(): void {
    this.recordId =
      this.embedded && this.recordIdInput != null && this.recordIdInput > 0 ? this.recordIdInput : null;

    this.statusOptions = [
      { label: this.translate.instant('meetings.status.draft'), value: MeetingStatus.Draft },
      { label: this.translate.instant('meetings.status.scheduled'), value: MeetingStatus.Scheduled },
      { label: this.translate.instant('meetings.status.inProgress'), value: MeetingStatus.InProgress },
      { label: this.translate.instant('meetings.status.completed'), value: MeetingStatus.Completed },
      { label: this.translate.instant('meetings.status.cancelled'), value: MeetingStatus.Cancelled },
    ];
    this.attendeeRoleOptions = [
      { label: this.translate.instant('meetings.attendeeRole.required'), value: MeetingAttendeeRole.Required },
      { label: this.translate.instant('meetings.attendeeRole.optional'), value: MeetingAttendeeRole.Optional },
    ];
    this.attendeeResponseOptions = [
      { label: this.translate.instant('meetings.attendeeResponse.pending'), value: MeetingAttendeeResponse.Pending },
      { label: this.translate.instant('meetings.attendeeResponse.accepted'), value: MeetingAttendeeResponse.Accepted },
      { label: this.translate.instant('meetings.attendeeResponse.declined'), value: MeetingAttendeeResponse.Declined },
      { label: this.translate.instant('meetings.attendeeResponse.tentative'), value: MeetingAttendeeResponse.Tentative },
    ];
    this.taskStatusOptions = [
      { label: this.translate.instant('meetings.taskStatus.open'), value: MeetingTaskStatus.Open },
      { label: this.translate.instant('meetings.taskStatus.inProgress'), value: MeetingTaskStatus.InProgress },
      { label: this.translate.instant('meetings.taskStatus.done'), value: MeetingTaskStatus.Done },
      { label: this.translate.instant('meetings.taskStatus.cancelled'), value: MeetingTaskStatus.Cancelled },
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

    this.applyDefaultSchoolId();
    if (this.embedded && this.presetSchoolId != null && this.presetSchoolId > 0) {
      this.schoolID = this.presetSchoolId;
    }

    if (this.recordId) {
      this.loadRecord();
    } else {
      this.loadEmployees();
      this.addAttendeeRow();
    }
  }

  onSchoolChange(): void {
    this.loadEmployees();
  }

  private applyDefaultSchoolId(): void {
    if (this.schoolID != null && this.schoolID > 0) return;
    if (typeof localStorage === 'undefined') return;
    const raw = localStorage.getItem('schoolId');
    const n = raw != null && raw !== '' ? Number(raw) : NaN;
    if (Number.isFinite(n) && n > 0) this.schoolID = n;
  }

  private loadEmployees(): void {
    const sid = this.schoolID;
    if (sid == null || sid <= 0) {
      this.employeeOptions = [];
      return;
    }
    const body: EmployeeProfilePageRequestDto = {
      pageIndex: 0,
      pageSize: 500,
      filter: { schoolID: sid },
    };
    this.employeesHr
      .getEmployeesPage(body)
      .pipe(
        map((p) => {
          const rows = p?.data ?? [];
          return rows.map((o: EmployeeProfileOptionDto) => ({
            label: this.displayNameFromOption(o),
            value: o.id,
          }));
        }),
        catchError(() => of([] as { label: string; value: number }[])),
      )
      .subscribe((opts) => (this.employeeOptions = opts.filter((x) => x.value > 0)));
  }

  private displayNameFromOption(o: EmployeeProfileOptionDto): string {
    const n = o.fullName;
    if (!n) return String(o.id);
    const parts = [n.firstName, n.middleName, n.lastName].filter((x) => !!x?.trim());
    return parts.length ? parts.join(' ') : String(o.id);
  }

  private loadRecord(): void {
    const id = this.recordId;
    if (id == null || id <= 0) return;
    this.loading = true;
    this.svc
      .getMeeting(id)
      .pipe(
        finalize(() => (this.loading = false)),
        catchError((e) => {
          this.toastr.error(readMeetingHttpError(e));
          return of(null);
        }),
      )
      .subscribe((d) => {
        if (!d) return;
        this.detail = d;
        this.schoolID = d.schoolID;
        this.organizerEmployeeProfileID = d.organizerEmployeeProfileID;
        this.title = d.title;
        this.description = d.description ?? '';
        this.location = d.location ?? '';
        this.startAtLocal = isoUtcToDatetimeLocal(d.startAtUtc);
        this.endAtLocal = d.endAtUtc ? isoUtcToDatetimeLocal(d.endAtUtc) : '';
        this.status = d.status as MeetingStatus;
        this.attendeeRows = (d.attendees ?? []).map((a) => ({
          employeeProfileID: a.employeeProfileID,
          role: a.role as MeetingAttendeeRole,
          response: a.response as MeetingAttendeeResponse,
          notes: a.notes ?? '',
        }));
        if (this.attendeeRows.length === 0) this.addAttendeeRow();
        const m = d.minutes;
        if (m) {
          this.minutesBody = m.body;
          this.minutesRecordedByEmployeeProfileID = m.recordedByEmployeeProfileID;
          this.minutesApprovedByEmployeeProfileID = m.approvedByEmployeeProfileID ?? null;
          this.minutesApprovedAtLocal = m.approvedAtUtc ? isoUtcToDatetimeLocal(m.approvedAtUtc) : '';
        } else {
          this.minutesBody = '';
          this.minutesRecordedByEmployeeProfileID = this.organizerEmployeeProfileID;
          this.minutesApprovedByEmployeeProfileID = null;
          this.minutesApprovedAtLocal = '';
        }
        this.taskRows = (d.tasks ?? []).map((t) => ({
          title: t.title,
          details: t.details ?? '',
          assignedToEmployeeProfileID: t.assignedToEmployeeProfileID ?? null,
          dueAtLocal: t.dueAtUtc ? isoUtcToDatetimeLocal(t.dueAtUtc) : '',
          status: t.status as MeetingTaskStatus,
          sortOrder: t.sortOrder,
          followUps: (t.followUps ?? []).map((f) => ({
            note: f.note,
            progressPercent: f.progressPercent ?? null,
            authorEmployeeProfileID: f.authorEmployeeProfileID ?? null,
          })),
        }));
        this.loadEmployees();
      });
  }

  addAttendeeRow(): void {
    this.attendeeRows.push({
      employeeProfileID: null,
      role: MeetingAttendeeRole.Required,
      response: MeetingAttendeeResponse.Pending,
      notes: '',
    });
  }

  removeAttendeeRow(i: number): void {
    this.attendeeRows.splice(i, 1);
    if (this.attendeeRows.length === 0) this.addAttendeeRow();
  }

  addTaskRow(): void {
    this.taskRows.push({
      title: '',
      details: '',
      assignedToEmployeeProfileID: null,
      dueAtLocal: '',
      status: MeetingTaskStatus.Open,
      sortOrder: this.taskRows.length,
      followUps: [],
    });
  }

  removeTaskRow(i: number): void {
    this.taskRows.splice(i, 1);
  }

  addFollowUp(task: TaskFormRow): void {
    task.followUps.push({ note: '', progressPercent: null, authorEmployeeProfileID: null });
  }

  removeFollowUp(task: TaskFormRow, i: number): void {
    task.followUps.splice(i, 1);
  }

  cancel(): void {
    this.closed.emit();
  }

  private buildAttendees(): MeetingAttendeeWriteDto[] {
    return this.attendeeRows
      .filter((r) => r.employeeProfileID != null && r.employeeProfileID > 0)
      .map((r) => ({
        employeeProfileID: r.employeeProfileID as number,
        role: r.role,
        response: r.response,
        notes: r.notes?.trim() ? r.notes.trim() : null,
      }));
  }

  private buildWriteDto(): MeetingWriteDto | null {
    const sid = this.schoolID;
    if (sid == null || sid <= 0) {
      this.toastr.warning(this.translate.instant('meetings.form.validationSchool'));
      return null;
    }
    const org = this.organizerEmployeeProfileID;
    if (org == null || org <= 0) {
      this.toastr.warning(this.translate.instant('meetings.form.validationOrganizer'));
      return null;
    }
    if (!this.title.trim()) {
      this.toastr.warning(this.translate.instant('meetings.form.validationTitle'));
      return null;
    }
    if (!this.startAtLocal) {
      this.toastr.warning(this.translate.instant('meetings.form.validationStart'));
      return null;
    }
    return {
      schoolID: sid,
      academicYearID: null,
      organizerEmployeeProfileID: org,
      title: this.title.trim(),
      description: this.description.trim() || null,
      location: this.location.trim() || null,
      startAtUtc: datetimeLocalToIsoUtc(this.startAtLocal),
      endAtUtc: this.endAtLocal ? datetimeLocalToIsoUtc(this.endAtLocal) : null,
      status: this.status,
      attendees: this.buildAttendees(),
    };
  }

  private buildMinutesDto(): MeetingMinutesWriteDto | null {
    if (!this.minutesBody.trim()) return null;
    const rec = this.minutesRecordedByEmployeeProfileID;
    if (rec == null || rec <= 0) {
      this.toastr.warning(this.translate.instant('meetings.form.validationMinutesRecorder'));
      return null;
    }
    return {
      body: this.minutesBody.trim(),
      recordedByEmployeeProfileID: rec,
      approvedByEmployeeProfileID: this.minutesApprovedByEmployeeProfileID,
      approvedAtUtc: this.minutesApprovedAtLocal ? datetimeLocalToIsoUtc(this.minutesApprovedAtLocal) : null,
    };
  }

  private buildTaskDtos(): MeetingTaskWriteDto[] {
    return this.taskRows
      .filter((t) => t.title.trim())
      .map((t) => {
        const followUps: MeetingTaskFollowUpWriteDto[] = (t.followUps ?? [])
          .filter((f) => f.note.trim())
          .map((f) => {
            const pct = f.progressPercent != null ? Number(f.progressPercent) : NaN;
            return {
              note: f.note.trim(),
              progressPercent: Number.isFinite(pct) && pct >= 0 ? pct : null,
              authorEmployeeProfileID: f.authorEmployeeProfileID,
            };
          });
        const so = typeof t.sortOrder === 'number' ? t.sortOrder : Number(t.sortOrder);
        return {
          title: t.title.trim(),
          details: t.details.trim() || null,
          assignedToEmployeeProfileID: t.assignedToEmployeeProfileID,
          dueAtUtc: t.dueAtLocal ? datetimeLocalToIsoUtc(t.dueAtLocal) : null,
          status: t.status,
          sortOrder: Number.isFinite(so) ? so : 0,
          followUps,
        };
      });
  }

  save(): void {
    const dto = this.buildWriteDto();
    if (!dto) return;
    this.saving = true;
    const afterId$ = this.isEdit
      ? this.svc.updateMeeting(this.recordId!, dto).pipe(map(() => this.recordId!))
      : this.svc.createMeeting(dto);

    afterId$
      .pipe(
        switchMap((meetingId) => {
          const minutes = this.buildMinutesDto();
          const tasks = this.buildTaskDtos();
          const parts: Observable<number>[] = [];
          if (minutes) parts.push(this.svc.upsertMinutes(meetingId, minutes));
          parts.push(this.svc.replaceTasks(meetingId, tasks));
          return forkJoin(parts).pipe(map(() => meetingId));
        }),
        finalize(() => (this.saving = false)),
        catchError((e) => {
          this.toastr.error(readMeetingHttpError(e));
          return of(null);
        }),
      )
      .subscribe((id) => {
        if (id == null) return;
        this.toastr.success(this.translate.instant(this.isEdit ? 'meetings.toast.updated' : 'meetings.toast.created'));
        this.saved.emit();
      });
  }
}
