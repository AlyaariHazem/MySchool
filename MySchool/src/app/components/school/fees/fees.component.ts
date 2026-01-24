import { Component, inject } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { MatDialog } from '@angular/material/dialog';
import { PaginatorState } from 'primeng/paginator';
import { ToastrService } from 'ngx-toastr';
import { Store } from '@ngrx/store';
import { map } from 'rxjs';

import { VoucherService } from '../core/services/voucher.service';
import { Voucher } from '../core/models/voucher.model';
import { selectLanguage } from '../../../core/store/language/language.selectors';

@Component({
  selector: 'app-fees',
  templateUrl: './fees.component.html',
  styleUrls: ['./fees.component.scss',
    './../../../shared/styles/style-select.scss']
})
export class FeesComponent {
  form: FormGroup;

  private voucherService = inject(VoucherService);
  
  visible: boolean = false;
  vouchers: Voucher[] = [];
  vouchersDisplay: Voucher[] = [];
  selectedVoucher: Voucher | undefined;
  totalRecords: number = 0;

  readonly dir$ = this.store.select(selectLanguage).pipe(
    map(l => (l === 'ar' ? 'rtl' : 'ltr')),
  );

  constructor(
    private formBuilder: FormBuilder,
    public dialog: MatDialog,
    private toastr: ToastrService,
    private store:Store
  ) {
    this.form = this.formBuilder.group({
      stage: ['', Validators.required],
      gradeName: ['', Validators.required],
    });
  }

  ngOnInit(): void {
    this.getPaginatedVouchers(this.first / this.rows + 1, this.rows);
  }

  showDialog() {
    this.visible = true;
    this.selectedVoucher = undefined;
  }
  ngOnDestroy(): void {
  }
  getAllVouchers() {
    // Keep this method for backward compatibility if needed
    this.voucherService.getAll().subscribe({
      next: (res) => {
        if (!res.isSuccess) {
          this.toastr.error(res.errorMasseges[0] || 'Failed to load vouchers.');
          return;
        }
        this.vouchers = res.result;
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

  getPaginatedVouchers(pageNumber: number, pageSize: number, filters: Record<string, string> = {}): void {
    this.voucherService.getPaginatedVouchers(pageNumber, pageSize, filters).subscribe({
      next: (paginatedResult) => {
        this.vouchersDisplay = paginatedResult.data || [];
        this.totalRecords = paginatedResult.totalCount || 0;
      },
      error: (err) => {
        this.toastr.error('Server error occurred while loading vouchers.');
        console.error(err);
      }
    });
  }

  onPageChange(event: PaginatorState) {
    this.first = event.first || 0;
    this.rows = event.rows!;
    const pageNumber = Math.floor(this.first / this.rows) + 1;
    this.getPaginatedVouchers(pageNumber, this.rows);
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
    // Refresh data when dialog closes (only after add, not update)
    if (!visible && !this.selectedVoucher) {
      this.getPaginatedVouchers(Math.floor(this.first / this.rows) + 1, this.rows);
    }
    // Clear selected voucher when dialog closes
    if (!visible) {
      this.selectedVoucher = undefined;
    }
  }

  onVoucherUpdated(updatedVoucher: Voucher) {
    // Update the voucher in the local array without calling backend
    const index = this.vouchersDisplay.findIndex(v => v.voucherID === updatedVoucher.voucherID);
    if (index !== -1) {
      this.vouchersDisplay[index] = updatedVoucher;
      // Create a new array reference to trigger change detection
      this.vouchersDisplay = [...this.vouchersDisplay];
    }
    this.selectedVoucher = undefined;
  }

  Delete(id: number) {
    this.voucherService.Delete(id).subscribe({
      next: res=>{
        if(!res.isSuccess){
          this.toastr.error(res.errorMasseges[0] || 'Failed to delete voucher.');
          return;
        }
        this.toastr.success(res.result || 'Voucher deleted successfully.');
        this.getPaginatedVouchers(Math.floor(this.first / this.rows) + 1, this.rows);
      },
      error: err => {
        this.toastr.error('Server error occurred while deleting voucher.');
        console.error(err);
      }
    }
    )
  }
}

