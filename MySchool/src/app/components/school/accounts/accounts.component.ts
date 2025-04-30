import { Component, inject, OnInit } from '@angular/core';
import { FormBuilder, FormControl, FormGroup, Validators } from '@angular/forms';
import { ToastrService } from 'ngx-toastr';
import { MatDialog } from '@angular/material/dialog';
import { PageEvent } from '@angular/material/paginator';
import { Subscription } from 'rxjs';

import { LanguageService } from '../../../core/services/language.service';
import { AccountService } from '../core/services/account.service';
import { Account } from '../core/models/accounts.model';

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

  values = new FormControl<string[] | null>(null);
  max = 2;

  selectedCity: AccountType | undefined;

  languageService = inject(LanguageService);

  displayedaccounts: Account[] = []; // Students for the current page

  isSmallScreen = false;
  private mediaSub: Subscription | null = null;

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
    this.accountType = [
      { name: 'Guardain', code: 1 },
      { name: 'School', code: 2 },
      { name: 'Branches', code: 3 },
      { name: 'Funds', code: 4 },
      { name: 'Employees', code: 5 },
      { name: 'Banks', code: 6 }
    ];
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
        this.length = this.accounts.length;
        this.updateDisplayedaccounts();
      },
      error: (err) => {
        this.toastr.error('Server error occurred while fetching accounts.');
        console.error(err);
        this.accounts = [];
        this.displayedaccounts = [];
      }
    });
  }
  
  ngOnDestroy(): void {
    if (this.mediaSub) {
      this.mediaSub.unsubscribe();
    }
  }
  currentPage: number = 0; // Current page index
  pageSize: number = 5; // Number of items per page
  length: number = 0; // Total number of items
  updateDisplayedaccounts(): void {
    const startIndex = this.currentPage * this.pageSize;
    const endIndex = startIndex + this.pageSize;
    this.displayedaccounts = this.accounts.slice(startIndex, endIndex);
  }
  // Handle paginator events
  onPageChange(event: PageEvent): void {
    this.currentPage = event.pageIndex;
    this.pageSize = event.pageSize;
    this.updateDisplayedaccounts();
  }
  changeState(student: Account) {
    student.state = !student.state;
  }
  editAccount(account: Account) {
    this.EditAccount = account;
    this.visible = true;
  }
}
