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
import { VouchersGuardian } from '../../core/models/vouchers-guardian.model';
import { PayBy } from '../../core/models/payBy.model';


@Component({
  selector: 'app-new-capture',
  templateUrl: './new-capture.component.html',
  styleUrls: ['./new-capture.component.scss', './../../../../shared/styles/style-select.scss'],
  providers: [DatePipe] // Add this line
})
export class NewCaptureComponent implements OnInit, OnChanges, OnDestroy {
  @Input() voucherData: Voucher | undefined;
  @Input() visible: boolean = false;
  @Output() visibleChange = new EventEmitter<boolean>(); // Emit visibility change

  formGroup: FormGroup;

  private accountService = inject(AccountService);
  private voucherService = inject(VoucherService);
  private fileService = inject(FileService);

  accounts: StudentAccounts[] = [];
  filteredAccounts:StudentAccounts[]=[];
  attachments: string[] = [];
  files: File[] = [];
  selectedAccount!: StudentAccounts;
  payBy!: PayBy;
  voucherID: number | undefined;
  vouchersGuardian: VouchersGuardian[] = [];

  showDiv2 = false;
  changeView: boolean = true;

  voucherAdded = false; // New flag for success
  formSubmitted = false; // Track if form is submitted

  paymentMethods: PayBy[] = [
    { label: 'Cash', value: 'cash' },
    { label: 'الكريمي', value: 'visa' }
  ];

  constructor(private formBuilder: FormBuilder, private cdr: ChangeDetectorRef,
    private toastr: ToastrService) {
    this.formGroup = this.formBuilder.group({
      voucherID: ['', Validators.required],
      receipt: [''],
      hireDate: ['', Validators.required], // format the date here
      note: [''],
      payBy: ['', Validators.required],
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
    this.fileService.uploadFiles(this.files,"Vouchers", voucherID).subscribe(res => {

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
  filteredVouchers: VouchersGuardian[] = [];
  setStudentID(value: any): void {
    if (!value) {
      this.studentID = 0;
      this.filteredVouchers = [];
      return;
    }
  
    this.studentID = value.studentID;
    this.cdr.detectChanges();  // Trigger change detection
  }
  

  setAccountStudentGuardianID(value: any): void {
    if (!value) {
      
      this.filteredAccounts = this.accounts;
      this.accountStudentGuardianID = 0;
      return;
    }
  
    this.filteredVouchers = this.vouchersGuardian.filter(v => v.guardianID == value.guardianID);
    this.filteredAccounts = this.accounts.filter(a => a.guardianID == value.guardianID);
  
    this.accountStudentGuardianID = value.accountStudentGuardianID;
    this.cdr.detectChanges();  // Trigger change detection
  }
  

  printVoucher() {
    const page = document.getElementById('reportVoucher');
    if (!page) { return; }
  
    const links = Array.from(document.querySelectorAll('link[rel="stylesheet"], style'))
      .filter((el: Element) => el.getAttribute('href') !== 'assets/print.css')
      .map(el => el.outerHTML)
      .join('');
  
    const base = `<base href="${document.baseURI}">`;
  
    const popup = window.open('', '', 'width=1000px,height=auto');
    if (!popup) { return; }
  
    popup.document.write(`
      <html><head>
      <link href="https://fonts.googleapis.com/css2?family=Cairo:wght@400;700&display=swap" rel="stylesheet">
      <title>prinat any thing you want</title>
        ${base}
        ${links}
        <style>
        
          @media print {
            body{margin:0;direction:rtl;font-family:"Cairo","Tahoma",sans-serif}
            .report,*{letter-spacing:0!important}   /* إبقاء العربية متصلة */
          }
        </style>
      </head><body dir="rtl">
        ${page.outerHTML}
      </body></html>
    `);
  
    popup.document.close();
    popup.onload = () => popup.print();
  
  }

  toggleDiv() {
    this.showDiv2 = !this.showDiv2;
    this.changeView = !this.changeView;
  }
  getAccounts() {
    this.accountService.getAccountAndStudentNames().subscribe({
      next: res=>{
        if(!res.isSuccess){
          this.toastr.warning(res.errorMasseges[0] || 'Failed to load accounts.');
          this.accounts = [];
          return;
        }
        this.accounts = res.result;
        this.filteredAccounts=this.accounts;
        
      },
      error: err => {
        this.toastr.error('Server error occurred while fetching accounts.');
        console.error(err);
        this.accounts = [];
      }
    });
  }
  ngOnInit() {
    this.getAccounts();
    this.ngOnChanges();
    this.getAllVouchersGuardian();
    const today = new Date().toISOString().split('T')[0];
    this.formGroup.patchValue({ hireDate: today });
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

    if (this.voucherData == undefined)
      this.formGroup.reset({
        hireDate: new Date().toISOString().split('T')[0]
      });
      
  }

  ngOnDestroy() {
    this.voucherData = undefined;
    this.voucherData = undefined;

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
  getAllVouchersGuardian(): void {
    this.voucherService.getAllVouchersGuardian().subscribe({
      next: res => {
        if (!res.isSuccess) {
          this.toastr.warning(res.errorMasseges[0] || 'Failed to load vouchers.');
          this.vouchersGuardian = [];
          return;
        }
        this.vouchersGuardian = res.result;
      },
      error: err => {
        this.toastr.error('Server error occurred while fetching vouchers.');
        console.error(err);
        this.vouchersGuardian = [];
      }
    });
  }
  trackByIndex(index: number): number {
    return index;
  }
  print(): void {
    const page = document.getElementById('report');
    if (!page) { return; }
  
    const links = Array.from(document.querySelectorAll('link[rel="stylesheet"], style'))
      .filter((el: Element) => el.getAttribute('href') !== 'assets/print.css')
      .map(el => el.outerHTML)
      .join('');
  
    const base = `<base href="${document.baseURI}">`;
  
    const popup = window.open('', '', 'width=1000px,height=auto');
    if (!popup) { return; }
  
    popup.document.write(`
      <html><head>
      <link href="https://fonts.googleapis.com/css2?family=Cairo:wght@400;700&display=swap" rel="stylesheet">
      <title>prinat ${this.selectedAccount.studentName+"_"+this.selectedAccount.studentID}</title>
        ${base}
        ${links}
        <style>
        
          @media print {
            body{margin:0;direction:rtl;font-family:"Cairo","Tahoma",sans-serif}
            .report,*{letter-spacing:0!important}   /* إبقاء العربية متصلة */
          }
        </style>
      </head><body dir="rtl">
        ${page.outerHTML}
      </body></html>
    `);
  
    popup.document.close();
    popup.onload = () => popup.print();
  }
}
