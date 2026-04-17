import { NgFor, NgIf } from '@angular/common';
import { Component, OnInit, inject } from '@angular/core';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { FormBuilder, FormsModule, ReactiveFormsModule, Validators } from '@angular/forms';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { ButtonModule } from 'primeng/button';
import { DatePicker } from 'primeng/datepicker';
import { FloatLabelModule } from 'primeng/floatlabel';
import { InputNumberModule } from 'primeng/inputnumber';
import { InputTextModule } from 'primeng/inputtext';
import { ProgressSpinnerModule } from 'primeng/progressspinner';
import { Select } from 'primeng/select';
import { TextareaModule } from 'primeng/textarea';
import { ToastrService } from 'ngx-toastr';
import { forkJoin, of } from 'rxjs';
import { finalize, switchMap } from 'rxjs/operators';

import { PagePermission, PermissionService } from 'app/core/services/permission.service';
import { SchoolService } from 'app/core/services/school.service';
import { YearService } from 'app/core/services/year.service';
import { School } from 'app/core/models/school.modul';
import { Year } from 'app/core/models/year.model';
import { ShardModule } from 'app/shared/shard.module';

import { EmployeeProfileReadDto } from '../../employees-hr/employees-hr.models';
import { EmployeesHrService } from '../../employees-hr/employees-hr.service';
import {
  DailyEvaluationCreateDto,
  DailyEvaluationFullDto,
  DailyEvaluationItemReadDto,
  DailyEvaluationStatus,
  DailyEvaluationTemplateListDto,
  EvaluationTemplateStatus,
} from '../daily-evaluations.models';
import { DailyEvaluationsService, readDailyEvalHttpError } from '../daily-evaluations.service';

@Component({
  selector: 'app-daily-evaluations-form',
  standalone: true,
  imports: [
    ShardModule,
    NgIf,
    NgFor,
    FormsModule,
    ReactiveFormsModule,
    TranslateModule,
    RouterLink,
    ButtonModule,
    Select,
    FloatLabelModule,
    DatePicker,
    InputTextModule,
    TextareaModule,
    InputNumberModule,
    ProgressSpinnerModule,
  ],
  templateUrl: './daily-evaluations-form.component.html',
  styleUrl: './daily-evaluations-form.component.scss',
})
export class DailyEvaluationsFormComponent implements OnInit {
  private readonly fb = inject(FormBuilder);
  private readonly svc = inject(DailyEvaluationsService);
  private readonly employeesHr = inject(EmployeesHrService);
  private readonly schoolService = inject(SchoolService);
  private readonly yearService = inject(YearService);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly toastr = inject(ToastrService);
  private readonly translate = inject(TranslateService);
  private readonly perm = inject(PermissionService);

  schools: School[] = [];
  years: Year[] = [];
  employees: EmployeeProfileReadDto[] = [];
  templates: DailyEvaluationTemplateListDto[] = [];
  schoolOptions: { label: string; value: number }[] = [];
  yearOptions: { label: string; value: number }[] = [];
  employeeOptions: { label: string; value: number }[] = [];
  templateOptions: { label: string; value: number }[] = [];

  loadingMeta = true;
  saving = false;
  evaluationId: number | null = null;
  full: DailyEvaluationFullDto | null = null;
  itemDrafts: Record<number, { score: number; comment: string }> = {};
  critById = new Map<number, { min: number; max: number; mandatory: boolean }>();

  canCreate = this.perm.hasPermission(PagePermission.Evaluations.Create);
  canUpdate = this.perm.hasPermission(PagePermission.Evaluations.Update);

  headerForm = this.fb.nonNullable.group({
    schoolID: [null as number | null, Validators.required],
    academicYearID: [null as number | null, Validators.required],
    evaluatedEmployeeProfileID: [null as number | null, Validators.required],
    dailyEvaluationTemplateID: [null as number | null, Validators.required],
    evaluationDate: [null as Date | null, Validators.required],
    notes: [''],
  });

