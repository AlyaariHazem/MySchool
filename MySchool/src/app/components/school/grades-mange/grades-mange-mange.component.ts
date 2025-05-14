import { Component, inject, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { PaginatorState } from 'primeng/paginator';

import { LanguageService } from '../../../core/services/language.service';
import { GradeTypeService } from '../core/services/grade-type.service';
import { GradeType } from '../core/models/gradeType.model';


@Component({
  selector: 'app-grades-mange',
  templateUrl: './grades-mange.component.html',
  styleUrls: ['./grades-mange.component.scss', '../../../shared/styles/style-table.scss']
})
export class GradesMangeComponent implements OnInit {
  form: FormGroup;
  search: any;
  gradeTypeServce=inject(GradeTypeService);

  gradeTypes:GradeType[]=[];
  paginatedGradeTypes: GradeType[] = []; 
  isActive: boolean = false;
  isLoading: boolean = true;

  // Paginator properties
  first: number = 0;
  rows: number = 4;

  languageService=inject(LanguageService);

  constructor(
    private formBuilder: FormBuilder,) {
    this.form = this.formBuilder.group({
      name: ['',Validators.required],
      maxGrade: [,Validators.required],
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
          this.isLoading=false;
        }else{
          console.log('Error fetching grade types:', res.errorMasseges[0]);
          this.gradeTypes=[];
          this.isLoading=false;
        }
      }
    })
  }
  edit(gradeType: GradeType): void {
    this.form.patchValue({
      name: gradeType.name,
      maxGrade: gradeType.maxGrade,
    });
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

  Add() {
    console.log('the Book is added successfully!');
    console.log('the data are',this.form.value);
    this.form.reset();
  }
}
