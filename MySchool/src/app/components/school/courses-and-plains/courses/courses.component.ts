import { Component, inject, OnInit } from '@angular/core';
import { FormBuilder, FormControl, FormGroup, Validators } from '@angular/forms';
import { MatDialog } from '@angular/material/dialog';
import { PageEvent } from '@angular/material/paginator';
import { LanguageService } from '../../../../core/services/language.service';
import { SubjectService } from '../../core/services/subject.service';
import { ClassService } from '../../core/services/class.service';
import { Subjects } from '../../core/models/subjects.model';
import { Class } from '../../core/models/stages-grades.modul';
interface City {
  name: string;
  code: string;
}
@Component({
  selector: 'app-courses',
  templateUrl: './courses.component.html',
  styleUrls: ['./courses.component.scss',
              './../../../../shared/styles/style-table.scss',
              './../../../../shared/styles/style-select.scss'
            ]
})
export class CoursesComponent implements OnInit {
  form: FormGroup;

  languageService=inject(LanguageService);
  subjectService=inject(SubjectService);
  ClassService=inject(ClassService);

  subjects: Subjects[] =[];
  classes: Class[] = [];

  values = new FormControl<string[] | null>(null);
  max = 2;
  selectedCity: City | undefined;
  selectedBook:City | undefined;

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
  showDialog() {
    console.log('the Book is added successfully!');
  }
  ngOnInit(): void {
    this.subjectService.getAllSubjects().subscribe(res => this.subjects = res);
    this.ClassService.GetAll().subscribe(res => this.classes = res);
    this.length = this.students.length;
    this.updateDisplayedStudents(); 
    this.form = this.formBuilder.group({
      subjectID: [0, Validators.required],
      curriculumName: [''],
      classID: [0, Validators.required],
      not: ['', Validators.required],
    });
    
    this.languageService.currentLanguage();
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
