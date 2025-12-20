import { Component, inject, Inject, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';

import { manager } from '../../core/models/manager.model';
import { managerInfo } from '../../core/models/managerInfo.model';
import { ManagerService } from '../../../../core/services/manager.service';
import { TenantService } from '../../../../core/services/tenant.service';
import { Tenant } from '../../core/models/tenant.model';
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
export class AddManagerComponent implements OnInit {
  managerForm: FormGroup;
  submitted = false;
  tenants:Tenant[]=[];
  schools:School[]=[];
  isEditMode: boolean = false;
  managerID?: number;
  pageTitle: string = 'إضافة مدير مدرسة';
  
  tenantService = inject(TenantService);
  managerService=inject(ManagerService);
  schoolService=inject(SchoolService);

  constructor(
    private fb: FormBuilder,
    private dialogRef: MatDialogRef<AddManagerComponent>,
    @Inject(MAT_DIALOG_DATA) public data: { manager?: managerInfo; isEditMode?: boolean }
  ) {
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

  ngOnInit(): void {
    if (this.data && this.data.isEditMode && this.data.manager) {
      this.isEditMode = true;
      this.managerID = this.data.manager.managerID;
      this.pageTitle = 'تعديل مدير مدرسة';
      // Make password optional in edit mode
      this.managerForm.get('password')?.clearValidators();
      this.managerForm.get('password')?.updateValueAndValidity();
      // Wait for schools to load before populating form
      if (this.schools.length > 0) {
        this.populateForm(this.data.manager);
      } else {
        // If schools haven't loaded yet, populate after they load
        this.schoolService.getAllSchools().subscribe(schools => {
          this.schools = schools;
          this.populateForm(this.data.manager!);
        });
      }
      // Fetch full manager details to get tenantID if available
      this.managerService.getManagerById(this.data.manager.managerID).subscribe({
        next: (fullManager) => {
          // If the backend returns tenantID in the full manager response, use it
          // For now, we'll proceed without it since GetManagerDTO doesn't include it
        },
        error: (err) => {
          console.error('Error fetching manager details:', err);
        }
      });
    }
  }

  populateForm(manager: managerInfo): void {
    // Find schoolID by matching schoolName
    const school = this.schools.find(s => s.schoolName === manager.schoolName);
    const schoolID = school?.schoolID || null;

    this.managerForm.patchValue({
      firstName: manager.fullName.firstName,
      middleName: manager.fullName.middleName || '',
      lastName: manager.fullName.lastName,
      schoolID: schoolID,
      userName: manager.userName,
      password: '', // Leave password empty in edit mode
      email: manager.email,
      userType: manager.userType,
      tenantID: manager.tenantID ?? null,
      phoneNumber: manager.phoneNumber
    });
  }
  getAllTenant(): void {
    this.tenantService.getAllTenants().subscribe(res => {
      this.tenants = res;
    });
  }
  getAllSchools(): void {
    this.schoolService.getAllSchools().subscribe(res => {
      this.schools = res;
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
        password: this.managerForm.value.password || '', // Use empty string if password not provided in edit mode
        email: this.managerForm.value.email,
        userType: 'MANAGER',
        tenantID: this.managerForm.value.tenantID,
        phoneNumber: this.managerForm.value.phoneNumber
      };
      
      if (this.isEditMode && this.managerID) {
        // Update existing manager
        this.managerService.updateManager(this.managerID, formData).subscribe({
          next: (res) => {
            console.log("Manager updated:", res);
            this.dialogRef.close(true);
          },
          error: (err) => {
            console.error("Error updating manager:", err);
          }
        });
      } else {
        // Add new manager
        this.managerService.addManager(formData).subscribe({
          next: (res) => {
            console.log("Manager added:", res);
            this.dialogRef.close(true);
          },
          error: (err) => {
            console.error("Error adding manager:", err);
          }
        });
      }
      this.submitted = true;
    }
  }
}
