import { AsyncPipe, DatePipe, NgIf } from '@angular/common';
import { Component, OnDestroy, OnInit, inject } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { Store } from '@ngrx/store';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { ConfirmationService } from 'primeng/api';
import { ButtonModule } from 'primeng/button';
import { ConfirmDialogModule } from 'primeng/confirmdialog';
import { FloatLabelModule } from 'primeng/floatlabel';
import { ProgressSpinnerModule } from 'primeng/progressspinner';
import { Select } from 'primeng/select';
import { TableLazyLoadEvent, TableModule } from 'primeng/table';
import { TooltipModule } from 'primeng/tooltip';
import { map } from 'rxjs';
import { ToastrService } from 'ngx-toastr';
import { finalize } from 'rxjs/operators';

import { PagePermission, PermissionService } from 'app/core/services/permission.service';
import { SchoolService } from 'app/core/services/school.service';
import { YearService } from 'app/core/services/year.service';
import { Year } from 'app/core/models/year.model';
import { School } from 'app/core/models/school.modul';
import { selectLanguage } from 'app/core/store/language/language.selectors';
import { isSchoolManagerUser } from 'app/core/utils/school-role.util';
import { ShardModule } from 'app/shared/shard.module';

import {
  DailyEvaluationTemplateFilterDto,
  DailyEvaluationTemplateListDto,
  EvaluationTemplateStatus,
} from '../daily-evaluations.models';
import { DailyEvaluationsNavService } from '../daily-evaluations-nav.service';
import { DailyEvaluationsService, readDailyEvalHttpError } from '../daily-evaluations.service';

@Component({
  selector: 'app-daily-eval-templates-list',
  standalone: true,
  imports: [
    ShardModule,
    NgIf,
    AsyncPipe,
    DatePipe,
    FormsModule,
    RouterLink,
    TranslateModule,
    TableModule,
    ButtonModule,
    Select,
    FloatLabelModule,
    ProgressSpinnerModule,
    TooltipModule,
    ConfirmDialogModule,
  ],
  providers: [ConfirmationService],
  templateUrl: './daily-eval-templates-list.component.html',
  styleUrl: './daily-eval-templates-list.component.scss',
})
export class DailyEvalTemplatesListComponent implements OnInit, OnDestroy {
  private readonly svc = inject(DailyEvaluationsService);
  private readonly schoolService = inject(SchoolService);
  private readonly yearService = inject(YearService);
  private readonly toastr = inject(ToastrService);
  private readonly confirm = inject(ConfirmationService);
  private readonly translate = inject(TranslateService);
  private readonly perm = inject(PermissionService);
  private readonly store = inject(Store);
  readonly dailyEvalNav = inject(DailyEvaluationsNavService);

  readonly filterSelectPanelStyle: Record<string, string> = {
    maxWidth: 'min(22rem, calc(100vw - 2rem))',
  };

  readonly dir$ = this.store.select(selectLanguage).pipe(map((l) => (l === 'ar' ? 'rtl' : 'ltr')));

  schools: School[] = [];
  years: Year[] = [];
  rows: DailyEvaluationTemplateListDto[] = [];
  totalRecords = 0;
  first = 0;
  pageSize = 10;
  loading = false;
  /** School HR: wait for years before first lazy load (filter uses active year). */
  yearLoading = false;
  error: string | null = null;

  filter: DailyEvaluationTemplateFilterDto = {};

  canView = this.perm.hasPermission(PagePermission.Evaluations.View);
  canManage = this.perm.hasPermission(PagePermission.Evaluations.Update);

  schoolOptions: { label: string; value: number }[] = [];
  statusOptions: { label: string; value: EvaluationTemplateStatus }[] = [];
  activeOptions: { label: string; value: boolean | null }[] = [];

  EvaluationTemplateStatus = EvaluationTemplateStatus;

  ngOnInit(): void {
    this.statusOptions = [
      { label: this.translate.instant('dailyEvaluations.templateStatus.draft'), value: EvaluationTemplateStatus.Draft },
      { label: this.translate.instant('dailyEvaluations.templateStatus.active'), value: EvaluationTemplateStatus.Active },
      {
        label: this.translate.instant('dailyEvaluations.templateStatus.inactive'),
        value: EvaluationTemplateStatus.Inactive,
      },
      {
        label: this.translate.instant('dailyEvaluations.templateStatus.archived'),
        value: EvaluationTemplateStatus.Archived,
      },
    ];
    this.activeOptions = [
      { label: this.translate.instant('employeesHr.filter.allActive'), value: null },
      { label: this.translate.instant('employeesHr.filter.activeOnly'), value: true },
      { label: this.translate.instant('employeesHr.filter.inactiveOnly'), value: false },
    ];
    if (this.isTeacherEvaluations) {
      const opt = this.dailyEvalNav.teacherSessionSchoolOption();
      if (opt) {
        this.filter.schoolID = opt.value;
        this.schoolOptions = [opt];
        this.schools = [{ schoolID: opt.value, schoolName: opt.label } as School];
      }
      const yid = this.dailyEvalNav.teacherSessionYearId();
      if (yid != null) {
        this.filter.academicYearID = yid;
      }
      this.yearLoading = false;
      return;
    }

    if (this.isSchoolManager) {
      const sid = Number(typeof localStorage !== 'undefined' ? localStorage.getItem('schoolId') : '');
      if (Number.isFinite(sid) && sid > 0) {
        this.filter.schoolID = sid;
      }
    }
    this.yearLoading = true;
    this.schoolService.getAllSchools().subscribe({
      next: (list) => {
        this.schools = list ?? [];
        this.schoolOptions = this.schools
          .filter((s): s is School & { schoolID: number } => s.schoolID != null && s.schoolID > 0)
          .map((s) => ({ label: s.schoolName || String(s.schoolID), value: s.schoolID }));
      },
      error: () => this.toastr.error('employeesHr.errors.loadSchools'),
    });
    this.yearService.getAllYears().subscribe({
      next: (list) => {
        this.years = list ?? [];
        this.yearLoading = false;
      },
      error: () => {
        this.yearLoading = false;
        this.toastr.error('employeesHr.errors.loadYears');
      },
    });
  }

