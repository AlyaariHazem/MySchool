import { Component, EventEmitter, inject, Input, OnChanges, OnDestroy, OnInit, Output } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ChangeDetectorRef } from '@angular/core';
import { DatePipe } from '@angular/common';
import { ToastrService } from 'ngx-toastr';



import { AccountService } from '../../core/services/account.service';
import { StudentAccounts } from '../../core/models/accounts.model';
import { VoucherService } from '../../core/services/voucher.service';
import { Voucher, VoucherAddUpdate } from '../../core/models/voucher.model';
import { FileService } from '../../../../core/services/file.service';
import { VouchersGuardian } from '../../core/models/vouchers-guardian.model';
import { IpaymentMethods } from '../../core/models/paymentMethods.model';
import { PAYMENTMETHODS } from '../../core/data/paymentMethods';
import { VouchersGuardianStoreService } from '../../core/services/vouchers-guardian-store.service';
import { Subscription } from 'rxjs';


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
  @Output() voucherUpdated = new EventEmitter<Voucher>(); // Emit updated voucher

  formGroup: FormGroup;

  private accountService = inject(AccountService);
  private voucherService = inject(VoucherService);
  private fileService = inject(FileService);
  private vouchersGuardianStore = inject(VouchersGuardianStoreService);
  
  private vouchersSubscription?: Subscription;

  accounts: StudentAccounts[] = [];
  filteredAccounts:StudentAccounts[]=[];
  attachments: string[] = [];
  files: File[] = [];
  selectedAccount!: StudentAccounts;
  payBy!: IpaymentMethods;
  voucherID: number | undefined;
  vouchersGuardian: VouchersGuardian[] = [];

  showDiv2 = false;
  changeView: boolean = true;

  voucherAdded = false; // New flag for success
  formSubmitted = false; // Track if form is submitted
  formPopulated = false; // Flag to prevent duplicate form population calls
  isSettingAccountProgrammatically = false; // Flag to prevent ngModelChange trigger during programmatic updates

  paymentMethods:IpaymentMethods[]=PAYMENTMETHODS;

  constructor(private formBuilder: FormBuilder, private cdr: ChangeDetectorRef,
    private toastr: ToastrService) {
    this.formGroup = this.formBuilder.group({
      voucherID: [''],
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
      console.log('Form is invalid',this.formGroup.value);
      return;
    }
    console.log('formGroup', formGroup.value);
    
    // Get payBy value - check form control first, then fallback to this.value
    let payByValue = this.value;
    if (formGroup.value.payBy) {
      // If form control has a payment method object, extract its value
      if (typeof formGroup.value.payBy === 'object' && formGroup.value.payBy.value) {
        payByValue = formGroup.value.payBy.value;
      } else if (typeof formGroup.value.payBy === 'string') {
        payByValue = formGroup.value.payBy;
      }
    }
    
    // Fallback: if still empty, try to get from this.payBy
    if (!payByValue && this.payBy && this.payBy.value) {
      payByValue = this.payBy.value;
    }
    
    // Create the voucher data object
    const voucherData: VoucherAddUpdate = {
      receipt: formGroup.value.receipt,
      hireDate: formGroup.value.hireDate,
      note: formGroup.value.note,
      payBy: payByValue || '',
      accountStudentGuardianID: this.accountStudentGuardianID,
      attachments: this.attachments, // Or update this dynamically if needed
      studentID: this.studentID,
    };

    this.voucherService.Add(voucherData).subscribe(res => {
      this.voucherID = +res;
      this.toastr.success('Voucher added successfully!');

      this.voucherAdded = true; // Set the flag to true on success
      this.formSubmitted = true; // Mark the form as submitted

      // Clear cache for this guardian to refresh data
      if (this.accountStudentGuardianID) {
        const guardianID = this.accounts.find(a => a.accountStudentGuardianID === this.accountStudentGuardianID)?.guardianID;
        if (guardianID) {
          this.vouchersGuardianStore.clearVouchersForGuardian(guardianID);
        }
      }

      this.uploadFiles(this.voucherID!);
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
      this.payBy = value;
      this.value = value.value;
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
  

  setAccountStudentGuardianID(value: any, skipVouchersCall: boolean = false): void {
    // If this is called from ngModelChange during programmatic update, skip it
    if (this.isSettingAccountProgrammatically) {
      return;
    }

    if (!value) {
      this.filteredAccounts = this.accounts;
      this.accountStudentGuardianID = 0;
      this.filteredVouchers = [];
      // Unsubscribe from previous guardian's vouchers
      if (this.vouchersSubscription) {
        this.vouchersSubscription.unsubscribe();
        this.vouchersSubscription = undefined;
      }
      this.vouchersGuardianStore.clearAllVouchers();
      return;
    }
  
    // Filter accounts on frontend
    this.filteredAccounts = this.accounts.filter(a => a.guardianID == value.guardianID);
    this.accountStudentGuardianID = value.accountStudentGuardianID;
    
    // Fetch vouchers filtered by guardianID from store (will use cache if available)
    if (!skipVouchersCall) {
      this.loadVouchersForGuardian(value.guardianID);
    }
    this.cdr.detectChanges();  // Trigger change detection
  }

  private loadVouchersForGuardian(guardianID: number): void {
    // Unsubscribe from previous subscription
    if (this.vouchersSubscription) {
      this.vouchersSubscription.unsubscribe();
    }

    // Subscribe to vouchers from store (will use cache or fetch if needed)
    this.vouchersSubscription = this.vouchersGuardianStore.getVouchersGuardian(guardianID).subscribe({
      next: (vouchers) => {
        this.vouchersGuardian = vouchers;
        this.filteredVouchers = vouchers;
        this.cdr.detectChanges();
      },
      error: (err) => {
        console.error('Error loading vouchers:', err);
        this.vouchersGuardian = [];
        this.filteredVouchers = [];
      }
    });
  }
  

  printVoucher() {
    const page = document.getElementById('reportVoucher');
    const header = document.getElementById('header');
    if (!page) { return; }
    if (!header) { return; }
  
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
           .print{display:none;}
            body{margin:0;direction:rtl;font-family:"Cairo","Tahoma",sans-serif}
            .report,*{letter-spacing:0!important}
          }
        </style>
      </head><body dir="rtl">
        ${header.outerHTML}
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
        
        // If editing and form not yet populated, populate form after accounts are loaded
        // Only populate if ngOnChanges hasn't already done it (check formPopulated flag)
        if (this.voucherData && !this.selectedAccount && !this.formPopulated) {
          this.populateFormForEdit();
        }
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
    const today = new Date().toISOString().split('T')[0];
    this.formGroup.patchValue({ hireDate: today });
  }

  private populateFormForEdit() {
    if (!this.voucherData || !this.accounts || this.accounts.length === 0 || this.formPopulated) {
      return;
    }
    
    this.formPopulated = true; // Mark as populated to prevent duplicate calls

    // Find the matching account from accounts array
    const matchingAccount = this.accounts.find(
      acc => acc.accountStudentGuardianID === this.voucherData!.accountStudentGuardianID
    );
    
    // Find the matching student account
    const matchingStudentAccount = this.accounts.find(
      acc => acc.studentID === this.voucherData!.studentID && 
             acc.accountStudentGuardianID === this.voucherData!.accountStudentGuardianID
    );

    // Prepare date value
    let hireDateValue: string;
    if (this.voucherData.hireDate) {
      const dateValue = this.voucherData.hireDate as any;
      if (typeof dateValue === 'string') {
        hireDateValue = dateValue.split('T')[0];
      } else {
        hireDateValue = new Date(dateValue).toISOString().split('T')[0];
      }
    } else {
      hireDateValue = new Date().toISOString().split('T')[0];
    }

    // Set account and student if found
    if (matchingAccount) {
      // Set flag to prevent ngModelChange from triggering getAllVouchersGuardian
      this.isSettingAccountProgrammatically = true;
      
      // Manually set the filtered accounts and accountStudentGuardianID
      this.filteredAccounts = this.accounts.filter(a => a.guardianID == matchingAccount.guardianID);
      this.accountStudentGuardianID = matchingAccount.accountStudentGuardianID;
      
      // Load vouchers for this guardian from store (will use cache if available)
      this.loadVouchersForGuardian(matchingAccount.guardianID);
      
      // Clear the flag
      this.isSettingAccountProgrammatically = false;
    }
    
    // Set selectedAccount to the student account (since both dropdowns use this variable)
    // This ensures both dropdowns display correctly
    if (matchingStudentAccount) {
      this.selectedAccount = matchingStudentAccount;
      this.setStudentID(matchingStudentAccount);
    } else if (matchingAccount) {
      // If no matching student account found, set to guardian account
      this.selectedAccount = matchingAccount;
    }

    // Find matching payment method
    const matchingPaymentMethod = this.paymentMethods.find(
      pm => pm.value === this.voucherData!.payBy
    );
    if (matchingPaymentMethod) {
      this.payBy = matchingPaymentMethod;
      this.value = matchingPaymentMethod.value;
    }

    // Set all form values at once with proper objects
    console.log('populateFormForEdit', this.voucherData);
    const formValues: any = {
      voucherID: this.voucherData.voucherID,
      receipt: this.voucherData.receipt,
      hireDate: hireDateValue,
      note: this.voucherData.note || ''
    };

    // Set form controls with the account objects (not IDs or strings)
    if (matchingAccount) {
      formValues.accountStudentGuardianID = matchingAccount;
    }
    if (matchingStudentAccount) {
      formValues.studentID = matchingStudentAccount;
    }
    if (matchingPaymentMethod) {
      formValues.payBy = matchingPaymentMethod;
    }

    this.formGroup.patchValue(formValues);

    // Set the accountStudentGuardianID and studentID properties
    this.accountStudentGuardianID = this.voucherData.accountStudentGuardianID;
    this.studentID = this.voucherData.studentID;
  }

  ngOnChanges() {
    // Only populate if accounts are already loaded, otherwise getAccounts() will handle it
    if (this.voucherData && this.accounts && this.accounts.length > 0) {
      this.populateFormForEdit();
    }
    
    if (this.voucherData == undefined) {
      this.formPopulated = false; // Reset flag when clearing
      this.isSettingAccountProgrammatically = false; // Reset programmatic flag
      // Unsubscribe from vouchers
      if (this.vouchersSubscription) {
        this.vouchersSubscription.unsubscribe();
        this.vouchersSubscription = undefined;
      }
      this.formGroup.reset({
        hireDate: new Date().toISOString().split('T')[0]
      });
      this.selectedAccount = undefined as any;
      this.payBy = undefined as any;
      this.accountStudentGuardianID = 0;
      this.studentID = 0;
      this.value = '';
      this.filteredAccounts = this.accounts || [];
      this.filteredVouchers = [];
      this.vouchersGuardian = [];
    }
  }

  ngOnDestroy() {
    // Unsubscribe from vouchers subscription
    if (this.vouchersSubscription) {
      this.vouchersSubscription.unsubscribe();
    }
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
    
    // Get payBy value - check form control first, then fallback to this.value
    let payByValue = this.value;
    if (this.formGroup.value.payBy) {
      // If form control has a payment method object, extract its value
      if (typeof this.formGroup.value.payBy === 'object' && this.formGroup.value.payBy.value) {
        payByValue = this.formGroup.value.payBy.value;
      } else if (typeof this.formGroup.value.payBy === 'string') {
        payByValue = this.formGroup.value.payBy;
      }
    }
    
    // Fallback: if still empty, try to get from this.payBy
    if (!payByValue && this.payBy && this.payBy.value) {
      payByValue = this.payBy.value;
    }
    
    const updatedVoucher: VoucherAddUpdate = {
      receipt: this.formGroup.value.receipt,
      hireDate: this.formGroup.value.hireDate,
      note: this.formGroup.value.note,
      payBy: payByValue || '',
      accountStudentGuardianID: this.accountStudentGuardianID,
      attachments: this.attachments,
      studentID: this.studentID,
    };

    this.voucherService.Update(this.voucherData?.voucherID, updatedVoucher).subscribe(res => {
      if (res.isSuccess) {
        this.toastr.success('Voucher updated successfully!');
        
        // Find the account name for the updated account
        const updatedAccount = this.accounts.find(
          acc => acc.accountStudentGuardianID === updatedVoucher.accountStudentGuardianID
        );
        
        // Clear cache for the guardian to refresh data
        if (updatedAccount?.guardianID) {
          this.vouchersGuardianStore.clearVouchersForGuardian(updatedAccount.guardianID);
        }
        
        // Create updated voucher object with new values
        const updatedVoucherData: Voucher = {
          ...this.voucherData!,
          receipt: updatedVoucher.receipt,
          hireDate: updatedVoucher.hireDate,
          note: updatedVoucher.note || '',
          payBy: updatedVoucher.payBy,
          accountStudentGuardianID: updatedVoucher.accountStudentGuardianID,
          studentID: updatedVoucher.studentID,
          accountName: updatedAccount?.accountName || this.voucherData!.accountName
        };
        
        // Emit the updated voucher to parent
        this.voucherUpdated.emit(updatedVoucherData);
        this.visibleChange.emit(false);  // Close dialog by emitting false
      } else {
        this.toastr.error(res.errorMasseges[0] || 'Failed to update voucher.');
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
      <title>prinat ${this.selectedAccount.accountName+"_"+this.selectedAccount.studentID}</title>
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
