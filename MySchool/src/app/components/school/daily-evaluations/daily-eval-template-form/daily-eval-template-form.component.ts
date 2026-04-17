import { NgIf } from '@angular/common';
import { Component, OnInit, inject } from '@angular/core';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import {
  AbstractControl,
  FormBuilder,
  FormGroup,
  ReactiveFormsModule,
  ValidationErrors,
  Validators,
} from '@angular/forms';
import { TranslateModule } from '@ngx-translate/core';
import { ButtonModule } from 'primeng/button';
import { DatePicker } from 'primeng/datepicker';
import { FloatLabelModule } from 'primeng/floatlabel';
import { InputSwitchModule } from 'primeng/inputswitch';
import { InputTextModule } from 'primeng/inputtext';
import { ProgressSpinnerModule } from 'primeng/progressspinner';
import { Select } from 'primeng/select';
import { TextareaModule } from 'primeng/textarea';
import { ToastrService } from 'ngx-toastr';
import { finalize } from 'rxjs/operators';

import { PagePermission, PermissionService } from 'app/core/services/permission.service';
import { SchoolService } from 'app/core/services/school.service';
import { YearService } from 'app/core/services/year.service';
import { Year } from 'app/core/models/year.model';
import { School } from 'app/core/models/school.modul';
import { ShardModule } from 'app/shared/shard.module';

import { EmployeeJobTypeDto } from '../../employees-hr/employees-hr.models';
import { EmployeesHrService } from '../../employees-hr/employees-hr.service';
import { DailyEvaluationTemplateCreateDto, DailyEvaluationTemplateUpdateDto } from '../daily-evaluations.models';
import { DailyEvaluationsService, readDailyEvalHttpError } from '../daily-evaluations.service';

@Component({
  selector: 'app-daily-eval-template-form',
  standalone: true,
  imports: [
    ShardModule,
    NgIf,
    ReactiveFormsModule,
    TranslateModule,
    RouterLink,
    ButtonModule,
    InputTextModule,
    TextareaModule,
    Select,
    FloatLabelModule,
    DatePicker,
    InputSwitchModule,
    ProgressSpinnerModule,
  ],
  templateUrl: './daily-eval-template-form.component.html',
  styleUrl: './daily-eval-template-form.component.scss',
})
export class DailyEvalTemplateFormComponent implements OnInit {
  private readonly fb = inject(FormBuilder);
  private readonly svc = inject(DailyEvaluationsService);
  private readonly schoolService = inject(SchoolService);
  private readonly yearService = inject(YearService);
  private readonly employeesHr = inject(EmployeesHrService);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly toastr = inject(ToastrService);
  private readonly perm = inject(PermissionService);

  schools: School[] = [];
  years: Year[] = [];
  jobTypeOptions: { label: string; value: number }[] = [];
  schoolOptions: { label: string; value: number }[] = [];
  yearOptions: { label: string; value: number }[] = [];
  loadingMeta = true;
  submitting = false;
  loadingEdit = false;

  editId: number | null = null;

  canManage = this.perm.hasPermission(PagePermission.Evaluations.Update);

  form = this.fb.nonNullable.group(
    {
      schoolID: [null as number | null, Validators.required],
      academicYearID: [null as number | null, Validators.required],
      employeeJobTypeID: [null as number | null],
      name: ['', Validators.required],
      description: [''],
      effectiveFrom: [null as Date | null],
      effectiveTo: [null as Date | null],
      isDefault: [false],
      isActive: [true],
    },
    { validators: [DailyEvalTemplateFormComponent.effectiveRangeValidator] },
  );

