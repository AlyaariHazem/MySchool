import { Component, inject, OnInit } from '@angular/core';
import { FormBuilder, FormControl, FormGroup } from '@angular/forms';
import { ToastrService } from 'ngx-toastr';
import { Store } from '@ngrx/store';
import { PaginatorState } from 'primeng/paginator';

import { ClassService } from '../../core/services/class.service';
import { CurriculmsPlanService } from '../../core/services/curriculms-plan.service';
import { CurriculmsPlanSubject } from '../../core/models/curriculmsPlans.model';
import { Paginates } from '../../core/models/Pagination.model';
import { TermGrades, TermlyGrade } from '../../core/models/term.model';
import { TermlyGradeService } from '../../core/services/termly-grade.service';

interface Term {
  name: string;
  id: number;
}


@Component({
  selector: 'app-grades-term',
  templateUrl: './grades-term.component.html',
  styleUrls: ['./grades-term.component.scss',
    './../../../../shared/styles/style-select.scss',
    './../../../../shared/styles/style-table.scss',
    '../../../../shared/styles/button.scss'
  ]
})
export class GradesTermComponent implements OnInit {
  form!: FormGroup;
  values = new FormControl<string[] | null>(null);
  max = 2;
  termlyGradeService = inject(TermlyGradeService);
  classService = inject(ClassService);

  monthlyGrades: TermGrades[] = [];
  displayedStudents: TermlyGrade[] = [];

  // Track the loading state
  isLoading = true;
  visible: boolean = true;

  langDir!: string;
  languageStore = inject(Store);
  dir: string = "ltr";
  currentLanguage(): void {
    this.languageStore.select("language").subscribe((res) => {
      this.langDir = res;
      console.log("the language is", this.langDir);
      this.dir = (res == "en") ? "ltr" : "rtl";
    });
  }
  currentStudentIndex = 0;
  CurrentStudent!: TermGrades; // Will be assigned in ngOnInit()

  currentPage = 0;
  pageSize = 5;
  length = 0;
  AllClasses: any;
  selectedTerm = 1;
  note = "";
  selectedClass = 1;
  selectedSubject = 1;

  constructor(
    private formBuilder: FormBuilder,
    private curriculmsPlanService: CurriculmsPlanService,
    private toastr: ToastrService,
  ) { }

  ngOnInit(): void {
    this.getAllClasses();
    this.getAllSubjects();
    // Initialize form
    this.getAllMonthlyGrades(1, 1, 1, 0);
    this.form = this.formBuilder.group({
      selectedClass: [this.selectedClass],
      selectedTerm: [this.selectedTerm],
      selectedSubject: [this.selectedSubject],
      note: [this.note],
    });
    this.currentLanguage();
    // Example data for dropdowns

    this.updatePaginatedData();
  }

  // Example data for dropdowns
  terms: Term[] = [
    { name: 'الأول', id: 1 },
    { name: 'الثاني', id: 2 }
  ];

