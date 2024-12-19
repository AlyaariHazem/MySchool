import { Component } from '@angular/core';
import { FormBuilder, FormControl, FormGroup, Validators } from '@angular/forms';
import { MatDialog } from '@angular/material/dialog';
import { PageEvent } from '@angular/material/paginator';
interface City {
  name: string;
  code: string;
}

@Component({
  selector: 'app-books',
  templateUrl: './books.component.html',
  styleUrls: ['./books.component.scss',
              './../../../../shared/styles/style-table.scss'
  ]
})
export class BooksComponent {

  showDialog() {
    console.log('the Book is added successfully!');
  }
  form: FormGroup;
  cities: City[] | undefined;

  values = new FormControl<string[] | null>(null);
  max = 2;

  selectedCity: City | undefined;

  students = [1, 2, 3, 4, 5, 6, 7, 8, 9, 10,];
  displayedStudents: number[] = []; // Students for the current page

  isSmallScreen = false;

  constructor(
    private formBuilder: FormBuilder,
    public dialog: MatDialog
  ) {
    this.form = this.formBuilder.group({
      BookID: ['', Validators.required],
      ClassID: ['', Validators.required],
    });
  }

  ngOnInit(): void {
    this.length = this.students.length; // Set the total number of items
    this.updateDisplayedStudents(); // Initialize the displayed students
    this.cities = [
      { name: 'New Yorkaaaaaaaaaaa', code: 'NY' },
      { name: 'Rome', code: 'RM' },
      { name: 'London', code: 'LDN' },
      { name: 'Istanbul', code: 'IST' },
      { name: 'Paris', code: 'PRS' }
    ];
  }
  currentPage: number = 0; // Current page index
  pageSize: number = 5; // Number of items per page
  length: number = 0; // Total number of items
  updateDisplayedStudents(): void {
    const startIndex = this.currentPage * this.pageSize;
    const endIndex = startIndex + this.pageSize;
    this.displayedStudents = this.students.slice(startIndex, endIndex);
  }
  // Handle paginator events
  onPageChange(event: PageEvent): void {
    this.currentPage = event.pageIndex;
    this.pageSize = event.pageSize;
    this.updateDisplayedStudents();
  }

}
