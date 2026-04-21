import { DatePipe, NgFor, NgIf } from '@angular/common';
import { Component, OnInit, inject } from '@angular/core';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { Store } from '@ngrx/store';
import { TranslateModule } from '@ngx-translate/core';
import { ButtonModule } from 'primeng/button';
import { CardModule } from 'primeng/card';
import { FloatLabelModule } from 'primeng/floatlabel';
import { ProgressSpinnerModule } from 'primeng/progressspinner';
import { Select } from 'primeng/select';
import { map } from 'rxjs';
import { ToastrService } from 'ngx-toastr';

import { selectLanguage } from 'app/core/store/language/language.selectors';
import { StudentService } from 'app/core/services/student.service';
import { ShardModule } from 'app/shared/shard.module';

import { TeacherFeedbackOpenCycleDto } from '../teacher-feedback.models';
import { readTeacherFeedbackHttpError, TeacherFeedbackService } from '../teacher-feedback.service';

@Component({
  selector: 'app-teacher-feedback-portal',
  standalone: true,
  imports: [
    ShardModule,
    NgIf,
    NgFor,
    FormsModule,
    TranslateModule,
    RouterLink,
    ButtonModule,
    CardModule,
    FloatLabelModule,
    Select,
    ProgressSpinnerModule,
    DatePipe,
  ],
  templateUrl: './teacher-feedback-portal.component.html',
  styleUrl: './teacher-feedback-portal.component.scss',
})
export class TeacherFeedbackPortalComponent implements OnInit {
  private readonly route = inject(ActivatedRoute);
  private readonly svc = inject(TeacherFeedbackService);
  private readonly studentService = inject(StudentService);
  private readonly toastr = inject(ToastrService);
  private readonly store = inject(Store);

  readonly dir$ = this.store.select(selectLanguage).pipe(map((l) => (l === 'ar' ? 'rtl' : 'ltr')));

  participant: 'student' | 'guardian' = 'student';
  loading = false;
  cycles: TeacherFeedbackOpenCycleDto[] = [];

  childOptions: { label: string; value: number }[] = [];
  selectedStudentId: number | null = null;

  ngOnInit(): void {
    this.participant = (this.route.parent?.snapshot.data['tfParticipant'] as 'student' | 'guardian') ?? 'student';
    if (this.participant === 'guardian') this.loadGuardianChildrenThenCycles();
    else this.loadStudentCycles();
  }

  private loadStudentCycles(): void {
    this.loading = true;
    this.svc.studentOpenCycles().subscribe({
      next: (rows) => {
        this.cycles = rows ?? [];
        this.loading = false;
      },
      error: (e) => {
        this.toastr.error(readTeacherFeedbackHttpError(e));
        this.loading = false;
      },
    });
  }

  private loadGuardianChildrenThenCycles(): void {
    this.loading = true;
    this.studentService.getGuardianMyChildrenForReport().subscribe({
      next: (res) => {
        if (!res.isSuccess) {
          this.toastr.warning(res.errorMasseges?.[0] ?? '');
          this.childOptions = [];
          this.loading = false;
          return;
        }
        const raw = (res.result ?? []) as Record<string, unknown>[];
        this.childOptions = raw.map((r) => ({
          label: String(r['displayName'] ?? r['DisplayName'] ?? ''),
          value: Number(r['studentID'] ?? r['StudentID'] ?? 0),
        }));
        if (this.childOptions.length === 1) this.selectedStudentId = this.childOptions[0].value;
        this.loadGuardianCycles();
      },
      error: () => {
        this.toastr.error(readTeacherFeedbackHttpError(new Error('network')));
        this.loading = false;
      },
    });
  }

  private loadGuardianCycles(): void {
    this.svc.parentOpenCycles().subscribe({
      next: (rows) => {
        this.cycles = rows ?? [];
        this.loading = false;
      },
      error: (e) => {
        this.toastr.error(readTeacherFeedbackHttpError(e));
        this.loading = false;
      },
    });
  }

  linkFor(c: TeacherFeedbackOpenCycleDto): string[] {
    return this.participant === 'guardian'
      ? ['/guardian', 'teacher-feedback', 'fill', String(c.teacherFeedbackCycleID)]
      : ['/students', 'teacher-feedback', 'fill', String(c.teacherFeedbackCycleID)];
  }

  guardianQueryParams(): Record<string, string> | null {
    if (this.participant !== 'guardian') return null;
    if (this.selectedStudentId == null || this.selectedStudentId <= 0) return null;
    return { studentId: String(this.selectedStudentId) };
  }
}
