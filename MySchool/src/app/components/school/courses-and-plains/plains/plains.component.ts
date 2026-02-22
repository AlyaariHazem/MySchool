import { Component } from '@angular/core';
import { FormBuilder, FormControl, FormGroup, Validators } from '@angular/forms';
import { MatDialog } from '@angular/material/dialog';
import { ToastrService } from 'ngx-toastr';
import { PaginatorState } from 'primeng/paginator';
import { Store } from '@ngrx/store';
import { map } from 'rxjs';

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
import { YearService } from '../../../../core/services/year.service';
import { Year } from '../../../../core/models/year.model';
import { selectLanguage } from '../../../../core/store/language/language.selectors';

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
  classes: ClassNames[] | undefined;
  divisions: divisions[] = [];
  fiteredDivisions: divisions[] | undefined;
  teachers: Teachers[] | undefined;
  terms: Terms[] = [];
  years: (Year & { displayLabel?: string })[] = [];
  ClassSubjects: Curriculms[] = [];
  filteredSubjects: Curriculms[] | undefined;
  curriculmsPlan: CurriculmsPlans[] = [];
  first: number = 0;
  rows: number = 4;
  curriculmsPlans: CurriculmsPlans[] = [];
  editMode: boolean = false;
  editingPlan: CurriculmsPlans | null = null;
  values = new FormControl<string[] | null>(null);
  max = 2;

  readonly dir$ = this.store.select(selectLanguage).pipe(
    map(l => (l === 'ar' ? 'rtl' : 'ltr')),
  );

  constructor(
    private formBuilder: FormBuilder,
    public dialog: MatDialog,
    private subjectService: SubjectService,
    private curriculmsService: CurriculmService,
    private toastr: ToastrService,
    private divisionSerivce: DivisionService,
    private curriculmsPlanService: CurriculmsPlanService,
    private classService: ClassService,
    private termService: TermService,
    private store:Store,
    private teacherService: TeacherService,
    private yearService: YearService
  ) {
    this.form = this.formBuilder.group({
      classID: [null, Validators.required],
      subjectID: [null, Validators.required],
      divisionID: [null, Validators.required],
      teacherID: [null, Validators.required],
      termID: [null, Validators.required],
      yearID: [null, Validators.required]
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
    this.getAllYears();

    this.form.get('classID')?.valueChanges.subscribe((selectedClassID: number) => {
      this.filteredSubjects = this.ClassSubjects.filter(c => c.classID === selectedClassID);
      this.fiteredDivisions = this.divisions.filter(d => d.classID === selectedClassID);
      this.form.patchValue({ subjectID: null });
    });

    // Set default year from localStorage if available
    const yearFromLocal = localStorage.getItem('yearID');
    if (yearFromLocal) {
      this.form.patchValue({ yearID: Number(yearFromLocal) });
    }

    this.form.reset();
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

  getAllYears(): void {
    this.yearService.getAllYears().subscribe({
      next: (years: Year[]) => {
        // Add display property to each year for better dropdown display
        this.years = years.map((year: Year) => ({
          ...year,
          displayLabel: this.getYearDisplay(year)
        }));
        // Set default to active year if no year is selected
        if (!this.form.get('yearID')?.value && years.length > 0) {
          const activeYear = years.find((y: Year) => y.active);
          if (activeYear) {
            this.form.patchValue({ yearID: activeYear.yearID });
          } else {
            this.form.patchValue({ yearID: years[0].yearID });
          }
        }
      },
      error: () => this.toastr.error('Error fetching years')
    });
  }

  Add(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    const { classID, subjectID, divisionID, teacherID, termID, yearID } = this.form.value;

    if (!yearID) {
      this.toastr.warning('يرجى اختيار سنة دراسية');
      return;
    }

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
        this.editMode = false;
        this.editingPlan = null;
        this.form.reset();
      },
      error: (err) => {
        const errorMessage = err?.error?.errorMasseges?.[0] || err?.error?.message || 'Server error occurred';
        this.toastr.error(errorMessage);
      }
    });
  }

  editCurriculumPlan(plan: CurriculmsPlans): void {
    if (!plan.subjectID || !plan.classID || !plan.divisionID || !plan.teacherID || !plan.termID || !plan.yearID) {
      this.toastr.error('Cannot edit: Course plan data is incomplete');
      return;
    }

    // Fetch the full plan details to ensure we have the correct composite key values
    this.curriculmsPlanService.getCurriculmPlanById(
      plan.yearID,
      plan.teacherID,
      plan.classID,
      plan.divisionID,
      plan.subjectID
    ).subscribe({
      next: (res) => {
        if (!res.isSuccess || !res.result) {
          this.toastr.warning(res.errorMasseges?.[0] || 'Failed to load course plan details');
          return;
        }

        const planData = res.result;
        this.form.patchValue({
          subjectID: planData.subjectID,
          classID: planData.classID,
          divisionID: planData.divisionID,
          teacherID: planData.teacherID,
          termID: planData.termID,
          yearID: planData.yearID
        });

        // Update filtered lists based on selected class
        if (planData.classID) {
          this.filteredSubjects = this.ClassSubjects.filter(c => c.classID === planData.classID);
          this.fiteredDivisions = this.divisions.filter(d => d.classID === planData.classID);
        }

        // Store the full plan data with all IDs
        this.editingPlan = planData;
        this.editMode = true;
      },
      error: (err) => {
        const errorMessage = err?.error?.errorMasseges?.[0] || err?.error?.message || 'Failed to load course plan details';
        this.toastr.error(errorMessage);
      }
    });
  }

  updateCurriculum(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    if (!this.editingPlan) {
      this.toastr.error('No course plan selected for editing');
      return;
    }

    // Validate that editingPlan has all required values
    if (!this.editingPlan.yearID || !this.editingPlan.teacherID || !this.editingPlan.classID || 
        !this.editingPlan.divisionID || !this.editingPlan.subjectID) {
      this.toastr.error('Course plan data is incomplete. Please refresh and try again.');
      return;
    }

    const { classID, subjectID, divisionID, teacherID, termID, yearID } = this.form.value;

    if (!yearID) {
      this.toastr.warning('يرجى اختيار سنة دراسية');
      return;
    }

    const updatedPlan: CurriculmsPlan = {
      subjectID,
      classID,
      divisionID,
      teacherID,
      termID,
      yearID: Number(yearID),
    };

    // Use the old composite key values from editingPlan (these are the actual values stored in DB)
    // The backend will find the record using these exact values, then update it
    this.curriculmsPlanService.updateCurriculmPlan(
      this.editingPlan.yearID,      // Use the actual yearID from the database record
      this.editingPlan.teacherID,
      this.editingPlan.classID,
      this.editingPlan.divisionID,
      this.editingPlan.subjectID,
      updatedPlan
    ).subscribe({
      next: (res) => {
        if (!res.isSuccess) {
          this.toastr.warning(res.errorMasseges[0] || 'Failed to update course plan');
          return;
        }
        this.toastr.success(res.result || 'Course plan updated successfully');
        this.getAllCurriculmPlan();
        this.editMode = false;
        this.editingPlan = null;
        this.form.reset();
      },
      error: (err) => {
        const errorMessage = err?.error?.errorMasseges?.[0] || err?.error?.message || 'Server error occurred';
        this.toastr.error(errorMessage);
      }
    });
  }

  deleteCurriculmPlan(plan: CurriculmsPlans): void {
    if (!plan.subjectID || !plan.classID || !plan.divisionID || !plan.teacherID || !plan.yearID) {
      this.toastr.error('Cannot delete: Course plan data is incomplete');
      return;
    }

    if (!confirm('Are you sure you want to delete this course plan?')) {
      return;
    }

    this.curriculmsPlanService.deleteCurriculmPlan(
      plan.yearID,
      plan.teacherID,
      plan.classID,
      plan.divisionID,
      plan.subjectID
    ).subscribe({
      next: (res) => {
        if (!res.isSuccess) {
          this.toastr.warning(res.errorMasseges[0] || 'Failed to delete course plan');
          return;
        }
        this.toastr.success(res.result || 'Course plan deleted successfully');
        this.getAllCurriculmPlan();
      },
      error: (err) => {
        const errorMessage = err?.error?.errorMasseges?.[0] || err?.error?.message || 'Error while deleting course plan';
        this.toastr.error(errorMessage);
      }
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

  getYearDisplay(year: Year): string {
    if (!year || !year.yearDateStart || !year.yearDateEnd) {
      return '';
    }
    const startYear = new Date(year.yearDateStart).getFullYear();
    const endYear = new Date(year.yearDateEnd).getFullYear();
    return `${startYear} - ${endYear}`;
  }
}
