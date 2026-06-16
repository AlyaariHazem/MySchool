import { HttpErrorResponse } from '@angular/common/http';
import { Component, EventEmitter, Input, OnInit, Output } from '@angular/core';
import { AbstractControl, FormBuilder, ValidationErrors, Validators } from '@angular/forms';
import { ToastrService } from 'ngx-toastr';
import { Router } from '@angular/router';

import { Account } from '../../core/models/accounts.model';
import { AccountService } from '../../core/services/account.service';
import { IpaymentMethods } from '../../core/models/paymentMethods.model';
import { PAYMENTMETHODS } from '../../core/data/paymentMethods';

/** Body shape for api/Accounts (camelCase JSON). */
interface AccountsApiBody {
  accountID?: number;
  guardianName?: string | null;
  accountName?: string;
  state: boolean;
  note?: string | null;
  openBalance: number;
  typeOpenBalance: boolean;
  hireDate: string;
  typeAccountID: number;
}

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

  paymentMethods: IpaymentMethods[] = PAYMENTMETHODS;
  currentAccountId?: number;

  /** Server / client messages shown under the header (matches API spelling errorMasseges). */
  apiErrorMessages: string[] = [];

  form = this.fb.group({
    accountName: ['', [Validators.required, AddAccountComponent.noWhitespaceOnly]],
    parentAccount: [null as string | null],
    typeOpenBalance: [false],
    accountType: [null as string | null],
    openingBalance: [0, Validators.min(0)],
    openingBalanceType: [null as string | null],
    description: [''],
    createdDate: ['', Validators.required],
  });

  accountType = [
    { name: 'Guardain', parentAccount: 1 },
    { name: 'School', parentAccount: 2 },
    { name: 'Branches', parentAccount: 3 },
    { name: 'Funds', parentAccount: 4 },
    { name: 'Employees', parentAccount: 5 },
    { name: 'Banks', parentAccount: 6 }
  ];

  /** Not in Accounts API yet; keep empty so p-select does not break on `[options]=""`. */
  readonly openingBalanceTypeOptions: { label: string; name: string }[] = [];

  constructor(
    private fb: FormBuilder,
    private accountSrv: AccountService,
    private toast: ToastrService,
    private router: Router
  ) { }

  /** Reject empty or whitespace-only names (avoids red “invalid” with no obvious reason). */
  private static noWhitespaceOnly(control: AbstractControl): ValidationErrors | null {
    const v = control.value;
    if (v == null || (typeof v === 'string' && v.trim() === '')) {
      return { whitespace: true };
    }
    return null;
  }

  ngOnInit(): void {
    this.apiErrorMessages = [];
    if (this.account) {
      this.patchFormFromAccount(this.account);
      this.currentAccountId = this.account.accountID;
    } else {
      this.form.get('parentAccount')?.addValidators(Validators.required);
      this.form.get('parentAccount')?.updateValueAndValidity();
      const today = new Date().toISOString().split('T')[0];
      this.form.patchValue({ createdDate: today });
    }
  }

  private patchFormFromAccount(acc: Account): void {
    const anyAcc = acc as unknown as Record<string, unknown>;
    const typeId = (acc.typeAccountID ?? anyAcc['typeAccountID']) as number | undefined;
    const parentName = this.accountType.find((x) => x.parentAccount === typeId)?.name ?? null;
    const hireRaw = acc.hireDate ?? anyAcc['hireDate'];
    const dateStr = hireRaw ? this.toInputDateString(hireRaw as string | Date) : '';

    this.form.patchValue({
      accountName: acc.accountName ?? '',
      parentAccount: parentName,
      typeOpenBalance: !!acc.typeOpenBalance,
      openingBalance: acc.openBalance != null && acc.openBalance !== '' ? Number(acc.openBalance) : 0,
      description: acc.note ?? '',
      createdDate: dateStr,
      accountType: null,
      openingBalanceType: null,
    });
  }

  private toInputDateString(value: string | Date): string {
    const d = typeof value === 'string' ? new Date(value) : value;
    if (Number.isNaN(d.getTime())) {
      return '';
    }
    return d.toISOString().split('T')[0];
  }

  private buildAccountsBody(): AccountsApiBody {
    const fv = this.form.getRawValue();
    const selected = this.accountType.find((x) => x.name === fv.parentAccount);
    const typeAccountID =
      selected?.parentAccount ?? this.account?.typeAccountID ?? 1;

    const hireDate = fv.createdDate
      ? new Date(`${fv.createdDate}T12:00:00`)
      : new Date();

    const body: AccountsApiBody = {
      accountName: (fv.accountName ?? '').trim(),
      guardianName: this.account?.guardianName ?? null,
      state: this.account?.state ?? true,
      note: fv.description ?? '',
      openBalance: fv.openingBalance != null ? Number(fv.openingBalance) : 0,
      typeOpenBalance: !!fv.typeOpenBalance,
      hireDate: hireDate.toISOString(),
      typeAccountID,
    };

    if (this.account?.accountID != null) {
      body.accountID = this.account.accountID;
    }

    return body;
  }

  fieldInvalid(controlName: string): boolean {
    const c = this.form.get(controlName);
    return !!(c && c.invalid && (c.dirty || c.touched));
  }

  fieldError(controlName: string): string | null {
    const c = this.form.get(controlName);
    if (!c || !this.fieldInvalid(controlName)) {
      return null;
    }
    if (c.errors?.['required'] || c.errors?.['whitespace']) {
      return 'هذا الحقل مطلوب';
    }
    if (c.errors?.['min']) {
      return 'القيمة يجب ألا تكون سالبة';
    }
    return 'قيمة غير صالحة';
  }

  save(): void {
    this.apiErrorMessages = [];
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      this.toast.warning('يرجى تعبئة الحقول المطلوبة بشكل صحيح قبل الحفظ.');
      return;
    }

    const body = this.buildAccountsBody();

    const request$ = this.account?.accountID != null
      ? this.accountSrv.UpdateAccount(this.account.accountID, body as unknown as Account)
      : this.accountSrv.AddAccount(body as unknown as Account);

    request$.subscribe({
      next: res => {
        if (!res.isSuccess) {
          this.apiErrorMessages = res.errorMasseges?.length
            ? [...res.errorMasseges]
            : ['فشل الحفظ'];
          this.toast.warning(this.apiErrorMessages[0]);
          return;
        }

        this.apiErrorMessages = [];
        this.toast.success('تم الحفظ بنجاح');
        const result = res.result as Account | undefined;
        if (result?.accountID) {
          this.currentAccountId = result.accountID;
        } else if (this.account?.accountID) {
          this.currentAccountId = this.account.accountID;
        }
        this.saved.emit(result ?? this.account);
      },
      error: (err: HttpErrorResponse) => {
        const body = err.error as { errorMasseges?: string[] } | null;
        this.apiErrorMessages =
          body?.errorMasseges?.length ? [...body.errorMasseges] : ['تعذر الاتصال بالخادم'];
        this.toast.error(this.apiErrorMessages[0]);
      }
    });
  }

  printAccountReport(): void {
    if (!this.currentAccountId) {
      this.toast.warning('يرجى حفظ الحساب أولاً قبل الطباعة', 'تحذير');
      return;
    }

    const url = this.router.createUrlTree(['/school/reports/account'], {
      queryParams: { accountId: this.currentAccountId }
    });
    window.open(url.toString(), '_blank');
  }
}
