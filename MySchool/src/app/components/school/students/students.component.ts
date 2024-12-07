import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormControl, FormGroup, Validators } from '@angular/forms';
import { MatDialog, MatDialogConfig } from '@angular/material/dialog';
import { ToastrService } from 'ngx-toastr';
import { NewStudentComponent } from './new-student/new-student.component';
import { ActivatedRoute } from '@angular/router';

@Component({
  selector: 'app-students',
  templateUrl: './students.component.html',
  styleUrls: ['./students.component.scss'],
})
export class StudentsComponent implements OnInit {
  form: FormGroup;
  values = new FormControl<string[] | null>(null);
  max = 2;
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

  showGrid:boolean=true;
  showCulomn:boolean=false;
  showStudentCulomn():void{
    this.showCulomn=true;
    this.showGrid=false;
  }
  showStudentGrid():void{
    this.showCulomn=false;
    this.showGrid=true;
  }
  students = [
    {
      id: 1,
      name: 'أحمد علي',
      stage: 'المرحلة الأولى',
      class: 'الصف الأول',
      division: 'الشعبة أ',
      age: 10,
      gender: 'ذكر',
      registrationDate: '2024-11-01',
    },
    // Add more student data as needed
  ];

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

  id!:number;
  ngOnInit(): void {
    this.id = Number(this.route.snapshot.paramMap.get('id'));
    if(this.id){
      //this for add student 
      this.openDialog();
    }
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

  deleteStudent(studentId: number): void {
    const confirmDelete = confirm('هل أنت متأكد من حذف هذا الطالب؟');
  if (confirmDelete) {
    // Filter out the student with the given ID
    this.students = this.students.filter((student) => student.id !== studentId);

    // Show success notification
    this.toastr.warning('تم حذف الطالب بنجاح');
  }
  }
  cards:number[]=[1,2,3,4,5,6,7,8,9];
  
}
