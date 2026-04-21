import { NgFor, NgIf } from '@angular/common';
import { Component, EventEmitter, Input, OnInit, Output, inject } from '@angular/core';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { FormBuilder, FormsModule, ReactiveFormsModule, Validators } from '@angular/forms';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { ButtonModule } from 'primeng/button';
import { DatePicker } from 'primeng/datepicker';
import { FloatLabelModule } from 'primeng/floatlabel';
import { InputNumberModule } from 'primeng/inputnumber';
import { InputTextModule } from 'primeng/inputtext';
import { Select, SelectLazyLoadEvent } from 'primeng/select';
import { TextareaModule } from 'primeng/textarea';
import { ToastrService } from 'ngx-toastr';
import { forkJoin, of } from 'rxjs';
import { catchError, finalize, map, switchMap } from 'rxjs/operators';

import { isSchoolManagerUser } from 'app/core/utils/school-role.util';
import { PagePermission, PermissionService } from 'app/core/services/permission.service';
import { SchoolService } from 'app/core/services/school.service';
import { YearService } from 'app/core/services/year.service';
import { School } from 'app/core/models/school.modul';
import { Year } from 'app/core/models/year.model';
import { ShardModule } from 'app/shared/shard.module';
import { PagedResultDto } from 'app/core/models/students.model';

import { EmployeeProfileOptionDto } from '../../employees-hr/employees-hr.models';
import { EmployeesHrService } from '../../employees-hr/employees-hr.service';
import {
  DailyEvaluationCreateDto,
  DailyEvaluationFullDto,
  DailyEvaluationItemReadDto,
  DailyEvaluationStatus,
  DailyEvaluationTemplateFilterDto,
  DailyEvaluationTemplateListDto,
  EvaluationTemplateStatus,
  TeacherEvaluationOptionDto,
} from '../daily-evaluations.models';
import { DailyEvaluationsNavService } from '../daily-evaluations-nav.service';
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
  ],
  templateUrl: './daily-evaluations-form.component.html',
  styleUrl: './daily-evaluations-form.component.scss',
})
export class DailyEvaluationsFormComponent implements OnInit {
  /** When true, driven by inputs/outputs instead of route params (e.g. list dialogs). */
  @Input() embedded = false;
  /** Create = null, edit = id. Used when `embedded` is true. */
  @Input() evaluationIdInput: number | null = null;
  /** Optional defaults from list filters (school HR). */
  @Input() presetSchoolId: number | null = null;
  @Input() presetAcademicYearId: number | null = null;

  @Output() closed = new EventEmitter<void>();
  @Output() saved = new EventEmitter<void>();

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
  readonly dailyEvalNav = inject(DailyEvaluationsNavService);

  schools: School[] = [];
  years: Year[] = [];
  templates: DailyEvaluationTemplateListDto[] = [];
  schoolOptions: { label: string; value: number }[] = [];
  yearOptions: { label: string; value: number }[] = [];
  employeeOptions: { label: string; value: number }[] = [];
  templateOptions: { label: string; value: number }[] = [];

  /** Same as list page — keeps select overlays from stretching full width in RTL grids. */
  readonly filterSelectPanelStyle: Record<string, string> = {
    maxWidth: 'min(22rem, calc(100vw - 2rem))',
  };

  /** Admin HR dropdown: paged + lazy scroll (not used for student/teacher portal pickers). */
  readonly employeePageSize = 30;
  employeeTotalCount = 0;
  employeePageLoading = false;
  /** Next zero-based page index; first page is loaded with templates in forkJoin. */
  employeeNextPageIndex = 0;

  loadingMeta = true;
  saving = false;
  evaluationId: number | null = null;
  full: DailyEvaluationFullDto | null = null;
  itemDrafts: Record<number, { score: number; comment: string }> = {};
  critById = new Map<number, { min: number; max: number; mandatory: boolean }>();

  get isTeacherEvaluations(): boolean {
    return this.dailyEvalNav.isTeacherDailyEvaluationsRoute();
  }

  get isStudentDailyEvaluations(): boolean {
    return this.dailyEvalNav.isStudentDailyEvaluationsRoute();
  }

