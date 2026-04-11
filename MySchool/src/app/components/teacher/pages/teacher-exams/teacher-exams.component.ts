import { Component, inject, OnInit } from '@angular/core';
import { Store } from '@ngrx/store';
import { ToastrService } from 'ngx-toastr';
import { map } from 'rxjs';

import { selectLanguage } from 'app/core/store/language/language.selectors';
import { ExamsService } from '../../../../core/services/exams.service';
import { ExamResultRow, ScheduledExamList } from '../../../../core/models/exams.model';

@Component({
  selector: 'app-teacher-exams',
  templateUrl: './teacher-exams.component.html',
  styleUrls: ['./teacher-exams.component.scss'],
})
export class TeacherExamsComponent implements OnInit {
  private readonly exams = inject(ExamsService);
  private readonly toastr = inject(ToastrService);
  private readonly store = inject(Store);

  readonly dir$ = this.store.select(selectLanguage).pipe(map((l) => (l === 'ar' ? 'rtl' : 'ltr')));

  readonly pageSubtitle =
    'الامتحانات المجدولة للمواد والشعب التي تدرّستها.';

  list: ScheduledExamList[] = [];
  loading = false;
  selected: ScheduledExamList | null = null;
  results: ExamResultRow[] = [];
  loadingResults = false;

  ngOnInit(): void {
    this.load();
  }

  load(): void {
    this.loading = true;
    this.exams.getTeacherMy({}).subscribe({
      next: (r) => {
        this.list = (r.result ?? []) as ScheduledExamList[];
        this.loading = false;
      },
      error: () => {
        this.loading = false;
        this.toastr.error('تعذر تحميل الامتحانات');
      },
    });
  }

  openResults(row: ScheduledExamList): void {
    this.selected = row;
    this.loadingResults = true;
    this.exams.getResults(row.scheduledExamID).subscribe({
      next: (r) => {
        this.results = (r.result ?? []) as ExamResultRow[];
        this.loadingResults = false;
      },
      error: () => {
        this.loadingResults = false;
        this.toastr.error('تعذر تحميل الدرجات');
      },
    });
  }

  saveMarks(): void {
    if (!this.selected) return;
    this.exams.saveResults(this.selected.scheduledExamID, this.results).subscribe({
      next: () => this.toastr.success('تم حفظ الدرجات'),
      error: (e) => this.toastr.error(e?.error?.errorMasseges?.[0] ?? 'تعذر الحفظ'),
    });
  }

  publish(publish: boolean): void {
    if (!this.selected) return;
    this.exams.publishResults(this.selected.scheduledExamID, publish).subscribe({
      next: () => {
        this.toastr.success(publish ? 'تم نشر النتائج' : 'تم إلغاء النشر');
        this.load();
        if (this.selected) this.selected.resultsPublished = publish;
      },
      error: () => this.toastr.error('تعذر تحديث النشر'),
    });
  }

  closeResults(): void {
    this.selected = null;
    this.results = [];
  }
}
