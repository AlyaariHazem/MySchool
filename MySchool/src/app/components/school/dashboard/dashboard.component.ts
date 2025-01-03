import { Component, inject, OnInit } from '@angular/core';
import { PaginatorState } from 'primeng/paginator';
import { Store } from '@ngrx/store'; 

import { StudentDetailsDTO } from '../../../core/models/students.model';
import { StudentService } from '../../../core/services/student.service';

@Component({
  selector: 'app-dashboard',
  templateUrl: './dashboard.component.html',
  styleUrl: './dashboard.component.scss',
})
export class DashboardComponent implements OnInit {
  constructor() {}
  
  langDir!:string;
  languageStore=inject(Store);
  dir:string="ltr";
  currentLanguage():void{
    this.languageStore.select("language").subscribe((res)=>{
      this.langDir=res;
      console.log("the language is",this.langDir);
      this.dir=(res=="en")?"ltr":"rtl";
    });
  }

  students: StudentDetailsDTO[] = [];
  studentService = inject(StudentService);

  ngOnInit(): void {
    this.getAllStudent();
    this.currentLanguage();
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
