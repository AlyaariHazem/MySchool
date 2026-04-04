import {
  Component,
  ChangeDetectorRef,
  OnInit,
  PLATFORM_ID,
  inject,
  effect,
  DestroyRef,
} from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { Store } from '@ngrx/store';
import { isPlatformBrowser } from '@angular/common';
import { PaginatorState } from 'primeng/paginator';
import { combineLatest, debounceTime, distinctUntilChanged, map, Subject } from 'rxjs';

import { StudentDetailsDTO } from '../../../core/models/students.model';
import { Year } from '../../../core/models/year.model';
import { DashboardSummary, StudentEnrollmentTrend } from '../../../core/models/dashboard.model';
import { StudentService } from '../../../core/services/student.service';
import { YearService } from '../../../core/services/year.service';
import { DashboardService } from '../../../core/services/dashboard.service';
import { selectLanguage } from '../../../core/store/language/language.selectors';
@Component({
  selector: 'app-dashboard',
  templateUrl: './dashboard.component.html',
  styleUrl: './dashboard.component.scss',
})
export class DashboardComponent implements OnInit {
  // ────────────────────────────  DI
  private studentService = inject(StudentService);
  private yearService = inject(YearService);
  private dashboardService = inject(DashboardService);
  private platformId = inject(PLATFORM_ID);
  private destroyRef = inject(DestroyRef);

  private readonly studentSearchInput$ = new Subject<string>();

  constructor(private cd: ChangeDetectorRef, private store: Store) {
    this.studentSearchInput$
      .pipe(
        debounceTime(300),
        distinctUntilChanged(),
        takeUntilDestroyed(this.destroyRef),
      )
      .subscribe(() => {
        this.currentPage = 1;
        this.first = 0;
        this.loadPaginatedStudents();
      });
  }

  readonly dir$ = this.store.select(selectLanguage).pipe(
    map(l => (l === 'ar' ? 'rtl' : 'ltr')),
  );
  // ────────────────────────────  Data
  students: StudentDetailsDTO[] = []; // All students for chart
  paginatedStudents: StudentDetailsDTO[] = []; // Paginated students for table
  years: Year[] = [];
  isLoading = true;
  
  // Dashboard data from API
  dashboardSummary: DashboardSummary | null = null;
  studentEnrollmentTrend: StudentEnrollmentTrend[] = [];

  // ────────────────────────────  Pagination
  first = 0;
  rows = 10;
  currentPage = 1;
  pageSize = 10;
  totalRecords = 0;

  /** Live filter for “كل الطلاب” — debounced → POST /Students/page with `search` */
  studentTableSearchText = '';

  onPageChange(event: PaginatorState): void {
    this.first = event.first ?? 0;
    this.rows = event.rows ?? this.pageSize;
    this.currentPage = Math.floor((event.first || 0) / (event.rows || this.pageSize)) + 1;
    this.pageSize = event.rows || 10;
    this.loadPaginatedStudents();
  }

  basicData: any;
  basicOptions: any;

  // ────────────────────────────  INIT
  ngOnInit(): void {
    // Load dashboard data from API
    this.dashboardService.getDashboardData().subscribe({
      next: (response) => {
        if (response.isSuccess && response.result) {
          this.dashboardSummary = response.result.summary;
          this.studentEnrollmentTrend = response.result.studentEnrollmentTrend || [];
          this.initChartFromEnrollmentTrend();
        }
        this.isLoading = false;
      },
      error: (err) => {
        console.error("Error loading dashboard data:", err);
        this.isLoading = false;
      }
    });

    // Load paginated students for table
    combineLatest([
      this.yearService.getAllYears(),
      this.studentService.getStudentsPage(this.currentPage, this.pageSize),
    ]).subscribe({
      next: ([years, paginatedResult]) => {
        this.years = years;
        this.students = paginatedResult.data || [];
        this.totalRecords = paginatedResult.totalCount || 0;
        this.loadPaginatedStudents(); // Load paginated data for table
      },
      error: (err) => {
        console.error("Error loading students data:", err);
        this.loadPaginatedStudents(); // Still try to load paginated data
      }
    });
  }

  onStudentSearchChange(value: string): void {
    this.studentSearchInput$.next((value ?? '').trim());
  }

  /** First + middle + last for hover tooltip */
  studentFullNameTooltip(student: StudentDetailsDTO): string {
    const fn = student?.fullName;
    if (!fn) {
      return '';
    }
    const parts = [fn.firstName, fn.middleName, fn.lastName].filter(
      (p) => p != null && String(p).trim() !== '',
    );
    return parts.join(' ').trim();
  }

