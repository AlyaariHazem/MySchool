import { CommonModule } from '@angular/common';
import {
  Component,
  DestroyRef,
  EventEmitter,
  inject,
  Input,
  OnChanges,
  OnInit,
  Output,
  SimpleChanges,
} from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { AbstractControl, FormBuilder, ReactiveFormsModule, ValidationErrors, Validators } from '@angular/forms';
import { TranslateModule } from '@ngx-translate/core';
import { ButtonModule } from 'primeng/button';
import { DatePicker } from 'primeng/datepicker';
import { FloatLabelModule } from 'primeng/floatlabel';
import { InputNumberModule } from 'primeng/inputnumber';
import { InputTextModule } from 'primeng/inputtext';
import { Select } from 'primeng/select';
import { TextareaModule } from 'primeng/textarea';

import { Year } from 'app/core/models/year.model';
import { readSchoolApplicantPrefill } from 'app/core/utils/applicant-prefill.util';

import {
  JobApplicationCreateDto,
  JobApplicationReadDto,
  JobPostingListDto,
  JobPostingReadDto,
} from '../recruitment.models';

function optionalEmail(control: AbstractControl): ValidationErrors | null {
  const v = (control.value as string | null | undefined)?.trim();
  if (!v) return null;
  return Validators.email(control);
}

@Component({
  selector: 'app-job-application-form',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    TranslateModule,
    ButtonModule,
    InputTextModule,
    TextareaModule,
    Select,
    FloatLabelModule,
    DatePicker,
    InputNumberModule,
  ],
  templateUrl: './job-application-form.component.html',
  styleUrl: './job-application-form.component.scss',
})
export class JobApplicationFormComponent implements OnChanges, OnInit {
  private readonly fb = inject(FormBuilder);
  private readonly destroyRef = inject(DestroyRef);

  @Input() years: Year[] = [];
  /** Open postings for create mode. */
  @Input() openPostings: JobPostingListDto[] = [];
  @Input() initial: JobApplicationReadDto | null = null;
  /** When set in create mode, pre-selects the job posting (e.g. from query <c>jobPostingID</c>). */
  @Input() presetJobPostingId: number | null = null;
  /** When true, jobPostingId is disabled (fixed context). */
  @Input() lockPostingId = false;
  /** Rich posting context when applying from a specific vacancy. */
  @Input() postingSummary: JobPostingReadDto | null = null;
  /** Resolved school name for <c>postingSummary</c>. */
  @Input() postingSchoolName: string | null = null;
  @Input() submitting = false;
  @Input() submitLabelKey = 'recruitment.applications.save';
  @Input() mode: 'create' | 'edit' = 'create';

  @Output() submitted = new EventEmitter<JobApplicationCreateDto | Record<string, unknown>>();
  @Output() cancelled = new EventEmitter<void>();

  postingOptions: { label: string; value: number }[] = [];

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
    email: ['', [optionalEmail]],
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
    this.applySessionPrefill();
    this.form.controls.jobPostingID.valueChanges
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe(() => {
        if (this.mode === 'create') this.applyAutoAcademicYear();
      });
  }

  ngOnChanges(changes: SimpleChanges): void {
    if (changes['openPostings'] || changes['initial'] || changes['presetJobPostingId']) {
      this.rebuildPostingOptions();
      if (this.initial && this.mode === 'edit') this.patchEdit(this.initial);
      else if (this.mode === 'create' && this.presetJobPostingId && this.presetJobPostingId > 0) {
        this.form.patchValue({ jobPostingID: this.presetJobPostingId }, { emitEvent: false });
      }
    }
    if (changes['postingSummary']?.currentValue && this.mode === 'create') {
      const p = changes['postingSummary'].currentValue as JobPostingReadDto;
      this.form.patchValue({ jobPostingID: p.jobPostingID }, { emitEvent: false });
    }
    if (changes['mode']) this.applyModeValidators();

    const shouldRecomputeYear =
      this.mode === 'create' &&
      (changes['years'] ||
        changes['openPostings'] ||
        changes['postingSummary'] ||
        changes['presetJobPostingId']);
    if (shouldRecomputeYear) this.applyAutoAcademicYear();
  }

  private applySessionPrefill(): void {
    if (this.mode !== 'create' || this.initial) return;
    const pre = readSchoolApplicantPrefill();
    const patch: Record<string, string | null> = {};
    const fn = this.form.get('applicantFirstName')?.value?.trim();
    const ln = this.form.get('applicantLastName')?.value?.trim();
    if (!fn && pre.applicantFirstName) patch['applicantFirstName'] = pre.applicantFirstName;
    if (!ln && pre.applicantLastName) patch['applicantLastName'] = pre.applicantLastName;
    if (!this.form.get('email')?.value?.trim() && pre.email) patch['email'] = pre.email;
    if (!this.form.get('phone')?.value?.trim() && pre.phone) patch['phone'] = pre.phone;
    if (Object.keys(patch).length) this.form.patchValue(patch, { emitEvent: false });
  }

  showFieldError(controlName: string): boolean {
    const c = this.form.get(controlName);
    return !!c && c.invalid && (c.touched || c.dirty);
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

  /** Create mode: posting year wins, else active tenant year. */
  private applyAutoAcademicYear(): void {
    if (this.mode !== 'create') return;
    const id = this.resolveAcademicYearId();
    this.form.patchValue({ academicYearID: id }, { emitEvent: false });
  }

  private resolveAcademicYearId(): number | null {
    const fromPosting = this.postingSummary?.academicYearID;
    if (fromPosting != null && fromPosting > 0) return fromPosting;

    const jid = this.form.get('jobPostingID')?.value;
    if (jid && jid > 0) {
      const row = this.openPostings?.find((p) => p.jobPostingID === jid);
      const y = row?.academicYearID;
      if (y != null && y > 0) return y;
    }

    const active = this.years?.find((x) => x.active && x.yearID > 0);
    return active?.yearID ?? null;
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
