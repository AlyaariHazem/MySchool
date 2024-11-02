import { Component, inject, OnInit } from '@angular/core';
import { FormBuilder, FormGroup } from '@angular/forms';
import { ToastrService } from 'ngx-toastr';
import { DivisionService } from '../../../../../core/services/division.service';
import { GradeService } from '../../../../../core/services/grade.service';
import { Division, Grades } from '../../../../../core/models/stages-grades.modul';

@Component({
  selector: 'app-division',
  templateUrl: './division.component.html',
  styleUrls: ['./division.component.scss']
})
export class DivisionComponent implements OnInit {
  division: Array<Division> = [];
  form: FormGroup;
  grades: Array<Grades> = [];

  divisionService = inject(DivisionService);
  gradeService = inject(GradeService);
  toastr = inject(ToastrService);

  constructor(private formBuilder: FormBuilder) {
    this.form = this.formBuilder.group({
      id: '',
      division: '',
      grade: '',
      state: true
    });
  }

  ngOnInit(): void {
    this.gradeService.getGrades().subscribe(res => {
      this.grades = res;
    });
    this.loadDivisions();
  }

  loadDivisions() {
    this.divisionService.getDivision().subscribe(res => {
      this.division = res;
    });
  }

  addDivision() {
    if (this.form.valid) {
      this.divisionService.addDivision(this.form.value).subscribe(() => {
        this.toastr.success('تم إضافة الشعبة بنجاح');
        this.loadDivisions(); // Refresh the list of divisions
      });
    } else {
      this.toastr.error('يجب إدخال بيانات ');
    }
  }

  getGradeName(gradeId: string): string {
    const grade = this.grades.find(g => g.id === gradeId);
    return grade ? grade.grade : 'N/A';
  }
  toggleStateDropdown(item: any) {
    item.isDropdownOpen = !item.isDropdownOpen;
  }

  changeState(item: any, newState: boolean) {
    item.state = newState;
    this.divisionService.editDivision(item).subscribe(() => {
      this.toastr.success('State updated successfully');
    });
  }
}