  ngOnInit(): void {
    if (!this.canManage) {
      this.loadingMeta = false;
      return;
    }
    this.editId = Number(this.route.snapshot.paramMap.get('templateId')) || null;
    this.schoolService.getAllSchools().subscribe({
      next: (s) => {
        this.schools = s ?? [];
        this.schoolOptions = this.schools
          .filter((x): x is School & { schoolID: number } => x.schoolID != null && x.schoolID > 0)
          .map((x) => ({ label: x.schoolName || String(x.schoolID), value: x.schoolID }));
      },
      error: () => this.toastr.error('employeesHr.errors.loadSchools'),
    });
    this.yearService.getAllYears().subscribe({
      next: (y) => {
        this.years = y ?? [];
        this.rebuildYearOptions();
      },
      error: () => this.toastr.error('employeesHr.errors.loadYears'),
    });
    this.employeesHr.getEmployeeJobTypes().subscribe({
      next: (rows) => {
        this.jobTypeOptions = (rows ?? []).map((j: EmployeeJobTypeDto) => ({
          label: `${j.name}${j.code ? ` (${j.code})` : ''}`,
          value: j.employeeJobTypeID,
        }));
      },
    });
    this.form.get('schoolID')?.valueChanges.subscribe(() => this.onSchoolChange());
    if (this.editId) {
      this.loadingEdit = true;
      this.svc
        .getTemplateById(this.editId)
        .pipe(finalize(() => (this.loadingEdit = false)))
        .subscribe({
          next: (t) => {
            this.form.patchValue({
              schoolID: t.schoolID,
              academicYearID: t.academicYearID,
              employeeJobTypeID: t.employeeJobTypeID ?? null,
              name: t.name,
              description: t.description ?? '',
              effectiveFrom: t.effectiveFrom ? this.parseDate(t.effectiveFrom) : null,
              effectiveTo: t.effectiveTo ? this.parseDate(t.effectiveTo) : null,
              isDefault: t.isDefault,
              isActive: t.isActive,
            });
            this.rebuildYearOptions();
          },
          error: (err) => this.toastr.error(readDailyEvalHttpError(err)),
        });
    }
    this.loadingMeta = false;
  }

  private parseDate(s: string): Date {
    const d = new Date(s + 'T12:00:00');
    return Number.isNaN(d.getTime()) ? new Date() : d;
  }

  private formatDate(d: Date | null): string | null {
    if (!d) return null;
    return d.toISOString().slice(0, 10);
  }

  private static effectiveRangeValidator(group: AbstractControl): ValidationErrors | null {
    const g = group as FormGroup;
    const from = g.get('effectiveFrom')?.value as Date | null;
    const to = g.get('effectiveTo')?.value as Date | null;
    if (from && to && to < from) return { effectiveRange: true };
    return null;
  }

  onSchoolChange(): void {
    this.rebuildYearOptions();
    const yid = this.form.get('academicYearID')?.value;
    if (yid && !this.yearOptions.some((y) => y.value === yid)) {
      this.form.patchValue({ academicYearID: null });
    }
  }

  private rebuildYearOptions(): void {
    const sid = this.form.get('schoolID')?.value;
    const filtered =
      sid != null && sid > 0 ? this.years.filter((y) => y.schoolID === sid) : [...this.years];
    this.yearOptions = filtered.map((y) => ({
      label: `${y.yearID} — ${y.yearDateStart ? new Date(y.yearDateStart).toLocaleDateString() : ''}`,
      value: y.yearID,
    }));
  }

  cancel(): void {
    this.router.navigate(['/school/daily-evaluations/templates']).catch(() => undefined);
  }

  submit(): void {
    if (this.form.invalid || !this.canManage) {
      this.form.markAllAsTouched();
      return;
    }
    const v = this.form.getRawValue();
    this.submitting = true;
    if (this.editId) {
      const upd: DailyEvaluationTemplateUpdateDto = {
        name: v.name.trim(),
        description: v.description?.trim() || null,
        effectiveFrom: this.formatDate(v.effectiveFrom),
        effectiveTo: this.formatDate(v.effectiveTo),
        isDefault: v.isDefault,
        isActive: v.isActive,
      };
      this.svc
        .updateTemplate(this.editId, upd)
        .pipe(finalize(() => (this.submitting = false)))
        .subscribe({
          next: () => {
            this.toastr.success('dailyEvaluations.toast.templateSaved');
            this.router.navigate(['/school/daily-evaluations/templates', this.editId]).catch(() => undefined);
          },
          error: (err) => this.toastr.error(readDailyEvalHttpError(err)),
        });
    } else {
      const cr: DailyEvaluationTemplateCreateDto = {
        schoolID: v.schoolID!,
        academicYearID: v.academicYearID!,
        employeeJobTypeID: v.employeeJobTypeID && v.employeeJobTypeID > 0 ? v.employeeJobTypeID : null,
        name: v.name.trim(),
        description: v.description?.trim() || null,
        effectiveFrom: this.formatDate(v.effectiveFrom),
        effectiveTo: this.formatDate(v.effectiveTo),
        isDefault: v.isDefault,
      };
      this.svc
        .createTemplate(cr)
        .pipe(finalize(() => (this.submitting = false)))
        .subscribe({
          next: (row) => {
            this.toastr.success('dailyEvaluations.toast.templateCreated');
            this.router.navigate(['/school/daily-evaluations/templates', row.dailyEvaluationTemplateID]).catch(() => undefined);
          },
          error: (err) => this.toastr.error(readDailyEvalHttpError(err)),
        });
    }
  }
}