  /** Teacher or student portal — session school/year, no Evaluations.* permission required. */
  get isSessionPortalDailyEvaluations(): boolean {
    return this.isTeacherEvaluations || this.isStudentDailyEvaluations;
  }

  get isSchoolManager(): boolean {
    return isSchoolManagerUser();
  }

  /** When true, employee dropdown uses POST /employees/page + virtual scroll. */
  get employeeSelectLazy(): boolean {
    return !this.isStudentDailyEvaluations && !this.isTeacherEvaluations;
  }

  get canCreateEvaluations(): boolean {
    if (this.isSessionPortalDailyEvaluations) return true;
    return this.perm.hasPermission(PagePermission.Evaluations.Create);
  }

  get canUpdateEvaluations(): boolean {
    if (this.isSessionPortalDailyEvaluations) return true;
    return this.perm.hasPermission(PagePermission.Evaluations.Update);
  }

  headerForm = this.fb.nonNullable.group({
    schoolID: [null as number | null, Validators.required],
    academicYearID: [null as number | null, Validators.required],
    evaluatedEmployeeProfileID: [null as number | null, Validators.required],
    dailyEvaluationTemplateID: [null as number | null, Validators.required],
    evaluationDate: [null as Date | null, Validators.required],
    notes: [''],
  });

