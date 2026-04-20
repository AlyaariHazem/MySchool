
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
import { NgIf } from '@angular/common';
import { Subscription } from 'rxjs';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { ButtonModule } from 'primeng/button';
import { DatePicker } from 'primeng/datepicker';
import { InputNumberModule } from 'primeng/inputnumber';
import { InputTextModule } from 'primeng/inputtext';
import { ProgressSpinnerModule } from 'primeng/progressspinner';
import { Select } from 'primeng/select';
import { TextareaModule } from 'primeng/textarea';
import { finalize } from 'rxjs/operators';

import {
  EmployeeJobTypeDto,
  EmployeeProfileCreateDto,
  EmployeeProfileReadDto,
  EmploymentStatus,
} from '../employees-hr.models';
import { EmployeesHrService } from '../employees-hr.service';

@Component({
  selector: 'app-employees-hr-profile-form',
  standalone: true,
  imports: [
    NgIf,
    ReactiveFormsModule,
    TranslateModule,
    ButtonModule,
    InputTextModule,
    TextareaModule,
    Select,
    DatePicker,
    InputNumberModule,
    ProgressSpinnerModule,
  ],
  templateUrl: './employees-hr-profile-form.component.html',
  styleUrl: './employees-hr-profile-form.component.scss',
})
export class EmployeesHrProfileFormComponent implements OnChanges, OnInit, OnDestroy {
  private readonly fb = inject(FormBuilder);
  private readonly translate = inject(TranslateService);
  private readonly employeesHr = inject(EmployeesHrService);

  /** Keeps PrimeNG select panels from stretching full viewport width (RTL / modals). */
  readonly selectPanelStyle: Record<string, string> = {
    maxWidth: 'min(22rem, calc(100vw - 2rem))',
  };

  @Input() initial: EmployeeProfileReadDto | null = null;
  @Input() submitting = false;
  @Input() submitLabelKey = 'employeesHr.form.save';

  @Output() submitted = new EventEmitter<EmployeeProfileCreateDto>();
  @Output() cancelled = new EventEmitter<void>();

  private jobTypesRows: EmployeeJobTypeDto[] = [];
  jobTypesForSelect: { label: string; employeeJobTypeID: number }[] = [];
  jobTypesLoading = false;
  private langSub?: Subscription;
  employmentStatusOptions: { label: string; value: EmploymentStatus }[] = [];
  /** Internal values for p-select; API receives Arabic strings via `selectValueToApiGender`. */
  genderOptions: { label: string; value: 'male' | 'female' }[] = [];

  form = this.fb.nonNullable.group({
    employeeJobTypeID: [0, [Validators.required, Validators.min(1)]],
    firstName: ['', Validators.required],
    middleName: [''],
    lastName: ['', Validators.required],
    firstNameEng: [''],
    middleNameEng: [''],
    lastNameEng: [''],
    nationalId: [''],
    dateOfBirth: [null as Date | null],
    gender: [null as 'male' | 'female' | null],
    phone: [''],
    email: [''],
    address: [''],
    hireDate: [null as Date | null],
    employmentStatus: [EmploymentStatus.Active, Validators.required],
    notes: [''],
    isActive: [true],
    userId: [''],
    teacherID: [null as number | null],
    managerID: [null as number | null],
    schoolStaffID: [null as number | null],
  });

  ngOnInit(): void {
    this.employmentStatusOptions = [
      { label: this.translate.instant('employeesHr.status.active'), value: EmploymentStatus.Active },
      { label: this.translate.instant('employeesHr.status.onLeave'), value: EmploymentStatus.OnLeave },
      { label: this.translate.instant('employeesHr.status.suspended'), value: EmploymentStatus.Suspended },
      { label: this.translate.instant('employeesHr.status.terminated'), value: EmploymentStatus.Terminated },
    ];
    this.rebuildGenderOptions();
    this.loadJobTypes();
    this.langSub = this.translate.onLangChange.subscribe(() => {
      this.rebuildJobTypeSelectLabels();
      this.rebuildGenderOptions();
    });
  }

  private rebuildGenderOptions(): void {
    this.genderOptions = [
      { label: this.translate.instant('employeesHr.form.genderMale'), value: 'male' },
      { label: this.translate.instant('employeesHr.form.genderFemale'), value: 'female' },
    ];
  }

  /** Map stored API / legacy strings to select value. */
  private parseGenderToSelectValue(raw: string | null | undefined): 'male' | 'female' | null {
    const s = (raw ?? '').trim();
    if (!s) return null;
    const lower = s.toLowerCase();
    if (lower === 'male' || lower === 'm' || s === 'ذكر') return 'male';
    if (lower === 'female' || lower === 'f' || s === 'أنثى' || s === 'انثى' || s === 'انثي') return 'female';
    return null;
  }

