import { NgIf } from '@angular/common';
import {
  Component,
  EventEmitter,
  inject,
  Input,
  OnChanges,
  OnDestroy,
  OnInit,
  Output,
  SimpleChanges,
} from '@angular/core';
import { Subscription } from 'rxjs';
import { AbstractControl, FormBuilder, ReactiveFormsModule, ValidationErrors, Validators } from '@angular/forms';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { ButtonModule } from 'primeng/button';
import { DatePicker } from 'primeng/datepicker';
import { FloatLabelModule } from 'primeng/floatlabel';
import { InputNumberModule } from 'primeng/inputnumber';
import { InputTextModule } from 'primeng/inputtext';
import { ProgressSpinnerModule } from 'primeng/progressspinner';
import { Select } from 'primeng/select';
import { TextareaModule } from 'primeng/textarea';
import { InputSwitchModule } from 'primeng/inputswitch';

import { Year } from 'app/core/models/year.model';
import { School } from 'app/core/models/school.modul';

import { EmployeeJobTypeDto } from '../../employees-hr/employees-hr.models';
import { EmployeesHrService } from '../../employees-hr/employees-hr.service';
import { JobPostingCreateDto, JobPostingReadDto, JobPostingStatus } from '../recruitment.models';

@Component({
  selector: 'app-job-posting-form',
  standalone: true,
  imports: [
    NgIf,
    ReactiveFormsModule,
    TranslateModule,
    ButtonModule,
    InputTextModule,
    TextareaModule,
    Select,
    FloatLabelModule,
    DatePicker,
    InputNumberModule,
    ProgressSpinnerModule,
    InputSwitchModule,
  ],
  templateUrl: './job-posting-form.component.html',
  styleUrl: './job-posting-form.component.scss',
})
export class JobPostingFormComponent implements OnChanges, OnInit, OnDestroy {
  private readonly fb = inject(FormBuilder);
  private readonly translate = inject(TranslateService);
  private readonly employeesHr = inject(EmployeesHrService);

  @Input() schools: School[] = [];
  @Input() years: Year[] = [];
  @Input() initial: JobPostingReadDto | null = null;
  @Input() submitting = false;
  @Input() submitLabelKey = 'recruitment.postings.save';

  @Output() submitted = new EventEmitter<JobPostingCreateDto>();
  @Output() cancelled = new EventEmitter<void>();

  private jobTypesRows: EmployeeJobTypeDto[] = [];
  jobTypeOptions: { label: string; value: number }[] = [];
  jobTypesLoading = false;
  private langSub?: Subscription;

  schoolOptions: { label: string; value: number }[] = [];
  yearOptions: { label: string; value: number }[] = [];

  statusOptions: { label: string; value: JobPostingStatus }[] = [];

  JobPostingStatus = JobPostingStatus;

  form = this.fb.nonNullable.group(
    {
      schoolID: [0, [Validators.required, Validators.min(1)]],
      academicYearID: [null as number | null],
      employeeJobTypeID: [0, [Validators.required, Validators.min(1)]],
      title: ['', [Validators.required, Validators.maxLength(256)]],
      department: [''],
      description: [''],
      requirements: [''],
      responsibilities: [''],
      employmentType: [''],
      numberOfOpenings: [1, [Validators.required, Validators.min(1), Validators.max(1000)]],
      postingDate: [null as Date | null, Validators.required],
      closingDate: [null as Date | null],
      status: [JobPostingStatus.Draft, Validators.required],
      notes: [''],
      isActive: [true],
    },
    { validators: [JobPostingFormComponent.dateOrderValidator] },
  );

  private static dateOrderValidator(control: AbstractControl): ValidationErrors | null {
    const g = control;
    const p = g.get('postingDate')?.value as Date | null;
    const c = g.get('closingDate')?.value as Date | null;
    if (p && c && c < p) return { closingBeforePosting: true };
    return null;
  }

