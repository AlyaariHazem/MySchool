import { Component, inject, OnInit } from '@angular/core';
import { FormBuilder, FormControl, FormGroup } from '@angular/forms';
import { ToastrService } from 'ngx-toastr';
import { Store } from '@ngrx/store';
import { PaginatorState } from 'primeng/paginator';

import { ClassService } from '../../core/services/class.service';
import { CurriculmsPlanService } from '../../core/services/curriculms-plan.service';
import { CurriculmsPlanSubject } from '../../core/models/curriculmsPlans.model';
import { Paginates } from '../../core/models/Pagination.model';
import { ITerm, TermGrades, TermlyGrade } from '../../core/models/term.model';
import { TermlyGradeService } from '../../core/services/termly-grade.service';
import { TERMS } from '../../core/data/terms';

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

  terms: ITerm[] = TERMS;
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
  selectedSubject = 0; // Default to "All" (0 represents "All")
  private classesLoaded = false;
  private subjectsLoaded = false;

  constructor(
    private formBuilder: FormBuilder,
    private curriculmsPlanService: CurriculmsPlanService,
    private toastr: ToastrService,
  ) { }

  ngOnInit(): void {
    // Initialize form first - default subject to 0 (All)
    this.form = this.formBuilder.group({
      selectedClass: [null],
      selectedTerm: [this.selectedTerm],
      selectedSubject: [0], // Default to "All" (0)
      note: [this.note],
    });
    this.currentLanguage();
    
    // Load classes and subjects first, then set form values
    // updatePaginatedData() will be called once after both are loaded
    this.getAllClasses();
    this.getAllSubjects();
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

  subjects: CurriculmsPlanSubject[] = [];
  getAllSubjects() {
    this.curriculmsPlanService.getAllCurriculmPlanSubjects().subscribe({
      next: (res) => {
        if (!res.isSuccess) {
          this.toastr.warning(res.errorMasseges[0] || 'Failed to load curriculum plans');
          return;
        }
        // this.subjects = res.result;
        this.subjects = [{ subjectID: 0, subjectName: 'All' }, ...res.result];
        this.subjectsLoaded = true;
        
        // Set the form value after subjects are loaded - default to "All" (subjectID: 0)
        if (this.subjects && this.subjects.length > 0) {
          // Find "All" option (subjectID: 0) or use the first one
          const defaultSubject = this.subjects.find((s: any) => s.subjectID === 0) || this.subjects.find((s: any) => s.subjectID === this.selectedSubject) || this.subjects[0];
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
    const classId = this.form.get('selectedClass')?.value ?? this.selectedClass;
    
    // Update component property
    this.selectedClass = classId;
    
    // Only call updatePaginatedData (it will make the API call)
    this.updatePaginatedData();
  }

  selectTerm(_: any): void {
    const termId = this.form.get('selectedTerm')?.value ?? this.selectedTerm;
    
    // Update component property
    this.selectedTerm = termId;
    
    // Only call updatePaginatedData (it will make the API call)
    this.updatePaginatedData();
  }

  selectSubject(_: any): void {
    const subjectId = this.form.get('selectedSubject')?.value ?? this.selectedSubject;
    
    // Update component property
    this.selectedSubject = subjectId;
    
    // Only call updatePaginatedData (it will make the API call)
    this.updatePaginatedData();
  }

  selectMonth(_: any): void {
    // This method is not used for term grades, but keeping for consistency
    // Only call updatePaginatedData (it will make the API call)
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
  getAllMonthlyGrades(TermId: number, yearId: number, ClassId: number, SubjectId: number): void {
    // Ensure we have valid values
    const termId = TermId ?? this.selectedTerm;
    const classId = ClassId ?? this.selectedClass;
    const subjectId = SubjectId ?? this.selectedSubject ?? 0;
    const yearIdToSend = yearId ?? this.yearID;
    
    // Don't make API call if required values are missing
    if (!termId || !classId) {
      console.warn('Missing required values for getAllMonthlyGrades:', { termId, classId, subjectId });
      return;
    }
    
    // Send yearID from parameter or localStorage (backend will ignore it and use active year)
    this.termlyGradeService.getTermlyGradesReport(termId, yearIdToSend, classId, subjectId, this.first / this.rows + 1, this.rows).subscribe((res) => {
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
    const termId = this.form.get('selectedTerm')?.value ?? this.selectedTerm;
    const classId = this.form.get('selectedClass')?.value ?? this.selectedClass;
    const subjectId = this.form.get('selectedSubject')?.value ?? this.selectedSubject ?? 0;
    
    // Don't make API call if required values are missing
    if (!termId || !classId) {
      console.warn('Missing required values for updatePaginatedData:', { termId, classId, subjectId });
      return;
    }
    
    this.visible = true;
    this.isLoading = true;
    // Send yearID from localStorage (backend will ignore it and use active year)
    this.termlyGradeService.getTermlyGradesReport(termId, this.yearID, classId, subjectId, this.first / this.rows + 1, this.rows).subscribe(res => {
      this.paginates = res;
      this.monthlyGrades = res.data;
      this.displayedStudents = res.data;
      this.isLoading = false;
      this.visible = false;
      
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
