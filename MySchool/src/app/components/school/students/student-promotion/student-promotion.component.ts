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

@Component({
  selector: 'app-student-promotion',
  templateUrl: './student-promotion.component.html',
  styleUrls: ['./student-promotion.component.scss']
})
export class StudentPromotionComponent implements OnInit {
  studentService = inject(StudentService);
  toastr = inject(ToastrService);
  dialogService = inject(DialogService);
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
  targetYearID: number | null = null;

  isLoading: boolean = false;
  Math = Math;

  constructor(private store: Store) {
    this.searchForm = this.fb.group({
      studentName: [''],
      stageID: [null]
    });
  }

  ngOnInit(): void {
    this.loadUnregisteredStudents();
  }

  loadUnregisteredStudents(): void {
    this.isLoading = true;
    this.studentService.getUnregisteredStudents(
      this.currentPage,
      this.pageSize,
      this.targetYearID || undefined,
      this.studentNameFilter || undefined,
      this.stageIDFilter || undefined
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

    const ref: DynamicDialogRef = this.dialogService.open(PromoteStudentDialogComponent, {
      header: 'ترقية الطلاب',
      width: '80%',
      data: {
        students: this.selectedStudents
      }
    });

    ref.onClose.subscribe((result: { students: PromoteStudentRequest[] } | null) => {
      if (result && result.students && result.students.length > 0) {
        this.performPromotion(result.students);
      }
    });
  }

  private performPromotion(students: PromoteStudentRequest[]): void {
    this.isLoading = true;
    this.studentService.promoteStudents(students, this.targetYearID || undefined).subscribe({
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
