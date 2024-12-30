// // File: src/app/components/edit-student/edit-student.component.ts

// import { Component, OnInit, AfterViewInit, ChangeDetectorRef, Input } from '@angular/core';
// import { FormBuilder, FormGroup, Validators, FormArray } from '@angular/forms';
// import { ActivatedRoute, Router } from '@angular/router';
// import { StudentService } from '../../../../../core/services/student.service';
// import { UpdateStudent, Discount } from '../../../../../core/models/update-student.model';
// import { ToastrService } from 'ngx-toastr';

// @Component({
//     selector: 'app-edit-student',
//     templateUrl: './edit-student.component.html',
//     styleUrls: ['./edit-student.component.scss']
// })
// export class EditStudentComponent implements OnInit, AfterViewInit {
//   @Input() formGroup!: FormGroup;
//     activeTab: string = 'DataStudent'; // Default active tab
//     studentID: number = 0; // Initialize with a default placeholder value
//     selectedFiles: File[] = [];
//     attachments: string[] = [];
//     studentImageURL: string = '';
//     StudentImage!: File;
//     isLoading: boolean = false;

//     constructor(
//         private fb: FormBuilder,
//         private studentService: StudentService,
//         private route: ActivatedRoute,
//         private router: Router,
//         private toastr: ToastrService,
//         private changeDetectorRef: ChangeDetectorRef
//     ) {
//         // Initialize the main form with nested form groups
//         this.formGroup = this.fb.group({
//             studentID: [{ value: this.studentID, disabled: true }, Validators.required],
//             primaryData: this.fb.group({
//                 studentFirstName: ['', Validators.required],
//                 studentMiddleName: ['', Validators.required],
//                 studentLastName: ['', Validators.required],
//                 studentFirstNameEng: [''],
//                 studentMiddleNameEng: [''],
//                 studentLastNameEng: [''],
//                 studentGender: ['Male', Validators.required],
//                 studentDOB: ['', Validators.required],
//                 studentPassword: ['Student'],
//                 classID: [null, Validators.required],
//                 amount: [0, Validators.required],
//                 divisionID: [null, Validators.required],
//                 studentAddress: [''],
//             }),
//             optionData: this.fb.group({
//                 placeBirth: [''],
//                 studentPhone: [''],
//                 hireDate: [''],
//                 studentAddress: [''],
//             }),
//             guardian: this.fb.group({
//                 guardianFullName: ['', Validators.required],
//                 guardianType: ['Guardian'],
//                 relationship: ['', Validators.required],
//                 guardianEmail: ['', [Validators.required, Validators.email]],
//                 guardianPassword: ['Guardian'],
//                 guardianPhone: ['', Validators.required],
//                 guardianGender: ['Male'],
//                 guardianDOB: ['', Validators.required],
//                 guardianAddress: ['', Validators.required]
//             }),
//             fees: this.fb.group({
//                 discounts: this.fb.array([], Validators.required)
//             }),
//             documents: this.fb.group({
//                 attachments: [[]], // Array of strings for URLs
//             }),
//             studentImageURL: [''],
//         });
//     }

//     ngOnInit(): void {
//         // Retrieve the student ID from the route
//         this.studentID = Number(this.route.snapshot.paramMap.get('id'));
//         this.formGroup.patchValue({ studentID: this.studentID });

//         // Fetch existing student data to populate the form
//         this.fetchStudentDetails();
//     }

//     ngAfterViewInit(): void {
//         setTimeout(() => {
//             const defaultOpen = document.getElementById('defaultOpen');
//             if (defaultOpen) {
//                 defaultOpen.click();
//             }
//         }, 0);
//     }

//     // Fetch student details by ID
//     fetchStudentDetails(): void {
//         this.isLoading = true;
//         this.studentService.getStudentById(this.studentID).subscribe({
//             next: (student) => {
//                 this.isLoading = false;
//                 if (student) {
//                     this.populateForm(student);
//                 } else {
//                     this.toastr.error(`Student with ID ${this.studentID} not found.`);
//                     this.router.navigate(['/students']);
//                 }
//             },
//             error: (err) => {
//                 this.isLoading = false;
//                 console.error("Error fetching student details:", err);
//                 this.toastr.error("Failed to fetch student details.");
//             }
//         });
//     }

