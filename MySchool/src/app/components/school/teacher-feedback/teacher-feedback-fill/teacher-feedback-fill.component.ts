import { DatePipe, NgFor, NgIf } from '@angular/common';
import { Component, OnInit, inject } from '@angular/core';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { Store } from '@ngrx/store';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { ButtonModule } from 'primeng/button';
import { InputNumberModule } from 'primeng/inputnumber';
import { ProgressSpinnerModule } from 'primeng/progressspinner';
import { Select } from 'primeng/select';
import { TextareaModule } from 'primeng/textarea';
import { map } from 'rxjs';
import { ToastrService } from 'ngx-toastr';
import { finalize } from 'rxjs/operators';

import { selectLanguage } from 'app/core/store/language/language.selectors';
import { ShardModule } from 'app/shared/shard.module';

import {
  FeedbackQuestionType,
  FeedbackResponseItemDto,
  FeedbackSubmissionStatus,
  ParentFeedbackSubmitDto,
  StudentFeedbackSubmitDto,
  TeacherFeedbackParticipantFormDto,
} from '../teacher-feedback.models';
import { readTeacherFeedbackHttpError, TeacherFeedbackService } from '../teacher-feedback.service';

@Component({
  selector: 'app-teacher-feedback-fill',
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
    TextareaModule,
    InputNumberModule,
    ProgressSpinnerModule,
    DatePipe,
  ],
  templateUrl: './teacher-feedback-fill.component.html',
  styleUrl: './teacher-feedback-fill.component.scss',
})
export class TeacherFeedbackFillComponent implements OnInit {
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly svc = inject(TeacherFeedbackService);
  private readonly toastr = inject(ToastrService);
  private readonly translate = inject(TranslateService);
  private readonly store = inject(Store);

  readonly dir$ = this.store.select(selectLanguage).pipe(map((l) => (l === 'ar' ? 'rtl' : 'ltr')));

  participant: 'student' | 'guardian' = 'student';
  cycleId = 0;
  guardianStudentId: number | null = null;

  loading = false;
  saving = false;
  form: TeacherFeedbackParticipantFormDto | null = null;

  /** Per questionId */
  answers: Record<number, { rating?: number | null; text?: string; yesNo?: boolean | null }> = {};

  readonly QuestionType = FeedbackQuestionType;

  yesNoOptions: { label: string; value: boolean }[] = [];

  ngOnInit(): void {
    this.participant = (this.route.parent?.snapshot.data['tfParticipant'] as 'student' | 'guardian') ?? 'student';
    this.cycleId = Number(this.route.snapshot.paramMap.get('cycleId'));
    this.guardianStudentId = Number(this.route.snapshot.queryParamMap.get('studentId')) || null;

    this.yesNoOptions = [
      { label: this.translate.instant('teacherFeedback.fill.yes'), value: true },
      { label: this.translate.instant('teacherFeedback.fill.no'), value: false },
    ];

    if (!this.cycleId) {
      this.toastr.error('Invalid cycle');
      this.goBack();
      return;
    }
    if (this.participant === 'guardian' && (!this.guardianStudentId || this.guardianStudentId <= 0)) {
      this.toastr.warning(this.translate.instant('teacherFeedback.portal.pickChildFirst'));
      this.goBack();
      return;
    }
    this.load();
  }

  private goBack(): void {
    const path = this.participant === 'guardian' ? ['/guardian', 'teacher-feedback'] : ['/students', 'teacher-feedback'];
    this.router.navigate(path).catch(() => undefined);
  }

  load(): void {
    this.loading = true;
    const req =
      this.participant === 'guardian' && this.guardianStudentId
        ? this.svc.parentCycleForm(this.cycleId, this.guardianStudentId)
        : this.svc.studentCycleForm(this.cycleId);
    req.pipe(finalize(() => (this.loading = false))).subscribe({
      next: (f) => {
        this.form = f;
        this.answers = {};
        for (const q of f.questions) {
          this.answers[q.feedbackQuestionID] = {};
        }
        for (const r of f.existingResponses ?? []) {
          this.answers[r.questionId] = {
            rating: r.rating ?? null,
            text: r.text ?? '',
            yesNo: r.yesNo ?? null,
          };
        }
      },
      error: (e) => {
        this.toastr.error(readTeacherFeedbackHttpError(e));
        this.goBack();
      },
    });
  }

  get isSubmitted(): boolean {
    return this.form != null && this.form.submissionStatus === FeedbackSubmissionStatus.Submitted;
  }

  private buildResponses(): FeedbackResponseItemDto[] {
    if (!this.form) return [];
    return this.form.questions.map((q) => {
      const a = this.answers[q.feedbackQuestionID] ?? {};
      return {
        questionId: q.feedbackQuestionID,
        rating: q.questionType === FeedbackQuestionType.Rating1To5 ? a.rating ?? null : null,
        text: q.questionType === FeedbackQuestionType.Text ? (a.text ?? '').trim() || null : null,
        yesNo: q.questionType === FeedbackQuestionType.YesNo ? (a.yesNo ?? null) : null,
      };
    });
  }

  saveDraft(): void {
    this.submit(false);
  }

  submitFinal(): void {
    this.submit(true);
  }

  private submit(submit: boolean): void {
    if (!this.form || this.isSubmitted) return;
    const responses = this.buildResponses();
    const dtoBase = { teacherFeedbackCycleID: this.cycleId, submit, responses };
    this.saving = true;
    const call =
      this.participant === 'guardian' && this.guardianStudentId
        ? this.svc.parentSubmit({ ...dtoBase, studentID: this.guardianStudentId } as ParentFeedbackSubmitDto)
        : this.svc.studentSubmit(dtoBase as StudentFeedbackSubmitDto);
    call.pipe(finalize(() => (this.saving = false))).subscribe({
      next: () => {
        this.toastr.success(this.translate.instant('teacherFeedback.fill.saved'));
        this.goBack();
      },
      error: (e) => this.toastr.error(readTeacherFeedbackHttpError(e)),
    });
  }
}
