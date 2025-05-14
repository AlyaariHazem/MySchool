import { Component, EventEmitter, Input, OnInit, Output } from '@angular/core';
import { FormBuilder, Validators } from '@angular/forms';
import { ToastrService } from 'ngx-toastr';

import { Account } from '../../core/models/accounts.model';
import { AccountService } from '../../core/services/account.service';
import { IpaymentMethods } from '../../core/models/paymentMethods.model';
import { PAYMENTMETHODS } from '../../core/data/paymentMethods';


@Component({
  selector: 'app-add-account',
  templateUrl: './add-account.component.html',
  styleUrls: ['./add-account.component.scss',
    './../../../../shared/styles/style-select.scss'
  ]
})
export class AddAccountComponent implements OnInit {
  @Input() account?: Account;
  @Output() saved = new EventEmitter<Account>();

  paymentMethods:IpaymentMethods[]=PAYMENTMETHODS;
  
  form = this.fb.group({
    accountName: ['', Validators.required],
    parentAccount: [null],
    typeOpenBalance: [false],
    accountType: [null, Validators.required],
    openingBalance: [0, Validators.min(0)],
    openingBalanceType: [null],
    description: [''],
    createdDate: ['', Validators.required],
  });

  constructor(private fb: FormBuilder,
    private accountSrv: AccountService,
    private toast: ToastrService) { }

  ngOnInit() {
    if (this.account) {
      this.form.patchValue(this.account);           // populate fields for EDIT
    }
    const today = new Date().toISOString().split('T')[0]; // "YYYY-MM-DD"
    this.form.patchValue({ createdDate: today });
  }

  accountType = [
    { name: 'Guardain', parentAccount: 1 },
    { name: 'School', parentAccount: 2 },
    { name: 'Branches', parentAccount: 3 },
    { name: 'Funds', parentAccount: 4 },
    { name: 'Employees', parentAccount: 5 },
    { name: 'Banks', parentAccount: 6 }
  ];
  save() {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    const body = this.form.value as Account;

    const request$ = this.account
      ? this.accountSrv.UpdateAccount(this.account.accountID!, body)
      : this.accountSrv.AddAccount(body);

    request$.subscribe({
      next: res => {
        if (!res.isSuccess) {
          this.toast.warning(res.errorMasseges[0] || 'فشل الحفظ');
          return;
        }

        this.toast.success('تم الحفظ بنجاح');
        this.saved.emit(res.result); // Emit the saved Account object
      },
      error: () => this.toast.error('حدث خطأ أثناء الحفظ')
    });
  }

}
