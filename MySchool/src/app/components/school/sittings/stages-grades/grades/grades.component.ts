import { Component, HostListener, inject, OnInit } from '@angular/core';
import { ToastrService } from 'ngx-toastr';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { GradeService } from '../../../../../core/services/grade.service';
import { StageService } from '../../../../../core/services/stage.service';
import { Division, Grades, Stages } from '../../../../../core/models/stages-grades.modul';
import { combineLatest, map, Observable } from 'rxjs';
import { DivisionService } from '../../../../../core/services/division.service';

@Component({
  selector: 'app-grades',
  templateUrl: './grades.component.html',
  styleUrls: ['./grades.component.scss']
})
export class GradesComponent implements OnInit {
  
  stageForm: FormGroup;
  stages: Array<Stages> = [];
  grades: Array<Grades> = [];
  division: Array<Division> = [];

  private toastr = inject(ToastrService);
  private getStage = inject(StageService);

  constructor(private fb: FormBuilder, private stagesService: StageService,
    private gradesService: GradeService,
    private divisionsService: DivisionService) {
    this.stageForm = this.fb.group({
      id: '',
      grade: ['', [Validators.required, Validators.minLength(3)]],
      stage: ['', Validators.required],
      state: true
    });
  }

  refresh() {
    this.getStage.getStages().subscribe(res => {
      this.stages = res;
      console.log('the stages', this.stages);
    });
    this.gradesService.getGrades().subscribe(res => {
      this.grades = res;
      console.log('the grade is ', this.grades);
    })
  }

  onSubmit(): void {
    if (this.stageForm.valid) {
      const gradeData = this.stageForm.value;
      this.gradesService.addGrade(gradeData).subscribe(() => {
        this.toastr.success('تم إضافة الصف بنجاح');
        this.refresh();
      }, error => {
        this.toastr.error('حدث خطأ أثناء إضافة الصف');
        console.error(error);
      });
      console.log('Form Submitted', this.stageForm.value);
    } else {
      this.toastr.error('إدخل بيانات');
      console.log('Form is invalid', this.stageForm);
    }
  }
  //this is for combine data from stages and grades and divisions and to display the divison that in the grade
  combinedData$: Observable<any[]> | undefined;
  ngOnInit(): void {
    this.refresh();
    const stages$ = this.stagesService.getStages();
    const grades$ = this.gradesService.getGrades();
    const divisions$ = this.divisionsService.getDivision();

    this.combinedData$ = combineLatest([stages$, grades$, divisions$]).pipe(
      map(([stages, grades, divisions]) => {
        return grades.map(grade => {
          const stage = stages.find(stage => stage.id === grade.stage);
          const divisionsForGrade = divisions.filter(division => division.grade === grade.id);
          return {
            ...grade,
            stageName: stage ? stage.stage : '',
            divisions: divisionsForGrade,
            note: stage ? stage.note : '',
            state: stage ? stage.state : false
          };
        });
      })
    );
  }
  //this for nested dropdown list 
  openOuterDropdown: any = null;
  openInnerDropdown: any = null;
  openInnerDivision: any = null;

  toggleOuterDropdown(item: any): void {
    if (this.openOuterDropdown === item) {
      this.openOuterDropdown = null;
    } else {
      this.openOuterDropdown = item;
    }
  }

  isOuterDropdownOpen(item: any): boolean {
    return this.openOuterDropdown === item;
  }

  toggleInnerDropdown(item: any, division: any): void {
    if (this.openInnerDropdown === item && this.openInnerDivision === division) {
      this.openInnerDropdown = null;
      this.openInnerDivision = null;
    } else {
      this.openInnerDropdown = item;
      this.openInnerDivision = division;
    }
  }

  isInnerDropdownOpen(item: any, division: any): boolean {
    return this.openInnerDropdown === item && this.openInnerDivision === division;
  }

  @HostListener('document:click', ['$event'])
  clickout(event: Event) {
    const target = event.target as HTMLElement;

    if (!target.closest('.dropdown-menu') && !target.closest('.btn')) {
      this.openOuterDropdown = null;
      this.openInnerDropdown = null;
      this.openInnerDivision = null;
    }
  }
}
