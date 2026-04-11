import { ChangeDetectorRef, Component, OnInit, inject } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ToastrService } from 'ngx-toastr';
import { debounceTime, distinctUntilChanged } from 'rxjs';

import { MonthlyResult } from '../../core/models/monthly-result.model';
import { MonthlyResultService } from '../../core/services/monthly-resultt.service';
import { MonthService } from '../../core/services/month.service';
import { ClassService } from '../../core/services/class.service';
import { DivisionService } from '../../core/services/division.service';
import { StudentService } from '../../../../core/services/student.service';
import { MonthDto } from '../../core/models/month.model';
import { TERMS } from '../../core/data/terms';
import { ITerm } from '../../core/models/term.model';
import { divisions } from '../../core/models/division.model';

type StudentRow = { studentID: number; displayName: string; classID: number; divisionID: number };

@Component({
  selector: 'app-term-result',
  templateUrl: './term-result.component.html',
  styleUrls: ['./term-result.component.scss']
})
export class TermResultComponent implements OnInit {
  private readonly cdr = inject(ChangeDetectorRef);
  private readonly fb = inject(FormBuilder);
  private readonly monthlyResult = inject(MonthlyResultService);
  private readonly monthService = inject(MonthService);
  private readonly classService = inject(ClassService);
  private readonly divisionService = inject(DivisionService);
  private readonly studentService = inject(StudentService);
  private readonly toastr = inject(ToastrService);

  monthlyReports: MonthlyResult[] = [];
  printing = false;

  terms: ITerm[] = TERMS;
  months: MonthDto[] = [];
  filteredMonths: MonthDto[] = [];
  classes: { classID: number; className: string }[] = [];
  divisions: divisions[] = [];
  filteredDivisions: divisions[] = [];
  studentOptions: StudentRow[] = [];

  subjectNames: string[] = [];
  loading = false;

  form: FormGroup = this.fb.group({
    termId: [1, Validators.required],
    monthId: [null as number | null, Validators.required],
    classId: [null as number | null, Validators.required],
    divisionId: [null as number | null, Validators.required],
    studentId: [0]
  });

  print(): void {
    const styleId = 'term-result-print-page';
    let styleEl = document.getElementById(styleId) as HTMLStyleElement | null;
    if (!styleEl) {
      styleEl = document.createElement('style');
      styleEl.id = styleId;
      document.head.appendChild(styleEl);
    }
    styleEl.textContent =
      '@media print { @page { size: A4 landscape; margin: 5mm; } }';

    this.printing = true;
    this.cdr.detectChanges();

    const cleanup = (): void => {
      this.printing = false;
      this.cdr.detectChanges();
      styleEl?.remove();
      window.removeEventListener('afterprint', cleanup);
    };
    window.addEventListener('afterprint', cleanup);

    requestAnimationFrame(() => {
      window.print();
    });
  }

  ngOnInit(): void {
    this.loadMonths();
    this.loadClasses();
    this.loadDivisions();

    this.form.get('termId')?.valueChanges.subscribe((termId: number) => {
      this.applyMonthFilter(termId);
    });

    this.form.get('classId')?.valueChanges.subscribe((classId: number) => {
      this.filteredDivisions = this.divisions.filter((d) => d.classID === classId);
      const divId = this.form.get('divisionId')?.value;
      if (!this.filteredDivisions.some((d) => d.divisionID === divId)) {
        const first = this.filteredDivisions[0]?.divisionID ?? null;
        this.form.patchValue({ divisionId: first, studentId: 0 }, { emitEvent: false });
      }
      this.loadStudentOptions();
    });

    this.form.get('divisionId')?.valueChanges.subscribe(() => {
      this.loadStudentOptions();
    });

    this.form.valueChanges
      .pipe(
        debounceTime(150),
        distinctUntilChanged((a, b) => JSON.stringify(a) === JSON.stringify(b))
      )
      .subscribe(() => this.loadReport());
  }

  private loadMonths(): void {
    this.monthService.getAllMonths().subscribe({
      next: (res) => {
        if (!res.isSuccess || !res.result?.length) {
          this.toastr.warning(res.errorMasseges?.[0] || 'تعذر تحميل قائمة الأشهر.');
          this.months = [];
          this.filteredMonths = [];
          return;
        }
        this.months = res.result;
        this.applyMonthFilter(Number(this.form.get('termId')?.value));
        if (this.filteredMonths.length && this.form.get('monthId')?.value == null) {
          this.form.patchValue({ monthId: this.filteredMonths[0].monthID }, { emitEvent: true });
        }
      },
      error: () => this.toastr.error('خطأ في تحميل الأشهر')
    });
  }

