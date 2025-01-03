import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormControl, FormGroup, Validators } from '@angular/forms';
import { MatDialog } from '@angular/material/dialog';
import { PaginatorState } from 'primeng/paginator';

interface City {
  name: string;
  code: string;
}

interface Student {
  id: number;
  name: string;
  age: number;
  grade: string;
  gender: string;
  book: string; // e.g., a field for "المقرر"
}

@Component({
  selector: 'app-grades-month',
  templateUrl: './grades-month.component.html',
  styleUrls: [
    './grades-month.component.scss',
    './../../../../shared/styles/style-select.scss',
    './../../../../shared/styles/style-table.scss'
  ]
})
export class GradesMonthComponent implements OnInit {
  form!: FormGroup;
  Books: City[] = [];
  classes: City[] = [];
  values = new FormControl<string[] | null>(null);
  max = 2;

  SelectBook = false;
  SelectClass = false;

  studentsData: Student[] = [
    {
      id: 1232,
      name: 'حازم عبدالله اليعري',
      age: 12,
      grade: 'A',
      gender: 'male',
      book: 'قران'
    },
    {
      id: 221,
      name: 'محمد علي',
      age: 11,
      grade: 'B',
      gender: 'male',
      book: 'رياضيات'
    },
    {
      id: 4321,
      name: 'سارة خالد',
      age: 12,
      grade: 'A',
      gender: 'female',
      book: 'تاريخ'
    },
    {
      id: 221,
      name: 'محمد علي',
      age: 11,
      grade: 'B',
      gender: 'male',
      book: 'رياضيات'
    },
    {
      id: 4321,
      name: 'سارة خالد',
      age: 12,
      grade: 'A',
      gender: 'female',
      book: 'تاريخ'
    },
    {
      id: 221,
      name: 'محمد علي',
      age: 11,
      grade: 'B',
      gender: 'male',
      book: 'رياضيات'
    },
    {
      id: 4321,
      name: 'سارة خالد',
      age: 12,
      grade: 'A',
      gender: 'female',
      book: 'تاريخ'
    },
    {
      id: 221,
      name: 'محمد علي',
      age: 11,
      grade: 'B',
      gender: 'male',
      book: 'رياضيات'
    },
    {
      id: 4321,
      name: 'سارة خالد',
      age: 12,
      grade: 'A',
      gender: 'female',
      book: 'تاريخ'
    },
    {
      id: 221,
      name: 'محمد علي',
      age: 11,
      grade: 'B',
      gender: 'male',
      book: 'رياضيات'
    },
    {
      id: 4321,
      name: 'سارة خالد',
      age: 12,
      grade: 'A',
      gender: 'female',
      book: 'تاريخ'
    },
    {
      id: 221,
      name: 'محمد علي',
      age: 11,
      grade: 'B',
      gender: 'male',
      book: 'رياضيات'
    },
    {
      id: 4321,
      name: 'سارة خالد',
      age: 12,
      grade: 'A',
      gender: 'female',
      book: 'تاريخ'
    }
  ];

  currentStudentIndex = 0;
  CurrentStudent!: Student; // Will be assigned in ngOnInit()

  students = [1, 2, 3, 4, 5, 6, 7, 8, 9, 10];
  displayedStudents: number[] = [];
  currentPage = 0;
  pageSize = 5;
  length = 0;

  constructor(
    private formBuilder: FormBuilder,
    public dialog: MatDialog
  ) { }

  ngOnInit(): void {
    // Initialize form
    this.form = this.formBuilder.group({
      BookID: [null, Validators.required],
      ClassID: [null, Validators.required]
    });

    // Example data for dropdowns
    this.Books = [
      { name: 'Math', code: 'MATH' },
      { name: 'Science', code: 'SCI' },
      { name: 'History', code: 'HIST' },
      { name: 'Geography', code: 'GEO' },
      { name: 'English', code: 'ENG' },
    ];
    this.classes = [
      { name: 'Grade 1', code: 'G1' },
      { name: 'Grade 2', code: 'G2' },
      { name: 'Grade 3', code: 'G3' },
      { name: 'Grade 4', code: 'G4' },
      { name: 'Grade 5', code: 'G5' },
    ];
    if (this.studentsData.length > 0) {
      this.CurrentStudent = this.studentsData[this.currentStudentIndex];
    }
    this.updatePaginatedData();
  }
  goNextStudent(): void {
    if (this.currentStudentIndex < this.studentsData.length - 1) {
      this.currentStudentIndex++;
      this.CurrentStudent = this.studentsData[this.currentStudentIndex];
    }
  }

  goPreviousStudent(): void {
    if (this.currentStudentIndex > 0) {
      this.currentStudentIndex--;
      this.CurrentStudent = this.studentsData[this.currentStudentIndex];
    }
  }

  // Event Handlers
  selectBook(): void {
    this.SelectBook = true;
  }

  selectClass(): void {
    this.SelectClass = true;
  }

  showDialog(): void {
    console.log('the Book is added successfully!');
  }

  updateDisplayedStudents(): void {
    const startIndex = this.currentPage * this.pageSize;
    const endIndex = startIndex + this.pageSize;
    this.displayedStudents = this.students.slice(startIndex, endIndex);
  }
  trackByIndex(index: number, item: any): number {
    return index;
  }

  hidden: boolean = false;
  toggleHidden() {
    this.hidden = !this.hidden;
  }
  paginatedStudents: Student[] = [];

  first: number = 0;
  rows: number = 4;
  updatePaginatedData(): void {
    const start = this.first;
    const end = this.first + this.rows;
    this.paginatedStudents = this.studentsData.slice(start, end);
  }

  // Handle page change event from PrimeNG paginator
  onPageChange(event: PaginatorState): void {
    this.first = event.first || 0; // Default to 0 if undefined
    this.rows = event.rows || 4; // Default to 4 rows
    this.updatePaginatedData();
  }
}
