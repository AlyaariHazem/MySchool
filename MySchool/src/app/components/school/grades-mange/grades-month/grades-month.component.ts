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
import { IMonth } from '../../core/models/month.model';
import { ITerm } from '../../core/models/term.model';
import { TERMS } from '../../core/data/terms';
import { MONTHS } from '../../core/data/months';


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
  filteredMonths: IMonth[] = [];
  terms: ITerm[] = TERMS;
  months: IMonth[] = MONTHS;

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
  private classesLoaded = false;
  private subjectsLoaded = false;

  constructor(
    private formBuilder: FormBuilder,
    private curriculmsPlanService: CurriculmsPlanService,
    private toastr: ToastrService,
  ) { }

  ngOnInit(): void {
    // Initialize form with null values first, will be set after data loads
    this.form = this.formBuilder.group({
      selectedClass: [null],
      selectedTerm: [this.selectedTerm],
      selectedSubject: [null],
      selectedMonth: [this.selectedMonth],
    });
    this.currentLanguage();
    
    // Initialize filtered months based on selected term
    this.filterMonthsByTerm();
    
    // Load classes and subjects first, then set form values
    // The form values will be set in getAllClasses() and getAllCurriculm() callbacks
    // and updatePaginatedData() will be called from there
    this.getAllClasses();
    this.getAllCurriculm();
  }


  getAllClasses() {
    this.classService.GetAllNames().subscribe({
      next: (res) => {
        if (!res.isSuccess) {
          this.toastr.warning(res.errorMasseges[0] || 'Failed to load classes.');
          return;
        }
        this.AllClasses = res.result;
        this.classesLoaded = true;
        
        // Set the form value after classes are loaded
        // Find the class with ID matching selectedClass, or use first class if available
        if (this.AllClasses && this.AllClasses.length > 0) {
          const defaultClass = this.AllClasses.find((c: any) => c.classID === this.selectedClass) || this.AllClasses[0];
          if (this.form && defaultClass) {
            this.form.patchValue({ selectedClass: defaultClass.classID });
            this.selectedClass = defaultClass.classID;
          }
        }
        
        // Only call updatePaginatedData once when both classes and subjects are loaded
        if (this.subjectsLoaded) {
          this.updatePaginatedData();
        }
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
        // this.curriculmsPlan = res.result;
        this.curriculmsPlan = [{ subjectID: 0, subjectName: 'الكل' }, ...res.result];
        this.subjectsLoaded = true;
        
        // Set the form value after subjects are loaded
        // Find the subject with ID matching selectedSubject, or use first subject if available
        if (this.curriculmsPlan && this.curriculmsPlan.length > 0) {
          const defaultSubject = this.curriculmsPlan.find((s: any) => s.subjectID === this.selectedSubject) || this.curriculmsPlan[0];
          if (this.form && defaultSubject) {
            this.form.patchValue({ selectedSubject: defaultSubject.subjectID });
            this.selectedSubject = defaultSubject.subjectID;
          }
        }
        
        // Only call updatePaginatedData once when both classes and subjects are loaded
        if (this.classesLoaded) {
          this.updatePaginatedData();
        }
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
    const selectedTerm = this.form?.get('selectedTerm')?.value ?? this.selectedTerm;
    this.filteredMonths = this.months.filter(m => m.termId === selectedTerm);
    
    // If current selected month is not in filtered months, reset to first available month
    if (this.filteredMonths.length > 0) {
      const currentMonth = this.form?.get('selectedMonth')?.value ?? this.selectedMonth;
      const monthExists = this.filteredMonths.some(m => m.id === currentMonth);
      if (!monthExists && this.form) {
        this.form.patchValue({ selectedMonth: this.filteredMonths[0].id });
        this.selectedMonth = this.filteredMonths[0].id;
      }
    }
  }

  selectClass(_: any): void {
    const classId = this.form.get('selectedClass')?.value ?? this.selectedClass;
    
    // Update component properties
    this.selectedClass = classId;
    
    // Only call updatePaginatedData (it will make the API call)
    this.updatePaginatedData();
    this.filterMonthsByTerm();
  }

  selectTerm(_: any): void {
    this.filterMonthsByTerm();
    
    const termId = this.form.get('selectedTerm')?.value ?? this.selectedTerm;
    
    // Update component properties
    this.selectedTerm = termId;
    
    // Only call updatePaginatedData (it will make the API call)
    this.updatePaginatedData();
  }

  selectSubject(_: any): void {
    const subjectId = this.form.get('selectedSubject')?.value ?? this.selectedSubject;
    
    // Update component properties
    this.selectedSubject = subjectId;
    
    // Only call updatePaginatedData (it will make the API call)
    this.updatePaginatedData();
    this.filterMonthsByTerm();
  }

  selectMonth(_: any): void {
    const monthId = this.form.get('selectedMonth')?.value ?? this.selectedMonth;
    
    // Update component properties
    this.selectedMonth = monthId;
    
    // Only call updatePaginatedData (it will make the API call)
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
          this.toastr.warning('you must to insert or update the grades first');
        }
      });
  }

  hidden: boolean = false;
  hiddenFrom: boolean = false;
  toggleHidden() {
    this.hidden = !this.hidden;
  }
  toggleHiddenFrom() {
    this.hiddenFrom = !this.hiddenFrom;
  }
  first: number = 0;
  rows: number = 5;

  paginates!: Paginates;
  getAllMonthlyGrades(TermId: number, MonthId: number, ClassId: number, SubjectId: number): void {
    // Ensure we have valid values, use 0 for SubjectId if null (means "all")
    const termId = TermId ?? this.selectedTerm;
    const monthId = MonthId ?? this.selectedMonth;
    const classId = ClassId ?? this.selectedClass;
    const subjectId = SubjectId ?? this.selectedSubject ?? 0;
    
    // Don't make API call if required values are missing
    if (!termId || !monthId || !classId) {
      console.warn('Missing required values for getAllMonthlyGrades:', { termId, monthId, classId, subjectId });
      return;
    }
    
    this.monthlyGradesService.getAllMonthlyGrades(termId, monthId, classId, subjectId, this.first / this.rows + 1, this.rows).subscribe((res) => {
      this.paginates = res; // تأكد أن res يحتوي totalCount
      this.monthlyGrades = res.data;
      this.displayedStudents = res.data;
      console.log('monthly Grades are', this.monthlyGrades);
      if (this.monthlyGrades.length > 0) {
        this.CurrentStudent = this.monthlyGrades[this.currentStudentIndex];
      } else {
        this.CurrentStudent = null!;
      }
    });
  }

  updatePaginatedData(): void {
    const termId = this.form.get('selectedTerm')?.value ?? this.selectedTerm;
    const monthId = this.form.get('selectedMonth')?.value ?? this.selectedMonth;
    const classId = this.form.get('selectedClass')?.value ?? this.selectedClass;
    const subjectId = this.form.get('selectedSubject')?.value ?? this.selectedSubject ?? 0;
    
    // Don't make API call if required values are missing
    if (!termId || !monthId || !classId) {
      console.warn('Missing required values for updatePaginatedData:', { termId, monthId, classId, subjectId });
      return;
    }
    
    this.monthlyGradesService.getAllMonthlyGrades(termId, monthId, classId, subjectId, this.first / this.rows + 1, this.rows).subscribe(res => {
      this.paginates = res;
      this.monthlyGrades = res.data;
      this.displayedStudents = res.data;
      
      // Set CurrentStudent when data loads
      if (this.monthlyGrades.length > 0) {
        // Reset index if it's out of bounds
        if (this.currentStudentIndex >= this.monthlyGrades.length) {
          this.currentStudentIndex = 0;
        }
        this.CurrentStudent = this.monthlyGrades[this.currentStudentIndex];
      } else {
        this.CurrentStudent = null!;
        this.toastr.info('No students found for the selected criteria.');
      }
    });
  }

  // Handle page change event from PrimeNG paginator 
  onPageChange(event: PaginatorState): void {
    this.first = event.first || 0; // Update first index based on page
    this.rows = event.rows || 5; // Update rows per page
    this.currentStudentIndex = 0; // Reset to first student when page changes
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
