import { FormBuilder, FormControl, FormGroup, Validators } from '@angular/forms';
import { Component, inject, OnChanges, OnInit, SimpleChanges } from '@angular/core';
import { PaginatorState } from 'primeng/paginator';
import { MatDialog, MatDialogConfig } from '@angular/material/dialog';
import { ToastrService } from 'ngx-toastr';
import { ActivatedRoute } from '@angular/router';

import { NewStudentComponent } from './new-student/new-student.component';
import { StudentDetailsDTO } from '../../../core/models/students.model';
import { StudentService } from '../../../core/services/student.service';
import { TranslationService } from '../../../core/services/translation.service';
import { LanguageService } from '../../../core/services/language.service';

@Component({
  selector: 'app-students',
  templateUrl: './students.component.html',
  styleUrls: ['./students.component.scss'],
})
export class StudentsComponent implements OnInit,OnChanges {
  form: FormGroup;

  translationService=inject(TranslationService);
  studentService=inject(StudentService);
  languageService=inject(LanguageService);
  
  Students:StudentDetailsDTO[]=[]
  values = new FormControl<string[] | null>(null);
  paginatedStudents: StudentDetailsDTO[] = []; // Paginated data
  max = 2;

  first: number = 0; // Current starting index
  rows: number = 4; // Number of rows per page
  updatePaginatedData(): void {
    const start = this.first;
    const end = this.first + this.rows;
    this.paginatedStudents = this.Students.slice(start, end);
  }

  // Handle page change event from PrimeNG paginator
  onPageChange(event: PaginatorState): void {
    this.first = event.first || 0; // Default to 0 if undefined
    this.rows = event.rows || 4; // Default to 4 rows
    this.updatePaginatedData();
  }
  
  // Dropdown options
  studentOptions: string[] = ['طالب 1', 'طالب 2', 'طالب 3'];
  stageOptions: string[] = ['المرحلة الأولى', 'المرحلة الثانية', 'المرحلة الثالثة'];
  classOptions: string[] = ['الصف الأول', 'الصف الثاني', 'الصف الثالث'];

  // Selection states
  selectedStudent: string | null = null;
  selectedStage: string | null = null;
  selectedClass: string | null = null;

  isStudentSelected = false;
  isStageSelected = false;
  isClassSelected = false;

  showGrid:boolean=false;
  showCulomn:boolean=true;
  showStudentCulomn():void{
    this.showCulomn=true;
    this.showGrid=false;
  }
  showStudentGrid():void{
    this.showCulomn=false;
    this.showGrid=true;
  }
 
  constructor(
    private formBuilder: FormBuilder,
    private toastr: ToastrService,
    public dialog: MatDialog,
    private route: ActivatedRoute
  ) {
    this.form = this.formBuilder.group({
      stage: ['', Validators.required],
      gradeName: ['', Validators.required],
    });
  }
  ngOnChanges(changes: SimpleChanges): void {
    this.languageService.currentLanguage();
    this.translationService.changeLanguage(this.languageService.langDir);
  }

  id!:number;
  ngOnInit(): void {
    this.id = Number(this.route.snapshot.paramMap.get('id'));
    if(this.id){
      //this for add student 
      this.openDialog();
    }
    this.getAllStudents();
    this.languageService.currentLanguage();
    this.translationService.changeLanguage(this.languageService.langDir);
  }
getAllStudents():void{
  this.studentService.getAllStudents().subscribe((res)=>{
    this.Students=res;
    this.updatePaginatedData(); // Initial slicing
  })
}
getStudentByID(id:number):void{
  this.studentService.getStudentById(id).subscribe({
    next:(res)=>res,
  });
}

  // Track selection changes
  onSelectionChange(type: string, value: string): void {
    if (type === 'student') {
      this.selectedStudent = value;
      this.isStudentSelected = value !== null;
    } else if (type === 'stage') {
      this.selectedStage = value;
      this.isStageSelected = value !== null;
    } else if (type === 'class') {
      this.selectedClass = value;
      this.isClassSelected = value !== null;
    }
  }

  // Clear the selection
  clearSelection(type: string): void {
    if (type === 'student') {
      this.selectedStudent = null;
      this.isStudentSelected = false;
    } else if (type === 'stage') {
      this.selectedStage = null;
      this.isStageSelected = false;
    } else if (type === 'class') {
      this.selectedClass = null;
      this.isClassSelected = false;
    }
  }

  openDialog(): void {
    const dialogConfig = new MatDialogConfig();
    dialogConfig.width = '80%';
    dialogConfig.panelClass = 'custom-dialog-container';

    const dialogRef = this.dialog.open(NewStudentComponent, dialogConfig);

    dialogRef.afterClosed().subscribe((result) => {
      if (result) {
        this.toastr.success('تم إضافة الطالب بنجاح');
      }
    });
  }
 // students.component.ts
EditDialog(id: number): void {
  this.studentService.getStudentById(id).subscribe((res) => {
    console.log("Editing student data:", res);

    // Pass the student data and a 'mode' flag to the dialog
    const dialogConfig = new MatDialogConfig();
    dialogConfig.width = '80%';
    dialogConfig.panelClass = 'custom-dialog-container';

    // IMPORTANT: Pass data to the dialog using 'data' property
    dialogConfig.data = {
      mode: 'edit',
      student: res, // edit student
    };

    const dialogRef = this.dialog.open(NewStudentComponent, dialogConfig);

    dialogRef.afterClosed().subscribe((result) => {
      if (result) {
        this.toastr.success('تم تعديل الطالب بنجاح');
        this.getAllStudents(); // Refresh table or handle updates
      }
    });
  });
}

  deleteStudent(studentId: number): void {
    const confirmDelete = confirm('هل أنت متأكد من حذف هذا الطالب؟');
  if (confirmDelete) {
    // Filter out the student with the given ID
   this.studentService.DeleteStudent(studentId).subscribe(()=>{
    this.toastr.warning('تم حذف الطالب بنجاح');
    this.getAllStudents();
   })
  }
  }
  
}