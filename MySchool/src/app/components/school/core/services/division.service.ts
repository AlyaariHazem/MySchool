import { inject, Injectable } from '@angular/core';
import { Observable, throwError } from 'rxjs';
import { catchError } from 'rxjs/operators';
import { BackendAspService } from '../../../../ASP.NET/backend-asp.service';
import { Division, divisions } from '../models/division.model';
import { ApiResponse } from '../../../../core/models/response.model';

@Injectable({
  providedIn: 'root'
})
export class DivisionService {
  private API = inject(BackendAspService);

  constructor() { }

  GetAll(yearID?: number): Observable<ApiResponse<divisions[]>> {
    const url = yearID ? `Divisions?yearID=${yearID}` : 'Divisions';
    return this.API.http.get<ApiResponse<divisions[]>>(`${this.API.baseUrl}/${url}`).pipe(
      catchError(error => {
        console.error('Error loading divisions:', error);
        return throwError(() => error);
      })
    );
  }

  Add(division: Division): Observable<ApiResponse<Division>> {
    return this.API.postRequest<Division>('Divisions', division);
  }

  Delete(id: number): Observable<ApiResponse<any>> {
    return this.API.deleteRequest<any>(`Divisions/${id}`);
  }

  partialUpdate(id: number, patchDoc: any): Observable<ApiResponse<any>> {
    return this.API.patchRequest<any>(`Divisions/${id}`, patchDoc);
  }

  UpdateDivision(id: number, division: Division): Observable<ApiResponse<Division>> {
    return this.API.putRequest<Division>(`Divisions/${id}`, division);
  }
}
