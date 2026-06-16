import { AsyncPipe, DatePipe, DecimalPipe, NgFor, NgIf } from '@angular/common';
import { Component, OnDestroy, OnInit, inject } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { Store } from '@ngrx/store';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { ButtonModule } from 'primeng/button';
import { FloatLabelModule } from 'primeng/floatlabel';
import { Select } from 'primeng/select';
import { TableModule } from 'primeng/table';
import { TabsModule } from 'primeng/tabs';
import { ToastrService } from 'ngx-toastr';
import { finalize, map } from 'rxjs/operators';
import { Subscription } from 'rxjs';

import { PagePermission, PermissionService } from 'app/core/services/permission.service';
import { SchoolService } from 'app/core/services/school.service';
import { School } from 'app/core/models/school.modul';
import { selectLanguage } from 'app/core/store/language/language.selectors';
import { isSchoolManagerUser } from 'app/core/utils/school-role.util';
import { ShardModule } from 'app/shared/shard.module';

import {
  AnalyticsGenerateRequest,
  AnalyticsPeriodKind,
  DashboardAudience,
  DepartmentAnalyticsDto,
  KpiSnapshotDto,
  SchoolAnalyticsDto,
  TeacherAnalyticsDto,
  TrendAnalysisDto,
} from './analytics.models';
import { AnalyticsService, readAnalyticsHttpError } from './analytics.service';

@Component({
  selector: 'app-analytics-page',
  standalone: true,
  imports: [
    ShardModule,
    NgIf,
    NgFor,
    AsyncPipe,
    DatePipe,
    DecimalPipe,
    FormsModule,
    TranslateModule,
    TabsModule,
    TableModule,
    ButtonModule,
    Select,
    FloatLabelModule,
  ],
  templateUrl: './analytics-page.component.html',
  styleUrl: './analytics-page.component.scss',
})
export class AnalyticsPageComponent implements OnInit, OnDestroy {
  private readonly svc = inject(AnalyticsService);
  private readonly schoolService = inject(SchoolService);
  private readonly toastr = inject(ToastrService);
  private readonly translate = inject(TranslateService);
  private readonly perm = inject(PermissionService);
  private readonly store = inject(Store);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private routeSub?: Subscription;

  readonly dir$ = this.store.select(selectLanguage).pipe(map((l) => (l === 'ar' ? 'rtl' : 'ltr')));
  readonly filterSelectPanelStyle: Record<string, string> = { maxWidth: 'min(22rem, calc(100vw - 2rem))' };

  activeTab = 'top';
  loading = false;
  generating = false;

  schoolOptions: { label: string; value: number }[] = [];
  periodKindOptions: { label: string; value: AnalyticsPeriodKind }[] = [];
  filterSchoolID: number | null = null;
  filterPeriodKind: AnalyticsPeriodKind = AnalyticsPeriodKind.Monthly;

  cards: { code: string; label: string; value: number; target?: number | null; trend?: number | null }[] = [];
  snapshotRows: KpiSnapshotDto[] = [];
  trendRows: TrendAnalysisDto[] = [];
  departmentRows: DepartmentAnalyticsDto[] = [];
  teacherRows: TeacherAnalyticsDto[] = [];
  schoolRows: SchoolAnalyticsDto[] = [];

  get isSchoolManager(): boolean {
    return isSchoolManagerUser();
  }
  get canView(): boolean {
    return this.perm.hasPermission(PagePermission.Employees.View);
  }

  get canManageSnapshots(): boolean {
    if (typeof window === 'undefined') return false;
    const t = localStorage.getItem('userType');
    return t === 'ADMIN' || t === 'MANAGER';
  }