  ngOnInit(): void {
    this.evaluationId = Number(this.route.snapshot.paramMap.get('evaluationId')) || null;

    this.schoolService.getAllSchools().subscribe({
      next: (s) => {
        this.schools = s ?? [];
        this.schoolOptions = this.schools
          .filter((x): x is School & { schoolID: number } => x.schoolID != null && x.schoolID > 0)
          .map((x) => ({ label: x.schoolName || String(x.schoolID), value: x.schoolID }));
      },
    });
    this.yearService.getAllYears().subscribe({
      next: (y) => {
        this.years = y ?? [];
        this.rebuildYearOptions();
      },
    });

    if (this.evaluationId) {
      if (!this.canUpdate) {
        this.loadingMeta = false;
        return;
      }
      this.loadEdit(this.evaluationId);
    } else {
      if (!this.canCreate) {
        this.loadingMeta = false;
        return;
      }
      this.headerForm.patchValue({ evaluationDate: new Date() });
      this.loadingMeta = false;
    }

    this.headerForm.get('schoolID')?.valueChanges.subscribe(() => this.onSchoolChange());
    this.headerForm.get('academicYearID')?.valueChanges.subscribe(() => {
      if (!this.evaluationId) this.reloadEmployeesAndTemplates();
    });
  }

  private loadEdit(id: number): void {
    this.svc
      .getEvaluationFull(id)
      .pipe(
        switchMap((f) =>
          forkJoin({
            f: of(f),
            crit: this.svc.getCriteria(f.dailyEvaluationTemplateID),
          }),
        ),
        finalize(() => (this.loadingMeta = false)),
      )
      .subscribe({
        next: ({ f, crit }) => {
          this.full = f;
          this.critById.clear();
          (crit ?? []).forEach((c) =>
            this.critById.set(c.dailyEvaluationCriteriaID, {
              min: Number(c.minScore),
              max: Number(c.maxScore),
              mandatory: c.isMandatory,
            }),
          );
          if (f.isLocked || f.status === DailyEvaluationStatus.Locked) {
            this.toastr.warning(this.translate.instant('dailyEvaluations.evaluations.readOnlyLocked'));
            this.router.navigate(['/school/daily-evaluations', id]).catch(() => undefined);
            return;
          }
          this.headerForm.patchValue({
            schoolID: f.schoolID,
            academicYearID: f.academicYearID,
            evaluatedEmployeeProfileID: f.evaluatedEmployeeProfileID,
            dailyEvaluationTemplateID: f.dailyEvaluationTemplateID,
            evaluationDate: f.evaluationDate ? this.parseD(f.evaluationDate) : new Date(),
            notes: f.notes ?? '',
          });
          this.headerForm.disable();
          this.rebuildYearOptions();
          this.reloadEmployeesAndTemplates(() => {
            f.items?.forEach((it) => {
              this.itemDrafts[it.dailyEvaluationItemID] = {
                score: it.score,
                comment: it.comment ?? '',
              };
            });
          });
        },
        error: (err) => {
          this.toastr.error(readDailyEvalHttpError(err));
          this.router.navigate(['/school/daily-evaluations']).catch(() => undefined);
        },
      });
  }

  private parseD(s: string): Date {
    const d = new Date(s + 'T12:00:00');
    return Number.isNaN(d.getTime()) ? new Date() : d;
  }

  private onSchoolChange(): void {
    this.rebuildYearOptions();
    const yid = this.headerForm.get('academicYearID')?.value;
    if (yid && !this.yearOptions.some((y) => y.value === yid)) {
      this.headerForm.patchValue({ academicYearID: null });
    }
    if (!this.evaluationId) {
      this.reloadEmployeesAndTemplates();
    }
  }

  private rebuildYearOptions(): void {
    const sid = this.headerForm.get('schoolID')?.value ?? this.full?.schoolID;
    const filtered =
      sid != null && sid > 0 ? this.years.filter((y) => y.schoolID === sid) : [...this.years];
    this.yearOptions = filtered.map((y) => ({
      label: `${y.yearID} — ${y.yearDateStart ? new Date(y.yearDateStart).toLocaleDateString() : ''}`,
      value: y.yearID,
    }));
  }