//     // Populate the form with existing student data
//     populateForm(student: any): void {
//         // Populate primaryData
//         this.formGroup.get('primaryData')?.patchValue({
//             studentFirstName: student.fullName.firstName,
//             studentMiddleName: student.fullName.middleName,
//             studentLastName: student.fullName.lastName,
//             studentFirstNameEng: student.fullNameAlis?.firstNameEng || '',
//             studentMiddleNameEng: student.fullNameAlis?.middleNameEng || '',
//             studentLastNameEng: student.fullNameAlis?.lastNameEng || '',
//             studentGender: student.gender,
//             studentDOB: this.formatDate(student.studentDOB),
//             studentPassword: '', // Leave blank or handle accordingly
//             classID: student.classID,
//             amount: student.amount,
//             divisionID: student.divisionID,
//             studentAddress: student.studentAddress,
//         });

//         // Populate optionData
//         this.formGroup.get('optionData')?.patchValue({
//             placeBirth: student.placeBirth,
//             studentPhone: student.studentPhone,
//             hireDate: this.formatDate(student.hireDate),
//             studentAddress: student.studentAddress,
//         });

//         // Populate guardian
//         this.formGroup.get('guardian')?.patchValue({
//             guardianFullName: student.guardian.guardianFullName,
//             guardianType: student.guardian.guardianType,
//             relationship: student.guardian.relationship,
//             guardianEmail: student.guardian.guardianEmail,
//             guardianPassword: '', // Leave blank or handle accordingly
//             guardianPhone: student.guardian.guardianPhone,
//             guardianGender: student.guardian.guardianGender,
//             guardianDOB: this.formatDate(student.guardian.guardianDOB),
//             guardianAddress: student.guardian.guardianAddress
//         });

//         // Populate fees.discounts
//         const discountsArray = this.formGroup.get('fees.discounts') as FormArray;
//         discountsArray.clear();
//         if (student.discounts && student.discounts.length > 0) {
//             student.discounts.forEach((disc: Discount) => {
//                 discountsArray.push(this.fb.group({
//                     feeClassID: [disc.feeClassID, Validators.required],
//                     amountDiscount: [disc.amountDiscount, Validators.required],
//                     noteDiscount: [disc.noteDiscount]
//                 }));
//             });
//         }

//         // Populate documents.attachments
//         this.formGroup.get('documents.attachments')?.setValue(student.attachments || []);
//         this.attachments = student.attachments || [];

//         // Populate studentImageURL
//         this.formGroup.get('studentImageURL')?.setValue(student.photoUrl || '');
//         this.studentImageURL = student.photoUrl || '';
//     }

//     // Helper method to format date strings
//     formatDate(dateString: string): string {
//         const date = new Date(dateString);
//         return date.toISOString().substring(0, 10); // YYYY-MM-DD
//     }

//     // Accessor for discounts FormArray
//     get discountsFormArray(): FormArray {
//         return this.formGroup.get('fees.discounts') as FormArray;
//     }

//     // Add a new discount
//     addDiscount(): void {
//         this.discountsFormArray.push(this.fb.group({
//             feeClassID: ['', Validators.required],
//             amountDiscount: [0, Validators.required],
//             noteDiscount: ['']
//         }));
//     }

//     // Remove a discount by index
//     removeDiscount(index: number): void {
//         this.discountsFormArray.removeAt(index);
//     }

//     // Handle file selection for attachments
//     onFileSelected(event: any): void {
//         if (event.target.files && event.target.files.length > 0) {
//             for (let file of event.target.files) {
//                 this.selectedFiles.push(file);
//             }
//         }
//     }

//     // Handle image selection
//     onImageSelected(event: any): void {
//         const input = event.target as HTMLInputElement;

//         if (input.files?.[0]) {
//             this.StudentImage = input.files[0]; // Assign the single file
//             this.studentImageURL = `${this.studentID}_${this.StudentImage.name}`;
//             this.formGroup.patchValue({ studentImageURL: this.studentImageURL });
//         } else {
//             console.error('No file selected');
//         }
//     }

//     // Handle form submission for updating a student
//     onSubmit(): void {
//         if (this.formGroup.invalid) {
//             this.toastr.error("Please fill in all required fields.");
//             return;
//         }

