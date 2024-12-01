import { Component } from '@angular/core';
import { MediaChange, MediaObserver } from '@angular/flex-layout';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { MatDialog } from '@angular/material/dialog';
import { ToastrService } from 'ngx-toastr';
import { Subscription } from 'rxjs';
interface City {
  name: string;
  code: string;
}

@Component({
  selector: 'app-fees',
  templateUrl: './fees.component.html',
  styleUrls: ['./fees.component.scss','./../../../shared/styles/style-select.scss']
})
export class FeesComponent {
  visible: boolean = false;

  showDialog() {
      this.visible = true;
  }
  form: FormGroup;
  cities: City[] | undefined;

  selectedCity: City | undefined;

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
  
  isSmallScreen = false;
  private mediaSub: Subscription | null = null;

  constructor(
    private formBuilder: FormBuilder,
    private toastr: ToastrService,
    public dialog: MatDialog,
    private mediaObserver: MediaObserver
  ) {
    this.form = this.formBuilder.group({
      stage: ['', Validators.required],
      gradeName: ['', Validators.required],
    });
  }

  ngOnInit(): void {
    this.mediaSub = this.mediaObserver.asObservable().subscribe((changes: MediaChange[]) => {
      this.isSmallScreen = changes.some(
        (change) => change.mqAlias === 'xs' || change.mqAlias === 'sm'
      );
    });
    this.cities = [
      { name: 'New Yorkaaaaaaaaaaa', code: 'NY' },
      { name: 'Rome', code: 'RM' },
      { name: 'London', code: 'LDN' },
      { name: 'Istanbul', code: 'IST' },
      { name: 'Paris', code: 'PRS' }
  ];
  }

  ngOnDestroy(): void {
    if (this.mediaSub) {
      this.mediaSub.unsubscribe();
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


  deleteStudent(studentId: number): void {
    this.students = this.students.filter((student) => student.id !== studentId);
    this.toastr.warning('تم حذف الطالب');
  }
}
