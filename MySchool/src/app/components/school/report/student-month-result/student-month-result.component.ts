// student-month-result.component.ts
import { Component, inject, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';

import { MonthlyResultService } from '../../core/services/monthly-resultt.service';
import { MonthlyResult } from '../../core/models/monthly-result.model';
import { ClassService } from '../../core/services/class.service';
import { Month } from '../../core/models/month.model';
import { DivisionService } from '../../core/services/division.service';
import { divisions } from '../../core/models/division.model';
import { PaginatorState } from 'primeng/paginator';
import { ToastrService } from 'ngx-toastr';

interface Term {
  termID: number;
  termName: string;
}

@Component({
  selector: 'app-student-month-result',
  templateUrl: `./student-month-result.component.html`,
  styleUrls: [`./student-month-result.component.scss`],
})
export class StudentMonthResultComponent implements OnInit {
  form: FormGroup = new FormGroup({});
  monthlyResults!: MonthlyResult[];
  private title = 'Student Month Result';
  AllClasses: any;
  filteredMonths: Month[] = [];
  divisions: divisions[] = [];
  fiteredDivisions: divisions[] = [];

  yearId = Number(localStorage.getItem('yearId'));
  divisionSerivce = inject(DivisionService);
  classService = inject(ClassService);
  monthlyResult = inject(MonthlyResultService);
  private toastr = inject(ToastrService);

  getAllMonthlyGradesReport(termId = 1, monthId = 5, classId = 1, divisionId = 1, studentId: number = 0): void {
    this.monthlyResult.getMonthlyGradesReport(this.yearId, termId, monthId, classId, divisionId, studentId).subscribe({
      next: (res) => {
        if (!res.isSuccess) {
          this.toastr.warning(res.errorMasseges[0] || 'Unexpected error');
          this.monthlyResults = [];
          return;
        }
        this.monthlyResults = res.result;
      },
      error: (err) => {
        this.toastr.error('Something went wrong');
        console.error(err);
      }
    });
  }
  terms: Term[] = [
    { termID: 1, termName: 'الاول' },
    { termID: 2, termName: 'الثاني' },
  ];
  months: Month[] = [
    { id: 5, name: 'مايو', termId: 1 },
    { id: 6, name: 'يونيو', termId: 1 },
    { id: 7, name: 'يوليو', termId: 1 },
    { id: 8, name: 'أغسطس', termId: 1 },
    { id: 9, name: 'سبتمبر', termId: 2 },
    { id: 10, name: 'أكتوبر', termId: 2 },
    { id: 11, name: 'نوفمبر', termId: 2 },
    { id: 12, name: 'ديسمبر', termId: 2 },
  ];

  getTitle(): string {
    return this.title;
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
      }
    })
  }
  SchoolLogo = localStorage.getItem('SchoolImageURL');
  ngOnInit(): void {
    this.getAllMonthlyGradesReport();
    this.getAllClasses();
    this.getAllDivision();
    this.form = new FormBuilder().group({
      termId: [, Validators.required],
      classId: [, Validators.required],
      studentId: [, Validators.required],
      monthId: [, Validators.required],
      divisionId: [, Validators.required]
    });
    this.form.get('termId')?.valueChanges.subscribe((termId: number) => {
      this.filteredMonths = this.months.filter(month => month.termId === termId);
    });
    this.form.get('classId')?.valueChanges.subscribe((classId: number) => {
      this.fiteredDivisions = this.divisions.filter(d => d.classID === classId);
    });
  }
  selectTerm(): void {
    const selectedTerm = this.form.get('termID')?.value;
    this.filteredMonths = this.months.filter(month => month.termId === selectedTerm);
  }
  getAllDivision(): void {
    this.divisionSerivce.GetAll().subscribe({
      next: (res) => {
        if (!res.isSuccess) {
          this.toastr.warning(res.errorMasseges[0] || 'Failed to load divisions.');
          return;
        }
        this.divisions = res.result;
      },
      error: () => this.toastr.error('Server error occurred')
    });
  }
  selectDivision(): void {
    const selectedDivision = this.form.get('divisionId')?.value;
    const selectedClass = this.form.get('classId')?.value;
    const selectedTerm = this.form.get('termId')?.value;
    const selectedMonth = this.form.get('monthId')?.value;
    // const selectedStudent = this.form.get('studentId')?.value;
    this.getAllMonthlyGradesReport(selectedTerm, selectedMonth, selectedClass, selectedDivision, 0);

  }
  printReport(subj: MonthlyResult): void {
    const page = document.getElementById(`report-${subj.studentID}`);
    if (!page) return;
    this.title = `${subj.studentName}_${subj.year}_${subj.month}_${subj.class}`;
    const links = Array.from(document.querySelectorAll('link[rel="stylesheet"], style'))
      .filter((el: Element) => el.getAttribute('href') !== 'assets/print.css')
      .map(el => el.outerHTML)
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
          @media print {
            body { margin: 0; direction: rtl; font-family: "Tajawal", "Arial", sans-serif; }
            .report, * { letter-spacing: 0 !important; }
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