  private selectValueToApiGender(key: 'male' | 'female' | null | undefined): string | null {
    if (key === 'male') return 'ذكر';
    if (key === 'female') return 'أنثى';
    return null;
  }

  ngOnDestroy(): void {
    this.langSub?.unsubscribe();
  }

  private loadJobTypes(): void {
    this.jobTypesLoading = true;
    this.employeesHr
      .getEmployeeJobTypes()
      .pipe(finalize(() => (this.jobTypesLoading = false)))
      .subscribe({
        next: (rows) => {
          this.jobTypesRows = rows ?? [];
          this.rebuildJobTypeSelectLabels();
        },
      });
  }

  private rebuildJobTypeSelectLabels(): void {
    this.jobTypesForSelect = this.jobTypesRows.map((j) => ({
      employeeJobTypeID: j.employeeJobTypeID,
      label: this.jobTypeOptionLabel(j),
    }));
  }

  private jobTypeOptionLabel(j: EmployeeJobTypeDto): string {
    const lang = this.translate.currentLang || '';
    const primary = lang.startsWith('ar') && j.nameAr?.trim() ? j.nameAr.trim() : j.name;
    const inactive = !j.isActive
      ? ` (${this.translate.instant('employeesHr.form.jobTypeInactiveSuffix')})`
      : '';
    return `${primary} (${j.code})${inactive}`;
  }

  ngOnChanges(changes: SimpleChanges): void {
    if (changes['initial'] && this.initial) {
      this.patchFromProfile(this.initial);
    }
  }

  private patchFromProfile(p: EmployeeProfileReadDto): void {
    this.form.patchValue({
      employeeJobTypeID: p.employeeJobTypeID,
      firstName: p.fullName?.firstName ?? '',
      middleName: p.fullName?.middleName ?? '',
      lastName: p.fullName?.lastName ?? '',
      firstNameEng: p.fullNameAlis?.firstNameEng ?? '',
      middleNameEng: p.fullNameAlis?.middleNameEng ?? '',
      lastNameEng: p.fullNameAlis?.lastNameEng ?? '',
      nationalId: p.nationalId ?? '',
      dateOfBirth: p.dateOfBirth ? new Date(p.dateOfBirth) : null,
      gender: this.parseGenderToSelectValue(p.gender),
      phone: p.phone ?? '',
      email: p.email ?? '',
      address: p.address ?? '',
      hireDate: p.hireDate ? new Date(p.hireDate) : null,
      employmentStatus: p.employmentStatus,
      notes: p.notes ?? '',
      isActive: p.isActive,
      userId: p.userId ?? '',
      teacherID: p.teacherID ?? null,
      managerID: p.managerID ?? null,
      schoolStaffID: p.schoolStaffID ?? null,
    });
  }

  private toIso(d: Date | null | undefined): string | undefined {
    if (!d) return undefined;
    return d.toISOString();
  }

  emitSubmit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }
    const v = this.form.getRawValue();
    const dto: EmployeeProfileCreateDto = {
      employeeJobTypeID: v.employeeJobTypeID,
      ...(this.initial != null
        ? { employeeCode: (this.initial.employeeCode ?? '').trim() }
        : {}),
      fullName: {
        firstName: v.firstName.trim(),
        middleName: v.middleName?.trim() || null,
        lastName: v.lastName.trim(),
      },
      fullNameAlis:
        v.firstNameEng || v.middleNameEng || v.lastNameEng
          ? {
              firstNameEng: v.firstNameEng?.trim() || null,
              middleNameEng: v.middleNameEng?.trim() || null,
              lastNameEng: v.lastNameEng?.trim() || null,
            }
          : null,
      nationalId: v.nationalId?.trim() || null,
      dateOfBirth: this.toIso(v.dateOfBirth ?? undefined),
      gender: this.selectValueToApiGender(v.gender as 'male' | 'female' | null | undefined),
      phone: v.phone?.trim() || null,
      email: v.email?.trim() || null,
      address: v.address?.trim() || null,
      hireDate: this.toIso(v.hireDate ?? undefined),
      employmentStatus: v.employmentStatus,
      notes: v.notes?.trim() || null,
      isActive: v.isActive,
      userId: v.userId?.trim() || null,
      teacherID: v.teacherID && v.teacherID > 0 ? v.teacherID : null,
      managerID: v.managerID && v.managerID > 0 ? v.managerID : null,
      schoolStaffID: v.schoolStaffID && v.schoolStaffID > 0 ? v.schoolStaffID : null,
    };
    this.submitted.emit(dto);
  }
}
