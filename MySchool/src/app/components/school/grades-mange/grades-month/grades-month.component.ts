import { Component, inject, OnInit } from '@angular/core';
import { FormBuilder, FormControl, FormGroup } from '@angular/forms';
import { ToastrService } from 'ngx-toastr';
import { Store } from '@ngrx/store';
import { PaginatorState } from 'primeng/paginator';

import { MonthlyGradesService } from '../../core/services/monthly-grades.service';
import { MonthlyGrade, updateMonthlyGrades } from '../../core/models/MonthlyGrade.model';
import { ClassService } from '../../core/services/class.service';
import { CurriculmsPlanService } from '../../core/services/curriculms-plan.service';
import { CurriculmsPlanSubject } from '../../core/models/curriculmsPlans.model';
import { Paginates } from '../../core/models/Pagination.model';
import { Month } from '../../core/models/month.model';

interface Term {
  name: string;
  id: number;
}


@Component({
  selector: 'app-grades-month',
  templateUrl: './grades-month.component.html',
  styleUrls: [
    './grades-month.component.scss',
    './../../../../shared/styles/style-select.scss',
    './../../../../shared/styles/style-table.scss',
    '../../../../shared/styles/button.scss'
  ]
})
export class GradesMonthComponent implements OnInit {
  form!: FormGroup;
  values = new FormControl<string[] | null>(null);
  max = 2;
  monthlyGradesService = inject(MonthlyGradesService);
  classService = inject(ClassService);

  monthlyGrades: MonthlyGrade[] = [];
  displayedStudents: MonthlyGrade[] = [];
  filteredMonths: Month[] = [];

  // Track the loading state
  isLoading = true;
  visible: boolean = true

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
  CurrentStudent!: MonthlyGrade; // Will be assigned in ngOnInit()

  currentPage = 0;
  pageSize = 5;
  length = 0;
  AllClasses: any;
  selectedTerm = 1;
  selectedMonth = 6;
  selectedClass = 1;
  selectedSubject = 1;

  constructor(
    private formBuilder: FormBuilder,
    private curriculmsPlanService: CurriculmsPlanService,
    private toastr: ToastrService,
  ) { }

