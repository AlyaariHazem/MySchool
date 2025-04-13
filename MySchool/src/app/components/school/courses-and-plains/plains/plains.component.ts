import { Component } from '@angular/core';
import { FormBuilder, FormControl, FormGroup, Validators } from '@angular/forms';
import { MatDialog } from '@angular/material/dialog';
import { ToastrService } from 'ngx-toastr';
import { PaginatorState } from 'primeng/paginator';

import { LanguageService } from '../../../../core/services/language.service';
import { SubjectService } from '../../core/services/subject.service';
import { ClassNames } from '../../core/models/classes.model';
import { Curriculms } from '../../core/models/Curriculms.model';
import { CurriculmService } from '../../core/services/curriculm.service';
import { divisions } from '../../core/models/division.model';
import { Teachers } from '../../core/models/teacher.model';
import { Subjects } from '../../core/models/subjects.model';
import { CurriculmsPlan, CurriculmsPlans } from '../../core/models/curriculmsPlans.model';
import { CurriculmsPlanService } from '../../core/services/curriculms-plan.service';
import { ClassService } from '../../core/services/class.service';
import { DivisionService } from '../../core/services/division.service';
import { Terms } from '../../core/models/term.model';
import { TermService } from '../../core/services/term.service';
import { TeacherService } from '../../core/services/teacher.service';

@Component({
  selector: 'app-plains',
  templateUrl: './plains.component.html',
  styleUrls: [
    './plains.component.scss',
    './../../../../shared/styles/style-table.scss',
    './../../../../shared/styles/style-select.scss'
  ]
})
export class PlainsComponent {
  // Reactive Form
  form: FormGroup;

  // Data arrays
  subjects: Subjects[] = [];
  classes: ClassNames[] = [];
  divisions: divisions[] = [];
  fiteredDivisions: divisions[] = [];
  teachers: Teachers[] = [];
  terms: Terms[] = [];
  filteredTerms: Terms[] = [];

  ClassSubjects: Curriculms[] = [];

  // This is the array that you bind to the Subject dropdown after filtering
  filteredSubjects: Curriculms[] = [];

  // For listing existing plans in the table
  curriculmsPlan: CurriculmsPlans[] = [];

  // Pagination
  first: number = 0;
  rows: number = 4;
  curriculmsPlans: CurriculmsPlans[] = [];

  // Edit/Chips
  editMode: boolean = false;
  values = new FormControl<string[] | null>(null);
  max = 2;

  constructor(
    private formBuilder: FormBuilder,
    public dialog: MatDialog,
    public languageService: LanguageService,
    private subjectService: SubjectService,
    private curriculmsService: CurriculmService,
    private toastr: ToastrService,
    private divisionSerivce: DivisionService,
    private curriculmsPlanService: CurriculmsPlanService,
    private classService: ClassService,
    private termService:TermService,
    private teacherService: TeacherService
  ) {
    // Build the form group (include subjectID & yearID)
    this.form = this.formBuilder.group({
      classID: [null, Validators.required],
      subjectID: [null, Validators.required],
      divisionID: [null, Validators.required],
      teacherID: [null, Validators.required],
      termID: [null, Validators.required]
    });
  }

  ngOnInit(): void {
    // Example: fetch data from services
    this.subjectService.getAllSubjects().subscribe((res) => {
      this.subjects = res;
    });
    this.getAllCurriculmPlan();
    this.getAllCurriculm();
    this.getAllClasses();
    this.getAllDivision();
    this.getAllTerms();
    this.getAllTeachers();

    // When class changes, filter subjects:
    this.form.get('classID')?.valueChanges.subscribe((selectedClassID: number) => {
      // Filter ClassSubjects
      this.filteredSubjects = this.ClassSubjects.filter(c => c.classID === selectedClassID);
      this.fiteredDivisions= this.divisions.filter(d=>d.classID===selectedClassID);
      this.filteredTerms = this.terms.filter(t=>t.yearID===Number(localStorage.getItem('yearID')||'1'));
      // Clear the selected subject if class changes
      this.form.patchValue({ subjectID: null });
    });

    // Possibly set yearID from localStorage if you prefer
    const yearFromLocal = localStorage.getItem('yearID') || '1';
    this.form.patchValue({ yearID: Number(yearFromLocal) });

    // Initialize form & language
    this.form.reset();
    this.languageService.currentLanguage();
  }

  getAllCurriculmPlan(): void {
    this.curriculmsPlanService.getAllCurriculmPlan().subscribe(res => {
      this.curriculmsPlan = res;
      this.updatePaginatedData();
    });
  }

  getAllCurriculm(): void {
    this.curriculmsService.getAllCurriculm().subscribe(res => {
      this.ClassSubjects = res;
      // If you want to set filteredSubjects to all at the start, do it here
      this.filteredSubjects = [...this.ClassSubjects];
      this.updatePaginatedData();
    });
  }

  getAllClasses(): void {
    this.classService.GetAllNames().subscribe(res => {
      this.classes = res;
    }
    );
  }
  getAllDivision(): void {
    this.divisionSerivce.GetAll().subscribe(res => {
      this.divisions = res;
    });
  }
  getAllTerms(): void {
    this.termService.getAllTerm().subscribe(res => {
      this.terms = res;
    });
  }
  getAllTeachers(): void {
    this.teacherService.getAllTeacher().subscribe(res => {
      this.teachers = res;
      console.log('the teachers are',this.teachers);
    })
  }
  Add(): void {
    if (this.form.invalid) {
      console.log('Form is invalid');
      return;
    }
    const { classID, subjectID, divisionID, teacherID, termID } = this.form.value;
    const yearID = localStorage.getItem('yearID') || '1';
    // Build local CurriculmsPlan or your final object
    const localCurriculm: CurriculmsPlan = {
      subjectID,
      classID,
      divisionID,
      teacherID,
      termID,
      yearID: Number(yearID),
    };

    console.log('Form Data =>', localCurriculm);
    this.curriculmsPlanService.addCurriculmPlan(localCurriculm).subscribe(res => {
      this.toastr.success(res);
      this.getAllCurriculmPlan();
      this.updatePaginatedData();
    });
    // Force refresh table or do anything else

    // Reset form after add
    this.form.reset();
  }

  editCurriculum(curriculum: Curriculms): void {
    // Pre-fill form with existing data
    this.form.patchValue({
      subjectID: curriculum.subjectID,
      classID: curriculum.classID,
      note: curriculum.note,
      // etc...
    });
    this.editMode = true;
  }

  /**
   * Update the existing curriculum
   */
  updateCurriculum(): void {
    if (this.form.invalid) {
      console.log('Form is invalid');
      return;
    }
    const { subjectID, classID, note } = this.form.value;

    // Build local Curriculm
    const editCurriculms: Curriculms = {
      subjectID,
      classID,
      curriculumName: '',   // fill in or compute from subject/class if needed
      note,
      hireDate: new Date().toISOString(),
    };

    console.log('Editing =>', editCurriculms);
    this.editMode = false;
    this.form.reset();
  }

  deleteCurriculm(id1: number, id2: number): void {
    this.curriculmsService.deleteCurriculm(id1, id2).subscribe((res) => {
      this.toastr.success(res);
      this.getAllCurriculm();
    });
  }

  /**
   * Pagination Helpers
   */
  updatePaginatedData(): void {
    const start = this.first;
    const end = this.first + this.rows;
    this.curriculmsPlans = this.curriculmsPlan.slice(start, end);
  }

  onPageChange(event: PaginatorState): void {
    this.first = event.first || 0;
    this.rows = event.rows || 4;
    this.updatePaginatedData();
  }
}