  ngOnInit(): void {
    this.statusOptions = [
      { label: this.translate.instant('recruitment.postingStatus.draft'), value: JobPostingStatus.Draft },
      { label: this.translate.instant('recruitment.postingStatus.open'), value: JobPostingStatus.Open },
      { label: this.translate.instant('recruitment.postingStatus.closed'), value: JobPostingStatus.Closed },
      { label: this.translate.instant('recruitment.postingStatus.archived'), value: JobPostingStatus.Archived },
    ];
    this.loadJobTypes();
    this.langSub = this.translate.onLangChange.subscribe(() => this.rebuildJobTypeLabels());
  }

  ngOnDestroy(): void {
    this.langSub?.unsubscribe();
  }

  ngOnChanges(changes: SimpleChanges): void {
    if (changes['schools'] || changes['years']) this.rebuildSchoolYearOptions();
    if (changes['initial'] && this.initial) this.patchFromInitial(this.initial);
  }

  private rebuildJobTypeLabels(): void {
    const lang = this.translate.currentLang;
    this.jobTypeOptions = this.jobTypesRows.map((j) => ({
      value: j.employeeJobTypeID,
      label: lang === 'ar' && j.nameAr ? j.nameAr : j.name,
    }));
  }

  private loadJobTypes(): void {
    this.jobTypesLoading = true;
    this.employeesHr.getEmployeeJobTypes().subscribe({
      next: (rows) => {
        this.jobTypesRows = rows;
        this.rebuildJobTypeLabels();
        this.jobTypesLoading = false;
      },
      error: () => (this.jobTypesLoading = false),
    });
  }

  private rebuildSchoolYearOptions(): void {
    this.schoolOptions = this.schools
      .filter((s): s is School & { schoolID: number } => s.schoolID != null && s.schoolID > 0)
      .map((s) => ({ label: s.schoolName || String(s.schoolID), value: s.schoolID }));
    this.onSchoolChange();
  }

  onSchoolChange(): void {
    const sid = this.form.controls.schoolID.value;
    const list = sid ? this.years.filter((y) => y.schoolID === sid) : this.years;
    this.yearOptions = list
      .filter((y) => y.yearID != null && y.yearID > 0)
      .map((y) => ({
        label: `${y.yearDateStart ? new Date(y.yearDateStart).getFullYear() : y.yearID}`,
        value: y.yearID!,
      }));
  }

  private patchFromInitial(p: JobPostingReadDto): void {
    this.form.patchValue({
      schoolID: p.schoolID,
      academicYearID: p.academicYearID ?? null,
      employeeJobTypeID: p.employeeJobTypeID,
      title: p.title,
      department: p.department ?? '',
      description: p.description ?? '',
      requirements: p.requirements ?? '',
      responsibilities: p.responsibilities ?? '',
      employmentType: p.employmentType ?? '',
      numberOfOpenings: p.numberOfOpenings,
      postingDate: p.postingDate ? new Date(p.postingDate) : null,
      closingDate: p.closingDate ? new Date(p.closingDate) : null,
      status: p.status,
      notes: p.notes ?? '',
      isActive: p.isActive,
    });
    this.onSchoolChange();
  }

  submit(): void {
    this.form.markAllAsTouched();
    if (this.form.invalid) return;
    const v = this.form.getRawValue();
    const dto: JobPostingCreateDto = {
      schoolID: v.schoolID,
      academicYearID: v.academicYearID && v.academicYearID > 0 ? v.academicYearID : null,
      employeeJobTypeID: v.employeeJobTypeID,
      title: v.title.trim(),
      department: v.department?.trim() || null,
      description: v.description || null,
      requirements: v.requirements || null,
      responsibilities: v.responsibilities || null,
      employmentType: v.employmentType?.trim() || null,
      numberOfOpenings: v.numberOfOpenings,
      postingDate: v.postingDate!.toISOString(),
      closingDate: v.closingDate ? v.closingDate.toISOString() : null,
      status: v.status,
      notes: v.notes?.trim() || null,
      isActive: v.isActive,
    };
    this.submitted.emit(dto);
  }

  cancel(): void {
    this.cancelled.emit();
  }
}
