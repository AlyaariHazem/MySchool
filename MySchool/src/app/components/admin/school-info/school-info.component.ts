import { Component, inject, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ToastrService } from 'ngx-toastr';
import { SchoolService } from '../../../core/services/school.service';
import { School } from '../../../core/models/school.modul';
import { LanguageService } from '../../../core/services/language.service';

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
  Books: any[] = [];
  classes: any[] = [];
  school: School[] = []; // list of schools

  // To store the currently selected school (if editing)
  currentSchool?: School;

  constructor(private fb: FormBuilder) {
    // Setup validators as needed. Here we mark schoolName and email as required for example.
    this.form = this.fb.group({
      schoolID: [0],
      schoolName: ['', Validators.required],
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
      email: ['', [Validators.required, Validators.email]],
      fax: [''],
      zone: [''],
    });
  }

  ngOnInit(): void {
    // Load schools from the API
    this.schoolService.getAllSchools().subscribe((res) => {
      this.school = res;
      console.log('the schools are', this.school);
      if (res.length > 0) {
        // If at least one school exists, we take the first one to edit.
        this.currentSchool = res[0];
        this.initializeForm(this.currentSchool);
      }
    });

    // For School Type
    this.Books = [
      { name: 'بنين', code: 'MALE' },
      { name: 'بنات', code: 'FEMALE' },
      { name: 'مختلط', code: 'MIXED' },
    ];

    // For School Category
    this.classes = [
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
      schoolID: school.schoolID,
      schoolName: school.schoolName,
      schoolNameEn: school.schoolNameEn,
      schoolType: school.schoolType,
      email: school.email,
      country: school.country,
      schoolPhone: school.schoolPhone,
      city: school.city,
      zone: school.zone,
      street: school.street,
      mobile: school.mobile,
      website: school.website,
      schoolCategory: school.schoolCategory,
      description: school.description,
      address: school.address,
      fax: school.fax,
      schoolVison: school.schoolVison,
      schoolMission: school.schoolMission,
      schoolGoal: school.schoolGoal,
      notes: school.notes,
      hireDate: school.hireDate,
    });
  }

  // Called on form submission
  onSubmit(): void {
    if (this.form.valid) {
      const schoolData: School = this.form.value;
      // Decide to add or update based on schoolID (or existence of currentSchool)
      if (this.currentSchool && this.currentSchool.schoolID) {
        // Update existing school
        this.schoolService
          .updateSchool(this.currentSchool.schoolID, schoolData)
          .subscribe({
            next: (res) => {
              this.toaster.success('Updated successfully');
              // Optionally update local data
              this.currentSchool = res;
            },
            error: (err) => {
              console.error('Error updating school', err);
              this.toaster.error('Update failed');
            },
          });
      } else {
        // Add new school
        this.schoolService.addSchool(schoolData).subscribe({
          next: (res) => {
            this.toaster.success('Added successfully');
            // Optionally push the new school to local list
            this.school.push(res);
            this.currentSchool = res;
          },
          error: (err) => {
            console.error('Error adding school', err);
            console.error('school data is', schoolData);
            this.toaster.error('Add failed');
          },
        });
      }
    } else {
      this.toaster.error('Please fill all required fields');
    }
  }

  // Method to delete the current school
  deleteSchool(): void {
    if (this.currentSchool && this.currentSchool.schoolID) {
      if (confirm('Are you sure you want to delete this school?')) {
        this.schoolService.deleteSchool(this.currentSchool.schoolID).subscribe({
          next: () => {
            this.toaster.success('Deleted successfully');
            // Remove the deleted school from the list and clear the form
            this.school = this.school.filter(s => s.schoolID !== this.currentSchool?.schoolID);
            this.currentSchool = undefined;
            this.form.reset();
          },
          error: (err) => {
            console.error('Error deleting school', err);
            this.toaster.error('Delete failed');
          },
        });
      }
    } else {
      this.toaster.error('No school selected to delete');
    }
  }

  // Optional: reset form for adding a new school
  resetForm(): void {
    this.currentSchool = undefined;
    this.form.reset({
      schoolID: 0,
      hireDate: '2024-01-01'
    });
  }

  // For photo upload example
  uploadPhoto(event: Event): void {
    // Add your file upload logic here
    console.log('Photo uploaded!', event);
  }
}
