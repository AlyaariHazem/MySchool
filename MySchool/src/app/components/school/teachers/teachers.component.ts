import { Component, inject, OnInit} from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { PaginatorState } from 'primeng/paginator';
import { ToastrService } from 'ngx-toastr';
import { MatDialog, MatDialogConfig } from '@angular/material/dialog';
import { ActivatedRoute } from '@angular/router';

import { EmployeeComponent } from './employee/employee.component';
import { EmployeeService } from '../core/services/employee.service';
import { Employee } from '../core/models/employee.model';
import { PaginatorService } from '../../../core/services/paginator.service';
import { Store } from '@ngrx/store';
import { map } from 'rxjs';
import { selectLanguage } from '../../../core/store/language/language.selectors';
import { TableColumn } from '../../../shared/components/custom-table/custom-table.component';
import { ConfirmationService } from 'primeng/api';

@Component({
  selector: 'app-teachers',
  templateUrl: './teachers.component.html',
  providers: [ConfirmationService],
  styleUrls: ['./teachers.component.scss', './../../../shared/styles/style-table.scss'],
})
export class TeachersComponent implements OnInit {
  form: FormGroup;

  employeeService = inject(EmployeeService);
  paginatorService = inject(PaginatorService);
  confirmationService = inject(ConfirmationService);

  Employees: Employee[] = []
  paginated: Employee[] = []; // Paginated data
  max = 2;
  isLoading: boolean = true; // Loading state for the component
  totalRecords: number = 0;

  // Pagination properties
  currentPage: number = 1;
  pageSize: number = 10;

  showGrid: boolean = false;
  showCulomn: boolean = true;
  
  // Table columns configuration
  tableColumns: TableColumn[] = [
    { field: 'employeeID', header: 'رقم المستخدم', sortable: true, filterable: true },
    { 
      field: 'fullName', 
      header: 'اسم المستخدم', 
      sortable: true, 
      filterable: true,
      template: 'custom',
      formatter: (value: any, row: Employee) => {
        return `${row.firstName || ''} ${row.lastName || ''}`.trim();
      }
    },
    { field: 'jopName', header: 'الوضيفة', sortable: true, filterable: true },
    { field: 'employeeID', header: 'رقم المستخدم', sortable: true, filterable: true },
    { 
      field: 'age', 
      header: 'العمر', 
      sortable: true, 
      filterable: true
    },
    { 
      field: 'gender', 
      header: 'النوع', 
      sortable: true, 
      filterable: true,
      template: 'custom',
      formatter: (value: any, row: Employee) => {
        return row.gender === 'Male' ? 'ذكر' : 'انثى';
      }
    },
    { field: 'hireDate', header: 'تاريخ الإنشاء', sortable: true, filterable: true, template: 'date' },
    { field: 'address', header: 'العنوان', sortable: true, filterable: true },
    { field: 'mobile', header: 'رقم الهاتف', sortable: true, filterable: true },
  ];
  readonly dir$ = this.store.select(selectLanguage).pipe(
    map(l => (l === 'ar' ? 'rtl' : 'ltr')),
  );
  showteacherCulomn(): void {
    this.showCulomn = true;
    this.showGrid = false;
  }
  showteacherGrid(): void {
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
    
    // Always fetch from server (server-side pagination)
    this.currentPage = newPage;
    this.pageSize = newPageSize;
    this.getAllEmployees();
  }
  
  // Handle row edit
  onRowEdit(employee: Employee): void {
    this.EditDialog(employee);
  }
  
  // Handle row delete
  onRowDelete(employee: Employee): void {
    if (employee.employeeID && employee.jopName) {
      this.deleteEmployee(employee.employeeID, employee.jopName);
    }
  }
  constructor(
    private formBuilder: FormBuilder,
    private toastr: ToastrService,
    public dialog: MatDialog,
    private store:Store,
    private route: ActivatedRoute
  ) {
    this.form = this.formBuilder.group({
      stage: ['', Validators.required],
      gradeName: ['', Validators.required],
    });
  }

  id!: number;
  ngOnInit(): void {
    this.id = Number(this.route.snapshot.paramMap.get('id'));
    if (this.id) {
      //this for add teacher 
      this.openDialog();
    }
    // Initialize pagination
    this.currentPage = 1;
    this.pageSize = 10;
    this.paginatorService.first.set(0);
    this.paginatorService.rows.set(10);
    this.getAllEmployees();
  }
  
