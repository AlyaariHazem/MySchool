import { Component, DestroyRef, inject, OnInit } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { ActivatedRoute, Data } from '@angular/router';
import { FormBuilder } from '@angular/forms';
import { Store } from '@ngrx/store';
import { ToastrService } from 'ngx-toastr';
import { map } from 'rxjs';

import { MonthlyGradesService } from '../../school/core/services/monthly-grades.service';
import { GuardianMonthlyGradeRow } from '../../school/core/models/MonthlyGrade.model';
import { TermService } from '../../school/core/services/term.service';
import { YearService } from '../../../core/services/year.service';
import { MONTHS } from '../../school/core/data/months';
import { IMonth } from '../../school/core/models/month.model';
import { Terms } from '../../school/core/models/term.model';
import { Year } from '../../../core/models/year.model';
import { selectLanguage } from '../../../core/store/language/language.selectors';

@Component({
  selector: 'app-guardian-monthly-reports',
  templateUrl: './guardian-monthly-reports.component.html',
  styleUrls: ['./guardian-monthly-reports.component.scss'],
})
export class GuardianMonthlyReportsComponent implements OnInit {
  private readonly monthlyGrades = inject(MonthlyGradesService);
  private readonly termsApi = inject(TermService);
  private readonly yearsApi = inject(YearService);
  private readonly toastr = inject(ToastrService);
  private readonly fb = inject(FormBuilder);
  private readonly store = inject(Store);
  private readonly route = inject(ActivatedRoute);
  private readonly destroyRef = inject(DestroyRef);

  readonly dir$ = this.store.select(selectLanguage).pipe(map((l) => (l === 'ar' ? 'rtl' : 'ltr')));

  /** Shown in header; overridden by route `data.pageTitle` (grades/month vs reports/monthly). */
  pageTitle = 'تقارير شهرية';
  pageSubtitle =
    'الدرجات الشهرية المسجّلة لأبنائك حسب السنة والفصل والشهر (اترك الحقول فارغة لعرض كل ما يخص السنة النشطة).';

  readonly months: IMonth[] = MONTHS;

  termList: Terms[] = [];
  yearList: Year[] = [];
  /** Labels for year dropdown (p-select). */
  yearOptions: { label: string; yearID: number }[] = [];

  filterForm = this.fb.group({
    yearID: [null as number | null],
    termID: [null as number | null],
    monthID: [null as number | null],
  });

  rows: GuardianMonthlyGradeRow[] = [];
  loading = false;

  /** When true (route `reports/monthly`), show printable monthly certificate for guardian children. */
  showPrintableCertificate = false;

  ngOnInit(): void {
    this.applyPageMeta(this.route.snapshot.data);
    this.showPrintableCertificate = this.route.snapshot.data['showPrintableCertificate'] === true;
    this.route.data.pipe(takeUntilDestroyed(this.destroyRef)).subscribe((d) => {
      this.applyPageMeta(d);
      this.showPrintableCertificate = d['showPrintableCertificate'] === true;
    });

    this.yearsApi.getAllYears().subscribe({
      next: (years) => {
        this.yearList = years ?? [];
        this.yearOptions = this.yearList.map((y) => ({
          label: String(new Date(y.yearDateStart).getFullYear()),
          yearID: y.yearID,
        }));
        const active = this.yearList.find((y) => y.active);
        this.filterForm.patchValue({ yearID: active?.yearID ?? null });
        this.load();
      },
      error: () => {
        this.load();
      },
    });

    this.termsApi.getAllTerm().subscribe({
      next: (r) => {
        this.termList = r.result ?? [];
      },
      error: () => this.toastr.error('تعذر تحميل الفصول'),
    });
  }

  private applyPageMeta(d: Data): void {
    const title = d['pageTitle'];
    if (typeof title === 'string' && title.trim()) {
      this.pageTitle = title.trim();
    }
    const sub = d['pageSubtitle'];
    if (typeof sub === 'string' && sub.trim()) {
      this.pageSubtitle = sub.trim();
    }
  }

  load(): void {
    const v = this.filterForm.getRawValue();
    const yearId = v.yearID ?? undefined;
    const termId = v.termID ?? undefined;
    const monthId = v.monthID ?? undefined;

    this.loading = true;
    this.monthlyGrades.getGuardianMy(yearId, termId, monthId).subscribe({
      next: (r) => {
        this.rows = (r.result ?? []) as GuardianMonthlyGradeRow[];
        this.loading = false;
      },
      error: () => {
        this.loading = false;
        this.toastr.error('تعذر تحميل التقارير الشهرية');
      },
    });
  }
}
