import { NgIf, NgTemplateOutlet } from '@angular/common';
import { Component, EventEmitter, Input, OnInit, Output, inject } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { ButtonModule } from 'primeng/button';
import { DialogModule } from 'primeng/dialog';
import { FloatLabelModule } from 'primeng/floatlabel';
import { InputNumberModule } from 'primeng/inputnumber';
import { InputTextModule } from 'primeng/inputtext';
import { InputSwitchModule } from 'primeng/inputswitch';
import { TableModule } from 'primeng/table';
import { TextareaModule } from 'primeng/textarea';
import { TooltipModule } from 'primeng/tooltip';
import { ToastrService } from 'ngx-toastr';
import { finalize } from 'rxjs/operators';

import { PagePermission, PermissionService } from 'app/core/services/permission.service';
import { SchoolService } from 'app/core/services/school.service';
import { YearService } from 'app/core/services/year.service';
import { School } from 'app/core/models/school.modul';
import { Year } from 'app/core/models/year.model';
import { ShardModule } from 'app/shared/shard.module';

import {
  DailyEvaluationCriteriaReadDto,
  DailyEvaluationTemplateReadDto,
  EvaluationTemplateStatus,
} from '../daily-evaluations.models';
import { DailyEvaluationsNavService } from '../daily-evaluations-nav.service';
import { DailyEvaluationsService, readDailyEvalHttpError } from '../daily-evaluations.service';

@Component({
  selector: 'app-daily-eval-template-detail',
  standalone: true,
  imports: [
    ShardModule,
    NgIf,
    NgTemplateOutlet,
    RouterLink,
    TranslateModule,
    ReactiveFormsModule,
    ButtonModule,
    TableModule,
    DialogModule,
    FloatLabelModule,
    InputTextModule,
    TextareaModule,
    InputNumberModule,
    InputSwitchModule,
    TooltipModule,
  ],
  templateUrl: './daily-eval-template-detail.component.html',
  styleUrl: './daily-eval-template-detail.component.scss',
})
export class DailyEvalTemplateDetailComponent implements OnInit {
  private readonly svc = inject(DailyEvaluationsService);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly toastr = inject(ToastrService);
  private readonly perm = inject(PermissionService);
  private readonly schoolService = inject(SchoolService);
  private readonly yearService = inject(YearService);
  private readonly fb = inject(FormBuilder);
  private readonly translate = inject(TranslateService);
  readonly dailyEvalNav = inject(DailyEvaluationsNavService);

  template: DailyEvaluationTemplateReadDto | null = null;
  criteria: DailyEvaluationCriteriaReadDto[] = [];
  schools: School[] = [];
  years: Year[] = [];
  loading = true;
  criteriaLoading = false;

  canView = this.perm.hasPermission(PagePermission.Evaluations.View);
  canManage = this.perm.hasPermission(PagePermission.Evaluations.Update);

  EvaluationTemplateStatus = EvaluationTemplateStatus;

  criteriaDialog = false;
  editingCriteriaId: number | null = null;

  critForm = this.fb.nonNullable.group({
    name: ['', Validators.required],
    description: [''],
    weight: [1, [Validators.required]],
    minScore: [0, [Validators.required]],
    maxScore: [10, [Validators.required]],
    isMandatory: [false],
    sortOrder: [0, [Validators.required]],
    isActive: [true],
    notes: [''],
  });

  templateId = 0;

  /** When true, shown inside a dialog on the list page (no route). */
  @Input() embedded = false;
  /** Template id when embedded (from list). */
  @Input() templateIdInput: number | null = null;

  @Output() closed = new EventEmitter<void>();
  @Output() requestEdit = new EventEmitter<number>();

  ngOnInit(): void {
    this.templateId = this.embedded
      ? this.templateIdInput != null && this.templateIdInput > 0
        ? this.templateIdInput
        : 0
      : Number(this.route.snapshot.paramMap.get('templateId')) || 0;
    if (!this.templateId) {
      if (!this.embedded) {
        this.router.navigate([this.dailyEvalNav.basePath(), 'templates']).catch(() => undefined);
      }
      return;
    }
    if (this.dailyEvalNav.isTeacherDailyEvaluationsRoute()) {
      const opt = this.dailyEvalNav.teacherSessionSchoolOption();
      this.schools = opt ? ([{ schoolID: opt.value, schoolName: opt.label }] as School[]) : [];
    } else {
      this.schoolService.getAllSchools().subscribe({ next: (s) => (this.schools = s ?? []) });
    }
    this.yearService.getAllYears().subscribe({ next: (y) => (this.years = y ?? []) });
    this.reload();
  }

