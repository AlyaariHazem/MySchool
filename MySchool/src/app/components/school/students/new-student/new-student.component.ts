import { Component, OnInit, AfterViewInit, Inject, ChangeDetectorRef, inject, OnDestroy } from '@angular/core';
import { FormBuilder, FormGroup, FormArray } from '@angular/forms';
import { MatDialogRef, MAT_DIALOG_DATA } from '@angular/material/dialog';
import { ToastrService } from 'ngx-toastr';

import {
  AddStudent, StudentPayload, UpdateDiscount
} from '../../../../core/models/students.model';
import { FeeClasses } from '../../core/models/Fee.model';
import { StudentService } from '../../../../core/services/student.service';
import { WebcamImage } from 'ngx-webcam';
import { StudentFormStoreService } from '../../core/store/student-form-store.service';

interface Attachment {
  attachmentID: number;
  attachmentURL: string;
  voucherID?: number;
  studentID?: number;
}

@Component({
  selector: 'app-new-student',
  templateUrl: './new-student.component.html',
  styleUrls: ['./new-student.component.scss']
})
export class NewStudentComponent implements OnInit, AfterViewInit, OnDestroy {

  /* ---------- reactive‑form ---------- */
  formGroup: FormGroup = this.formStore.getForm();
  get discountsArray() { return this.formGroup.get('fees.discounts') as FormArray; }

  /* ---------- UI state ---------- */
  activeTab = 'DataStudent';
  isEditMode = false;
  studentID = 0;
  studentImageURL = '';
  studentImageURL2 = '';
  files: File[] = [];
  attachments: string[] = [];
  studentName$ = this.formStore.fullName$;

  /* ---------- DI ---------- */
  private toastr = inject(ToastrService);
  private studentService = inject(StudentService);

  /* ---------- ctor ---------- */
  constructor(
    private fb: FormBuilder,
    private cd: ChangeDetectorRef,
    private formStore: StudentFormStoreService,
    public dialogRef: MatDialogRef<NewStudentComponent>,
    @Inject(MAT_DIALOG_DATA) public data: any
  ) { }
  ngOnDestroy(): void {
    this.formStore.resetForm();
  }

  /* ---------- lifecycle ---------- */
  ngOnInit(): void {
    this.initAddOrEdit();
    this.studentID=this.formGroup.get('studentID')!.value;
  }
  ngAfterViewInit(): void {
    setTimeout(() => document.getElementById('defaultOpen')?.click());
  }

  /* ---------- submit ---------- */
  onSubmit(): void { this.isEditMode ? this.onUpdate() : this.onAdd(); }

  /* ---------- add ---------- */
  private onAdd(): void {
    if (this.formGroup.invalid) return;
    this.attachments = this.formGroup.get('documents.attachments')?.value || [];

    const formData: AddStudent = {
      studentID: this.formGroup.get('studentID')!.value,
      existingGuardianId: this.formGroup.get('existingGuardianId')!.value,
      ...this.formGroup.getRawValue().primaryData,
      ...this.formGroup.getRawValue().guardian,
      ...this.formGroup.getRawValue().optionData,
      ...this.formGroup.getRawValue().fees,
      attachments: this.attachments,
      studentImageURL: this.studentImageURL2
    };

    this.studentService.addStudent(formData).subscribe({
      next: r => {
        this.toastr.success('Student Added Successfully! ', r.message);
        this.uploadImageAndFiles();
        this.generateStudentID();
      }

    });
  }

  /* ---------- update ---------- */
  onUpdate(): void {
    this.attachments = this.formGroup.get('documents.attachments')?.value || [];
    const updateDiscounts: UpdateDiscount[] = this.discountsArray.value.map((d: any) => ({
      studentClassFeeID: d.studentClassFeeID ?? 0,
      studentID: this.formGroup.get('studentID')!.value,
      feeClassID: d.feeClassID,
      amountDiscount: d.amountDiscount ?? 0,
      noteDiscount: d.noteDiscount ?? '',
      mandatory: d.mandatory ?? false
    }));

    const payload: StudentPayload = {
      ...this.formGroup.getRawValue().primaryData,
      ...this.formGroup.getRawValue().guardian,
      studentID: this.formGroup.get('studentID')!.value,
      existingGuardianId: this.formGroup.get('existingGuardianId')!.value,
      attachments: this.attachments,
      studentImageURL: this.studentImageURL2 || '',
      updateDiscounts
    };

    this.studentService.updateStudent(payload).subscribe({
      next: (res) => {
        this.toastr.success('Student updated successfully', res.message);
        this.uploadImageAndFiles();
        this.formStore.resetForm();
        this.dialogRef.close(payload);
      }
    });
  }

