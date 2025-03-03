import { inject, Injectable } from '@angular/core';

import { BackendAspService } from '../../environments/ASP.NET/backend-asp.service';
import { catchError, map, Observable, throwError } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class GuardianService {
  private API = inject(BackendAspService);
  constructor() { }
  
  getAllGuardians(): Observable<any> {
    return this.API.http.get<any>(`${this.API.baseUrl}/Guardian`).pipe(
      map(response => response.result),
      catchError((error) => {
        console.error('Error fetching guardians:', error);
        return throwError(() => new Error('Failed to fetch guardians.'));
      })
    );
  }
  
  getGuardianById(id: number): Observable<any> {
    return this.API.http.get<any>(`${this.API.baseUrl}/Guardian/${id}`).pipe(
      map(response => response.result),
      catchError((error) => {
        console.error('Error fetching guardian:', error);
        return throwError(() => new Error('Failed to fetch guardian.'));
      })
    );
  }

  updateGuardian(id: number, guardian: any): Observable<any> {
    return this.API.http.put<any>(`${this.API.baseUrl}/Guardian/${id}`, guardian).pipe(
      map(response => response.result),
      catchError((error) => {
        console.error('Error updating guardian:', error);
        return throwError(() => new Error('Failed to update guardian.'));
      })
    );
  }


}