  getAllEmployees(): void {
    this.isLoading = true;
    this.employeeService.getEmployeesPage(this.currentPage, this.pageSize, {}).subscribe({
      next: (res) => {
        const rawEmployees = res.data || [];
        
        // Map API response to Employee model structure
        // API returns teacherID, phoneNumber but Employee model expects employeeID, mobile
        const employees = rawEmployees.map((teacher: any) => ({
          employeeID: teacher.teacherID, // Map teacherID to employeeID
          firstName: teacher.firstName || '',
          middleName: teacher.middleName || '',
          lastName: teacher.lastName || '',
          jopName: teacher.jopName || 'Teacher', // Default to Teacher if not provided
          address: teacher.address || null,
          mobile: teacher.phoneNumber || '', // Map phoneNumber to mobile
          gender: teacher.gender || 'Male',
          hireDate: teacher.hireDate || new Date(),
          dob: teacher.dob || new Date(),
          email: teacher.email || null,
          imageURL: teacher.imageURL || null,
          managerID: teacher.managerID || null,
          // Keep original fields for reference
          teacherID: teacher.teacherID,
          userID: teacher.userID,
          userName: teacher.userName
        }));
        
        // Get totalRecords from server
        this.totalRecords = res.totalCount || 0;
        this.currentPage = res.pageNumber || this.currentPage;
        this.pageSize = res.pageSize || this.pageSize;
        
        // Update employees and paginated employees
        this.Employees = employees;
        this.paginated = employees;
        
        // Update paginator service for UI consistency
        this.paginatorService.first.set((this.currentPage - 1) * this.pageSize);
        this.paginatorService.rows.set(this.pageSize);
        this.isLoading = false;
      },
      error: (err) => {
        console.error("Error fetching employees:", err);
        this.toastr.error('فشل في تحميل بيانات الموظفين. تأكد من تشغيل الخادم.', 'خطأ');
        this.paginated = [];
        this.Employees = [];
        this.totalRecords = 0;
        this.isLoading = false;
      }
    });
  }

  openDialog(): void {
    const dialogConfig = new MatDialogConfig();
    dialogConfig.panelClass = 'custom-dialog-container';
    dialogConfig.height = '60%';

    const dialogRef = this.dialog.open(EmployeeComponent, dialogConfig);

    dialogRef.afterClosed().subscribe((result) => {
      if (result) {
        this.toastr.success('تم إضافة الموظف بنجاح');
        // Reload employees to get updated list from server
        this.getAllEmployees();
      }
    });
  }
  // Teachers.component.ts
  EditDialog(employee: Employee): void {

    // Pass the teacher data and a 'mode' flag to the dialog
    const dialogConfig = new MatDialogConfig();
    dialogConfig.panelClass = 'custom-dialog-container';
    dialogConfig.height = '60%';
    // IMPORTANT: Pass data to the dialog using 'data' property
    dialogConfig.data = {
      mode: 'edit',
      teacher: employee, // edit teacher
    };

    const dialogRef = this.dialog.open(EmployeeComponent, dialogConfig);

    dialogRef.afterClosed().subscribe((result: Employee) => {
      if (result) {
        this.toastr.success('تم تعديل الموظف بنجاح');
        // Reload employees to get updated list from server
        this.getAllEmployees();
      }
    });
  }
  //show confirm dialog before deleting
  deleteEmployee(id: number, jobType: string): void {
    this.confirmationService.confirm({
      message: 'هل أنت متأكد من حذف الموظف؟',
      header: 'تأكيد الحذف',
      icon: 'pi pi-exclamation-triangle',
      acceptLabel: 'نعم',
      rejectLabel: 'لا',
      acceptIcon: 'pi pi-check',
      rejectIcon: 'pi pi-times',
      acceptButtonStyleClass: 'p-button-danger',
      rejectButtonStyleClass: 'p-button-secondary',
      accept: () => {
        this.employeeService.deleteEmployee(id, jobType).subscribe({
          next: (res) => {
            console.log('Employee deleted successfully', res);
            this.toastr.success('تم حذف الموظف بنجاح');
            // Reload employees to get updated list from server
            this.getAllEmployees();
          },
          error: (err) => {
            console.error('Error deleting employee:', err);
            this.toastr.error('فشل في حذف الموظف', 'خطأ');
          }
        });
      }
    });
  }
}
