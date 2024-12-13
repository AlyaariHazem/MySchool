import { Component, AfterViewInit, OnInit, Inject, ChangeDetectorRef, inject } from '@angular/core';
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
export class NewStudentComponent implements OnInit, AfterViewInit {
  activeTab: string = 'DataStudent'; // Default active tab
  formGroup: FormGroup;
  toastr = inject(ToastrService)
  combinedData$: Observable<any[]> | undefined;
  currentPage: { [key: string]: number } = {};
  studentService = inject(StudentService);
  studentID: number = 0; // Initialize with a default placeholder value
  constructor(
    private formBuilder: FormBuilder,
    private changeDetectorRef: ChangeDetectorRef,
    public dialogRef: MatDialogRef<NewStudentComponent>, // Inject MatDialogRef
    @Inject(MAT_DIALOG_DATA) public data: any
  ) {
    // Create the main form

    this.formGroup = this.formBuilder.group({
      studentID: [this.studentID],
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
        studentAddress: ['Sana\'a'],
      }),
      optionData: this.formBuilder.group({
        placeBirth: ['Sana\'a'],
        studentPhone: [77],
        studentAddress: ['Sana\'a'],
      }),
      guardian: this.formBuilder.group({
        guardianFullName: ['', Validators.required],
        guardianType: ['Guardian'],
        relationship: ['', Validators.required],
        guardianEmail: ['', [Validators.required, Validators.email]],
        guardianPassword: ['Guardian'],
        guardianPhone: ['', Validators.required],
        guardianGender: ['Male'],
        guardianDOB: ['', Validators.required],
        guardianAddress: ['', Validators.required]
      }),
      fees: this.formBuilder.group({
        discounts: this.formBuilder.array([], Validators.required) // Initialize as a FormArray
      }),
      documents: this.formBuilder.group({
        attachments: [[]], // Array of strings for URLs
      })
    });
  }

  get discountsArray() {
    return this.formGroup.get('fees.discounts') as FormArray;
  }
  loadFeesForClass(feeClasses: FeeClasses[]) {
    const discountsArray = this.discountsArray; // Access FormArray for discounts
    discountsArray.clear(); // Clear any existing discounts

    feeClasses.forEach((fee) => {
      discountsArray.push(
        this.formBuilder.group({
          noteDiscount: [fee.noteDiscount || ''],
          amountDiscount: [fee.amountDiscount || 0],
          feeClassID: [fee.feeClassID, Validators.required],
        })
      );
    });
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
        ...this.formGroup.get('primaryData')?.value,
        ...this.formGroup.get('guardian')?.value,
        ...this.formGroup.get('fees')?.value,
        attachments: this.formGroup.get('documents.attachments')?.value || [],
      };
      console.log('the data for form', formData);
      this.studentService.addStudent(formData).subscribe({
        next: (res) => {
          this.toastr.success('student added successfully', res.message);
        }
      })
    } else {
      console.log('Form is not valid', this.formGroup.value);
    }
  }

  onRequiredFeesChanged(requiredFees: number): void {
    this.formGroup.get('primaryData.amount')?.patchValue(requiredFees);
    console.log('Required Fees:', requiredFees);
  }

  ngOnInit(): void {
    console.log(this.formGroup.get('primaryData')); // Should log a FormGroup object
    this.generateStudentID();
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

  check() {
    console.log("the form", this.formGroup.value)
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

  openOuterDropdown: any = null;
  openInnerDropdown: any = null;
  openInnerDivision: any = null;

  toggleOuterDropdown(item: any): void {
    if (this.openOuterDropdown === item) {
      this.openOuterDropdown = null;
    } else {
      this.openOuterDropdown = item;
    }
  }

  isOuterDropdownOpen(item: any): boolean {
    return this.openOuterDropdown === item;
  }

  toggleInnerDropdown(item: any, division: any): void {
    if (this.openInnerDropdown === item && this.openInnerDivision === division) {
      this.openInnerDropdown = null;
      this.openInnerDivision = null;
    } else {
      this.openInnerDropdown = item;
      this.openInnerDivision = division;
    }
  }

  isInnerDropdownOpen(item: any, division: any): boolean {
    return this.openInnerDropdown === item && this.openInnerDivision === division;
  }

  maxRowsPerPage = 3;

  getPaginatedGrades(item: any) {
    const startIndex = this.currentPage[item.id] * this.maxRowsPerPage;
    const endIndex = startIndex + this.maxRowsPerPage;
    return item.grades.slice(startIndex, endIndex);
  }

  nextPage(item: any) {
    if ((this.currentPage[item.id] + 1) * this.maxRowsPerPage < item.grades.length) {
      this.currentPage[item.id]++;
    }
  }

  previousPage(item: any) {
    if (this.currentPage[item.id] > 0) {
      this.currentPage[item.id]--;
    }
  }

  updateAttachments(fileNames: string[]): void {
    // Update the attachments array in the parent form
    this.formGroup.get('documents.attachments')?.setValue(fileNames);
  }

  getTotalPages(item: any): number {
    return Math.ceil(item.grades.length / this.maxRowsPerPage);
  }
  selectedFiles: File[] = [];
  attachments: string[] = [];

  onFileSelected(event: Event): void {
    const input = event.target as HTMLInputElement;

    if (input.files && input.files.length > 0) {
      const selectedFile = input.files[0];

      // Avoid duplicate files
      const fileExists = this.selectedFiles.some(file => file.name === selectedFile.name);
      if (!fileExists) {
        this.selectedFiles.push(selectedFile);
        this.attachments.push(selectedFile.name);

      } else {
        alert('This file has already been selected.');
      }
    }
  }
}