  /* ---------- helpers ---------- */
  private uploadImageAndFiles(): void {
    if (this.StudentImage)
      this.studentService.uploadStudentImage(this.StudentImage, this.studentID).subscribe();

    const files=this.formStore.getFiles();
    
    if (files.length)
      this.studentService.uploadFiles(files, this.studentID).subscribe();
  }

  private initAddOrEdit(): void {

    if (this.data?.mode === 'edit' && this.data.student) {
      this.isEditMode = true;
      const s = this.data.student;

      this.formGroup.patchValue({
        studentID: s.studentID,
        existingGuardianId: s.existingGuardianId,
        studentImageURL: s.studentImageURL,
        primaryData: {
          studentFirstName: s.studentFirstName,
          studentMiddleName: s.studentMiddleName,
          studentLastName: s.studentLastName,
          studentFirstNameEng: s.studentFirstNameEng,
          studentMiddleNameEng: s.studentMiddleNameEng,
          studentLastNameEng: s.studentLastNameEng,
          studentGender: s.studentGender,
          studentDOB: s.studentDOB,
          classID: s.classID,
          divisionID: s.divisionID,
          amount: s.amount
        },
        optionData: {
          placeBirth: s.placeBirth,
          studentPhone: s.studentPhone,
          studentAddress: s.studentAddress
        },
        guardian: {
          guardianFullName: s.guardianFullName,
          guardianGender: s.guardianGender,
          guardianDOB: s.guardianDOB,
          guardianAddress: s.guardianAddress,
          guardianEmail: s.guardianEmail,
          guardianPhone: s.guardianPhone,
          guardianType: s.guardianType
        },
      });

      const attachments = s.attachments.map((a: Attachment) => a.attachmentURL);
      this.formGroup.get('documents.attachments')!.setValue(attachments);
      this.formGroup.get('documents.attachmentsURLs')!.setValue(attachments);

      this.formStore.updateForm({
        arguments: attachments,
      });

      this.studentImageURL = s.studentImageURL || '';
      this.patchFees(s.discounts);
    } else {
      this.generateStudentID();
    }
  }

  private patchFees(discounts: FeeClasses[] = []): void {
    const arr = this.discountsArray;
    arr.clear();
    discounts.forEach(f =>
      arr.push(this.fb.group({
        feeClassID: [f.feeClassID],
        amountDiscount: [f.amountDiscount],
        noteDiscount: [f.noteDiscount],
        className: [f.className],
        feeName: [f.feeName],
        mandatory: [f.mandatory]
      }))
    );
  }

  private generateStudentID(): void {
    this.studentService.MaxStudentID().subscribe({
      next: n => {
        const nextId = (n ?? 0) + 1;
        this.studentID = nextId;                      // keep the field in sync
        this.formGroup.patchValue({ studentID: nextId });
      },
      error: () => {
        this.studentID = 1;
        this.formGroup.patchValue({ studentID: 1 });
      }
    });
  }

  /* ---------- UI ---------- */
  openPage(tab: string, btn: EventTarget | null): void {
    this.activeTab = tab;
    Array.from(document.getElementsByClassName('tablink')).forEach(b => b.classList.remove('active'));
    (btn as HTMLElement)?.classList.add('active');
    this.cd.detectChanges();
  }

  closeModal(): void { this.dialogRef.close(); }

  /* ---------- photo / camera ---------- */
  StudentImage!: File;
  showCamera = false;
  webcamImage: WebcamImage | null = null;

  onFileSelected(e: Event): void {
    const f = (e.target as HTMLInputElement).files?.[0];
    if (!f) return;
    this.StudentImage = f;
    const reader = new FileReader();
    reader.onload = ev => this.studentImageURL = ev.target?.result as string;
    this.studentImageURL2 = f.name;
    console.log('studentImageURl2', this.studentImageURL2);
    reader.readAsDataURL(f);
  }

  showCameraFun() { this.showCamera = true; }

  handleCapturedImage(payload: { file: File; previewUrl: string }): void {
    if (!payload) return;

    this.StudentImage = payload.file;
    this.studentImageURL = payload.previewUrl;
    this.studentImageURL2 = payload.file.name;

    this.showCamera = false;
  }

  public resetForm(): void {

    this.formStore.resetForm();
  }
}
