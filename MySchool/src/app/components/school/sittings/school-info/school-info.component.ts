import { Component, inject, OnInit } from '@angular/core';
import { FormBuilder, FormGroup } from '@angular/forms';
import { SchoolService } from '../../../../core/services/school.service';
import { School } from '../../../../core/models/school.modul';

@Component({
    selector: 'app-school-info',
    templateUrl: './school-info.component.html',
    styleUrls: ['./school-info.component.scss',
        './../../../../shared/styles/style-primeng-input.scss',
        './../../../../shared/styles/style-select.scss',
    ]
})
export class SchoolInfoComponent implements OnInit {
  private schoolService = inject(SchoolService);

  form: FormGroup;
  Books: any[] = [];
  classes: any[] = [];
  school: School[] = [];

  constructor(private fb: FormBuilder) {
    this.form = this.fb.group({
      schoolID:[1],
      schoolName:[''],
      schoolNameEn: [''],
      schoolVison: [''],
      schoolType: [''],
      hireDate: ['2024-01-01'],
      schoolGoal: [''],
      notes: [''],
      country: [''],
      city: [''],
      schoolPhone: [''],
      schoolMission: [''],
      street: [''],
      email: [''],
      fax: [''],
      zone: [''],
    });
  }

  ngOnInit(): void {
    this.schoolService.getAllSchools().subscribe((res) => {
      this.school = res;
      console.log('the schools are', this.school)
      if (res.length > 0) {
        this.initializeForm(res[0]); // Initialize the form with the first school data
      }
    });

    this.Books = [
      { name: 'Public', code: 'PUBLIC' },
      { name: 'Private', code: 'PRIVATE' },
    ];

    this.classes = [
      { name: 'Primary', code: 'PRIMARY' },
      { name: 'Secondary', code: 'SECONDARY' },
    ];
  }

 initializeForm(school: School): void {
  this.form.patchValue({
    schoolName: school.schoolName,
    schoolNameEn: school.schoolNameEn,
    schoolType: school.schoolType,
    email: school.email,
    country: school.country,
    schoolPhone: school.schoolPhone,
    city: school.city,
    zone: school.zone,
    street: school.street,
    fax: school.fax,
    schoolVison: school.schoolVison,
    schoolMission: school.schoolMission
  });
}


  uploadPhoto(event: Event): void {
    // Logic for uploading photos
    console.log('Photo uploaded!');
  }

  onSubmit(): void {
    if (this.form.valid) {
      console.log('added successfully',this.form);
      this.schoolService.updateSchool(this.school[0].schoolID,this.form.value).subscribe(res=>{
        console.log('added successfully',res);
      })
    } else {
      console.log('Form is invalid!',this.form.value);
    }
  }
}
