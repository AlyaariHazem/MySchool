import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';

@Component({
  selector: 'app-schools',
  templateUrl: './schools.component.html',
  styleUrls: ['./schools.component.scss']
})
export class SchoolsComponent implements OnInit {
  schoolForm: FormGroup;

  constructor(private fb: FormBuilder) {
    this.schoolForm = this.fb.group({
      schoolName: ['', Validators.required],
      schoolNameEn: ['', Validators.required],
      schoolCreaDate: ['', Validators.required],
      schoolVision: [''],
      schoolMission: [''],
      schoolGoal: ['', Validators.required],
      notes: [''],
      country: ['', Validators.required],
      city: ['', Validators.required],
      schoolPhone: ['', Validators.required],
      street: ['', Validators.required],
      schoolType: ['', Validators.required],
      email: ['', [Validators.required, Validators.email]],
      fax: [''],
      zone: ['', Validators.required]
    });
  }

  ngOnInit(): void {}

  onSubmit(): void {
    if (this.schoolForm.valid) {
      console.log(this.schoolForm.value);
      // Handle form submission logic here
    }
  }
}
