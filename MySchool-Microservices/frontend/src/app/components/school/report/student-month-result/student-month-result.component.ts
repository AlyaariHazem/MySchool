// student-month-result.component.ts
import { Component, inject, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ToastrService } from 'ngx-toastr';

import { MonthlyResultService } from '../../core/services/monthly-resultt.service';
import { MonthlyResult } from '../../core/models/monthly-result.model';
import { ClassService } from '../../core/services/class.service';
import { IMonth } from '../../core/models/month.model';
import { DivisionService } from '../../core/services/division.service';
import { divisions } from '../../core/models/division.model';
import { PaginatorState } from 'primeng/paginator';
import { ITerm } from '../../core/models/term.model';
import { MONTHS } from '../../core/data/months';
import { TERMS } from '../../core/data/terms';
import { StudentService } from '../../../../core/services/student.service';

type StudentSelectRow = {
  studentID: number;
  displayName: string;
  classID: number;
  divisionID: number;
};

@Component({
  selector: 'app-student-month-result',
  templateUrl: `./student-month-result.component.html`,
  styleUrls: [`./student-month-result.component.scss`],
})
export class StudentMonthResultComponent implements OnInit {
  form: FormGroup = new FormGroup({});
  monthlyResults: MonthlyResult[] = [];
  private title = 'Student Month Result';
  AllClasses: any[] | undefined;
  filteredMonths: IMonth[] = [];
  divisions: divisions[] = [];
  fiteredDivisions: divisions[] = [];
  terms: ITerm[] = TERMS;
  months: IMonth[] = MONTHS;

  /** Rows for طالب dropdown (guardian: own children only; staff: division roster + «جميع الطلاب»). */
  studentSelectOptions: StudentSelectRow[] = [];

  readonly isGuardian =
    typeof localStorage !== 'undefined' && localStorage.getItem('userType') === 'GUARDIAN';

  divisionSerivce = inject(DivisionService);
  classService = inject(ClassService);
  monthlyResult = inject(MonthlyResultService);
  private studentService = inject(StudentService);
  private toastr = inject(ToastrService);

  SchoolLogo = localStorage.getItem('SchoolImageURL');
  ngOnInit(): void {
    this.getAllClasses();
    this.getAllDivision();
    this.form = new FormBuilder().group({
      termId: [1, Validators.required],
      classId: [this.isGuardian ? 0 : 1],
      studentId: [0],
      monthId: [6, Validators.required],
      divisionId: [this.isGuardian ? 0 : 1],
    });

    const termCtrl = this.form.get('termId');
    this.filteredMonths = this.months.filter((m) => m.termId === termCtrl?.value);
    termCtrl?.valueChanges.subscribe((termId: number) => {
      this.filteredMonths = this.months.filter((month) => month.termId === termId);
    });

    if (!this.isGuardian) {
      this.form.get('classId')?.valueChanges.subscribe((classId: number) => {
        this.fiteredDivisions = this.divisions.filter((d) => d.classID === classId);
      });
      this.form.get('divisionId')?.valueChanges.subscribe(() => {
        this.loadStaffStudentOptions();
      });
    }

    this.form.get('studentId')?.valueChanges.subscribe((id: number) => {
      if (this.isGuardian && id) {
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

    if (this.isGuardian) {
      this.loadGuardianStudentOptions();
    }
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

  private loadStaffStudentOptions(): void {
    const classId = Number(this.form.get('classId')?.value);
    const divisionId = Number(this.form.get('divisionId')?.value);
    if (!classId || !divisionId) {
      this.studentSelectOptions = [];
      return;
    }
    this.studentService.getStudentsPageForAttendance(1, 500, { classId, divisionId }).subscribe({
      next: (page) => {
        const rows = page.data ?? [];
        const mapped: StudentSelectRow[] = rows.map((s) => ({
          studentID: s.studentID,
          displayName: [s.fullName?.firstName, s.fullName?.middleName, s.fullName?.lastName]
            .filter(Boolean)
            .join(' ')
            .replace(/\s+/g, ' ')
            .trim(),
          classID: classId,
          divisionID: divisionId,
        }));
        this.studentSelectOptions = [
          {
            studentID: 0,
            displayName: 'جميع الطلاب',
            classID: classId,
            divisionID: divisionId,
          },
          ...mapped,
        ];
      },
      error: (err) => {
        console.error(err);
        this.toastr.error('تعذر تحميل قائمة الطلاب');
        this.studentSelectOptions = [];
      },
    });
  }

  getAllMonthlyGradesReport(termId = 1, monthId = 5, classId = 1, divisionId = 1, studentId: number = 0): void {
    this.monthlyResult.getMonthlyGradesReport(termId, monthId, classId, divisionId, studentId).subscribe({
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

  getAllClasses(): void {
    this.classService.GetAllNames().subscribe({
      next: (res) => {
        if (!res.isSuccess) {
          this.toastr.warning(res.errorMasseges[0] || 'Failed to load classes.');
          return;
        }
        this.AllClasses = res.result;
      },
      error: (err) => {
        this.toastr.error('Server error occurred');
        console.error(err);
      },
    });
  }
  getAllDivision(): void {
    this.divisionSerivce.GetAll().subscribe({
      next: (res) => {
        if (!res.isSuccess) {
          this.toastr.warning(res.errorMasseges[0] || 'Failed to load divisions.');
          return;
        }
        this.divisions = res.result;
        if (!this.isGuardian) {
          const cid = this.form.get('classId')?.value;
          this.fiteredDivisions = this.divisions.filter((d) => d.classID === cid);
          this.loadStaffStudentOptions();
        }
      },
      error: () => this.toastr.error('Server error occurred'),
    });
  }
  getAllGrades(): void {
    const selectedDivision = this.form.get('divisionId')?.value;
    const selectedClass = this.form.get('classId')?.value;
    const selectedTerm = this.form.get('termId')?.value;
    const selectedMonth = this.form.get('monthId')?.value;
    const selectedStudent = this.form.get('studentId')?.value ?? 0;

    if (this.isGuardian) {
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
      return;
    }

    const ready =
      selectedTerm != null &&
      selectedTerm !== '' &&
      selectedMonth != null &&
      selectedMonth !== '' &&
      selectedClass != null &&
      selectedClass !== '' &&
      selectedDivision != null &&
      selectedDivision !== '';

    if (!ready) {
      this.monthlyResults = [];
      this.first = 0;
      return;
    }

    this.getAllMonthlyGradesReport(
      Number(selectedTerm),
      Number(selectedMonth),
      Number(selectedClass),
      Number(selectedDivision),
      Number(selectedStudent)
    );
  }

  get pagedMonthlyResults(): MonthlyResult[] {
    return this.monthlyResults.slice(this.first, this.first + this.rows);
  }
  printReport(subj: MonthlyResult): void {
    console.log('Printing report for:', subj);
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
          /* Override app global html,body{height:100%} — otherwise print layout reserves a full viewport and Chrome adds a blank second page. */
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
  // Handle page change event from PrimeNG paginator 
  onPageChange(event: PaginatorState): void {
    this.first = event.first || 0; // Update first index based on page
    this.rows = event.rows || 5; // Update rows per page
    // this.updatePaginatedData();
  }

}