  getAllClasses() {
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
    });
  }

  subjects: CurriculmsPlanSubject[] = [];
  getAllSubjects() {
    this.curriculmsPlanService.getAllCurriculmPlanSubjects().subscribe({
      next: (res) => {
        if (!res.isSuccess) {
          this.toastr.warning(res.errorMasseges[0] || 'Failed to load curriculum plans');
          return;
        }
        this.subjects = res.result;
      },
      error: () => {
        this.toastr.error('Error fetching curriculum plans');
      }
    });
  }
  goNextStudent(): void {
    if (this.currentStudentIndex < this.monthlyGrades.length - 1) {
      this.currentStudentIndex++;
      this.CurrentStudent = this.monthlyGrades[this.currentStudentIndex];
    }
  }

  goPreviousStudent(): void {
    if (this.currentStudentIndex > 0) {
      this.currentStudentIndex--;
      this.CurrentStudent = this.monthlyGrades[this.currentStudentIndex];
    }
  }

  selectClass(_: any): void {

    this.getAllMonthlyGrades(
      this.form.get('selectedTerm')?.value,
      this.yearID,
      this.form.get('selectedClass')?.value,
      this.form.get('selectedSubject')?.value
    );
    this.updatePaginatedData();

  }

  selectTerm(_: any): void {

    this.getAllMonthlyGrades(
      this.form.get('selectedTerm')?.value,
      this.yearID,
      this.form.get('selectedClass')?.value,
      this.form.get('selectedSubject')?.value
    );
    this.updatePaginatedData();

  }

  selectSubject(_: any): void {
    this.getAllMonthlyGrades(
      this.form.get('selectedTerm')?.value,
      this.yearID,
      this.form.get('selectedClass')?.value,
      this.form.get('selectedSubject')?.value
    );
    this.updatePaginatedData();

  }

  selectMonth(_: any): void {
    this.getAllMonthlyGrades(
      this.form.get('selectedTerm')?.value,
      this.yearID,
      this.form.get('selectedClass')?.value,
      this.form.get('selectedSubject')?.value
    );
    this.updatePaginatedData();

  }

  yearID: number = Number(localStorage.getItem('yearID') || '1');
  saveAllGrades() {
    if (!this.selectedTerm || !this.selectedClass) {
      alert('Please select term, month, class, and subject first.');
      return;
    }
    const payload: TermlyGrade[] = this.displayedStudents.flatMap(stu => ({
      studentID: stu.studentID,
      yearId: this.yearID,
      classID: this.form.get('selectedClass')?.value,
      termID: this.form.get('selectedTerm')?.value,
      grade: +stu.grade,
      note: stu.note,
      termlyGradeID: stu.termlyGradeID,
      subjectID: stu.subjectID
    })
    );

    console.log('the data are', payload);
    this.termlyGradeService.updateTermlyGrades(payload)
      .subscribe({
        next: res => {
          this.toastr.success('Grades saved successfully', res);
        },
        error: err => {
          console.error(err);
          this.toastr.error('Error occurred while saving');
        }
      });
  }

  hidden: boolean = false;
  toggleHidden() {
    this.hidden = !this.hidden;
  }
  first: number = 0;
  rows: number = 5;

  paginates!: Paginates;
  getAllMonthlyGrades(TermId: number, yearId: number, ClassId: number, SubjectId: number): void {
    this.termlyGradeService.getTermlyGradesReport(TermId, yearId, ClassId, SubjectId, this.first / this.rows + 1, this.rows).subscribe((res) => {
      this.paginates = res; // تأكد أن res يحتوي totalCount
      this.monthlyGrades = res.data;
      this.displayedStudents = res.data;
      console.log('monthly Grades are', this.monthlyGrades);
      this.isLoading = false;
      this.visible = false;
      if (this.monthlyGrades.length > 0) {
        this.CurrentStudent = this.monthlyGrades[this.currentStudentIndex];
      } else {
        this.CurrentStudent = null!;
      }
    });
  }

  updatePaginatedData(): void {
    this.visible = true;
    this.isLoading = true;
    this.termlyGradeService.getTermlyGradesReport(this.form.get('selectedTerm')?.value, this.yearID, this.form.get('selectedClass')?.value, this.form.get('selectedSubject')?.value, this.first / this.rows + 1, this.rows).subscribe((res) => {
      this.paginates = res;
      this.monthlyGrades = res.data;
      this.displayedStudents = res.data;
      this.isLoading = false;
      this.visible = false;
    });
  }

  // Handle page change event from PrimeNG paginator 
  onPageChange(event: PaginatorState): void {
    this.first = event.first || 0; // Update first index based on page
    this.rows = event.rows || 5; // Update rows per page
    this.updatePaginatedData();
  }

  clampGrade(g: TermGrades): void {

    let value = Number(g.grade);

    if (isNaN(value)) {
      value = 0;
      return;
    }
    this.monthlyGrades[this.currentStudentIndex].grade = g.grade;
    this.monthlyGrades[this.currentStudentIndex].note = g.note;
  }
  Delete(): void {
    this.toastr.info('it is not implemented yet.');
  }
}
