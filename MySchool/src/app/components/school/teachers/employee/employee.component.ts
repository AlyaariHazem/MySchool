import { Component, inject, Inject, OnInit } from '@angular/core';
import { FormBuilder, FormGroup } from '@angular/forms';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';
import { EmployeeService } from '../../core/services/employee.service';
interface Gender {
  name: string;
  value: string;
}
interface EmployeeType {
  name: string;
  value: string;
}
@Component({
  selector: 'app-employee',
  templateUrl: './employee.component.html',
  styleUrl: './employee.component.scss'
})
export class EmployeeComponent implements OnInit {
  form: FormGroup;
  employeeService = inject(EmployeeService);
  mode: string = 'add';
  genders: Gender[] = [{ name: 'ذكر', value: 'Male' }, { name: 'انثى', value: 'Female' }];
  employeeTypes: EmployeeType[] = [{ name: 'معلم', value: 'Teacher' }, { name: 'مدير', value: 'Manager' }];
  constructor(public dialogRef: MatDialogRef<EmployeeComponent>,
    @Inject(MAT_DIALOG_DATA) public data: any) {
    this.form = new FormBuilder().group({
      employeeID: [0],
      firstName: [''],
      lastName: [''],
      birthPlace: [''],
      email: [''],
      mobile: [''],
      address: [''],
      dob: [''],
      jopName: [''],
      gender: [''],
    });
  }
  private toIsoDate(d: string | Date): string {
    return new Date(d).toISOString().substring(0, 10); // "yyyy-MM-dd"
  }

  date: Date = new Date();
  ngOnInit(): void {
    if (this.data && this.data.teacher) {
      const employee = this.data.teacher;
      
      // Handle both employeeID (from mapped data) and teacherID (from API)
      const employeeID = employee.employeeID || employee.teacherID;
      
      this.date = employee.dob;
      this.form.patchValue({
        employeeID: employeeID, // Use employeeID or fallback to teacherID
        firstName: employee.firstName || '',
        lastName: employee.lastName || '',
        birthPlace: employee.birthPlace || '',
        dob: employee.dob ? this.toIsoDate(employee.dob) : '',
        email: employee.email || '',
        mobile: employee.mobile || employee.phoneNumber || '', // Handle both mobile and phoneNumber
        address: employee.address || '',
        jopName: employee.jopName || 'Teacher',
        gender: employee.gender || 'Male'
      });
      this.mode = this.data.mode || 'add';
    } else {
      this.form.reset();
      this.mode = 'add';
    }
  }
  AddEmployee() {
    if (this.form.valid) {
      console.log('the form data are', this.form.value);
      this.employeeService.addEmployee(this.form.value).subscribe(res => {
        console.log('Employee added successfully', res);
        this.dialogRef.close(this.form.value);
      });
      // this.dialogRef.close(this.form.value);
    } else {
      console.log('Form is invalid');
      console.log('the form data are', this.form.value);
    }
  }
  UpdateEmployee(): void {
    if (this.form.valid) {
      const employeeID = this.form.get('employeeID')?.value;
      
      // Validate employeeID
      if (!employeeID || employeeID === 0) {
        console.error('Invalid employeeID:', employeeID);
        alert('خطأ: رقم الموظف غير صحيح');
        return;
      }
      
      console.log('Updating employee with ID:', employeeID);
      console.log('Form data:', this.form.value);
      
      this.employeeService.updateEmployee(Number(employeeID), this.form.value).subscribe({
        next: (res) => {
          console.log('Employee updated successfully', res);
          this.dialogRef.close(this.form.value);
        },
        error: (err) => {
          console.error('Error updating employee:', err);
          alert('فشل في تحديث بيانات الموظف');
        }
      });
    } else {
      console.log('Form is invalid');
      console.log('Form errors:', this.form.errors);
      console.log('Form value:', this.form.value);
    }
  }
}
