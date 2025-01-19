import { Component, inject, OnInit } from '@angular/core';
import { FormControl } from '@angular/forms';
import { PageEvent } from '@angular/material/paginator';
import { LanguageService } from '../../../../core/services/language.service';

@Component({
  selector: 'app-study-year',
  templateUrl: './study-year.component.html',
  styleUrls: ['./study-year.component.scss',
    './../../../../shared/styles/style-input.scss'
  ]
})
export class StudyYearComponent implements OnInit {
  constructor() { }

  visible: boolean = false;
  showDialog() {
    this.visible = true;
  }
  values = new FormControl<string[] | null>(null);
  max = 2;

  years: string[] = ['2023-2024', '2024-2025', '2025-2026', '2026-2027', '2027-2028', '2028-2029', '2029-2030', '2030-2031'];
  viewYear: string[] = [];

  languageService=inject(LanguageService);

  ngOnInit(): void {
    this.viewYear = this.years;
    this.updateDisplayedStudents();
    this.languageService.currentLanguage();
  }

  currentPage: number = 0; // Current page index
  pageSize: number = 5; // Number of items per page
  length: number = 0; // Total number of items

  updateDisplayedStudents(): void {
    const startIndex = this.currentPage * this.pageSize;
    const endIndex = startIndex + this.pageSize;
    this.viewYear = this.years.slice(startIndex, endIndex);
  }
  // Handle paginator events
  onPageChange(event: PageEvent): void {
    this.currentPage = event.pageIndex;
    this.pageSize = event.pageSize;
    this.updateDisplayedStudents();
  }
}
