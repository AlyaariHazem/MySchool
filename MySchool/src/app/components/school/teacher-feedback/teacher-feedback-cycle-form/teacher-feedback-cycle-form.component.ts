import { AsyncPipe } from '@angular/common';
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
import { finalize, map } from 'rxjs';

import { isSchoolManagerUser } from 'app/core/utils/school-role.util';
import { PagePermission, PermissionService } from 'app/core/services/permission.service';
import { SchoolService } from 'app/core/services/school.service';
import { YearService } from 'app/core/services/year.service';
import { School } from 'app/core/models/school.modul';
import { Year } from 'app/core/models/year.model';
import { ShardModule } from 'app/shared/shard.module';

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
        if (!this.isEdit) this.patchActiveAcademicYearFromSchool();
      },
      error: () => undefined,
    });

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
    this.patchActiveAcademicYearFromSchool();
  }

  /** Same rules as backend active year for a school: marked active, else latest YearID for that school. */
  private yearIsActive(y: Year): boolean {
    const raw = y as unknown as { active?: boolean; Active?: boolean };
    return !!(raw.active ?? raw.Active);
  }

  private yearSchoolId(y: Year): number {
    const raw = y as unknown as { schoolID?: number; SchoolID?: number };
    return raw.schoolID ?? raw.SchoolID ?? 0;
  }

  private yearIdNum(y: Year): number {
    const raw = y as unknown as { yearID?: number; YearID?: number };
    const n = raw.yearID ?? raw.YearID;
    return typeof n === 'number' && !Number.isNaN(n) ? n : 0;
  }

  private resolveActiveYearIdForSchool(schoolId: number | null | undefined): number | null {
    if (schoolId == null || schoolId <= 0) return null;
    const forSchool = this.allYears.filter((x) => this.yearSchoolId(x) === schoolId);
    const actives = forSchool.filter((x) => this.yearIsActive(x)).sort((a, b) => this.yearIdNum(a) - this.yearIdNum(b));
    if (actives.length) return this.yearIdNum(actives[0]);
    const latest = [...forSchool].sort((a, b) => this.yearIdNum(b) - this.yearIdNum(a));
    return latest.length ? this.yearIdNum(latest[0]) : null;
  }

  /** Sets academic year from year API (active year for school, else latest); clears when school is missing. */
  private patchActiveAcademicYearFromSchool(): void {
    this.academicYearID = this.resolveActiveYearIdForSchool(this.schoolID);
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
    if (!this.teacherID || !this.title.trim()) {
      this.toastr.warning(this.translate.instant('teacherFeedback.form.validationRequired'));
      return;
    }
    if (!this.isEdit) this.patchActiveAcademicYearFromSchool();
    if (!this.academicYearID || this.academicYearID <= 0) {
      this.toastr.warning(this.translate.instant('teacherFeedback.form.validationNoActiveYear'));
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
