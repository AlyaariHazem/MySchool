
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

import { Year } from 'app/core/models/year.model';
import { School } from 'app/core/models/school.modul';

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

  @Input() schools: School[] = [];
  @Input() years: Year[] = [];
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

  yearOptions: { label: string; value: number }[] = [];
  schoolOptions: { label: string; value: number }[] = [];

  form = this.fb.nonNullable.group({
    schoolID: [0, [Validators.required, Validators.min(1)]],
    currentAcademicYearID: [0, [Validators.required, Validators.min(1)]],
    employeeJobTypeID: [0, [Validators.required, Validators.min(1)]],
    employeeCode: ['', [Validators.required, Validators.maxLength(64)]],
    firstName: ['', Validators.required],
    middleName: [''],
    lastName: ['', Validators.required],
    firstNameEng: [''],
    middleNameEng: [''],
    lastNameEng: [''],
    nationalId: [''],
    dateOfBirth: [null as Date | null],
    gender: [''],
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
    this.loadJobTypes();
    this.langSub = this.translate.onLangChange.subscribe(() => this.rebuildJobTypeSelectLabels());
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
    if (changes['schools'] || changes['years']) {
      this.rebuildSchoolOptions();
      this.rebuildYearOptions();
    }
    if (changes['initial'] && this.initial) {
      this.patchFromProfile(this.initial);
    }
  }

  private rebuildSchoolOptions(): void {
    this.schoolOptions = (this.schools ?? []).map((s) => ({
      label: s.schoolName || String(s.schoolID),
      value: s.schoolID ?? 0,
    }));
  }

  private rebuildYearOptions(): void {
    const sid = this.form.get('schoolID')?.value;
    const filtered =
      sid != null && sid > 0 ? (this.years ?? []).filter((y) => y.schoolID === sid) : [...(this.years ?? [])];
    this.yearOptions = filtered.map((y) => ({
      label: `${y.yearID}`,
      value: y.yearID,
    }));
  }

  onSchoolChange(): void {
    this.rebuildYearOptions();
    const yid = this.form.get('currentAcademicYearID')?.value;
    if (yid && !this.yearOptions.some((o) => o.value === yid)) {
      this.form.patchValue({ currentAcademicYearID: 0 });
    }
  }

  private patchFromProfile(p: EmployeeProfileReadDto): void {
    this.form.patchValue({
      schoolID: p.schoolID,
      currentAcademicYearID: p.currentAcademicYearID,
      employeeJobTypeID: p.employeeJobTypeID,
      employeeCode: p.employeeCode,
      firstName: p.fullName?.firstName ?? '',
      middleName: p.fullName?.middleName ?? '',
      lastName: p.fullName?.lastName ?? '',
      firstNameEng: p.fullNameAlis?.firstNameEng ?? '',
      middleNameEng: p.fullNameAlis?.middleNameEng ?? '',
      lastNameEng: p.fullNameAlis?.lastNameEng ?? '',
      nationalId: p.nationalId ?? '',
      dateOfBirth: p.dateOfBirth ? new Date(p.dateOfBirth) : null,
      gender: p.gender ?? '',
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
    this.rebuildYearOptions();
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
      schoolID: v.schoolID,
      currentAcademicYearID: v.currentAcademicYearID,
      employeeJobTypeID: v.employeeJobTypeID,
      employeeCode: v.employeeCode.trim(),
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
      gender: v.gender?.trim() || null,
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
