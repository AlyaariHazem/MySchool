import { Component, inject, SimpleChanges } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { PaginatorState } from 'primeng/paginator';
import { ToastrService } from 'ngx-toastr';
import { MatDialog, MatDialogConfig } from '@angular/material/dialog';
import { ActivatedRoute } from '@angular/router';

import { TranslationService } from '../../../core/services/translation.service';
import { LanguageService } from '../../../core/services/language.service';
import { EmployeeComponent } from './employee/employee.component';
import { EmployeeService } from '../core/services/employee.service';
import { Employee } from '../core/models/employee.model';

@Component({
  selector: 'app-teachers',
  templateUrl: './teachers.component.html',
  styleUrls: ['./teachers.component.scss']
})
export class TeachersComponent {
  form: FormGroup;

  translationService = inject(TranslationService);
  employeeService = inject(EmployeeService);
  languageService = inject(LanguageService);

  Employees: Employee[] = []
  paginated: Employee[] = []; // Paginated data
  max = 2;
  isLoading: boolean = true; // Loading state for the component
  first: number = 0; // Current starting index
  rows: number = 4; // Number of rows per page
  updatePaginatedData(): void {
    const start = this.first;
    const end = this.first + this.rows;
    this.paginated = this.Employees.slice(start, end);
  }

  // Handle page change event from PrimeNG paginator
  onPageChange(event: PaginatorState): void {
    this.first = event.first || 0; // Default to 0 if undefined
    this.rows = event.rows || 4; // Default to 4 rows
    this.updatePaginatedData();
  }

  showGrid: boolean = false;
  showCulomn: boolean = true;
  showteacherCulomn(): void {
    this.showCulomn = true;
    this.showGrid = false;
  }
  showteacherGrid(): void {
    this.showCulomn = false;
    this.showGrid = true;
  }

  constructor(
    private formBuilder: FormBuilder,
    private toastr: ToastrService,
    public dialog: MatDialog,
    private route: ActivatedRoute
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
      //this for add teacher 
      this.openDialog();
    }
    this.getAllTeachers();
    this.languageService.currentLanguage();
    this.translationService.changeLanguage(this.languageService.langDir);
  }
  getAllTeachers(): void {
    this.employeeService.getAllEmployees().subscribe((res) => {
      this.Employees = res;
      this.isLoading = false;
      this.updatePaginatedData(); // Initial slicing
    })
  }
  getTeacherByID(id: number): void {
    //  this.employeeService.getTeacherById(id).subscribe({
    //    next:(res)=>res,
    //  });
  }


  openDialog(): void {
    const dialogConfig = new MatDialogConfig();
    dialogConfig.panelClass = 'custom-dialog-container';
    dialogConfig.height = '80%';

    const dialogRef = this.dialog.open(EmployeeComponent, dialogConfig);

    dialogRef.afterClosed().subscribe((result) => {
      if (result) {
        this.toastr.success('تم إضافة الطالب بنجاح');
      }
    });
  }
  // Teachers.component.ts
  EditDialog(employee: Employee): void {

    // Pass the teacher data and a 'mode' flag to the dialog
    const dialogConfig = new MatDialogConfig();
    dialogConfig.panelClass = 'custom-dialog-container';
    dialogConfig.height = '80%';
    // IMPORTANT: Pass data to the dialog using 'data' property
    dialogConfig.data = {
      mode: 'edit',
      teacher: employee, // edit teacher
    };

    const dialogRef = this.dialog.open(EmployeeComponent, dialogConfig);

    dialogRef.afterClosed().subscribe((result) => {
      if (result) {
        this.toastr.success('تم تعديل الطالب بنجاح');
        this.getAllTeachers(); // Refresh table or handle updates
      }
    });
  }

  deleteTeacher(employee: Employee): void {
    const confirmDelete = confirm('هل أنت متأكد من حذف هذا الطالب؟');
    if (confirmDelete) {
      // Filter out the teacher with the given ID
      //   this.employeeService.deleteTeacher(teacherId).subscribe(()=>{
      //    this.toastr.warning('تم حذف الطالب بنجاح');
      //    this.getAllTeachers();
      //   })
    }
  }

}
