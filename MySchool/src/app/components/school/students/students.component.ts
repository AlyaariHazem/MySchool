import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Component, inject, OnInit, OnDestroy } from '@angular/core';
import { MatDialog, MatDialogConfig } from '@angular/material/dialog';
import { PaginatorState } from 'primeng/paginator';
import { ToastrService } from 'ngx-toastr';
import { ActivatedRoute } from '@angular/router';
import { DialogService } from 'primeng/dynamicdialog';

import { NewStudentComponent } from './new-student/new-student.component';
import { StudentDetailsDTO } from '../../../core/models/students.model';
import { StudentService } from '../../../core/services/student.service';
import { ConfirmDialogComponent } from '../../../shared/components/confirm-dialog/confirm-dialog.component';
import { GuardianService } from '../core/services/guardian.service';
import { EditParentsComponent } from '../parents/edit-parents/edit-parents.component';
import { PaginatorService } from '../../../core/services/paginator.service';
import { StudentsDataService } from '../../../core/services/students-data.service';
import { selectLanguage } from '../../../core/store/language/language.selectors';
import { map, takeUntil } from 'rxjs';
import { Store } from '@ngrx/store';
import { Subject } from 'rxjs';
import { TableColumn } from '../../../shared/components/custom-table/custom-table.component';

@Component({
  selector: 'app-students',
  templateUrl: './students.component.html',
  styleUrls: ['./students.component.scss'],
})
export class StudentsComponent implements OnInit, OnDestroy {
  form: FormGroup;

  studentService = inject(StudentService);
  guardianService = inject(GuardianService);
  paginatorService = inject(PaginatorService);
  studentsDataService = inject(StudentsDataService);
  
  readonly dir$ = this.store.select(selectLanguage).pipe(
    map(l => (l === 'ar' ? 'rtl' : 'ltr')),
  );

  private destroy$ = new Subject<void>();
  
  Students: StudentDetailsDTO[] = []
  allStudentsCache: StudentDetailsDTO[] = []; // Cache for when filters are active
  paginatedStudents: StudentDetailsDTO[] = [];
  filteredStudents: StudentDetailsDTO[] = [];
  hiddenFrom: boolean = false;
  filtersActive: boolean = false;

  // Pagination properties
  totalRecords: number = 0;
  currentPage: number = 1;
  pageSize: number = 10;

  showGrid: boolean = false;
  showCulomn: boolean = true;
  
  // Table columns configuration
  tableColumns: TableColumn[] = [
    { field: 'studentID', header: 'رقم الطالب', sortable: true, filterable: true },
    { 
      field: 'fullName', 
      header: 'اسم الطالب', 
      sortable: true, 
      filterable: true,
      template: 'custom',
      formatter: (value: any, row: StudentDetailsDTO) => {
        return `${row.fullName?.firstName || ''} ${row.fullName?.middleName || ''} ${row.fullName?.lastName || ''}`.trim();
      }
    },
    { field: 'stageName', header: 'المرحلة', sortable: true, filterable: true },
    { field: 'className', header: 'الفصل', sortable: true, filterable: true },
    { field: 'divisionName', header: 'الشعبة', sortable: true, filterable: true },
    { field: 'age', header: 'العمر', sortable: true, filterable: true },
    { field: 'gender', header: 'النوع', sortable: true, filterable: true },
    { field: 'hireDate', header: 'تاريخ التسجيل', sortable: true, filterable: true, template: 'date' },
  ];
  
  showStudentCulomn(): void {
    this.showCulomn = true;
    this.showGrid = false;
  }
  showStudentGrid(): void {
    this.showCulomn = false;
    this.showGrid = true;
  }
  handlePageChange(event: PaginatorState): void {
    // Update paginator service state first
    this.paginatorService.first.set(event.first || 0);
    this.paginatorService.rows.set(event.rows || this.pageSize);
    
    // Calculate current page and page size
    const newPageSize = event.rows || this.pageSize;
    const newPage = Math.floor((event.first || 0) / newPageSize) + 1;
    
    // Check if filters are active
    const hasFilters = Object.keys(this.studentsDataService.getCurrentFilters()).length > 0;
    
    // If page size changed, fetch new data from server
    if (newPageSize !== this.pageSize) {
      this.pageSize = newPageSize;
      this.currentPage = 1;
      this.getAllStudents();
    } else if (hasFilters) {
      // If filters are active, use client-side pagination on filtered results
      this.currentPage = newPage;
      this.updatePaginatedStudents();
      this.paginatorService.first.set((this.currentPage - 1) * this.pageSize);
    } else {
      // No filters: fetch new page from server
      this.currentPage = newPage;
      this.pageSize = newPageSize;
      this.getAllStudents();
    }
  }
  constructor(
    private formBuilder: FormBuilder,
    private toastr: ToastrService,
    public dialog: MatDialog,
    private store: Store,
    private route: ActivatedRoute,
    private dialogService: DialogService
  ) {
    this.form = this.formBuilder.group({
      stage: ['', Validators.required],
      gradeName: ['', Validators.required],
    });
  }
  toggleHidden() {
    this.hiddenFrom = !this.hiddenFrom;
  }