  reloadEmployeesAndTemplates(done?: () => void): void {
    const sid = this.headerForm.get('schoolID')?.value;
    const yid = this.headerForm.get('academicYearID')?.value;
    if (!sid || !yid) {
      this.employees = [];
      this.templates = [];
      this.employeeOptions = [];
      this.templateOptions = [];
      done?.();
      return;
    }
    forkJoin({
      em: this.employeesHr.getEmployees({ schoolID: sid, academicYearID: yid }),
      tpl: this.svc.getTemplates({ schoolID: sid, academicYearID: yid, isActive: true }),
    }).subscribe({
      next: ({ em, tpl }) => {
        this.employees = em ?? [];
        this.templates = (tpl ?? []).filter((t) => t.status === EvaluationTemplateStatus.Active);
        this.employeeOptions = this.employees.map((e) => ({
          label: this.displayName(e),
          value: e.employeeProfileID,
        }));
        this.templateOptions = this.templates.map((t) => ({
          label: t.name,
          value: t.dailyEvaluationTemplateID,
        }));
        done?.();
      },
      error: () => done?.(),
    });
  }

  displayName(e: EmployeeProfileReadDto): string {
    const n = e.fullName;
    if (!n) return e.employeeCode || String(e.employeeProfileID);
    return [n.firstName, n.middleName, n.lastName].filter(Boolean).join(' ');
  }

  cancel(): void {
    this.router.navigate(['/school/daily-evaluations']).catch(() => undefined);
  }

  createEvaluation(): void {
    if (this.headerForm.invalid || !this.canCreate) {
      this.headerForm.markAllAsTouched();
      return;
    }
    const v = this.headerForm.getRawValue();
    const dto: DailyEvaluationCreateDto = {
      schoolID: v.schoolID!,
      academicYearID: v.academicYearID!,
      evaluatedEmployeeProfileID: v.evaluatedEmployeeProfileID!,
      dailyEvaluationTemplateID: v.dailyEvaluationTemplateID!,
      evaluationDate: (v.evaluationDate as Date).toISOString().slice(0, 10),
      notes: v.notes?.trim() || null,
    };
    this.saving = true;
    this.svc
      .createEvaluation(dto)
      .pipe(finalize(() => (this.saving = false)))
      .subscribe({
        next: (row) => {
          this.toastr.success('dailyEvaluations.toast.evaluationCreated');
          this.router.navigate(['/school/daily-evaluations', row.dailyEvaluationID, 'edit']).catch(() => undefined);
        },
        error: (err) => this.toastr.error(readDailyEvalHttpError(err)),
      });
  }

  saveItemsAndNotes(): void {
    if (!this.full || !this.canUpdate) return;
    if (this.full.isLocked) {
      this.toastr.error(this.translate.instant('dailyEvaluations.evaluations.readOnlyLocked'));
      return;
    }
    this.saving = true;
    this.svc
      .updateEvaluation(this.full.dailyEvaluationID, { notes: this.full.notes?.trim() || null })
      .pipe(
        switchMap(() => {
          const items = this.full?.items ?? [];
          if (items.length === 0) return of(null);
          return forkJoin(
            items.map((it) => {
              const d = this.itemRow(it);
              return this.svc.updateEvaluationItem(it.dailyEvaluationItemID, {
                score: d.score,
                comment: d.comment?.trim() || null,
              });
            }),
          );
        }),
        finalize(() => (this.saving = false)),
      )
      .subscribe({
        next: () => {
          this.toastr.success('dailyEvaluations.toast.evaluationSaved');
          this.loadEdit(this.full!.dailyEvaluationID);
        },
        error: (err) => this.toastr.error(readDailyEvalHttpError(err)),
      });
  }

  submitEvaluation(): void {
    if (!this.full) return;
    this.saving = true;
    this.svc
      .submitEvaluation(this.full.dailyEvaluationID)
      .pipe(finalize(() => (this.saving = false)))
      .subscribe({
        next: () => {
          this.toastr.success('dailyEvaluations.toast.evaluationSubmitted');
          this.router.navigate(['/school/daily-evaluations', this.full!.dailyEvaluationID]).catch(() => undefined);
        },
        error: (err) => this.toastr.error(readDailyEvalHttpError(err)),
      });
  }

  itemRow(it: DailyEvaluationItemReadDto): { score: number; comment: string } {
    if (!this.itemDrafts[it.dailyEvaluationItemID]) {
      this.itemDrafts[it.dailyEvaluationItemID] = {
        score: it.score,
        comment: it.comment ?? '',
      };
    }
    return this.itemDrafts[it.dailyEvaluationItemID];
  }

  criteriaRange(it: DailyEvaluationItemReadDto): { min: number; max: number } {
    const c = this.critById.get(it.dailyEvaluationCriteriaID);
    return c ? { min: c.min, max: c.max } : { min: 0, max: 100 };
  }
}
