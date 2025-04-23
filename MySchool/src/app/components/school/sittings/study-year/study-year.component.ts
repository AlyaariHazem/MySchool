import { Component, inject, OnInit } from '@angular/core';
import { FormControl } from '@angular/forms';
import { ToastrService } from 'ngx-toastr';
import { PaginatorState } from 'primeng/paginator';

import { LanguageService } from '../../../../core/services/language.service';
import { YearService } from '../../../../core/services/year.service';
import { Year } from '../../../../core/models/year.model';

@Component({
  selector: 'app-study-year',
  templateUrl: './study-year.component.html',
  styleUrls: [
    './study-year.component.scss',
    './../../../../shared/styles/style-input.scss'
  ]
})
export class StudyYearComponent implements OnInit {
  visible: boolean = false;
  values = new FormControl<string[] | null>(null);
  max = 2;
  years: Year[] = [];
  viewYear: Year[] = [];

  yearService = inject(YearService);
  languageService = inject(LanguageService);
  toaster = inject(ToastrService);

  currentPage: number = 0; // Current page index
  pageSize: number = 5; // Number of items per page

  ngOnInit(): void {
    this.getAllYears();
  }

  showDialogAddYear() {
    this.visible = true;
  }

  getAllYears() {
    this.yearService.getAllYears().subscribe(res => {
      this.years = res;
      this.viewYear = this.years;
      this.updatePaginatedData();
    });
  }

  deleteYear(id: number) {
    this.yearService.deleteYear(id).subscribe(res => {
      this.getAllYears();
      this.toaster.success('تم حذف العام الدراسي بنجاح');
    });
  }

 
  isEditMode: boolean = false;
  changeYear(year: Year, isActive: boolean): void {
    const patchDoc = [
      { op: "replace", path: "/active", value: isActive }
    ];

    this.yearService.partialUpdate(year.yearID, patchDoc).subscribe({
      next: (response) => {
        if (response) {
          this.toaster.success(response);
          this.getAllYears(); // Refresh the list to show updated data
        }
      },
      error: () => this.toaster.error('Failed to update year', 'Error')
    });

    this.isEditMode = false;
  }

  // Paginator properties
  first: number = 0;
  rows: number = 4;
  paginatedYears: Year[] = [];
  updatePaginatedData(): void {
    const start = this.first;
    const end = this.first + this.rows;
    this.paginatedYears = this.years.slice(start, end);
  }
  // Handle paginator events
  onPageChange(event: PaginatorState): void {
    this.first = event.first || 0;
    this.rows = event.rows || 4;
    this.updatePaginatedData();
  }

  // This method will be called when a new year is added
  handleYearAdded(newYear: Year) {
    // Optionally refresh the years list
    this.getAllYears();
    // Close the dialog
    this.visible = false;
  }
}
