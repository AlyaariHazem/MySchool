import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormControl, FormGroup, Validators } from '@angular/forms';
import { MatDialog } from '@angular/material/dialog';
import { ToastrService } from 'ngx-toastr';
import { PaginatorState } from 'primeng/paginator';
import { Store } from '@ngrx/store';
import { map } from 'rxjs';

import { SubjectService } from '../../core/services/subject.service';
import { ClassService } from '../../core/services/class.service';
import { Subjects } from '../../core/models/subjects.model';
import { ClassNames } from '../../core/models/classes.model';
import { Curriculms } from '../../core/models/Curriculms.model';
import { CurriculmService } from '../../core/services/curriculm.service';
import { selectLanguage } from '../../../../core/store/language/language.selectors';

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
  isLoading: boolean = true;

  values = new FormControl<string[] | null>(null);
  max = 2;

  readonly dir$ = this.store.select(selectLanguage).pipe(
    map(l => (l === 'ar' ? 'rtl' : 'ltr')),
  );

  constructor(
    private formBuilder: FormBuilder,
    public dialog: MatDialog,
    private store:Store,
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
    this.getAllClasses();
    this.getAllSubjects();
    this.getAllCurriculm();
    this.form.reset();
  }

  getAllCurriculm(): void {
    this.curriculmsService.getAllCurriculm().subscribe({
      next: (res) => {
        if (!res.isSuccess) {
          this.toastr.warning(res.errorMasseges[0] || 'Failed to load curriculums');
          this.curriculms = [];
          return;
        }

        this.curriculms = res.result;
        this.updatePaginatedData();
      },
      error: () => {
        this.toastr.error('Server error while loading curriculums');
        this.curriculms = [];
      }
    });
  }
  getAllSubjects(): void {
    this.isLoading = true;
    this.subjectService.getAllSubjects().subscribe({
      next: (res) => {
        if (!res.isSuccess) {
          this.toastr.warning(res.errorMasseges[0] || 'Failed to load subjects');
          return;
        }
        this.subjects = res.result;
        this.isLoading = false;
      },
      error: (err) => {
        this.toastr.error('Server error occurred');
        console.error(err);
      }
    });
  }
  getAllClasses(): void {
    this.isLoading = true;
    this.classService.GetAllNames().subscribe({
      next: (res) => {
        if (!res.isSuccess) {
          this.toastr.warning(res.errorMasseges[0] || 'Failed to load classes.');
          return;
        }
        this.classes = res.result;
        this.isLoading = false;
      },
      error: (err) => {
        this.toastr.error('Server error occurred');
        console.error(err);
      }
    });
  }

  Add(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }
    const subjectID = this.form.get('subjectID')?.value;
    const classID = this.form.get('classID')?.value;

    const subjectName = this.subjects.find((subj) => subj.subjectID === subjectID)?.subjectName || '';
    const className = this.classes.find((cls) => cls.classID === classID)?.className || '';

    const curriculumName = `${subjectName}-${className}`;
    const newCurriculm: Curriculms = {
      subjectID,
      classID,
      curriculumName,
      note: this.form.get('note')?.value || '',
      hireDate: new Date().toISOString(),
    };

    this.curriculmsService.addCurriculm(newCurriculm).subscribe({
      next: (res) => {
        if (!res.isSuccess) {
          this.toastr.warning(res.errorMasseges[0] || 'Failed to add curriculum');
          return;
        }

        newCurriculm.subjectName = subjectName;
        newCurriculm.className = className;
        this.curriculms.push(newCurriculm);
        this.updatePaginatedData();
        this.form.reset();
        this.toastr.success('Curriculum added successfully');
      },
      error: (err) => {
        console.log(err);
        this.toastr.error('this Curriculum is existing')
      }
    });
  }


  editCurriculum(curriculum: Curriculms): void {
    this.form.patchValue({
      subjectID: curriculum.subjectID,
      classID: curriculum.classID,
      note: curriculum.note,
    });
    this.editMode = true;
  }
  updateCurriculum(): void {
    const subjectID = this.form.get('subjectID')?.value;
    const classID = this.form.get('classID')?.value;

    const subjectName = this.subjects.find((s) => s.subjectID === subjectID)?.subjectName || '';
    const className = this.classes.find((c) => c.classID === classID)?.className || '';
    const curriculumName = `${subjectName}-${className}`;

    const updated: Curriculms = {
      subjectID,
      classID,
      curriculumName,
      note: this.form.get('note')?.value || '',
      hireDate: new Date().toISOString(),
    };

    this.curriculmsService.updateCurriculm(subjectID, classID, updated).subscribe({
      next: (res) => {
        if (!res.isSuccess) {
          this.toastr.warning(res.errorMasseges[0] || 'Failed to update curriculum');
          return;
        }

        this.toastr.success(res.result || 'Curriculum updated successfully');
        this.getAllCurriculm();
        this.form.reset();
        this.editMode = false;
      },
      error: () => this.toastr.error('Server error while updating curriculum')
    });
  }

  deleteCurriculm(id1: number, id2: number): void {
    this.curriculmsService.deleteCurriculm(id1, id2).subscribe({
      next: (res) => {
        if (!res.isSuccess) {
          this.toastr.warning(res.errorMasseges[0] || 'Failed to delete curriculum');
          return;
        }

        this.toastr.success(res.result || 'Curriculum deleted successfully');
        this.getAllCurriculm();
      },
      error: () => this.toastr.error('Server error while deleting curriculum')
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
