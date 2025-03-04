import { inject, Injectable } from '@angular/core';
import { catchError, map, Observable, throwError } from 'rxjs';

import { BackendAspService } from '../../environments/ASP.NET/backend-asp.service';

@Injectable({
  providedIn: 'root'
})
export class AccountService {
  private API = inject(BackendAspService);
  constructor() { }

  getAllAccounts(): Observable<any> {
    return this.API.http.get<any>(`${this.API.baseUrl}/Accounts`).pipe(
      map(response => response.result),
      catchError((error) => {
        console.error('Error fetching accounts:', error);
        return throwError(() => new Error('Failed to fetch accounts.'));
      })
    );
  }
}
