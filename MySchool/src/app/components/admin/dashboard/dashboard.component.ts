import { Component, inject, OnInit } from '@angular/core';

import { StudentsServicesService } from '../../../core/services/student.service';
import { Students } from '../../../core/models/students.model';
import { ToastrService } from 'ngx-toastr';
import { MatDialog } from '@angular/material/dialog';

export interface DialogData {
    student: Students;
  }

@Component({
    selector: 'app-dashboard',
    templateUrl: './dashboard.component.html',
    styleUrl: './dashboard.component.scss',
})
export class DashboardComponent implements OnInit {
    students: Students[] = [];
    
   manageStudents = inject(StudentsServicesService);
  toastr = inject(ToastrService);
  constructor(public dialog: MatDialog) {}

  

  ngOnInit(): void {
    this.refreshStudent();
  }

  refreshStudent(): void {
    this.manageStudents.getStudents().subscribe(students => {
      this.students = students;
    });
  }

  deleteStudent(student: Students): void {
    this.manageStudents.deleteStudent(student.id).subscribe(() => {
      this.toastr.success('Student deleted successfully');
      this.refreshStudent();
    });
  }

  // this is for firebase upload 
}
