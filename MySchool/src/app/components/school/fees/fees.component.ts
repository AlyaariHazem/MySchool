import { Component, inject } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { MatDialog } from '@angular/material/dialog';
import { PaginatorState } from 'primeng/paginator';

import { LanguageService } from '../../../core/services/language.service';
import { VoucherService } from '../core/services/voucher.service';
import { Voucher } from '../core/models/voucher.model';
import { ToastrService } from 'ngx-toastr';

@Component({
  selector: 'app-fees',
  templateUrl: './fees.component.html',
  styleUrls: ['./fees.component.scss',
    './../../../shared/styles/style-select.scss']
})
export class FeesComponent {
  form: FormGroup;

  private voucherService = inject(VoucherService);
  languageService = inject(LanguageService);
  
  visible: boolean = false;
  vouchers: Voucher[] = [];
  vouchersDisplay: Voucher[] = [];
  selectedVoucher: Voucher | undefined;

  constructor(
    private formBuilder: FormBuilder,
    public dialog: MatDialog,
    private toastr: ToastrService
  ) {
    this.form = this.formBuilder.group({
      stage: ['', Validators.required],
      gradeName: ['', Validators.required],
    });
  }

  ngOnInit(): void {
    this.refreshVouchers();

    this.updateDisplayedStudents(); // Initialize the displayed students
    this.languageService.currentLanguage();
  }

  showDialog() {
    this.visible = true;
    this.selectedVoucher = undefined;
  }
  ngOnDestroy(): void {
  }
  refreshVouchers() {
    this.voucherService.getAll().subscribe({
      next: (res) => {
        if (!res.isSuccess) {
          this.toastr.error(res.errorMasseges[0] || 'Failed to load vouchers.');
          return;
        }
        this.vouchers = res.result;
        this.updateDisplayedStudents();
      },
      error: (err) => {
        this.toastr.error('Server error occurred while loading vouchers.');
        console.error(err);
      }
    });
  }

  max = 2;
  first: number = 0;
  rows: number = 4;
  updateDisplayedStudents(): void {
    const start = this.first;
    const end = this.first + this.rows;
    this.vouchersDisplay = this.vouchers.slice(start, end);
  }

  onPageChange(event: PaginatorState) {
    this.first = event.first || 0; // Default to 0 if undefined
    this.rows = event.rows!;
    this.updateDisplayedStudents();
  }

  // Handle the "Print" button
  printVoucher() {
    const voucherElement = document.getElementById('voucherContent');
    if (!voucherElement) return;

    const printContents = voucherElement.innerHTML;
    const originalContents = document.body.innerHTML;

    document.body.innerHTML = printContents;
    window.print();
    document.body.innerHTML = originalContents;
  }
  // Method to handle the "Edit" button click
  onEdit(voucher: Voucher): void {
    this.selectedVoucher = voucher; // Create a copy of the voucher for editing
    this.visible = true; // Show the dialog
  }

  onDialogVisibilityChange(visible: boolean) {
    this.visible = visible; // Update the visible state
  }

  Delete(id: number) {
    this.voucherService.Delete(id).subscribe({
      next: res=>{
        if(!res.isSuccess){
          this.toastr.error(res.errorMasseges[0] || 'Failed to delete voucher.');
          return;
        }
        this.toastr.success(res.result || 'Voucher deleted successfully.');
        this.refreshVouchers();
      },
      error: err => {
        this.toastr.error('Server error occurred while deleting voucher.');
        console.error(err);
      }
    }
    )
  }
}
