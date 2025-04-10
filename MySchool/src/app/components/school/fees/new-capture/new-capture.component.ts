import { Component, EventEmitter, inject, Input, OnChanges, OnDestroy, OnInit, Output } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ChangeDetectorRef } from '@angular/core';
import { DatePipe } from '@angular/common';
import { ToastrService } from 'ngx-toastr';



import { AccountService } from '../../core/services/account.service';
import { StudentAccounts } from '../../core/models/accounts.model';
import { VoucherService } from '../../core/services/voucher.service';
import { Voucher, VoucherAdd } from '../../core/models/voucher.model';
import { FileService } from '../../../../core/services/file.service';

interface PayBy {
  label: string;
  value: string;
}

@Component({
  selector: 'app-new-capture',
  templateUrl: './new-capture.component.html',
  styleUrls: ['./new-capture.component.scss', './../../../../shared/styles/style-select.scss'],
  providers: [DatePipe] // Add this line
})
export class NewCaptureComponent implements OnInit, OnChanges,OnDestroy {
  @Input() voucherData: Voucher | undefined;
  @Input() visible: boolean = false;
  @Output() visibleChange = new EventEmitter<boolean>(); // Emit visibility change
   
  formGroup: FormGroup;

  private accountService = inject(AccountService);
  private voucherService = inject(VoucherService);
  private fileService = inject(FileService);

  accounts: StudentAccounts[] = [];
  attachments: string[] = [];
  files: File[] = [];
  selectedAccount!: StudentAccounts;
  payBy!: PayBy;
  voucherID: number | undefined;

  showDiv2 = false;
  changeView: boolean = true;

  voucherAdded = false; // New flag for success
  formSubmitted = false; // Track if form is submitted

  paymentMethods: PayBy[] = [
    { label: 'Cash', value: 'cash' },
    { label: 'الكريمي', value: 'visa' }
  ];

  constructor(private formBuilder: FormBuilder, private cdr: ChangeDetectorRef,
    private toastr: ToastrService, private datePipe: DatePipe) {
    this.formGroup = this.formBuilder.group({
      voucherID: [''],
      receipt: [''],
      hireDate: [this.datePipe.transform(Date.now(), 'yyyy-MM-dd')], // format the date here
      note: [''],
      payBy: [''],
      accountStudentGuardianID: ['', Validators.required],
      attachments: [[]],
      studentID: ['', Validators.required],
    });

  }

  addVoucher(formGroup: FormGroup) {
    // Ensure the form is valid before proceeding
    if (formGroup.invalid) {
      console.log('Form is invalid');
      return;
    }
    console.log('formGroup', formGroup.value);
    // Create the voucher data object
    const voucherData: VoucherAdd = {
      receipt: formGroup.value.receipt,
      hireDate: formGroup.value.hireDate,
      note: formGroup.value.note,
      payBy: this.value,
      accountStudentGuardianID: this.accountStudentGuardianID,
      attachments: this.attachments, // Or update this dynamically if needed
      studentID: this.studentID,
    };

    this.voucherService.Add(voucherData).subscribe(res => {
      this.voucherID = +res;
      this.toastr.success('Voucher added successfully!');

      this.voucherAdded = true; // Set the flag to true on success
      this.formSubmitted = true; // Mark the form as submitted

      this.uploadFiles(this.voucherID!);// I want to upload file withe voucherID but it is being undefined how can do that
    });
    console.log('Voucher Data:', voucherData);
    // Optionally reset the form after submission
    formGroup.reset();
  }

  uploadFiles(voucherID: number) {
    this.fileService.uploadFile(this.files, this.studentID, voucherID).subscribe(res => {

      console.log('File upload response:', res);
    });
  }
  accountStudentGuardianID!: number;
  studentID!: number;
  value!: string;

  setPayBy(value: any): void {
    if (value) {
      this.value = this.payBy.value;
      console.log('Selected Pay By:', this.payBy);
    }
    this.cdr.detectChanges();  // Trigger change detection
  }

  setStudentID(value: any): void {
    if (value) {
      this.studentID = value.studentID;
      console.log('Selected Student ID:', this.studentID);
    }
    this.cdr.detectChanges();  // Trigger change detection
  }

  setAccountStudentGuardianID(value: any): void {
    if (value) {
      this.accountStudentGuardianID = value.accountStudentGuardianID;
      console.log('Selected Account Student Guardian ID:', this.accountStudentGuardianID);
    }
    this.cdr.detectChanges();  // Trigger change detection
  }

  printVoucher() {
    const voucherElement = document.getElementById('voucherContent');
    if (!voucherElement) return;

    const printContents = voucherElement.innerHTML;
    const originalContents = document.body.innerHTML;

    document.body.innerHTML = printContents;
    window.print();
    document.body.innerHTML = originalContents;
  }

  toggleDiv() {
    this.showDiv2 = !this.showDiv2;
    this.changeView = !this.changeView;
  }
getAccounts(){
  this.accountService.getAccountAndStudentNames().subscribe(res => {
    this.accounts = res;
  });
}
  ngOnInit() {
    this.getAccounts();
    this.ngOnChanges();
  }

  ngOnChanges() {
    if (this.voucherData) {
      this.formGroup.patchValue({
        voucherID: this.voucherData.voucherID,
        receipt: this.voucherData.receipt,
        hireDate: this.voucherData.hireDate,
        note: this.voucherData.note,
        payBy: this.voucherData.payBy,
        studentID: this.voucherData.accountName
      });
    }

    if(this.voucherData==undefined)
      this.formGroup.reset();
  }

  ngOnDestroy() {
    this.voucherData = undefined;
    this.voucherData=undefined;
    
  }
  updateAttachments(event: any): void {
    const input = event.target as HTMLInputElement;

    if (input.files && input.files.length > 0) {
      const selectedFile = input.files[0];

      // Avoid duplicate files
      const fileExists = this.files.some(file => file.name === selectedFile.name);
      if (!fileExists) {
        this.files.push(selectedFile);
        this.attachments.push(selectedFile.name);
      }
    }
  }

  updateVoucher() {
    if (this.formGroup.invalid) {
      this.toastr.error('Please fill in all fields.');
      return;
    }
    const updatedVoucher: VoucherAdd = {
      receipt: this.formGroup.value.receipt,
      hireDate: this.formGroup.value.hireDate,
      note: this.formGroup.value.note,
      payBy: this.value,
      accountStudentGuardianID: this.accountStudentGuardianID,
      attachments: this.attachments,
      studentID: this.studentID,
    };

    this.voucherService.Update(this.voucherData?.voucherID, updatedVoucher).subscribe(res => {
      this.toastr.success('Voucher updated successfully!');
      this.visibleChange.emit(false);  // Close dialog by emitting false
    });
  }

  trackByIndex(index: number): number {
    return index;
  }
}
