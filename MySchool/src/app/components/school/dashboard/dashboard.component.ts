import { Component, inject, OnInit } from '@angular/core';

import { StudentsServicesService } from '../../../core/services/student.service';
import { Students } from '../../../core/models/students.model';
import { ToastrService } from 'ngx-toastr';
import { MatDialog } from '@angular/material/dialog';
import { EditStudentComponent } from '../students/edit-student/edit-student.component';

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

  openDialog(student: Students): void {
    const dialogRef = this.dialog.open(EditStudentComponent, {
      data: { student: student },
    });

    dialogRef.afterClosed().subscribe((result) => {
      if (result) {
        // Update the student list with the edited student
        this.students = this.students.map((s) => (s.id === result.id ? result : s));
        // Refresh the student list
        this.refreshStudent();
      }
      this.refreshStudent();
    });
    this.refreshStudent();
  }
  

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
