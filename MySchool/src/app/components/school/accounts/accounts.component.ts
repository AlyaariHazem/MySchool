import { Component, inject, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ToastrService } from 'ngx-toastr';
import { MatDialog } from '@angular/material/dialog';
import { Store } from '@ngrx/store';
import { PaginatorState } from 'primeng/paginator';
import { map } from 'rxjs';

import { AccountService } from '../core/services/account.service';
import { Account } from '../core/models/accounts.model';
import { PaginatorService } from '../../../core/services/paginator.service';
import { selectLanguage } from '../../../core/store/language/language.selectors';
import { ConfirmationService } from 'primeng/api';

interface AccountType {
  name: string;
  code: number;
}

@Component({
  selector: 'app-accounts',
  templateUrl: './accounts.component.html',
  styleUrls: ['./accounts.component.scss',
    './../../../shared/styles/style-select.scss']
})
export class AccountsComponent implements OnInit {
  visible: boolean = false;

  accountService = inject(AccountService);
  paginatorService = inject(PaginatorService);
  confirmationService = inject(ConfirmationService);

  accounts: Account[] = [];
  EditAccount: Account | undefined;
  isLoading: boolean = true;

  showDialog() {
    this.visible = true;
  }
  form: FormGroup;
  accountType: AccountType[] | undefined;
  max = 2;
  readonly dir$ = this.store.select(selectLanguage).pipe(
    map(l => (l === 'ar' ? 'rtl' : 'ltr')),
  );
  displayedaccounts: Account[] = []; // Students for the current page

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
    this.getAllAccounts();
  }

  getAllAccounts(): void {
    this.accountService.getAllAccounts().subscribe({
      next: (res) => {
        if (!res.isSuccess) {
          this.toastr.warning(res.errorMasseges[0] || 'Failed to load accounts.');
          this.accounts = [];
          this.displayedaccounts = [];
          return;
        }
        this.isLoading=false;
        this.accounts = res.result;
        this.displayedaccounts = this.paginatorService.pageSlice(this.accounts);
      },
      error: (err) => {
        this.toastr.error('Server error occurred while fetching accounts.');
        console.error(err);
        this.accounts = [];
        this.isLoading=false;
        this.displayedaccounts = [];
      }
    });
  }
  
  handlePageChange(event: PaginatorState): void {
    this.paginatorService.onPageChange(event);
    this.displayedaccounts = this.paginatorService.pageSlice(this.accounts);
  }

  changeState(student: Account) {
    student.state = !student.state;
  }
  editAccount(account: Account) {
    this.EditAccount = account;
    this.visible = true;
    console.log('Editing =>', account);
  }
  deleteAccount(id: number) {
    //confirm dialog
    this.confirmationService.confirm({
      message: 'هل أنت متأكد من حذف هذا الحساب؟',
      header: 'تأكيد الحذف',
      icon: 'pi pi-exclamation-triangle',
      acceptLabel: 'نعم',
      rejectLabel: 'لا',
      accept: () => {
      this.accountService.DeleteAccount(id).subscribe({
        next: (res) => {
          if (!res.isSuccess) {
            this.toastr.error(res.errorMasseges[0] || 'Failed to delete account.');
            return;
          }
            this.toastr.success('تم حذف الحساب');
            this.getAllAccounts();
          },
          error: (err) => {
            this.toastr.error('Failed to delete account.');
            console.error(err);
          }
        });
      }
    });
  }
}
