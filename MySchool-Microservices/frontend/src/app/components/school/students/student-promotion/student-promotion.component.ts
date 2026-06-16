import { Component, inject, OnInit } from '@angular/core';
import { FormBuilder, FormGroup } from '@angular/forms';
import { ToastrService } from 'ngx-toastr';
import { StudentService } from '../../../../core/services/student.service';
import { UnregisteredStudent, PromoteStudentRequest, PromoteStudentsResponse, PromoteStudentResult } from '../../../../core/models/students.model';
import { Store } from '@ngrx/store';
import { selectLanguage } from '../../../../core/store/language/language.selectors';
import { map } from 'rxjs';
import { PaginatorState } from 'primeng/paginator';
import { DialogService, DynamicDialogRef } from 'primeng/dynamicdialog';
import { PromoteStudentDialogComponent } from './promote-student-dialog/promote-student-dialog.component';
import { YearService } from '../../../../core/services/year.service';
import { Year } from '../../../../core/models/year.model';
import { StageService } from '../../core/services/stage.service';
import { ClassService } from '../../core/services/class.service';

@Component({
  selector: 'app-student-promotion',
  templateUrl: './student-promotion.component.html',
  styleUrls: ['./student-promotion.component.scss']
})
export class StudentPromotionComponent implements OnInit {
  studentService = inject(StudentService);
  toastr = inject(ToastrService);
  dialogService = inject(DialogService);
  yearService = inject(YearService);
  stageService = inject(StageService);
  classService = inject(ClassService);
  private fb = inject(FormBuilder);
  
  readonly dir$ = this.store.select(selectLanguage).pipe(
    map(l => (l === 'ar' ? 'rtl' : 'ltr')),
  );

  searchForm: FormGroup;
  students: UnregisteredStudent[] = [];
  selectedStudents: UnregisteredStudent[] = [];
  
  // Pagination
  totalRecords: number = 0;
  currentPage: number = 1;
  pageSize: number = 5;

  // Filters
  studentNameFilter: string = '';
  stageIDFilter: number | null = null;
  classIDFilter: number | null = null;
  targetYearID: number | null = null;

  // Dropdown options
  stages: any[] = [];
  classes: any[] = [];
  filteredClasses: any[] = []; // Classes filtered by selected stage

  isLoading: boolean = false;
  Math = Math;

  constructor(private store: Store) {
    this.searchForm = this.fb.group({
      studentName: [''],
      stageID: [null],
      classID: [null]
    });
  }

  ngOnInit(): void {
    this.loadStages();
    this.loadClasses();
    this.determineTargetYear();
    
    // Listen to stage changes to filter classes
    this.searchForm.get('stageID')?.valueChanges.subscribe(stageID => {
      this.onStageChange(stageID);
    });
  }

  loadStages(): void {
    this.stageService.getAllStages().subscribe({
      next: (response: any) => {
        this.stages = response || [];
      },
      error: (error: any) => {
        console.error('Error loading stages:', error);
        this.toastr.error('فشل في تحميل المراحل', 'خطأ');
      }
    });
  }

  loadClasses(): void {
    this.classService.GetAll().subscribe({
      next: (response: any) => {
        if (response.isSuccess) {
          this.classes = response.result || [];
          this.filteredClasses = this.classes;
        }
      },
      error: (error: any) => {
        console.error('Error loading classes:', error);
        this.toastr.error('فشل في تحميل الصفوف', 'خطأ');
      }
    });
  }

  onStageChange(stageID: number | null): void {
    if (stageID) {
      // Filter classes by selected stage
      this.filteredClasses = this.classes.filter(c => c.stageID === stageID);
      
      // Clear class selection if current class is not in filtered list
      const currentClassID = this.searchForm.get('classID')?.value;
      if (currentClassID && !this.filteredClasses.find(c => c.classID === currentClassID)) {
        this.searchForm.patchValue({ classID: null });
      }
    } else {
      // Show all classes if no stage selected
      this.filteredClasses = this.classes;
      this.searchForm.patchValue({ classID: null });
    }
  }

  private determineTargetYear(): void {
    // Get all years to find the target year (next year after active year)
    this.yearService.getAllYears().subscribe({
      next: (years: Year[]) => {
        if (years && years.length > 0) {
          // Find active year
          const activeYear = years.find(y => y.active);
          
          if (activeYear) {
            // Find next year (inactive year with YearID > activeYear.YearID, or any year with YearID > activeYear.YearID)
            let targetYear = years
              .filter(y => !y.active && y.yearID > activeYear.yearID)
              .sort((a, b) => a.yearID - b.yearID)[0];
            
            // If no inactive year found, try any year with YearID > activeYear.YearID
            if (!targetYear) {
              targetYear = years
                .filter(y => y.yearID > activeYear.yearID)
                .sort((a, b) => a.yearID - b.yearID)[0];
            }
            
            if (targetYear) {
              this.targetYearID = targetYear.yearID;
              console.log(`✅ Target year determined: YearID ${targetYear.yearID} (Active year: YearID ${activeYear.yearID})`);
            } else {
              console.warn('⚠️ No target year found. Using null (backend will determine automatically).');
              this.toastr.warning('لم يتم العثور على سنة مستهدفة. سيتم التحديد تلقائياً من الخادم', 'تحذير', { timeOut: 4000 });
            }
          } else {
            console.warn('No active year found. Using null (backend will determine automatically).');
          }
        }
        
        // Load students after determining target year
        this.loadUnregisteredStudents();
      },
      error: (error) => {
        console.error('Error loading years:', error);
        this.toastr.warning('فشل في تحديد السنة المستهدفة. سيتم التحديد تلقائياً من الخادم', 'تحذير');
        // Continue with null targetYearID - backend will determine it
        this.loadUnregisteredStudents();
      }
    });
  }

