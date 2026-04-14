import { Component, inject, OnInit} from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { PaginatorState } from 'primeng/paginator';
import { ToastrService } from 'ngx-toastr';
import { MatDialog, MatDialogConfig } from '@angular/material/dialog';
import { ActivatedRoute } from '@angular/router';

import { EmployeeComponent } from './employee/employee.component';
import { EmployeeService } from '../core/services/employee.service';
import { Employee } from '../core/models/employee.model';
import { YearService } from '../../../core/services/year.service';
import { Year } from '../../../core/models/year.model';
import { PaginatorService } from '../../../core/services/paginator.service';
import { Store } from '@ngrx/store';
import { map } from 'rxjs';
import { selectLanguage } from '../../../core/store/language/language.selectors';
import { TableColumn } from '../../../shared/components/custom-table/custom-table.component';
import { ConfirmationService } from 'primeng/api';

/**
 * School staff directory: teachers and managers for the selected academic year.
 * Data source: POST /api/Employee/page — unified school users (teachers, managers, school staff, students, guardians).
 */
@Component({
  selector: 'app-teachers',
  templateUrl: './teachers.component.html',
  styleUrls: ['./teachers.component.scss', './../../../shared/styles/style-table.scss'],
})
export class TeachersComponent implements OnInit {
  form: FormGroup;

  employeeService = inject(EmployeeService);
  private yearService = inject(YearService);
  paginatorService = inject(PaginatorService);
  confirmationService = inject(ConfirmationService);

  /** Active (or latest) academic year for POST Teacher/page filters — aligns with EmployeeYearAssignments. */
  listYearId: number | null = null;

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
    {
      field: 'jopName',
      header: 'الوظيفة',
      sortable: true,
      filterable: true,
      template: 'custom',
      formatter: (_value: unknown, row: Employee) => this.jobLabelAr(row.jopName),
    },
    { field: 'employeeID', header: 'رقم الموظف', sortable: true, filterable: true },
    {
      field: 'fullName',
      header: 'اسم الموظف',
      sortable: true,
      filterable: true,
      template: 'custom',
      formatter: (_value: unknown, row: Employee) => {
        return `${row.firstName || ''} ${row.lastName || ''}`.trim();
      },
    },
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

  /** Arabic label for backend <c>jopName</c> (API role keys). */
  jobLabelAr(jopName: string | undefined): string {
    const j = (jopName || '').trim();
    const map: Record<string, string> = {
      Teacher: 'معلم',
      Manager: 'مدير',
      SystemAdmin: 'مدير النظام',
      EducationalSupervisor: 'مشرف تربوي',
      AdministrativeSupervisor: 'مشرف إداري',
      AdministrativeEmployee: 'موظف إداري',
      Student: 'طالب',
      Guardian: 'ولي أمر',
    };
    return map[j] ?? (j || '—');
  }

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
    const id = employee.employeeID;
    if (id == null) {
      return;
    }
    this.deleteEmployee(id, employee.jopName || 'Teacher');
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

    this.yearService.getAllYears().subscribe({
      next: (years: Year[]) => {
        const active = years.find(y => y.active);
        const sorted = [...years].sort((a, b) => b.yearID - a.yearID);
        this.listYearId = active?.yearID ?? sorted[0]?.yearID ?? null;
        this.getAllEmployees();
      },
      error: () => this.getAllEmployees(),
    });
  }
  
  getAllEmployees(): void {
    this.isLoading = true;
    const filters: Record<string, string> = {};
    if (this.listYearId != null) {
      filters['yearId'] = String(this.listYearId);
    }
    this.employeeService.getEmployeesPage(this.currentPage, this.pageSize, filters).subscribe({
      next: (res) => {
        const rawEmployees = res.data || [];
        
        // Map API response to Employee model structure
        // API returns teacherID, phoneNumber but Employee model expects employeeID, mobile
        const employees: Employee[] = rawEmployees.map((row: any) => {
          const jopName = row.jopName ?? row.JopName ?? 'Teacher';
          const id = row.employeeID ?? row.EmployeeID ?? row.teacherID ?? row.TeacherID;
          const phone = row.mobile ?? row.Mobile ?? row.phoneNumber ?? row.PhoneNumber ?? '';
          return {
            employeeID: id,
            employeeRowKey: `${jopName}-${id}`,
            firstName: row.firstName ?? row.FirstName ?? '',
            middleName: row.middleName ?? row.MiddleName ?? '',
            lastName: row.lastName ?? row.LastName ?? '',
            jopName,
            address: row.address ?? row.Address ?? null,
            mobile: phone,
            gender: row.gender ?? row.Gender ?? 'Male',
            hireDate: row.hireDate ?? row.HireDate ?? new Date(),
            dob: row.dob ?? row.DOB ?? new Date(),
            email: row.email ?? row.Email ?? null,
            imageURL: row.imageURL ?? row.ImageURL ?? null,
            managerID: row.managerID ?? row.ManagerID ?? null,
            teacherID: row.teacherID ?? row.TeacherID,
            userID: row.userID ?? row.UserID,
            userName: row.userName ?? row.UserName,
          };
        });
        
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
      message: 'سيتم أرشفة الموظف للسنة الحالية (لن يُحذف من قاعدة البيانات). هل تريد المتابعة؟',
      header: 'تأكيد الأرشفة',
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
            console.log('Employee archived successfully', res);
            this.toastr.success('تم أرشفة الموظف بنجاح');
            // Reload employees to get updated list from server
            this.getAllEmployees();
          },
          error: (err) => {
            console.error('Error archiving employee:', err);
            this.toastr.error('فشل في أرشفة الموظف', 'خطأ');
          }
        });
      }
    });
  }
}
