import { Component, inject, Inject, OnInit } from '@angular/core';
import { FormBuilder, FormGroup } from '@angular/forms';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';
import { EmployeeService } from '../../core/services/employee.service';
import { DivisionService } from '../../core/services/division.service';
import { divisions } from '../../core/models/division.model';

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
  private divisionService = inject(DivisionService);
  mode: string = 'add';
  genders: Gender[] = [{ name: 'ذكر', value: 'Male' }, { name: 'انثى', value: 'Female' }];
  /** Matches backend <c>jopName</c> (e.g. SystemAdmin, Teacher). */
  employeeTypes: EmployeeType[] = [
    { name: 'مدير النظام', value: 'SystemAdmin' },
    { name: 'مشرف تربوي', value: 'EducationalSupervisor' },
    { name: 'مشرف إداري', value: 'AdministrativeSupervisor' },
    { name: 'معلم', value: 'Teacher' },
    { name: 'موظف إداري', value: 'AdministrativeEmployee' },
    { name: 'مدير', value: 'Manager' },
    { name: 'طالب', value: 'Student' },
    { name: 'ولي أمر', value: 'Guardian' },
  ];
  divisionOptions: { label: string; value: number }[] = [];
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
      password: [''],
      schoolID: [null as number | null],
      divisionID: [null as number | null],
      guardianID: [null as number | null],
    });
  }
  private toIsoDate(d: string | Date): string {
    return new Date(d).toISOString().substring(0, 10); // "yyyy-MM-dd"
  }

  date: Date = new Date();
  ngOnInit(): void {
    this.divisionService.GetAll().subscribe({
      next: (res) => {
        const rows = (res?.result ?? []) as divisions[];
        this.divisionOptions = rows.map((d) => ({
          label: d.divisionName + (d.classesName ? ` — ${d.classesName}` : ''),
          value: d.divisionID,
        }));
      },
      error: () => {
        this.divisionOptions = [];
      },
    });

    if (this.data && this.data.teacher) {
      const employee = this.data.teacher;
      const employeeID = employee.employeeID || employee.teacherID;
      this.date = employee.dob;
      this.form.patchValue({
        employeeID: employeeID,
        firstName: employee.firstName || '',
        lastName: employee.lastName || '',
        birthPlace: employee.birthPlace || '',
        dob: employee.dob ? this.toIsoDate(employee.dob) : '',
        email: employee.email || '',
        mobile: employee.mobile || employee.phoneNumber || '',
        address: employee.address || '',
        jopName: employee.jopName || 'Teacher',
        gender: employee.gender || 'Male',
        password: '',
        schoolID: employee.schoolID ?? employee.SchoolID ?? null,
        divisionID: employee.divisionID ?? employee.DivisionID ?? null,
        guardianID: employee.guardianID ?? employee.GuardianID ?? null,
      });
      this.mode = this.data.mode || 'add';
    } else {
      this.form.reset();
      this.mode = 'add';
    }
  }
  isStudentRole(): boolean {
    return this.form.get('jopName')?.value === 'Student';
  }
  onSaveClick(): void {
    if (this.mode === 'add') {
      this.AddEmployee();
    } else {
      this.UpdateEmployee();
    }
  }

  AddEmployee() {
    if (this.form.valid) {
      const raw = this.form.value;
      const payload: Record<string, unknown> = {
        ...raw,
        dob: raw.dob instanceof Date ? this.toIsoDate(raw.dob) : raw.dob,
      };
      if (!payload['password']) {
        delete payload['password'];
      }
      console.log('the form data are', payload);
      this.employeeService.addEmployee(payload as any).subscribe(res => {
        console.log('Employee added successfully', res);
        this.dialogRef.close(this.form.value);
      });
    } else {
      console.log('Form is invalid');
      console.log('the form data are', this.form.value);
    }
  }
  UpdateEmployee(): void {
    if (this.form.valid) {
      const employeeID = this.form.get('employeeID')?.value;
      if (!employeeID || employeeID === 0) {
        console.error('Invalid employeeID:', employeeID);
        alert('خطأ: رقم الموظف غير صحيح');
        return;
      }
      const raw = this.form.value;
      const payload: Record<string, unknown> = {
        ...raw,
        employeeID: Number(employeeID),
        dob: this.form.value.dob instanceof Date
          ? this.toIsoDate(this.form.value.dob)
          : this.form.value.dob,
      };
      if (!payload['password']) {
        delete payload['password'];
      }
      this.employeeService.updateEmployee(payload).subscribe({
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