  private applyMonthFilter(termId: number): void {
    this.filteredMonths = this.months.filter(
      (m) => m.termID == null || m.termID === termId
    );
    const current = this.form.get('monthId')?.value;
    if (!this.filteredMonths.some((m) => m.monthID === current)) {
      const first = this.filteredMonths[0]?.monthID ?? null;
      this.form.patchValue({ monthId: first }, { emitEvent: true });
    }
  }

  private loadClasses(): void {
    this.classService.GetAllNames().subscribe({
      next: (res) => {
        if (!res.isSuccess || !res.result?.length) {
          this.toastr.warning(res.errorMasseges?.[0] || 'تعذر تحميل الصفوف.');
          return;
        }
        this.classes = res.result as { classID: number; className: string }[];
        const first = this.classes[0]?.classID ?? null;
        this.form.patchValue({ classId: first }, { emitEvent: true });
      },
      error: () => this.toastr.error('خطأ في تحميل الصفوف')
    });
  }

  private loadDivisions(): void {
    this.divisionService.GetAll().subscribe({
      next: (res) => {
        if (!res.isSuccess || !res.result) {
          return;
        }
        this.divisions = res.result;
        const classId = Number(this.form.get('classId')?.value);
        this.filteredDivisions = this.divisions.filter((d) => d.classID === classId);
        const firstDiv = this.filteredDivisions[0]?.divisionID ?? null;
        if (this.form.get('divisionId')?.value == null) {
          this.form.patchValue({ divisionId: firstDiv }, { emitEvent: true });
        }
        this.loadStudentOptions();
      },
      error: () => this.toastr.error('خطأ في تحميل الشعب')
    });
  }

  private loadStudentOptions(): void {
    const classId = Number(this.form.get('classId')?.value);
    const divisionId = Number(this.form.get('divisionId')?.value);
    if (!classId || !divisionId) {
      this.studentOptions = [];
      return;
    }
    this.studentService.getStudentsPageForAttendance(1, 500, { classId, divisionId }).subscribe({
      next: (page) => {
        const rows = page.data ?? [];
        const mapped: StudentRow[] = rows.map((s) => ({
          studentID: s.studentID,
          displayName: [s.fullName?.firstName, s.fullName?.middleName, s.fullName?.lastName]
            .filter(Boolean)
            .join(' ')
            .replace(/\s+/g, ' ')
            .trim(),
          classID: classId,
          divisionID: divisionId
        }));
        this.studentOptions = [
          { studentID: 0, displayName: 'جميع الطلاب', classID: classId, divisionID: divisionId },
          ...mapped
        ];
      },
      error: () => {
        this.studentOptions = [];
      }
    });
  }

  private loadReport(): void {
    const termId = Number(this.form.get('termId')?.value);
    const monthId = Number(this.form.get('monthId')?.value);
    const classId = Number(this.form.get('classId')?.value);
    const divisionId = Number(this.form.get('divisionId')?.value);
    const studentId = Number(this.form.get('studentId')?.value ?? 0);

    if (!termId || !monthId || !classId || !divisionId) {
      this.monthlyReports = [];
      this.subjectNames = [];
      return;
    }

    this.loading = true;
    this.monthlyResult.getMonthlyGradesReport(termId, monthId, classId, divisionId, studentId).subscribe({
      next: (res) => {
        this.loading = false;
        if (!res.isSuccess) {
          this.toastr.warning(res.errorMasseges?.[0] || 'لا توجد بيانات للمعايير المحددة.');
          this.monthlyReports = [];
          this.subjectNames = [];
          return;
        }
        this.monthlyReports = res.result ?? [];
        this.prepareHeaders();
      },
      error: () => {
        this.loading = false;
        this.toastr.error('تعذر تحميل التقرير');
        this.monthlyReports = [];
        this.subjectNames = [];
      }
    });
  }

  private prepareHeaders(): void {
    this.subjectNames = [
      ...new Set(this.monthlyReports.flatMap((r) => (r.gradeSubjects ?? []).map((s) => s.subjectName)))
    ];
  }

  getGradeForSubject(row: MonthlyResult, subjectName: string): number | string {
    const g = row.gradeSubjects?.find((s) => s.subjectName === subjectName)?.grade;
    return g ?? '';
  }

  /** Average of subject grades (same idea as monthly student report). */
  getAverage(row: MonthlyResult): string {
    const total = row.grade;
    const n = row.gradeSubjects?.length ?? 0;
    if (total == null || !n) {
      return '';
    }
    return (Number(total) / n).toFixed(2);
  }
}
