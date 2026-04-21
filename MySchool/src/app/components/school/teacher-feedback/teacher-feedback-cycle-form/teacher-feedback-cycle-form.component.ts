import { AsyncPipe, DatePipe, NgFor, NgIf } from '@angular/common';
import { Component, EventEmitter, Input, OnInit, Output, inject } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Store } from '@ngrx/store';
import { selectLanguage } from 'app/core/store/language/language.selectors';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { ButtonModule } from 'primeng/button';
import { DatePicker } from 'primeng/datepicker';
import { FloatLabelModule } from 'primeng/floatlabel';
import { CheckboxModule } from 'primeng/checkbox';
import { InputTextModule } from 'primeng/inputtext';
import { ProgressSpinnerModule } from 'primeng/progressspinner';
import { Select } from 'primeng/select';
import { TextareaModule } from 'primeng/textarea';
import { ToastrService } from 'ngx-toastr';
import { HttpClient } from '@angular/common/http';
import { catchError, finalize, map, of } from 'rxjs';

import { isSchoolManagerUser } from 'app/core/utils/school-role.util';
import { PagePermission, PermissionService } from 'app/core/services/permission.service';
import { SchoolService } from 'app/core/services/school.service';
import { YearService } from 'app/core/services/year.service';
import { School } from 'app/core/models/school.modul';
import { Year } from 'app/core/models/year.model';
import { ShardModule } from 'app/shared/shard.module';
import { BackendAspService } from 'app/ASP.NET/backend-asp.service';
import { ApiResponse } from 'app/core/models/response.model';
import { PagedResultDto } from 'app/core/models/students.model';

import {
  FeedbackQuestionAudience,
  FeedbackQuestionType,
  FeedbackQuestionWriteDto,
  TeacherFeedbackCycleStatus,
  TeacherFeedbackCycleWriteDto,
} from '../teacher-feedback.models';
import { readTeacherFeedbackHttpError, TeacherFeedbackService } from '../teacher-feedback.service';

type QuestionRow = FeedbackQuestionWriteDto & { _key: number };

@Component({
  selector: 'app-teacher-feedback-cycle-form',
  standalone: true,
  imports: [
    ShardModule,
    NgIf,
    NgFor,
    FormsModule,
    TranslateModule,
    RouterLink,
    ButtonModule,
    Select,
    FloatLabelModule,
    InputTextModule,
    TextareaModule,
    CheckboxModule,
    ProgressSpinnerModule,
    DatePicker,
    DatePipe,
    AsyncPipe,
  ],
  templateUrl: './teacher-feedback-cycle-form.component.html',
  styleUrl: './teacher-feedback-cycle-form.component.scss',
})
export class TeacherFeedbackCycleFormComponent implements OnInit {
  @Input() embedded = false;
  @Input() cycleIdInput: number | null = null;
  @Input() presetSchoolId: number | null = null;

  @Output() closed = new EventEmitter<void>();
  @Output() saved = new EventEmitter<void>();

  private readonly svc = inject(TeacherFeedbackService);
  private readonly schoolService = inject(SchoolService);
  private readonly yearService = inject(YearService);
  private readonly http = inject(HttpClient);
  private readonly api = inject(BackendAspService);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly toastr = inject(ToastrService);
  private readonly translate = inject(TranslateService);
  private readonly perm = inject(PermissionService);
  private readonly store = inject(Store);

  readonly dir$ = this.store.select(selectLanguage).pipe(map((l) => (l === 'ar' ? 'rtl' : 'ltr')));

  readonly filterSelectPanelStyle: Record<string, string> = {
    maxWidth: 'min(22rem, calc(100vw - 2rem))',
  };

  loading = false;
  saving = false;
  cycleId: number | null = null;

  schoolID: number | null = null;
  academicYearID: number | null = null;
  teacherID: number | null = null;
  title = '';
  description = '';
  opensAt: Date | null = new Date();
  closesAt: Date | null = new Date(Date.now() + 7 * 86400000);
  status: TeacherFeedbackCycleStatus = TeacherFeedbackCycleStatus.Draft;

  schoolOptions: { label: string; value: number }[] = [];
  yearOptions: { label: string; value: number }[] = [];
  teacherOptions: { label: string; value: number }[] = [];
  allYears: Year[] = [];

  statusOptions: { label: string; value: number }[] = [];
  questionTypeOptions: { label: string; value: number }[] = [];
  audienceOptions: { label: string; value: number }[] = [];

  questions: QuestionRow[] = [];
  private nextQKey = 1;

  get isSchoolManager(): boolean {
    return isSchoolManagerUser();
  }

