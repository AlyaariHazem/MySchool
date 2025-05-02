import { Component, inject, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ToastrService } from 'ngx-toastr';
import { MatDialog } from '@angular/material/dialog';

import { LanguageService } from '../../../core/services/language.service';
import { AccountService } from '../core/services/account.service';
import { Account } from '../core/models/accounts.model';
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

  accounts: Account[] = [];
  EditAccount: Account | undefined;

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
  
        this.accounts = res.result;
        this.updatePaginatedData();
      },
      error: (err) => {
        this.toastr.error('Server error occurred while fetching accounts.');
        console.error(err);
        this.accounts = [];
        this.displayedaccounts = [];
      }
    });
  }
  
    isLoading: boolean = true; // Loading state for the component
    first: number = 0; // Current starting index
    rows: number = 4; // Number of rows per page
    updatePaginatedData(): void {
      const start = this.first;
      const end = this.first + this.rows;
      this.displayedaccounts = this.accounts.slice(start, end);
    }
  
    // Handle page change event from PrimeNG paginator
    onPageChange(event: PaginatorState): void {
      this.first = event.first || 0; // Default to 0 if undefined
      this.rows = event.rows || 4; // Default to 4 rows
      this.updatePaginatedData();
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
