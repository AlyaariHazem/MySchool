import { Component, inject, OnInit } from '@angular/core';
import { FormBuilder, FormControl, FormGroup, Validators } from '@angular/forms';
import { Store } from '@ngrx/store';
import { PaginatorState } from 'primeng/paginator';
import { MonthlyGradesService } from '../../core/services/monthly-grades.service';
import { MonthlyGrade, updateMonthlyGrades } from '../../core/models/MonthlyGrade.model';
import { ClassService } from '../../core/services/class.service';

interface Term {
  name: string;
  id: number;
}
interface Months{
  name:string;
  id:number;
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

  SelectBook = false;
  SelectClass = false;

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
  AllClasses:any;
  selectedTerm = 0;
  selectedMonth = 0;
  selectedClass = 0;
  selectedSubject = 0;

  constructor(
    private formBuilder: FormBuilder,
    
  ) { }

  ngOnInit(): void {
    this.getAllClasses();
    // Initialize form
    this.monthlyGradesService.getAllMonthlyGrades(1, 6, 1, 1).subscribe(res => {
      this.monthlyGrades = res;
      if (this.monthlyGrades.length > 0) {
        this.CurrentStudent = this.monthlyGrades[this.currentStudentIndex];
      }
      console.log("the monthly grades are", res);
    });
    this.form = this.formBuilder.group({
      BookID: [null, Validators.required],
      ClassID: [null, Validators.required]
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

  months: Months[] = [
    { name: 'يناير', id: 1 },
    { name: 'فبراير', id: 2 },
    { name: 'مارس', id: 3 },
    { name: 'أبريل', id: 4 },
    { name: 'مايو', id: 5 },
    { name: 'يونيو', id: 6 },
    { name: 'يوليو', id: 7 },
    { name: 'أغسطس', id: 8 },
    { name: 'سبتمبر', id: 9 },
    { name: 'أكتوبر', id: 10 },
    { name: 'نوفمبر', id: 11 },
    { name: 'ديسمبر', id: 12 }
  ];
  getAllClasses(){
    this.classService.GetAllNames().subscribe(res => {
      this.AllClasses=res;
    });
  }
  getAll(){}
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

  // Event Handlers
  selectBook(): void {
    this.SelectBook = true;
  }

  selectClass(): void {
    this.SelectClass = true;
  }

  saveAllGrades() {
    if (!this.selectedTerm || !this.selectedMonth || !this.selectedClass || !this.selectedSubject) {
      alert('Please select term, month, class, and subject first.');
      return;
    }
  
    const payload: updateMonthlyGrades[] = this.monthlyGrades.flatMap(stu =>
      stu.grades.map(g => ({
        studentID:  stu.studentID,
        subjectID:  this.selectedSubject,
        monthID:    this.selectedMonth,
        classID:    this.selectedClass,
        termID:     this.selectedTerm,
        gradeTypeID:g.gradeTypeID,
        grade:      +g.maxGrade            // convert to number
      }))
    );
  
    this.monthlyGradesService.updateMonthlyGrades(payload)
        .subscribe({
          next: _ => {
            alert('Grades saved successfully');
          },
          error: err => {
            console.error(err);
            alert('Error occurred while saving');
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
}
