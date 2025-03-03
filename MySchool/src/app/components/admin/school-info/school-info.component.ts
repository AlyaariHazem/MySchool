import { Component, OnInit, Inject, inject } from '@angular/core';
import { FormBuilder, FormGroup } from '@angular/forms';
import { ToastrService } from 'ngx-toastr';
import { SchoolService } from '../../../core/services/school.service';
import { School } from '../../../core/models/school.modul';
import { LanguageService } from '../../../core/services/language.service';
import { MatDialogRef, MAT_DIALOG_DATA } from '@angular/material/dialog';

@Component({
  selector: 'app-school-info',
  templateUrl: './school-info.component.html',
  styleUrls: [
    './school-info.component.scss',
    './../../../shared/styles/style-primeng-input.scss',
    './../../../shared/styles/style-select.scss',
    './../../../shared/styles/button.scss',
  ],
})
export class SchoolInfoComponent implements OnInit {
  private schoolService = inject(SchoolService);
  private toaster = inject(ToastrService);
  languageService = inject(LanguageService);

  form: FormGroup;
  schoolType: any[] = [];
  schoolCategory: any[] = [];
  currentSchool?: School;
  isAddMode: boolean = true;

  constructor(
    private fb: FormBuilder,
    private dialogRef: MatDialogRef<SchoolInfoComponent>,
    @Inject(MAT_DIALOG_DATA) public data: any
  ) {
    this.form = this.fb.group({
      schoolID: [0],
      schoolName: [''],
      schoolNameEn: [''],
      schoolVison: [''],
      schoolType: [''],
      hireDate: ['2024-01-01'],
      schoolGoal: [''],
      notes: [''],
      country: [''],
      city: [''],
      address: [''],
      mobile: [''],
      website: [''],
      schoolCategory: [''],
      description: [''],
      schoolPhone: [''],
      schoolMission: [''],
      street: [''],
      email: [''],
      fax: [''],
      zone: [''],
    });
  }

  ngOnInit(): void {
    // If dialog data indicates edit mode, prefill the form
    if (this.data && this.data.isEditMode && this.data.schoolData) {
      this.isAddMode = false;
      this.currentSchool = this.data.schoolData;
      this.initializeForm(this.data.schoolData);
    } else {
      this.isAddMode = true;
      // For add mode, leave the form with default/empty values
    }

    this.schoolType = [
      { name: 'بنين', code: 'MALE' },
      { name: 'بنات', code: 'FEMALE' },
      { name: 'مختلط', code: 'MIXED' },
    ];

    this.schoolCategory = [
      { name: 'تمهيدي', code: 'KINDERGARTEN' },
      { name: 'أساسي', code: 'PRIMARY' },
      { name: 'ثانوي', code: 'SECONDARY' },
      { name: 'ثانوي,تمهيدي,أساسي', code: 'SECONDARY,KINDERGARTEN,PRIMARY' },
      { name: 'تمهيدي,أساسي', code: 'SECONDARY,PRIMARY' },
    ];

    this.languageService.currentLanguage();
  }

  initializeForm(school: School): void {
    this.form.patchValue({
      schoolID: school.schoolID || 0,
      schoolName: school.schoolName || '',
      schoolNameEn: school.schoolNameEn || '',
      schoolType: school.schoolType || '',
      email: school.email || '',
      country: school.country || '',
      schoolPhone: school.schoolPhone?.toString() || '',
      city: school.city || '',
      zone: school.zone || '',
      street: school.street || '',
      mobile: school.mobile || '',
      website: school.website || '',
      schoolCategory: school.schoolCategory || '',
      description: school.description || '',
      address: school.address || '',
      fax: school.fax || '',
      schoolVison: school.schoolVison || '',
      schoolMission: school.schoolMission || '',
      schoolGoal: school.schoolGoal || '',
      notes: school.notes || '',
      hireDate: school.hireDate || '2025-01-01',
    });
    console.log('Form after initialization:', this.form.value);
  }

  onSubmit(): void {
    if (this.form.valid) {
      const schoolData: School = this.form.value;
      if (this.isAddMode) {
        // When adding, remove the schoolID so the backend creates a new entry
        schoolData.schoolID = undefined;
        this.schoolService.addSchool(schoolData).subscribe({
          next: () => {
            this.toaster.success('Added successfully');
            this.dialogRef.close();
          },
          error: (err) => {
            console.error('Error adding school', err);
            console.log('Form is invalid:', this.form);
            this.toaster.error('Please fill all required fields');
          },
        });
      } else {
        if (this.currentSchool?.schoolID) {
          this.schoolService.updateSchool(this.currentSchool.schoolID, schoolData).subscribe({
            next: () => {
              this.toaster.success('Updated successfully');
              this.dialogRef.close();
            },
            error: (err) => {
              console.error('Error updating school', err);
              this.toaster.error('Update failed');
            },
          });
        } else {
          this.toaster.error('No school ID found for update');
        }
      }
    } else {
      this.toaster.error('Please fill all required fields');
    }
  }

  resetForm(): void {
    this.currentSchool = undefined;
    this.form.reset({
      schoolID: 0,
      hireDate: '2024-01-01',
    });
  }

  uploadPhoto(event: Event): void {
    console.log('Photo uploaded!', event);
  }
}
