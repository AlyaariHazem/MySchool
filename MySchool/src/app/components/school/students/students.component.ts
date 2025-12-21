import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Component, inject,  OnInit } from '@angular/core';
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
import { selectLanguage } from '../../../core/store/language/language.selectors';
import { map } from 'rxjs';
import { Store } from '@ngrx/store';

@Component({
  selector: 'app-students',
  templateUrl: './students.component.html',
  styleUrls: ['./students.component.scss'],
})
export class StudentsComponent implements OnInit {
  form: FormGroup;

  studentService = inject(StudentService);
  guardianService = inject(GuardianService);
  paginatorService = inject(PaginatorService);
  
  readonly dir$ = this.store.select(selectLanguage).pipe(
    map(l => (l === 'ar' ? 'rtl' : 'ltr')),
  );

  Students: StudentDetailsDTO[] = []
  paginatedStudents: StudentDetailsDTO[] = [];
  hiddenFrom: boolean = false;

  // Pagination properties
  totalRecords: number = 0;
  currentPage: number = 1;
  pageSize: number = 10;

  showGrid: boolean = false;
  showCulomn: boolean = true;
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
    this.pageSize = event.rows || this.pageSize;
    this.currentPage = Math.floor((event.first || 0) / this.pageSize) + 1;
    
    // Fetch new data
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
    this.pageSize = 8;
    this.paginatorService.first.set(0);
    this.paginatorService.rows.set(8);
    this.getAllStudents();
  }
  getAllStudents(): void {
    this.studentService.getAllStudentsPaginated(this.currentPage, this.pageSize).subscribe({
      next: (res) => {
        this.paginatedStudents = res.data || res; // Handle both wrapped and unwrapped responses
        this.totalRecords = res.totalCount || 0;
        this.currentPage = res.pageNumber || this.currentPage;
        this.pageSize = res.pageSize || this.pageSize;
        
        // Update paginator service for UI consistency
        this.paginatorService.first.set((this.currentPage - 1) * this.pageSize);
        this.paginatorService.rows.set(this.pageSize);
      },
      error: (err) => {
        console.error("Error fetching students:", err);
        this.toastr.error('فشل في تحميل بيانات الطلاب. تأكد من تشغيل الخادم.', 'خطأ');
        this.paginatedStudents = [];
        this.totalRecords = 0;
      }
    })
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
      }
    });
  }
  deleteStudent(id: number): void {
    const ref = this.dialogService.open(ConfirmDialogComponent, {
      header: 'Confirm',
      width: 'auto',
      data: {
        title: 'Delete Student',
        message: 'Are you sure you want to delete this student?',
        deleteFn: () => this.studentService.DeleteStudent(id),
        successMessage: 'Student deleted successfully'
      }
    });

    ref.onClose.subscribe((confirmed: boolean) => {
      if (confirmed) {
        this.paginatedStudents = this.paginatedStudents.filter(s => s.studentID !== id);
      }
    });
  }

  EditStudentDialog(id: number): void {
    this.studentService.getStudentById(id).subscribe((res) => {
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
          this.paginatedStudents = this.paginatedStudents.map((student) =>
            student.studentID === result.studentID ? { ...student, ...result } : student
          );
          this.paginatedStudents = this.paginatorService.pageSlice(this.Students);
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
        if (result && result.studentID) {
          this.paginatedStudents = this.paginatedStudents.map((student) =>
            student.studentID === result.studentID ? { ...student, ...result } : student
          );
          this.paginatedStudents = this.paginatorService.pageSlice(this.Students);
        }
      });

    });
  }

}