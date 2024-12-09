import { Component, AfterViewInit, OnInit, Inject, ChangeDetectorRef } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { MatDialogRef, MAT_DIALOG_DATA } from '@angular/material/dialog';
import { Observable} from 'rxjs';

@Component({
  selector: 'app-new-student',
  templateUrl: './new-student.component.html',
  styleUrls: ['./new-student.component.scss']
})
export class NewStudentComponent implements OnInit, AfterViewInit {
  activeTab: string = 'DataStudent'; // Default active tab
  mainForm: FormGroup;
  combinedData$: Observable<any[]> | undefined;
  currentPage: { [key: string]: number } = {};

  constructor(
    private formBuilder: FormBuilder,
    private changeDetectorRef: ChangeDetectorRef,
    public dialogRef: MatDialogRef<NewStudentComponent>, // Inject MatDialogRef
    @Inject(MAT_DIALOG_DATA) public data: any
  ) {
    // Create the main form
    this.mainForm = this.formBuilder.group({
      primaryData: this.formBuilder.group({
        // Full Names
        fullNameAr: [{ value: '', disabled: true }, Validators.required],
        fullNameEn: [{ value: '', disabled: true }, Validators.required],

        // Arabic Names
        firstNameAr: ['', Validators.required],
        secondNameAr: ['', Validators.required],
        lastNameAr: ['', Validators.required],

        // English Names
        firstNameEn: ['', Validators.required],
        secondNameEn: ['', Validators.required],
        lastNameEn: ['', Validators.required],

        // Other Information
        dob: ['', Validators.required],
        grade: ['', [Validators.required, Validators.maxLength(50)]],
        division: ['', [Validators.required, Validators.maxLength(50)]],
        sex: ['', Validators.required]

      }),
      optionData: this.formBuilder.group({
        placeOfBirth: ['', [Validators.required, Validators.maxLength(100)]],
        mobileNumber: ['', [Validators.required]],
        address: ['', [Validators.required, Validators.maxLength(200)]]

      }),
      guardian: this.formBuilder.group({
        guardianName: ['', [Validators.required]],         // Guardian's full name
        relationship: ['', [Validators.required]],         // Relationship to the student
        email: ['', [Validators.required]], // Email
        phone: ['', [Validators.required]],  // Phone number (10 digits)
        dob: ['', [Validators.required]],                  // Date of Birth
        address: ['', [Validators.required]],
      }),
      fees: this.formBuilder.group({
        
      }),
      document: this.formBuilder.group({
        ImageURL: ['', [Validators.required]],

      })
    });
  }
  onSubmit() {
    if (this.mainForm.valid) {
      console.log('Form Submitted', this.mainForm.value);
    } else {
      console.log('Form is not valid', this.mainForm.value);
    }
  }

  ngOnInit(): void {
    console.log(this.mainForm.get('primaryData')); // Should log a FormGroup object
  }
  
  check(){
    console.log("the form",this.mainForm.value)
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

  getTotalPages(item: any): number {
    return Math.ceil(item.grades.length / this.maxRowsPerPage);
  }
  // Method to handle the photo upload (file selection)
  uploadPhoto(event: Event): void {
    const input = event.target as HTMLInputElement;
    if (input?.files?.[0]) {
      const file = input.files[0];
      const reader = new FileReader();

      reader.onload = (e: ProgressEvent<FileReader>) => {
        const photoUrl = e.target?.result as string;
        // Logic to update student photo
        console.log('Photo uploaded:', photoUrl);
      };

      reader.readAsDataURL(file);
    }
  }
  takePhoto() {
    // Implement logic to open camera and take a photo
  }
}
