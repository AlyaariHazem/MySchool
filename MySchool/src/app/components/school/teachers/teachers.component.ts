import { Component, inject} from '@angular/core';
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

@Component({
  selector: 'app-teachers',
  templateUrl: './teachers.component.html',
  styleUrls: ['./teachers.component.scss', './../../../shared/styles/style-table.scss'],
})
export class TeachersComponent {
  form: FormGroup;

  employeeService = inject(EmployeeService);
  paginatorService = inject(PaginatorService);

  Employees: Employee[] = []
  paginated: Employee[] = []; // Paginated data
  max = 2;
  isLoading: boolean = true; // Loading state for the component
  totalRecords: number = 0;

  showGrid: boolean = false;
  showCulomn: boolean = true;
  
  // Table columns configuration
  tableColumns: TableColumn[] = [
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
    this.paginatorService.onPageChange(event);
    this.paginated = this.paginatorService.pageSlice(this.Employees);
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
    this.getAllEmployees();
  }
  getAllEmployees(): void {
    this.employeeService.getAllEmployees().subscribe({
      next: (res) => {
        if (!res.isSuccess) {
          this.toastr.error(res.errorMasseges[0] || 'Failed to load employees.');
          return;
        }
        this.Employees = res.result;
        this.totalRecords = this.Employees.length;
        this.paginated = this.paginatorService.pageSlice(this.Employees);
        this.isLoading = false;
      }
    })
  }

  openDialog(): void {
    const dialogConfig = new MatDialogConfig();
    dialogConfig.panelClass = 'custom-dialog-container';
    dialogConfig.height = '50%';

    const dialogRef = this.dialog.open(EmployeeComponent, dialogConfig);

    dialogRef.afterClosed().subscribe((result) => {
      if (result) {
        this.paginated.push(result);
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

    dialogRef.afterClosed().subscribe((result: Employee) => {
      if (result) {
        this.paginated = this.paginated.map(emp => emp.employeeID === result.employeeID ? result : emp);
        this.toastr.success('تم تعديل الطالب بنجاح');
      }
    });
  }
  deleteEmployee(id: number, jobType: string): void {
    this.employeeService.deleteEmployee(id, jobType).subscribe(res => {
      this.paginated = this.paginated.filter(employee => employee.employeeID !== id);
      console.log('Employee deleted successfully', res);
      this.toastr.success('تم حذف الموظف بنجاح');
    });
  }
}
