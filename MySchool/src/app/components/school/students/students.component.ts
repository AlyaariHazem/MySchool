import { FormBuilder, FormControl, FormGroup, Validators } from '@angular/forms';
import { Component, inject, OnChanges, OnInit, SimpleChanges } from '@angular/core';
import { PaginatorState } from 'primeng/paginator';
import { MatDialog, MatDialogConfig } from '@angular/material/dialog';
import { ToastrService } from 'ngx-toastr';
import { ActivatedRoute } from '@angular/router';
import { DialogService } from 'primeng/dynamicdialog';

import { NewStudentComponent } from './new-student/new-student.component';
import { StudentDetailsDTO } from '../../../core/models/students.model';
import { StudentService } from '../../../core/services/student.service';
import { TranslationService } from '../../../core/services/translation.service';
import { LanguageService } from '../../../core/services/language.service';
import { ConfirmDialogComponent } from '../../../shared/components/confirm-dialog/confirm-dialog.component';
import { GuardianService } from '../core/services/guardian.service';
import { EditParentsComponent } from '../parents/edit-parents/edit-parents.component';

@Component({
  selector: 'app-students',
  templateUrl: './students.component.html',
  styleUrls: ['./students.component.scss'],
})
export class StudentsComponent implements OnInit, OnChanges {
  form: FormGroup;

  translationService = inject(TranslationService);
  studentService = inject(StudentService);
  guardianService = inject(GuardianService);
  languageService = inject(LanguageService);

  Students: StudentDetailsDTO[] = []
  values = new FormControl<string[] | null>(null);
  paginatedStudents: StudentDetailsDTO[] = []; // Paginated data
  max = 2;
  isLoading: boolean = true; // Loading state for the component
  first: number = 0; // Current starting index
  rows: number = 4; // Number of rows per page
  updatePaginatedData(): void {
    const start = this.first;
    const end = this.first + this.rows;
    this.paginatedStudents = this.Students.slice(start, end);
  }

  // Handle page change event from PrimeNG paginator
  onPageChange(event: PaginatorState): void {
    this.first = event.first || 0; // Default to 0 if undefined
    this.rows = event.rows || 4; // Default to 4 rows
    this.updatePaginatedData();
  }

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

  constructor(
    private formBuilder: FormBuilder,
    private toastr: ToastrService,
    public dialog: MatDialog,
    private route: ActivatedRoute,
    private dialogService: DialogService
  ) {
    this.form = this.formBuilder.group({
      stage: ['', Validators.required],
      gradeName: ['', Validators.required],
    });
  }
  ngOnChanges(changes: SimpleChanges): void {
    this.languageService.currentLanguage();
    this.translationService.changeLanguage(this.languageService.langDir);
  }

  id!: number;
  ngOnInit(): void {
    this.id = Number(this.route.snapshot.paramMap.get('id'));
    if (this.id) {
      //this for add student 
      this.openDialog();
    }
    this.getAllStudents();
    this.languageService.currentLanguage();
    this.translationService.changeLanguage(this.languageService.langDir);
  }
  getAllStudents(): void {
    this.studentService.getAllStudents().subscribe((res) => {
      this.Students = res;
      this.isLoading = false;
      this.updatePaginatedData(); // Initial slicing
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
  

  // students.component.ts
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
          this.updatePaginatedData(); // refresh visible list if using pagination
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
          this.updatePaginatedData(); // refresh visible list if using pagination
        }
      });
      
    });
  }

}