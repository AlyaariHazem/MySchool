import { Component, inject, OnInit } from '@angular/core';
import { FormBuilder } from '@angular/forms';
import { Store } from '@ngrx/store';
import { ToastrService } from 'ngx-toastr';
import { map } from 'rxjs';

import { selectLanguage } from '../../../core/store/language/language.selectors';
import { HomeworkService } from '../../../core/services/homework.service';
import {
  GuardianStudentHomeworkRow,
  homeworkStatusLabelAr,
  StudentHomeworkDetail,
} from '../../../core/models/homework.model';

@Component({
  selector: 'app-guardian-homework',
  templateUrl: './guardian-homework.component.html',
  styleUrls: ['./guardian-homework.component.scss'],
})
export class GuardianHomeworkComponent implements OnInit {
  private readonly homework = inject(HomeworkService);
  private readonly toastr = inject(ToastrService);
  private readonly fb = inject(FormBuilder);
  private readonly store = inject(Store);

  readonly dir$ = this.store.select(selectLanguage).pipe(map((l) => (l === 'ar' ? 'rtl' : 'ltr')));

  readonly pageSubtitle =
    'عرض مواعيد الاستحقاق وحالة التسليم لجميع الطلاب المرتبطين بحسابك، مع إمكانية فتح التفاصيل لكل واجب.';

  readonly filterOptions: { label: string; value: string }[] = [
    { label: 'الكل', value: 'all' },
    { label: 'اليوم', value: 'today' },
    { label: 'قادمة', value: 'upcoming' },
    { label: 'متأخرة', value: 'overdue' },
    { label: 'مكتملة', value: 'completed' },
    { label: 'معلقة', value: 'pending' },
  ];

  filterForm = this.fb.group({
    view: ['all'],
  });

  rows: GuardianStudentHomeworkRow[] = [];
  loading = false;
  statusLabel = homeworkStatusLabelAr;

  showDetail = false;
  detail: StudentHomeworkDetail | null = null;
  loadingDetail = false;
  detailStudentName = '';

  ngOnInit(): void {
    this.load();
  }

  get dialogTitle(): string {
    return this.detail?.title?.trim() ? this.detail!.title : 'تفاصيل الواجب';
  }

  statusClass(status: number): string {
    switch (status) {
      case 0:
        return 'hw-status--pending';
      case 1:
        return 'hw-status--submitted';
      case 2:
        return 'hw-status--late';
      case 3:
        return 'hw-status--graded';
      case 4:
        return 'hw-status--completed';
      case 5:
        return 'hw-status--missing';
      default:
        return 'hw-status--default';
    }
  }

  load(): void {
    this.loading = true;
    const view = this.filterForm.get('view')?.value ?? 'all';
    const f = view === 'all' ? undefined : view;
    this.homework.getGuardianAllTasks(f).subscribe({
      next: (r) => {
        this.rows = (r.result ?? []) as GuardianStudentHomeworkRow[];
        this.loading = false;
      },
      error: () => {
        this.loading = false;
        this.toastr.error('تعذر تحميل الواجبات');
      },
    });
  }

  openDetail(row: GuardianStudentHomeworkRow): void {
    this.showDetail = true;
    this.loadingDetail = true;
    this.detail = null;
    this.detailStudentName = row.studentName?.trim() || '—';
    this.homework.getGuardianStudentTaskDetail(row.studentID, row.homeworkTaskID).subscribe({
      next: (r) => {
        this.detail = (r.result ?? null) as StudentHomeworkDetail | null;
        this.loadingDetail = false;
      },
      error: () => {
        this.loadingDetail = false;
        this.toastr.error('تعذر التحميل');
      },
    });
  }
}
