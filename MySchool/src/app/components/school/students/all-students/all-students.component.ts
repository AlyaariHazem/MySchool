import { Component, inject } from '@angular/core';
import { MatDialog } from '@angular/material/dialog';
import { ToastrService } from 'ngx-toastr';

import { Students } from '../../../../core/models/students.model';

export interface DialogData {
  student: Students;
}

@Component({
  selector: 'app-all-students',
  templateUrl: './all-students.component.html',
  styleUrls: ['./all-students.component.scss']
})
export class AllStudentsComponent  {
  students: Array<Students> = [];

  toastr = inject(ToastrService);


  constructor(public dialog: MatDialog) {}

 


}
