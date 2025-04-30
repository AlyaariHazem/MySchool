import { Component, inject, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { MatDialog } from '@angular/material/dialog';
import { PaginatorState } from 'primeng/paginator';

import { LanguageService } from '../../../core/services/language.service';
import { GradeTypeService } from '../core/services/grade-type.service';
import { GradeType } from '../core/models/gradeType.model';

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
  gradeTypeServce=inject(GradeTypeService);

  gradeTypes:GradeType[]=[];
  paginatedGradeTypes: GradeType[] = []; 
  books: number[] = [1, 2, 3, 4, 5, 6, 7, 8, 9, 10];
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
    this.getAllGradeTypes();
    this.updatePaginatedData();
    this.languageService.currentLanguage();
  }
  getAllGradeTypes(): void {
    this.gradeTypeServce.getAllGradeType().subscribe({
      next: res=>{
        if(res.isSuccess){
          this.gradeTypes=res.result;
          this.updatePaginatedData();
        }else{
          console.log('Error fetching grade types:', res.errorMasseges[0]);
          this.gradeTypes=[];
        }
      }
    })
  }
  updatePaginatedData(): void {
    const start = this.first;
    const end = this.first + this.rows;
    this.paginatedGradeTypes = this.gradeTypes.slice(start, end);
  }

  onPageChange(event: PaginatorState): void {
    this.first = event.first || 0;
    this.rows = event.rows || 4;
    this.updatePaginatedData();
  }

  toggleIsActive(gradeType:GradeType) {
    gradeType.isActive=!gradeType.isActive;
  }

  showDialog() {
    console.log('the Book is added successfully!');
  }
}
