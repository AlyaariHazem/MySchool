import {Component,ChangeDetectorRef,OnInit,PLATFORM_ID,inject,effect,} from '@angular/core';
import { Store } from '@ngrx/store';
import { isPlatformBrowser } from '@angular/common';
import { PaginatorState } from 'primeng/paginator';
import { combineLatest, map } from 'rxjs';

import { StudentDetailsDTO } from '../../../core/models/students.model';
import { Year } from '../../../core/models/year.model';
import { StudentService } from '../../../core/services/student.service';
import { YearService } from '../../../core/services/year.service';
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
  private platformId = inject(PLATFORM_ID);

  constructor(private cd: ChangeDetectorRef,private store:Store) { }

  readonly dir$ = this.store.select(selectLanguage).pipe(
    map(l => (l === 'ar' ? 'rtl' : 'ltr')),
  );
  // ────────────────────────────  Data
  students: StudentDetailsDTO[] = []; // All students for chart
  paginatedStudents: StudentDetailsDTO[] = []; // Paginated students for table
  years: Year[] = [];
  isLoading = true;

  // ────────────────────────────  Pagination
  first = 0;
  rows = 10;
  currentPage = 1;
  pageSize = 10;
  totalRecords = 0;
  
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
    combineLatest([
      this.yearService.getAllYears(),
      // Fetch students with max page size (10) for chart
      // Note: Chart will only show data for first 10 students due to API limit
      this.studentService.getAllStudentsPaginated(1, 10), // Max page size
    ]).subscribe({
      next: ([years, paginatedResult]) => {
        this.years = years;
        // Extract students array from paginated result
        this.students = paginatedResult.data || []; // Students for chart (max 10)
        this.totalRecords = paginatedResult.totalCount || 0;
        this.isLoading = false;
        this.initChart();
        this.loadPaginatedStudents(); // Load paginated data for table
      },
      error: (err) => {
        console.error("Error loading dashboard data:", err);
        this.isLoading = false;
        this.loadPaginatedStudents(); // Still try to load paginated data
      }
    });
  }

  private loadPaginatedStudents(): void {
    this.studentService.getAllStudentsPaginated(this.currentPage, this.pageSize).subscribe({
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

  themeEffect = effect(() => {
  });
}
