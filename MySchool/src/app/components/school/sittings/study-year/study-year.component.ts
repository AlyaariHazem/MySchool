import { Component, inject } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ToastrService } from 'ngx-toastr';
import { ClassDTO } from '../../../../core/models/class.model';
import { MatDialog, MatDialogConfig } from '@angular/material/dialog';
import { NewYearComponent } from './new-year/new-year.component';


@Component({
  selector: 'app-study-year',
  templateUrl: './study-year.component.html',
  styleUrl: './study-year.component.scss'
})
export class StudyYearComponent {
  constructor(public dialog: MatDialog, private toastr:ToastrService){}
  
  openDialog(): void {
    const dialogConfig = new MatDialogConfig();
    dialogConfig.width = '80%';
    dialogConfig.height = '70%';
    dialogConfig.panelClass = 'custom-dialog-container';

    const dialogRef = this.dialog.open(NewYearComponent, dialogConfig);

    dialogRef.afterClosed().subscribe((result) => {
      if (result) {
        this.toastr.success('تم إضافة الطالب بنجاح');
      }
    });
  }


}
