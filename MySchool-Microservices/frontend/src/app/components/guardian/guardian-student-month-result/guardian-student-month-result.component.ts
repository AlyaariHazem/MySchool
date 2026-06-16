import { Component, inject, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ToastrService } from 'ngx-toastr';

import { MonthlyResultService } from '../../school/core/services/monthly-resultt.service';
import { MonthlyResult } from '../../school/core/models/monthly-result.model';
import { IMonth } from '../../school/core/models/month.model';
import { PaginatorState } from 'primeng/paginator';
import { ITerm } from '../../school/core/models/term.model';
import { MONTHS } from '../../school/core/data/months';
import { TERMS } from '../../school/core/data/terms';
import { StudentService } from '../../../core/services/student.service';

type StudentSelectRow = {
  studentID: number;
  displayName: string;
  classID: number;
  divisionID: number;
};

/** Printable monthly certificate for guardians only (children list + report). */
@Component({
  selector: 'app-guardian-student-month-result',
  templateUrl: './guardian-student-month-result.component.html',
  styleUrls: ['./guardian-student-month-result.component.scss'],
})
export class GuardianStudentMonthResultComponent implements OnInit {
  form: FormGroup = new FormGroup({});
  monthlyResults: MonthlyResult[] = [];
  private title = 'Student Month Result';
  filteredMonths: IMonth[] = [];
  terms: ITerm[] = TERMS;
  months: IMonth[] = MONTHS;

  studentSelectOptions: StudentSelectRow[] = [];

  monthlyResult = inject(MonthlyResultService);
  private studentService = inject(StudentService);
  private toastr = inject(ToastrService);

  SchoolLogo = localStorage.getItem('SchoolImageURL');

  ngOnInit(): void {
    this.form = new FormBuilder().group({
      termId: [1, Validators.required],
      classId: [0],
      studentId: [0],
      monthId: [6, Validators.required],
      divisionId: [0],
    });

    const termCtrl = this.form.get('termId');
    this.filteredMonths = this.months.filter((m) => m.termId === termCtrl?.value);
    termCtrl?.valueChanges.subscribe((termId: number) => {
      this.filteredMonths = this.months.filter((month) => month.termId === termId);
    });

    this.form.get('studentId')?.valueChanges.subscribe((id: number) => {
      if (id) {
        const row = this.studentSelectOptions.find((s) => s.studentID === id);
        if (row) {
          this.form.patchValue(
            { classId: row.classID, divisionId: row.divisionID },
            { emitEvent: false }
          );
        }
      }
      this.getAllGrades();
    });

    this.loadGuardianStudentOptions();
  }

  private loadGuardianStudentOptions(): void {
    this.studentService.getGuardianMyChildrenForReport().subscribe({
      next: (res) => {
        if (!res.isSuccess) {
          this.toastr.warning(res.errorMasseges[0] || 'تعذر تحميل قائمة الأبناء.');
          this.studentSelectOptions = [];
          return;
        }
        const raw = (res.result ?? []) as Record<string, unknown>[];
        this.studentSelectOptions = raw.map((r) => ({
          studentID: Number(r['studentID'] ?? r['StudentID'] ?? 0),
          displayName: String(r['displayName'] ?? r['DisplayName'] ?? ''),
          classID: Number(r['classID'] ?? r['ClassID'] ?? 0),
          divisionID: Number(r['divisionID'] ?? r['DivisionID'] ?? 0),
        }));
        if (this.studentSelectOptions.length === 1) {
          const only = this.studentSelectOptions[0];
          this.form.patchValue({
            studentId: only.studentID,
            classId: only.classID,
            divisionId: only.divisionID,
          });
        }
      },
      error: (err) => {
        console.error(err);
        this.toastr.error('خطأ في تحميل قائمة الطلاب');
        this.studentSelectOptions = [];
      },
    });
  }

  getAllMonthlyGradesReport(termId = 1, monthId = 5, classId = 1, divisionId = 1, studentId: number = 0): void {
    this.monthlyResult
      .getMonthlyGradesReport(termId, monthId, classId, divisionId, studentId)
      .subscribe({
        next: (res) => {
          if (!res.isSuccess) {
            this.toastr.warning(res.errorMasseges[0] || 'Unexpected error');
            this.monthlyResults = [];
            this.first = 0;
            return;
          }
          this.monthlyResults = res.result;
          this.first = 0;
        },
        error: (err) => {
          this.toastr.warning('لا يوجد طلاب في هذا!');
          console.error(err);
          this.monthlyResults = [];
          this.first = 0;
        },
      });
  }

  getAllGrades(): void {
    const selectedDivision = this.form.get('divisionId')?.value;
    const selectedClass = this.form.get('classId')?.value;
    const selectedTerm = this.form.get('termId')?.value;
    const selectedMonth = this.form.get('monthId')?.value;
    const selectedStudent = this.form.get('studentId')?.value ?? 0;

    if (!selectedTerm || selectedTerm === '' || !selectedMonth || selectedMonth === '') {
      this.monthlyResults = [];
      this.first = 0;
      return;
    }
    if (!Number(selectedStudent)) {
      this.monthlyResults = [];
      this.first = 0;
      return;
    }
    const selectedClassNum = Number(selectedClass);
    const selectedDivisionNum = Number(selectedDivision);
    if (!selectedClassNum || !selectedDivisionNum) {
      this.monthlyResults = [];
      this.first = 0;
      return;
    }
    this.getAllMonthlyGradesReport(
      Number(selectedTerm),
      Number(selectedMonth),
      selectedClassNum,
      selectedDivisionNum,
      Number(selectedStudent)
    );
  }

  get pagedMonthlyResults(): MonthlyResult[] {
    return this.monthlyResults.slice(this.first, this.first + this.rows);
  }

  printReport(subj: MonthlyResult): void {
    const page = document.getElementById(`report-${subj.studentID}`);
    if (!page) return;
    this.title = `${subj.studentName}_${subj.year}_${subj.month}_${subj.class}`;
    const links = Array.from(document.querySelectorAll('link[rel="stylesheet"], style'))
      .filter((el: Element) => el.getAttribute('href') !== 'assets/print.css')
      .map((el) => el.outerHTML)
      .join('');
    const base = `<base href="${document.baseURI}">`;

    const popup = window.open('', '', 'width=1000px,height=auto');
    if (!popup) return;

    popup.document.write(`
    <html>
      <head>
        <title>${this.title}</title>
        ${base}
        ${links}
        <style>
          html,
          body {
            height: auto !important;
            min-height: 0 !important;
            max-height: none !important;
            margin: 0 !important;
            padding: 0 !important;
          }
          @media print {
            html,
            body {
              height: auto !important;
              min-height: 0 !important;
              max-height: none !important;
              overflow: visible !important;
            }
            body {
              margin: 0;
              direction: rtl;
              font-family: "Tajawal", "Arial", sans-serif;
            }
            .report {
              margin: 0 !important;
              margin-top: 0 !important;
              page-break-after: avoid !important;
              break-after: avoid !important;
            }
            .d-print-none {
              display: none !important;
            }
            .report,
            * {
              letter-spacing: 0 !important;
            }
          }
        </style>
      </head>
      <body>
        ${page.outerHTML}
      </body>
    </html>
  `);

    popup.document.close();
    popup.onload = () => popup.print();
  }

  first: number = 0;
  rows: number = 5;

  onPageChange(event: PaginatorState): void {
    this.first = event.first || 0;
    this.rows = event.rows || 5;
  }
}
