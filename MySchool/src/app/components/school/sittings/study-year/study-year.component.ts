// study-year.component.ts
import { Component, OnInit } from '@angular/core';
import { Store } from '@ngrx/store';
import { map } from 'rxjs';
import { ToastrService } from 'ngx-toastr';
import { PaginatorState } from 'primeng/paginator';

import { YearService } from '../../../../core/services/year.service';
import { Year } from '../../../../core/models/year.model';
import { selectLanguage } from '../../../../core/store/language/language.selectors';

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

  visible = false;
  visibleEdit = false;
  selectedYear: Year | null = null;

  showDialogAddYear() {
    this.visible = true;
  }

  showDialogEditYear(year: Year) {
    this.selectedYear = year;
    this.visibleEdit = true;
  }

  /* ---------- data ---------- */
  years: Year[] = [];
  paginatedYears: Year[] = [];
  isLoading = true;

  /* ---------- paginator ---------- */
  first = 0;
  rows  = 4;
  onPageChange({ first = 0, rows = 4 }: PaginatorState) {
    this.first = first;
    this.rows  = rows;
    this.updatePaginatedData();
  }

  constructor(
    private store      : Store,
    private yearService: YearService,
    private toaster    : ToastrService,
  ) {}

  ngOnInit() {
    this.getAllYears();
  }

  getAllYears() {
    this.yearService.getAllYears().subscribe(res => {
      this.years     = res;
      this.isLoading = false;
      this.updatePaginatedData();
    });
  }

  /* pagination helper */
  private updatePaginatedData() {
    this.paginatedYears = this.years.slice(this.first, this.first + this.rows);
  }

  /* ---------- CRUD ---------- */
  deleteYear(id: number) {
    this.yearService.deleteYear(id).subscribe(() => {
      this.getAllYears();
      this.toaster.success('تم حذف العام الدراسي بنجاح');
    });
  }

  changeYear(year: Year, isActive: boolean) {
    const patch = [{ op: 'replace', path: '/active', value: isActive }];
    this.yearService.partialUpdate(year.yearID, patch).subscribe({
      next : msg => {
        this.toaster.success(msg);
        this.getAllYears();
      },
      error: () => this.toaster.error('Failed to update year', 'Error'),
    });
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
    this.selectedYear = null;
  }
}
