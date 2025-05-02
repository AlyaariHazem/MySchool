import { Component, inject, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ToastrService } from 'ngx-toastr';

import { SchoolService } from '../../../../core/services/school.service';
import { LanguageService } from '../../../../core/services/language.service';
import { School } from '../../../../core/models/school.modul';
import { FileService } from '../../../../core/services/file.service';

@Component({
  selector: 'app-school-info',
  templateUrl: './school-info.component.html',
  styleUrls: [
    './school-info.component.scss',
    './../../../../shared/styles/style-primeng-input.scss',
    './../../../../shared/styles/style-select.scss',
    './../../../../shared/styles/button.scss',
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
  schoolId!: number;
  fileManage = inject(FileService);
  isLoading: boolean = true;

  constructor(private fb: FormBuilder) {
    // Set up validators as needed
    this.form = this.fb.group({
      schoolID: [0],
      schoolName: ['', Validators.required],
      schoolNameEn: [''],
      schoolVison: [''],
      schoolType: [''],
      hireDate: ['2025-01-01'],
      schoolGoal: [''],
      notes: [''],
      country: [''],
      imageURL: [''],
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
    // 1) Load the school from the backend
    // Here, we're hard-coding ID=1, but in a real scenario you might get it from the route param
    this.schoolId = Number(localStorage.getItem('schoolId'));
    this.loadSchool(Number(this.schoolId));

    // 2) Define options for dropdowns
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

  loadSchool(schoolId: number): void {
    this.schoolService.getSchoolByID(schoolId).subscribe({
      next: (res: School) => {
        this.currentSchool = res;
        this.isLoading = false;
        this.initializeForm(res); // Populate the form once data is received
      },
      error: (err) => {
        console.error('Error fetching school:', err);
        this.isLoading = false;
        this.toaster.error('Failed to load school data');
      },
    });
  }

  initializeForm(school: School): void {
    this.form.patchValue({
      schoolID: school.schoolID || 1,
      schoolName: school.schoolName || '',
      schoolNameEn: school.schoolNameEn || '',
      schoolType: school.schoolType || '',
      email: school.email || '',
      country: school.country || '',
      schoolPhone: school.schoolPhone || '',
      city: school.city || '',
      zone: school.zone || '',
      street: school.street || '',
      mobile: school.mobile || '',
      website: school.website || '',
      schoolCategory: school.schoolCategory || '',
      description: school.description || '',
      fax: school.fax || '',
      schoolVison: school.schoolVison || '',
      schoolMission: school.schoolMission || '',
      schoolGoal: school.schoolGoal || '',
      notes: school.notes || '',
      hireDate: school.hireDate || '2025-01-01',
    });
    this.SchoolImageURL= school.imageURL || '';
    localStorage.setItem('SchoolImageURL', this.SchoolImageURL);
    console.log('Form after initialization:', this.form.value);
  }

  SchoolImage!: File;
  SchoolImageURL: string = '';
  uploadPhoto(event: Event): void {
    const input = event.target as HTMLInputElement;

    if (input.files?.[0]) {
      this.SchoolImage = input.files[0];

      const reader = new FileReader();
      reader.onload = (e: any) => {
        this.SchoolImageURL = e.target.result;

        // Patch form only after image load
        this.form.patchValue({
          imageURL: this.SchoolImageURL,
        });
      };

      reader.readAsDataURL(this.SchoolImage);
    } else {
      console.error('No file selected');
    }
  }

  onSubmit(): void {
    if (this.form.valid) {
      const schoolData: School = this.form.value;
      if(this.SchoolImage!=undefined)
       {
        schoolData.imageURL = this.SchoolImage.name;
        localStorage.setItem('SchoolImageURL', this.currentSchool!.imageURL);
       }else{
        schoolData.imageURL=localStorage.getItem('SchoolImageURL')!;
       }

      // Check if we have a current school and a valid ID
      if (this.currentSchool && this.currentSchool.schoolID) {
        // 1) Update the existing school
        this.schoolService.updateSchool(this.currentSchool.schoolID, schoolData).subscribe({
          next: (res) => {
            this.toaster.success('Updated successfully');
          },
          error: (err) => {
            console.error('Error updating school', err);
            this.toaster.error('Update failed');
          },
        });
        this.fileManage.DeleteFile('School', Number(this.schoolId)).subscribe();
        this.fileManage.uploadFile(this.SchoolImage, 'School', Number(this.schoolId)).subscribe();
      } else {
        this.toaster.error('No school selected for update');
      }
    } else {
      this.toaster.error('Please fill all required fields');
    }
  }
}
