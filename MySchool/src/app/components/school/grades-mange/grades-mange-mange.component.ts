import { Component, inject, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { MatDialog } from '@angular/material/dialog';
import { PaginatorState } from 'primeng/paginator';
// Import html2canvas and jsPDF
import jsPDF from 'jspdf';
import html2canvas from 'html2canvas';

import { LanguageService } from '../../../core/services/language.service';

interface City {
  name: string;
  code: string;
}

@Component({
  selector: 'app-grades-mange',
  templateUrl: './grades-mange.component.html',
  styleUrls: ['./grades-mange.component.scss', '../../../shared/styles/style-table.scss']
})
export class GradesMangeComponent implements OnInit {
  form: FormGroup;
  cities: City[] | undefined;
  search: any;

  books: number[] = [1, 2, 3, 4, 5, 6, 7, 8, 9, 10];
  paginatedBooks: number[] = [];
  isActive: boolean = false;

  // Paginator properties
  first: number = 0;
  rows: number = 4;

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
    this.languageService.currentLanguage();
  }

  updatePaginatedData(): void {
    const start = this.first;
    const end = this.first + this.rows;
    this.paginatedBooks = this.books.slice(start, end);
  }

  onPageChange(event: PaginatorState): void {
    this.first = event.first || 0;
    this.rows = event.rows || 4;
    this.updatePaginatedData();
  }

  toggleIsActive() {
    this.isActive = !this.isActive;
  }

  showDialog() {
    console.log('the Book is added successfully!');
  }

  /**
   * Captures the rendered HTML table (with styles) and saves it as a PDF.
   */
  captureAsPDF(): void {
    const tableElement = document.getElementById('printableTable') as HTMLElement;

    html2canvas(tableElement).then((canvas) => {
      const imgData = canvas.toDataURL('image/png');

      // Create jsPDF instance: 'p' = portrait, 'pt' = points, 'a4' = page size
      const pdf = new jsPDF('p', 'pt', 'a4');

      // Calculate image dimensions to fit A4 page width
      const pageWidth = pdf.internal.pageSize.getWidth();
      // const pageHeight = pdf.internal.pageSize.getHeight();
      const canvasWidth = canvas.width;
      const canvasHeight = canvas.height;

      // Scale image height proportionally to fit the page width
      const ratio = canvasHeight / canvasWidth;
      const imgWidth = pageWidth;
      const imgHeight = pageWidth * ratio;

      // Add image to PDF (top-left corner at 0,0)
      pdf.addImage(imgData, 'PNG', 0, 0, imgWidth, imgHeight);

      // If the table is very long, you'll need more logic for multi-page output.
      // For a shorter table that fits on one page, this is sufficient.

      pdf.save('grades-table.pdf');
    });
  }
}
