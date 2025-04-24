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
    if (this.data) {
      const employee = this.data.teacher;
      this.date = employee.dob;
      this.form.patchValue({
        employeeID: employee.employeeID,
        firstName: employee.firstName,
        lastName: employee.lastName,
        birthPlace: employee.birthPlace,
        dob: this.toIsoDate(employee.dob),
        email: employee.email,
        mobile: employee.mobile,
        address: employee.address,
        jopName: employee.jopName,
        gender: employee.gender
      })
      this.mode = this.data.mode;
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
      console.log('the form data are', this.form.value);
      this.employeeService.updateEmployee(this.form.value.employeeID, this.form.value).subscribe(res => {
        console.log('Employee updated successfully', res);
        this.dialogRef.close(this.form.value);
      });
      // this.dialogRef.close(this.form.value);
    } else {
      console.log('Form is invalid');
      console.log('the form data are', this.form.value);
    }
  }
}
