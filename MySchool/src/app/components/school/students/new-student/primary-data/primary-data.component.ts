import { Component, EventEmitter, inject, Input, OnInit, Output } from '@angular/core';
import { FormGroup } from '@angular/forms';

import { ClassService } from '../../../core/services/class.service';
import { DivisionService } from '../../../core/services/division.service';
import { ClassDTO } from '../../../core/models/class.model';
import { divisions } from '../../../core/models/division.model';

@Component({
  selector: 'app-primary-data',
  templateUrl: './primary-data.component.html',
  styleUrls: [
    './primary-data.component.scss',
    '../../../../../shared/styles/style-input.scss']// this is very important
})
export class PrimaryDataComponent implements OnInit {
  @Input() formGroup!: FormGroup;
  @Output() classSelected = new EventEmitter<number>();

  get fullNameAr(): string {
    return `${this.formGroup.get('studentFirstName')?.value} ${this.formGroup.get('studentMiddleName')?.value} ${this.formGroup.get('studentLastName')?.value}`.trim();
  }

  get fullNameEn(): string {
    return `${this.formGroup.get('studentFirstNameEng')?.value} ${this.formGroup.get('studentMiddleNameEng')?.value} ${this.formGroup.get('studentLastNameEng')?.value}`.trim();
  }

  selectedClass!: string;
  selectedDivision!: string;
  selectedSex!: string;

  classes: ClassDTO[] = [];
  divisions: divisions[] = [];
  allDivisions: divisions[] = []; // Store all divisions

  Classes = inject(ClassService);
  Divisions = inject(DivisionService);

  isClassSelected: boolean = false;
  isDivisionSelected: boolean = false;
  isSexSelected: boolean = false;

  genderOptions: { value: string; label: string }[] = [
    { value: 'Male', label: 'ذكر' },
    { value: 'Female', label: 'أنثى' }
  ];

  ngOnInit(): void {
    this.getAllDivision();
    this.getAllClass();

    const dobValue = this.formGroup.get('studentDOB')?.value;
    if (dobValue) {
      this.formGroup.get('studentDOB')?.setValue(this.formatDateForInput(dobValue));
    }

    const initialClass = this.formGroup.get('classID')?.value;
    if (initialClass) {
      this.onSelectionChange('classID', initialClass);
    }

    console.log('Class id is:', this.formGroup.get('classID')?.value);
    console.log('Primary Data Group:', this.formGroup.value);
  }

  formatDateForInput(isoDate: string): string {
    const date = new Date(isoDate);
    const year = date.getFullYear();
    const month = String(date.getMonth() + 1).padStart(2, '0'); // Ensure 2 digits
    const day = String(date.getDate()).padStart(2, '0'); // Ensure 2 digits
    return `${year}-${month}-${day}`;
  }

  getAllDivision(): void {
    this.Divisions.GetAll().subscribe({
      next: (res) => {
        this.allDivisions = res.result;

        const initialClassId = this.formGroup.get('classID')?.value;
        if (initialClassId) {
          this.updateDivisionsByClass(initialClassId);
        }
      },
      error: (err) => console.error('Error loading divisions:', err)
    });
  }


  getAllClass(): void {
    this.Classes.GetAll().subscribe({
      next: (res) => {
        if(!res.isSuccess){
          console.error('Error loading classes:', res.errorMasseges[0]);
        }
        this.classes = res.result;
      },
      error: (err) => console.error('Error loading classes:', err)
    });
  }


  onSelectionChange(type: string, value: string): void {
    if (type === 'classID') {
      this.selectedClass = value;
      this.isClassSelected = !!value;
      this.classSelected.emit(+value);
      this.updateDivisionsByClass(value); // Automatically set the first division
    } else if (type === 'divisionID') {
      this.selectedDivision = value;
      this.isDivisionSelected = !!value;
    } else if (type === 'studentGender') {
      this.selectedSex = value;
      this.isSexSelected = !!value;
    }
  }
  
  updateDivisionsByClass(classId: string): void {
    // Filter divisions based on selected class
    this.divisions = this.allDivisions.filter(
      (division) => division.classID === +classId
    );
    
    if (this.divisions.length > 0) {
      const firstDivisionId = this.divisions[0].divisionID;
      this.formGroup.get('divisionID')?.setValue(firstDivisionId);
      this.selectedDivision = firstDivisionId.toString(); // Convert to string
      this.isDivisionSelected = true;
    } else {
      this.formGroup.get('divisionID')?.setValue(null);
      this.selectedDivision = '';
      this.isDivisionSelected = false;
    }
  }
  
  clearSelection(type: string): void {
    if (type === 'classID') {
      this.selectedClass = '';
      this.isClassSelected = false;
      this.formGroup.get('classID')?.setValue(null);
      this.divisions = []; // Clear divisions when class is cleared
    } else if (type === 'divisionID') {
      this.selectedDivision = '';
      this.isDivisionSelected = false;
      this.formGroup.get('divisionID')?.setValue(null);
    } else if (type === 'studentGender') {
      this.selectedSex = '';
      this.isSexSelected = false;
      this.formGroup.get('studentGender')?.setValue(null);
    }
  }
}
