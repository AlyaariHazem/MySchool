import { Component, inject, OnInit } from '@angular/core';
import { FormBuilder, FormControl, FormGroup, Validators } from '@angular/forms';
import { ToastrService } from 'ngx-toastr';
import { Store } from '@ngrx/store';
import { PaginatorState } from 'primeng/paginator';

import { MonthlyGradesService } from '../../core/services/monthly-grades.service';
import { MonthlyGrade, updateMonthlyGrades } from '../../core/models/MonthlyGrade.model';
import { ClassService } from '../../core/services/class.service';
import { CurriculmsPlanService } from '../../core/services/curriculms-plan.service';
import { CurriculmsPlanSubject } from '../../core/models/curriculmsPlans.model';

interface Term {
  name: string;
  id: number;
}
interface Month {
  id: number;       // رقم الشهر
  name: string;     // الاسم
  termId: number;   // إلى أيّ فصل ينتمي
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
  isLoading = false;
  visible: boolean = false;

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
      BookID: [null, Validators.required],
      ClassID: [null, Validators.required]
    });
    this.currentLanguage();
    // Example data for dropdowns

    this.updatePaginatedData();
  }
  getAllMonthlyGrades(TermID: number, MonthID: number, ClassID: number, SubjectID: number): void {
    this.isLoading = true;
    this.visible = true;
    this.monthlyGradesService.getAllMonthlyGrades(
      TermID,
      MonthID,
      ClassID,
      SubjectID
    ).subscribe(res => {
      this.monthlyGrades = res;
      this.isLoading = false;
      this.visible = false;
      if (this.monthlyGrades.length > 0) {
        this.CurrentStudent = this.monthlyGrades[this.currentStudentIndex];
      }
      console.log("the monthly grades are", res);
      this.updatePaginatedData(); // للتحديث عند كل تحميل
    });
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
    this.classService.GetAllNames().subscribe(res => {
      this.AllClasses = res;
    });
  }
  curriculmsPlan: CurriculmsPlanSubject[] = [];
  getAllCurriculm() {
    this.curriculmsPlanService.getAllCurriculmPlanSubjects().subscribe(res => {
      this.curriculmsPlan = [...res, { subjectName: "All", subjectID: 0 }];
      this.updatePaginatedData();
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
    this.filteredMonths = this.months.filter(m => m.termId === this.selectedTerm); // إفراغ الاختيار القديم
  }

  // Event Handlers
  selectBook(subjectId: number): void {
    this.selectedSubject = subjectId;
    this.getAllMonthlyGrades(this.selectedTerm, this.selectedMonth, this.selectedClass, subjectId);
    this.filterMonthsByTerm();
  }

  onTermChange(termId: number): void {
    this.selectedTerm = +termId;        // تأكد أنها رقم
    this.getAllMonthlyGrades(this.selectedTerm, this.selectedMonth, this.selectedClass, this.selectedSubject);
    this.filterMonthsByTerm();
  }

  selectMonth(monthId: number): void {
    this.selectedMonth = monthId;
    this.getAllMonthlyGrades(this.selectedTerm, monthId, this.selectedClass, this.selectedSubject);
    this.filterMonthsByTerm();
  }

  selectClass(classId: number): void {
    this.selectedClass = classId;
    this.getAllMonthlyGrades(this.selectedTerm, this.selectedMonth, classId, this.selectedSubject);
    this.filterMonthsByTerm();
  }
  
  yearID:number = Number(localStorage.getItem('yearID') || '1');
  saveAllGrades() {
    if (!this.selectedTerm || !this.selectedMonth || !this.selectedClass) {
      alert('Please select term, month, class, and subject first.');
      return;
    }
    const payload: updateMonthlyGrades[] = this.monthlyGrades.flatMap(stu =>
      stu.grades.map(g => ({
        studentID: stu.studentID,
        subjectID: stu.subjectID,
        yearID: this.yearID,
        monthID: this.selectedMonth,
        classID: this.selectedClass,
        termID: this.selectedTerm,
        gradeTypeID: g.gradeTypeID,
        grade: +g.maxGrade            // convert to number
      }))
    );
    console.log('the data are',payload);
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

  updateDisplayedStudents(): void {
    const startIndex = this.currentPage * this.pageSize;
    const endIndex = startIndex + this.pageSize;
    this.displayedStudents = this.monthlyGrades.slice(startIndex, endIndex);
  }
  trackByIndex(index: number, item: any): number {
    return index;
  }

  hidden: boolean = false;
  toggleHidden() {
    this.hidden = !this.hidden;
  }

  paginatedStudents: MonthlyGrade[] = [];

  first: number = 0;
  rows: number = 4;
  updatePaginatedData(): void {
    const start = this.first;
    const end = this.first + this.rows;
    this.paginatedStudents = this.monthlyGrades.slice(start, end);
  }

  // Handle page change event from PrimeNG paginator
  onPageChange(event: PaginatorState): void {
    this.first = event.first || 0; // Default to 0 if undefined
    this.rows = event.rows || 4; // Default to 4 rows
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
    const limit = this.gradeLimits[g.gradeTypeID] ?? 100;  // إذا لم يتم تحديد حد، استخدم 100
    let value = Number(g.maxGrade);
  
    // إذا كانت القيمة غير رقم، رجّعها صفر
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
  }
  enforceLimit(evt: Event, g: { gradeTypeID: number; maxGrade: any }): void {
    const input = evt.target as HTMLInputElement;
    const limit = this.gradeLimits[g.gradeTypeID] ?? 100;
  
    // احصل على الرقم الحالى (قد يكون فارغاً)
    let val = Number(input.value);
    if (isNaN(val)) { val = 0; }
  
    // قصّ إلى الحدّ
    if (val > limit)   val = limit;
    if (val < 0)       val = 0;
  
    // حدِّث نموذج البيانات وواجهة المستخدم فوراً
    g.maxGrade   = val;
    input.value  = String(val);   // يُجبر الـ input على إظهار الرقم المقصوص
  }
  
calcTotal(grades: { maxGrade: any }[]): number {
  return grades.reduce((sum, g) => sum + (+g.maxGrade || 0), 0);
}

calcPercent(grades: { maxGrade: any }[]): string {
  const total = this.calcTotal(grades);
  return total.toString();
}
  

}