//         // Construct the UpdateStudent object
//         const formValue = this.formGroup.getRawValue(); // getRawValue to include disabled fields
//         const updateStudent: UpdateStudent = {
//             studentID: formValue.studentID,
//             guardianEmail: formValue.guardian.guardianEmail,
//             guardianPassword: formValue.guardian.guardianPassword,
//             guardianAddress: formValue.guardian.guardianAddress,
//             guardianGender: formValue.guardian.guardianGender,
//             guardianFullName: formValue.guardian.guardianFullName,
//             guardianType: formValue.guardian.guardianType,
//             guardianPhone: formValue.guardian.guardianPhone,
//             guardianDOB: formValue.guardian.guardianDOB,
//             studentEmail: formValue.primaryData.studentEmail,
//             studentPassword: formValue.primaryData.studentPassword,
//             studentAddress: formValue.primaryData.studentAddress,
//             studentGender: formValue.primaryData.studentGender,
//             studentFirstName: formValue.primaryData.studentFirstName,
//             studentMiddleName: formValue.primaryData.studentMiddleName,
//             studentLastName: formValue.primaryData.studentLastName,
//             studentFirstNameEng: formValue.primaryData.studentFirstNameEng,
//             studentMiddleNameEng: formValue.primaryData.studentMiddleNameEng,
//             studentLastNameEng: formValue.primaryData.studentLastNameEng,
//             studentImageURL: formValue.studentImageURL,
//             divisionID: formValue.primaryData.divisionID,
//             placeBirth: formValue.optionData.placeBirth,
//             studentPhone: formValue.optionData.studentPhone,
//             studentDOB: formValue.primaryData.studentDOB,
//             hireDate: formValue.optionData.hireDate,
//             amount: formValue.primaryData.amount,
//             attachments: this.attachments, // Existing attachments
//             discounts: formValue.fees.discounts
//         };

//         // Call the updateStudent method in the service
//         this.isLoading = true;
//         this.studentService.updateStudent(updateStudent).subscribe({
//             next: (res) => {
//                 this.isLoading = false;
//                 this.toastr.success('Student updated successfully.');

//                 // Upload selected attachments if any
//                 if (this.selectedFiles.length > 0) {
//                     this.studentService.uploadAttachments(this.selectedFiles, this.studentID).subscribe({
//                         next: (res) => {
//                             this.toastr.success("Attachments uploaded successfully.");
//                             // Optionally, update the attachments list
//                             this.attachments = res.filePaths || [];
//                         },
//                         error: (err) => {
//                             this.toastr.error("Failed to upload attachments.");
//                             console.error("Error uploading attachments:", err);
//                         }
//                     });
//                 }

//                 // Upload student image if changed
//                 if (this.StudentImage) {
//                     this.studentService.uploadStudentImage(this.StudentImage, this.studentID).subscribe({
//                         next: (res) => {
//                             this.toastr.success("Student image uploaded successfully.");
//                             // Optionally, update the image URL if returned by backend
//                         },
//                         error: (err) => {
//                             this.toastr.error("Failed to upload student image.");
//                             console.error("Error uploading student image:", err);
//                         }
//                     });
//                 }

//                 // Optionally, navigate to another page
//                 this.router.navigate(['/students']);
//             },
//             error: (err) => {
//                 this.isLoading = false;
//                 this.toastr.error("Failed to update student.");
//                 console.error("Error updating student:", err);
//             }
//         });
//     }

//     // Handle tab switching
//     openPage(pageName: string, elmnt: EventTarget | null): void {
//         this.activeTab = pageName; // Update activeTab property

//         // Remove active class from all buttons
//         const tablinks = document.getElementsByClassName("tablink") as HTMLCollectionOf<HTMLElement>;
//         for (let i = 0; i < tablinks.length; i++) {
//             tablinks[i].classList.remove('active');
//         }

//         // Add active class to the clicked button
//         if (elmnt instanceof HTMLElement) {
//             elmnt.classList.add('active');
//         }

//         // Force change detection (optional)
//         this.changeDetectorRef.detectChanges();
//     }

//     // Reset the form to initial state
//     resetForm(): void {
//         this.fetchStudentDetails();
//     }

//     // Update attachments when child component emits changes
//     updateAttachments(event: { attachments: string[]; files: File[] }): void {
//         this.formGroup.get('documents.attachments')?.setValue(event.attachments);
//         this.attachments = event.attachments;
//         this.selectedFiles = event.files;

//         console.log('Updated Attachments:', this.attachments);
//         console.log('Updated Files:', this.selectedFiles);
//     }

//     // Additional methods for dropdowns, pagination, etc., can remain unchanged
// }
