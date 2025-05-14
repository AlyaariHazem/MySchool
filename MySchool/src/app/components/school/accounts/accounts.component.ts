import { Component, inject, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ToastrService } from 'ngx-toastr';
import { MatDialog } from '@angular/material/dialog';

import { LanguageService } from '../../../core/services/language.service';
import { AccountService } from '../core/services/account.service';
import { Account } from '../core/models/accounts.model';
import { PaginatorService } from '../../../core/services/paginator.service';
import { PaginatorState } from 'primeng/paginator';

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

  accounts: Account[] = [];
  EditAccount: Account | undefined;
  isLoading: boolean = true;

  showDialog() {
    this.visible = true;
  }
  form: FormGroup;
  accountType: AccountType[] | undefined;
  max = 2;

  languageService = inject(LanguageService);

  displayedaccounts: Account[] = []; // Students for the current page

  constructor(
    private formBuilder: FormBuilder,
    public dialog: MatDialog,
    private toastr: ToastrService,
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
}