  ngOnInit(): void {
    this.evaluationId = this.embedded
      ? this.evaluationIdInput ?? null
      : Number(this.route.snapshot.paramMap.get('evaluationId')) || null;

    if (this.isSessionPortalDailyEvaluations) {
      const opt = this.dailyEvalNav.teacherSessionSchoolOption();
      if (opt) {
        this.schools = [{ schoolID: opt.value, schoolName: opt.label } as School];
        this.schoolOptions = [opt];
      }
      if (!this.evaluationId) {
        if (this.isStudentDailyEvaluations) {
          if (opt) {
            this.headerForm.patchValue({ schoolID: opt.value });
          }
        } else if (this.isTeacherEvaluations) {
          const yid = this.dailyEvalNav.teacherSessionYearId();
          if (yid != null) {
            this.headerForm.patchValue({
              schoolID: opt?.value ?? null,
              academicYearID: yid,
            });
          } else if (opt) {
            this.headerForm.patchValue({ schoolID: opt.value });
          }
        }
      }
    } else {
      if (this.isSchoolManager) {
        const sid = Number(typeof localStorage !== 'undefined' ? localStorage.getItem('schoolId') : '');
        if (Number.isFinite(sid) && sid > 0) {
          this.headerForm.patchValue({ schoolID: sid });
        }
      }
      this.schoolService.getAllSchools().subscribe({
        next: (s) => {
          this.schools = s ?? [];
          this.schoolOptions = this.schools
            .filter((x): x is School & { schoolID: number } => x.schoolID != null && x.schoolID > 0)
            .map((x) => ({ label: x.schoolName || String(x.schoolID), value: x.schoolID }));
        },
      });
    }
    this.yearService.getAllYears().subscribe({
      next: (y) => {
        this.years = y ?? [];
        this.rebuildYearOptions();
        if (!this.evaluationId) {
          if (this.embedded && this.presetSchoolId != null && this.presetSchoolId > 0) {
            this.headerForm.patchValue({ schoolID: this.presetSchoolId }, { emitEvent: false });
            this.rebuildYearOptions();
          }
          if (!this.isTeacherEvaluations) {
            if (!(this.embedded && this.presetAcademicYearId != null && this.presetAcademicYearId > 0)) {
              this.patchActiveYearForCurrentSchool();
            }
          }
          if (this.embedded && this.presetAcademicYearId != null && this.presetAcademicYearId > 0) {
            this.headerForm.patchValue({ academicYearID: this.presetAcademicYearId }, { emitEvent: false });
          }
          this.reloadEmployeesAndTemplates();
        }
      },
      error: () => {
        if (!this.evaluationId && !this.isTeacherEvaluations) {
          this.loadingMeta = false;
        }
      },
    });

    if (this.evaluationId) {
      if (!this.canUpdateEvaluations) {
        this.loadingMeta = false;
        return;
      }
      this.loadEdit(this.evaluationId);
    } else {
      if (!this.canCreateEvaluations) {
        this.loadingMeta = false;
        return;
      }
      this.headerForm.patchValue({ evaluationDate: new Date() });
      this.loadingMeta = this.isSessionPortalDailyEvaluations;
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
            if (this.embedded) {
              this.closed.emit();
            } else {
              this.router.navigate([this.dailyEvalNav.basePath(), id]).catch(() => undefined);
            }
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
          if (this.embedded) {
            this.closed.emit();
          } else {
            this.router.navigate([this.dailyEvalNav.basePath()]).catch(() => undefined);
          }
        },
      });
  }

  private parseD(s: string): Date {
    const d = new Date(s + 'T12:00:00');
    return Number.isNaN(d.getTime()) ? new Date() : d;
  }

  private onSchoolChange(): void {
    this.rebuildYearOptions();
    if (this.isTeacherEvaluations) {
      const yid = this.headerForm.get('academicYearID')?.value;
      if (yid && !this.yearOptions.some((opt) => opt.value === yid)) {
        this.headerForm.patchValue({ academicYearID: null });
      }
    }
    if (!this.isTeacherEvaluations && !this.evaluationId) {
      this.patchActiveYearForCurrentSchool();
      this.reloadEmployeesAndTemplates();
      return;
    }
    if (!this.evaluationId) {
      this.reloadEmployeesAndTemplates();
    }
  }

  private yearIsActive(y: Year): boolean {
    const raw = y as unknown as { active?: boolean; Active?: boolean };
    return !!(raw.active ?? raw.Active);
  }

  private yearSchoolId(y: Year): number {
    const raw = y as unknown as { schoolID?: number; SchoolID?: number };
    return raw.schoolID ?? raw.SchoolID ?? 0;
  }

  private yearIdNum(y: Year): number {
    const raw = y as unknown as { yearID?: number; YearID?: number };
    const n = raw.yearID ?? raw.YearID;
    return typeof n === 'number' && !Number.isNaN(n) ? n : 0;
  }

  /** Same rules as backend GetActiveYearIdForSchoolAsync: Active for school, else latest YearID. */
  private resolveActiveYearIdForSchool(schoolId: number | null): number | null {
    if (schoolId == null || schoolId <= 0) return null;
    const forSchool = this.years.filter((x) => this.yearSchoolId(x) === schoolId);
    const actives = forSchool.filter((x) => this.yearIsActive(x)).sort((a, b) => this.yearIdNum(a) - this.yearIdNum(b));
    if (actives.length) return this.yearIdNum(actives[0]);
    const latest = [...forSchool].sort((a, b) => this.yearIdNum(b) - this.yearIdNum(a));
    return latest.length ? this.yearIdNum(latest[0]) : null;
  }

  /** School HR + student: no year dropdown — active year for the selected school (matches backend). */
  private patchActiveYearForCurrentSchool(): void {
    if (this.evaluationId || this.isTeacherEvaluations) return;
    const sid = this.headerForm.get('schoolID')?.value ?? null;
    const yid = this.resolveActiveYearIdForSchool(sid);
    if (yid != null) {
      this.headerForm.patchValue({ academicYearID: yid }, { emitEvent: false });
    }
  }

  private rebuildYearOptions(): void {
    const sid = this.headerForm.get('schoolID')?.value ?? this.full?.schoolID;
    const filtered =
      sid != null && sid > 0 ? this.years.filter((y) => this.yearSchoolId(y) === sid) : [...this.years];
    this.yearOptions = filtered.map((y) => ({
      label: `${this.yearIdNum(y)} — ${y.yearDateStart ? new Date(y.yearDateStart).toLocaleDateString() : ''}`,
      value: this.yearIdNum(y),
    }));
  }

  /** Templates that the API allows for new evaluations (active + flagged active). */
  private templateFilterForCreate(sid: number, yid: number): DailyEvaluationTemplateFilterDto {
    return {
      schoolID: sid,
      academicYearID: yid,
      status: EvaluationTemplateStatus.Active,
      isActive: true,
    };
  }

  private applyTemplateList(tpl: DailyEvaluationTemplateListDto[] | null): void {
    this.templates = (tpl ?? []).filter(
      (t) => t.status === EvaluationTemplateStatus.Active && t.isActive,
    );
    this.templateOptions = this.templates.map((t) => ({
      label: t.name,
      value: t.dailyEvaluationTemplateID,
    }));
  }

  reloadEmployeesAndTemplates(done?: () => void): void {
    const sid = this.headerForm.get('schoolID')?.value;
    const yid = this.headerForm.get('academicYearID')?.value;
    if (!sid || !yid) {
      this.resetEmployeeScrollState();
      this.templates = [];
      this.employeeOptions = [];
      this.templateOptions = [];
      if (this.isSessionPortalDailyEvaluations && !this.evaluationId) {
        this.loadingMeta = false;
      }
      done?.();
      return;
    }

    if (this.isStudentDailyEvaluations) {
      if (this.evaluationId) {
        this.svc
          .getTemplates(this.templateFilterForCreate(sid, yid))
          .pipe(
            finalize(() => {
              if (!this.evaluationId) {
                this.loadingMeta = false;
              }
              done?.();
            }),
          )
          .subscribe({
            next: (tpl) => this.applyTemplateList(tpl),
            error: (err) => {
              this.toastr.error(readDailyEvalHttpError(err));
              this.applyTemplateList([]);
              if (!this.evaluationId) {
                this.loadingMeta = false;
              }
            },
          });
        return;
      }

      const tpl$ = this.svc.getTemplates(this.templateFilterForCreate(sid, yid)).pipe(
        catchError((err) => {
          this.toastr.error(readDailyEvalHttpError(err));
          return of([] as DailyEvaluationTemplateListDto[]);
        }),
      );
      const teachers$ = this.svc.getTeachersForStudentEvaluation(sid).pipe(
        catchError((err) => {
          this.toastr.error(readDailyEvalHttpError(err));
          return of([] as TeacherEvaluationOptionDto[]);
        }),
      );

      forkJoin({ tpl: tpl$, teachers: teachers$ })
        .pipe(
          finalize(() => {
            if (!this.evaluationId) {
              this.loadingMeta = false;
            }
            done?.();
          }),
        )
        .subscribe({
          next: ({ tpl, teachers }) => {
            this.applyTemplateList(tpl);
            this.employeeOptions = (teachers ?? []).map((r) => ({
              label: r.displayName,
              value: r.employeeProfileID,
            }));
          },
        });
      return;
    }

    if (this.isTeacherEvaluations) {
      this.svc
        .getTemplates(this.templateFilterForCreate(sid, yid))
        .pipe(
          switchMap((tpl) => {
            if (this.evaluationId) {
              return of({ tpl, empId: null as number | null });
            }
            return this.svc.getMyEmployeeProfileId().pipe(
              map((empId) => ({ tpl, empId })),
              catchError((err) => {
                this.toastr.error(readDailyEvalHttpError(err));
                return of({ tpl, empId: null as number | null });
              }),
            );
          }),
          finalize(() => {
            if (!this.evaluationId) {
              this.loadingMeta = false;
            }
            done?.();
          }),
        )
        .subscribe({
          next: ({ tpl, empId }) => {
            this.applyTemplateList(tpl);
            this.employeeOptions = [];
            if (!this.evaluationId && empId != null) {
              this.headerForm.patchValue({ evaluatedEmployeeProfileID: empId });
            }
          },
          error: (err) => {
            this.toastr.error(readDailyEvalHttpError(err));
            this.applyTemplateList([]);
            if (!this.evaluationId) {
              this.loadingMeta = false;
            }
          },
        });
      return;
    }

    const emptyEmpPage: PagedResultDto<EmployeeProfileOptionDto> = {
      data: [],
      pageNumber: 0,
      pageSize: 0,
      totalCount: 0,
      totalPages: 0,
    };

    this.resetEmployeeScrollState();

    forkJoin({
      em: this.employeesHr
        .getEmployeesPage({
          pageIndex: 0,
          pageSize: this.employeePageSize,
          filter: { schoolID: sid },
        })
        .pipe(catchError(() => of(emptyEmpPage))),
      tpl: this.svc.getTemplates(this.templateFilterForCreate(sid, yid)).pipe(
        catchError((err) => {
          this.toastr.error(readDailyEvalHttpError(err));
          return of([] as DailyEvaluationTemplateListDto[]);
        }),
      ),
    }).subscribe({
      next: ({ em, tpl }) => {
        this.applyEmployeePageResult(em, false);
        this.applyTemplateList(tpl);
        done?.();
      },
      error: () => done?.(),
    });
  }

  private resetEmployeeScrollState(): void {
    this.employeeTotalCount = 0;
    this.employeeNextPageIndex = 0;
    this.employeePageLoading = false;
  }

  private applyEmployeePageResult(p: PagedResultDto<EmployeeProfileOptionDto>, append: boolean): void {
    const rows = p.data ?? [];
    const mapped = rows.map((o) => ({
      label: this.displayNameFromOption(o),
      value: o.id,
    }));
    if (append) {
      const existing = new Set(this.employeeOptions.map((x) => x.value));
      const merged = [...this.employeeOptions];
      for (const m of mapped) {
        if (!existing.has(m.value)) {
          merged.push(m);
          existing.add(m.value);
        }
      }
      this.employeeOptions = merged;
    } else {
      this.employeeOptions = mapped;
    }
    this.employeeTotalCount = p.totalCount;
    this.employeeNextPageIndex = p.pageNumber;
  }

  displayNameFromOption(o: EmployeeProfileOptionDto): string {
    const n = o.fullName;
    if (!n) return String(o.id);
    return [n.firstName, n.middleName, n.lastName].filter(Boolean).join(' ');
  }

  onEmployeeLazyLoad(event: SelectLazyLoadEvent): void {
    if (!this.employeeSelectLazy) return;
    if (this.employeePageLoading) return;
    const sid = this.headerForm.get('schoolID')?.value;
    const yid = this.headerForm.get('academicYearID')?.value;
    if (!sid || !yid) return;
    if (this.employeeNextPageIndex === 0) return;
    if (this.employeeTotalCount <= 0) return;
    const loaded = this.employeeOptions.length;
    if (loaded >= this.employeeTotalCount) return;
    const buffer = 5;
    if (loaded > 0 && event.last + buffer < loaded - 1) return;

    this.appendEmployeePage();
  }

  private appendEmployeePage(): void {
    if (!this.employeeSelectLazy) return;
    if (this.employeePageLoading) return;
    const sid = this.headerForm.get('schoolID')?.value;
    const yid = this.headerForm.get('academicYearID')?.value;
    if (!sid || !yid) return;
    if (this.employeeTotalCount > 0 && this.employeeOptions.length >= this.employeeTotalCount) return;

    this.employeePageLoading = true;
    this.employeesHr
      .getEmployeesPage({
        pageIndex: this.employeeNextPageIndex,
        pageSize: this.employeePageSize,
        filter: { schoolID: sid },
      })
      .pipe(finalize(() => (this.employeePageLoading = false)))
      .subscribe({
        next: (p) => this.applyEmployeePageResult(p, true),
        error: () => undefined,
      });
  }

  cancel(): void {
    if (this.embedded) {
      this.closed.emit();
      return;
    }
    this.router.navigate([this.dailyEvalNav.basePath()]).catch(() => undefined);
  }

  createEvaluation(): void {
    if (this.headerForm.invalid || !this.canCreateEvaluations) {
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
          if (this.embedded) {
            this.saved.emit();
            this.closed.emit();
          } else {
            this.router.navigate([this.dailyEvalNav.basePath(), row.dailyEvaluationID, 'edit']).catch(() => undefined);
          }
        },
        error: (err) => this.toastr.error(readDailyEvalHttpError(err)),
      });
  }

  saveItemsAndNotes(): void {
    if (!this.full || !this.canUpdateEvaluations) return;
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
          if (this.embedded) {
            this.saved.emit();
          }
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
          if (this.embedded) {
            this.saved.emit();
            this.closed.emit();
          } else {
            this.router.navigate([this.dailyEvalNav.basePath(), this.full!.dailyEvaluationID]).catch(() => undefined);
          }
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