  reload(): void {
    if (!this.canView) {
      this.loading = false;
      return;
    }
    this.loading = true;
    this.svc
      .getTemplateById(this.templateId)
      .pipe(finalize(() => (this.loading = false)))
      .subscribe({
        next: (t) => {
          this.template = t;
          this.loadCriteria();
        },
        error: (err) => {
          this.toastr.error(readDailyEvalHttpError(err));
          if (this.embedded) {
            this.closed.emit();
          } else {
            this.router.navigate([this.dailyEvalNav.basePath(), 'templates']).catch(() => undefined);
          }
        },
      });
  }

  loadCriteria(): void {
    this.criteriaLoading = true;
    this.svc
      .getCriteria(this.templateId)
      .pipe(finalize(() => (this.criteriaLoading = false)))
      .subscribe({
        next: (rows) => (this.criteria = rows ?? []),
        error: (err) => this.toastr.error(readDailyEvalHttpError(err)),
      });
  }

  schoolName(id: number): string {
    return this.schools.find((s) => s.schoolID === id)?.schoolName ?? String(id);
  }

  yearLabel(id: number): string {
    const y = this.years.find((x) => x.yearID === id);
    if (y) {
      return `${y.yearID} — ${y.yearDateStart ? new Date(y.yearDateStart).toLocaleDateString() : ''}`.trim();
    }
    return String(id);
  }

  closeEmbedded(): void {
    this.closed.emit();
  }

  emitEditRequest(): void {
    this.requestEdit.emit(this.templateId);
  }

  statusLabelKey(s: EvaluationTemplateStatus): string {
    const m: Record<EvaluationTemplateStatus, string> = {
      [EvaluationTemplateStatus.Draft]: 'draft',
      [EvaluationTemplateStatus.Active]: 'active',
      [EvaluationTemplateStatus.Inactive]: 'inactive',
      [EvaluationTemplateStatus.Archived]: 'archived',
    };
    return m[s] ?? String(s);
  }

  openNewCriteria(): void {
    this.editingCriteriaId = null;
    this.critForm.reset({
      name: '',
      description: '',
      weight: 1,
      minScore: 0,
      maxScore: 10,
      isMandatory: false,
      sortOrder: (this.criteria?.length ?? 0) + 1,
      isActive: true,
      notes: '',
    });
    this.criteriaDialog = true;
  }

  openEditCriteria(row: DailyEvaluationCriteriaReadDto): void {
    this.editingCriteriaId = row.dailyEvaluationCriteriaID;
    this.critForm.patchValue({
      name: row.name,
      description: row.description ?? '',
      weight: row.weight,
      minScore: row.minScore,
      maxScore: row.maxScore,
      isMandatory: row.isMandatory,
      sortOrder: row.sortOrder,
      isActive: row.isActive,
      notes: row.notes ?? '',
    });
    this.criteriaDialog = true;
  }

  saveCriteria(): void {
    if (this.critForm.invalid || !this.canManage) {
      this.critForm.markAllAsTouched();
      return;
    }
    const v = this.critForm.getRawValue();
    if (v.maxScore < v.minScore) {
      this.toastr.error(this.translate.instant('dailyEvaluations.criteria.scoreRangeError'));
      return;
    }
    const saving = this.editingCriteriaId
      ? this.svc.updateCriteria(this.editingCriteriaId, {
          name: v.name.trim(),
          description: v.description?.trim() || null,
          weight: v.weight,
          minScore: v.minScore,
          maxScore: v.maxScore,
          isMandatory: v.isMandatory,
          sortOrder: v.sortOrder,
          isActive: v.isActive,
          notes: v.notes?.trim() || null,
        })
      : this.svc.createCriteria(this.templateId, {
          name: v.name.trim(),
          description: v.description?.trim() || null,
          weight: v.weight,
          minScore: v.minScore,
          maxScore: v.maxScore,
          isMandatory: v.isMandatory,
          sortOrder: v.sortOrder,
          notes: v.notes?.trim() || null,
        });
    saving.subscribe({
      next: () => {
        this.toastr.success('dailyEvaluations.toast.criteriaSaved');
        this.criteriaDialog = false;
        this.loadCriteria();
      },
      error: (err) => this.toastr.error(readDailyEvalHttpError(err)),
    });
  }
}
