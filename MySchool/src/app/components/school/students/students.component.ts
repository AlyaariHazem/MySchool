import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { StageService } from '../../../core/services/stage.service';
import { ToastrService } from 'ngx-toastr';
import { Observable } from 'rxjs';
import { MatDialog, MatDialogConfig } from '@angular/material/dialog';
import { NewStudentComponent } from './new-student/new-student.component';

@Component({
  selector: 'app-students',
  templateUrl: './students.component.html',
  styleUrls: ['./students.component.scss']
})
export class StudentsComponent implements OnInit {
  activeTab: string = 'News';
  form: FormGroup;
  combinedData$: Observable<any[]> | undefined;
  outerDropdownState: { [key: string]: boolean } = {};
  innerDropdownState: { [key: string]: { [key: string]: boolean } } = {};
  currentPage: { [key: string]: number } = {};

  constructor(
    private formBuilder: FormBuilder,
    private toastr: ToastrService,
    public dialog: MatDialog
  ) {
    this.form = this.formBuilder.group({
      id: '',
      stage: ['', Validators.required],
      note: '',
      state: true
    });
  }

  ngOnInit(): void {
    
  }

  openDialog(): void {
    const dialogConfig = new MatDialogConfig();
    dialogConfig.width = '80%';
    dialogConfig.data = {}; // Pass any necessary data here
    dialogConfig.panelClass = 'custom-dialog-container'; // Apply the custom CSS class

    const dialogRef = this.dialog.open(NewStudentComponent, dialogConfig);

    dialogRef.afterClosed().subscribe(result => {
      // Handle the result if needed
    });
  }

}
