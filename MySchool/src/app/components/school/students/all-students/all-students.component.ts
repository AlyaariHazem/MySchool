import { Component, inject } from '@angular/core';
import { MatDialog } from '@angular/material/dialog';
import { ToastrService } from 'ngx-toastr';


export interface DialogData {
}

@Component({
    selector: 'app-all-students',
    templateUrl: './all-students.component.html',
    styleUrls: ['./all-students.component.scss']
})
export class AllStudentsComponent  {

  toastr = inject(ToastrService);


  constructor(public dialog: MatDialog) {}

 


}