  id!: number;
  ngOnInit(): void {
    this.id = Number(this.route.snapshot.paramMap.get('id'));
    if (this.id) {
      //this for add student 
      this.openDialog();
    }
    // Initialize pagination
    this.currentPage = 1;
    this.pageSize = 8;
    this.paginatorService.first.set(0);
    this.paginatorService.rows.set(8);
    this.getAllStudents();

    // Subscribe to filtered students from service
    this.studentsDataService.filteredStudents$
      .pipe(takeUntil(this.destroy$))
      .subscribe(students => {
        // Only process filtered students if filters are actually active
        const hasFilters = Object.keys(this.studentsDataService.getCurrentFilters()).length > 0;
        if (hasFilters) {
          this.filteredStudents = students;
          this.updatePaginatedStudents();
        }
        // If no filters, ignore this update - we use server-side pagination
      });
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  getAllStudents(): void {
    // Check if filters are active
    const hasFilters = Object.keys(this.studentsDataService.getCurrentFilters()).length > 0;
    
    if (hasFilters && this.allStudentsCache.length === 0) {
      // Filters active but no cache: load all students for filtering
      this.loadAllStudentsForFiltering();
      return;
    }
    
    if (hasFilters && this.allStudentsCache.length > 0) {
      // Filters active and cache exists: use cache, filtering is handled by service
      this.studentsDataService.setStudents(this.allStudentsCache);
      this.updatePaginatedStudents();
      return;
    }
    
    // No filters: fetch current page from server
    this.studentsDataService.setLoading(true);
    this.studentService.getAllStudentsPaginated(this.currentPage, this.pageSize).subscribe({
      next: (res) => {
        const students = res.data || res;
        // IMPORTANT: Always get totalRecords from server when no filters
        this.totalRecords = res.totalCount || 0;
        this.currentPage = res.pageNumber || this.currentPage;
        this.pageSize = res.pageSize || this.pageSize;
        
        // No filters: use server-side pagination
        this.Students = students;
        this.paginatedStudents = students;
        
        // Update service with current page data
        this.studentsDataService.setStudents(students);
        this.studentsDataService.setPagination({
          currentPage: this.currentPage,
          pageSize: this.pageSize,
          totalRecords: this.totalRecords
        });
        
        // Update paginator service for UI consistency
        this.paginatorService.first.set((this.currentPage - 1) * this.pageSize);
        this.paginatorService.rows.set(this.pageSize);
        this.studentsDataService.setLoading(false);
      },
      error: (err) => {
        console.error("Error fetching students:", err);
        this.toastr.error('فشل في تحميل بيانات الطلاب. تأكد من تشغيل الخادم.', 'خطأ');
        this.paginatedStudents = [];
        this.filteredStudents = [];
        this.totalRecords = 0;
        this.studentsDataService.setLoading(false);
      }
    })
  }

  loadAllStudentsForFiltering(): void {
    // Load all students when filters are first applied
    this.studentsDataService.setLoading(true);
    this.studentService.getAllStudentsPaginated(1, 10000).subscribe({
      next: (res) => {
        const allStudents = res.data || res;
        this.allStudentsCache = allStudents; // Cache all students
        this.Students = allStudents;
        
        // Update service with all students for filtering
        this.studentsDataService.setStudents(allStudents);
        
        // Filtered students will be updated via subscription, then paginate
        this.updatePaginatedStudents();
        this.studentsDataService.setLoading(false);
      },
      error: (err) => {
        console.error("Error fetching all students:", err);
        this.studentsDataService.setLoading(false);
      }
    });
  }

  updatePaginatedStudents(): void {
    // Check if filters are active
    const hasFilters = Object.keys(this.studentsDataService.getCurrentFilters()).length > 0;
    this.filtersActive = hasFilters;
    
    if (hasFilters) {
      // Apply pagination to filtered students (client-side)
      const start = (this.currentPage - 1) * this.pageSize;
      const end = start + this.pageSize;
      this.paginatedStudents = this.filteredStudents.slice(start, end);
      // Update total records to reflect filtered count
      this.totalRecords = this.filteredStudents.length;
      
      // Update paginator service
      this.paginatorService.first.set((this.currentPage - 1) * this.pageSize);
      this.paginatorService.rows.set(this.pageSize);
    } else {
      // No filters: paginatedStudents should already be set from getAllStudents
      // totalRecords should come from server response, not filtered count
      if (this.paginatedStudents.length === 0 && this.Students.length > 0) {
        this.paginatedStudents = this.Students;
      }
      // Don't update totalRecords here - it should come from server
    }
  }

  onFilterChange(filters: Record<string, string>): void {
    // Check if filters were added or removed
    const hasFilters = Object.keys(filters).length > 0;
    
    if (hasFilters && !this.filtersActive) {
      // Filters added for the first time: load all students for client-side filtering
      this.currentPage = 1;
      this.paginatorService.first.set(0);
      this.allStudentsCache = []; // Clear cache to force reload
      this.loadAllStudentsForFiltering();
    } else if (!hasFilters && this.filtersActive) {
      // Filters cleared: go back to server-side pagination
      this.currentPage = 1;
      this.paginatorService.first.set(0);
      this.allStudentsCache = []; // Clear cache
      this.filtersActive = false;
      // Reset filtered students to empty to prevent subscription from interfering
      this.filteredStudents = [];
      // Fetch fresh data from server with correct totalRecords
      this.getAllStudents();
    } else if (hasFilters) {
      // Filters changed but were already active: just update pagination
      this.currentPage = 1;
      this.paginatorService.first.set(0);
      this.updatePaginatedStudents();
    }
  }
  getStudentByID(id: number): void {
    this.studentService.getStudentById(id).subscribe({
      next: (res) => res,
    });
  }


  openDialog(): void {
    const dialogConfig = new MatDialogConfig();
    dialogConfig.width = '80%';
    dialogConfig.panelClass = 'custom-dialog-container';

    const dialogRef = this.dialog.open(NewStudentComponent, dialogConfig);

    dialogRef.afterClosed().subscribe((result) => {
      if (result) {
        this.toastr.success('تم إضافة الطالب بنجاح');
        // Reload students
        this.getAllStudents();
      }
    });
  }
  deleteStudent(student: any): void {
    const studentId = student.studentID || student;
    const ref = this.dialogService.open(ConfirmDialogComponent, {
      header: 'Confirm',
      width: 'auto',
      data: {
        title: 'Delete Student',
        message: 'Are you sure you want to delete this student?',
        deleteFn: () => this.studentService.DeleteStudent(studentId),
        successMessage: 'Student deleted successfully'
      }
    });

    ref.onClose.subscribe((confirmed: boolean) => {
      if (confirmed) {
        this.studentsDataService.deleteStudent(studentId);
        this.getAllStudents();
      }
    });
  }

  EditStudentDialog(student: any): void {
    const studentId = student.studentID || student;
    this.studentService.getStudentById(studentId).subscribe((res) => {
      console.log("Editing student data:", res);

      // Pass the student data and a 'mode' flag to the dialog
      const dialogConfig = new MatDialogConfig();
      dialogConfig.width = '80%';
      dialogConfig.panelClass = 'custom-dialog-container';

      // IMPORTANT: Pass data to the dialog using 'data' property
      dialogConfig.data = {
        mode: 'edit',
        student: res, // edit student
      };

      const dialogRef = this.dialog.open(NewStudentComponent, dialogConfig);

      dialogRef.afterClosed().subscribe((result) => {
        if (result && result.studentID) {
          this.studentsDataService.updateStudent(result);
          this.getAllStudents();
        }
      });

    });
  }
  EditGuardianDialog(id: number): void {
    this.guardianService.getGuardianById(id).subscribe((res) => {
      console.log("Editing guardian data:", res);

      // Pass the student data and a 'mode' flag to the dialog
      const dialogConfig = new MatDialogConfig();
      dialogConfig.width = '80%';
      dialogConfig.panelClass = 'custom-dialog-container';

      // IMPORTANT: Pass data to the dialog using 'data' property
      dialogConfig.data = {
        mode: 'edit',
        student: res, // edit student
      };

      const dialogRef = this.dialog.open(EditParentsComponent, dialogConfig);

      dialogRef.afterClosed().subscribe((result) => {
        if (result) {
          this.getAllStudents();
        }
      });

    });
  }

}