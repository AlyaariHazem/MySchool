import { Component, inject, Input, OnInit } from '@angular/core';
import { FormGroup } from '@angular/forms';

import { ClassService } from '../../../../../core/services/class.service';
import { DivisionService } from '../../../../../core/services/division.service';
import { ClassDTO } from '../../../../../core/models/class.model';
import { divisions } from '../../../../../core/models/division.model';

@Component({
  selector: 'app-primary-data',
  templateUrl: './primary-data.component.html',
  styleUrls: [
    './primary-data.component.scss',
    '../../../../../shared/styles/style-input.scss']// this is very important
})
export class PrimaryDataComponent implements OnInit {
  @Input() formGroup!: FormGroup;

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
    this.getAllClass();
    this.getAllDivision();
  }

  getAllDivision(): void {
    this.Divisions.GetAll().subscribe({
      next: (res) => {
        this.allDivisions = res.divisionInfo; // Store all divisions
      }
    });
  }

  getAllClass(): void {
    this.Classes.GetAll().subscribe({
      next: (res) => (this.classes = res)
    });
  }

  onSelectionChange(type: string, value: string): void {
    if (type === 'Class') {
      this.selectedClass = value;
      this.isClassSelected = !!value;
      this.updateDivisionsByClass(value); // Filter divisions
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
  }

  clearSelection(type: string): void {
    if (type === 'Class') {
      this.selectedClass = '';
      this.isClassSelected = false;
      this.formGroup.get('Class')?.setValue(null);
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
