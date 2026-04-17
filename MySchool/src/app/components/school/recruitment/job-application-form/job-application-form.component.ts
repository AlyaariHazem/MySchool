import { NgIf } from '@angular/common';
import {
  Component,
  EventEmitter,
  inject,
  Input,
  OnChanges,
  OnInit,
  Output,
  SimpleChanges,
} from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { TranslateModule } from '@ngx-translate/core';
import { ButtonModule } from 'primeng/button';
import { DatePicker } from 'primeng/datepicker';
import { FloatLabelModule } from 'primeng/floatlabel';
import { InputNumberModule } from 'primeng/inputnumber';
import { InputTextModule } from 'primeng/inputtext';
import { ProgressSpinnerModule } from 'primeng/progressspinner';
import { Select } from 'primeng/select';
import { TextareaModule } from 'primeng/textarea';

import { Year } from 'app/core/models/year.model';

import { JobApplicationCreateDto, JobApplicationReadDto, JobPostingListDto } from '../recruitment.models';

@Component({
  selector: 'app-job-application-form',
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
  ],
  templateUrl: './job-application-form.component.html',
  styleUrl: './job-application-form.component.scss',
})
export class JobApplicationFormComponent implements OnChanges, OnInit {
  private readonly fb = inject(FormBuilder);

  @Input() years: Year[] = [];
  /** Open postings for create mode. */
  @Input() openPostings: JobPostingListDto[] = [];
  @Input() initial: JobApplicationReadDto | null = null;
  /** When true, jobPostingId is disabled (fixed context). */
  @Input() lockPostingId = false;
  @Input() submitting = false;
  @Input() submitLabelKey = 'recruitment.applications.save';
  @Input() mode: 'create' | 'edit' = 'create';

  @Output() submitted = new EventEmitter<JobApplicationCreateDto | Record<string, unknown>>();
  @Output() cancelled = new EventEmitter<void>();

  postingOptions: { label: string; value: number }[] = [];
  yearOptions: { label: string; value: number }[] = [];

  form = this.fb.nonNullable.group({
    jobPostingID: [0, [Validators.required, Validators.min(1)]],
    academicYearID: [null as number | null],
    applicantFirstName: ['', Validators.required],
    applicantLastName: ['', Validators.required],
    applicantArabicName: [''],
    applicantEnglishName: [''],
    nationalID: [''],
    dateOfBirth: [null as Date | null],
    gender: [''],
    phone: [''],
    email: [''],
    address: [''],
    highestQualification: [''],
    specialization: [''],
    yearsOfExperience: [null as number | null],
    currentEmployer: [''],
    resumeFileUrl: [''],
    coverLetter: [''],
    source: [''],
    notes: [''],
  });

  ngOnInit(): void {
    this.rebuildPostingOptions();
    this.applyModeValidators();
  }

  ngOnChanges(changes: SimpleChanges): void {
    if (changes['openPostings'] || changes['initial']) {
      this.rebuildPostingOptions();
      if (this.initial && this.mode === 'edit') this.patchEdit(this.initial);
    }
    if (changes['years']) this.rebuildYearOptions();
    if (changes['mode']) this.applyModeValidators();
  }

  private applyModeValidators(): void {
    const jp = this.form.controls.jobPostingID;
    if (this.mode === 'edit') {
      jp.clearValidators();
    } else {
      jp.setValidators([Validators.required, Validators.min(1)]);
    }
    jp.updateValueAndValidity({ emitEvent: false });
  }

  private rebuildPostingOptions(): void {
    this.postingOptions = (this.openPostings ?? []).map((p) => ({
      value: p.jobPostingID,
      label: `${p.title}${p.department ? ' — ' + p.department : ''}`,
    }));
  }

  private rebuildYearOptions(): void {
    this.yearOptions = this.years
      .filter((y) => y.yearID != null && y.yearID > 0)
      .map((y) => ({
        label: `${y.yearDateStart ? new Date(y.yearDateStart).getFullYear() : y.yearID}`,
        value: y.yearID!,
      }));
  }

  private patchEdit(p: JobApplicationReadDto): void {
    this.form.patchValue({
      jobPostingID: p.jobPostingID,
      academicYearID: p.academicYearID,
      applicantFirstName: p.applicantFirstName,
      applicantLastName: p.applicantLastName,
      applicantArabicName: p.applicantArabicName ?? '',
      applicantEnglishName: p.applicantEnglishName ?? '',
      nationalID: p.nationalID ?? '',
      dateOfBirth: p.dateOfBirth ? new Date(p.dateOfBirth) : null,
      gender: p.gender ?? '',
      phone: p.phone ?? '',
      email: p.email ?? '',
      address: p.address ?? '',
      highestQualification: p.highestQualification ?? '',
      specialization: p.specialization ?? '',
      yearsOfExperience: p.yearsOfExperience ?? null,
      currentEmployer: p.currentEmployer ?? '',
      resumeFileUrl: p.resumeFileUrl ?? '',
      coverLetter: p.coverLetter ?? '',
      source: p.source ?? '',
      notes: p.notes ?? '',
    });
  }

  submit(): void {
    this.form.markAllAsTouched();
    if (this.form.invalid) return;
    const v = this.form.getRawValue();
    if (this.mode === 'create') {
      const dto: JobApplicationCreateDto = {
        jobPostingID: v.jobPostingID,
        academicYearID: v.academicYearID && v.academicYearID > 0 ? v.academicYearID : null,
        applicantFirstName: v.applicantFirstName.trim(),
        applicantLastName: v.applicantLastName.trim(),
        applicantArabicName: v.applicantArabicName?.trim() || null,
        applicantEnglishName: v.applicantEnglishName?.trim() || null,
        nationalID: v.nationalID?.trim() || null,
        dateOfBirth: v.dateOfBirth ? v.dateOfBirth.toISOString() : null,
        gender: v.gender?.trim() || null,
        phone: v.phone?.trim() || null,
        email: v.email?.trim() || null,
        address: v.address?.trim() || null,
        highestQualification: v.highestQualification?.trim() || null,
        specialization: v.specialization?.trim() || null,
        yearsOfExperience: v.yearsOfExperience,
        currentEmployer: v.currentEmployer?.trim() || null,
        resumeFileUrl: v.resumeFileUrl?.trim() || null,
        coverLetter: v.coverLetter?.trim() || null,
        source: v.source?.trim() || null,
        notes: v.notes?.trim() || null,
      };
      this.submitted.emit(dto);
    } else {
      const edit: Record<string, unknown> = {
        applicantFirstName: v.applicantFirstName.trim(),
        applicantLastName: v.applicantLastName.trim(),
        applicantArabicName: v.applicantArabicName?.trim() || null,
        applicantEnglishName: v.applicantEnglishName?.trim() || null,
        nationalID: v.nationalID?.trim() || null,
        dateOfBirth: v.dateOfBirth ? v.dateOfBirth.toISOString() : null,
        gender: v.gender?.trim() || null,
        phone: v.phone?.trim() || null,
        email: v.email?.trim() || null,
        address: v.address?.trim() || null,
        highestQualification: v.highestQualification?.trim() || null,
        specialization: v.specialization?.trim() || null,
        yearsOfExperience: v.yearsOfExperience,
        currentEmployer: v.currentEmployer?.trim() || null,
        resumeFileUrl: v.resumeFileUrl?.trim() || null,
        coverLetter: v.coverLetter?.trim() || null,
        source: v.source?.trim() || null,
        notes: v.notes?.trim() || null,
      };
      this.submitted.emit(edit);
    }
  }

  cancel(): void {
    this.cancelled.emit();
  }
}
