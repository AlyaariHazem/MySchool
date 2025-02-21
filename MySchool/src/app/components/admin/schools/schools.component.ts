import { Component, OnInit } from '@angular/core';
import { MatDialog } from '@angular/material/dialog';

import { SchoolInfoComponent } from '../school-info/school-info.component';
import { SchoolService } from '../../../core/services/school.service';
import { School } from '../../../core/models/school.modul';
import { PaginatorState } from 'primeng/paginator';

@Component({
  selector: 'app-schools',
  templateUrl: './schools.component.html',
  styleUrls: ['./schools.component.scss']
})
export class SchoolsComponent implements OnInit {

  schools: School[] = [];
  paginatedSchools: School[] = [];
  constructor(private schoolService: SchoolService, public dialog: MatDialog) { }

  ngOnInit(): void {
    this.getAllSchools();
    
  }

  getAllSchools(): void {
    this.schoolService.getAllSchools().subscribe(
      res => {
        this.schools = res;
        this.updatePaginatedData();
      },
    );
  }

  displayedColumns: string[] = ['schoolName', 'schoolNameEn', 'schoolCreaDate', 'schoolType', 'city', 'schoolPhone', 'email', 'actions'];
  //how can I fix this to wrok fine?
  openAddSchoolForm() {
    // Opens the form dialog in "Add" mode
    this.dialog.open(SchoolInfoComponent, {
      width: '80%',
      height: '80%',
      data: { isEditMode: false } // optional
    });
  }

  openEditSchoolForm(school: School) {
    // Opens the form dialog in "Edit" mode, passing the school data
    this.dialog.open(SchoolInfoComponent, {
      width: '80%',
      height: '80%',
      data: { isEditMode: true, schoolData: school }
    });
  }

  deleteSchool(school: School) {
    if (confirm('هل أنت متأكد من حذف هذه المدرسة؟')) {
      this.schoolService.deleteSchool(school.schoolID!).subscribe({
        next: () => {
          // Remove it from the local array if needed
          this.schools = this.schools.filter(s => s.schoolID !== school.schoolID);
        },
        error: (err) => {
          console.error('Error deleting school:', err);
        }
      });
    }
  }
  first: number = 0;
  rows: number = 4;
  updatePaginatedData(): void {
    const start = this.first;
    const end = this.first + this.rows;
    this.paginatedSchools = this.schools.slice(start, end);
  }

  onPageChange(event: PaginatorState) {
    this.first = event.first || 0; // Default to 0 if undefined
    this.rows = event.rows!;
    this.updatePaginatedData();
  }
}