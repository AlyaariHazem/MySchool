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
  form: FormGroup;
  subjects: Subjects[] = [];
  classes: ClassNames[] = [];
  divisions: divisions[] = [];
  fiteredDivisions: divisions[] = [];
  teachers: Teachers[] = [];
  terms: Terms[] = [];
  ClassSubjects: Curriculms[] = [];
  filteredSubjects: Curriculms[] = [];
  curriculmsPlan: CurriculmsPlans[] = [];
  first: number = 0;
  rows: number = 4;
  curriculmsPlans: CurriculmsPlans[] = [];
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
    private termService: TermService,
    private teacherService: TeacherService
  ) {
    this.form = this.formBuilder.group({
      classID: [null, Validators.required],
      subjectID: [null, Validators.required],
      divisionID: [null, Validators.required],
      teacherID: [null, Validators.required],
      termID: [null, Validators.required]
    });
  }

  ngOnInit(): void {
    this.getAllSubjects();
    this.getAllCurriculmPlan();
    this.getAllCurriculm();
    this.getAllClasses();
    this.getAllDivision();
    this.getAllTerms();
    this.getAllTeachers();

    this.form.get('classID')?.valueChanges.subscribe((selectedClassID: number) => {
      this.filteredSubjects = this.ClassSubjects.filter(c => c.classID === selectedClassID);
      this.fiteredDivisions = this.divisions.filter(d => d.classID === selectedClassID);
      this.form.patchValue({ subjectID: null });
    });

    const yearFromLocal = localStorage.getItem('yearID') || '1';
    this.form.patchValue({ yearID: Number(yearFromLocal) });

    this.form.reset();
    this.languageService.currentLanguage();
  }
  getAllSubjects(): void {
    this.subjectService.getAllSubjects().subscribe({
      next: (res) => {
        if (!res.isSuccess) {
          this.toastr.warning(res.errorMasseges[0] || 'Failed to load subjects');
          return;
        }
        this.subjects = res.result;
      },
      error: () => this.toastr.error('Error fetching subjects')
    });
  }

  getAllCurriculmPlan(): void {
    this.curriculmsPlanService.getAllCurriculmPlan().subscribe({
      next: (res) => {
        if (!res.isSuccess) {
          this.toastr.warning(res.errorMasseges[0] || 'Failed to load curriculum plans');
          return;
        }
        this.curriculmsPlan = res.result;
        this.updatePaginatedData();
      },
      error: () => this.toastr.error('Error fetching curriculum plans')
    });
  }

  getAllCurriculm(): void {
    this.curriculmsService.getAllCurriculm().subscribe({
      next: (res) => {
        if (!res.isSuccess) {
          this.toastr.warning(res.errorMasseges[0] || 'Failed to load curriculum');
          return;
        }
        this.ClassSubjects = res.result;
        this.filteredSubjects = [...this.ClassSubjects];
        this.updatePaginatedData();
      },
      error: () => this.toastr.error('Error fetching curriculum data')
    });
  }

  getAllClasses(): void {
    this.classService.GetAllNames().subscribe({
      next: (res) => {
        if (!res.isSuccess) {
          this.toastr.warning(res.errorMasseges[0] || 'Failed to load classes.');
          return;
        }
        this.classes = res.result;
      },
      error: () => this.toastr.error('Server error occurred')
    });
  }

  getAllDivision(): void {
    this.divisionSerivce.GetAll().subscribe({
      next: (res) => {
        if (!res.isSuccess) {
          this.toastr.warning(res.errorMasseges[0] || 'Failed to load divisions.');
          return;
        }
        this.divisions = res.result;
      },
      error: () => this.toastr.error('Server error occurred')
    });
  }

  getAllTerms(): void {
    this.termService.getAllTerm().subscribe({
      next: (res) => {
        if (!res.isSuccess) {
          this.toastr.warning(res.errorMasseges[0] || 'Failed to load terms.');
          return;
        }
        this.terms = res.result;
      },
      error: () => this.toastr.error('Server error occurred')
    });
  }

  getAllTeachers(): void {
    this.teacherService.getAllTeacher().subscribe({
      next: (res) => {
        if (!res.isSuccess) {
          this.toastr.warning(res.errorMasseges[0] || 'Failed to load teachers.');
          return;
        }
        this.teachers = res.result;
      },
      error: () => this.toastr.error('Server error occurred')
    });
  }

  Add(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    const { classID, subjectID, divisionID, teacherID, termID } = this.form.value;
    const yearID = Number(localStorage.getItem('yearID') || '1');

    const localCurriculm: CurriculmsPlan = {
      subjectID,
      classID,
      divisionID,
      teacherID,
      termID,
      yearID: Number(yearID),
    };

    this.curriculmsPlanService.addCurriculmPlan(localCurriculm).subscribe({
      next: (res) => {
        if (!res.isSuccess) {
          this.toastr.warning(res.errorMasseges[0] || 'Failed to add curriculum');
          return;
        }
        this.toastr.success(res.result || 'Curriculum added successfully');
        this.getAllCurriculmPlan();
      },
      error: () => this.toastr.error('Server error occurred')
    });

    this.form.reset();
  }

  editCurriculum(curriculum: Curriculms): void {
    this.form.patchValue({
      subjectID: curriculum.subjectID,
      classID: curriculum.classID,
      note: curriculum.note
    });
    this.editMode = true;
  }

  updateCurriculum(): void {
    if (this.form.invalid) {
      return;
    }

    const { subjectID, classID, note } = this.form.value;

    const editCurriculms: Curriculms = {
      subjectID,
      classID,
      curriculumName: '',
      note,
      hireDate: new Date().toISOString()
    };
    console.log('Editing =>', editCurriculms);
    this.editMode = false;
    this.form.reset();
  }

  deleteCurriculm(id1: number, id2: number): void {
    this.curriculmsService.deleteCurriculm(id1, id2).subscribe({
      next: (res) => {
        if (!res.isSuccess) {
          this.toastr.warning(res.errorMasseges[0] || 'Failed to delete curriculum');
          return;
        }
        this.toastr.success(res.result || 'Curriculum deleted');
        this.getAllCurriculm();
      },
      error: () => this.toastr.error('Error while deleting curriculum')
    });
  }

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