  loadUnregisteredStudents(): void {
    this.isLoading = true;
    this.studentService.getUnregisteredStudents(
      this.currentPage,
      this.pageSize,
      this.targetYearID || undefined,
      this.studentNameFilter || undefined,
      this.stageIDFilter || undefined,
      this.classIDFilter || undefined
    ).subscribe({
      next: (response) => {
        this.students = response.data || [];
        this.totalRecords = response.totalCount || 0;
        this.currentPage = response.pageNumber || 1;
        this.pageSize = response.pageSize || 5;
        // Clear selection when data changes
        this.selectedStudents = this.selectedStudents.filter(selected => 
          this.students.some(s => s.studentID === selected.studentID)
        );
        this.isLoading = false;
      },
      error: (error) => {
        console.error('Error loading unregistered students:', error);
        this.toastr.error('فشل في تحميل بيانات الطلاب', 'خطأ');
        this.isLoading = false;
      }
    });
  }

  onSearch(): void {
    this.studentNameFilter = this.searchForm.get('studentName')?.value || '';
    this.stageIDFilter = this.searchForm.get('stageID')?.value || null;
    this.classIDFilter = this.searchForm.get('classID')?.value || null;
    this.currentPage = 1;
    this.loadUnregisteredStudents();
  }

  onPageChange(event: PaginatorState): void {
    this.currentPage = Math.floor((event.first || 0) / (event.rows || this.pageSize)) + 1;
    this.pageSize = event.rows || 5;
    this.loadUnregisteredStudents();
  }

  onStudentSelect(student: UnregisteredStudent, event: any): void {
    if (event.target.checked) {
      if (!this.selectedStudents.find(s => s.studentID === student.studentID)) {
        this.selectedStudents.push(student);
      }
    } else {
      this.selectedStudents = this.selectedStudents.filter(s => s.studentID !== student.studentID);
    }
  }

  onSelectAll(event: any): void {
    if (event.target.checked) {
      // Add all current page students that aren't already selected
      this.students.forEach(student => {
        if (!this.selectedStudents.some(s => s.studentID === student.studentID)) {
          this.selectedStudents.push(student);
        }
      });
    } else {
      // Remove only students from current page
      const currentPageStudentIDs = this.students.map(s => s.studentID);
      this.selectedStudents = this.selectedStudents.filter(
        s => !currentPageStudentIDs.includes(s.studentID)
      );
    }
  }

  isStudentSelected(studentID: number): boolean {
    return this.selectedStudents.some(s => s.studentID === studentID);
  }

  areAllStudentsSelected(): boolean {
    return this.students.length > 0 && 
           this.students.every(s => this.isStudentSelected(s.studentID));
  }

  promoteSelectedStudents(): void {
    if (this.selectedStudents.length === 0) {
      this.toastr.warning('يرجى اختيار طالب واحد على الأقل للترقية', 'تحذير');
      return;
    }

    console.log('Opening promotion dialog with:', {
      studentsCount: this.selectedStudents.length,
      targetYearID: this.targetYearID
    });

    const ref: DynamicDialogRef = this.dialogService.open(PromoteStudentDialogComponent, {
      header: 'ترقية الطلاب',
      width: '85%',
      draggable: true,
      resizable: true,
      closable: true,
      dismissableMask: true,
      styleClass: 'promote-students-dialog',
      data: {
        students: this.selectedStudents,
        targetYearID: this.targetYearID
      }
    });

    ref.onClose.subscribe((result: { students: PromoteStudentRequest[], copyCoursePlansFromCurrentYear?: boolean } | null) => {
      if (result && result.students && result.students.length > 0) {
        this.performPromotion(result.students, result.copyCoursePlansFromCurrentYear || false);
      }
    });
  }

  private performPromotion(students: PromoteStudentRequest[], copyCoursePlansFromCurrentYear: boolean = false): void {
    this.isLoading = true;
    this.studentService.promoteStudents(students, this.targetYearID || undefined, copyCoursePlansFromCurrentYear).subscribe({
      next: (response) => {
        const result: PromoteStudentsResponse = response.result;
        if (result) {
          if (result.successCount > 0) {
            this.toastr.success(
              `تم ترقية ${result.successCount} طالب بنجاح${result.failedCount > 0 ? ` (فشل ${result.failedCount} طالب)` : ''}`,
              'نجاح'
            );
          }
          
          if (result.failedCount > 0) {
            // Show detailed error messages for failed students
            const failedStudents = result.results.filter((r: PromoteStudentResult) => !r.success);
            failedStudents.forEach((failed: PromoteStudentResult) => {
              this.toastr.warning(
                `${failed.studentName}: ${failed.errorMessage || 'فشل في الترقية'}`,
                'تحذير',
                { timeOut: 5000 }
              );
            });
          }
        } else {
          this.toastr.success(response.message || 'تمت العملية بنجاح', 'نجاح');
        }
        
        this.selectedStudents = [];
        this.loadUnregisteredStudents();
      },
      error: (error) => {
        console.error('Error promoting students:', error);
        const errorMessage = error?.error?.message || error?.message || 'فشل في ترقية الطلاب';
        this.toastr.error(errorMessage, 'خطأ');
        this.isLoading = false;
      }
    });
  }
}
