import { Component, inject, OnInit } from '@angular/core';
import { PaginatorState } from 'primeng/paginator';

import { StudentDetailsDTO } from '../../../core/models/students.model';
import { StudentService } from '../../../core/services/student.service';
import { Store } from '@ngrx/store';
import { selectLanguage } from '../../../core/store/language/language.selectors';
import { map } from 'rxjs';

@Component({
  selector: 'app-dashboard',
  templateUrl: './dashboard.component.html',
  styleUrl: './dashboard.component.scss',
})
export class DashboardComponent implements OnInit {
  constructor(private store: Store) {}
  readonly dir$ = this.store.select(selectLanguage).pipe(
      map(l => (l === 'ar' ? 'rtl' : 'ltr')),
    );
  
  students: StudentDetailsDTO[] = [];
  studentService = inject(StudentService);

  ngOnInit(): void {
    this.getAllStudent();
  }

  getAllStudent(): void {
    this.studentService.getAllStudents().subscribe(res => this.students = res);
  }

  first: number = 0;
  rows: number = 4;
  onPageChange(event: PaginatorState) {
    this.first = event.first || 0; // Default to 0 if undefined
    this.rows = event.rows!;
  }

}
