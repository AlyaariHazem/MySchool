import { inject, Injectable } from '@angular/core';

import { BackendAspService } from '../../environments/ASP.NET/backend-asp.service';
import { catchError, Observable, throwError } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class GuardianService {
  private API = inject(BackendAspService);
  constructor() { }
  getAllGuardians(): Observable<any> {
    return this.API.http.get(`${this.API.baseUrl}/Guardian`).pipe(
      catchError((error) => {
        console.error('Error fetching guardians:', error);
        return throwError(() => new Error('Failed to fetch guardians.'));
      })
    );
  }
  

}
