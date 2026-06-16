import { Component, inject, OnInit } from '@angular/core';
import { FormBuilder } from '@angular/forms';
import { Store } from '@ngrx/store';
import { ToastrService } from 'ngx-toastr';
import { map } from 'rxjs';

import { ExamsService } from '../../../core/services/exams.service';
import { GuardianStudentExamCard } from '../../../core/models/exams.model';
import { selectLanguage } from '../../../core/store/language/language.selectors';

@Component({
  selector: 'app-guardian-exams',
  templateUrl: './guardian-exams.component.html',
  styleUrls: ['./guardian-exams.component.scss'],
})
export class GuardianExamsComponent implements OnInit {
  private readonly exams = inject(ExamsService);
  private readonly toastr = inject(ToastrService);
  private readonly fb = inject(FormBuilder);
  private readonly store = inject(Store);

  readonly dir$ = this.store.select(selectLanguage).pipe(map((l) => (l === 'ar' ? 'rtl' : 'ltr')));

  readonly filterOptions: { label: string; value: 'all' | 'upcoming' }[] = [
    { label: 'الكل', value: 'all' },
    { label: 'القادمة فقط', value: 'upcoming' },
  ];

  filterForm = this.fb.group({
    view: ['all' as 'all' | 'upcoming'],
  });

  rows: GuardianStudentExamCard[] = [];
  loading = false;

  ngOnInit(): void {
    this.load();
  }

  load(): void {
    this.loading = true;
    const upcomingOnly = this.filterForm.get('view')?.value === 'upcoming';
    this.exams.getGuardianMy(upcomingOnly).subscribe({
      next: (r) => {
        this.rows = (r.result ?? []) as GuardianStudentExamCard[];
        this.loading = false;
      },
      error: () => {
        this.loading = false;
        this.toastr.error('تعذر تحميل الامتحانات');
      },
    });
  }

  resultLabel(row: GuardianStudentExamCard): string {
    if (!row.resultsPublished) {
      return 'غير منشور';
    }
    if (row.isAbsent) {
      return 'غائب';
    }
    return row.passed ? 'ناجح' : 'راسب';
  }

  scoreCell(row: GuardianStudentExamCard): string {
    if (!row.resultsPublished) {
      return '—';
    }
    if (row.isAbsent) {
      return 'غائب';
    }
    const s = row.score;
    return s != null ? `${s} / ${row.totalMarks}` : '—';
  }
}