  get isTeacherEvaluations(): boolean {
    return this.dailyEvalNav.isTeacherDailyEvaluationsRoute();
  }

  get isSchoolManager(): boolean {
    return isSchoolManagerUser();
  }

  ngOnDestroy(): void {}

  onSchoolChange(): void {
    this.first = 0;
    this.load();
  }

  applyFilters(): void {
    this.first = 0;
    this.load();
  }

  onLazyLoad(event: TableLazyLoadEvent): void {
    this.first = event.first ?? 0;
    const r = event.rows;
    if (r != null && r > 0) {
      this.pageSize = r;
    }
    this.load();
  }

  private yearIdNum(y: Year): number {
    const raw = y as unknown as { yearID?: number; YearID?: number };
    const n = raw.yearID ?? raw.YearID;
    return typeof n === 'number' && !Number.isNaN(n) ? n : 0;
  }

  load(): void {
    if (!this.canView) {
      this.error = 'dailyEvaluations.errors.noPermission';
      return;
    }
    if (this.yearLoading) {
      return;
    }
    this.loading = true;
    this.error = null;
    const pageIndex = this.pageSize > 0 ? Math.floor(this.first / this.pageSize) : 0;
    this.svc
      .getTemplatesPage({
        pageIndex,
        pageSize: this.pageSize,
        filter: this.filter,
      })
      .pipe(finalize(() => (this.loading = false)))
      .subscribe({
        next: (page) => {
          this.rows = page.data ?? [];
          this.totalRecords = page.totalCount ?? 0;
        },
        error: (err) => {
          this.error = readDailyEvalHttpError(err);
          this.toastr.error(this.error);
        },
      });
  }

  schoolName(id: number): string {
    return this.schools.find((s) => s.schoolID === id)?.schoolName ?? String(id);
  }

  yearLabel(id: number): string {
    const y = this.years.find((x) => this.yearIdNum(x) === id);
    if (y) {
      return `${y.yearID} — ${y.yearDateStart ? new Date(y.yearDateStart).toLocaleDateString() : ''}`.trim();
    }
    if (this.isTeacherEvaluations) {
      const raw =
        typeof localStorage !== 'undefined'
          ? (localStorage.getItem('academicYear') ?? localStorage.getItem('studyYearName') ?? '').trim()
          : '';
      return raw ? `${id} — ${raw}` : String(id);
    }
    return String(id);
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

  confirmActivate(row: DailyEvaluationTemplateListDto): void {
    this.confirm.confirm({
      message: this.translate.instant('dailyEvaluations.templates.confirmActivate', { name: row.name }),
      header: this.translate.instant('employeesHr.list.confirmHeader'),
      icon: 'pi pi-check',
      accept: () =>
        this.svc.activateTemplate(row.dailyEvaluationTemplateID).subscribe({
          next: () => {
            this.toastr.success('dailyEvaluations.toast.templateActivated');
            this.load();
          },
          error: (err) => this.toastr.error(readDailyEvalHttpError(err)),
        }),
    });
  }

  confirmDeactivate(row: DailyEvaluationTemplateListDto): void {
    this.confirm.confirm({
      message: this.translate.instant('dailyEvaluations.templates.confirmDeactivate', { name: row.name }),
      header: this.translate.instant('employeesHr.list.confirmHeader'),
      icon: 'pi pi-pause',
      accept: () =>
        this.svc.deactivateTemplate(row.dailyEvaluationTemplateID).subscribe({
          next: () => {
            this.toastr.success('dailyEvaluations.toast.templateDeactivated');
            this.load();
          },
          error: (err) => this.toastr.error(readDailyEvalHttpError(err)),
        }),
    });
  }

  confirmArchive(row: DailyEvaluationTemplateListDto): void {
    this.confirm.confirm({
      message: this.translate.instant('dailyEvaluations.templates.confirmArchive', { name: row.name }),
      header: this.translate.instant('employeesHr.list.confirmHeader'),
      icon: 'pi pi-exclamation-triangle',
      accept: () =>
        this.svc.archiveTemplate(row.dailyEvaluationTemplateID).subscribe({
          next: () => {
            this.toastr.success('dailyEvaluations.toast.templateArchived');
            this.load();
          },
          error: (err) => this.toastr.error(readDailyEvalHttpError(err)),
        }),
    });
  }
}
