import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';

import { BackendAspService } from '../../../../ASP.NET/backend-asp.service';
import { Account, StudentAccounts } from '../models/accounts.model';

@Injectable({
  providedIn: 'root'
})
export class AccountService {
  private API = inject(BackendAspService);
  constructor() { }

  getAccountAndStudentNames(): Observable<StudentAccounts[]> {
    return this.API.getRequest<StudentAccounts[]>("Accounts/studentAndAccountNames");
  }

  getAllAccounts(): Observable<Account[]> {
    return this.API.getRequest<Account[]>("Accounts");
  }

  AddAccount(account: Account): Observable<Account> {
    return this.API.postRequest<Account>("Accounts", account);
  }

  UpdateAccount(id: number, account: Account): Observable<Account> {
    return this.API.putRequest<Account>(`Accounts/${id}`, account);
  }

  DeleteAccount(id: number): Observable<Account> {
    return this.API.deleteRequest<Account>(`Accounts/${id}`);
  }
  
}
