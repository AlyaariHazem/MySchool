// study-year.component.ts
import { Component, OnInit, inject } from '@angular/core';
import { Store } from '@ngrx/store';
import { map } from 'rxjs';
import { ToastrService } from 'ngx-toastr';
import { PaginatorState } from 'primeng/paginator';

import { YearService } from '../../../../core/services/year.service';
import { Year } from '../../../../core/models/year.model';
import { selectLanguage } from '../../../../core/store/language/language.selectors';
import { PaginatorService } from '../../../../core/services/paginator.service';
import { TableColumn } from '../../../../shared/components/custom-table/custom-table.component';

@Component({
  selector   : 'app-study-year',
  templateUrl: './study-year.component.html',
  styleUrls  : [
    './study-year.component.scss',
    './../../../../shared/styles/style-input.scss',
  ],
})
export class StudyYearComponent implements OnInit {
  
  readonly dir$ = this.store.select(selectLanguage).pipe(
    map(l => (l === 'ar' ? 'rtl' : 'ltr')),
  );

  yearService = inject(YearService);
  paginatorService = inject(PaginatorService);

  visible = false;
  visibleEdit = false;
  selectedYear: Year | null = null;

  showDialogAddYear() {
    this.visible = true;
  }

  showDialogEditYear(year: Year) {
    // Create a copy of the year object to avoid reference issues
    this.selectedYear = { ...year };
    this.visibleEdit = true;
  }

  /* ---------- data ---------- */
  years: Year[] = [];
  paginatedYears: Year[] = [];
  isLoading = true;
  filtersActive: boolean = false;
  currentFilters: Record<string, string> = {};

  /* ---------- pagination ---------- */
  totalRecords: number = 0;
  currentPage: number = 1;
  pageSize: number = 8;

  // Table columns configuration
  tableColumns: TableColumn[] = [
    { field: 'yearID', header: '#', sortable: true, filterable: true },
    { field: 'yearDateStart', header: 'تاريخ بدء الدراسة', sortable: true, filterable: true, template: 'date' },
    { field: 'yearDateEnd', header: 'تاريخ إنتهاء الدراسة', sortable: true, filterable: true, template: 'date' },
    { field: 'hireDate', header: 'تاريخ الإنشاء', sortable: true, filterable: true, template: 'date' },
    { 
      field: 'active', 
      header: 'الحالة', 
      sortable: true, 
      filterable: true,
      template: 'custom',
      formatter: (value: any) => {
        return value ? 'نشط' : 'خامل';
      }
    },
  ];

  constructor(
    private store      : Store,
    private toaster    : ToastrService,
  ) {}

  ngOnInit() {
    // Initialize pagination
    this.currentPage = 1;
    this.pageSize = 8;
    this.paginatorService.first.set(0);
    this.paginatorService.rows.set(8);
    this.getAllYears();
  }

  getAllYears() {
    this.isLoading = true;
    this.yearService.getYearsPage(this.currentPage, this.pageSize, this.currentFilters).subscribe({
      next: (res) => {
        // Ensure data is an array
        const years = Array.isArray(res.data) ? res.data : [];
        this.totalRecords = res.totalCount || 0;
        this.currentPage = res.pageNumber || this.currentPage;
        this.pageSize = res.pageSize || this.pageSize;
        
        // Create new array references to avoid object reference issues
        this.years = years.map((year: Year) => ({ ...year }));
        this.paginatedYears = years.map((year: Year) => ({ ...year }));
        
        // Update paginator service for UI consistency
        this.paginatorService.first.set((this.currentPage - 1) * this.pageSize);
        this.paginatorService.rows.set(this.pageSize);
        this.isLoading = false;
      },
      error: (err) => {
        console.error("Error fetching years:", err);
        this.toaster.error('فشل في تحميل بيانات السنوات الدراسية. تأكد من تشغيل الخادم.', 'خطأ');
        this.paginatedYears = [];
        this.years = [];
        this.totalRecords = 0;
        this.isLoading = false;
      }
    });
  }

  handlePageChange(event: PaginatorState): void {
    // Update paginator service state first
    this.paginatorService.first.set(event.first || 0);
    this.paginatorService.rows.set(event.rows || this.pageSize);
    
    // Calculate current page and page size
    const newPageSize = event.rows || this.pageSize;
    const newPage = Math.floor((event.first || 0) / newPageSize) + 1;
    
    // Fetch new page from server
    this.currentPage = newPage;
    this.pageSize = newPageSize;
    this.getAllYears();
  }

  onFilterChange(filters: Record<string, string>): void {
    // Update filters and reset to first page
    this.currentFilters = filters;
    this.currentPage = 1;
    this.paginatorService.first.set(0);
    this.filtersActive = Object.keys(filters).length > 0;
    this.getAllYears();
  }

  /* ---------- CRUD ---------- */
  deleteYear(year: any): void {
    const yearId = year.yearID || year;
    this.yearService.deleteYear(yearId).subscribe({
      next: () => {
        this.getAllYears();
        this.toaster.success('تم حذف العام الدراسي بنجاح');
      },
      error: () => {
        this.toaster.error('فشل في حذف العام الدراسي', 'خطأ');
      }
    });
  }

  editYear(year: Year): void {
    // Create a copy to ensure fresh data is passed
    const yearCopy: Year = {
      yearID: year.yearID,
      yearDateStart: year.yearDateStart,
      yearDateEnd: year.yearDateEnd,
      hireDate: year.hireDate,
      active: year.active,
      schoolID: year.schoolID
    };
    this.showDialogEditYear(yearCopy);
  }

  changeYearStatus(year: Year, isActive: boolean): void {
    const patch = [{ op: 'replace', path: '/active', value: isActive }];
    this.yearService.partialUpdate(year.yearID, patch).subscribe({
      next: (msg) => {
        this.toaster.success(msg);
        this.getAllYears();
      },
      error: () => this.toaster.error('فشل في تحديث حالة العام الدراسي', 'خطأ'),
    });
  }

  changeYear(year: Year, isActive: boolean) {
    this.changeYearStatus(year, isActive);
  }

  /* emitted from <app-new-year> */
  handleYearAdded() {
    this.getAllYears();
    this.visible = false;
  }

  /* emitted from <app-new-year> when editing */
  handleYearUpdated() {
    this.getAllYears();
    this.visibleEdit = false;
    // selectedYear will be cleared by dialog's onHide event
  }
}