  get isEdit(): boolean {
    return this.cycleId != null && this.cycleId > 0;
  }

  get canSubmit(): boolean {
    return this.isEdit
      ? this.perm.hasPermission(PagePermission.Employees.Update)
      : this.perm.hasPermission(PagePermission.Employees.Create);
  }

  ngOnInit(): void {
    this.cycleId = this.embedded
      ? this.cycleIdInput != null && this.cycleIdInput > 0
        ? this.cycleIdInput
        : null
      : Number(this.route.snapshot.paramMap.get('id')) || null;

    if (!this.embedded) {
      if (this.cycleId && !this.perm.hasPermission(PagePermission.Employees.Update)) {
        this.router.navigate(['/school/teacher-feedback']).catch(() => undefined);
        return;
      }
      if (!this.cycleId && !this.perm.hasPermission(PagePermission.Employees.Create)) {
        this.router.navigate(['/school/teacher-feedback']).catch(() => undefined);
        return;
      }
    }

    this.statusOptions = [
      { label: this.translate.instant('teacherFeedback.status.draft'), value: TeacherFeedbackCycleStatus.Draft },
      { label: this.translate.instant('teacherFeedback.status.active'), value: TeacherFeedbackCycleStatus.Active },
      { label: this.translate.instant('teacherFeedback.status.closed'), value: TeacherFeedbackCycleStatus.Closed },
    ];
    this.questionTypeOptions = [
      { label: this.translate.instant('teacherFeedback.questionType.rating'), value: FeedbackQuestionType.Rating1To5 },
      { label: this.translate.instant('teacherFeedback.questionType.text'), value: FeedbackQuestionType.Text },
      { label: this.translate.instant('teacherFeedback.questionType.yesNo'), value: FeedbackQuestionType.YesNo },
    ];
    this.audienceOptions = [
      { label: this.translate.instant('teacherFeedback.audience.students'), value: FeedbackQuestionAudience.StudentsOnly },
      { label: this.translate.instant('teacherFeedback.audience.parents'), value: FeedbackQuestionAudience.ParentsOnly },
      { label: this.translate.instant('teacherFeedback.audience.both'), value: FeedbackQuestionAudience.Both },
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

    this.yearService.getAllYears().subscribe({
      next: (y) => {
        this.allYears = y ?? [];
        this.applyDefaultSchoolId();
        this.refreshYearOptions();
      },
      error: () => undefined,
    });

    this.loadTeachers();
    this.applyDefaultSchoolId();
    if (this.presetSchoolId != null && this.presetSchoolId > 0) this.schoolID = this.presetSchoolId;

    if (this.cycleId) this.loadCycle();
    else this.addQuestion();
  }

  private applyDefaultSchoolId(): void {
    if (this.schoolID != null && this.schoolID > 0) return;
    if (this.presetSchoolId != null && this.presetSchoolId > 0) {
      this.schoolID = this.presetSchoolId;
      return;
    }
    if (typeof localStorage === 'undefined') return;
    const raw = localStorage.getItem('schoolId');
    const n = raw != null && raw !== '' ? Number(raw) : NaN;
    if (Number.isFinite(n) && n > 0) this.schoolID = n;
  }

  onSchoolChange(): void {
    this.refreshYearOptions();
  }

  private refreshYearOptions(): void {
    const sid = this.schoolID;
    const list =
      sid != null && sid > 0 ? this.allYears.filter((y) => y.schoolID === sid) : [...this.allYears];
    this.yearOptions = list
      .slice()
      .sort((a, b) => b.yearID - a.yearID)
      .map((y) => ({
        label: (y.active ? '● ' : '') + String(y.yearID),
        value: y.yearID,
      }));
  }

  private loadTeachers(): void {
    this.http
      .post<ApiResponse<PagedResultDto<{ teacherID?: number; TeacherID?: number; fullName?: string; FullName?: string }>>>(
        `${this.api.baseUrl}/Teacher/names/page`,
        { pageIndex: 0, pageSize: 500, search: null },
      )
      .pipe(
        map((r) => {
          const b = r as unknown as Record<string, unknown>;
          const ok = (b['isSuccess'] ?? b['IsSuccess']) !== false;
          const errs = (b['errorMasseges'] ?? b['ErrorMasseges']) as string[] | undefined;
          if (!ok && errs?.length) throw new Error(errs.join('; '));
          const p = (b['result'] ?? b['Result']) as PagedResultDto<Record<string, unknown>>;
          const rows = p?.data ?? [];
          return rows.map((raw) => {
            const o = raw as Record<string, unknown>;
            const id = Number(o['teacherID'] ?? o['TeacherID']);
            const name = String(o['fullName'] ?? o['FullName'] ?? '');
            return { label: name || `#${id}`, value: id };
          });
        }),
        catchError(() => of([] as { label: string; value: number }[])),
      )
      .subscribe((opts) => (this.teacherOptions = opts.filter((x) => x.value > 0)));
  }

  private loadCycle(): void {
    if (!this.cycleId) return;
    this.loading = true;
    this.svc
      .getCycle(this.cycleId)
      .pipe(finalize(() => (this.loading = false)))
      .subscribe({
        next: (d) => {
          this.schoolID = d.schoolID;
          this.academicYearID = d.academicYearID;
          this.teacherID = d.teacherID;
          this.title = d.title;
          this.description = d.description ?? '';
          this.opensAt = d.opensAtUtc ? new Date(d.opensAtUtc) : new Date();
          this.closesAt = d.closesAtUtc ? new Date(d.closesAtUtc) : new Date();
          this.status = d.status as TeacherFeedbackCycleStatus;
          this.refreshYearOptions();
          this.questions = (d.questions ?? []).map((q) => ({
            _key: this.nextQKey++,
            feedbackQuestionID: q.feedbackQuestionID,
            sortOrder: q.sortOrder,
            questionText: q.questionText,
            questionType: q.questionType,
            audience: q.audience,
            isRequired: q.isRequired,
          }));
          if (this.questions.length === 0) this.addQuestion();
        },
        error: (e) => {
          this.toastr.error(readTeacherFeedbackHttpError(e));
          if (this.embedded) this.closed.emit();
          else this.router.navigate(['/school/teacher-feedback']).catch(() => undefined);
        },
      });
  }

  emptyQuestion(): QuestionRow {
    return {
      _key: this.nextQKey++,
      sortOrder: this.questions.length,
      questionText: '',
      questionType: FeedbackQuestionType.Rating1To5,
      audience: FeedbackQuestionAudience.Both,
      isRequired: true,
    };
  }

  addQuestion(): void {
    this.questions.push(this.emptyQuestion());
  }

  removeQuestion(i: number): void {
    this.questions.splice(i, 1);
    if (this.questions.length === 0) this.addQuestion();
  }

  cancel(): void {
    if (this.embedded) this.closed.emit();
    else this.router.navigate(['/school/teacher-feedback']).catch(() => undefined);
  }

  save(): void {
    if (!this.canSubmit) return;
    this.applyDefaultSchoolId();
    if (!this.schoolID || this.schoolID <= 0) {
      this.toastr.warning(this.translate.instant('teacherFeedback.form.validationSchool'));
      return;
    }
    if (!this.academicYearID || !this.teacherID || !this.title.trim()) {
      this.toastr.warning(this.translate.instant('teacherFeedback.form.validationRequired'));
      return;
    }
    if (!this.opensAt || !this.closesAt || this.closesAt < this.opensAt) {
      this.toastr.warning(this.translate.instant('teacherFeedback.form.validationDates'));
      return;
    }

    const dto: TeacherFeedbackCycleWriteDto = {
      schoolID: this.schoolID,
      academicYearID: this.academicYearID,
      teacherID: this.teacherID,
      title: this.title.trim(),
      description: this.description.trim() || null,
      opensAtUtc: this.opensAt.toISOString(),
      closesAtUtc: this.closesAt.toISOString(),
      status: this.status,
      questions: this.questions.map((q, i) => ({
        feedbackQuestionID: q.feedbackQuestionID,
        sortOrder: i,
        questionText: q.questionText.trim(),
        questionType: q.questionType,
        audience: q.audience,
        isRequired: q.isRequired,
      })),
    };

    const invalidQ = dto.questions?.some((q) => !q.questionText);
    if (invalidQ) {
      this.toastr.warning(this.translate.instant('teacherFeedback.form.validationQuestions'));
      return;
    }

    this.saving = true;
    const req$ =
      this.isEdit && this.cycleId
        ? this.svc.updateCycle(this.cycleId, dto)
        : this.svc.createCycle(dto).pipe(map(() => undefined));
    req$.pipe(finalize(() => (this.saving = false))).subscribe({
      next: () => {
        this.toastr.success(
          this.translate.instant(this.isEdit ? 'teacherFeedback.toast.updated' : 'teacherFeedback.toast.created'),
        );
        if (this.embedded) this.saved.emit();
        else this.router.navigate(['/school/teacher-feedback']).catch(() => undefined);
      },
      error: (e: unknown) => this.toastr.error(readTeacherFeedbackHttpError(e)),
    });
  }
}
