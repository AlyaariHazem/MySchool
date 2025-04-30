import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';

import { BackendAspService } from '../../../../ASP.NET/backend-asp.service';
import { Account, StudentAccounts } from '../models/accounts.model';
import { ApiResponse } from '../../../../core/models/response.model';

@Injectable({
  providedIn: 'root'
})
export class AccountService {
  private API = inject(BackendAspService);

  constructor() { }

  // ✅ Return full ApiResponse
  getAccountAndStudentNames(): Observable<ApiResponse<StudentAccounts[]>> {
    return this.API.getRequest<StudentAccounts[]>("Accounts/studentAndAccountNames");
  }

  getAllAccounts(): Observable<ApiResponse<Account[]>> {
    return this.API.getRequest<Account[]>("Accounts");
  }

  AddAccount(account: Account): Observable<ApiResponse<Account>> {
    return this.API.postRequest<Account>("Accounts", account);
  }

  UpdateAccount(id: number, account: Account): Observable<ApiResponse<Account>> {
    return this.API.putRequest<Account>(`Accounts/${id}`, account);
  }

  DeleteAccount(id: number): Observable<ApiResponse<any>> {
    return this.API.deleteRequest<any>(`Accounts/${id}`);
  }
}
