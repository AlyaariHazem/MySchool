import { Component, inject, Input, OnInit } from '@angular/core';
import { FormGroup } from '@angular/forms';
import { ClassService } from '../../../../../core/services/class.service';
import { DivisionService } from '../../../../../core/services/division.service';
import { ClassDTO } from '../../../../../core/models/class.model';
import { divisions } from '../../../../../core/models/division.model';

@Component({
  selector: 'app-primary-data',
  templateUrl: './primary-data.component.html',
  styleUrls: ['./primary-data.component.scss']
})
export class PrimaryDataComponent implements OnInit {
  @Input() formGroup!: FormGroup;

  selectedClass!: string;
  selectedDivision!: string;
  selectedSex!: string;

  classes: ClassDTO[] = [];
  divisiones: divisions[] = [];
  allDivisions: divisions[] = []; // Store all divisions

  Classes = inject(ClassService);
  Divisions = inject(DivisionService);

  isClassSelected: boolean = false;
  isDivisionSelected: boolean = false;
  isSexSelected: boolean = false;

  sexOptions: { value: string; label: string }[] = [
    { value: 'male', label: 'ذكر' },
    { value: 'female', label: 'أنثى' }
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
    } else if (type === 'division') {
      this.selectedDivision = value;
      this.isDivisionSelected = !!value;
    } else if (type === 'sex') {
      this.selectedSex = value;
      this.isSexSelected = !!value;
    }
  }

  updateDivisionsByClass(classId: string): void {
    // Filter divisions based on selected class
    this.divisiones = this.allDivisions.filter(
      (division) => division.classID === +classId
    );
  }

  clearSelection(type: string): void {
    if (type === 'Class') {
      this.selectedClass = '';
      this.isClassSelected = false;
      this.formGroup.get('Class')?.setValue(null);
      this.divisiones = []; // Clear divisions when class is cleared
    } else if (type === 'division') {
      this.selectedDivision = '';
      this.isDivisionSelected = false;
      this.formGroup.get('division')?.setValue(null);
    } else if (type === 'sex') {
      this.selectedSex = '';
      this.isSexSelected = false;
      this.formGroup.get('sex')?.setValue(null);
    }
  }
}
