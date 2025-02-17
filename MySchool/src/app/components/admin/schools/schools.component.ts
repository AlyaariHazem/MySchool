import { Component, OnInit } from '@angular/core';

@Component({
  selector: 'app-schools',
  templateUrl: './schools.component.html',
  styleUrls: ['./schools.component.scss']
})
export class SchoolsComponent implements OnInit {

  ngOnInit(): void {}

  schools = [
    { schoolName: 'مدرسة 1', schoolNameEn: 'School 1', schoolCreaDate: '2000-01-01', schoolType: 'male', city: 'مدينة 1', schoolPhone: '123456789', email: 'school1@example.com' },
    { schoolName: 'مدرسة 2', schoolNameEn: 'School 2', schoolCreaDate: '2005-05-10', schoolType: 'female', city: 'مدينة 2', schoolPhone: '987654321', email: 'school2@example.com' }
  ];
  displayedColumns: string[] = ['schoolName', 'schoolNameEn', 'schoolCreaDate', 'schoolType', 'city', 'schoolPhone', 'email'];

  openAddSchoolForm() {
    // Logic to open the form for adding a new school
  }
  
}
