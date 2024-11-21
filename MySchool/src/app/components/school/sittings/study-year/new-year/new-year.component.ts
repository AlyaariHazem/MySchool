import { Component, Inject } from '@angular/core';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';
import { StudyYearComponent } from '../study-year.component';

@Component({
  selector: 'app-new-year',
  templateUrl: './new-year.component.html',
  styleUrl: './new-year.component.scss'
})
export class NewYearComponent {
  constructor(public dialogRef: MatDialogRef<StudyYearComponent>, // Inject MatDialogRef
    @Inject(MAT_DIALOG_DATA) public data: any) { }

    closeModal(): void {
    this.dialogRef.close(); // Close the modal
  }

}
