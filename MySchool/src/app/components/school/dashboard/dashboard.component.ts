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
  students: StudentDetailsDTO[] = [];
  years: Year[] = [];
  isLoading = true;

  // ────────────────────────────  Pagination
  first = 0;
  rows = 4;
  onPageChange(event: PaginatorState): void {
    this.first = event.first ?? 0;
    this.rows = event.rows ?? 4;
  }

  basicData: any;
  basicOptions: any;

  // ────────────────────────────  INIT
  ngOnInit(): void {
    combineLatest([
      this.yearService.getAllYears(),
      this.studentService.getAllStudents(),
    ]).subscribe(([years, students]) => {
      this.years = years;
      this.students = students;
      this.isLoading = false;
      this.initChart();
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
