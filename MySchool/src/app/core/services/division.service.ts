import { inject, Injectable } from '@angular/core';
import { BackendAspService } from '../../environments/ASP.NET/backend-asp.service';
import { catchError, map, Observable } from 'rxjs';
import { Division } from '../models/division.model';

@Injectable({
  providedIn: 'root'
})
export class DivisionService {
  private API = inject(BackendAspService);

  constructor() { }

  GetAll(): Observable<any> {
    return this.API.http.get(`${this.API.baseUrl}/Divisions`).pipe(
      map(response => response)
    );
  }
  Add(division: Division): Observable<any> {
    return this.API.http.post(`${this.API.baseUrl}/Divisions`, division).pipe(
      map(response => response)
    );
  }

  Delete(id: number): Observable<any> {
    return this.API.http.delete(`${this.API.baseUrl}/Divisions/${id}`);
  }

  partialUpdate(id: number, patchDoc: any): Observable<any> {
    return this.API.http.patch(`${this.API.baseUrl}/Divisions/${id}`, patchDoc).pipe(
      map(response => response),
      catchError(error => {
        console.error("Error with partial update:", error);
        throw error;
      })
    );
  }

  UpdateDivision(id: number, division: Division): Observable<any> {
    return this.API.http.put(`${this.API.baseUrl}/Divisions/${id}`, division).pipe(
      map(res => res),
      catchError(error => {
        console.log('Error with update division', error)
        throw error;
      })
    );

  }

}
