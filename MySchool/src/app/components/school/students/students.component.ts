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
import { map } from 'rxjs';
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
    
    // Always fetch from server (server-side pagination and filtering)
    this.currentPage = newPage;
    this.pageSize = newPageSize;
    this.getAllStudents();
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
    this.pageSize = 10;
    this.paginatorService.first.set(0);
    this.paginatorService.rows.set(10);
    this.getAllStudents();
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  getAllStudents(): void {
    // Use POST endpoint with filters for server-side filtering and pagination
    this.studentsDataService.setLoading(true);
    const currentFilters = this.studentsDataService.getCurrentFilters();
    this.studentService.getStudentsPage(this.currentPage, this.pageSize, currentFilters).subscribe({
      next: (res) => {
        const students = res.data || res;
        // Get totalRecords from server (includes filtered count if filters are active)
        this.totalRecords = res.totalCount || 0;
        this.currentPage = res.pageNumber || this.currentPage;
        this.pageSize = res.pageSize || this.pageSize;
        
        // Update students and paginated students
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


  onFilterChange(filters: Record<string, string>): void {
    // Check if filters were added or removed
    const hasFilters = Object.keys(filters).length > 0;
    
    // Always use server-side filtering with POST endpoint
    this.currentPage = 1;
    this.paginatorService.first.set(0);
    this.filtersActive = hasFilters;
    
    if (!hasFilters) {
      // Filters cleared: clear cache and filtered students
      this.allStudentsCache = [];
      this.filteredStudents = [];
    }
    
    // Fetch data from server with current filters (server-side filtering)
    this.getAllStudents();
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
  handleImageError(event: Event): void {
    const img = event.target as HTMLImageElement;
    if (img) {
      img.src = './../../../../../assets/img/user.jpg';
    }
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