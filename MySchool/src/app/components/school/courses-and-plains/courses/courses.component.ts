import { Component } from '@angular/core';
import { FormBuilder, FormControl, FormGroup, Validators } from '@angular/forms';
import { MatDialog } from '@angular/material/dialog';
import { PageEvent } from '@angular/material/paginator';
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
export class CoursesComponent {

  showDialog() {
    console.log('the Book is added successfully!');
  }
  form: FormGroup;
  Books: City[] | undefined;
  classes: City[] | undefined;

  values = new FormControl<string[] | null>(null);
  max = 2;
  SelectBook:boolean=false;
  SelectClass:boolean=false;
  selectedCity: City | undefined;
  selectedBook:City | undefined;

  selectBook():void{
    this.SelectBook=true;
  }
  selectClass():void{
    this.SelectClass=true;
  }
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
    this.length = this.students.length;
    this.updateDisplayedStudents(); 
    this.form = this.formBuilder.group({
      BookID: [null, Validators.required],
      ClassID: [null, Validators.required],
    });
    
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
