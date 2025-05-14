import { Component, inject, OnInit } from '@angular/core';
import { MatDialog } from '@angular/material/dialog';

import { SchoolInfoComponent } from '../school-info/school-info.component';
import { SchoolService } from '../../../core/services/school.service';
import { School } from '../../../core/models/school.modul';
import { PaginatorService } from '../../../core/services/paginator.service';
import { PaginatorState } from 'primeng/paginator';

@Component({
  selector: 'app-schools',
  templateUrl: './schools.component.html',
  styleUrls: ['./schools.component.scss']
})
export class SchoolsComponent implements OnInit {

  paginatorService = inject(PaginatorService);
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
        this.paginatedSchools = this.paginatorService.pageSlice(this.schools);
      },
    );
  }

  handlePageChange(event: PaginatorState): void {
      this.paginatorService.onPageChange(event);
      this.paginatedSchools = this.paginatorService.pageSlice(this.schools);
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

}