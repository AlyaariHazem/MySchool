import { Component, inject, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { MatDialog } from '@angular/material/dialog';
import { PaginatorState } from 'primeng/paginator';
import { LanguageService } from '../../../../core/services/language.service';
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
export class BooksComponent implements OnInit {

  showDialog() {
    console.log('the Book is added successfully!');
  }
  form: FormGroup;
  cities: City[] | undefined;
  search:any;
  books:number[]=[1,2,3,4,5,6,7,8,9,10];
  
  selectedCity: City | undefined;
  paginatedBooks: number[] = []; // Students for the current page

  isSmallScreen = false;

 langDir!:string;
 languageService=inject(LanguageService);

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
    this.updatePaginatedData();
    this.cities = [
      { name: 'New Yorkaaaaaaaaaaa', code: 'NY' },
      { name: 'Rome', code: 'RM' },
      { name: 'London', code: 'LDN' },
      { name: 'Istanbul', code: 'IST' },
      { name: 'Paris', code: 'PRS' }
    ];
    this.languageService.currentLanguage();
  }
 
  first: number = 0; // Current starting index
  rows: number = 4; // Number of rows per page
  updatePaginatedData(): void {
    const start = this.first;
    const end = this.first + this.rows;
    this.paginatedBooks = this.books.slice(start, end);
  }

  // Handle page change event from PrimeNG paginator
  onPageChange(event: PaginatorState): void {
    this.first = event.first || 0; // Default to 0 if undefined
    this.rows = event.rows || 4; // Default to 4 rows
    this.updatePaginatedData();
  }
}