  private loadPaginatedStudents(): void {
    const filters: Record<string, string> = {};
    const q = this.studentTableSearchText.trim();
    if (q.length > 0) {
      filters['search'] = q;
    }

    this.studentService.getStudentsPage(this.currentPage, this.pageSize, filters).subscribe({
      next: (res) => {
        this.paginatedStudents = res.data || [];
        this.totalRecords = res.totalCount || 0;
        this.currentPage = res.pageNumber || this.currentPage;
        this.pageSize = res.pageSize || this.pageSize;
        this.first = (this.currentPage - 1) * this.pageSize;
        this.rows = this.pageSize;
      },
      error: (err) => {
        console.error("Error fetching paginated students:", err);
        this.paginatedStudents = [];
        this.totalRecords = 0;
      }
    });
  }

  private countStudentsPerYear(): number[] {
    return this.years.map(yr => {
      const start = new Date(yr.yearDateStart);
      const end = new Date(yr.yearDateEnd);

      return this.students.filter(st => {
        if (!st.hireDate) return false;
        const h = new Date(st.hireDate);
        return h >= start && h <= end;
      }).length;
    });
  }


  // ────────────────────────────  CHART
  initChart(): void {
    if (!isPlatformBrowser(this.platformId)) return;

    const css = getComputedStyle(document.documentElement);
    const textColor = css.getPropertyValue('--p-text-color');
    const textMuted = css.getPropertyValue('--p-text-muted-color');
    const borderClr = css.getPropertyValue('--p-content-border-color');

    const labels = this.years
      .map((y) => new Date(y.yearDateStart).getFullYear().toString())
      .sort();

    const data = this.countStudentsPerYear();

    if (!labels.length) return;

    const paletteBg = [
      'rgba(249,115,22,0.2)',
      'rgba(6,182,212,0.2)',
      'rgba(107,114,128,0.2)',
      'rgba(139,92,246,0.2)',
    ];
    const paletteBr = [
      'rgb(249,115,22)',
      'rgb(6,182,212)',
      'rgb(107,114,128)',
      'rgb(139,92,246)',
    ];

    this.basicData = {
      labels,
      datasets: [
        {
          label: 'Students',
          data,
          backgroundColor: labels.map((_, i) => paletteBg[i % paletteBg.length]),
          borderColor: labels.map((_, i) => paletteBr[i % paletteBr.length]),
          borderWidth: 1,
        },
      ],
    };

    this.basicOptions = {
      plugins: {
        legend: {
          labels: { color: textColor },
        },
      },
      scales: {
        x: {
          ticks: { color: textMuted },
          grid: { color: borderClr },
        },
        y: {
          beginAtZero: true,
          ticks: { color: textMuted },
          grid: { color: borderClr },
        },
      },
    };

    this.cd.markForCheck();
  }

  // Initialize chart from enrollment trend data from API
  initChartFromEnrollmentTrend(): void {
    if (!isPlatformBrowser(this.platformId)) return;
    if (!this.studentEnrollmentTrend || this.studentEnrollmentTrend.length === 0) return;

    const css = getComputedStyle(document.documentElement);
    const textColor = css.getPropertyValue('--p-text-color');
    const textMuted = css.getPropertyValue('--p-text-muted-color');
    const borderClr = css.getPropertyValue('--p-content-border-color');

    const labels = this.studentEnrollmentTrend.map(trend => trend.year.toString());
    const data = this.studentEnrollmentTrend.map(trend => trend.studentCount);

    const paletteBg = [
      'rgba(249,115,22,0.2)',
      'rgba(6,182,212,0.2)',
      'rgba(107,114,128,0.2)',
      'rgba(139,92,246,0.2)',
    ];
    const paletteBr = [
      'rgb(249,115,22)',
      'rgb(6,182,212)',
      'rgb(107,114,128)',
      'rgb(139,92,246)',
    ];

    this.basicData = {
      labels,
      datasets: [
        {
          label: 'Students',
          data,
          backgroundColor: labels.map((_, i) => paletteBg[i % paletteBg.length]),
          borderColor: labels.map((_, i) => paletteBr[i % paletteBr.length]),
          borderWidth: 1,
        },
      ],
    };

    this.basicOptions = {
      plugins: {
        legend: {
          labels: { color: textColor },
        },
      },
      scales: {
        x: {
          ticks: { color: textMuted },
          grid: { color: borderClr },
        },
        y: {
          beginAtZero: true,
          ticks: { color: textMuted },
          grid: { color: borderClr },
        },
      },
    };

    this.cd.markForCheck();
  }

  themeEffect = effect(() => {
  });
}
