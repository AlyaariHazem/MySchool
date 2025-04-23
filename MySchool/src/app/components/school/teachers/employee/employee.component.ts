import { Component, Inject, OnInit } from '@angular/core';
import { FormBuilder, FormGroup } from '@angular/forms';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';
interface Gender{
  name:string;
  value:string;
}
interface EmployeeType{
  name:string;
  value:string;
}
@Component({
  selector: 'app-employee',
  templateUrl: './employee.component.html',
  styleUrl: './employee.component.scss'
})
export class EmployeeComponent implements OnInit {
  form:FormGroup;
  genders:Gender[]=[{name:'ذكر',value:'Male'},{name:'انثى',value:'Female'}];
  employeeTypes:EmployeeType[]=[{name:'معلم',value:'Teacher'},{name:'مدير',value:'Manager'}];
  constructor(public dialogRef: MatDialogRef<EmployeeComponent>,
    @Inject(MAT_DIALOG_DATA) public data: any) {
      this.form=new FormBuilder().group({
        employeeID: [0],
        firstName: [''],
        lastName: [''],
        birthPlace: [''],
        email: [''],
        phone: [''],
        address: [''],
        birthDate: [''],
        employeeType: [''],
        gender: [''],
      });
  }
  date:string='2025-10-01';
  ngOnInit(): void {
    if (this.data) {
      const employee =this.data.teacher;
      this.date=employee.birthDate;
      this.form.patchValue({
        employeeID: employee.employeeID,
        firstName: employee.firstName,
        lastName: employee.lastName,
        birthPlace:employee.birthPlace,
        email: employee.email,
        phone: employee.mobile,
        address: employee.address,
        employeeType:employee.employeeType,
        gender:employee.gender
      })
    }
  }
  onSubmit() {
    if (this.form.valid) {
      console.log('the form data are',this.form.value);
      // this.dialogRef.close(this.form.value);
    } else {
      console.log('Form is invalid');
      console.log('the form data are',this.form.value);
    }
  }
}