  ngOnInit(): void {
    this.getAllClasses();
    this.getAllCurriculm();
    // Initialize form
    this.getAllMonthlyGrades(1, 6, 1, 0);
    this.form = this.formBuilder.group({
      selectedClass: [this.selectedClass],
      selectedTerm: [this.selectedTerm],
      selectedSubject: [this.selectedSubject],
      selectedMonth: [this.selectedMonth],
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

  curriculmsPlan: CurriculmsPlanSubject[] = [];
  getAllCurriculm() {
    this.curriculmsPlanService.getAllCurriculmPlanSubjects().subscribe({
      next: (res) => {
        if (!res.isSuccess) {
          this.toastr.warning(res.errorMasseges[0] || 'Failed to load curriculums.');
          return;
        }
        this.curriculmsPlan = res.result;
      },
      error: (err) => {
        this.toastr.error('Server error occurred');
        console.error(err);
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

  private filterMonthsByTerm(): void {
    this.filteredMonths = this.months.filter(m => m.termId === this.form.get('selectedTerm')?.value);
  }

  selectClass(_: any): void {

    this.getAllMonthlyGrades(
      this.form.get('selectedTerm')?.value,
      this.form.get('selectedMonth')?.value,
      this.form.get('selectedClass')?.value,
      this.form.get('selectedSubject')?.value
    );
    this.updatePaginatedData();
    this.filterMonthsByTerm();
  }

  selectTerm(_: any): void {
    this.filterMonthsByTerm();
    this.getAllMonthlyGrades(
      this.form.get('selectedTerm')?.value,
      this.form.get('selectedMonth')?.value,
      this.form.get('selectedClass')?.value,
      this.form.get('selectedSubject')?.value
    );
    this.updatePaginatedData();
    this.filterMonthsByTerm();
  }

  selectSubject(_: any): void {
    this.getAllMonthlyGrades(
      this.form.get('selectedTerm')?.value,
      this.form.get('selectedMonth')?.value,
      this.form.get('selectedClass')?.value,
      this.form.get('selectedSubject')?.value
    );
    this.updatePaginatedData();
    this.filterMonthsByTerm();
  }

  selectMonth(_: any): void {
    this.getAllMonthlyGrades(
      this.form.get('selectedTerm')?.value,
      this.form.get('selectedMonth')?.value,
      this.form.get('selectedClass')?.value,
      this.form.get('selectedSubject')?.value
    );
    this.updatePaginatedData();
    this.filterMonthsByTerm();
  }

  yearID: number = Number(localStorage.getItem('yearID') || '1');
  saveAllGrades() {
    if (!this.selectedTerm || !this.selectedMonth || !this.selectedClass) {
      alert('Please select term, month, class, and subject first.');
      return;
    }
    const payload: updateMonthlyGrades[] = this.displayedStudents.flatMap(stu =>
      stu.grades.map(g => ({
        studentID: stu.studentID,
        subjectID: stu.subjectID,
        yearID: this.yearID,
        monthID: this.form.get('selectedMonth')?.value,
        classID: this.form.get('selectedClass')?.value,
        termID: this.form.get('selectedTerm')?.value,
        gradeTypeID: g.gradeTypeID,
        grade: +g.maxGrade
      }))
    );

    console.log('the data are', payload);
    this.monthlyGradesService.updateMonthlyGrades(payload)
      .subscribe({
        next: _ => {
          this.toastr.success('Grades saved successfully');
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
  getAllMonthlyGrades(TermId: number, MonthId: number, ClassId: number, SubjectId: number): void {
    this.monthlyGradesService.getAllMonthlyGrades(TermId, MonthId, ClassId, SubjectId, this.first / this.rows + 1, this.rows).subscribe((res) => {
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
    this.monthlyGradesService.getAllMonthlyGrades(this.form.get('selectedTerm')?.value, this.form.get('selectedMonth')?.value, this.form.get('selectedClass')?.value, this.form.get('selectedSubject')?.value, this.first / this.rows + 1, this.rows).subscribe((res) => {
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
  // ثابت يحتوى الحدود حسب gradeTypeID
  gradeLimits: { [typeId: number]: number } = {
    1: 20, // واجبات
    2: 20, // مواظبة
    3: 10, // مشارك
    4: 10, // شفهي
    5: 40  // تحرير
  };

  // تُستدعى عند كل تغيّر فى حقل الدرجة
  clampGrade(g: { gradeTypeID: number, maxGrade: any }): void {
    const limit = this.gradeLimits[g.gradeTypeID] ?? 100;
    let value = Number(g.maxGrade);

    if (isNaN(value)) {
      g.maxGrade = 0;
      return;
    }

    // إذا كانت أكبر من الحد، قصّها تلقائيًا
    if (value > limit) {
      g.maxGrade = limit;
    } else if (value < 0) {
      g.maxGrade = 0;
    }
    // this.monthlyGrades[this.currentStudentIndex].grades[this.currentStudentIndex].maxGrade = g.maxGrade;
  }
  enforceLimit(evt: Event, g: { gradeTypeID: number; maxGrade: any }): void {
    const input = evt.target as HTMLInputElement;
    const limit = this.gradeLimits[g.gradeTypeID] ?? 100;

    // احصل على الرقم الحالى (قد يكون فارغاً)
    let val = Number(input.value);
    if (isNaN(val)) { val = 0; }

    // قصّ إلى الحدّ
    if (val > limit) val = limit;
    if (val < 0) val = 0;

    // حدِّث نموذج البيانات وواجهة المستخدم فوراً
    g.maxGrade = val;
    input.value = String(val);   // يُجبر الـ input على إظهار الرقم المقصوص
  }

  calcTotal(grades: { maxGrade: any }[]): number {
    return grades.reduce((sum, g) => sum + (+g.maxGrade || 0), 0);
  }

  calcPercent(grades: { maxGrade: any }[]): string {
    const total = this.calcTotal(grades);
    return total.toString();
  }

  Delete(): void {
    this.toastr.info('it is not implemented yet.');
  }
}
