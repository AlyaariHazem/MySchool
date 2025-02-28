import { Component, EventEmitter, inject, Output } from '@angular/core';
import { FormBuilder, FormGroup } from '@angular/forms';
import { YearService } from '../../../../../core/services/year.service';
import { Year } from '../../../../../core/models/year.model';
import { ToastrService } from 'ngx-toastr';

@Component({
  selector: 'app-new-year',
  templateUrl: './new-year.component.html',
  styleUrls: ['./new-year.component.scss']
})
export class NewYearComponent {
  formGroup: FormGroup;
  isActive: boolean = true;
  year!: Year;

  // Emit an event to the parent when a year is added successfully.
  @Output() yearAdded = new EventEmitter<Year>();

  yearService = inject(YearService);
  tosater = inject(ToastrService);

  currantSchool = localStorage.getItem('schoolId');
  
  constructor(private formBuilder: FormBuilder) {
    this.formGroup = this.formBuilder.group({
      yearDateStart: new Date(), // using new Date() instead of Date.now()
      yearDateEnd: new Date(),
      hireDate: [new Date().toISOString().split('T')[0]],
      active: true,
      schoolID: this.currantSchool
    });
  }

  toggleIsActive() {
    this.formGroup.get('active')!.setValue(!this.formGroup.get('active')!.value);
    this.isActive = !this.isActive;
  }

  yearAdd() {
    this.yearService.addYear(this.formGroup.value).subscribe(
      (response: any) => {
        console.log('Year added successfully', response);
        this.tosater.success("Added Year Successfully");
        this.yearAdded.emit(response as Year);
      },
      (error: any) => {
        console.error('Error adding year', error);
        this.tosater.error("Error Adding Year");
      }
    );
  }  
}
