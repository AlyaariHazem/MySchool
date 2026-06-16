import { Component, OnInit } from '@angular/core';
import { finalize } from 'rxjs';
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
  totalCurriculums = 0;
  displayCurriculums: Curriculms[] = [];

  editMode: boolean = false; // Flag to check if in edit mode
  isLoading: boolean = true;
  isFetchingCurriculums = false;
  isMutating = false;

  get isBusy(): boolean {
    return this.isLoading || this.isFetchingCurriculums || this.isMutating;
  }

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
    this.loadCurriculumsPage();
    this.form.reset();
  }

  loadCurriculumsPage(): void {
    this.isFetchingCurriculums = true;
    const pageIndex = this.rows > 0 ? Math.floor(this.first / this.rows) : 0;
    this.curriculmsService.getCurriculmPage({ pageIndex, pageSize: this.rows }).pipe(
      finalize(() => {
        this.isFetchingCurriculums = false;
      }),
    ).subscribe({
      next: (res) => {
        if (!res.isSuccess || !res.result) {
          this.toastr.warning(res.errorMasseges[0] || 'Failed to load curriculums');
          this.displayCurriculums = [];
          this.totalCurriculums = 0;
          return;
        }
        const p = res.result;
        if (p.data.length === 0 && p.totalCount > 0 && pageIndex > 0) {
          this.first = (pageIndex - 1) * this.rows;
          this.loadCurriculumsPage();
          return;
        }
        this.displayCurriculums = p.data;
        this.totalCurriculums = p.totalCount;
      },
      error: () => {
        this.toastr.error('Server error while loading curriculums');
        this.displayCurriculums = [];
        this.totalCurriculums = 0;
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
    if (this.isBusy) {
      return;
    }
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

    this.isMutating = true;
    this.curriculmsService.addCurriculm(newCurriculm).pipe(
      finalize(() => {
        this.isMutating = false;
      }),
    ).subscribe({
      next: (res) => {
        if (!res.isSuccess) {
          this.toastr.warning(res.errorMasseges[0] || 'Failed to add curriculum');
          return;
        }

        newCurriculm.subjectName = subjectName;
        newCurriculm.className = className;
        this.loadCurriculumsPage();
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
    if (this.isBusy) {
      return;
    }
    this.form.patchValue({
      subjectID: curriculum.subjectID,
      classID: curriculum.classID,
      note: curriculum.note,
    });
    this.editMode = true;
  }
  updateCurriculum(): void {
    if (this.isBusy) {
      return;
    }
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

    this.isMutating = true;
    this.curriculmsService.updateCurriculm(subjectID, classID, updated).pipe(
      finalize(() => {
        this.isMutating = false;
      }),
    ).subscribe({
      next: (res) => {
        if (!res.isSuccess) {
          this.toastr.warning(res.errorMasseges[0] || 'Failed to update curriculum');
          return;
        }

        this.toastr.success(res.result || 'Curriculum updated successfully');
        this.loadCurriculumsPage();
        this.form.reset();
        this.editMode = false;
      },
      error: () => this.toastr.error('Server error while updating curriculum')
    });
  }

  deleteCurriculm(id1: number, id2: number): void {
    if (this.isBusy) {
      return;
    }
    this.isMutating = true;
    this.curriculmsService.deleteCurriculm(id1, id2).pipe(
      finalize(() => {
        this.isMutating = false;
      }),
    ).subscribe({
      next: (res) => {
        if (!res.isSuccess) {
          this.toastr.warning(res.errorMasseges[0] || 'Failed to delete curriculum');
          return;
        }

        this.toastr.success(res.result || 'Curriculum deleted successfully');
        this.loadCurriculumsPage();
      },
      error: () => this.toastr.error('Server error while deleting curriculum')
    });
  }

  first: number = 0;
  rows: number = 4;

  onPageChange(event: PaginatorState): void {
    if (this.isFetchingCurriculums || this.isMutating) {
      return;
    }
    this.first = event.first ?? 0;
    this.rows = event.rows ?? 4;
    this.loadCurriculumsPage();
  }
}
