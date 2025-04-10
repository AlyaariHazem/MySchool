import { Component, inject, OnInit } from '@angular/core';
import { FormControl } from '@angular/forms';
import { PageEvent } from '@angular/material/paginator';
import { LanguageService } from '../../../../core/services/language.service';
import { YearService } from '../../../../core/services/year.service';
import { Year } from '../../../../core/models/year.model';
import { ToastrService } from 'ngx-toastr';

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
    this.updateDisplayedStudents();
  }

  showDialogAddYear() {
    this.visible = true;
  }

  getAllYears() {
    this.yearService.getAllYears().subscribe(res => {
      this.years = res;
      this.viewYear = this.years;
      this.updateDisplayedStudents();
    });
  }

  deleteYear(id: number) {
    this.yearService.deleteYear(id).subscribe(res => {
      this.getAllYears();
      this.toaster.success('تم حذف العام الدراسي بنجاح');
    });
  }

  updateDisplayedStudents(): void {
    const startIndex = this.currentPage * this.pageSize;
    const endIndex = startIndex + this.pageSize;
    this.viewYear = this.years.slice(startIndex, endIndex);
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

  // Handle paginator events
  onPageChange(event: PageEvent): void {
    this.currentPage = event.pageIndex;
    this.pageSize = event.pageSize;
    this.updateDisplayedStudents();
  }

  // This method will be called when a new year is added
  handleYearAdded(newYear: Year) {
    // Optionally refresh the years list
    this.getAllYears();
    // Close the dialog
    this.visible = false;
  }
}