  ngOnInit(): void {
    this.periodKindOptions = [
      { label: this.translate.instant('analytics.period.daily'), value: AnalyticsPeriodKind.Daily },
      { label: this.translate.instant('analytics.period.weekly'), value: AnalyticsPeriodKind.Weekly },
      { label: this.translate.instant('analytics.period.monthly'), value: AnalyticsPeriodKind.Monthly },
      { label: this.translate.instant('analytics.period.termly'), value: AnalyticsPeriodKind.Termly },
      { label: this.translate.instant('analytics.period.yearly'), value: AnalyticsPeriodKind.Yearly },
    ];

    if (!this.isSchoolManager) {
      this.schoolService.getAllSchools().subscribe({
        next: (schools: School[]) => {
          this.schoolOptions = (schools ?? [])
            .filter((s) => s.schoolID != null && s.schoolID > 0)
            .map((s) => ({ label: s.schoolName ?? String(s.schoolID), value: s.schoolID as number }));
        },
      });
    } else {
      const raw = typeof localStorage !== 'undefined' ? localStorage.getItem('schoolId') : null;
      const sid = raw != null ? Number(raw) : NaN;
      if (Number.isFinite(sid) && sid > 0) this.filterSchoolID = sid;
    }

    this.routeSub = this.route.paramMap.subscribe((pm) => {
      const tab = pm.get('tab');
      const key = tab ? this.tabToAudienceKey(tab) : null;
      if (!key) {
        void this.router.navigate(['/school/analytics/top'], { replaceUrl: true });
        return;
      }
      this.activeTab = key;
      if (this.canView) this.loadDashboard(this.currentAudience());
    });
  }

  ngOnDestroy(): void {
    this.routeSub?.unsubscribe();
  }

  private tabToAudienceKey(routeTab: string): string | null {
    const m: Record<string, string> = {
      top: 'top',
      edu: 'edu',
      admin: 'admin',
      self: 'self',
      school: 'school',
      years: 'years',
    };
    return m[routeTab] ?? null;
  }

  private audienceKeyToRoute(key: string): string {
    return key;
  }

  onTabChange(tab: string): void {
    void this.router.navigate(['/school/analytics', this.audienceKeyToRoute(tab)], { replaceUrl: true });
  }

  applyFilters(): void {
    this.loadDashboard(this.currentAudience());
  }

  generateSnapshots(): void {
    const sid = this.filterSchoolID;
    if (sid == null || sid <= 0) {
      this.toastr.warning(this.translate.instant('analytics.generate.needSchool'));
      return;
    }
    const body: AnalyticsGenerateRequest = {
      schoolID: sid,
      periodKind: this.filterPeriodKind,
      replaceExistingForPeriod: true,
    };
    this.generating = true;
    this.svc
      .generateSnapshots(body)
      .pipe(finalize(() => (this.generating = false)))
      .subscribe({
        next: () => {
          this.toastr.success(this.translate.instant('analytics.generate.done'));
          this.applyFilters();
        },
        error: (e) => this.toastr.error(readAnalyticsHttpError(e)),
      });
  }

  private currentAudience(): DashboardAudience {
    switch (this.activeTab) {
      case 'edu':
        return DashboardAudience.EducationalSupervisor;
      case 'admin':
        return DashboardAudience.AdministrativeSupervisor;
      case 'self':
        return DashboardAudience.EmployeeSelf;
      case 'school':
        return DashboardAudience.School;
      case 'years':
        return DashboardAudience.YearComparison;
      default:
        return DashboardAudience.TopManagement;
    }
  }

  private loadDashboard(audience: DashboardAudience): void {
    this.loading = true;
    this.svc
      .getDashboard({
        audience,
        schoolID: this.filterSchoolID,
        periodKind: this.filterPeriodKind,
      })
      .pipe(finalize(() => (this.loading = false)))
      .subscribe({
        next: (res) => {
          this.cards = res.cards ?? [];
          this.snapshotRows = res.snapshots ?? [];
          this.trendRows = res.trends ?? [];
          this.departmentRows = res.departments ?? [];
          this.teacherRows = res.teachers ?? [];
          this.schoolRows = res.school ?? [];
        },
        error: (e) => this.toastr.error(readAnalyticsHttpError(e)),
      });
  }
}
