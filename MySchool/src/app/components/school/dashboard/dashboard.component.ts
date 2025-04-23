import { Component, inject, OnInit } from '@angular/core';
import { PaginatorState } from 'primeng/paginator';

import { StudentDetailsDTO } from '../../../core/models/students.model';
import { StudentService } from '../../../core/services/student.service';
import { LanguageService } from '../../../core/services/language.service';
import { TranslationService } from '../../../core/services/translation.service';

@Component({
  selector: 'app-dashboard',
  templateUrl: './dashboard.component.html',
  styleUrl: './dashboard.component.scss',
})
export class DashboardComponent implements OnInit {
  constructor() {}
  
  languageService=inject(LanguageService);
  translationService=inject(TranslationService);
  
  students: StudentDetailsDTO[] = [];
  studentService = inject(StudentService);
  isLoading: boolean = true; // Loading state for the component

  ngOnInit(): void {
    this.getAllStudent();
    this.languageService.currentLanguage();
    this.translationService.changeLanguage(this.languageService.langDir);
  }

  getAllStudent(): void {
    this.studentService.getAllStudents().subscribe(res => {
      this.students = res;
      this.isLoading=false;
    });
  }

  first: number = 0;
  rows: number = 4;
  onPageChange(event: PaginatorState) {
    this.first = event.first || 0; // Default to 0 if undefined
    this.rows = event.rows!;
  }

}
