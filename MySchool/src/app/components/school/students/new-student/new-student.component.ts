// new-student.component.ts (Parent Component)
import { Component, AfterViewInit, OnInit, Inject, ChangeDetectorRef, SimpleChanges, OnChanges, inject, EventEmitter } from '@angular/core';
import { FormArray, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { MatDialogRef, MAT_DIALOG_DATA } from '@angular/material/dialog';
import { Observable } from 'rxjs';

import { AddStudent } from '../../../../core/models/students.model';
import { Discount, FeeClasses } from '../../../../core/models/Fee.model';
import { StudentService } from '../../../../core/services/student.service';
import { ToastrService } from 'ngx-toastr';

@Component({
  selector: 'app-new-student',
  templateUrl: './new-student.component.html',
  styleUrls: ['./new-student.component.scss']
})
export class NewStudentComponent implements OnInit, AfterViewInit, OnChanges {
  activeTab: string = 'DataStudent'; // Default active tab
  formGroup: FormGroup;
  toastr = inject(ToastrService)
  combinedData$: Observable<any[]> | undefined;
  currentPage: { [key: string]: number } = {};
  studentService = inject(StudentService);
  studentID: number = 0; // Initialize with a default placeholder value
  files!: File[];

  constructor(
    private formBuilder: FormBuilder,
    private changeDetectorRef: ChangeDetectorRef,
    public dialogRef: MatDialogRef<NewStudentComponent>, // Inject MatDialogRef
    @Inject(MAT_DIALOG_DATA) public data: any
  ) {
    // Create the main form

    this.formGroup = this.formBuilder.group({
      studentID: [this.studentID],
      existingGuardianId: [null],
      primaryData: this.formBuilder.group({
        studentFirstName: ['', Validators.required],
        studentMiddleName: ['', Validators.required],
        studentLastName: ['', Validators.required],
        studentFirstNameEng: [''],
        studentMiddleNameEng: [''],
        studentLastNameEng: [''],
        studentGender: ['Male', Validators.required],
        studentDOB: ['', Validators.required],
        studentPassword: ['Student'],
        classID: [null, Validators.required],
        amount: [0, Validators.required],
        divisionID: [null, Validators.required],
        studentAddress: [''],
      }),
      optionData: this.formBuilder.group({
        placeBirth: [''],
        studentPhone: [776137120],
        studentAddress: [''],
      }),
      guardian: this.formBuilder.group({
        guardianFullName: [''],
        guardianType: [''],
        guardianEmail: [''],
        guardianPassword: ['Guardian'],
        guardianPhone: [''],
        guardianGender: ['Male'],
        guardianDOB: [''],
        guardianAddress: ['']
      }),
      fees: this.formBuilder.group({
        discounts: this.formBuilder.array([], Validators.required)
      }),
      documents: this.formBuilder.group({
        attachments: [[],Validators.required], // Array of strings for URLs
      }),
      studentImageURL: [''],
    });
  }

  get discountsArray() {
    return this.formGroup.get('fees.discounts') as FormArray;
  }

  existingGuardian(event: number) {
    this.formGroup.get('existingGuardianId')?.patchValue(event);
  }

  // Submit form data as an `AddStudent` object
  onSubmit() {
    if (this.formGroup.valid) {
      const fees = this.formGroup.get('fees')?.value;
      const discounts = fees.discounts; // Access submitted discounts

      // Process each discount
      discounts.forEach((discount: Discount) => {
        console.log('Discount Amount:', discount.amount);
      });

      const formData: AddStudent = {
        studentID: this.formGroup.get('studentID')?.value,
        existingGuardianId: this.formGroup.get('existingGuardianId')?.value,
        ...this.formGroup.get('primaryData')?.value,
        ...this.formGroup.get('guardian')?.value,
        ...this.formGroup.get('optionData')?.value,
        ...this.formGroup.get('fees')?.value,
        attachments: this.formGroup.get('documents.attachments')?.value || [],
        studentImageURL: this.studentImageURL2
      };
      this.studentService.addStudent(formData).subscribe({
        next: (res) => {
          this.toastr.success('Student added successfully', res.message);
        }
      });
      this.studentService.uploadStudentImage(this.StudentImage, this.studentID).subscribe(() => {
        console.log('Image is uploaded successfully!');
      });
      this.studentService.uploadAttachments(this.files, this.studentID).subscribe({
        next: () => {
          this.toastr.success('Files uploaded successfully!');
        },
        error: (error) => {
          console.error('File upload failed:', error);
          this.toastr.error('Failed to upload files.');
        }
      });
      this.generateStudentID();
    } else {
      console.log('Form is not valid', this.formGroup.value);
    }
  }

  onRequiredFeesChanged(requiredFees: number): void {
    this.formGroup.get('primaryData.amount')?.patchValue(requiredFees);
    console.log('Required Fees:', requiredFees);
  }

  isEditMode = false;
  ngOnInit(): void {
    // If we have data passed in from the dialog:
    if (this.data) {
      if (this.data.mode === 'edit' && this.data.student) {
        // We are in EDIT mode, patch the form with existing student data
        const student = this.data.student;
        console.log("the student that want to edit is", student);
        // StudentID
        this.formGroup.patchValue({
          studentID: student.studentID,
          // Fill primaryData
          primaryData: {
            studentFirstName: student.studentFirstName || '',
            studentMiddleName: student.studentMiddleName || '',
            studentLastName: student.studentLastName || '',
            studentFirstNameEng: student.studentFirstNameEng || '',
            studentMiddleNameEng: student.studentMiddleNameEng || '',
            studentLastNameEng: student.studentLastNameEng || '',
            studentGender: student.gender || 'Male',
            studentDOB: student.studentDOB,
            classID: student.classID,
            divisionID: student.divisionID,
            // ...any other fields you have
          },
          // Fill optional data
          optionData: {
            placeBirth: student.placeBirth || '',
            studentPhone: student.studentPhone || '',
            studentAddress: student.studentAddress || '',
          },
          // Guardian
          guardian: {
            guardianFullName: student.guardianFullName || '',
            guardianGender: student.guardianGender || 'Male',
            guardianDOB: student.guardianDOB,
            guardianAddress: student.guardianAddress || '',
            guardianEmail: student.guardianEmail || '',
            guardianPhone: student.guardianPhone || '',
            guardianType: student.guardianType || '',
            // ...etc.
          },
          // If you also want to patch fees data
          fees: this.formBuilder.group({
            discounts: this.formBuilder.array([], Validators.required)
          }),
          documents: {
            attachments: student.attachments
          },
          studentImageURL: student.photoUrl
        });
        this.patchFees(student.discounts);
        this.isEditMode = true; // a local flag you can define
        this.studentImageURL= student.studentImageURL;
        this.refreshFees();
      }
      else {
        // Default behavior for ADD mode
        this.isEditMode = false;
        this.generateStudentID();
      }
    } else {
      // No data => default to ADD mode
      this.isEditMode = false;
      this.generateStudentID();
    }
  }

  private patchFees(discounts: FeeClasses[]): void {
   
    this.refreshFees();
  }
  
  loadFeesForClass(feeClasses: FeeClasses[]) {
    const discountsArray = this.formGroup.get('fees.discounts') as FormArray;
    discountsArray.clear(); // Clear any existing discounts

    feeClasses.forEach((fee) => {
      discountsArray.push(
        this.formBuilder.group({
          noteDiscount: [fee.noteDiscount || ''],
          amountDiscount: [fee.amountDiscount || 0],
          feeClassID: [fee.feeClassID, Validators.required],
          feeName: [fee.feeName || ''], // Add feeName if needed
          amount: [fee.amount || 0],
          className: [fee.className || ''], // Add className if needed
          mandatory: [fee.mandatory || false],
        })
      );
    });
  }

  private generateStudentID(): void {
    this.studentService.MaxStudentID().subscribe({
      next: (res) => {
        const maxValue = (res && typeof res === 'number') ? res + 1 : 1; // Default to 1 if invalid
        this.studentID = maxValue;
        this.formGroup.patchValue({ studentID: maxValue });
      },
      error: (err) => {
        this.toastr.error('Could not fetch maximum student ID. Defaulting to 1.');
        this.formGroup.patchValue({ studentID: 1 });
      }
    });
  }

  ngAfterViewInit(): void {
    setTimeout(() => {
      const defaultOpen = document.getElementById('defaultOpen');
      if (defaultOpen) {
        defaultOpen.click();
      }
    }, 0);
  }

  openPage(pageName: string, elmnt: EventTarget | null): void {
    this.activeTab = pageName; // Update activeTab property

    // Remove active class from all buttons
    const tablinks = document.getElementsByClassName("tablink") as HTMLCollectionOf<HTMLElement>;
    for (let i = 0; i < tablinks.length; i++) {
      tablinks[i].classList.remove('active');
    }

    // Add active class to the clicked button
    if (elmnt instanceof HTMLElement) {
      elmnt.classList.add('active');
    }

    // Force change detection (optional)
    this.changeDetectorRef.detectChanges();
  }

  closeModal(): void {
    this.dialogRef.close(); // Close the modal
  }

  updateAttachments(event: { attachments: string[]; files: File[] }): void {
    this.formGroup.get('documents.attachments')?.setValue(event.attachments);
    this.attachments = event.attachments;
    this.files = event.files;

    console.log('Updated Attachments:', this.attachments);
    console.log('Updated files:', this.files);
  }

  attachments: string[] = [];

  StudentImage!: File;
  studentImageURL: string = '';
  studentImageURL2: string = '';

  onFileSelected(event: Event): void {
    const input = event.target as HTMLInputElement;

    if (input.files?.[0]) {
      this.StudentImage = input.files[0]; // Assign the single file

      // Generate a preview URL
      const reader = new FileReader();
      reader.onload = (e: any) => {
        this.studentImageURL = e.target.result;
      };
      this.studentImageURL2 = `${this.studentID}_${this.StudentImage.name}`;
      reader.readAsDataURL(this.StudentImage);
    } else {
      console.error('No file selected');
    }
  }

  ngOnChanges(changes: SimpleChanges): void {
    if (changes['activeTab'] && this.activeTab === 'fees') {
      // Refresh fees data if needed
      this.refreshFees();
    }
  }

  selectedClassID: number | string = '';
  onClassSelected(classID: number | string) {
    this.selectedClassID = classID;
    this.formGroup.get('primaryData.classID')?.setValue(classID);
  }

  feeClassesChanged = new EventEmitter<any[]>();

  refreshFees(): void {
    console.log('Refreshing fees data...');
    this.feeClassesChanged.emit(this.formGroup.get('fees.discounts')?.value);
  }
}
