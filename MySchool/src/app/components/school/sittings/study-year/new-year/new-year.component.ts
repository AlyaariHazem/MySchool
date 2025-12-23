import { Component, EventEmitter, inject, Input, OnInit, OnChanges, SimpleChanges, Output } from '@angular/core';
import { FormBuilder, FormGroup } from '@angular/forms';
import { YearService } from '../../../../../core/services/year.service';
import { Year } from '../../../../../core/models/year.model';
import { ToastrService } from 'ngx-toastr';

@Component({
  selector: 'app-new-year',
  templateUrl: './new-year.component.html',
  styleUrls: ['./new-year.component.scss']
})
export class NewYearComponent implements OnInit, OnChanges {
  formGroup: FormGroup;
  isActive: boolean = true;
  
  @Input() year: Year | null = null;
  @Input() isEditMode: boolean = false;

  // Emit an event to the parent when a year is added successfully.
  @Output() yearAdded = new EventEmitter<Year>();

  yearService = inject(YearService);
  tosater = inject(ToastrService);

  currantSchool = localStorage.getItem('schoolId');
  
  constructor(private formBuilder: FormBuilder) {
    const schoolId = this.currantSchool ? Number(this.currantSchool) : null;
    this.formGroup = this.formBuilder.group({
      yearDateStart: new Date(),
      yearDateEnd: new Date(),
      hireDate: [new Date().toISOString().split('T')[0]],
      active: true,
      schoolID: schoolId
    });
  }

  ngOnInit() {
    // Initialize form for new year by default
    const schoolId = this.currantSchool ? Number(this.currantSchool) : null;
    this.formGroup.patchValue({
      schoolID: schoolId
    });
  }

  ngOnChanges(changes: SimpleChanges) {
    // React to changes in the year input or edit mode
    if (changes['year'] || changes['isEditMode']) {
      if (this.isEditMode && this.year) {
        this.populateForm(this.year);
      } else if (!this.isEditMode) {
        // Reset form for new year mode
        this.resetForm();
      } else if (this.isEditMode && !this.year) {
        // Year cleared in edit mode - reset form
        this.resetForm();
      }
    }
  }

  private populateForm(year: Year): void {
    // Populate form with existing year data
    const yearDateStart = year.yearDateStart ? new Date(year.yearDateStart) : new Date();
    const yearDateEnd = year.yearDateEnd ? new Date(year.yearDateEnd) : new Date();
    const hireDate = year.hireDate ? new Date(year.hireDate).toISOString().split('T')[0] : new Date().toISOString().split('T')[0];
    
    // Ensure schoolID is a number, fallback to current school if null
    const schoolId = year.schoolID ? Number(year.schoolID) : (this.currantSchool ? Number(this.currantSchool) : null);
    
    this.formGroup.patchValue({
      yearDateStart: yearDateStart,
      yearDateEnd: yearDateEnd,
      hireDate: hireDate,
      active: year.active,
      schoolID: schoolId
    });
    
    this.isActive = year.active;
  }

  private resetForm(): void {
    // Reset form to default values for new year
    const schoolId = this.currantSchool ? Number(this.currantSchool) : null;
    this.formGroup.patchValue({
      yearDateStart: new Date(),
      yearDateEnd: new Date(),
      hireDate: new Date().toISOString().split('T')[0],
      active: true,
      schoolID: schoolId
    });
    this.isActive = true;
  }

  toggleIsActive() {
    this.formGroup.get('active')!.setValue(!this.formGroup.get('active')!.value);
    this.isActive = !this.isActive;
  }

  yearAdd() {
    // Ensure schoolID is a number
    const formValue = { ...this.formGroup.value };
    formValue.schoolID = formValue.schoolID ? Number(formValue.schoolID) : (this.currantSchool ? Number(this.currantSchool) : null);
    
    if (this.isEditMode && this.year) {
      // Update existing year - ensure yearID is included
      const yearData: Year = {
        ...formValue,
        yearID: this.year.yearID
      };
      
      this.yearService.updateYear(yearData, this.year.yearID).subscribe({
        next: (response: Year) => {
          console.log('Year updated successfully', response);
          this.tosater.success("تم تحديث السنة الدراسية بنجاح");
          this.yearAdded.emit(response);
        },
        error: (error: any) => {
          console.error('Error updating year', error);
          this.tosater.error("فشل تحديث السنة الدراسية");
        }
      });
    } else {
      // Add new year
      this.yearService.addYear(formValue).subscribe({
        next: (response: any) => {
          console.log('Year added successfully', response);
          this.tosater.success("تم إضافة السنة الدراسية بنجاح");
          // Handle both direct response and wrapped response
          const year = response?.result || response;
          this.yearAdded.emit(year as Year);
        },
        error: (error: any) => {
          console.error('Error adding year', error);
          this.tosater.error("فشل إضافة السنة الدراسية");
        }
      });
    }
  }  
}
