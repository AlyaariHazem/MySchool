import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormControl, FormGroup, Validators } from '@angular/forms';
import { MatDialog } from '@angular/material/dialog';
import { ToastrService } from 'ngx-toastr';
import { PaginatorState } from 'primeng/paginator';

import { LanguageService } from '../../../../core/services/language.service';
import { SubjectService } from '../../core/services/subject.service';
import { ClassService } from '../../core/services/class.service';
import { Subjects } from '../../core/models/subjects.model';
import { ClassNames } from '../../core/models/classes.model';
import { Curriculm, Curriculms } from '../../core/models/Curriculms.model';
import { CurriculmService } from '../../core/services/curriculm.service';

@Component({
  selector: 'app-courses',
  templateUrl: './courses.component.html',
  styleUrls: [
    './courses.component.scss',
    './../../../../shared/styles/style-table.scss',
    './../../../../shared/styles/style-select.scss'
  ]
})
export class CoursesComponent implements OnInit {
  form: FormGroup;

  subjects: Subjects[] = [];
  classes: ClassNames[] = [];
  curriculms: Curriculms[] = [];         // Entire array of curriculums

  editMode: boolean = false; // Flag to check if in edit mode

  values = new FormControl<string[] | null>(null);
  max = 2;

  constructor(
    private formBuilder: FormBuilder,
    public dialog: MatDialog,
    public languageService: LanguageService,
    private subjectService: SubjectService,
    private classService: ClassService,
    private curriculmsService: CurriculmService,
    private toastr: ToastrService
  ) {
    /** Initialize the Reactive Form */
    this.form = this.formBuilder.group({
      subjectID: [0, Validators.required],
      classID: [0, Validators.required],
      note: [''],
    });
  }

  ngOnInit(): void {
    this.subjectService.getAllSubjects().subscribe((res) => {
      this.subjects = res;
    });
    this.classService.GetAllNames().subscribe((res) => {
      this.classes = res;
    });
    this.getAllCurriculm();
    this.form.reset();
    // Set the current language
    this.languageService.currentLanguage();
  }
  getAllCurriculm():void{
    this.curriculmsService.getAllCurriculm().subscribe((res) => {
      this.curriculms = res;
      this.updatePaginatedData();
    }
    );
  }

  Add(): void {
    if (this.form.invalid) {
      console.log('Form is invalid');
      return;
    }

    const subjectID = this.form.get('subjectID')?.value;
    const classID = this.form.get('classID')?.value;

    const subjectName = this.subjects.find((subj) => subj.subjectID === subjectID)?.subjectName || '';
    const className = this.classes.find((cls) => cls.classID === classID)?.className || '';

    const curriculumName = `${subjectName}-${className}`;

    const localCurriculm: Curriculm = {
      subjectID,
      curriculumName,
      classID,
      note: this.form.get('note')?.value,
      hireDate: new Date().toISOString(),
    };

    const newCurriculms: Curriculms = {
      subjectID: localCurriculm.subjectID!,
      curriculumName: localCurriculm.curriculumName,
      classID: localCurriculm.classID!,
      note: localCurriculm.note || '',
      hireDate: localCurriculm.hireDate!,
    };

    this.curriculmsService.addCurriculm(newCurriculms).subscribe();
    newCurriculms.subjectName = subjectName;
    newCurriculms.className = className;
    this.curriculms.push(newCurriculms);
    this.updatePaginatedData();
    // Reset the form for a new entry
    this.form.reset();
  }
  
  editCurriculum(curriculum: Curriculms): void {
    this.form.patchValue({
      subjectID: curriculum.subjectID,
      classID: curriculum.classID,
      note: curriculum.note,
    });
    this.editMode=true;
  }
  updateCurriculum(): void {
    const subjectID = this.form.get('subjectID')?.value;
    const classID = this.form.get('classID')?.value;

    const subjectName = this.subjects.find((subj) => subj.subjectID === subjectID)?.subjectName || '';
    const className = this.classes.find((cls) => cls.classID === classID)?.className || '';

    const curriculumName = `${subjectName}-${className}`;

    // Build your local Curriculm object
    const localCurriculm: Curriculm = {
      subjectID,
      curriculumName,
      classID,
      note: this.form.get('note')?.value,
      hireDate: new Date().toISOString(),
    };

    // Adjust fields to match your exact interface
    const editCurriculms: Curriculms = {
      subjectID: localCurriculm.subjectID!,
      curriculumName: localCurriculm.curriculumName,
      classID: localCurriculm.classID!,
      note: localCurriculm.note || '',
      hireDate: localCurriculm.hireDate!,
    };
    this.curriculmsService.updateCurriculm(editCurriculms.subjectID!,editCurriculms.classID!, editCurriculms).subscribe(res=>{
      this.toastr.success(res);
      this.getAllCurriculm();
    });
    console.log('the edit',editCurriculms);
    this.editMode=false;
    this.form.reset();
  }
  
  deleteCurriculm(id1: number,id2:number): void {
    this.curriculmsService.deleteCurriculm(id1,id2).subscribe(res=>{
      this.toastr.success(res);
      this.getAllCurriculm();
    });
  }
  
  first: number = 0; // Current starting index for pagination
  rows: number = 4; // Number of rows per page
  displayCurriculums: Curriculms[] = [];
  updatePaginatedData(): void {
    const start = this.first;
    const end = this.first + this.rows;
    this.displayCurriculums = this.curriculms.slice(start, end);
  }
   onPageChange(event: PaginatorState): void {
      this.first = event.first || 0; // Update first index based on page
      this.rows = event.rows || 4; // Update rows per page
      this.updatePaginatedData();
    }
}
