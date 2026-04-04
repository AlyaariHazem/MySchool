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
import { DatePipe, isPlatformBrowser } from '@angular/common';
import type { jsPDF } from 'jspdf';
import { Store } from '@ngrx/store';
import { PaginatorState } from 'primeng/paginator';
import { TranslateService } from '@ngx-translate/core';
import { ToastrService } from 'ngx-toastr';
import {
  combineLatest,
  debounceTime,
  distinctUntilChanged,
  forkJoin,
  map,
  of,
  Subject,
  switchMap,
} from 'rxjs';

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
  private translate = inject(TranslateService);
  private toastr = inject(ToastrService);
  private datePipe = inject(DatePipe);

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

  /** Excel / PDF export in progress */
  studentsExporting = false;

  private static readonly exportPageSize = 100;

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

  exportStudentsExcel(): void {
    if (!isPlatformBrowser(this.platformId)) {
      return;
    }
    this.studentsExporting = true;
    this.fetchAllStudentsForExport().subscribe({
      next: async (rows) => {
        if (rows.length === 0) {
          this.studentsExporting = false;
          this.toastr.warning(this.translate.instant('dashboard.exportEmpty'));
          this.cd.markForCheck();
          return;
        }
        try {
          await this.downloadStudentsXlsx(rows);
          this.toastr.success(this.translate.instant('dashboard.exportSuccess'));
        } catch {
          this.toastr.error(this.translate.instant('dashboard.exportError'));
        } finally {
          this.studentsExporting = false;
          this.cd.markForCheck();
        }
      },
      error: () => {
        this.studentsExporting = false;
        this.toastr.error(this.translate.instant('dashboard.exportError'));
        this.cd.markForCheck();
      },
    });
  }

  exportStudentsPdf(): void {
    if (!isPlatformBrowser(this.platformId)) {
      return;
    }
    this.studentsExporting = true;
    this.fetchAllStudentsForExport().subscribe({
      next: async (rows) => {
        if (rows.length === 0) {
          this.studentsExporting = false;
          this.toastr.warning(this.translate.instant('dashboard.exportEmpty'));
          this.cd.markForCheck();
          return;
        }
        try {
          await this.downloadStudentsPdfAutoTable(rows);
          this.toastr.success(this.translate.instant('dashboard.exportSuccess'));
        } catch {
          this.toastr.error(this.translate.instant('dashboard.exportError'));
        } finally {
          this.studentsExporting = false;
          this.cd.markForCheck();
        }
      },
      error: () => {
        this.studentsExporting = false;
        this.toastr.error(this.translate.instant('dashboard.exportError'));
        this.cd.markForCheck();
      },
    });
  }

  private buildStudentTableFilters(): Record<string, string> {
    const filters: Record<string, string> = {};
    const q = this.studentTableSearchText.trim();
    if (q.length > 0) {
      filters['search'] = q;
    }
    return filters;
  }

  private fetchAllStudentsForExport() {
    const filters = this.buildStudentTableFilters();
    const pageSize = DashboardComponent.exportPageSize;
    return this.studentService.getStudentsPage(1, pageSize, filters).pipe(
      switchMap((first) => {
        const totalCount = Number(first.totalCount ?? 0);
        if (totalCount === 0) {
          return of([] as StudentDetailsDTO[]);
        }
        const normalizedFirst = (first.data || []).map((row: unknown) =>
          this.studentService.normalizeStudentDetailsDto(row),
        );
        const totalPages = Math.max(
          1,
          Number(first.totalPages ?? Math.ceil(totalCount / pageSize)),
        );
        if (totalPages <= 1) {
          return of(normalizedFirst);
        }
        const rest$ = Array.from({ length: totalPages - 1 }, (_, i) =>
          this.studentService.getStudentsPage(i + 2, pageSize, filters),
        );
        return forkJoin(rest$).pipe(
          map((pages) => {
            const rest = pages.flatMap((p) =>
              (p.data || []).map((row: unknown) => this.studentService.normalizeStudentDetailsDto(row)),
            );
            return [...normalizedFirst, ...rest];
          }),
        );
      }),
    );
  }

  private exportFileStamp(): string {
    const d = new Date();
    const p = (n: number) => String(n).padStart(2, '0');
    return `${d.getFullYear()}-${p(d.getMonth() + 1)}-${p(d.getDate())}_${p(d.getHours())}${p(d.getMinutes())}`;
  }

  private exportRowValues(student: StudentDetailsDTO): string[] {
    const g = student.guardians;
    return [
      this.studentFullNameTooltip(student),
      student.studentPhone ?? '',
      g?.guardianFullName ?? '',
      g?.guardianPhone ?? '',
      student.placeBirth ?? '',
      this.formatHireDate(student.hireDate),
      student.fee != null ? String(student.fee) : '',
    ];
  }

  private formatHireDate(d: Date | string | undefined): string {
    if (!d) {
      return '';
    }
    return this.datePipe.transform(d, 'dd-MM-yyyy') ?? '';
  }

  /** Real Excel workbook (.xlsx) so the file opens in the spreadsheet grid with columns. */
  private async downloadStudentsXlsx(rows: StudentDetailsDTO[]): Promise<void> {
    const XLSX = await import('xlsx');
    const headers = [
      this.translate.instant('dashboard.exportColName'),
      this.translate.instant('dashboard.exportColStudentNo'),
      this.translate.instant('dashboard.exportColFather'),
      this.translate.instant('dashboard.exportColPhone'),
      this.translate.instant('dashboard.exportColAddress'),
      this.translate.instant('dashboard.exportColAdmission'),
      this.translate.instant('dashboard.exportColFee'),
    ];
    const aoa: (string | number)[][] = [
      headers,
      ...rows.map((st) => {
        const text = this.exportRowValues(st);
        const feeNum = st.fee != null && !Number.isNaN(Number(st.fee)) ? Number(st.fee) : '';
        return [...text.slice(0, 6), feeNum];
      }),
    ];
    const ws = XLSX.utils.aoa_to_sheet(aoa);
    ws['!cols'] = [{ wch: 28 }, { wch: 14 }, { wch: 22 }, { wch: 14 }, { wch: 18 }, { wch: 14 }, { wch: 12 }];
    const wb = XLSX.utils.book_new();
    const sheetName = this.translate.instant('dashboard.exportSheetName').slice(0, 31) || 'Students';
    XLSX.utils.book_append_sheet(wb, ws, sheetName);
    XLSX.writeFile(wb, `students-${this.exportFileStamp()}.xlsx`);
  }

  /** jsPDF + autotable (vector PDF). html2canvas/html2pdf was producing empty ~3KB files here. */
  private async downloadStudentsPdfAutoTable(rows: StudentDetailsDTO[]): Promise<void> {
    const { jsPDF } = await import('jspdf');
    const autoTable = (await import('jspdf-autotable')).default;

    const doc = new jsPDF({ orientation: 'landscape', unit: 'mm', format: 'a4' });
    const pageW = doc.internal.pageSize.getWidth();
    const isRtl = (this.translate.currentLang || '').toLowerCase().startsWith('ar');
    const fontFamily = isRtl ? await this.tryEmbedNotoSansArabic(doc) : 'helvetica';

    const title = this.translate.instant('dashboard.exportTitle');
    doc.setFont(fontFamily, 'normal');
    doc.setFontSize(12);
    doc.setTextColor(20, 20, 20);
    const titleX = isRtl ? pageW - 14 : 14;
    doc.text(title, titleX, 10, { align: isRtl ? 'right' : 'left', maxWidth: pageW - 28 });

    const headLtr = [
      this.translate.instant('dashboard.exportColName'),
      this.translate.instant('dashboard.exportColStudentNo'),
      this.translate.instant('dashboard.exportColFather'),
      this.translate.instant('dashboard.exportColPhone'),
      this.translate.instant('dashboard.exportColAddress'),
      this.translate.instant('dashboard.exportColAdmission'),
      this.translate.instant('dashboard.exportColFee'),
    ];
    /** LTR indices: 0 name, 1 student no, … 6 fee. For Arabic PDF, draw LTR as fee→…→father→name→studentNo so RTL reading has name beside student no on the right. */
    const headRtlPdf = [
      headLtr[6],
      headLtr[5],
      headLtr[4],
      headLtr[3],
      headLtr[2],
      headLtr[0],
      headLtr[1],
    ];
    const head = [isRtl ? headRtlPdf : headLtr];
    const body = rows.map((st) => {
      const cells = this.exportRowValues(st);
      if (!isRtl) {
        return cells;
      }
      return [cells[6], cells[5], cells[4], cells[3], cells[2], cells[0], cells[1]];
    });

    /** #009879 */
    const headerGreen: [number, number, number] = [0, 152, 121];
    /** Repeat “name” on horizontal page breaks: col 0 in LTR; in RTL PDF order, name is index 5 */
    const repeatCol = isRtl ? 5 : 0;

    autoTable(doc, {
      startY: 14,
      head,
      body,
      theme: 'grid',
      styles: {
        font: fontFamily,
        fontSize: 7,
        cellPadding: 1.5,
        halign: isRtl ? 'right' : 'left',
        valign: 'middle',
        textColor: [20, 20, 20],
      },
      headStyles: {
        font: fontFamily,
        fillColor: headerGreen,
        textColor: 255,
        fontStyle: fontFamily === 'helvetica' ? 'bold' : 'normal',
        halign: isRtl ? 'right' : 'left',
      },
      alternateRowStyles: { fillColor: [248, 248, 248] },
      margin: { top: 14, left: 10, right: 10, bottom: 12 },
      tableWidth: 'auto',
      horizontalPageBreak: true,
      showHead: 'everyPage',
      horizontalPageBreakRepeat: repeatCol,
    });

    doc.save(`students-${this.exportFileStamp()}.pdf`);
  }

  /** Returns jsPDF font name: embedded Arabic-capable font or helvetica fallback. */
  private async tryEmbedNotoSansArabic(doc: jsPDF): Promise<string> {
    const urls = [
      'https://cdn.jsdelivr.net/gh/google/fonts@main/ofl/notosansarabic/NotoSansArabic%5Bwdth%2Cwght%5D.ttf',
      'https://raw.githubusercontent.com/google/fonts/main/ofl/notosansarabic/NotoSansArabic%5Bwdth%2Cwght%5D.ttf',
    ];
    const vfsName = 'NotoSansArabic.ttf';
    const fontName = 'NotoSansArabic';
    for (const url of urls) {
      try {
        const res = await fetch(url, { mode: 'cors' });
        if (!res.ok) {
          continue;
        }
        const buf = await res.arrayBuffer();
        if (buf.byteLength < 10_000) {
          continue;
        }
        const b64 = this.arrayBufferToBase64(buf);
        doc.addFileToVFS(vfsName, b64);
        doc.addFont(vfsName, fontName, 'normal');
        return fontName;
      } catch {
        /* network, CORS, or unsupported font file */
      }
    }
    return 'helvetica';
  }

  private arrayBufferToBase64(buffer: ArrayBuffer): string {
    const bytes = new Uint8Array(buffer);
    const chunk = 0x8000;
    let binary = '';
    for (let i = 0; i < bytes.length; i += chunk) {
      binary += String.fromCharCode.apply(null, bytes.subarray(i, i + chunk) as unknown as number[]);
    }
    return btoa(binary);
  }

  private loadPaginatedStudents(): void {
    this.studentService.getStudentsPage(this.currentPage, this.pageSize, this.buildStudentTableFilters()).subscribe({
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
