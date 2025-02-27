import { Component, inject } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';

import { manager } from '../../../../core/models/manage/manager.model';
import { ManagerService } from '../../../../core/services/Manage/manager.service';
import { TenantService } from '../../../../core/services/Manage/tenant.service';
import { Tenant } from '../../../../core/models/manage/tenant.model';
import { SchoolService } from '../../../../core/services/school.service';
import { School } from '../../../../core/models/school.modul';

@Component({
  selector: 'app-add-manager',
  templateUrl: './add-manager.component.html',
  styleUrls:[
    './add-manager.component.scss',
    './../../../../shared/styles/style-input.scss',
    './../../../../shared/styles/style-select.scss',
    './../../../../shared/styles/button.scss'
  ] 
})
export class AddManagerComponent {
  managerForm: FormGroup;
  submitted = false;
  tenants:Tenant[]=[];
  schools:School[]=[];
  
  tenantService = inject(TenantService);
  managerService=inject(ManagerService);
  schoolService=inject(SchoolService);

  constructor(private fb: FormBuilder) {
    this.getAllTenant();
    this.getAllSchools();
    this.managerForm = this.fb.group({
      firstName: ['', Validators.required],
      middleName: [''],
      lastName: ['', Validators.required],
      schoolID: [null, Validators.required],
      userName: ['', Validators.required],
      password: ['', Validators.required],
      email: ['', [Validators.required, Validators.email]],
      userType: [{ value: 'MANAGER', disabled: true }], // User Type is read-only
      tenantID: [null, Validators.required],
      phoneNumber: [null]
    });
  }
  getAllTenant(): void {
    this.tenantService.getAllTenants().subscribe(res => {
      this.tenants = res;
    });
  }
  getAllSchools(): void {
    this.schoolService.getAllSchools().subscribe(res => {
      this.schools=res;
    });
  }

  onSubmit(managerForm:FormGroup): void {
    if (this.managerForm.valid) {
      const formData: manager = {
        fullName: {
          firstName: this.managerForm.value.firstName,
          middleName: this.managerForm.value.middleName,
          lastName: this.managerForm.value.lastName
        },
        schoolID: this.managerForm.value.schoolID,
        userName: this.managerForm.value.userName,
        password: this.managerForm.value.password,
        email: this.managerForm.value.email,
        userType: 'MANAGER',
        tenantID: this.managerForm.value.tenantID,
        phoneNumber: this.managerForm.value.phoneNumber
      };
      console.log("the form is",formData);
      this.managerService.addManager(formData).subscribe(res => {
        console.log("the result is",res);
      });
      this.submitted = true;
    }
  }
}
